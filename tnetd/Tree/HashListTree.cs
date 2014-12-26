

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
    /// </summary>
    class HashListTree
    {
        long _TotalNodes = 0;

        public long TotalMoney = 0;

        public long TotalNodes
        {
            get { return _TotalNodes; }
        }

        TreeNode<LeafDataType> Root;

        public HashListTree()
        {
            Root = new TreeNode<LeafDataType>();
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
                if (Root.ID != null)
                {
                    return Root.ID;
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

            TreeNode<LeafDataType> TempRoot = Root;

            Stack<TreeNode<LeafDataType>> PathStack = new Stack<TreeNode<LeafDataType>>();

            PathStack.Push(TempRoot);

            int HLEN = ID.Hex.Length << 1; // Multiply by 2

            for (int i = 0; i < HLEN; i++)
            {
                byte Nibble = ID.GetNibble(i);

                // Insert new node
                if ((TempRoot.Children[Nibble] == null) && (i < (HLEN - 1)))
                {
                    TempRoot.Children[Nibble] = new TreeNode<LeafDataType>();
                    _TotalNodes++;
                }

                // Make child as curent
                if ((i < (HLEN - 1)))
                {
                    TempRoot = TempRoot.Children[Nibble];
                    PathStack.Push(TempRoot);
                }

                // Insert leaf
                if (i == (HLEN - 1))
                {
                    if (TempRoot.Children[Nibble] == null)
                    {
                        TempRoot.Children[Nibble] = new TreeLeafNode<LeafDataType>(ID, Value);
                        _TotalNodes++;

                        // DisplayUtils.Display("Node Added: " + HexUtil.ToString(ID.Hex));
                        Good = true;
                    }
                    else if (TempRoot.Children[Nibble].GetType() == typeof(TreeLeafNode<LeafDataType>))
                    {
                        if (TempRoot.Children[Nibble].ID == ID)
                        {
                            ((TreeLeafNode<LeafDataType>)TempRoot.Children[Nibble]).Value = Value;

                            Good = true;
                        }
                        else
                        {
                            throw new Exception("Node ID Mismatch, Bad Mess !!!");
                        }
                    }
                    else
                    {
                        throw new Exception("Node Already Exists");
                    }
                }
            }

            ///  Traceback.
            bool LeafDone = false;

            while (PathStack.Count > 0)
            {
                TreeNode<LeafDataType> val = PathStack.Pop();
                Hash NodeHash = null;

                if (!LeafDone)
                {
                    List<byte> hashDataArray = new List<byte>();
                    for (int i = 0; i < 16; i++)
                    {
                        if (val.Children[i] != null)
                        {
                            var ai = (TreeLeafNode<LeafDataType>)val.Children[i];
                            Hash hsh = ai.Value.GetHash();
                            hashDataArray.AddRange(hsh.Hex);

                            // DisplayUtils.Display("CH : - " + i.ToString("X") + " : " + HexUtil.ToString(hsh.Hex));
                        }
                    }
                    NodeHash = new Hash((new SHA512Managed()).ComputeHash(hashDataArray.ToArray()).Take(32).ToArray());
                    LeafDone = true;
                }
                else
                {
                    List<byte> hashDataArray = new List<byte>();
                    for (int i = 0; i < 16; i++)
                    {
                        if (val.Children[i] != null)
                        {
                            var ai = (TreeNode<LeafDataType>)val.Children[i];
                            hashDataArray.AddRange(ai.ID.Hex);

                            // DisplayUtils.Display("- " + i.ToString("X") + " : " + HexUtil.ToString(ai.ID.Hex) );
                        }
                    }
                    NodeHash = new Hash((new SHA512Managed()).ComputeHash(hashDataArray.ToArray()).Take(32).ToArray());
                }

                val.ID = NodeHash;
            }

            return Good;
        }

        public int TraverseNodes()
        {
            int nodes = 0;
            int depth = 0;
            TotalMoney = 0;
            TraverseTree(Root, ref nodes, depth);

            //TraverseLevelOrder(Root, ref nodes);
            return nodes;
        }

        private List<LeafDataType> TraverseTree(TreeNode<LeafDataType> Root, ref int FoundNodes, int _depth)
        {
            List<LeafDataType> foundNodes = new List<LeafDataType>();

            int depth = _depth + 1;

            for (int i = 0; i < 16; i++)
            {
                if (Root.Children[i] != null)
                {
                    /////////////////////////////////////
                    /*
                    if (Root.ID != null)
                    {
                        DisplayUtils.Display("ID : " + HexUtil.ToString(Root.ID.Hex) + " : " + depth);
                    }
                    else
                    {
                        DisplayUtils.Display("ID: NULL --------------- =============== ------------- :");
                    }*/

                    ////////////////////////////////

                    if (!Root.Children[i].IsLeaf)
                    {
                        TraverseTree(Root.Children[i], ref FoundNodes, depth);
                    }
                    if (Root.Children[i].IsLeaf)
                    {
                        TreeLeafNode<LeafDataType> Leaf = (TreeLeafNode<LeafDataType>)Root.Children[i];

                        foundNodes.Add(Leaf.Value);

                        TotalMoney += ((AccountInfo)Leaf.Value).Money;

                        //DisplayUtils.Display("Node Traversed: " + HexUtil.ToString(Leaf.ID.Hex) +" - "+ ((AccountInfo)Leaf.Value).Money);      
                        FoundNodes++;
                    }
                }
            }

            return foundNodes;
        }

        /// <summary>
        /// Gets the element at the position specified by the ID.
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public LeafDataType GetNodeData(Hash ID)
        {
            TreeNode<LeafDataType> TempRoot = Root;

            int HLEN = ID.Hex.Length << 1; // Multiply by 2

            for (int i = 0; i < HLEN; i++)
            {
                byte Nibble = ID.GetNibble(i);

                if ((i < (HLEN - 1)))
                {
                    if (TempRoot.Children[Nibble] == null) throw new Exception("Node does not exist. Midway Break");

                    TempRoot = TempRoot.Children[Nibble];
                }

                if (i == (HLEN - 1))
                {
                    if ((TempRoot.Children[Nibble] != null) && TempRoot.Children[Nibble].IsLeaf)
                    {
                        TreeLeafNode<LeafDataType> TLN = (TreeLeafNode<LeafDataType>)TempRoot.Children[Nibble];
                        return TLN.Value;
                    }
                    else
                    {
                        throw new Exception("Node does not exist.");
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
            TreeNode<LeafDataType> TempRoot = Root;

            int HLEN = ID.Hex.Length << 1; // Multiply by 2

            for (int i = 0; i < HLEN; i++)
            {
                byte Nibble = ID.GetNibble(i);

                if ((i < (HLEN - 1)))
                {
                    if (TempRoot.Children[Nibble] == null)
                    {
                        return false;
                    }

                    TempRoot = TempRoot.Children[Nibble];
                }

                if (i == (HLEN - 1))
                {
                    if ((TempRoot.Children[Nibble] != null) && TempRoot.Children[Nibble].IsLeaf)
                    {
                        return true;
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