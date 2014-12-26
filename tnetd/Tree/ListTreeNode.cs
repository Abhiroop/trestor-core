
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
        protected Hash hash;
        protected bool _IsLeaf;

        public Hash Hash
        {
            get
            {
                return hash;
            }

            set
            {
                hash = value;
            }
        }

        public bool IsLeaf
        {
            get
            {
                return _IsLeaf;
            }

            set
            {
                _IsLeaf = value;
            }
        }

        public ListTreeNode[] Children;

        public ListTreeNode()
        {
            Children = new ListTreeNode[16];
            _IsLeaf = false;
        }
    }

    

}


