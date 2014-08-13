
// @Author : Arpan Jati
// @Date: 12th Aug 2014

#include "FakeNetwork.h"

#include "tbb/concurrent_queue.h"
#include "tbb/concurrent_hash_map.h"

using namespace tbb;

concurrent_queue<NetworkPacketQueueEntry> QueuedPackets;
concurrent_hash_map<Hash, Node> NetworkedNodes;
concurrent_hash_map<Hash, function<void(NetworkPacket)>> Attachments;

concurrent_hash_map<Hash, function<void(NetworkPacket)>>::accessor Hash_NetworkCallback_acc;

FakeNetwork::FakeNetwork()
{

}

FakeNetwork::FakeNetwork(HANDLE & hTimerQueue, int Resolution_MS)
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
	int Period = Resolution_MS;

	if (!CreateTimerQueueTimer(&hTimer, hTimerQueue, (WAITORTIMERCALLBACK)TimerProc, this, DueTime, Period, 0))
	{
		GoodInit = false;
	}	
}

// Called every Resolution_MS milliseconds, used to process the pending Queue
void FakeNetwork::TimerCallback()
{
	if (QueuedPackets.unsafe_size() > 0)
	{
		NetworkPacketQueueEntry PQE;
		bool OK = QueuedPackets.try_pop(PQE);
		do 
		{			
			OK = QueuedPackets.try_pop(PQE);			
			bool found = Attachments.find(Hash_NetworkCallback_acc, PQE.PublicKey_Dest);
			if (found)
			{
				// Destination Exists !!! Now send the packet
				Hash_NetworkCallback_acc->second(PQE.Packet);
			}
		} while (OK);
	}
}

void FakeNetwork::SendPacket(NetworkPacketQueueEntry Packet)
{
	QueuedPackets.push(Packet);
}

void FakeNetwork::AttachReceiver(Hash SourcePK, function<void(NetworkPacket)> callback)
{
	Attachments.insert(std::make_pair(SourcePK, callback));
}

void CALLBACK TimerProc(void* lpParametar, BOOLEAN TimerOrWaitFired)
{
	FakeNetwork* obj = (FakeNetwork*)lpParametar;
	obj->TimerCallback();
}


