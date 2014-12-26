

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TNetD.Transactions;

namespace TNetD.Tree
{
    /// <summary>
    /// This represents a Merkle Hash tree, each node has 16 child nodes.
    /// The depth is defined by the length of the Hash in Nibbles.
    /// Insertion / Updation is non-recursive and constant time.
    /// The leaf nodes are sorted lists of elements.
    /// </summary>
    class ListHashTree
    {
        long _TraversedNodes = 0;

        public long TotalMoney = 0;

        public long TraversedNodes
        {
            get { return _TraversedNodes; }
        }

        ListTreeNode Root;

        public ListHashTree()
        {
            Root = new ListTreeNode();
        }

        public LeafDataType this[Hash index]
        {
            get { return GetNodeData(index); }
        }

        /// <summary>
        /// Get the RootHash of the Merkle Tree
        /// </summary>
        /// <returns></returns>
        public Hash GetRootHash()
        {
            if (Root != null)
            {
                if (Root.Hash != null)
                {
                    return Root.Hash;
                }
                else
                {
                    throw new Exception("Root Hash is NULL");
                }
            }
            else
            {
                throw new Exception("Root is NULL");
            }
        }

        /// <summary>
        /// Adds an element to the tree. If the element exists, update it.
        /// </summary>
        /// <param name="Value">The Value to be added</param>
        /// <returns></returns>
        public bool AddUpdate(LeafDataType Value)
        {
            bool Good = false;

            Hash ID = Value.GetID();

            ListTreeNode TempRoot = Root;

            Stack<ListTreeNode> PathStack = new Stack<ListTreeNode>();

            PathStack.Push(TempRoot);

            int LeafDepth = Constants.HashTree_NodeListDepth; //ID.Hex.Length << 1; // Multiply by 2

            for (int i = 0; i < LeafDepth; i++)
            {
                byte Nibble = ID.GetNibble(i);

                // Insert new node
                if ((TempRoot.Children[Nibble] == null) && (i < (LeafDepth - 1)))
                {
                    TempRoot.Children[Nibble] = new ListTreeNode();
                    _TraversedNodes++;
                }

                // Make child as curent
                if ((i < (LeafDepth - 1)))
                {
                    TempRoot = TempRoot.Children[Nibble];
                    PathStack.Push(TempRoot);
                }

                // Insert leaf
                if (i == (LeafDepth - 1))
                {
                    // Add new leaf node
                    if (TempRoot.Children[Nibble] == null)
                    {
                        Dictionary<Hash, LeafDataType> _Values = new Dictionary<Hash, LeafDataType>();
                        _Values.Add(Value.GetID(), Value);
                        TempRoot.Children[Nibble] = new ListTreeLeafNode(_Values);
                        _TraversedNodes++;

                        Good = true;
                    } // Add to current leaf node
                    else if (TempRoot.Children[Nibble].GetType() == typeof(ListTreeLeafNode))
                    {
                        ((ListTreeLeafNode)TempRoot.Children[Nibble]).Add(Value.GetID(), Value);
                        Good = true;
                    }
                    else
                    {
                        throw new Exception("Bad Mess !!!");
                    }
                }
            }

            ///  Traceback.
            bool LeafDone = false;

            while (PathStack.Count > 0)
            {
                ListTreeNode val = PathStack.Pop();
                Hash NodeHash = null;

                if (!LeafDone)
                {
                    // Do the hashing for the leaf.
                    List<byte> hashDataArray = new List<byte>();
                    for (int i = 0; i < 16; i++)
                    {
                        if (val.Children[i] != null)
                        {
                            var ai = (ListTreeLeafNode)val.Children[i];
                            Hash hsh = ai.GetHash();
                            hashDataArray.AddRange(hsh.Hex);
                            // DisplayUtils.Display("CH : - " + i.ToString("X") + " : " + HexUtil.ToString(hsh.Hex));
                        }
                    }
                    NodeHash = new Hash((new SHA512Managed()).ComputeHash(hashDataArray.ToArray()).Take(32).ToArray());
                    LeafDone = true;
                }
                else
                {
                    // Perform hashing for node.
                    List<byte> _tempHash = new List<byte>();
                    for (int i = 0; i < 16; i++)
                    {
                        if (val.Children[i] != null)
                        {
                            var ai = (ListTreeNode)val.Children[i];
                            _tempHash.AddRange(ai.Hash.Hex);
                            // DisplayUtils.Display("- " + i.ToString("X") + " : " + HexUtil.ToString(ai.ID.Hex) );
                        }
                    }
                    NodeHash = new Hash((new SHA512Managed()).ComputeHash(_tempHash.ToArray()).Take(32).ToArray());
                }

                val.Hash = NodeHash;
            }

            return Good;
        }

