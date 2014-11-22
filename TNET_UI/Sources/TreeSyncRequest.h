
#ifndef TreeSyncRequest_H
#define TreeSyncRequest_H

#include "Hash.h"
#include <stdint.h>
#include "SerializableBase.h"
#include "TreeSyncData.h"

class TreeSyncRequest : SerializableBase
{
public:

	//TreeSyncRootData(vector<TreeSyncData> _Tsd, Hash _RootHash);
	TreeSyncRequest();

	vector<byte> TSRs;
	Hash RootHash;
	int64_t LCL_Time;
	int64_t NodeCount;

	vector<byte> Serialize() override;
	void Deserialize(vector<byte> Data) override;
};

#endif 