
#ifndef VoteType_H
#define VoteType_H

#include "Utils.h"
#include "SerializableBase.h"

class VoteType : public SerializableBase
{
public:

	Hash TransactionID;
	// if vote is +ve the Vote = true
	// for -ve vote Vote = false
	bool Vote;
	VoteType();
	VoteType(Hash transactionID, bool vote);

	vector<byte> Serialize() override;
	void Deserialize(vector<byte> Data) override;
};


#endif