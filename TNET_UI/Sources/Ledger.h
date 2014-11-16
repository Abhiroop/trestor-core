#ifndef LEDGER_H
#define LEDGER_H

#include <hash_map>
#include <hash_set>
#include "HashTree.h"
#include "AccountInfo.h"
#include "TransactionContent.h"
#include "CreditDebitData.h"
#include "LedgerRootInfo.h"
#include "LedgerFileHandler.h"

// FOR TESTING
#include "FakeNetwork.h"

//SHIT JUST GOT REAL HERE!!!!

typedef struct LedgerOpStatistics
{
public:
	int64_t TotalTransactions = 0;
	int64_t FailedTransactions = 0;
	int64_t GoodTransactions = 0;
	int64_t BlackLists = 0;
	bool Response;
} LedgerOpStatistics;

class Ledger
{

private:

	vector<byte> GetRootInfo();

public:

	FakeNetwork network;
	
	LedgerRootInfo ledgerRootInfo;
	HashTree<AccountInfo, LedgerRootInfo> LedgerTree;
	LedgerFileHandler ledgerFileHandler;

	int64_t TransactionFees;
	int64_t CloseTime;

	/// <summary>
	/// List of public nodes which have sent bad transactions.
	/// </summary>
	hash_set<Hash> BlackList;

	Ledger();

	Ledger(string LedgerDB_FileName, FakeNetwork network);

	bool GetAccount(Hash userInfo, AccountInfo & ltd);

	bool GetAccountBalance(Hash userInfo, int64_t & balance);

	/// <summary>
	/// Add User, false if user already exists.
	/// </summary>
	/// <param name="userInfo"></param>
	/// <returns></returns>

	bool AddUserToLedger(AccountInfo userInfo);

	/// <summary>
	/// </summary>
	/// <param name="Candidates"></param>
	/// <returns></returns>
	//LedgerOpStatistics ApplyTransactionToLedger(hash_map<Hash, TransactionContent> Candidates);

	void RefreshValidTransactions();

	//hash_map<Hash, TransactionContent> getCandidates();

	Hash GetRootHash();

	void ProcessIncomingPacket(NetworkPacket packet);


};


#endif