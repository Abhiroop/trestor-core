///
// @Author : Arpan Jati
// @Date: 12th Aug 2014
//

#include "Simulator.h"
#include "Constants.h"
#include "Timer.h"

hash_map<Hash, Node> sim_nodes;
vector<Point2> sim_XY;
hash_map<Hash, NodeData> sim_nData;

void Simulator::Timestep()
{
	if (SimulationStarted)
	{

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

	Ledger lgr;

	vector<Hash> nodeHashes;

	for (int i = 0; i < 8; i++)
	{
		Node n = Node("NO_NAME", 4, lgr, 100 * i, 100);
		AccountInfo si = AccountInfo(n.PublicKey, 500, "NO_NAME", 0);
		nodeHashes.push_back(n.PublicKey);
		lgr.AddUserToLedger(si);
		sim_nodes[n.PublicKey] = n;

		GlobalNodes[n.PublicKey] = n;

		//function<void(NetworkPacket)> f = std::bind1st(std::mem_fun(&Node::ReceivedData), &n);
		function<void(NetworkPacket)> f = std::bind(&Node::Receive, &n, std::placeholders::_1);

		network.AttachReceiver(n.PublicKey, f);
	}

	int LinksPerNode = 3;

	for (int i = 0; i < 8; i++)
	{
		vector<int> perm = GenerateNonRepeatingDistribution((int)nodeHashes.size(), LinksPerNode, i);

		for (int k = 0; k < (int)perm.size(); k++)
		{
			Hash h = nodeHashes[perm[k]];
			Node n = sim_nodes[h];
			sim_nodes[nodeHashes[i]].Connections[h] = n;
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
}
