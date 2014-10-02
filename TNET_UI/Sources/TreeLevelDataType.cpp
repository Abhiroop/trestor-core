#include "TreeLevelDataType.h"
#include "ProtocolPackager.h"


TreeLevelDataType::TreeLevelDataType(TreeNodeX* _treeNodeX, vector<char> _address)
{
	treeNodeX = _treeNodeX;
	address = _address;
}


TreeLevelDataType::~TreeLevelDataType()
{
}

vector<byte> TreeLevelDataType::Serialize()
{
	vector<ProtocolDataType> PDTs;
	PDTs.push_back(*ProtocolPackager::Pack(address, 0));
	vector<byte> Hash = vector<byte>(treeNodeX->ID, treeNodeX->ID + 32);
	PDTs.push_back(*ProtocolPackager::Pack(Hash, 1));
	PDTs.push_back(*ProtocolPackager::Pack(treeNodeX->LeafCount, 2));
	return ProtocolPackager::PackRaw(PDTs);
}

void TreeLevelDataType::Deserialize(vector<byte> Data)
{



}


vector<byte> TreeLevelDataType::SerializeMe(vector <TreeLevelDataType> TLDs)
{
	vector<ProtocolDataType> PDTs;
	for (int i = 0; i < (int)TLDs.size(); i++)
		PDTs.push_back(*ProtocolPackager::Pack(TLDs[i].Serialize(), 0));
	return ProtocolPackager::PackRaw(PDTs);
}
