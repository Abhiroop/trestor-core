
/*
*
*  @Author: Arpan Jati
*  @Version: 1.0
*  @Date: 31 August 2014
*/

#include "AddressFactory.h"


vector<byte> AddressFactory::GetAddress(vector<byte> PublicKey, string UserName, byte NetworkType, byte AccountType)
{
	vector<byte> NAME(UserName.data(), UserName.data() + UserName.length());

	vector<byte> Hpk = sha512(PublicKey.data(), PublicKey.size());

	//byte[] Hpk = (new SHA512Managed()).ComputeHash(PublicKey);

	vector<byte> Hpk__PK__NAME;
	Hpk__PK__NAME.insert(Hpk__PK__NAME.end(), Hpk.begin(), Hpk.end());
	Hpk__PK__NAME.insert(Hpk__PK__NAME.end(), PublicKey.begin(), PublicKey.end());
	Hpk__PK__NAME.insert(Hpk__PK__NAME.end(), NAME.begin(), NAME.end());

	//byte[] Hpk__PK__NAME = Hpk.Concat(PublicKey).Concat(NAME).ToArray();

	vector<byte> H_Hpk__PK__NAME = sha512(Hpk__PK__NAME.data(), Hpk__PK__NAME.size());

	//byte[] H_Hpk__PK__NAME = (new SHA512Managed()).ComputeHash(Hpk__PK__NAME).Take(20).ToArray();

	byte Address_PH[26];

	Address_PH[0] = NetworkType;
	Address_PH[1] = AccountType;

	memcpy(Address_PH + 2, H_Hpk__PK__NAME.data(), 20);

	//Array.Copy(H_Hpk__PK__NAME, 0, Address_PH, 2, 20);

	vector<byte> CheckSum = sha512(Address_PH, 20);

	//byte[] CheckSum = (new SHA512Managed()).ComputeHash(Address_PH, 0, 22).Take(4).ToArray();

	memcpy(Address_PH + 22, CheckSum.data(), 4);

	//Array.Copy(CheckSum, 0, Address_PH, 22, 4);

	return vector<byte>(Address_PH, Address_PH + 26);
}

bool AddressFactory::ValidateAddress(vector<byte> Address)
{
	if (Address.size() != 26)
		return false;

	//byte[] CheckSum = (new SHA512Managed()).ComputeHash(Address, 0, 22).Take(4).ToArray();

	vector<byte> CheckSum = sha512(Address.data(), 20);

	return ByteArrayEquals(CheckSum, 0, Address, 22, 4);
}