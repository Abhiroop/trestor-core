

#include"CreditDebitData.h"


CreditDebitData::CreditDebitData(AccountInfo _ai, int64_t _Money, int64_t _Credits, int64_t _Debits)
{
	ai = _ai;
	Money = _Money;
	Credits = _Credits;
	Debits = _Debits;
}

CreditDebitData::CreditDebitData()
{

}