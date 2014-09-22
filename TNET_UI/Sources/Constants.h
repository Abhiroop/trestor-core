#ifndef CONSTANTS_H
#define CONSTANTS_H

#include <hash_map>
#include <vector>
#include "Utils.h"
#include "Node.h"
#include "TransactionSetType.h"
#include "AccountInfo.h"

#include "tbb/concurrent_queue.h"
#include "tbb/concurrent_hash_map.h"
#include "SQLiteCpp/CppSQLite3.h"
//extern CppSQLite3DB global_db;

extern tbb::concurrent_queue<string> MessageQueue;

extern concurrent_hash_map<Hash, AccountInfo> GLOBAL_LEDGER_MAP;

extern CppSQLite3DB ledger_db;

class Constants
{
public :

	
	static const uint64_t FIN_MIN_BALANCE = 1500;
	static const int VALIDATOR_COUNT = 5;
	static const int SIM_REFRESH_MS = 50;
	static const int SIM_REFRESH_MS_SIM = 50;

	static const int KEYLEN_PUBLIC = 32;
	static const int KEYLEN_PRIVATE = 32;
	static const int KEYLEN_SIGNATURE = 64;
		
	static string hexChars;

};

static const char* dbFile = "test.db";

static const char* Const_LedgerFileName = "Ledger.txt";

extern concurrent_hash_map<Hash, shared_ptr<Node>> GlobalNodes;


#endif