        public int TraverseNodes()
        {
            int nodes = 0;
            int depth = 0;
            _TraversedNodes = 0;
            TotalMoney = 0;
            TraverseTree(Root, ref nodes, depth);
            _TraversedNodes = nodes;
            //TraverseLevelOrder(Root, ref nodes);
            return nodes;
        }

        private List<LeafDataType> TraverseTree(ListTreeNode Root, ref int FoundElementCount, int _depth)
        {
            List<LeafDataType> foundElements = new List<LeafDataType>();

            int depth = _depth + 1;

            for (int i = 0; i < 16; i++)
            {
                if (Root.Children[i] != null)
                {
                    /////////////////////////////////////
                    
                    /*if (Root.Hash != null)
                    {
                        DisplayUtils.Display("ID : " + HexUtil.ToString(Root.Hash.Hex) + " : " + depth);
                    }
                    else
                    {
                        DisplayUtils.Display("ID: NULL --------------- =============== ------------- :");
                    }*/

                    ////////////////////////////////

                    if (!Root.Children[i].IsLeaf)
                    {
                        TraverseTree(Root.Children[i], ref FoundElementCount, depth);
                    }

                    if (Root.Children[i].IsLeaf)
                    {
                        ListTreeLeafNode Leaf = (ListTreeLeafNode)Root.Children[i];

                        LeafDataType[] ldts = Leaf.GetAllItems();

                        foundElements.AddRange(ldts);

                        //TotalMoney += ((AccountInfo)Leaf.Value).Money;
                        //DisplayUtils.Display("\nNode Traversed: " + HexUtil.ToString(Leaf.GetHash().Hex) + " - " +
                          //  AccountInfo.CalculateTotalMoney(ldts) );

                        foreach(LeafDataType ld in ldts)
                        {
                            //DisplayUtils.Display("          --- ID: " + HexUtil.ToString(ld.GetID().Hex) + " - Money " + ((AccountInfo)ld).Money);
                            TotalMoney +=((AccountInfo)ld).Money;
                        }

                        FoundElementCount += ldts.Length;
                    }
                }
            }

            return foundElements;
        }

        /// <summary>
        /// Gets the element at the position specified by the ID.
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public LeafDataType GetNodeData(Hash ID)
        {
            ListTreeNode TempRoot = Root;

            int LeafDepth = Constants.HashTree_NodeListDepth; //ID.Hex.Length << 1; // Multiply by 2

            for (int i = 0; i < LeafDepth; i++)
            {
                byte Nibble = ID.GetNibble(i);

                if ((i < (LeafDepth - 1)))
                {
                    if (TempRoot.Children[Nibble] == null) throw new Exception("Node does not exist. Midway Break");

                    TempRoot = TempRoot.Children[Nibble];
                }

                // Reached the leaf node. Now look for the actual node.
                if (i == (LeafDepth - 1))
                {
                    if ((TempRoot.Children[Nibble] != null) && TempRoot.Children[Nibble].IsLeaf)
                    {
                        ListTreeLeafNode TLN = (ListTreeLeafNode)TempRoot.Children[Nibble];
                        
                        if(TLN.ContainsElement(ID))
                        {
                            return TLN[ID];
                        }
                        else
                        {
                            throw new Exception("Leaf Node does not exist in List.");
                        }                        
                    }
                    else
                    {
                        throw new Exception("Leaf Node does not exist.");
                    }
                }
            }

            throw new Exception("Node does not exist.");
        }

        /// <summary>
        /// Traverse down tree to see the availability of given leaf node.
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool NodeExists(Hash ID)
        {
            ListTreeNode TempRoot = Root;

            int LeafDepth = Constants.HashTree_NodeListDepth;

            for (int i = 0; i < LeafDepth; i++)
            {
                byte Nibble = ID.GetNibble(i);

                if ((i < (LeafDepth - 1)))
                {
                    if (TempRoot.Children[Nibble] == null)
                    {
                        return false;
                    }

                    TempRoot = TempRoot.Children[Nibble];
                }

                if (i == (LeafDepth - 1))
                {
                    if ((TempRoot.Children[Nibble] != null) && TempRoot.Children[Nibble].IsLeaf)
                    {
                        ListTreeLeafNode TLN = (ListTreeLeafNode)TempRoot.Children[Nibble];

                        if (TLN.ContainsElement(ID))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        } 
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;
        }

    }
}