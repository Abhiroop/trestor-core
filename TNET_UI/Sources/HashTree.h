
/*
*
*  @Author: Arpan Jati
*  @Author: Aritra Dhar
*  @Version: 1.0
*  @Description: HashTree
*      Features: Depth 64
*      Child Per Node : 16
*      Traverses using the bytes of the ID
*      Insertion is Non-Recursive
*      Traversal is currently recursive.
*
* TEMPLETE CLASS
*
*/

#ifndef HASH_TREE_H
#define HASH_TREE_H

#include <inttypes.h>
#include <stack>

#include "TreeNodeX.h"

#include <functional>

#include "ed25519\sha512.h"

#include "Utils.h"
#include "TreeLevelDataType.h"
#include "TreeSyncData.h"
//#include "LedgerFileHandler.h"

#include "LeafDataType.h"

using namespace std;

template<typename T>
class TreeLeafNode : public TreeNodeX
{
public:
	T Value;

	TreeLeafNode(Hash ID, T Value);
	TreeLeafNode();
};

template<typename T>
TreeLeafNode<T>::TreeLeafNode(Hash _ID, T _Value)
{
	unsigned char* f = ID;

	f = _ID.data();
	Value = _Value;
	IsLeaf = true;
}


template<typename R>
class TreeRootNode : public TreeNodeX
{
public:
	R Value;

	TreeRootNode(Hash ID, R Value);
	TreeRootNode();
};

template<typename R>
TreeRootNode<R>::TreeRootNode(Hash _ID, R _Value)
{
	Value = _Value;

	IsRoot = true;
	IsLeaf = false;
}

/// <summary>
/// This represents a Merkle Hash tree, each node has 16 child nodes.
/// The depth is defined by the length of the Hash in Nibbles.
/// Insertion / Updation is non-recursive and constant time.
/// </summary>


template<typename T, typename R>
class HashTree
{
private:
	TreeRootNode<R>* Root;

public:

	int64_t _TotalNodes = 0;
	int64_t _TotalLeaves = 0;
	int64_t TotalMoney = 0;

	int THRESHOLD_DEPTH = 10;
	//template<class LeafDataType>
	int64_t TotalNodes();
	int64_t TotalLeaves();

	HashTree();

	HashTree(R Value);

	Hash GetRootHash();

	/// <summary>
	/// Adds an element to the tree. If the element exists, update it.
	/// </summary>
	/// <param name="Value">The Value to be added</param>
	/// <returns></returns>
	//bool AddUpdate(LeafDataType Value);

	R GetRootInfo();

	bool AddUpdate(T);

	int64_t TraverseNodes();

	int64_t TraverseNodesAndSave(fstream& Ledger);

	vector<shared_ptr<LeafDataType>> TraverseNodesAndReturnLeaf();

	void TraverseLevelOrder(TreeNodeX* Root, int64_t & FoundNodes);

	vector<TreeLevelDataType> TraverseLevelOrderDepth(int depth);

	vector<TreeSyncData> TraverseLevelOrderDepthSync(int depth);

	void TraverseTree(TreeNodeX* Root, int64_t & FoundNodes, int _depth);

	void TraverseTreeAndFetch(TreeNodeX* Root, int64_t & FoundNodes, int _depth, function<int(T)> fun);
	void TraverseTreeAndFetch_do(function<int(T)> fun);

	void TraverseTreeAndReturn(vector<shared_ptr<LeafDataType>> & tempLeaves, TreeNodeX* Root, int64_t & FoundNodes, int _depth);

	void GetDifference(TreeNodeX* Root_1, TreeNodeX* Root_2);

	void GetDifference(TreeNodeX* Root_1, TreeNodeX* Root_2, int depth);

	void getAllLeafUnderNode(TreeNodeX* Root);

	void getAllLeafUnderNode(TreeNodeX* Root, vector<T>& Leaves);

	bool getImmediateChildren(string ID, vector<TreeNodeX*> &vec);

	vector<TreeSyncData> GetDifference(vector<TreeSyncData> other, vector<TreeSyncData> me);

	vector<TreeSyncData> GetDifference(vector<TreeSyncData> other);

