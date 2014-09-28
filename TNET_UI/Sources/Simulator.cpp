//
// @Author : Arpan Jati
// @Date: 12th Aug 2014
//

#include "Simulator.h"
#include "Constants.h"
#include "Timer.h"

#include <functional>
#include <memory>

#include "tbb/concurrent_hash_map.h"

using namespace std;
using namespace std::placeholders;

//hash_map<Hash, shared_ptr<Node>> sim_nodes;
vector<Point2> sim_XY;
hash_map<Hash, NodeData> sim_nData;
//tbb::concurrent_hash_map<Hash, Node> NetworkedNodes;

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
	Simulator::Simulator(Constants::SIM_REFRESH_MS_SIM);
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

	Initialize(Constants::SIM_REFRESH_MS);
}


void Simulator::StartSimulation()
{
	//sim_nodes.clear();

	GlobalNodes.clear();

	//Ledger lgr;

	shared_ptr<Ledger> lgr(new Ledger());

	vector<Hash*> nodeHashes;

	for (int i = 0; i < 8; i++)
	{
		string CN = "NO_NAME_" + to_string(i) + "_";

		//Node n3 = Node(CN, 4, lgr, 100 * i, 100);

		shared_ptr<Node> NewNode(new Node(network, CN, 4, lgr, 100 * i, 100));
		
		//Node *n = dynamic_cast<Node*> (&n3);
		//Nodes.push_back(*n);

		Hash* pk = &NewNode->PublicKey;

		//shared_ptr<Hash> pk = n->PublicKey;

		AccountInfo si = AccountInfo(*pk, 500, CN, 0, 0);
		nodeHashes.push_back(pk);
		lgr->AddUserToLedger(si);
		//sim_nodes[*pk] = NewNode;

		GlobalNodes.insert(make_pair(*pk, NewNode));

		//GlobalNodes.

		//function<void(NetworkPacket)> f = std::bind1st(std::mem_fun(&Node::ReceivedData), &n);
	
		network.AttachReceiver(*pk, bind(&Node::Receive, *NewNode, _1));
	}

	
	int LinksPerNode = 3;

	for (int i = 0; i < 8; i++)
	{
		vector<int> perm_Conn = GenerateNonRepeatingDistribution((int)nodeHashes.size(), LinksPerNode, i);

		concurrent_hash_map<Hash, shared_ptr<Node>>::accessor Map_Acc;
		concurrent_hash_map<Hash, shared_ptr<Node>>::accessor Map_Acc_2;

		for (int k = 0; k < (int)perm_Conn.size(); k++)
		{
			Hash* h = nodeHashes[perm_Conn[k]];
			//shared_ptr<Node> n = GlobalNodes[*h];
			//GlobalNodes[*nodeHashes[i]]->Connections[*h] = n;

			if (GlobalNodes.find(Map_Acc, *h))
			{
				shared_ptr<Node> n = Map_Acc->second;
				if (GlobalNodes.find(Map_Acc_2, *nodeHashes[i]))
				{
					Map_Acc_2->second->Connections[*h] = n;
				}
				
			}
		}

		vector<int> perm_Trusted= GenerateNonRepeatingDistribution((int)nodeHashes.size(), LinksPerNode, i);

		for (int k = 0; k < (int)perm_Trusted.size(); k++)
		{
			Hash* h = nodeHashes[perm_Trusted[k]];
			//shared_ptr<Node> n = GlobalNodes[*h];
			//GlobalNodes[*nodeHashes[i]]->Connections[*h] = n;

			if (GlobalNodes.find(Map_Acc, *h))
			{
				shared_ptr<Node> n = Map_Acc->second;
				if (GlobalNodes.find(Map_Acc_2, *nodeHashes[i]))
				{
					Map_Acc_2->second->TrustedNodes[*h] = n;
				}

			}
		}

		/*for (int k = 0; k < (int)perm_Trusted.size(); k++)
		{
			Hash* h = nodeHashes[perm_Trusted[k]];
			shared_ptr<Node> n = GlobalNodes[*h];
			GlobalNodes[*nodeHashes[i]]->TrustedNodes[*h] = n;
		}*/
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
