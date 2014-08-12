
#ifndef TRNSACTIONSINK_H
#define TRNSACTIONSINK_H

#include "Utils.h"

class TransactionSink
{
public:
	Hash PublicKey_Sink;
	int64_t Amount;
	TransactionSink(Hash _PublicKey_Sink, int64_t _Amount);
};

#endif