
// @Author : Arpan Jati
// @Date: 12th Aug 2014

#include <Windows.h>
#include <Threadpoolapiset.h>

#include "FakeNetwork.h"

#include "tbb/concurrent_queue.h"
#include "tbb/concurrent_hash_map.h"

using namespace tbb;

concurrent_queue<NetworkPacketQueueEntry> QueuedPackets;

concurrent_hash_map<Hash, function<void(NetworkPacket)>> Attachments;

FakeNetwork::FakeNetwork()
{

}

FakeNetwork::FakeNetwork(HANDLE & hTimerQueue, int Resolution_MS)
{
	// Create a new Timer Queue If not there already
	//if (NULL == hTimerQueue)
	//{
	//	hTimerQueue = CreateTimerQueue();
	//}

	//// Fail if Init fails
	//if (NULL == hTimerQueue)
	//{
	//	GoodInit = false;
	//}

	//int DueTime = 0;
	//int Period = Resolution_MS;

	//if (!CreateTimerQueueTimer(&hTimer, hTimerQueue, (WAITORTIMERCALLBACK)TimerProc, this, DueTime, Period, 0))
	//{
	//	GoodInit = false;
	//}

	BOOL bRet = FALSE;

	FILETIME FileDueTime;
	ULARGE_INTEGER ulDueTime;

	InitializeThreadpoolEnvironment(&CallBackEnvironment);

	pool = CreateThreadpool(NULL);

	if (NULL == pool) { GoodInit = false; }

	SetThreadpoolThreadMaximum(pool, 1);
	bRet = SetThreadpoolThreadMinimum(pool, 1);

	if (FALSE == bRet) { GoodInit = false; }

	cleanupgroup = CreateThreadpoolCleanupGroup();

	if (NULL == cleanupgroup) { GoodInit = false; }

	SetThreadpoolCallbackPool(&CallBackEnvironment, pool);
	SetThreadpoolCallbackCleanupGroup(&CallBackEnvironment, cleanupgroup, NULL);
	timer = CreateThreadpoolTimer(timercallback, this, &CallBackEnvironment);

	if (NULL == timer) { GoodInit = false; }

	ulDueTime.QuadPart = (ULONGLONG)-(1 * 10 * 1000 * 1000);
	FileDueTime.dwHighDateTime = ulDueTime.HighPart;
	FileDueTime.dwLowDateTime = ulDueTime.LowPart;
	SetThreadpoolTimer(timer, &FileDueTime, Resolution_MS, 0);

}

// Called every Resolution_MS milliseconds, used to process the pending Queue
void FakeNetwork::TimerCallback()
{
	if (QueuedPackets.unsafe_size() > 0)
	{
		NetworkPacketQueueEntry PQE;
		bool OK;
		do
		{
			OK = QueuedPackets.try_pop(PQE);

			if (!OK) break;

			concurrent_hash_map<Hash, function<void(NetworkPacket)>>::accessor Hash_NetworkCallback_acc;

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


VOID
CALLBACK
TimerCallbackX(
PTP_CALLBACK_INSTANCE Instance,
PVOID                 Parameter,
PTP_TIMER             Timer
)
{
	// Instance, Parameter, and Timer not used in this example.
	UNREFERENCED_PARAMETER(Instance);
	UNREFERENCED_PARAMETER(Timer);

	FakeNetwork* obj = (FakeNetwork*)Parameter;
	obj->TimerCallback();
}

//
//void CALLBACK TimerProc(void* lpParametar, BOOLEAN TimerOrWaitFired)
//{
//	FakeNetwork* obj = (FakeNetwork*)lpParametar;
//	obj->TimerCallback();
//}


