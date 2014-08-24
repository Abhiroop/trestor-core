

#include "GetBalanceType.h"


void GetBalanceType::setBalance(int64_t _balance)
{
	balance = _balance;
}

void GetBalanceType::setTransactionHistory(vector<string> _transactionHistory)
{
	transactionHistory = _transactionHistory;
}

int64_t GetBalanceType::getBalance()
{
	return balance;
}

vector<string> GetBalanceType::getTransactionHistory()
{
	return transactionHistory;
}