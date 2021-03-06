﻿/*
 *  @Author: Arpan Jati | Aritra Dhar
 *  @Version: 1.0
 *  @Date: Dec 2014
 *
 *  @Description: ListHashTree
 *      Features: Depth (Config File)
 *      Child Per Node : 16
 *      Traverses using the bytes of the ID
 *      Insertion is Non-Recursive
 *      Traversal is currently recursive. 
 *      
 *  CRITICAL: SOME GOOD TEST VECTORS
 */

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

    public enum TreeResponseType { Added, Removed, Updated, Failed, NothingDone };

    public enum TraverseResult
    {
        Success, MidwayBreak, NodeDoesNotExist, LeafDoesNotExist, ElementDoesNotExistInLeaf,
        ReachedLeafNode, TooLongAddressPath
    }

    public delegate TreeResponseType LeafDataFetchEventHandler(LeafDataType[] accountInfo);

    /// <summary>
    /// This represents a Merkle Hash tree, each node has 16 child nodes.
    /// The depth is defined by the length of the Hash in Nibbles.
    /// Insertion / Updation is non-recursive and constant time.
    /// The leaf nodes are sorted lists of elements.
    /// </summary>
    public class ListHashTree
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
        public TreeResponseType AddUpdate(LeafDataType Value)
        {
            bool Good = false;
            bool Add = true;

            Hash ID = Value.GetID();

            ListTreeNode TempRoot = Root;

            Stack<ListTreeNode> PathStack = new Stack<ListTreeNode>();
            List<byte> addressNibbleList = new List<byte>();

            PathStack.Push(TempRoot);

            int LeafDepth = Constants.HashTree_NodeListDepth; //ID.Hex.Length << 1; // Multiply by 2

            for (int i = 0; i < LeafDepth; i++)
            {
                byte Nibble = ID.GetNibble(i);
                addressNibbleList.Add(Nibble);

                // Insert new node
                if ((TempRoot.Children[Nibble] == null) && (i < (LeafDepth - 1)))
                {
                    TempRoot.Children[Nibble] = new ListTreeNode();
                    TempRoot.Children[Nibble].addressNibbles = new Hash(addressNibbleList.ToArray());
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
                        TempRoot.Children[Nibble].addressNibbles = new Hash(addressNibbleList.ToArray());
                        _TraversedNodes++;
                        Add = true;
                        Good = true;
                    } // Add to current leaf node
                    else if (TempRoot.Children[Nibble].GetType() == typeof(ListTreeLeafNode))
                    {
                        var ltln = (ListTreeLeafNode)TempRoot.Children[Nibble];
                        Add = ltln.AddUpdate(Value.GetID(), Value);

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
                long entityCount = 0;
                List<byte> _tempHash = new List<byte>();
                if (!LeafDone)
                {
                    // Do the hashing for the leaf.
                    for (int i = 0; i < 16; i++)
                    {
                        if (val.Children[i] != null)
                        {
                            var ai = (ListTreeLeafNode)val.Children[i];
                            entityCount += ai.Count;
                            Hash hsh = ai.GetHash();
                            _tempHash.AddRange(hsh.Hex);
                            //DisplayUtils.Display("CH : - " + i.ToString("X") + " : " + HexUtil.ToString(hsh.Hex));
                        }
                    }
                    NodeHash = new Hash((new SHA512Cng()).ComputeHash(_tempHash.ToArray()).Take(32).ToArray());
                    LeafDone = true;
                }
                else
                {
                    // Perform hashing for node.                  
                    for (int i = 0; i < 16; i++)
                    {
                        if (val.Children[i] != null)
                        {
                            var ai = (ListTreeNode)val.Children[i];
                            entityCount += ai.LeafCount;
                            _tempHash.AddRange(ai.Hash.Hex);
                            //DisplayUtils.Display("- " + i.ToString("X") + " : " + HexUtil.ToString(ai.Hash.Hex));
                        }
                    }
                    NodeHash = new Hash((new SHA512Cng()).ComputeHash(_tempHash.ToArray()).Take(32).ToArray());
                }

                val.SetLeafCount(entityCount);
                val.SetHash(NodeHash);
            }

            return Good ? //IS Good
                (Add ? TreeResponseType.Added : TreeResponseType.Updated) : // YES: Added / Updated
                TreeResponseType.Failed; // NO
        }

        public void TraverseAllNodes(ref long FoundLeafDataCount, ref long FoundNodesCount, LeafDataFetchEventHandler leafDataFetch)
        {
            //int nodes = 0;
            //int elements = 0;
            int depth = 0;
            //_TraversedNodes = 0;
            //TotalMoney = 0;
            TraverseTree(Root, ref FoundLeafDataCount, ref FoundNodesCount, leafDataFetch, depth);
            //_TraversedNodes = nodes;
            //_TraversedElements = elements;
            //TraverseLevelOrder(Root, ref nodes);
            //return nodes;
        }

        private List<LeafDataType> TraverseTree(ListTreeNode Root, ref long FoundLeafDataCount, ref long FoundNodesCount, LeafDataFetchEventHandler leafDataFetch, long depth)
        {
            List<LeafDataType> foundElements = new List<LeafDataType>();

            // int depth = _depth + 1;

            for (int i = 0; i < 16; i++)
            {
                if (Root.Children[i] != null)
                {
                    /////////////////////////////////////

                    FoundNodesCount++;

                    if (Root.Hash != null)
                    {
                        //DisplayUtils.Display("ID : " + HexUtil.ToString(Root.Hash.Hex) + " : " + depth);                  
                    }
                    else
                    {
                        //DisplayUtils.Display("ID: NULL --------------- =============== ------------- :");
                    }

                    ////////////////////////////////

                    if (!Root.Children[i].IsLeaf)
                    {
                        //DisplayUtils.Display("Intermediate Traversed : " + Root.Children[i].Hash.ToString() + " : " + depth);

                        TraverseTree(Root.Children[i], ref FoundLeafDataCount, ref FoundNodesCount, leafDataFetch, depth + 1);
                    }
                    else
                    {
                        ListTreeLeafNode Leaf = (ListTreeLeafNode)Root.Children[i];

                        LeafDataType[] ldts = Leaf.GetAllItems();

                        if (leafDataFetch != null)
                            leafDataFetch(ldts);

                        foundElements.AddRange(ldts);

                        // TotalMoney += ((AccountInfo)Leaf.).Money;
                        //DisplayUtils.Display("\nLeaf Node Traversed: " + HexUtil.ToString(Leaf.GetHash().Hex) + " - " +
                        //AccountInfo.CalculateTotalMoney(ldts));

                        /*foreach (LeafDataType ld in ldts)
                        {
                            //DisplayUtils.Display("          --- ID: " + HexUtil.ToString(ld.GetID().Hex) + " - Money " + ((AccountInfo)ld).Money);
                            TotalMoney += ((AccountInfo)ld).Money;
                        }*/

                        FoundLeafDataCount += ldts.Length;
                    }
                }
            }

            return foundElements;
        }



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

        public TraverseResult TraverseToNode(Hash AddressNibbles, out ListTreeNode Leaf)
        {
            ListTreeNode TempRoot = Root;

            int LeafDepth = Constants.HashTree_NodeListDepth;

            byte[] addressNibbles = AddressNibbles.Hex;

            if (addressNibbles.Length <= LeafDepth)
            {
                for (int i = 0; i < AddressNibbles.Hex.Length; i++)
                {
                    byte Nibble = AddressNibbles.Hex[i];

                    if ((i < (LeafDepth - 1)))
                    {
                        if (TempRoot.Children[Nibble] == null)
                        {
                            Leaf = default(ListTreeLeafNode);
                            return TraverseResult.MidwayBreak;
                        }

                        TempRoot = TempRoot.Children[Nibble];
                    }

                    if (i == (addressNibbles.Length - 1)) // ValidateCondition
                    {
                        // Success
                        Leaf = TempRoot;
                        return TraverseResult.Success;
                    }
                }
            }
            else
            {
                Leaf = default(ListTreeLeafNode);
                return TraverseResult.TooLongAddressPath;
            }

            Leaf = default(ListTreeLeafNode);
            return TraverseResult.NodeDoesNotExist;
        }


        /// <summary>
        /// True if the node exists.
        /// This traverses the tree to get the info, so is slightly expensive. Still pretty fast.
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool NodeExists(Hash ID)
        {
            ListTreeLeafNode ltln;

            if (TraverseToLeaf(ID, out ltln) == TraverseResult.Success)
            {
                return true;
            }

            return false;
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

                    List<byte> _tempHash = new List<byte>();
                    long entityCount = 0;
                    int childCount = 0;

                    // Process the leaf [actual leafs are the clilds if this node]
                    if (!LeafDone)
                    {
                        // Do the hashing for the node.

                        for (int i = 0; i < 16; i++)
                        {
                            if (node.Children[i] != null)
                            {
                                ListTreeLeafNode leaf = (ListTreeLeafNode)node.Children[i];
                                entityCount += leaf.Count;
                                if (leaf.Count == 0) // No Elements in the Leaf
                                {
                                    node.Children[i] = null; // Delete the Leaf.
                                }
                                else
                                {
                                    Hash hsh = leaf.GetHash();
                                    _tempHash.AddRange(hsh.Hex);
                                    childCount++;
                                }
                            }
                        }

                        if (childCount > 0) // Update the value of the Leaf node hash.
                        {
                            Hash NodeHash = new Hash((new SHA512Cng()).ComputeHash(_tempHash.ToArray()).Take(32).ToArray());
                            node.SetHash(NodeHash);
                            node.SetLeafCount(entityCount);
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
                                    entityCount += ai.LeafCount;
                                    childCount++;
                                }
                            }
                        }

                        if (childCount > 0)
                        {
                            Hash NodeHash = new Hash((new SHA512Cng()).ComputeHash(_tempHash.ToArray()).Take(32).ToArray());
                            node.SetHash(NodeHash);
                            node.SetLeafCount(entityCount);
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

        //
        // Sync method ...
        //

        /// <summary>
        /// Make sure, no write operation is done on it.
        /// </summary>
        /// <returns></returns>
        public ListTreeNode RootNode
        {
            get { return Root; }
        }

        /// <summary>
        /// This method will give all the leaves under a node
        /// Method is recursive. Set MaxLeaves properly.
        /// </summary>
        /// /// <param name="MaxLeaves"></param>
        /// <param name="Node"></param>
        /// <param name="Leaves"></param>
        /// <returns></returns>
        public void GetAllLeavesUnderNode(long MaxLeaves, ListTreeNode Node, ref List<LeafDataType> Leaves)
        {
            //List<LeafDataType> leaves = new List<LeafDataType>();

            for (int i = 0; i < 16; i++)
            {
                if (Node.Children[i] != null)
                {         
                    // Base condition of the recursion
                    if (Node.Children[i].IsLeaf)
                    {
                        ListTreeLeafNode leafNode = (ListTreeLeafNode)Node.Children[i];

                        if ((Leaves.Count + leafNode.Count) > MaxLeaves) break;

                        SortedDictionary<Hash, LeafDataType> val = leafNode.Values;
                        foreach (KeyValuePair<Hash, LeafDataType> v in val)
                        {
                            Leaves.Add(v.Value);
                        }

                        return;
                    } // recusion steps                    
                    else
                    {
                        GetAllLeavesUnderNode(MaxLeaves, Node.Children[i], ref Leaves);
                    }
                }
            }
        }

        /*
          public List<TreeSyncData> getDifference(List<ListTreeNode> other, List<ListTreeNode> me)
          {
              List<TreeSyncData> difference = new List<TreeSyncData>();

              int i = 0, j = 0;

              while (true)
              {
                  if (i == other.Count)
                      break;

                  //all new
                  if (j == me.Count)
                  {
                      for (int t = i; t < other.Count; t++)
                      {
                          TreeSyncData TSD = new TreeSyncData(other[t], false);
                          TSD.setGetAll(other[t].LeafCount <= Constants.SYNC_LEAF_COUNT_THRESHOLD);
                      }
                      break;
                  }

                  if (other[i].addressNibbles.Hex.Length != me[j].addressNibbles.Hex.Length)
                  {
                      return new List<TreeSyncData>();
                  }

                  int compare = other[i].addressNibbles.CompareTo(me[j].addressNibbles);

                  if (compare == 0)
                  {
                      i++;
                      j++;
                  }
                  //new
                  else if (compare == -1)
                  {
                      TreeSyncData LSD = new TreeSyncData(other[i], false);
                      // Get leaf nodes if the count is below threshold.
                      LSD.setGetAll(other[i].LeafCount <= Constants.SYNC_LEAF_COUNT_THRESHOLD);
                      difference.Add(LSD);
                      i++;
                  }
                  else
                  {
                      // thenga i have you dont
                      //pretty much fucked up i guess :P
                      j++;
                  }
              }

              return difference;
          }

          public void sendTreeSyncData(List<TreeSyncData> incoming, out List<ListTreeNode> internalNodes, out List<LeafDataType> leaves)
          {
              List<ListTreeNode> outInternal = new List<ListTreeNode>();
              List<LeafDataType> outLeaves = new List<LeafDataType>();
              for (int i = 0; i < incoming.Count; i++)
              {
                  TreeSyncData TSD = incoming[i];
                  if (TSD.getAll)
                  {
                      ListTreeNode LTN = TSD.LTN;
                      List<LeafDataType> temp = new List<LeafDataType>();
                      this.getAllLeafUnderNode(LTN, out temp);
                      outLeaves.AddRange(temp);
                  }
                  else
                  {
                      Hash address = TSD.address;
                      List<ListTreeNode> temp = new List<ListTreeNode>();
                      this.getImmediateChildren(address, out temp);
                      outInternal.AddRange(temp);
                  }
              }
              internalNodes = outInternal;
              leaves = outLeaves;

          }


          //gives back immediate children
          public bool getImmediateChildren(Hash hash, out List<ListTreeNode> nodes)
          {
              nodes = new List<ListTreeNode>();

              List<ListTreeNode> nd = new List<ListTreeNode>();

              ListTreeNode TempRoot = Root;
              int depth = Constants.HashTree_NodeListDepth;

              for (int i = 0; i < depth; i++)
              {
                  byte Nibble = hash.GetNibble(i);

                  if ((i < (depth - 1)))
                  {
                      if (TempRoot.Children[Nibble] == null)
                      {
                          nodes = nd;
                          return false;
                      }

                      TempRoot = TempRoot.Children[Nibble];
                  }

                  if (i == (depth - 1))
                  {
                      nodes = nd;
                      return false;
                  }
              }

              for (int i = 0; i < 16; i++)
              {
                  if (TempRoot.Children[i] != null)
                      nodes.Add(TempRoot.Children[i]);
              }
              nodes = nd;
              return true;
          }

          public List<ListTreeNode> depthOrderTraversal(int depth)
          {
              List<ListTreeNode> listToReturn = new List<ListTreeNode>();
              ListTreeNode tempRoot = Root;

              List<ListTreeNode> list1 = new List<ListTreeNode>();
              List<ListTreeNode> list2 = new List<ListTreeNode>();

              list1.Add(tempRoot);

              int currentDepth = 0;

              while (currentDepth < depth)
              {
                  currentDepth++;

                  for (int item = 0; item < list1.Count; item++)
                  {
                      for (int i = 0; i < 16; i++)
                      {
                          ListTreeNode currentNode = list1[item];
                          if (currentNode.Children[i] != null)
                          {
                              list2.Add(currentNode.Children[i]);
                          }
                      }
                  }

                  list1.Clear();
                  list1.AddRange(list2);
                  list2.Clear();
              }

              listToReturn = list1;

              return listToReturn;

          }


          */


    }
}