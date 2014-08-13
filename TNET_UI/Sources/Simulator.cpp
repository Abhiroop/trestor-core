// @Author : Arpan Jati
// @Date: 12th Aug 2014

#include "Simulator.h"

//#include "NetworkVisualizer.h"

hash_map<Hash, Node> sim_nodes;
vector<Point2> sim_XY;
hash_map<Hash, NodeData> sim_nData;


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
}