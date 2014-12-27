﻿

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
        long _TraversedElements = 0;
        public long TotalMoney = 0;

        public long TraversedNodes
        {
            get { return _TraversedNodes; }
        }

        public long TraversedElements
        {
            get { return _TraversedElements; }
        }

        ListTreeNode Root;

        public ListHashTree()
        {
            Root = new ListTreeNode();
        }

        public LeafDataType this[Hash index]
        {
            get
            {
                LeafDataType ltd;

                TraverseResult tr = GetNodeData(index, out ltd);
                if (tr == TraverseResult.Success)
                {
                    return ltd;
                }
                else
                {
                    throw new ArgumentException("Could not fetch Element: " + tr);
                }
            }
        }

        /// <summary>
        /// Get the RootHash of the Merkle Tree.
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
                    List<byte> _tempHash = new List<byte>();
                    for (int i = 0; i < 16; i++)
                    {
                        if (val.Children[i] != null)
                        {
                            var ai = (ListTreeLeafNode)val.Children[i];
                            Hash hsh = ai.GetHash();
                            _tempHash.AddRange(hsh.Hex);
                            // DisplayUtils.Display("CH : - " + i.ToString("X") + " : " + HexUtil.ToString(hsh.Hex));
                        }
                    }
                    NodeHash = new Hash((new SHA512Managed()).ComputeHash(_tempHash.ToArray()).Take(32).ToArray());
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

                val.SetHash(NodeHash);
            }

            return Good;
        }

        public int TraverseNodes()
        {
            int nodes = 0;
            int elements = 0;
            int depth = 0;
            _TraversedNodes = 0;
            TotalMoney = 0;
            TraverseTree(Root, ref elements, ref nodes, depth);
            _TraversedNodes = nodes;
            _TraversedElements = elements;
            //TraverseLevelOrder(Root, ref nodes);
            return nodes;
        }

        private List<LeafDataType> TraverseTree(ListTreeNode Root, ref int FoundElementCount, ref int FoundNodesCount, int _depth)
        {
            List<LeafDataType> foundElements = new List<LeafDataType>();

            int depth = _depth + 1;

            for (int i = 0; i < 16; i++)
            {
                if (Root.Children[i] != null)
                {
                    /////////////////////////////////////

                    if (Root.Hash != null)
                    {
                        //DisplayUtils.Display("ID : " + HexUtil.ToString(Root.Hash.Hex) + " : " + depth);
                        FoundNodesCount++;
                    }
                    else
                    {
                        //DisplayUtils.Display("ID: NULL --------------- =============== ------------- :");
                    }

                    ////////////////////////////////

                    if (!Root.Children[i].IsLeaf)
                    {
                        TraverseTree(Root.Children[i], ref FoundElementCount, ref FoundNodesCount, depth);
                    }

                    if (Root.Children[i].IsLeaf)
                    {
                        ListTreeLeafNode Leaf = (ListTreeLeafNode)Root.Children[i];

                        LeafDataType[] ldts = Leaf.GetAllItems();

                        foundElements.AddRange(ldts);

                        // TotalMoney += ((AccountInfo)Leaf.).Money;
                        //DisplayUtils.Display("\nNode Traversed: " + HexUtil.ToString(Leaf.GetHash().Hex) + " - " +
                        //AccountInfo.CalculateTotalMoney(ldts));

                        foreach (LeafDataType ld in ldts)
                        {
                            //DisplayUtils.Display("          --- ID: " + HexUtil.ToString(ld.GetID().Hex) + " - Money " + ((AccountInfo)ld).Money);
                            TotalMoney += ((AccountInfo)ld).Money;
                        }

                        FoundElementCount += ldts.Length;
                    }
                }
            }

            return foundElements;
        }

        public enum TraverseResult { Success, MidwayBreak, NodeDoesNotExist, LeafDoesNotExist, ElementDoesNotExistInLeaf }

        /// <summary>
        /// Traverses to a leaf node and returns the stack while traversing.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Leaf"></param>
        /// <param name="PathStack"></param>
        /// <returns></returns>
        ///
        private TraverseResult TraverseToLeaf(Hash ID, out ListTreeLeafNode Leaf, out Stack<ListTreeNode> PathStack)
        {
            PathStack = new Stack<ListTreeNode>();

            ListTreeNode TempRoot = Root;
            PathStack.Push(TempRoot);

            int LeafDepth = Constants.HashTree_NodeListDepth;

            for (int i = 0; i < LeafDepth; i++)
            {
                byte Nibble = ID.GetNibble(i);

                if ((i < (LeafDepth - 1)))
                {
                    if (TempRoot.Children[Nibble] == null)
                    {
                        Leaf = default(ListTreeLeafNode);
                        return TraverseResult.MidwayBreak;
                    }

                    TempRoot = TempRoot.Children[Nibble];
                    PathStack.Push(TempRoot);
                }

                if (i == (LeafDepth - 1))
                {
                    if ((TempRoot.Children[Nibble] != null) && TempRoot.Children[Nibble].IsLeaf)
                    {
                        Leaf = (ListTreeLeafNode)TempRoot.Children[Nibble];
                        return TraverseResult.Success;
                    }
                    else
                    {
                        Leaf = default(ListTreeLeafNode);
                        return TraverseResult.LeafDoesNotExist;
                    }
                }
            }

            Leaf = default(ListTreeLeafNode);
            return TraverseResult.NodeDoesNotExist;
        }

        /// <summary>
        /// Traverses to a leaf node.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Leaf"></param>
        /// <returns></returns>
        private TraverseResult TraverseToLeaf(Hash ID, out ListTreeLeafNode Leaf)
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
                        Leaf = default(ListTreeLeafNode);
                        return TraverseResult.MidwayBreak;
                    }

                    TempRoot = TempRoot.Children[Nibble];
                }

                if (i == (LeafDepth - 1))
                {
                    if ((TempRoot.Children[Nibble] != null) && TempRoot.Children[Nibble].IsLeaf)
                    {
                        Leaf = (ListTreeLeafNode)TempRoot.Children[Nibble];
                        return TraverseResult.Success;
                    }
                    else
                    {
                        Leaf = default(ListTreeLeafNode);
                        return TraverseResult.LeafDoesNotExist;
                    }
                }
            }

            Leaf = default(ListTreeLeafNode);
            return TraverseResult.NodeDoesNotExist;
        }

        /// <summary>
        /// Gets the element at the position specified by the ID.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Leaf"></param>
        /// <returns></returns>
        public TraverseResult GetNodeData(Hash ID, out LeafDataType Leaf)
        {
            ListTreeLeafNode LN;
            TraverseResult tr = TraverseToLeaf(ID, out LN);

            if (tr == TraverseResult.Success)
            {
                if (LN.ContainsElement(ID))
                {
                    Leaf = LN[ID];
                    return TraverseResult.Success;
                }
                else
                {
                    Leaf = default(LeafDataType);
                    return TraverseResult.ElementDoesNotExistInLeaf;
                }
            }
            else
            {
                Leaf = default(LeafDataType);
                return tr;
            }
        }


        /// <summary>
        /// Traverse down tree to see the availability of given leaf node.
        /// This is equivalent to trying to fetch the element, in terms of complexity.
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool Exists(Hash ID)
        {
            LeafDataType Temp;

            return GetNodeData(ID, out Temp) == TraverseResult.Success;
        }

        public bool DeleteNode(Hash ID)
        {
            Stack<ListTreeNode> PathStack;
            ListTreeLeafNode LTLN;

            bool result = false;

            TraverseResult tr = TraverseToLeaf(ID, out LTLN, out PathStack);

            if (tr == TraverseResult.Success)
            {
                // Delete the node in the leaf.

                result = LTLN.DeleteElement(ID);

                // Fix the tree.
                ////////////////////////////

                bool LeafDone = false;

                while (PathStack.Count > 0)
                {
                    ListTreeNode node = PathStack.Pop();

                    // Process the leaf [actual leafs are the clilds if this node]
                    if (!LeafDone)
                    {
                        // Do the hashing for the node.
                        List<byte> _tempHash = new List<byte>();
                        int c_count = 0;
                        for (int i = 0; i < 16; i++)
                        {
                            if (node.Children[i] != null)
                            {
                                ListTreeLeafNode leaf = (ListTreeLeafNode)node.Children[i];

                                if (leaf.Count == 0) // No Elements in the Leaf
                                {
                                    node.Children[i] = null; // Delete the Leaf.
                                }
                                else
                                {
                                    Hash hsh = leaf.GetHash();
                                    _tempHash.AddRange(hsh.Hex);
                                    c_count++;
                                }
                            }
                        }

                        if (c_count > 0) // Update the value of the Leaf node hash.
                        {
                            Hash NodeHash = new Hash((new SHA512Managed()).ComputeHash(_tempHash.ToArray()).Take(32).ToArray());
                            node.SetHash(NodeHash);
                        }
                        else // No children: Node should be deleted.
                        {
                            node = null;
                        }

                        LeafDone = true;
                    }
                    else // Process the intermediate nodes
                    {
                        // Perform hashing for node.
                        List<byte> _tempHash = new List<byte>();
                        int c_count = 0;
                        for (int i = 0; i < 16; i++)
                        {
                            if (node.Children[i] != null)
                            {
                                if (node.Children[i].ChildCount() == 0)
                                {
                                    node.Children[i] = null;
                                }
                                else
                                {
                                    var ai = (ListTreeNode)node.Children[i];
                                    _tempHash.AddRange(ai.Hash.Hex);
                                    c_count++;
                                }
                            }
                        }

                        if (c_count > 0)
                        {
                            Hash NodeHash = new Hash((new SHA512Managed()).ComputeHash(_tempHash.ToArray()).Take(32).ToArray());
                            node.SetHash(NodeHash);
                        }
                        else // No children: Node should be deleted.
                        {
                            node = null;
                        }
                    }
                }
            }
            else
            {
                DisplayUtils.Display("Traverse Failure ... . !!!");
            }

            return result;
        }






    }
}