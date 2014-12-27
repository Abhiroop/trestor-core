using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Tree
{
    /// <summary>
    /// This is the LeafNode, all the data is stored in these nodes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class ListTreeLeafNode : ListTreeNode
    {
        SortedDictionary<Hash, LeafDataType> Values = new SortedDictionary<Hash, LeafDataType>();

        public ListTreeLeafNode(Dictionary<Hash, LeafDataType> NewValues)
            : base()
        {
            foreach (KeyValuePair<Hash, LeafDataType> val in NewValues)
            {
                this.Values.Add(val.Key, val.Value);
            }

            isLeaf = true;
        }

        public LeafDataType this[Hash hash]
        {
            get
            {
                if (Values.ContainsKey(hash)) return Values[hash];
                else throw new Exception("Key does not exist..."); // Think of better ways to do this...
            }
        }

        public bool ContainsElement(Hash ID)
        {
            bool CE = Values.ContainsKey(ID);

            /*if (!CE)
            {
                CE = Values.ContainsKey(ID);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine(" ----------- Delete Error\n To Delete:\n" + HexUtil.ToString(ID.Hex) + "\n---------------\n");
                foreach (Hash hd in Values.Keys)
                {
                    sb.AppendLine("" + HexUtil.ToString(hd.Hex) + "  --  ");

                    for(int i=0;i<32;i++)
                    {
                        sb.Append("" + (hd.Hex[i] ^ ID.Hex[i]).ToString("X2") + "");
                    }
                    sb.AppendLine();

                }
                sb.AppendLine("---------------");

                DisplayUtils.Display(sb.ToString());
            }*/

            return CE;
        }

        /// <summary>
        /// Gets the numebr of Elements in the leaf.
        /// </summary>
        public int Count
        {
            get { return Values.Count; }
        }

        /// <summary>
        /// Deletes the element if it exists in the Dictionary.
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool DeleteElement(Hash ID)
        {
            return Values.Remove(ID);       
        }

        public LeafDataType[] GetAllItems()
        {
            LeafDataType[] listItems = new LeafDataType[Values.Count];
            int elems = 0;
            foreach (KeyValuePair<Hash, LeafDataType> val in Values)
            {
                listItems[elems++] = val.Value;
            }

            if (elems != Values.Count) new InvalidOperationException("Addition or removal during fetch, thread-unsafe-operation");

            return listItems;
        }

        /// <summary>
        /// Adds an entry to the tree
        /// </summary>
        /// <param name="hash">Hash is the Full hash for the value (32 bytes)</param>
        /// <param name="value"></param>
        public void Add(Hash ID, LeafDataType value)
        {
            Values.Add(ID, value);
        }

        /// <summary>
        /// Gets the hash of all the nodes in the list, turns out to be a bit expensive, 
        /// because of the amount of hashing.
        /// </summary>
        /// <returns>SHA-512 hash of the node (all the leaf-data entries combined).</returns>
        public Hash GetHash()
        {
            List<byte> _tempHash = new List<byte>();

            foreach (KeyValuePair<Hash, LeafDataType> val in Values)
            {
                _tempHash.AddRange(val.Value.GetHash().Hex);
            }

            return new Hash((new SHA512Managed()).ComputeHash(_tempHash.ToArray()).Take(32).ToArray());
        }


    }
}
