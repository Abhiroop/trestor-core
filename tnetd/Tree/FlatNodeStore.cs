using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Tree
{
    /// <summary>
    /// Temporary class to store Account Information 
    /// </summary>
    class FlatNodeStore
    {
        SortedDictionary<Hash, LeafDataType> Values = new SortedDictionary<Hash,LeafDataType>();

        public LeafDataType this[Hash hash]
        {
            get
            {
                return Values[hash];
            }
        }

        public void Add(LeafDataType ldt)
        {
            Values.Add(ldt.GetID(), ldt);
        }

        public void Remove(Hash hash)
        {
            Values.Remove(hash);
        }


    }
}
