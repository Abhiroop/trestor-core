
#include "TransactionSetType.h"

TransactionSetType::TransactionSetType()
{
}

TransactionSetType::TransactionSetType(Hash publicKey, vector<TransactionIDInfo> tranIDinfo)
{
	PublicKey = publicKey;
	TranIDinfo = tranIDinfo;
}

Hash TransactionSetType::GetID()
{
	return PublicKey;
}