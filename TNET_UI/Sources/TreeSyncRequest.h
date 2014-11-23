
// @Author : Arpan Jati
// @Date : 23 Nov 2014

#ifndef TreeSyncRequest_H
#define TreeSyncRequest_H

#include "Hash.h"
#include <stdint.h>
#include <vector>
#include "SerializableBase.h"
#include "TreeSyncData.h"
#include "AccountInfo.h"

using namespace std;

class TreeSyncRequest : SerializableBase
{
public:

	vector<TreeSyncData> internalNodes;
	vector<AccountInfo> leafNodes;

	//TreeSyncRootData(vector<TreeSyncData> _Tsd, Hash _RootHash);
	TreeSyncRequest();
	TreeSyncRequest(vector<TreeSyncData> _internalNodes, vector<AccountInfo> _leafNodes);
	
	vector<byte> Serialize() override;
	void Deserialize(vector<byte> Data) override;
};

#endif 