
#include <algorithm>
#include "leveldb/env.h"
#include "port/port.h"

namespace leveldb {

namespace {

struct ThreadParam {
  ThreadParam(void (*function)(void* arg), void* arg)
    : function(function), arg(arg) {
  }

  void (*function)(void* arg);
  void* arg;
};

DWORD WINAPI ThreadProc(LPVOID lpParameter) {
  ThreadParam * param = static_cast<ThreadParam *>(lpParameter);
  void (*function)(void* arg) = param->function;
  void* arg = param->arg;
  delete param;
  function(arg);
  return 0;
}

class WindowsSequentialFile : public SequentialFile {
 public:
  WindowsSequentialFile(const std::string & fname, HANDLE file)
    : fname_(fname), file_(file) {
  }

  virtual ~WindowsSequentialFile() {
    CloseHandle(file_);
  }

  virtual Status Read(size_t n, Slice* result, char* scratch) {
    DWORD bytesRead = 0;
    BOOL success = ReadFile(file_, scratch, n, &bytesRead, NULL);
    *result = Slice(scratch, bytesRead);
    return success != FALSE ? Status::OK() : Status::IOError(fname_);
  }

  virtual Status Skip(uint64_t n) {
    LARGE_INTEGER li;
    li.QuadPart = n;
    BOOL success = SetFilePointerEx(file_, li, NULL, FILE_CURRENT);
    return success != FALSE ? Status::OK() : Status::IOError(fname_);
  }

 private:
  std::string fname_;
  HANDLE file_;
};

// A file abstraction for randomly reading the contents of a file.
class WindowsRandomAccessFile : public RandomAccessFile {
 public:
  WindowsRandomAccessFile(const std::string & fname, HANDLE file)
    : fname_(fname), file_(file) {
  }

  virtual ~WindowsRandomAccessFile() {
    CloseHandle(file_);
  }

  virtual Status Read(uint64_t offset, size_t n, Slice* result, char* scratch) const {
    OVERLAPPED overlapped = { 0 };
    overlapped.Offset = static_cast<DWORD>(offset);
    overlapped.OffsetHigh = static_cast<DWORD>(offset >> 32);
    DWORD bytesRead = 0;
    BOOL success = ReadFile(file_, scratch, n, &bytesRead, &overlapped);
    *result = Slice(scratch, bytesRead);
    return success != FALSE ? Status::OK() : Status::IOError(fname_);
  }

 private:
  std::string fname_;
  HANDLE file_;
};

class WindowsMappedFile : public RandomAccessFile {
 public:
  WindowsMappedFile(const std::string & fname, HANDLE file)
    : fname_(fname), file_(file) {
    mapping_ = CreateFileMapping(file_, NULL, PAGE_READONLY, 0, 0, NULL);
    view_ = MapViewOfFile(mapping_, FILE_MAP_READ, 0, 0, 0);
    base_ = static_cast<const BYTE *>(view_);
  }

  virtual ~WindowsMappedFile() {
    UnmapViewOfFile(view_);
    CloseHandle(mapping_);
    CloseHandle(file_);
  }

  virtual Status Read(uint64_t offset, size_t n, Slice* result, char* scratch) const {
    CopyMemory(scratch, base_ + offset, n);
    *result = Slice(scratch, n);
    return Status::OK();
  }

 private:
  std::string fname_;
  HANDLE file_;
  HANDLE mapping_;
  PVOID view_;
  const BYTE * base_;
};

class WindowsWritableFile : public WritableFile {
 public:
  WindowsWritableFile(const std::string & fname, HANDLE file)
    : fname_(fname), file_(file), pos_(0) {
  }

  virtual ~WindowsWritableFile() {
    CloseHandle(file_);
  }

  virtual Status Append(const Slice& data) {
    size_t totalBytesWritten = 0;
    while (totalBytesWritten < data.size())
    {
      if (pos_ == kBufferSize)
        Flush();

      size_t size = std::min(kBufferSize - pos_, data.size() - totalBytesWritten);
      memcpy(buffer_ + pos_, data.data() + totalBytesWritten, size);
      pos_ += size;
      totalBytesWritten += size;
    }
    return Status::OK();
  }

