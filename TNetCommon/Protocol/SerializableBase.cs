using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Protocol
{
    public interface ISerializableBase
    {
        byte[] Serialize();
        void Deserialize(byte[] Data);
    }
}
