
/*
*
*  @Author: Arpan Jati + Aritra Dhar
*  @Version: 1.0
*  @Date: August 2014
*/

#include "Ledger.h"

Ledger::Ledger()
{
	TransactionFees = 0;
	ledgerFileHandler = LedgerFileHandler(LedgerTree, "LedgerT.db");
}

Ledger::Ledger(string LedgerDB_FileName, FakeNetwork _network)
{
	network = _network;
	TransactionFees = 0;
	ledgerFileHandler = LedgerFileHandler(LedgerTree, LedgerDB_FileName);
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

void Ledger::ProcessIncomingPacket(NetworkPacket packet)
{

	switch (packet.Type)
	{

	case TPT_LSYNC_FETCH_ROOT:
	case TPT_LSYNC_FETCH_LAYER_INFO:
	case TPT_LSYNC_FETCH_LAYER_DATA:




		break;


	}


}