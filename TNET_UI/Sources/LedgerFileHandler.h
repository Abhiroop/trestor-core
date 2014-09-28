
#ifndef LedgerFileHandler_H
#define LedgerFileHandler_H

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
	
	HashTree< AccountInfo > DBToTree(Hash AccountID, int64_t Money, string Name, int64_t LastTransactionTime);

	LedgerFileHandler(HashTree< AccountInfo > accountTree);

	int SaveToDB_Callback(AccountInfo leaf);

	int SaveToDB();

	int MakeVerifyLedgerTree();
};

#endif