#ifndef CANDIDATE_SET_H
#define CANDIDATE_SET_H


#include <list>
#include <random>
#include <vector>
#include <hash_map>
#include "TransactionContent.h"
#include "AccountInfo.h"


class CandidateSet
{
public:

	std::hash_map<Hash,TransactionContent> _transactions;

	int getCount();

	std::hash_map<Hash, TransactionContent>  getTransactions();

	CandidateSet();

	void AddTransaction(TransactionContent transaction);

	//void GenerateTransactions(vector<AccountInfo> accounts);

};

#endif