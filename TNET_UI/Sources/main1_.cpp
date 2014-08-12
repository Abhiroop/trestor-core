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
//int main__()
//{
//
//	cout << "TEST 3.0" << endl;
//
//	HashTree< AccountInfo > ht;
//	
//	//lfh.loadledger();
//
//	byte PKS[] = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
//		0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F };
//
//	Hash h_PKS = Hash(PKS, PKS + 32);
//
//	uint64_t Taka = 1234;
//
//	AccountInfo si = AccountInfo(h_PKS, Taka++, "", 0);
//
//	ht.AddUpdate(si);
//
//	//for (int k = 0; k < 4; k++)
//	//for (int j = 0; j < 256; j++)
//	/*for (int i = 0; i < 1; i++)
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
//	}*/
//
//	//LedgerFileHandler lfh = LedgerFileHandler(ht);
//
//	//lfh.storeLedger();
//
//	//si.Money = 100000;
//
//	//ht.AddUpdate(si);
//
//	//AccountInfo hh = ht.GetNodeData(h_PKS);
//
//	printf("\n\n TOTAL MONEY ITERS: %d ", Taka);
//
//	//printf("\n\n MONEY: %d ", hh.Money);
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