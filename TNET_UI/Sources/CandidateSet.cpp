
#include "CandidateSet.h"

int CandidateSet::getCount()
{
	return _transactions.size();
}

std::hash_map<Hash, TransactionContent>  CandidateSet::getTransactions()
{
	return _transactions;
}

CandidateSet::CandidateSet()
{
	//_transactions;
}

void CandidateSet::AddTransaction(TransactionContent transaction)
{
	_transactions[transaction.GetHash()]= transaction;	
}

void CandidateSet::GenerateTransactions(vector<AccountInfo> accounts)
{
	_transactions.clear();

	int totalAccounts = (int)accounts.size();
	
	std::default_random_engine generator;

	generator.seed(rand());

	std::uniform_int_distribution<int> distribution(1, totalAccounts / 5);
	std::uniform_int_distribution<int> distribution_total(0, totalAccounts-1);

	double lower_bound = 0;
	double upper_bound = 1;
	std::uniform_real_distribution<double> unif(lower_bound, upper_bound);

	std::default_random_engine re;

	re.seed(rand());

	for (int i = 0; i < (int)accounts.size();i++)
	{
		AccountInfo account = accounts[i];

		int nodes = distribution(generator);

		long amountToSpend = (long)(account.Money * 1.5F);

		long spendingAmt = (amountToSpend / (long)nodes);

		vector<TransactionSink> tsks;

		for (int dest = 0; dest < nodes; dest++)
		{
			int _id = distribution_total(generator);
			AccountInfo sinkAccount = accounts[_id];

			if (sinkAccount.AccountID == account.AccountID) continue;
			double d = unif(re);
			long amountToSpendPerSink = (long)(spendingAmt * d);

			TransactionSink tsk = TransactionSink(sinkAccount.AccountID, amountToSpendPerSink);
			tsks.push_back(tsk);
		}

		TransactionContent tco =  TransactionContent(account.AccountID, 0, tsks, Hash() );
		Hash h = tco.GetHash();
		_transactions[h] = tco;
	}

}