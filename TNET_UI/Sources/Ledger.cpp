
/*
*
*  @Author: Arpan Jati + Aritra Dhar
*  @Version: 1.0
*  @Date: August 2014
*/

#include "Ledger.h"
#include "ProtocolPackager.h"
#include "Constants.h"

Ledger::Ledger()
{
	TransactionFees = 0;
	//ledgerFileHandler = LedgerFileHandler(LedgerTree, "LedgerT.db");
}

Ledger::Ledger(string LedgerDB_FileName, FakeNetwork _network)
{
	network = _network;
	TransactionFees = 0;
	ledgerFileHandler = LedgerFileHandler(LedgerTree, LedgerDB_FileName);

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
		/*
		// The client is asking for the root of the ledger.

		// Decode the request as a sequence of TransactionID's
		vector<vector<unsigned char>> TransactionIDs;
		ProtocolPackager::UnpackVectorVector_s(packet.Data, Constants::LEN_TRANSACTION_ID, TransactionIDs);

		vector<vector<unsigned char>> ResponseList;

		for (int i = 0; i < (int)TransactionIDs.size(); i++)
		{
			TransactionContentData TCD;
			// Apparently, we do have the TransactionID, yayy, add it to the list to be sent back.
			if (incomingTransactionMap.GetTransactionContentData(TCD, TransactionIDs[i]))
			{
				ResponseList.push_back(TCD.TC.Serialize());
			}
		}

		NetworkPacketQueueEntry npqe;

		//  Set the reply address
		npqe.PublicKey_Dest = packet.PublicKey_Src;
		npqe.Packet.Token = packet.Token;
		npqe.Packet.Type = TPT_CONS_RESP_TC_TX;
		npqe.Packet.PublicKey_Src = PublicKey;
		npqe.Packet.Data = ProtocolPackager::Pack(ResponseList);

		network.SendPacket(npqe);*/

	}

	case TPT_LSYNC_FETCH_LAYER_INFO:
	case TPT_LSYNC_FETCH_LAYER_DATA:




		break;


	}


}