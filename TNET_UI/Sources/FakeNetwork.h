
#ifndef FAKE_NETWORK_H
#define FAKE_NETWORK_H

#include "Utils.h"
#include "Node.h"

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

public:

	bool GoodInit = true;

	FakeNetwork()
	{

	}

	FakeNetwork(HANDLE & hTimerQueue)
	{
		// Create a new Timer Queue If not there already
		if (NULL == hTimerQueue)
		{
			hTimerQueue = CreateTimerQueue();
		}
		
		// Fail if Init fails
		if (NULL == hTimerQueue)
		{
			GoodInit = false;
		}

		int DueTime = 0;
		int Period = 100;

		if (!CreateTimerQueueTimer(&hTimer, hTimerQueue, (WAITORTIMERCALLBACK)TimerProc, this, DueTime, Period, 0))
		{
			GoodInit = false;
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



#endif
