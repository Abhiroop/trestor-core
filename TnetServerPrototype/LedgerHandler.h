#ifndef LedgerHandler_H
#define LedgerHandler_H

#include <string>
#include "SQLiteCpp\CppSQLite3.h"
#include <stdint.h>
#include <time.h>
#include "Constants2.h"
#include <functional>
#include "GetBalanceType.h"

using namespace std;

class LedgerHandler
{
	
public:

	int transaction(string senderName, string receiverName, int64_t transactionAmount, function<void(string)> transactionEvent);
	GetBalanceType getBalance(string UserName, const __int64 time, function<void(string)> transactionEvent);


};
#endif