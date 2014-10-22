#ifndef CONSTANTS_H
#define CONSTANTS_H

#include <hash_map>
#include <string>
#include <vector>
#include "Utils.h"
#include "Node.h"
#include "TransactionSetType.h"
#include "AccountInfo.h"

#include "tbb/concurrent_queue.h"
#include "tbb/concurrent_hash_map.h"
#include "tbb/concurrent_vector.h"
//#include "SQLiteCpp/CppSQLite3.h"
//extern CppSQLite3DB global_db;

using namespace tbb;

extern tbb::concurrent_queue<string> MessageQueue;

extern concurrent_hash_map<Hash, AccountInfo> GLOBAL_LEDGER_MAP;



class Constants
{
public :

	static const uint64_t FIN_MIN_BALANCE = 1500;
	static const int VALIDATOR_COUNT = 5;
	static const int SIM_REFRESH_MS = 50;
	static const int SIM_REFRESH_MS_SIM = 50;

	static const int LEN_TRANSACTION_ID = 32;

	static const int KEYLEN_PUBLIC = 32;
	static const int KEYLEN_PRIVATE = 32;
	static const int KEYLEN_SIGNATURE = 64;

	static const int CONS_TRUSTED_VALIDATOR_THRESHOLD_PERC = 50;
	static const int CONS_VOTING_ACCEPTANCE_THRESHOLD_PERC = 75;

	static const int SYNC_LEAF_COUNT_THRESHOLD = 200;
		
	static string hexChars;

};

//static const char* dbFile = "test.db";

///static const char* dbInfoFile = "LedgerInfo.db";

//static const char* Const_LedgerFileName = "Ledger.txt";

/// FOR SIMULATION PURPOSES ONLY
extern concurrent_hash_map<Hash, shared_ptr<Node>> GlobalNodes;


#endif