	//returns two vector of TreeSyncData and AccountInfo
	void GetSyncNodeInfo(vector<TreeSyncData> input, vector<TreeSyncData>& internalNodes, vector<T>& leadNodes);

	vector<TreeNodeX> NodeDifferenceVec;

	T FindData(TreeNodeX* root, Hash ValueID, int pos);

	bool getStack(TreeNodeX* root, Hash ValueID, int pos, stack<TreeNodeX*> treeNodeStack);

	bool getStack_Itr(Hash ID, stack<TreeNodeX*> &treeNodeStack);

	bool DeleteData(Hash ID);


	/// <summary>
	/// Gets the element at the position specified by the ID.
	/// </summary>
	/// <param name="ID"></param>
	/// <returns></returns>
	//T GetNodeData(Hash ID);

	bool GetNodeData(Hash ID, T & Response);

};


using namespace std;

template<typename T, typename R>
int64_t HashTree<T, R>::TotalNodes()
{
	return _TotalNodes;
}
template<typename T, typename R>
R HashTree<T, R>::GetRootInfo()
{
	return Root->Value;
}

/// <summary>
/// Gets the total leaves in the tree, this is incremented on add operation.
/// TODO: THINK OF A BETTER WAY
/// </summary>
/// <returns></returns>
template<typename T, typename R>
int64_t HashTree<T, R>::TotalLeaves()
{
	return _TotalLeaves;
}


template<typename T, typename R>
HashTree<T, R>::HashTree()
{
	//Root = new TreeNodeX();
	Hash ID;
	R Value;
	Root = new TreeRootNode<R>(ID, Value);
}


template<typename T, typename R>
HashTree<T, R>::HashTree(R Value)
{
	//Root = new TreeNodeX();
	Hash ID = (Value).getID();
	Root = new TreeRootNode<R>(ID, Value);
}

/// <summary>
/// Get the RootHash of the Merkle Tree
/// </summary>
/// <returns></returns>
template<typename T, typename R>
Hash HashTree<T, R>::GetRootHash()
{
	if (Root != nullptr)
	{
		if (Root->ID != nullptr)
		{
			return Hash(Root->ID, Root->ID + 32);
		}
		else
		{
			throw exception("Root Hash is NULL");
		}
	}
	else
	{
		throw exception("Root is NULL");
	}
}


inline byte GetNibble(Hash val, int NibbleIndex)
{
	int Address = NibbleIndex >> 1;
	int IsLow = NibbleIndex & 1;
	return (byte)((IsLow == 0) ? ((val[Address] >> 4) & 0xF) : (val[Address] & 0xF));
}

//inline void printHash(Hash hsh)
//{
//	for (uint32_t i = 0; i < hsh.size(); i++)
//	{
//		//printf("%02X", hsh[i]);
//	}
//}

/// <summary>
/// Adds an element to the tree. If the element exists, update it.
/// </summary>
/// <param name="Value">The Value to be added</param>
/// <returns></returns>