  virtual Status Close() {
    Status status = Flush();
    CloseHandle(file_);
    file_ = INVALID_HANDLE_VALUE;
    return status;
  }

  virtual Status Flush() {
    size_t pos = 0;
    while (pos < pos_) {
      DWORD bytesWritten = 0;
      BOOL success = WriteFile(file_, &buffer_[pos], pos_ - pos, &bytesWritten, NULL);
      if (success == FALSE)
        return Status::IOError(fname_);
      pos += bytesWritten;
    }
    pos_ = 0;
    return Status::OK();
  }

  virtual Status Sync() {
    Status status = Flush();
    if (!status.ok())
      return status;
    BOOL success = FlushFileBuffers(file_);
    return success != FALSE ? Status::OK() : Status::IOError(fname_);
  }

 private:
  static const size_t kBufferSize = 65536;
  std::string fname_;
  HANDLE file_;
  BYTE buffer_[kBufferSize];
  size_t pos_;
};

class WindowsFileLock : public FileLock {
 public:
  WindowsFileLock(const std::string & fname, HANDLE file)
    : fname_(fname), file_(file) {
  }

  const std::string & GetFileName() {
    return fname_;
  }

  bool Close() {
    bool success = file_ == INVALID_HANDLE_VALUE || CloseHandle(file_) != FALSE;
    file_ = INVALID_HANDLE_VALUE;
    return success;
  }

  virtual ~WindowsFileLock() {
    Close();
  }

 private:
  std::string fname_;
  HANDLE file_;
};

class WindowsLogger : public Logger {
public:
  WindowsLogger(WritableFile * log) : log_(log) {
  }

  ~WindowsLogger() {
    delete log_;
  }

  // Write an entry to the log file with the specified format.
  virtual void Logv(const char* format, va_list ap) {
    const size_t kBufSize = 4096;
    char buffer[kBufSize];
    int written = vsnprintf(buffer, kBufSize, format, ap);
    log_->Append(Slice(buffer, written == -1 ? kBufSize : written));
    log_->Append(Slice("\r\n", 2));
  }

private:
  WritableFile * log_;
};

}

class WindowsEnv : public Env {
 public:
  WindowsEnv() {
    QueryPerformanceFrequency(&freq_);
  }

  // Create a brand new sequentially-readable file with the specified name.
  // On success, stores a pointer to the new file in *result and returns OK.
  // On failure stores NULL in *result and returns non-OK.  If the file does
  // not exist, returns a non-OK status.
  //
  // The returned file will only be accessed by one thread at a time.
  virtual Status NewSequentialFile(const std::string& fname, SequentialFile** result) {
    *result = NULL;
    HANDLE file = CreateFileA(fname.c_str(), GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE,
      NULL, OPEN_EXISTING, FILE_FLAG_SEQUENTIAL_SCAN, NULL);
    if  (file == INVALID_HANDLE_VALUE)
      return Status::IOError(fname);
    *result = new WindowsSequentialFile(fname, file);
    return Status::OK();
  }

  // Create a brand new random access read-only file with the
  // specified name.  On success, stores a pointer to the new file in
  // *result and returns OK.  On failure stores NULL in *result and
  // returns non-OK.  If the file does not exist, returns a non-OK
  // status.
  //
  // The returned file may be concurrently accessed by multiple threads.
  virtual Status NewRandomAccessFile(const std::string& fname, RandomAccessFile** result) {
    *result = NULL;
    HANDLE file = CreateFileA(fname.c_str(), GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE,
      NULL, OPEN_EXISTING, FILE_FLAG_RANDOM_ACCESS, NULL);
    if  (file == INVALID_HANDLE_VALUE)
      return Status::IOError(fname);
    // use memory-mapped files if address space is available
#ifdef _M_X64
    *result = new WindowsMappedFile(fname, file);
#else
    *result = new WindowsRandomAccessFile(fname, file);
#endif
    return Status::OK();
  }

