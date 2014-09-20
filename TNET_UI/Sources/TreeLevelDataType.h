#ifndef TreeLevelDataType_H
#define TreeLevelDataType_H
#include "TreeNodeX.h"

class TreeLevelDataType
{
public:
	TreeLevelDataType(TreeNodeX* treeNodeX, vector<char> address);
	~TreeLevelDataType();

	TreeNodeX* treeNodeX;
	vector<char> address;
};

#endif