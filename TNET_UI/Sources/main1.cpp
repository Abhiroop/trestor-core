//
//
//// ARPAN MAIN
//
//#include <vector>
//#include <iostream>
//#include <sstream>
//#include <hash_map>
//
//#include "AccountInfo.h"
//
//#include "HashTree.h"
//
//#include "ed25519\sha512.h"
//#include "LedgerFileHandler.h"
//
//
//int main2()
//{
//
//	cout << "TEST 3.0" << endl;
//
//	HashTree< AccountInfo > ht;
//
//
//
//	//lfh.loadledger();
//
//	byte PKS[32] = { 128, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF,
//		0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF };
//
//	Hash h_PKS = Hash(PKS, PKS + 32);
//
//	uint64_t Taka = 0;
//
//	//for (int k = 0; k < 4; k++)
//	//for (int j = 0; j < 256; j++)
//	for (int i = 0; i < 10; i++)
//	{
//
//		byte PK[32] = { i, 2, 3, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF,
//			0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF };
//
//		//long TAKA = 1000;
//
//		Hash h = Hash(PK, PK + 32);
//
//		h_PKS = h;
//
//		string name = "a";
//		int64_t lastdate = 0;
//		AccountInfo si = AccountInfo(h, Taka++, name, lastdate);
//
//		//Hash HH = si.GetHash();
//
//		///for (int i = 0; i < 32; i++)
//		//{
//		//	printf("%02x, ", HH[i]);
//		//}
//
//		//Hash h3 = ht.GetRootHash();
//
//		ht.AddUpdate(si);
//
//	}
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
//	getchar();
//
//
//	return 0;
//}