template<typename T, typename R>
bool HashTree<T, R>::AddUpdate(T Value)
{
	bool Good = false;

	Hash ID = (Value).GetID();

	TreeNodeX* TempRoot = Root;

	stack<TreeNodeX*> PathStack;

	PathStack.push(TempRoot);

	int HLEN = ID.size() << 1; // Multiply by 2

	for (int i = 0; i < HLEN; i++)
	{
		byte Nibble = GetNibble(ID, i);

		// Insert new node
		if (((TempRoot->Children[Nibble]) == nullptr) && (i < (HLEN - 1)))
		{
			TempRoot->Children[Nibble] = new TreeNodeX();
			_TotalNodes++;
		}

		// Make child as curent
		if ((i < (HLEN - 1)))
		{
			TempRoot = TempRoot->Children[Nibble];
			PathStack.push(TempRoot);
		}

		// Insert leaf
		if (i == (HLEN - 1))
		{
			if (TempRoot->Children[Nibble] == nullptr)
			{
				TempRoot->Children[Nibble] = new TreeLeafNode<T>(ID, Value);
				_TotalNodes++;
				_TotalLeaves++;

				//printf("\nLeaf Node Added: %08x", (TempRoot->Children[Nibble]));
				// DisplayUtils.Display("Node Added: " + HexUtil.ToString(ID.Hex));
				Good = true;
			}
			else// if (TempRoot->Children[Nibble].GetType() == typeof(TreeLeafNode<LeafDataType>))
			{
				//if (*(TempRoot->Children[Nibble]->ID) == ID)
				{
					((TreeLeafNode<T>*)TempRoot->Children[Nibble])->Value = Value;
					Good = true;
				}
				//else
				//{
				//	throw exception("Node ID Mismatch, Bad Mess !!!");
				//}
			}
			/*else
			{
			throw exception("Node Already Exists");
			}*/
		}
	}

	///  Traceback.
	bool LeafDone = false;

	while (PathStack.size() > 0)
	{
		TreeNodeX* val = PathStack.top();
		PathStack.pop();
		//Hash NodeHash;

		if (!LeafDone)
		{
			vector<byte> hashDataArray;// = new vector<byte>();
			int nLeaves = 0;
			for (int i = 0; i < 16; i++)
			{
				if (val->Children[i] != nullptr)
				{
					//printf("\n CH : \n-  %01x - ", i);
					TreeLeafNode<T>* ai = (TreeLeafNode<T>*)val->Children[i];
					Hash hsh = ai->Value.GetHash();
					hashDataArray.insert(hashDataArray.end(), hsh.begin(), hsh.end());
					nLeaves++;
					//printHash(hsh);
				}
			}

			byte Hout[64];
			sha512(hashDataArray.data(), hashDataArray.size(), Hout);
			memcpy((unsigned char*)(val->ID), Hout, 32);
			val->LeafCount = nLeaves;
			LeafDone = true;
		}
		else
		{
			vector<byte> hashDataArray;
			int64_t nLeaves = 0;
			for (int i = 0; i < 16; i++)
			{
				if (val->Children[i] != nullptr)
				{
					//printf("\n-  %01x - ", i);
					TreeNodeX* ai = (TreeNodeX*)val->Children[i];
					unsigned char * IDS = ai->ID;
					vector<unsigned char> hsh(IDS, IDS + 32);
					hashDataArray.insert(hashDataArray.end(), hsh.begin(), hsh.end());
					nLeaves += ai->LeafCount;
					//printHash(hsh);
				}
			}

			byte Hout[64];
			sha512(hashDataArray.data(), hashDataArray.size(), Hout);
			memcpy((unsigned char*)(val->ID), Hout, 32);
			val->LeafCount = nLeaves;

		}
	}

	return Good;
}


template<typename T, typename R>
T HashTree<T, R>::FindData(TreeNodeX* root, Hash ValueID, int pos)
{
	byte Nibble = GetNibble(ID, pos);

	if (root->Children[Nibble] == null)
	{
		return nullptr;
	}

	else if (root->IsLeaf)
	{
		TreeLeafNode<T>* ai = (TreeLeafNode<T>*)root;
		return ai->Value;
	}
	else
	{
		int _pos = pos + 1;
		return FindData(root->Children[Nibble], ValueID, _pos);
	}
}

template<typename T, typename R>
bool HashTree<T, R>::getStack(TreeNodeX* root, Hash ValueID, int pos, stack<TreeNodeX*> Stack)
{
	byte Nibble = GetNibble(ID, pos);

	//in case there is not leaf return an empty stack
	if (root->Children[Nibble] == null)
	{
		return false;
	}

	else if (root->IsLeaf)
	{
		TreeLeafNode<T>* ai = (TreeLeafNode<T>*)root;
		Stack.push(ai);
		return true;
	}
	else
	{
		int _pos = pos + 1;
		Stack.push(root->Children[Nibble]);

		if (Stack.size > 64)
			return false;

		return FindData(root->Children[Nibble], ValueID, _pos, Stack);
	}
}

