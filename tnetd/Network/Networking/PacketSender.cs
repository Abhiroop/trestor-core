
// @Author : Arpan Jati
// @Date: Jan 2015

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Network.Networking
{
    public class PacketSender
    {
        public static async Task SendTransportPacket(NetworkStream sw, TransportPacketType type, byte[] Data)
        {
            TransportPacket tp = new TransportPacket(Data, type, Constants.TransportVersion);
            byte[] msg = PacketCodec.CreateTransportPacket(tp);
            sw.Write(msg, 0, msg.Length);
            await sw.FlushAsync();
        }
    }
}
