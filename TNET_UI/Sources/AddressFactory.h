
/*
*
*  @Author: Arpan Jati
*  @Version: 1.0
*  @Date: 31 August 2014
*/

#ifndef ADDRESS_FACTORY_H
#define ADDRESS_FACTORY_H

#include "Utils.h"
#include "ed25519\sha512.h"

class AddressFactory
{
	/// <summary>
	/// H = SHA512, 
	/// Address Format : A_prefix = NetType || AccountType || {[H(H(PK) || PK || NAME)], Take first 20 bytes}
	///                  Check = H (A_prefix), Take first 4 bytes, 
	///                  Address = [A_Prefix || Check]
	/// </summary>
public: 
	
	vector<byte> GetAddress(vector<byte> PublicKey, string UserName, byte NetworkType = 1, byte AccountType = 1);

	bool ValidateAddress(vector<byte> Address);

};

#endif