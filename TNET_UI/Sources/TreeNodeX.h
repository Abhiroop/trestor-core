
#ifndef TreeNode_H
#define TreeNode_H

/// <summary>
/// This is a node in the tree.
/// There can be 16 child nodes
/// </summary>
/// <typeparam name="T"></typeparam>

#include "Utils.h"

//template<class LeafDataType>
class TreeNodeX
{

public: 
	
	int64_t LeafCount;

	unsigned char ID[32];
	bool IsLeaf;

	bool IsRoot;

	TreeNodeX **Children = nullptr;

	TreeNodeX();
};


#endif

