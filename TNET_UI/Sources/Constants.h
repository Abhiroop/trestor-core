#ifndef CONSTANTS_H
#define CONSTANTS_H

#include <hash_map>
#include <vector>
#include "Utils.h"
#include "Node.h"

//#include "SQLiteCpp\CppSQLite3.h"
//extern CppSQLite3DB global_db;

class Constants
{
public :

	static const int SIM_REFRESH_MS = 1000;



};

static const char* dbFile = "test.db";

static const char* Const_LedgerFileName = "Ledger.txt";

extern hash_map<Hash, Node> GlobalNodes;


#endif