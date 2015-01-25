using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TNetD.Nodes
{
    class TreeDiffData
    {
        public TreeDiffData()
        {

        }
        
        public TreeDiffData(long AddValue, long RemoveValue)
        {
            this.AddValue = AddValue;
            this.RemoveValue = RemoveValue;
        }

        public Hash PublicKey { get; set; }
        public long RemoveValue { get; set; }
        public long AddValue { get; set; }
    }
}
