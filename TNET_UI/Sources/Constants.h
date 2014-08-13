#ifndef CONSTANTS_H
#define CONSTANTS_H

#include <hash_map>
#include <vector>
#include "Utils.h"
#include "Node.h"

//#include "SQLiteCpp\CppSQLite3.h"

//extern CppSQLite3DB global_db;

static const char* dbFile = "test.db";

static const char* Const_LedgerFileName = "Ledger.txt";


#ifdef __cplusplus_cli

extern hash_map<Hash, Node> GlobalNodes;

#endif 

#endif