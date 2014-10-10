
#ifndef TRANSACTIONCONTENT_H
#define TRANSACTIONCONTENT_H

#include "HashTree.h"
#include "Utils.h"
#include "TransactionSink.h"
#include <inttypes.h>
#include <list>
#include "ed25519\ed25519.h"
#include "ed25519\sha512.h"
#include "SerializableBase.h"

class TransactionContent : LeafDataType, public SerializableBase
{

private:

	Hash intHash;
	void UpdateIntHash();

public:
	/*
	Hash getPublicKey_Source();
	int64_t getTimestamp();
	vector<TransactionSink> getDestinations();
	Hash getSignature();*/

	Hash PublicKey_Source;
	int64_t Timestamp;
	vector<TransactionSink> Destinations;
	Hash Signature;

	TransactionContent(Hash _PublicKey_Source, int64_t _Timestamp, vector<TransactionSink> _Destinations, Hash _Signature);

	TransactionContent();

	Hash GetTransactionData();

	void UpdateAndSignContent(Hash _PublicKey_Source, int64_t _Timestamp, vector<TransactionSink> _Destinations, Hash _ExpandedPrivateKey);

	bool VerifySignature();

	Hash GetHash();

	Hash GetID();

	Hash GetTransactionDataAndSignature();

	vector<byte> Serialize() override;
	void Deserialize(vector<byte> Data) override;	

};

#endif