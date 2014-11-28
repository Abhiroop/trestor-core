
//@Author : Aritra Dhar + Arpan Jati

#ifndef LedgerFileHandler_H
#define LedgerFileHandler_H

#include <fstream>
#include <iostream>
#include "Hash.h"
#include "AccountInfo.h"
#include "LeafDataType.h"
#include "HashTree.h"
#include "LedgerRootInfo.h"
#include "SQLiteCpp\CppSQLite3.h"

class LedgerFileHandler
{
	CppSQLite3DB ledger_db;

public:

	int64_t LCLtime;
	int64_t LCLsequence;
	Hash LCLhash;
	HashTree< AccountInfo, LedgerRootInfo > AccountTree;
	
	LedgerFileHandler();
	LedgerFileHandler(HashTree< AccountInfo, LedgerRootInfo > accountTree, string LedgerDB_FileName);

	//static int fCall;

	int treeToDB(Hash accountID, int64_t money, string name, int64_t lastTransactionTime);
	
	HashTree< AccountInfo, LedgerRootInfo > DBToTree();
	
	int SaveToDB_Callback(AccountInfo leaf);

	int UpdateLedgerInfo(Hash LedgerHash, Hash LastLedgerHash, int64_t LCLTime, int64_t SequenceNumber);

	int SaveToDB();

	int VerifyDatabase();
};

#endif