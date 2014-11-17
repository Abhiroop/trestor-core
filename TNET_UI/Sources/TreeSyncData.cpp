#include "TreeSyncData.h"

#include "ProtocolPackager.h"

TreeSyncData::TreeSyncData(Hash _ID, vector<char> _Address, int64_t _LeafCount, bool _GetAll)
{
	ID = _ID;
	Address = _Address; 
	LeafCount = _LeafCount;
	GetAll = _GetAll;
}


TreeSyncData::TreeSyncData()
{
	ID = Hash();
}

TreeSyncData::TreeSyncData(TreeSyncData _LSD, bool _GetAll)
{
	*this = _LSD;
	GetAll = _GetAll;
	//TreeSyncData(_LSD.ID, _LSD.Address, _LSD.LeafCount, _GetAll);
}

vector<byte> TreeSyncData::Serialize()
{
	vector<ProtocolDataType> PDTs;
	PDTs.push_back(*ProtocolPackager::Pack(ID, 0));
	PDTs.push_back(*ProtocolPackager::Pack(Address, 1));
	PDTs.push_back(*ProtocolPackager::Pack(LeafCount, 2));
	PDTs.push_back(*ProtocolPackager::Pack(GetAll, 3));
	return ProtocolPackager::PackRaw(PDTs);
}

void TreeSyncData::Deserialize(vector<byte> Data)
{
	vector<ProtocolDataType> PDTs = ProtocolPackager::UnPackRaw(Data);
	int cnt = 0;

	while (cnt < (int)PDTs.size())
	{
		ProtocolDataType* PDT = &PDTs[cnt++];

		switch (PDT->NameType)
		{
		case 0:
			ProtocolPackager::UnpackByteVector(*PDT, 0, ID);
			break;

		case 1:
			ProtocolPackager::UnpackByteVector_char(*PDT, 1, Address);
			break;

		case 2:
			ProtocolPackager::UnpackInt64 (*PDT, 2, LeafCount);
			break;

		case 3:
			ProtocolPackager::UnpackBool(*PDT, 3, GetAll);
			break;
		}
	}

}