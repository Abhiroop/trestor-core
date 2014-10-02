#ifndef TreeLevelDataType_H
#define TreeLevelDataType_H

#include "TreeNodeX.h"
#include "SerializableBase.h"

class TreeLevelDataType : public SerializableBase
{
public:
	TreeLevelDataType(TreeNodeX* treeNodeX, vector<char> address);
	~TreeLevelDataType();

	TreeNodeX* treeNodeX;
	vector<char> address;

	vector<byte> Serialize() override;
	void Deserialize(vector<byte> Data) override;

	vector<byte> SerializeMe(vector <TreeLevelDataType> TLDs);

};

#endif