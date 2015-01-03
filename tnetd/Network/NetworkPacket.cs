
// @Author : Arpan Jati
// @Date: 25th Dec 2014 | Dec 29, 2014

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Network.Networking;
using TNetD.Protocol;

namespace TNetD.Network
{
    public class NetworkPacket : ISerializableBase
    {
        // Network layer security must make sure that this value is correct.
        // Must check signature.
        public Hash PublicKey_Src;
        public PacketType Type;
        public byte[] Data;
        public Hash Token;

        public NetworkPacket()
        {
            PublicKey_Src = new Hash();
            Type = PacketType.TPT_NOTHING;
            Data = new byte[0];
            Token = new Hash();
        }

        public NetworkPacket(Hash publicKey_Src, PacketType type, byte[] Data, Hash token)
        {
            PublicKey_Src = publicKey_Src;
            Type = type;
            this.Data = Data;
            Token = token;
        }

        public NetworkPacket(Hash publicKey_Src, PacketType type, byte[] Data)
        {
            PublicKey_Src = publicKey_Src;
            Type = type;
            this.Data = Data;
            Token = new Hash();
        }
        
        public byte[] Serialize()
        {
            List<ProtocolDataType> PDTs = new List<ProtocolDataType>();
            PDTs.Add(ProtocolPackager.Pack(PublicKey_Src, 0));
            PDTs.Add(ProtocolPackager.Pack((byte)Type, 1));
            PDTs.Add(ProtocolPackager.Pack(Data, 2));
            PDTs.Add(ProtocolPackager.Pack(Token, 3));
            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] Data)
        {
            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(Data);
            int cnt = 0;

            while (cnt < (int)PDTs.Count)
            {
                ProtocolDataType PDT = PDTs[cnt++];

                switch (PDT.NameType)
                {
                    case 0:
                        ProtocolPackager.UnpackHash(PDT, 0, out PublicKey_Src);
                        break;

                    case 1:
                        byte _type = (byte) PacketType.TPT_NOTHING;
                        ProtocolPackager.UnpackByte(PDT, 1, ref _type);
                        Type = (PacketType)_type;
                        break;

                    case 2:
                        ProtocolPackager.UnpackByteVector(PDT, 2, ref Data);
                        break;

                    case 3:
                        ProtocolPackager.UnpackHash(PDT, 3, out Token);
                        break;
                }
            }
        }
    };
}
