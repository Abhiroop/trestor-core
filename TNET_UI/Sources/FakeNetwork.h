
#ifndef FAKE_NETWORK_H
#define FAKE_NETWORK_H

#include "Utils.h"
#include "NodeController.h"

#include "Timer.h"

using namespace TimerX;

extern hash_map<Hash, Node> NetworkedNodes;

struct NetworkPacketQueueEntry
{
	Hash PublicKey_Src;
	Hash PublicKey_Dest;
	byte Type;
	vector<byte> Data;
};

struct NetworkPacket
{
	Hash PublicKey;
	byte Type;
	vector<byte> Data;
};

// //////////////////////////////////////////////////////////////////////////////

void CALLBACK TimerProc(void* lpParametar, BOOLEAN TimerOrWaitFired);


class FakeNetwork
{
	typedef void(*NetworkCallback)(NetworkPacket Packet);

	hash_map<Hash, NetworkCallback> Attachments;

	queue<NetworkPacketQueueEntry> QueuedPackets;

	void static FakeNetworkCallback()
	{

	}

	HANDLE hTimer = NULL;
	HANDLE hTimerQueue = NULL;

public:

	FakeNetwork()
	{

		// Create the timer queue.
		hTimerQueue = CreateTimerQueue();
		if (NULL == hTimerQueue)
		{
			//	printf("CreateTimerQueue failed (%d)\n", GetLastError());
			//	//return 2;
		}

		int DueTime = 0;
		int Period = 100; //100ms

		//CallBack = _Callback;
		// Set a timer to call the timer routine in 10 seconds.
		if (!CreateTimerQueueTimer(&hTimer, hTimerQueue, (WAITORTIMERCALLBACK)TimerProc, this, DueTime, Period, 0))
		{
			printf("CreateTimerQueueTimer failed (%d)\n", GetLastError());
			//return 3;
		}

	}

	void TimerCallback()
	{

	}

	void SendPacket(NetworkPacketQueueEntry Packet)
	{		
		QueuedPackets.push(Packet);
	}

	void AttachReceiver(NetworkCallback callback)
	{

		//Attachments.erase(Hash());
	}
	
};

void CALLBACK TimerProc(void* lpParametar, BOOLEAN TimerOrWaitFired)
{
	FakeNetwork* obj = (FakeNetwork*)lpParametar;
	obj->TimerCallback();
}

#endif