template<typename T, typename R>
bool HashTree<T, R>::getStack_Itr(Hash ID, stack<TreeNodeX*> &treeNodeStack)
{
	TreeNodeX* TempRoot = Root;
	int len = ID.size() * 2;

	for (int i = 0; i < len; i++)
	{
		byte Nibble = GetNibble(ID, i);

		TempRoot = TempRoot->Children[Nibble];

		//boundary conditions
		if (TempRoot == nullptr)
		{
			cout << "not found null  " << ID.ToString();
			return false;
		}

		//printf("\nPushing to Stack : %08x  : i = %d : Nibble = %d", (TempRoot), i, Nibble);
		treeNodeStack.push(TempRoot);

		if (TempRoot->IsLeaf)
		{
			cout << "found  " << ID.ToString();
			return true;
		}
	}

	cout << "not found  " << ID.ToString();
	return false;
}

/*
Delete a value from the hash tree
*/
template<typename T, typename R>
bool HashTree<T, R>::DeleteData(Hash ID)
{
	bool doDelete = true;
	int HLEN = ID.size() << 1;
	TreeNodeX* TempRoot = Root;

	stack<TreeNodeX*> treeNodeStack;
	bool b = getStack_Itr(ID, treeNodeStack);

	if (!b)
	{
		cout << "Leaf does not exists in the tree";
		return false;
	}

	else
	{
		if (treeNodeStack.size() < 64)
		{
			cout << "Problem level : Bitch please";
			return false;
		}

		//cout << endl << "Stack size  " << treeNodeStack.size();
		int counter = 0;
		while (treeNodeStack.size() > 0)
		{
			counter++;
			TreeNodeX* retNode = treeNodeStack.top();
			treeNodeStack.pop();

			if (retNode->IsLeaf)
			{
				//printf("\nDELETED Leaf Node: %08x", (retNode));

				delete retNode;
				retNode = nullptr;

				//cout << "\n###" << counter<< endl;
			}

			else
			{
				bool haveBabies = false;
				vector<byte> hashDataArray;

				int64_t nLeaves = 0;

				for (int i = 0; i < 16; i++)
				{

					if (retNode->Children[i] != nullptr)
					{
						nLeaves++;
						haveBabies = true;

						if (treeNodeStack.size() == 63)
						{
							TreeLeafNode<T>* ai = (TreeLeafNode<T>*)retNode->Children[i];
							Hash hsh = ai->Value.GetHash();
							hashDataArray.insert(hashDataArray.end(), hsh.begin(), hsh.end());
						}
						else
						{
							TreeNodeX* ai = (TreeNodeX*)retNode->Children[i];
							unsigned char * IDS = ai->ID;
							vector<unsigned char> hsh(IDS, IDS + 32);
							hashDataArray.insert(hashDataArray.end(), hsh.begin(), hsh.end());
						}
					}
				}


				if (!haveBabies)
				{
					//printf("\nDELETED Node: %08x", (retNode));
					delete retNode;
					retNode = nullptr;
					//cout << "\n##"<< counter;
				}
				else
				{
					byte Hout[64];
					sha512(hashDataArray.data(), hashDataArray.size(), Hout);
					memcpy((unsigned char*)(retNode->ID), Hout, 32);
					retNode->LeafCount = nLeaves;
				}

			}

		}

		//cout << endl << "Counter  " << counter << endl;
	}

	return true;
}


/// <summary>
/// Gets the element at the position specified by the ID.
/// </summary>
/// <param name="ID"></param>
/// <returns></returns>
template<typename T, typename R>
bool HashTree<T, R>::GetNodeData(Hash ID, T & Response)
{
	TreeNodeX *TempRoot = Root;

	int HLEN = ID.size() << 1; // Multiply by 2

	for (int i = 0; i < HLEN; i++)
	{
		byte Nibble = GetNibble(ID, i);

		if ((i < (HLEN - 1)))
		{
			if (TempRoot->Children[Nibble] == nullptr)
				return false;//throw exception("Node does not exist. Midway Break");

			TempRoot = TempRoot->Children[Nibble];
		}

		if (i == (HLEN - 1))
		{
			if ((TempRoot->Children[Nibble] != nullptr) && TempRoot->Children[Nibble]->IsLeaf)
			{
				//T TLN = TempRoot->Children[Nibble];

				Response = (((TreeLeafNode<T>*)TempRoot->Children[Nibble])->Value);
				return true;
			}
			else
			{
				return false;//throw exception("Node does not exist.");
			}
		}
	}

	return false;//throw exception("Node does not exist.");
}

