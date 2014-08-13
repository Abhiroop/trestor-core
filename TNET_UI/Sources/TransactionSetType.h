#ifndef TransactionSetType_H
#define TransactionSetType_H


#include "HashTree.h"
#include "Utils.h"

class TransactionSetType : public LeafDataType
{
public:

	Hash PublicKey;
	set<Hash> TransactionIDs;
	set<Hash> VoterPublickey;

	bool IsMine;


	TransactionSetType(){}

	Hash GetHash();
	Hash GetID();
};


#endif