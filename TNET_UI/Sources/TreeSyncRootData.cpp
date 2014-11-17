
#include "TreeSyncData.h"
#include "TreeSyncRootData.h"
#include "ProtocolPackager.h"

/*TreeSyncRootData::TreeSyncRootData(vector<byte> _Tsd, Hash _RootHash)
{
	TSDs = _Tsd;
	RootHash = _RootHash;
}*/

TreeSyncRootData::TreeSyncRootData()
{

}

vector<byte> TreeSyncRootData::Serialize()
{
	vector<ProtocolDataType> PDTs;
	PDTs.push_back(*ProtocolPackager::Pack(TSDs, 0));
	PDTs.push_back(*ProtocolPackager::Pack(RootHash, 1));
	PDTs.push_back(*ProtocolPackager::Pack(LCL_Time, 2));
	PDTs.push_back(*ProtocolPackager::Pack(NodeCount, 3));
	return ProtocolPackager::PackRaw(PDTs);
}

void TreeSyncRootData::Deserialize(vector<byte> Data)
{
	vector<ProtocolDataType> PDTs = ProtocolPackager::UnPackRaw(Data);
	int cnt = 0;

	while (cnt < (int)PDTs.size())
	{
		ProtocolDataType* PDT = &PDTs[cnt++];

		switch (PDT->NameType)
		{
		case 0:
			ProtocolPackager::UnpackByteVector(*PDT, 0, TSDs);
			break;

		case 1:
			ProtocolPackager::UnpackByteVector(*PDT, 1, RootHash);
			break;

		case 2:
			ProtocolPackager::UnpackInt64(*PDT, 2, LCL_Time);
			break;

		case 3:
			ProtocolPackager::UnpackInt64(*PDT, 3, NodeCount);
			break;

		}
	}

}