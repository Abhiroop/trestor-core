using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Protocol
{
    abstract class SerializableBase
    {
        public abstract byte[] Serialize();
        public abstract void Deserialize(byte[] Data);
    }
}
