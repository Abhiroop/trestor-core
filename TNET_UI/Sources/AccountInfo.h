/*
*
*  @Author: Arpan Jati
*  @Version: 1.0
*/

#ifndef AccountInfo_H
#define AccountInfo_H


#include "HashTree.h"
#include "Utils.h"

class AccountInfo : public LeafDataType
{
public:

	Hash AccountID;
	int64_t Money;
	string Name;
	int64_t LastTransactionTime;

	AccountInfo();
	AccountInfo(Hash _AccountID, int64_t _Money, string Name, int64_t LastTransactionTime);
	Hash GetHash();
	Hash GetID();
};


#endif
