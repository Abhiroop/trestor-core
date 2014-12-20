
/*
*
*  @Author: Arpan Jati
*  @Version: 1.0
*  @Date: December 2014
*/

#ifndef LedgerFileHandler_LevelDB_H
#define LedgerFileHandler_LevelDB_H

#include <fstream>
#include <iostream>
#include "Hash.h"
#include "AccountInfo.h"
#include "LeafDataType.h"
#include "HashTree.h"
#include "LedgerRootInfo.h"
//#include "SQLiteCpp\CppSQLite3.h"
#include "leveldb/db.h"
#include "SimpleIni.h"

class LedgerFileHandler_LevelDB
{
	leveldb::DB* db;
	leveldb::Options options;
	
	string LedgerDB_FileName;

public:

	//FILE* rootFile;	

	int64_t LCLtime;
	int64_t LCLsequence;
	Hash LCLhash;
	HashTree< AccountInfo, LedgerRootInfo > AccountTree;

	LedgerFileHandler_LevelDB();
	LedgerFileHandler_LevelDB(HashTree< AccountInfo, LedgerRootInfo > accountTree, string LedgerDB_FileName);

	//static int fCall;

	int treeToDB(Hash accountID, int64_t money, string name, int64_t lastTransactionTime);

	HashTree< AccountInfo, LedgerRootInfo > DBToTree();

	int SaveToDB_Callback(AccountInfo leaf);

	int UpdateLedgerInfo(Hash LedgerHash, Hash LastLedgerHash, int64_t LCLTime, int64_t SequenceNumber);

	int SaveToDB();

	int VerifyDatabase();

	////////////

	LedgerRootInfo LoadLRIinfo();
	void LedgerFileHandler_LevelDB::SaveLRIinfo(LedgerRootInfo lri);
};

#endif