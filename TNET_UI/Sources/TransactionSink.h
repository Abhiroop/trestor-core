
#ifndef TRNSACTIONSINK_H
#define TRNSACTIONSINK_H

#include "Utils.h"
#include "SerializableBase.h"

class TransactionSink : SerializableBase
{
public:
	Hash PublicKey_Sink;
	int64_t Amount;
	TransactionSink(Hash _PublicKey_Sink, int64_t _Amount);

	vector<byte> Serialize() override;
	void Deserialize(vector<byte> Data) override;
};

#endif