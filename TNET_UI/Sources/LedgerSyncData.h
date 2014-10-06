
#ifndef Ledger_Sync_Data_H
#define Ledger_Sync_Data_H

#include "Hash.h"
#include <stdint.h>

class LedgerSyncData
{
public:
	LedgerSyncData(Hash _ID, vector<char> _Address, int64_t _LeafCount, bool _GetAll);
	LedgerSyncData(LedgerSyncData _LSD, bool _GetAll);

	Hash ID;
	vector<char> Address;
	int64_t LeafCount;
	bool GetAll;
};

#endif 