  // Create an object that writes to a new file with the specified
  // name.  Deletes any existing file with the same name and creates a
  // new file.  On success, stores a pointer to the new file in
  // *result and returns OK.  On failure stores NULL in *result and
  // returns non-OK.
  //
  // The returned file will only be accessed by one thread at a time.
  virtual Status NewWritableFile(const std::string& fname, WritableFile** result) {
    *result = NULL;
    HANDLE file = CreateFileA(fname.c_str(), GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, 0, NULL);
    if  (file == INVALID_HANDLE_VALUE)
      return Status::IOError(fname);
    *result = new WindowsWritableFile(fname, file);
    return Status::OK();
  }

  // Returns true iff the named file exists.
  virtual bool FileExists(const std::string& fname) {
    DWORD attr = GetFileAttributesA(fname.c_str());
    return attr != INVALID_FILE_ATTRIBUTES && ((attr & FILE_ATTRIBUTE_DIRECTORY) == 0);
  }

  // Store in *result the names of the children of the specified directory.
  // The names are relative to "dir".
  // Original contents of *results are dropped.
  virtual Status GetChildren(const std::string& dir, std::vector<std::string>* result) {
    result->clear();
    std::string dirWildcard(dir);
    dirWildcard.append("\\*");
    WIN32_FIND_DATAA fd = { 0 };
    HANDLE h = FindFirstFileA(dirWildcard.c_str(), &fd);
    if (h == INVALID_HANDLE_VALUE)
      return Status::IOError(dir);
    do
    {
      if ((fd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) == 0)
        result->push_back(std::string(fd.cFileName));
    } while (FindNextFileA(h, &fd) != FALSE);
    FindClose(h);
    return Status::OK();
  }

  // Delete the named file.
  virtual Status DeleteFile(const std::string& fname) {
    BOOL success = DeleteFileA(fname.c_str());
    return success == FALSE ? Status::IOError(fname) : Status::OK();
  }

  // Create the specified directory.
  virtual Status CreateDir(const std::string& dirname) {
    BOOL success = CreateDirectoryA(dirname.c_str(), NULL);
    return success == FALSE ? Status::IOError(dirname) : Status::OK();
  }

  // Delete the specified directory.
  virtual Status DeleteDir(const std::string& dirname) {
    std::string dirname2(dirname);
    dirname2.push_back('\0');

    SHFILEOPSTRUCTA fileop = { 0 };
    fileop.wFunc = FO_DELETE;
    fileop.pFrom = dirname2.c_str();
    fileop.fFlags = FOF_NO_UI;
    int nResult = SHFileOperationA(&fileop);
    return (nResult == 0 && fileop.fAnyOperationsAborted == FALSE) ? Status::OK() : Status::IOError(dirname);
  }

  // Store the size of fname in *file_size.
  virtual Status GetFileSize(const std::string& fname, uint64_t* file_size) {
    *file_size = 0;
    WIN32_FIND_DATAA fd;
    HANDLE h = FindFirstFileA(fname.c_str(), &fd);
    if (h == INVALID_HANDLE_VALUE)
      return Status::IOError(fname);
    *file_size = (static_cast<uint64_t>(fd.nFileSizeHigh) << 32) + fd.nFileSizeLow;
    FindClose(h);
    return Status::OK();
  }

  // Rename file src to target.
  virtual Status RenameFile(const std::string& src, const std::string& target) {
    BOOL success = MoveFileExA(src.c_str(), target.c_str(), MOVEFILE_REPLACE_EXISTING);
    return success != FALSE ? Status::OK() : Status::IOError(src, target);
  }

  // Lock the specified file.  Used to prevent concurrent access to
  // the same db by multiple processes.  On failure, stores NULL in
  // *lock and returns non-OK.
  //
  // On success, stores a pointer to the object that represents the
  // acquired lock in *lock and returns OK.  The caller should call
  // UnlockFile(*lock) to release the lock.  If the process exits,
  // the lock will be automatically released.
  //
  // If somebody else already holds the lock, finishes immediately
  // with a failure.  I.e., this call does not wait for existing locks
  // to go away.
  //
  // May create the named file if it does not already exist.
  virtual Status LockFile(const std::string& fname, FileLock** lock) {
    *lock = NULL;
    HANDLE file = CreateFileA(fname.c_str(), GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_ALWAYS, 0, NULL);
    if (file == INVALID_HANDLE_VALUE)
      return Status::IOError(fname);
    *lock = new WindowsFileLock(fname, file);
    return Status::OK();
  }

