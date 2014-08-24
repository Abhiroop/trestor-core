#ifndef GetBalanceType_H
#define GetBalanceType_H

#include <stdint.h>
#include <vector>
#include <string.h>

using namespace std;

class BalanceType
{
	int64_t balance;
	//roe in base 64 format separated by pipes
	vector<string> transactionHistory;

public:
	void setBalance(int64_t _balance);
	void setTransactionHistory(vector<string> _transactionHistory);
	int64_t getBalance();
	vector<string> getTransactionHistory();

};
#endif