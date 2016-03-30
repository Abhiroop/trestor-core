
// @Author : Arpan Jati
// @Date: Dec 2014

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TNetD;

namespace TNetD.Tree
{
    /// <summary>
    /// This is a node in the tree.
    /// There can be 16 child nodes
    /// </summary>
    /// <typeparam name="LeafDataType"></typeparam>
    public class ListTreeNode
    {
        public Hash addressNibbles;
        protected long leafCount;
        protected Hash hash = new Hash();
        protected bool isLeaf;

        public long LeafCount
        {
            get
            {
                return leafCount;
            }

            /*set
            {
                leafCount = value;
            }*/
        }

        public void SetLeafCount(long leafCount)
        {
            this.leafCount = leafCount;
        }

        /*void updateLeafCount()
        {
            leafCount = 0;
            for (int i = 0; i < 16; i++)
            {
                if (Children[i] != null)
                {
                    leafCount += Children[i].LeafCount;
                }
            }
        }*/

        public Hash Hash
        {
            get
            {
                return hash;
            }

            /* set
             {
                 hash = value;
             }*/
        }

        public bool IsLeaf
        {
            get
            {
                return isLeaf;
            }

            /*set
            {
                isLeaf = value;
            }*/
        }

        public ListTreeNode[] Children;

        /// <summary>
        /// Get the number of child nodes for the current node.
        /// </summary>
        /// <returns></returns>
        public int ChildCount()
        {
            if (Children != null)
            {
                int count = 0;
                for (int i = 0; i < 16; i++)
                {
                    if (Children[i] != null)
                        count++;
                }
                return count;
            }
            return 0;
        }

        /// <summary>
        /// Sets a new hash for the node, typically as the result of a change in the tree.
        /// </summary>
        /// <param name="hash"></param>
        public void SetHash(Hash hash)
        {
            this.hash = hash;
        }

        public ListTreeNode()
        {
            Children = new ListTreeNode[16];
            isLeaf = false;
            leafCount = 0;
        }
    }



}