template<typename T, typename R>
int64_t HashTree<T, R>::TraverseNodes()
{
	int64_t nodes = 0;
	int depth = 0;
	TotalMoney = 0;
	TraverseTree(Root, nodes, depth);

	//TraverseLevelOrder(Root, ref nodes);
	return nodes;
}


template<typename T, typename R>
int64_t HashTree<T, R>::TraverseNodesAndSave(fstream& Ledger)
{
	int64_t nodes = 0;
	int depth = 0;
	TotalMoney = 0;
	TraverseTreeAndSave(Ledger, Root, nodes, depth);

	//TraverseLevelOrder(Root, ref nodes);
	return nodes;
}


template<typename T, typename R>
vector<shared_ptr<LeafDataType>> HashTree<T, R>::TraverseNodesAndReturnLeaf()
{
	vector<shared_ptr<LeafDataType>> tempLeaves;
	int64_t nodes = 0;
	int depth = 0;

	TraverseTreeAndReturn(tempLeaves, Root, nodes, depth);

	//TraverseLevelOrder(Root, ref nodes);
	return tempLeaves;
}

/*
Get difference of two trees rooted at Root_1 and Root_2
Root_1 at responder
Root_2 at query side
*/

template<typename T, typename R>
void HashTree<T, R>::GetDifference(TreeNodeX* Root_1, TreeNodeX* Root_2)
{
	if (Root_1->ID == Root_2->ID)
		return;

	if (Root_1->ID != Root_2->ID)
	{
		for (int i = 0; i < 16; i++)
		{
			if (Root_1->Children[i] != nullptr)
			{
				if (Root_1->Children[i]->IsLeaf)
				{
					//base case
					NodeDifferenceVec.push_back(Root_1->Children[i]);
				}
				else if (Root_2->Children[i] == nullptr)
				{
					//inder all leaves shold be pushed back
					getAllLeafUnderNode(Root_1);
				}
				else
				{
					GetDifference(Root_1->Children[i], Root_2->Children[i]);
				}
			}
		}
	}
}

//limit in depth
template<typename T, typename R>
void HashTree<T, R>::GetDifference(TreeNodeX* Root_1, TreeNodeX* Root_2, int depth)
{
	int newDepth = depth++;
	if (Root_1->ID == Root_2->ID)
		return;

	if (Root_1->ID != Root_2->ID)
	{
		//after thiis depth just take all the laves
		//under this node
		if (depth > THRESHOLD_DEPTH)
		{
			getAllLeafUnderNode(Root_1);
			return;
		}

		for (int i = 0; i < 16; i++)
		{
			if (Root_1->Children[i] != nullptr)
			{
				if (Root_1->Children[i]->IsLeaf)
				{
					//base case
					NodeDifferenceVec.push_back(Root_1->Children[i]);
				}
				else if (Root_2->Children[i] == nullptr)
				{
					//inder all leaves shold be pushed back
					getAllLeafUnderNode(Root_1);
				}
				else
				{
					GetDifference(Root_1->Children[i], Root_2->Children[i], newDepth);
				}
			}
		}
	}

}


template<typename T, typename R>
void HashTree<T, R>::TraverseLevelOrder(TreeNodeX* Root, int64_t & FoundNodes)
{
	TreeNodeX* temp = Root;
	queue<TreeNodeX*> q;
	while (temp)
	{
		NodeDifferenceVec.push_back(temp);
		for (int i = 0; i < 16; i++)
		{
			if (Root->Children[i] != nullptr)
			{
				q.push(Root);
			}
		}
		temp = q.front();
		q.pop();
	}

}
/*
Given a depth return a vector with the nodes and
their IDs.
starts with depth 0
*/
template<typename T, typename R>
vector<TreeLevelDataType> HashTree<T, R>::TraverseLevelOrderDepth(int depth)
{

	if (depth >= 64)
	{
		vector<TreeLevelDataType> list0;
		cout << "Bitch please!";
		return list0;
	}

	int _depth = 0;
	vector<TreeLevelDataType> list1;
	vector<TreeLevelDataType> list2;
	//initialize with the root

	vector<char> c;

	TreeLevelDataType td(Root, c);
	list1.push_back(td);

	while (_depth != depth)
	{
		for (int i = 0; i < (int)list1.size(); i++)
		{
			list2.push_back(list1.at(i));
		}
		_depth++;
		//destroy list 1
		list1.clear();

		for (int i = 0; i < (int)list2.size(); i++)
		{
			for (int j = 0; j < 16; j++)
			{
				if (list2[i].treeNodeX->Children[j] != nullptr)
				{
					vector<char> c = list2[i].address;
					c.push_back(Constants::hexChars[j]);
					TreeLevelDataType td(list2[i].treeNodeX->Children[j], c);

					list1.push_back(td);
				}
			}
		}
		//destroy list2
		list2.clear();
	}
	return list1;
}

