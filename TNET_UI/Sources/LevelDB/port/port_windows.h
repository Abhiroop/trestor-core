
#ifndef STORAGE_LEVELDB_PORT_PORT_WINDOWS_H_
#define STORAGE_LEVELDB_PORT_PORT_WINDOWS_H_

#include <stdint.h>
#include <string>

#include <Windows.h>
#undef DeleteFile
#undef min
#undef small

#define snprintf _snprintf

namespace leveldb {
namespace port {

// The following boolean constant must be true on a little-endian machine
// and false otherwise.
static const bool kLittleEndian = true;

// ------------------ Threading -------------------

// A Mutex represents an exclusive lock.
class Mutex {
 public:
  Mutex();
  ~Mutex();

  // Lock the mutex.  Waits until other lockers have exited.
  // Will deadlock if the mutex is already locked by this thread.
  void Lock();

  // Unlock the mutex.
  // REQUIRES: This mutex was locked by this thread.
  void Unlock();

  // Optionally crash if this thread does not hold this mutex.
  // The implementation must be fast, especially if NDEBUG is
  // defined.  The implementation is allowed to skip all checks.
  void AssertHeld();

 private:
   friend class CondVar;

#if NTDDI_VERSION < NTDDI_VISTA
  // critical sections are more efficient than mutexes
  // but they are not recursive and can only be used to synchronize threads within the same process
  // additionally they cannot be used with SignalObjectAndWait that we use for CondVar
  HANDLE mutex_;
#else
  CRITICAL_SECTION cs_;
#endif

  // No copying
  Mutex(const Mutex&);
  void operator=(const Mutex&);
};

class CondVar {
 public:
  explicit CondVar(Mutex* mu);
  ~CondVar();

  // Atomically release *mu and block on this condition variable until
  // either a call to SignalAll(), or a call to Signal() that picks
  // this thread to wakeup.
  // REQUIRES: this thread holds *mu
  void Wait();

  // If there are some threads waiting, wake up at least one of them.
  void Signal();

  // Wake up all waiting threads.
  void SignalAll();

private:
  Mutex * mu_;

#if NTDDI_VERSION < NTDDI_VISTA
  // Windows XP doesn't have condition variables, so we will implement our own condition variable with a semaphore
  // implementation as described in a paper written by Douglas C. Schmidt and Irfan Pyarali.
  Mutex wait_mtx_;
  long waiting_;
  
  HANDLE sema_;
  HANDLE event_;

  bool broadcasted_;
#else
  // Windows Vista/Server 2008 and later has native support for condition variables.
  CONDITION_VARIABLE cv_;
#endif
};

// Thread-safe initialization.
// Used as follows:
//      static port::OnceType init_control = LEVELDB_ONCE_INIT;
//      static void Initializer() { ... do something ...; }
//      ...
//      port::InitOnce(&init_control, &Initializer);
typedef INIT_ONCE OnceType;
#define LEVELDB_ONCE_INIT INIT_ONCE_STATIC_INIT
typedef void (*INIT_PROC)();
extern void InitOnce(port::OnceType*, INIT_PROC);

// A type that holds a pointer that can be read or written atomically
// (i.e., without word-tearing.)
class AtomicPointer {
 private:
  void * ptr_;
 public:
  // Initialize to arbitrary value
   AtomicPointer() { }

  // Initialize to hold v
  explicit AtomicPointer(void* v) : ptr_(v) { }

  // Read and return the stored pointer with the guarantee that no
  // later memory access (read or write) by this thread can be
  // reordered ahead of this read.
  void* Acquire_Load() const {
    void * p = ptr_;
    MemoryBarrier();
    return p;
  }

  // Set v as the stored pointer with the guarantee that no earlier
  // memory access (read or write) by this thread can be reordered
  // after this store.
  void Release_Store(void* v) {
    MemoryBarrier();
    ptr_ = v;
  }

  // Read the stored pointer with no ordering guarantees.
  void* NoBarrier_Load() const {
    return ptr_;
  }

  // Set va as the stored pointer with no ordering guarantees.
  void NoBarrier_Store(void* v) {
    ptr_ = v;
  }
};

// ------------------ Compression -------------------

// Store the snappy compression of "input[0,input_length-1]" in *output.
// Returns false if snappy is not supported by this port.
extern bool Snappy_Compress(const char* input, size_t input_length,
                            std::string* output);

// If input[0,input_length-1] looks like a valid snappy compressed
// buffer, store the size of the uncompressed data in *result and
// return true.  Else return false.
extern bool Snappy_GetUncompressedLength(const char* input, size_t length,
                                         size_t* result);

// Attempt to snappy uncompress input[0,input_length-1] into *output.
// Returns true if successful, false if the input is invalid lightweight
// compressed data.
//
// REQUIRES: at least the first "n" bytes of output[] must be writable
// where "n" is the result of a successful call to
// Snappy_GetUncompressedLength.
extern bool Snappy_Uncompress(const char* input_data, size_t input_length,
                              char* output);

// ------------------ Miscellaneous -------------------

// If heap profiling is not supported, returns false.
// Else repeatedly calls (*func)(arg, data, n) and then returns true.
// The concatenation of all "data[0,n-1]" fragments is the heap profile.
extern bool GetHeapProfile(void (*func)(void*, const char*, int), void* arg);

}
}

#endif  // STORAGE_LEVELDB_PORT_PORT_WINDOWS_H_
