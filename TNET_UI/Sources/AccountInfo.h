/*
*
*  @Author: Arpan Jati + Aritra Dhar
*  @Version: 1.0
*/

#ifndef AccountInfo_H
#define AccountInfo_H

#include "Utils.h"
#include "LeafDataType.h"
#include "SerializableBase.h"

class AccountInfo : public LeafDataType, SerializableBase
{
public:

	Hash AccountID;
	int64_t Money;
	string Name;
	byte IsBlocked;
	int64_t LastTransactionTime;

	AccountInfo();
	AccountInfo(Hash _AccountID, int64_t _Money, string Name, byte IsBlocked, int64_t LastTransactionTime);
	Hash GetHash();
	Hash GetID();

	vector<byte> Serialize() override;
	void Deserialize(vector<byte> Data) override;
};

#endif
