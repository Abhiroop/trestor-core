
#ifndef LedgerFileHandler_H
#define LedgerFileHandler_H

#include <fstream>
#include <iostream>
#include "AccountInfo.h"

class LedgerFileHandler
{
public:
	
	HashTree< AccountInfo > ledgerTree;

	int64_t LCLtime;
	int64_t LCLsequence;
	Hash LCLhash;

	void storeLedger();
	void loadledger();

	LedgerFileHandler(HashTree< AccountInfo > accountTree);

};

#endif