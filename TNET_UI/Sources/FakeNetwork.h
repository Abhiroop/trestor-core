
// @Author : Arpan Jati
// @Date: 12th Aug 2014

#ifndef FAKE_NETWORK_H
#define FAKE_NETWORK_H

#include <Windows.h>

#include "Utils.h"

//#include "Timer.h"

#include <functional>

void CALLBACK TimerProc(void* lpParametar, BOOLEAN TimerOrWaitFired);

//typedef void(*NetworkCallback)(NetworkPacket Packet);

class FakeNetwork
{
	HANDLE hTimer = NULL;

public:

	bool GoodInit = true;
	FakeNetwork();
	FakeNetwork(HANDLE & hTimerQueue, int ResolutionMS);	
	void SendPacket(NetworkPacketQueueEntry Packet);
	void AttachReceiver(Hash SourcePK, function<void(NetworkPacket)> callback);
	void TimerCallback();
};

#endif


