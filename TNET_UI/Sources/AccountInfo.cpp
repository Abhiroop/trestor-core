/*
*
*  @Author: Arpan Jati + Aritra Dhar
*  @Version: 1.0
*/

#include <vector>
#include <iostream>
#include <sstream>
#include <hash_map>
#include "AccountInfo.h"
#include "HashTree.h"
#include "ed25519\sha512.h"
#include "LedgerFileHandler.h"
#include "Conversions.h"
#include "ProtocolPackager.h"

using namespace std;

AccountInfo::AccountInfo()
{

}

AccountInfo::AccountInfo(Hash _AccountID, int64_t _Money, string _Name, byte _IsBlocked, int64_t _LastTransactionTime)
{
	AccountID = _AccountID;
	Money = _Money;
	Name = _Name;
	IsBlocked = _IsBlocked;
	LastTransactionTime = _LastTransactionTime;
}


Hash AccountInfo::GetHash()
{
	vector<byte> data;
	
	data.insert(data.end(), AccountID.begin(), AccountID.end());

	vector<byte> money = Conversions::Int64ToVector(Money);

	data.insert(data.end(), money.begin(), money.end());

	data.push_back(IsBlocked);

	byte Hout[64];

	sha512(data.data(), data.size(), Hout);

	vector<byte> vec(Hout, Hout + 32);

	return vec;
	//data.AddRange(BitConverter.GetBytes(Money));
	//return new Hash((new SHA256Managed()).ComputeHash(data.ToArray()));
}

Hash AccountInfo::GetID()
{
	return AccountID;
}

vector<byte> AccountInfo::Serialize()
{
	vector<ProtocolDataType> PDTs;
	PDTs.push_back(*ProtocolPackager::Pack(AccountID, 0));
	PDTs.push_back(*ProtocolPackager::Pack(Money, 1));
	PDTs.push_back(*ProtocolPackager::Pack(Name, 2));
	PDTs.push_back(*ProtocolPackager::Pack(IsBlocked, 3));
	PDTs.push_back(*ProtocolPackager::Pack(LastTransactionTime, 4));
	return ProtocolPackager::PackRaw(PDTs);
}

void AccountInfo::Deserialize(vector<byte> Data)
{
	vector<ProtocolDataType> PDTs = ProtocolPackager::UnPackRaw(Data);
	int cnt = 0;

	while (cnt < (int)PDTs.size())
	{
		ProtocolDataType* PDT = &PDTs[cnt++];

		switch (PDT->NameType)
		{
		case 0:
			ProtocolPackager::UnpackByteVector(*PDT, 0, AccountID);
			break;

		case 1:
			ProtocolPackager::UnpackInt64(*PDT, 1, Money);
			break;

		case 2:
			ProtocolPackager::UnpackString(*PDT, 2, Name);
			break;

		case 3:
			ProtocolPackager::UnpackByte(*PDT, 3, IsBlocked);
			break;

		case 4:
			ProtocolPackager::UnpackInt64(*PDT, 4, LastTransactionTime);
			break;
		}
	}

}