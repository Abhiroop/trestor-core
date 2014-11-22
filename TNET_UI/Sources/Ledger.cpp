
/*
*
*  @Author: Arpan Jati + Aritra Dhar
*  @Version: 1.0
*  @Date: August 2014
*/

#include "Ledger.h"
#include "TreeSyncData.h"
#include "TreeSyncRootData.h"
#include "ProtocolPackager.h"
#include "Constants.h"

Ledger::Ledger()
{
	TransactionFees = 0;
	//ledgerFileHandler = LedgerFileHandler(LedgerTree, "LedgerT.db");
}

Ledger::Ledger(State _state, string LedgerDB_FileName, FakeNetwork _network)
{
	network = _network;
	TransactionFees = 0;
	ledgerFileHandler = LedgerFileHandler(LedgerTree, LedgerDB_FileName);
	state = _state;

	// Load Database
	LedgerTree = ledgerFileHandler.DBToTree();
}

bool Ledger::AddUserToLedger(AccountInfo userInfo)
{
	// MAKE sure account does not exist
	AccountInfo ltd;
	bool contains = LedgerTree.GetNodeData(userInfo.AccountID, ltd);

	if (!contains)
	{
		LedgerTree.AddUpdate(userInfo);
		return true;
	}
	else
	{
		return false;
	}
}

bool Ledger::GetAccount(Hash userInfo, AccountInfo & ltd)
{
	return LedgerTree.GetNodeData(userInfo, ltd);
}

bool Ledger::GetAccountBalance(Hash userInfo, int64_t & balance)
{
	AccountInfo ac;

	if (LedgerTree.GetNodeData(userInfo, ac))
	{
		balance = ac.Money;
		return true;
	}

	return false;
}

void Ledger::RefreshValidTransactions()
{

}

Hash Ledger::GetRootHash()
{
	return LedgerTree.GetRootHash();
}

vector<byte> Ledger::GetRootInfo()
{
	Hash RootHash = GetRootHash();
	return RootHash;
}

// This gets the active nodes under a treenodex as an unsigned short,
// All the LSB contains the state of Child[0] and the MSB for Child[15]
uint16_t Ledger::GetActiveNodes(TreeNodeX* node)
{
	uint16_t val = 0;

	if (node != nullptr)
	{
		for (int i = 0; i < 16; i++)
		{
			bool has_Child = node->Children[i] != nullptr;
			if (has_Child)
			{
				val |= (1 << i);
			}
		}		
	}

	return val;
}

void Ledger::ProcessIncomingPacket(NetworkPacket packet)
{

	switch (packet.Type)
	{

	case TPT_LSYNC_FETCH_ROOT:

	{
		// YOU ARE THE SERVER : HAVING UPDATED LEDGER
		// The client is asking for the root of the ledger.
		// RoothHash, LCL_Time, 16 Nodes(treenode), cons seq counter,
		// LedgerRootInfo lri= LedgerTree.GetRootInfo();

		vector<TreeNodeX*> x;

		bool ok = LedgerTree.getImmediateChildren("", x);

		vector<vector<unsigned char>> ResponseList;

		if (ok)
		{
			for (int i = 0; i < 16; i++)
			{
				TreeNodeX* jj = x[i];

				if (jj != nullptr)
				{
					Hash h = Hash(jj->ID, jj->ID + 32);

					vector<char> vc;
					TreeSyncData TSD = TreeSyncData(h, vc, jj->LeafCount, GetActiveNodes(jj), false);

					ResponseList.push_back(TSD.Serialize());
				}
			}

			LedgerRootInfo ri = LedgerTree.GetRootInfo();

			TreeSyncRootData tsrd;
			tsrd.TSDs = ProtocolPackager::Pack(ResponseList);
			tsrd.LCL_Time = ri.LCLTime;
			tsrd.RootHash = ri.LedgerHash;
			tsrd.NodeCount = ri.SequenceNumber;

			NetworkPacketQueueEntry npqe;

			//  Set the reply address
			npqe.PublicKey_Dest = packet.PublicKey_Src;
			npqe.Packet.Token = packet.Token;
			npqe.Packet.Type = TPT_LSYNC_REPLY_ROOT;
			npqe.Packet.PublicKey_Src = state.PublicKey;
			npqe.Packet.Data = tsrd.Serialize();

			network.SendPacket(npqe);

		}



	}

	case TPT_LSYNC_FETCH_LAYER_INFO:
	case TPT_LSYNC_FETCH_LAYER_DATA:

	{
		// YOU ARE THE SERVER : HAVING LATEST LEDGER

		// CHECK FOR LATEST

	}

		break;

	case TPT_LSYNC_REPLY_ROOT:

	{
		// YOU ARE THE CLIENT : HAVING OLD LEDGER
		// The server has replied with the latest ledger entries. 
		// Get the difference and request more items accordingly

		TreeSyncRootData tsrd;
		tsrd.Deserialize(packet.Data);

		vector<vector<unsigned char>> ResponseList;

		bool ok = ProtocolPackager::UnpackVectorVector(tsrd.TSDs, ResponseList);

		if (ok)
		{
			vector<TreeSyncData> TSD;
			for (int i = 0; i < (int)ResponseList.size(); i++)
			{
				TreeSyncData tsd;
				tsd.Deserialize(ResponseList[i]);
				TSD.push_back(tsd);
			}

			if (TSD.size() > 0)
			{
				// Have the latest ledger items in row 0, calculate difference and fetch data.
				vector<TreeSyncData> root_level = LedgerTree.TraverseLevelOrderDepthSync(0);
				vector<TreeSyncData> TSD_Req = LedgerTree.GetDifference(TSD, root_level);

				if (TSD_Req.size() > 0)
				{
					vector<vector<unsigned char>> ResponseList;

					for (int i = 0; i < (int)TSD_Req.size(); i++)
					{
						ResponseList.push_back(TSD_Req[i].Serialize());
					}
				

					NetworkPacketQueueEntry npqe;

					//  Set the reply address
					npqe.PublicKey_Dest = packet.PublicKey_Src;
					npqe.Packet.Token = packet.Token;
					npqe.Packet.Type = TPT_LSYNC_FETCH_LAYER_DATA;
					npqe.Packet.PublicKey_Src = state.PublicKey;
					npqe.Packet.Data = ProtocolPackager::Pack(ResponseList);

					network.SendPacket(npqe);
				}
			}
		}


	}

		break;


	}


}