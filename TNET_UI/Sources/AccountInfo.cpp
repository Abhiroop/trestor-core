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

vector<byte> LongToVector(uint64_t data)
{
	vector<byte> out = vector<byte>(8);

	for (int i = 0; i < 8; i++)
	{
		out[i] = ( ((data >> (8*i)) & 0xFF) );		
	}

	return out;
}

Hash AccountInfo::GetHash()
{
	vector<byte> data;
	
	data.insert(data.end(), AccountID.begin(), AccountID.end());

	vector<byte> money = LongToVector(Money);

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

