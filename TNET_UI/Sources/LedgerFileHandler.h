
#ifndef LedgerFileHandler_H
#define LedgerFileHandler_H
//@Author : Aritra Dhar + Arpan Jati

#include <fstream>
#include <iostream>
#include "AccountInfo.h"
#include "LeafDataType.h"
#include "HashTree.h"

class LedgerFileHandler
{
public:

	int64_t LCLtime;
	int64_t LCLsequence;
	Hash LCLhash;
	HashTree< AccountInfo > AccountTree;

	//static int fCall;

	int treeToDB(Hash accountID, int64_t money, string name, int64_t lastTransactionTime);
	
	HashTree< AccountInfo > DBToTree();

	LedgerFileHandler(HashTree< AccountInfo > accountTree);

	int SaveToDB_Callback(AccountInfo leaf);

	int SaveToDB();

	int MakeVerifyLedgerTree();
};

#endif