template<typename T, typename R>
vector<TreeSyncData> HashTree<T, R>::TraverseLevelOrderDepthSync(int depth)
{
	if (depth >= 64)
	{
		vector<TreeSyncData> list0;
		cout << "Bitch please!";
		return list0;
	}

	int _depth = 0;
	vector<TreeLevelDataType> list1;
	vector<TreeLevelDataType> list2;
	vector<TreeSyncData> list3;
	//initialize with the root

	vector<char> c;

	TreeLevelDataType td(Root, c);
	list1.push_back(td);

	while (_depth != depth)
	{
		for (int i = 0; i < (int)list1.size(); i++)
		{
			list2.push_back(list1.at(i));
		}
		_depth++;
		//destroy list 1
		list1.clear();

		for (int i = 0; i < (int)list2.size(); i++)
		{
			for (int j = 0; j < 16; j++)
			{
				if (list2[i].treeNodeX->Children[j] != nullptr)
				{
					vector<char> c = list2[i].address;
					c.push_back(Constants::hexChars[j]);
					TreeLevelDataType td(list2[i].treeNodeX->Children[j], c);

					list1.push_back(td);
				}
			}
		}
		//destroy list2
		list2.clear();
	}

	for (int i = 0; i < (int)list1.size(); i++)
	{
		Hash hash(list1[i].treeNodeX->ID, list1[i].treeNodeX->ID + 32);
		TreeSyncData LSD(hash, list1[i].address, list1[i].treeNodeX->LeafCount, false);

		list3.push_back(LSD);
	}
	return list3;
}

template<typename T, typename R>
void HashTree<T, R>::getAllLeafUnderNode(TreeNodeX* Root)
{
	for (int i = 0; i < 16; i++)
	{
		if (Root->Children[i] != nullptr)
		{
			if (Root->Children[i]->IsLeaf)
			{
				//base case
				NodeDifferenceVec.push_back(Root->Children[i]);
				return;
			}
			else
			{
				//rec
				getAllLeafUnderNode(Root->Children[i]);
			}
		}
	}
}

template<typename T, typename R>
void HashTree<T, R>::getAllLeafUnderNode(TreeNodeX* Root, vector<T>& Leaves)
{
	for (int i = 0; i < 16; i++)
	{
		if (Root->Children[i] != nullptr)
		{
			if (Root->Children[i]->IsLeaf)
			{
				//base case
				TreeLeafNode<T>* Leaf = (TreeLeafNode<T>*)Root->Children[i];
				Leaves.push_back(Leaf->Value);
				return;
			}
			else
			{
				//recursive
				getAllLeafUnderNode(Root->Children[i], Leaves);
			}
		}
	}
}


template<typename T, typename R>
void HashTree<T, R>::TraverseTree(TreeNodeX* Root, int64_t & FoundNodes, int _depth)
{
	int depth = _depth + 1;

	for (int i = 0; i < 16; i++)
	{
		if (Root->Children[i] != nullptr)
		{
			/////////////////////////////////////

			//if (Root.ID != null)
			//{
			//DisplayUtils.Display("ID : " + HexUtil.ToString(Root.ID.Hex) + " : " + depth);
			//}
			//else
			//{
			//DisplayUtils.Display("ID: NULL --------------- =============== ------------- :");
			//}

			////////////////////////////////

			if (!Root->Children[i]->IsLeaf)
			{
				TraverseTree(Root->Children[i], FoundNodes, depth);
			}
			if (Root->Children[i]->IsLeaf)
			{
				TreeLeafNode<T>* Leaf = (TreeLeafNode<T>*)Root->Children[i];

				TotalMoney += ((AccountInfo)Leaf->Value).Money;

				//DisplayUtils.Display("Node Traversed: " + HexUtil.ToString(Leaf.ID.Hex) +" - "+ ((AccountInfo)Leaf.Value).Money);
				FoundNodes++;
			}
		}
	}
}


