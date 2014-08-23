#ifndef LedgerHandler_H
#define LedgerHandler_H

#include <string>
#include "SQLiteCpp\CppSQLite3.h"
#include <stdint.h>
#include "Constants2.h"

using namespace std;

class LedgerHandler
{
public:

	int transaction(string senderName, string receiverName, int64_t transactionAmount);
	int64_t getBalance(string UserName);

};
#endif