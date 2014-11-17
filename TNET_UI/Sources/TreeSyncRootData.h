
#ifndef Tree_Sync_Root_Data_H
#define Tree_Sync_Root_Data_H

#include "Hash.h"
#include <stdint.h>
#include "SerializableBase.h"
#include "TreeSyncData.h"

class TreeSyncRootData : SerializableBase
{
public:
	
	//TreeSyncRootData(vector<TreeSyncData> _Tsd, Hash _RootHash);
	TreeSyncRootData();

	vector<byte> TSDs;
	Hash RootHash;
	int64_t LCL_Time;
	int64_t NodeCount;
	
	vector<byte> Serialize() override;
	void Deserialize(vector<byte> Data) override;
};

#endif 