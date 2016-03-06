
// @Author : Arpan Jati
// @Date: Dec 2014

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;

namespace TNetD.Tree
{
    public abstract class LeafDataType
    {
        public abstract Hash GetHash();
        public abstract Hash GetID();
    }
}