  // Release the lock acquired by a previous successful call to LockFile.
  // REQUIRES: lock was returned by a successful LockFile() call
  // REQUIRES: lock has not already been unlocked.
  virtual Status UnlockFile(FileLock* lock) {
    WindowsFileLock* my_lock = reinterpret_cast<WindowsFileLock*>(lock);
    Status result;
    if (!my_lock->Close())
      result = Status::IOError(my_lock->GetFileName(), "Could not close lock file.");
    delete my_lock;
    return result;
  }

  // Arrange to run "(*function)(arg)" once in a background thread.
  //
  // "function" may run in an unspecified thread.  Multiple functions
  // added to the same Env may run concurrently in different threads.
  // I.e., the caller may not assume that background work items are
  // serialized.
  virtual void Schedule(void (*function)(void* arg), void* arg) {
    ThreadParam * param = new ThreadParam(function, arg);
    QueueUserWorkItem(ThreadProc, param, WT_EXECUTEDEFAULT);
  }

  // Start a new thread, invoking "function(arg)" within the new thread.
  // When "function(arg)" returns, the thread will be destroyed.
  virtual void StartThread(void (*function)(void* arg), void* arg) {
    ThreadParam * param = new ThreadParam(function, arg);
    CreateThread(NULL, 0, ThreadProc, param, 0, NULL);
  }

  // *path is set to a temporary directory that can be used for testing. It may
  // or many not have just been created. The directory may or may not differ
  // between runs of the same process, but subsequent calls will return the
  // same directory.
  virtual Status GetTestDirectory(std::string* path) {
    char tempPath[MAX_PATH];
    GetTempPathA(MAX_PATH, &tempPath[0]);
    *path = std::string(tempPath);
    return Status::OK();
  }

  // Create and return a log file for storing informational messages.
  virtual Status NewLogger(const std::string& fname, Logger** result) {
    *result = NULL;
    WritableFile * logfile;
    Status status = NewWritableFile(fname, &logfile);
    if (status.ok())
      *result = new WindowsLogger(logfile);
    return status;
  }

  // Returns the number of micro-seconds since some fixed point in time. Only
  // useful for computing deltas of time.
  virtual uint64_t NowMicros() {
    LARGE_INTEGER count;
    QueryPerformanceCounter(&count);
    return count.QuadPart * 1000000i64 / freq_.QuadPart;
  }

  // Sleep/delay the thread for the perscribed number of micro-seconds.
  virtual void SleepForMicroseconds(int micros) {
    // round up to the next millisecond
    Sleep((micros + 999) / 1000);
  }

private:
  LARGE_INTEGER freq_;
};

#if NTDDI_VERSION < NTDDI_VISTA

Env* g_env = NULL;

Env* Env::Default() {
  // Windows XP doesn't have one-time initialization functions, so use a simplistic approach that
  // uses static variables and possibly leaks memory (if called simultaneously on two threads).
  if (g_env == NULL)
    g_env = new WindowsEnv();
  return g_env;
}

#else // NTDDI_VERSION < NTDDI_VISTA

INIT_ONCE g_initOnce = INIT_ONCE_STATIC_INIT;

BOOL CALLBACK CreateEnv(PINIT_ONCE InitOnce, PVOID Parameter, PVOID *lpContext) {
  Env * env = new WindowsEnv();
  *lpContext = env;
  return TRUE;
}

Env* Env::Default() {
  LPVOID context;
  BOOL success = InitOnceExecuteOnce(&g_initOnce, CreateEnv, NULL, &context);
  return (success != FALSE) ? static_cast<Env *>(context) : NULL;
}

#endif // NTDDI_VERSION < NTDDI_VISTA

}
