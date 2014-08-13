
#include "FakeNetwork.h"

hash_map<Hash, Node> NetworkedNodes;




void CALLBACK TimerProc(void* lpParametar, BOOLEAN TimerOrWaitFired)
{
	FakeNetwork* obj = (FakeNetwork*)lpParametar;
	obj->TimerCallback();
}


