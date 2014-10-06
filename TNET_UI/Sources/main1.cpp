
// ARPAN MAIN

#include <vector>
#include <iostream>
#include <sstream>
#include <hash_map>

#include "AccountInfo.h"
#include "ProtocolPackager.h"
#include "HashTree.h"
#include "Constants.h"
#include "ed25519\sha512.h"
#include "LedgerFileHandler.h"
#include "LedgerRootInfo.h"

#pragma comment(lib, "advapi32.lib")

using namespace std;

int main()
{

	cout << "TEST 3.0" << endl;

	HashTree< AccountInfo, LedgerRootInfo > ht,ht1;
	
	//lfh.loadledger();
	
	HCRYPTPROV hProvider = 0;

	if (!::CryptAcquireContextW(&hProvider, 0, 0, PROV_RSA_FULL, CRYPT_VERIFYCONTEXT | CRYPT_SILENT))
		return 1;

	byte PKS[32] = { 128, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF,
		0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF };

	Hash h_PKS = Hash(PKS, PKS + 32);

	uint64_t Taka = 0;

	//for (int k = 0; k < 4; k++)
	//for (int j = 0; j < 256; j++)
	for (int i = 0; i < 1000000; i++)
	{

		/*byte PK[32] = { i, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF,
			0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF };*/
		
		byte PK[32];

		if (!::CryptGenRandom(hProvider, 32, PK))
		{
			::CryptReleaseContext(hProvider, 0);
			return 1;
		}

		//long TAKA = 1000;

		Hash h = Hash(PK, PK + 32);

		h_PKS = h;

		string name = "a";
		int64_t lastdate = 0;
		AccountInfo si = AccountInfo(h, Taka++, name, 1, lastdate);

		//Hash HH = si.GetHash();

		///for (int i = 0; i < 32; i++)
		//{
		//	printf("%02x, ", HH[i]);
		//}

		//Hash h3 = ht.GetRootHash();

		ht.AddUpdate(si);
		
	}
	
	vector<TreeLevelDataType> vc = ht.TraverseLevelOrderDepth(20);

	cout << "Vector: " << vc.size();

	/*
	for (int i = 0; i < 20; i++)
	{
		byte PK[32];

		if (!::CryptGenRandom(hProvider, 32, PK))
		{
			::CryptReleaseContext(hProvider, 0);
			return 1;
		}

		Hash h = Hash(PK, PK + 32);

		h_PKS = h;

		string name = "a";
		int64_t lastdate = 0;
		AccountInfo si = AccountInfo(h, Taka++, name, 1, lastdate);


		ht1.AddUpdate(si);

	}*/



	/*Open the ledger db file to instantiate
	*/

	/*
	ledger_db.open("LedgerT.db");
	

	LedgerFileHandler lh(ht);
	lh.MakeVerifyLedgerTree();

	lh.SaveToDB();

	//cout << "FCALL:" << LedgerFileHandler::fCall;

	vector<TreeLevelDataType> vc = ht.TraverseLevelOrderDepth(55);

	vector<ProtocolDataType> PDTs;


	byte search[] = { 0xA, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF,
		0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF };
	
	Hash h(search,search + 32);

	stack<TreeNodeX*> treeNodeStack, bp;
	ht.getStack_Itr(h, treeNodeStack);

	ht.DeleteData(h);

	ht.getStack_Itr(h, bp);

	for (int i = 0; i < (int)vc.size(); i++)
	{			
		PDTs.push_back(*ProtocolPackager::Pack(vc[i].address, 0));
		//PDTs.push_back(*ProtocolPackager::Pack(Vote, 1));
		vector<byte> dat =  ProtocolPackager::PackRaw(PDTs);

		cout << i << "Address : ";

		string s(vc[i].address.begin(), vc[i].address.end());

		cout << s << endl;

		cout << i << "HASH   ";

		for (int j = 0; j < 32; j++)
		{
			char aL = vc[i].treeNodeX->ID[j] & 0xF;
			char aH = (vc[i].treeNodeX->ID[j] >> 4) & 0xF;

			cout << Constants::hexChars[aH] << Constants::hexChars[aL] << "";
		}

		cout << endl << endl;
	}
	cout << vc.size() << endl;

	*/


	/*vector<TreeLevelDataType> hamba = ht.GetDifference(ht.TraverseLevelOrderDepth(2), ht1.TraverseLevelOrderDepth(2));


	for (int i = 0;i<hamba.size() ;i++)
	cout << "Hamba : " << i << ": " << string(hamba[i].address.begin(), hamba[i].address.end()) << endl;*/

	//
	//	LedgerFileHandler lfh = LedgerFileHandler(ht);
	//
	//	lfh.storeLedger();
	//
	//	//si.Money = 100000;
	//
	//	//ht.AddUpdate(si);
	//
	//	AccountInfo hh;
	//	ht.GetNodeData(h_PKS, hh);
	//
	//	printf("\n\n TOTAL MONEY ITERS: %d ", Taka);
	//
	//	printf("\n\n MONEY: %d ", hh.Money);
	//
	//	ht.TraverseNodes();
	//
	//	printf("\n\n TOTAL NODES: %lu ", ht.TotalNodes());
	//	printf("\n\n TOTAL MONEY: %llu ", ht.TotalMoney);
	//
	//
	//
	getchar();


	return 0;
}