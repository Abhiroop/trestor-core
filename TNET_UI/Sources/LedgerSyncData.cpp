#include "LedgerSyncData.h"

LedgerSyncData::LedgerSyncData(Hash _ID, vector<char> _Address, int64_t _LeafCount, bool _GetAll)
{
	ID = _ID;
	Address = _Address; 
	LeafCount = _LeafCount;
	GetAll = _GetAll;
}

LedgerSyncData::LedgerSyncData(LedgerSyncData _LSD, bool _GetAll)
{
	LedgerSyncData(_LSD.ID, _LSD.Address, _LSD.LeafCount, true);
}