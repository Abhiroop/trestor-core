#ifndef TIMER_H
#define TIMER_H

#ifdef __cplusplus_cli

#include <windows.h>

// @Author : Arpan Jati
// @Date: 12th Aug 2014

namespace TimerX
{

	/*HANDLE gDoneEvent;

	HANDLE hTimer = NULL;
	HANDLE hTimerQueue = NULL;
	int arg = 123;*/


	class Timer
	{
	private:

		HANDLE hTimer = NULL;
		HANDLE hTimerQueue = NULL;

		int DueTime;
		int Period;

		//static void(*CallBack)();// = NULL;

	public:

		/*VOID CALLBACK TimerRoutine(PVOID lpParam, BOOLEAN TimerOrWaitFired)
		{
			CallBack();
		}
		*/

		Timer(int _DueTime, int _Period, void(*_Callback)());

		//void Tick(void(*_Callback)());

	};

	/*


	VOID CALLBACK TimerRoutine(PVOID lpParam, BOOLEAN TimerOrWaitFired)
	{
	if (lpParam == NULL)
	{
	printf("TimerRoutine lpParam is NULL\n");
	}
	else
	{
	// lpParam points to the argument; in this case it is an int

	printf("Timer routine called. Parameter is %d.\n",
	*(int*)lpParam);
	if (TimerOrWaitFired)
	{
	printf("The wait timed out.\n");
	}
	else
	{
	printf("The wait event was signaled.\n");
	}
	}

	SetEvent(gDoneEvent);
	}

	int OnButtonBegin()
	{

	/// Use an event object to track the TimerRoutine execution
	gDoneEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
	if (NULL == gDoneEvent)
	{
	printf("CreateEvent failed (%d)\n", GetLastError());
	return 1;
	}

	// Create the timer queue.
	hTimerQueue = CreateTimerQueue();
	if (NULL == hTimerQueue)
	{
	printf("CreateTimerQueue failed (%d)\n", GetLastError());
	return 2;
	}

	// Set a timer to call the timer routine in 10 seconds.
	if (!CreateTimerQueueTimer(&hTimer, hTimerQueue,
	(WAITORTIMERCALLBACK)TimerRoutine, &arg, 10000, 0, 0))
	{
	printf("CreateTimerQueueTimer failed (%d)\n", GetLastError());
	return 3;
	}

	// TODO: Do other useful work here

	printf("Call timer routine in 10 seconds...\n");

	// Wait for the timer-queue thread to complete using an event
	// object. The thread will signal the event at that time.

	if (WaitForSingleObject(gDoneEvent, INFINITE) != WAIT_OBJECT_0)
	printf("WaitForSingleObject failed (%d)\n", GetLastError());

	CloseHandle(gDoneEvent);

	// Delete all timers in the timer queue.
	if (!DeleteTimerQueue(hTimerQueue))
	printf("DeleteTimerQueue failed (%d)\n", GetLastError());

	}

	void OnButtonStop()
	{
	// destroy the timer
	DeleteTimerQueueTimer(NULL, hTimer, NULL);
	CloseHandle(hTimer);
	}

	*/

}

#else

// REF: https://github.com/laurolins/nanocube/blob/master/src/util/timer.hh

#include <chrono>
#include <string>
#include <iostream>
#include <initializer_list>
#include <vector>
#include <deque>
#include <thread>
#include <mutex>

#include "Signal.h"

namespace timer {

	using Milliseconds = std::chrono::milliseconds;
	using Timestamp = std::chrono::high_resolution_clock::time_point;

	using TickCallback = std::function<void()>;

	struct Timer {

		Timer(Milliseconds tick_delay);

		~Timer();

		void run();
		void stop();

		void subscribe(TickCallback callback);

	public: // data members
		Milliseconds                tick_delay;
		bool                        stop_flag{ false };
		std::thread                 thread;
		sig::Signal<>               on_tick;

		std::vector<TickCallback>   to_subscribe;

		std::mutex                  mutex;
	};

	//    namespace io {
	//        std::ostream& operator<<(std::ostream &os, const Timer &timer);
	//    }

	//
	// Usage:
	// {
	//    timer::Timer timer(1000}; // sleep for one second and tick
	//
	//    auto f = []() {
	//        std::cout << "tick" << std::endl;
	//    }
	//
	//    start;
	//
	// }
	//

}

#endif // __cplusplus_cli

#endif // TIMER_H