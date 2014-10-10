
#ifndef TRANSACTIONCONTENT_H
#define TRANSACTIONCONTENT_H

#include "HashTree.h"
#include "Utils.h"
#include "TransactionEntity.h"
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
	vector<TransactionEntity> getDestinations();
	Hash getSignature();*/

	vector<TransactionEntity> Sources;
	int64_t Timestamp;
	vector<TransactionEntity> Destinations;
	vector<Hash> Signatures;

	TransactionContent(vector<TransactionEntity> _PublicKey_Source, int64_t _Timestamp, vector<TransactionEntity> _Destinations, vector<Hash> _Signature);

	TransactionContent();

	Hash GetTransactionData();

	void UpdateAndSignContent(vector<TransactionEntity> _PublicKey_Source, int64_t _Timestamp, vector<TransactionEntity> _Destinations, vector<Hash> _ExpandedPrivateKey);

	bool IntegrityCheck();

	bool VerifySignature();

	Hash GetHash();

	Hash GetID();

	Hash GetTransactionDataAndSignature();

	vector<byte> Serialize() override;
	void Deserialize(vector<byte> Data) override;	

};

#endif