/*
*
*  @Author: Arpan Jati
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

using namespace std;

AccountInfo::AccountInfo()
{

}

AccountInfo::AccountInfo(Hash _AccountID, int64_t _Money, string _Name, int64_t _LastTransactionTime)
{
	AccountID = _AccountID;
	Money = _Money;
	Name = _Name;
	LastTransactionTime = _LastTransactionTime;
}


Hash AccountInfo::GetHash()
{
	vector<byte> data;
	
	data.insert(data.end(), AccountID.begin(), AccountID.end());

	vector<byte> money = Conversions::Int64ToVector(Money);

	data.insert(data.end(), money.begin(), money.end());

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

