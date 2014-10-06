#include "TreeSyncData.h"

TreeSyncData::TreeSyncData(Hash _ID, vector<char> _Address, int64_t _LeafCount, bool _GetAll)
{
	ID = _ID;
	Address = _Address; 
	LeafCount = _LeafCount;
	GetAll = _GetAll;
}

TreeSyncData::TreeSyncData(TreeSyncData _LSD, bool _GetAll)
{
	*this = _LSD;

	GetAll = _GetAll;
	//TreeSyncData(_LSD.ID, _LSD.Address, _LSD.LeafCount, _GetAll);
}