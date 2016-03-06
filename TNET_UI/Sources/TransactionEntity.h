
#ifndef TransactionEntity_H
#define TransactionEntity_H

#include "Utils.h"
#include "SerializableBase.h"

class TransactionEntity : SerializableBase
{
public:
	Hash PublicKey;
	int64_t Amount;
	TransactionEntity(Hash _PublicKey, int64_t _Amount);
	TransactionEntity();

	vector<byte> Serialize() override;
	void Deserialize(vector<byte> Data) override;
};

#endif