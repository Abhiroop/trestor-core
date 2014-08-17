#ifndef CONSTANTS_H
#define CONSTANTS_H

#include <hash_map>
#include <vector>
#include "Utils.h"
#include "Node.h"

#include "tbb/concurrent_queue.h"

//#include "SQLiteCpp\CppSQLite3.h"
//extern CppSQLite3DB global_db;

extern tbb::concurrent_queue<string> MessageQueue;

class Constants
{
public :

	static const int VALIDATOR_COUNT = 5;
	static const int SIM_REFRESH_MS = 50;
	static const int SIM_REFRESH_MS_SIM = 50;

	static const int KEYLEN_PUBLIC = 32;
	static const int KEYLEN_PRIVATE = 32;
	static const int KEYLEN_SIGNATURE = 64;

	



};

static const char* dbFile = "test.db";

static const char* Const_LedgerFileName = "Ledger.txt";

extern hash_map<Hash, shared_ptr<Node>> GlobalNodes;


#endif