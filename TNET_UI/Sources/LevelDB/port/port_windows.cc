
#include "port/port_windows.h"

#include <cassert>

#ifdef HAVE_SNAPPY
#include <snappy.h>
#endif

namespace leveldb {
namespace port {

#if NTDDI_VERSION < NTDDI_VISTA

Mutex::Mutex() :
    mutex_(::CreateMutex(NULL, FALSE, NULL)) {
  assert(mutex_);
}

Mutex::~Mutex() {
  assert(mutex_);
  ::CloseHandle(mutex_);
}

void Mutex::Lock() {
  assert(mutex_);
  ::WaitForSingleObject(mutex_, INFINITE);
}

void Mutex::Unlock() {
  assert(mutex_);
  ::ReleaseMutex(mutex_);
}

void Mutex::AssertHeld() {
  assert(mutex_);
  assert(1);
}

CondVar::CondVar(Mutex* mu) :
    waiting_(0), 
    mu_(mu), 
    sema_(::CreateSemaphore(NULL, 0, 0x7fffffff, NULL)), 
    event_(::CreateEvent(NULL, FALSE, FALSE, NULL)),
    broadcasted_(false){
  assert(mu_);
}

CondVar::~CondVar() {
  ::CloseHandle(sema_);
  ::CloseHandle(event_);
}

void CondVar::Wait() {
  wait_mtx_.Lock();
  ++waiting_;
  assert(waiting_ > 0);
  wait_mtx_.Unlock();

  ::SignalObjectAndWait(mu_->mutex_, sema_, INFINITE, FALSE);

  wait_mtx_.Lock();
  bool last = broadcasted_ && (--waiting_ == 0);
  assert(waiting_ >= 0);
  wait_mtx_.Unlock();

  // we leave this function with the mutex held
  if (last)
  {
    ::SignalObjectAndWait(event_, mu_->mutex_, INFINITE, FALSE);
  }
  else
  {
    ::WaitForSingleObject(mu_->mutex_, INFINITE);
  }
}

void CondVar::Signal() {
  wait_mtx_.Lock();
  bool waiters = waiting_ > 0;
  wait_mtx_.Unlock();

  if (waiters)
  {
    ::ReleaseSemaphore(sema_, 1, 0);
  }
}

void CondVar::SignalAll() {
  wait_mtx_.Lock();

  broadcasted_ = (waiting_ > 0);

  if (broadcasted_)
  {
      // release all
    ::ReleaseSemaphore(sema_, waiting_, 0);
    wait_mtx_.Unlock();
    ::WaitForSingleObject(event_, INFINITE);
    broadcasted_ = false;
  }
  else
  {
    wait_mtx_.Unlock();
  }
}

#else // NTDDI_VERSION < NTDDI_VISTA

Mutex::Mutex() {
  InitializeCriticalSection(&cs_);
}

Mutex::~Mutex() {
  DeleteCriticalSection(&cs_);
}

void Mutex::Lock() {
  EnterCriticalSection(&cs_);
}

void Mutex::Unlock() {
  LeaveCriticalSection(&cs_);
}

void Mutex::AssertHeld() {
}

CondVar::CondVar(Mutex* mu)
    : mu_(mu) {
  InitializeConditionVariable(&cv_);
}

CondVar::~CondVar() {
}

void CondVar::Wait() {
  SleepConditionVariableCS(&cv_, &mu_->cs_, INFINITE);
}

void CondVar::Signal(){
  WakeConditionVariable(&cv_);
}

void CondVar::SignalAll() {
  WakeAllConditionVariable(&cv_);
}

BOOL CALLBACK RunInitializer(PINIT_ONCE InitOnce, PVOID Parameter, PVOID *lpContext) {
  INIT_PROC initializer = static_cast<INIT_PROC>(Parameter);
  initializer();
  return TRUE;
}

void InitOnce(port::OnceType * once, void (*initializer)()) {
  InitOnceExecuteOnce(once, RunInitializer, initializer, NULL);
}

#endif // NTDDI_VERSION < NTDDI_VISTA

bool Snappy_Compress(const char* input, size_t input_length, std::string* output) {
#ifdef HAVE_SNAPPY
  snappy::Compress(input, input_length, output);
  return true;
#else
  return false;
#endif
}

bool Snappy_GetUncompressedLength(const char* input, size_t length, size_t* result) {
#ifdef HAVE_SNAPPY
  return snappy::GetUncompressedLength(input, length, result);
#else
  return false;
#endif
}

bool Snappy_Uncompress(const char* input_data, size_t input_length, char* output) {
#ifdef HAVE_SNAPPY
  return snappy::RawUncompress(input_data, input_length, output);
#else
  return false;
#endif
}

bool GetHeapProfile(void (*func)(void*, const char*, int), void* arg) {
  return false;
}

}
}
