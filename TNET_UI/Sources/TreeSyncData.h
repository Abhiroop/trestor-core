
#ifndef Tree_Sync_Data_H
#define Tree_Sync_Data_H

#include "Hash.h"
#include <stdint.h>
#include "SerializableBase.h"

class TreeSyncData : SerializableBase
{
public:
	TreeSyncData(Hash _ID, vector<char> _Address, int64_t _LeafCount, bool _GetAll);
	TreeSyncData(TreeSyncData _LSD, bool _GetAll);
	TreeSyncData();

	Hash ID;
	vector<char> Address;
	int64_t LeafCount;
	bool GetAll;

	vector<byte> Serialize() override;
	void Deserialize(vector<byte> Data) override;
};

#endif 