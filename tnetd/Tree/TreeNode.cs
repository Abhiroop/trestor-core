/*
 *
    @Author: Arpan Jati
    @Version: 1.0
    @Description: TreeNode for a generic HashTree
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD;

namespace TNetD.Tree
{
    /// <summary>
    /// This is a node in the tree.
    /// There can be 16 child nodes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class TreeNode<T>
    {
        protected Hash _ID;
        protected bool _IsLeaf;

        public Hash ID
        {
            get
            {
                return _ID;
            }

            set
            {
                _ID = value;
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

        public TreeNode<T>[] Children;

        public TreeNode()
        {
            Children = new TreeNode<T>[16];
            _IsLeaf = false;
        }
    }

    /// <summary>
    /// This is the LeafNode, all the data is stored in these nodes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class TreeLeafNode<T> : TreeNode<T>
    {
        public T Value { get; set; }

        public TreeLeafNode(Hash ID, T Value)
            : base()
        {
            _ID = ID;
            this.Value = Value;
            _IsLeaf = true;
        }
    }

}





