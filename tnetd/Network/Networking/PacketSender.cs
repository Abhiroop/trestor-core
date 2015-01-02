using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TNetD.Network.Networking
{
    public class PacketSender
    {

        public static void SendTransportPacket(BinaryWriter sw, TransportPacketType type, byte[] Data, ref UInt32 counter)
        {
            TransportPacket tp = new TransportPacket(Data, type, Constants.TransportVersion);
            byte[] msg = PacketCodec.CreateTransportPacket(tp);
            sw.Write(msg, 0, msg.Length);
            sw.Flush();
            counter++;
        }


    }
}
