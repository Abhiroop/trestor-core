
// @Author : Arpan Jati
// @Date: 25th Dec 2014

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Network
{
    public class NetworkPacket
    {
        // Network layer security must make sure that this value is correct.
        // Must check signature.
        public Hash PublicKey_Src;
        public byte Type;
        public byte[] Data;
        public Hash Token;
        public NetworkPacket(Hash publicKey_Src, byte type, byte[] Data, Hash token)
        {
            PublicKey_Src = publicKey_Src;
            Type = type;
            this.Data = Data;
            Token = token;
        }
    };
}
