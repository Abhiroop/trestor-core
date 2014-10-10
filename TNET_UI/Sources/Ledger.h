#ifndef LEDGER_H
#define LEDGER_H

#include <hash_map>
#include <hash_set>
#include "HashTree.h"
#include "AccountInfo.h"
#include "TransactionContent.h"
#include "CreditDebitData.h"
#include "LedgerRootInfo.h"

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

public:

	LedgerRootInfo ledgerRootInfo;

	HashTree<AccountInfo, LedgerRootInfo> LedgerTree; //(ledgerRootInfo);
	int64_t TransactionFees;
	//int64_t TotalAmount;
	int64_t CloseTime;

	//hash_map<Hash, TransactionContent> newCandidates;

	/// <summary>
	/// List of public nodes which have sent bad transactions.
	/// </summary>
	hash_set<Hash> BlackList;

	Ledger();

	bool GetAccount(Hash userInfo, AccountInfo & ltd);

	/// <summary>
	/// Add User, false if user already exists.
	/// </summary>
	/// <param name="userInfo"></param>
	/// <returns></returns>

	bool AddUserToLedger(AccountInfo userInfo);

	/// <summary>
	/// Inserts the list of new transactions to the proposed candidate set.
	/// </summary>
	/// <param name="proposedTransactions"></param>
	//void PushNewProposedTransactions(vector<TransactionContent> proposedTransactions);

	/// <summary>
	/// Check the list of proposed transactions for consistencey, and add it to candidate set.
	/// Do it when the source is trustworthy [think].
	/// </summary>
	/// <param name="Candidates"></param>
	/// <returns></returns>
	//hash_map<Hash, TransactionContent> GetValidatedTransactions(hash_map<Hash, TransactionContent> Candidates);

	/// <summary>
	/// </summary>
	/// <param name="Candidates"></param>
	/// <returns></returns>
	//LedgerOpStatistics ApplyTransactionToLedger(hash_map<Hash, TransactionContent> Candidates);

	void RefreshValidTransactions();

	//hash_map<Hash, TransactionContent> getCandidates();

	Hash GetRootHash();

};


#endif