
// @Author : Arpan Jati
// @Date: 12th Aug 2014

#ifndef FAKE_NETWORK_H
#define FAKE_NETWORK_H

#include <Windows.h>

#include "Utils.h"

//#include "Timer.h"


#include <Windows.h>
#include <Threadpoolapiset.h>

#include <functional>

//void CALLBACK TimerProc(void* lpParametar, BOOLEAN TimerOrWaitFired);

//typedef void(*NetworkCallback)(NetworkPacket Packet);

VOID CALLBACK TimerCallbackX(PTP_CALLBACK_INSTANCE Instance, PVOID Parameter, PTP_TIMER Timer);

class FakeNetwork
{
	//HANDLE hTimer = NULL;

	PTP_TIMER timer = NULL;
	PTP_POOL pool = NULL;
	PTP_TIMER_CALLBACK timercallback = TimerCallbackX;
	TP_CALLBACK_ENVIRON CallBackEnvironment;
	PTP_CLEANUP_GROUP cleanupgroup = NULL;

public:

	bool GoodInit = true;
	FakeNetwork();
	FakeNetwork(HANDLE & hTimerQueue, int ResolutionMS);
	void SendPacket(NetworkPacketQueueEntry Packet);
	void AttachReceiver(Hash SourcePK, function<void(NetworkPacket)> callback);
	void TimerCallback();
};

#endif


