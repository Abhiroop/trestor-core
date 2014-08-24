#ifndef GetBalanceType_H
#define GetBalanceType_H

#include <stdint.h>
#include <vector>
#include <string.h>

using namespace std;

class GetBalanceType
{
	int64_t balance;
	vector<string> transactionHistory;

public:

	void setBalance(int64_t _balance)
	{
		balance = _balance;
	}

	void setTransactionHistory(vector<string> _transactionHistory)
	{
		transactionHistory = _transactionHistory;
	}

};
#endif