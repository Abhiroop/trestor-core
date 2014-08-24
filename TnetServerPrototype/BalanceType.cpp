

#include "BalanceType.h"


void BalanceType::setBalance(int64_t _balance)
{
	balance = _balance;
}

void BalanceType::setTransactionHistory(vector<string> _transactionHistory)
{
	transactionHistory = _transactionHistory;
}

int64_t BalanceType::getBalance()
{
	return balance;
}

vector<string> BalanceType::getTransactionHistory()
{
	return transactionHistory;
}