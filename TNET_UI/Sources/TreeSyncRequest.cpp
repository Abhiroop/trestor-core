
// @Author : Arpan Jati
// @Date : 23 Nov 2014

#include "TreeSyncRequest.h"
#include "ProtocolPackager.h"

TreeSyncRequest::TreeSyncRequest()
{

}

TreeSyncRequest::TreeSyncRequest(vector<TreeSyncData> _internalNodes, vector<AccountInfo> _leafNodes)
{
	internalNodes = _internalNodes;
	leafNodes = _leafNodes;
}

vector<byte> TreeSyncRequest::Serialize()
{
	vector<ProtocolDataType> PDTs;

	for (int i = 0; i < (int)internalNodes.size(); i++)
	{
		PDTs.push_back(*ProtocolPackager::Pack(internalNodes[i].Serialize(), 0));
	}

	for (int i = 0; i < (int)leafNodes.size(); i++)
	{
		PDTs.push_back(*ProtocolPackager::Pack(leafNodes[i].Serialize(), 1));
	}

	return ProtocolPackager::PackRaw(PDTs);
}

void TreeSyncRequest::Deserialize(vector<byte> Data)
{
	internalNodes.clear();
	leafNodes.clear();

	vector<ProtocolDataType> PDTs = ProtocolPackager::UnPackRaw(Data);
	int cnt = 0;

	while (cnt < (int)PDTs.size())
	{
		ProtocolDataType* PDT = &PDTs[cnt++];

		switch (PDT->NameType)
		{
		case 0:
		{
			vector<byte> _Data;
			ProtocolPackager::UnpackByteVector(*PDT, 0, _Data);
			TreeSyncData tsd;
			tsd.Deserialize(_Data);
			internalNodes.push_back(tsd);
		}
			break;

		case 1:
		{
			vector<byte> _Data;
			ProtocolPackager::UnpackByteVector(*PDT, 1, _Data);
			AccountInfo ai;
			ai.Deserialize(_Data);
			leafNodes.push_back(ai);
		}
			break;

		}
	}

}