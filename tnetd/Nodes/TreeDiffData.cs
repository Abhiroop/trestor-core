using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Nodes
{
    class TreeDiffData
    {
        public Hash PublicKey { get; set; }
        public long RemoveValue { get; set; }
        public long AddValue { get; set; }
    }
}
