#ifndef LedgerHandler_H
#define LedgerHandler_H

#include <string>
#include "SQLiteCpp\CppSQLite3.h"
#include <stdint.h>
#include <time.h>
#include "Constants2.h"
#include <functional>

using namespace std;

class LedgerHandler
{
	
	function<void(string)> transactionEvent;

public:

	int transaction(string senderName, string receiverName, int64_t transactionAmount);
	int64_t getBalance(string UserName);

	void attachHandler(function<void(string)> _transactionEvent);

};
#endif