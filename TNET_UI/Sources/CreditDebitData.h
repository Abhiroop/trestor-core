#ifndef CREDITDEBIT_H
#define CREDITDEBIT_H

#include "Utils.h"
#include "AccountInfo.h"

class CreditDebitData
{
public:
	AccountInfo ai;
	int64_t Money;
	int64_t Credits;
	int64_t Debits;
	
	CreditDebitData(AccountInfo _ai, int64_t _Money, int64_t _Credits, int64_t _Debits);
	CreditDebitData();
};
#endif