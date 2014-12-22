using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;

namespace TNetD.Tree
{
    public abstract class LeafDataType : SerializableBase
    {
        public abstract Hash GetHash();
        public abstract Hash GetID();
    }
}