template<typename T, typename R>
void HashTree<T, R>::TraverseTreeAndFetch_do(function<int(T)> fun)
{
	int64_t FoundNodes = 0; int _depth = 0;
	TraverseTreeAndFetch(Root, FoundNodes, _depth, fun);
}


template<typename T, typename R>
void HashTree<T, R>::TraverseTreeAndFetch(TreeNodeX* Root, int64_t & FoundNodes, int _depth, function<int(T)> fun)
{
	int depth = _depth + 1;

	//lf.MakeVerifyLedgerTree();

	for (int i = 0; i < 16; i++)
	{
		if (Root->Children[i] != nullptr)
		{
			if (!Root->Children[i]->IsLeaf)
			{
				TraverseTreeAndFetch(Root->Children[i], FoundNodes, depth, fun);
			}
			if (Root->Children[i]->IsLeaf && depth <= 64)
			{
				TreeLeafNode<T>* Leaf = (TreeLeafNode<T>*)Root->Children[i];

				fun(Leaf->Value);

				//AccountInfo ai = (AccountInfo)Leaf->Value;

				//TotalMoney += ai.Money;
				//Ledger << ToBase64String(ai.AccountID) << " " << ai.Money << " " << ai.Name << " " << ai.LastTransactionTime << "\n";
				//lf.treeToDB(ai.AccountID, ai.Money, ai.Money, ai.LastTransactionTime);

				FoundNodes++;
			}
		}
	}
}


template<typename T, typename R>
void HashTree<T, R>::TraverseTreeAndReturn(vector<shared_ptr<LeafDataType>> & tempLeaves, TreeNodeX* Root, int64_t & FoundNodes, int _depth)
{
	int depth = _depth + 1;

	for (int i = 0; i < 16; i++)
	{
		if (Root->Children[i] != nullptr)
		{
			if (!Root->Children[i]->IsLeaf)
			{
				TraverseTreeAndReturn(tempLeaves, Root->Children[i], FoundNodes, depth);
			}
			if (Root->Children[i]->IsLeaf)
			{
				TreeLeafNode<T>* Leaf = (TreeLeafNode<T>*)Root->Children[i];
				LeafDataType ld = Leaf->Value;

				shared_ptr<LeafDataType> data(&ld);
				tempLeaves.push_back(data);
				FoundNodes++;
			}
		}
	}
}

/*
Difference
*/
template<typename T, typename R>
vector<TreeSyncData> HashTree<T, R>::GetDifference(vector<TreeSyncData> other)
{
	vector<TreeSyncData> out;
	if (other.size() == 0 || other == nullptr)
	{
		return out;
	}

	for (int i = 0; i < other.size(); i++)
	{
		TreeSyncData TSD = other[i];
		vector<char> address = TSD.Address;

		if (address.size()>64)
		{
			continue;
		}

		TreeNodeX* temp = Root;
		vector<char> addr;

		for (int j = 0; i < address.size(); i++)
		{
			int indexPos = getImmediateChildren(address[j]);
			if (indexPos == -1)
			{
				break;
			}

			for (int k = 0; k < 16; k++)
			{
				if (k == indexPos)
				{
					if (temp->Children[k] == nullptr)
					{
						break;
					}

					temp = temp->Children[k];
					addr.push_back(Constants::hexChars[k]);
				}
			}
		}

		TreeSyncData toPacked = new TreeSyncData(temp->ID, addr, temp->LeafCount, false);
		out.push_back(toPacked);

	}
}

