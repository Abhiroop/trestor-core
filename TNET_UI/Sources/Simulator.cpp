///
// @Author : Arpan Jati
// @Date: 12th Aug 2014
//

#include "Simulator.h"
#include "Constants.h"
#include "Timer.h"

#include <functional>
#include <memory>

using namespace std;
using namespace std::placeholders;

hash_map<Hash, shared_ptr<Node>> sim_nodes;
vector<Point2> sim_XY;
hash_map<Hash, NodeData> sim_nData;

void Simulator::Timestep()
{
	if (SimulationStarted)
	{
		//hash<Hash> h;
		//Refreshed = true;


	}
}

void Simulator::Initialize(int Resolution_MS)
{
	network = FakeNetwork(hTimerQueue, Resolution_MS);
}

Simulator::Simulator()
{
	Simulator::Simulator(Constants::SIM_REFRESH_MS);
}

Simulator::Simulator(int Resolution_MS)
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

	if (!CreateTimerQueueTimer(&hTimer, hTimerQueue, (WAITORTIMERCALLBACK)TimerProcS, this, DueTime, Period, 0))
	{
		GoodInit = false;
	}

	Initialize(Resolution_MS);
}


void Simulator::StartSimulation()
{
	sim_nodes.clear();

	GlobalNodes.clear();

	//Ledger lgr;

	shared_ptr<Ledger> lgr(new Ledger());

	vector<Hash*> nodeHashes;

	for (int i = 0; i < 8; i++)
	{
		string CN = "NO_NAME_" + to_string(i) + "_";

		//Node n3 = Node(CN, 4, lgr, 100 * i, 100);

		shared_ptr<Node> n(new Node(CN, 4, lgr, 100 * i, 100));
		
		//Node *n = dynamic_cast<Node*> (&n3);

		//Nodes.push_back(*n);

		Hash* pk = &n->PublicKey;

		//shared_ptr<Hash> pk = n->PublicKey;

		AccountInfo si = AccountInfo(*pk, 500, CN, 0);
		nodeHashes.push_back(pk);
		lgr->AddUserToLedger(si);
		sim_nodes[*pk] = n;

		GlobalNodes[*pk] = n.get();

		//function<void(NetworkPacket)> f = std::bind1st(std::mem_fun(&Node::ReceivedData), &n);
	
		network.AttachReceiver(*pk, bind(&Node::Receive, *n, _1));
	}

	int LinksPerNode = 3;

	for (int i = 0; i < 8; i++)
	{
		vector<int> perm = GenerateNonRepeatingDistribution((int)nodeHashes.size(), LinksPerNode, i);

		for (int k = 0; k < (int)perm.size(); k++)
		{
			Hash* h = nodeHashes[perm[k]];
			shared_ptr<Node> n = sim_nodes[*h];
			sim_nodes[*nodeHashes[i]]->Connections[*h] = n;
		}
	}

	SimulationStarted = true;
}

void Simulator::StopSimulation()
{
	SimulationStarted = false;
}

void CALLBACK TimerProcS(void* lpParametar, BOOLEAN TimerOrWaitFired)
{
	Simulator* obj = (Simulator*)lpParametar;
	obj->Timestep();
	/*
	for (hash_map<Hash, Node*>::iterator links = GlobalNodes.begin(); links != GlobalNodes.end(); ++links)
	{
		links->second->UpdateEvent();
	}*/

}
