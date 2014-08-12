
/*
*
*  @Author: Arpan Jati
*  @Version: 1.0
*/

#include "Utils.h"
#include "TreeNodeX.h"

/// <summary>
/// This is a node in the tree.
/// There can be 16 child nodes
/// </summary>
/// <typeparam name="T"></typeparam>

TreeNodeX::TreeNodeX()
{
	//ID = NULL;

	Children = new TreeNodeX*[16];

	for (int i = 0; i < 16; i++)
		Children[i] = NULL;

	IsLeaf = false;
}