template<typename T, typename R>
vector<TreeSyncData> HashTree<T, R>::GetDifference(vector<TreeSyncData> other, vector<TreeSyncData> me)
{
	vector<TreeSyncData> out;
	int i, j;
	i = j = 0;

	while (true)
	{
		if (i == other.size())
			break;

		//all new
		if (j == me.size())
		{
			for (int t = i; t < other.size(); t++)
			{
				TreeSyncData LSD(other[t], false);

				// Get leaf nodes if the count is below threshold.
				LSD.GetAll = (other[t].LeafCount <= Constants::SYNC_LEAF_COUNT_THRESHOLD);

				out.push_back(LSD);
			}
			break;
		}
		if (other[i].Address.size() != me[j].Address.size())
		{
			return vector<TreeSyncData>();
		}
		int compare = CompareHexString(other[i].Address, me[j].Address);

		if (compare == 0)
		{

			if (!CompareCharString(other[i].ID, me[j].ID, 32, 32))
			{
				out.push_back(other[i]);
			}

			i++;
			j++;
		}
		//new
		else if (compare == -1)
		{
			TreeSyncData LSD(other[t], false);

			// Get leaf nodes if the count is below threshold.
			LSD.GetAll = (other[t].LeafCount <= Constants::SYNC_LEAF_COUNT_THRESHOLD);

			out.push_back(LSD);

			i++;
		}
		else
		{
			// thenga i have you dont
			j++;
		}
	}

	return out;
}

/*given a node id return all its 16 immediate children
*/
template<typename T, typename R>
bool HashTree<T, R>::getImmediateChildren(string ID, vector<TreeNodeX*> &vec)
{
	if (Root == nullptr)
		return false;

	TreeNodeX* temp = Root;

	TreeNodeX* pta[16];

	for (int j = 0; j < (int) ID.length(); j++)
	{
		for (int i = 0; i < 16; i++)
		{
			int index = getIndex(ID[j]);
			
			if (index == -1)
			{
				return false;
			}

			if (i == index)
			{
				if (temp->Children[i] != nullptr)
				{
					temp = temp->Children[i];
				}
				else
				{
					return false;
				}
			}
		}
	}

	for (int i = 0; i < 16; i++)
	{
		pta[i] = temp->Children[i];
	}

	vector<TreeNodeX*> v(pta, pta + 16);
	vec = v;

	return true;
}

// This will run for the synced node.
template<typename T, typename R>
void HashTree<T, R>::GetSyncNodeInfo(vector<TreeSyncData> input,
	vector<TreeSyncData>& internalNodes, vector<T>& leafNodes)
{
	//sanity check
	if (input.size() == 0)
		return;

	for (int i = 0; i < (int)input.size(); i++)
	{
		//if only next internal is required
		if (!input[i].GetAll)
		{
			// cout << "false " << input[i].GetAll << endl;
			vector<char> Address = input[i].Address;

			TreeNodeX* temp = Root;

			//get to node
			for (int j = 0; j < Address.size(); j++)
			{
				if (temp->Children[getIndex(Address[j])] == nullptr)
					return;

				temp = temp->Children[getIndex(Address[j])];
			}

			//fetch all of its children
			//and put to the vector
			for (int j = 0; j < 16; j++)
			{
				if (temp->Children[j] != nullptr)
				{
					TreeNodeX* chana = temp->Children[j];
					//make a local copy
					vector<char> _Address = Address;
					_Address.push_back(getNibbleSingle(j));

					Hash ChanaID(chana->ID, chana->ID + 32);
					TreeSyncData LSDchana(ChanaID, _Address, chana->LeafCount, false);
					internalNodes.push_back(LSDchana);
				}
			}
		}

		if (input[i].GetAll)
		{
			// cout << "true " << input[i].GetAll << endl;

			vector<char> Address = input[i].Address;

			TreeNodeX* temp = Root;

			// Get to node
			for (int j = 0; j < (int)Address.size(); j++)
			{
				if (temp->Children[getIndex(Address[j])] == nullptr)
					return;

				temp = temp->Children[getIndex(Address[j])];
			}

			// Fetch all of its leaves
			// and put to the vector 
			getAllLeafUnderNode(temp, leafNodes);

		}
	}
}



#endif