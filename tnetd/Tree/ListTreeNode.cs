
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
    class ListTreeNode
    {
        protected long leafCount;
        protected Hash hash;
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


