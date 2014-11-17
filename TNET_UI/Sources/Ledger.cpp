
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




void Ledger::ProcessIncomingPacket(NetworkPacket packet)
{

	switch (packet.Type)
	{

	case TPT_LSYNC_FETCH_ROOT:

	{
								 // The client is asking for the root of the ledger.
								 // RoothHash, LCL_Time, 16 Nodes(treenode), cons seq counter,
								 //LedgerRootInfo lri= LedgerTree.GetRootInfo();

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
											 TreeSyncData TSD = TreeSyncData(h, vc, jj->LeafCount, false);

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

		break;

	case TPT_LSYNC_REPLY_ROOT:

	{
								 TreeSyncRootData tsrd;
								 tsrd.Deserialize(packet.Data);

								 vector<vector<unsigned char>> ResponseList;

								 bool ok = ProtocolPackager::UnpackVectorVector(tsrd.TSDs, ResponseList);

								 if (ok)
								 {
									 

									 
									 for (int i = 0; i < 16; i++)
									 {
										 TreeSyncData tsd;
										 tsd.Deserialize(ResponseList[i]);




									 }
								 }


	}

		break;


	}


}