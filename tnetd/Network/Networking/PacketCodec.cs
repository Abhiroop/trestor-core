//
// File: PacketCodec.cs 
// Version: 1.0 
// Date: Jan 7-9, 2013
// Author : Arpan Jati

using TNetD;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TNetD.Network.Networking
{
    public class PacketCodec
    {
        public static byte[] CreateTransportPacket(TransportPacket packet)
        {
            // [TNW]:[TYPE]:[VERSION]:[DATA_SIZE]:[DATA]:[EOP] : MIN SIZE 12 Bytes
            // TNW      : 'T', 'N', 'W' : 3 BYTES            
            // TYPE     : 1 BYTE
            // VERSION  : 1 BYTE
            // DATA_SZ  : 4 BYTES [Little Endian Byte Order]
            // DATA     : DATA_SZ BYTES
            // EOP      : 'E', 'O', 'P' : 3 BYTES

            MemoryStream ms = new MemoryStream();
   
            ms.WriteByte((byte)'T');
            ms.WriteByte((byte)'N');
            ms.WriteByte((byte)'W');
            ms.WriteByte((byte)packet.Type);
            ms.WriteByte((byte)packet.Version);
            ms.Write(Utils.GetLengthAsBytes(packet.Data.Length), 0, 4);
            ms.Write(packet.Data, 0, packet.Data.Length);
            ms.WriteByte((byte)'E');
            ms.WriteByte((byte)'O');
            ms.WriteByte((byte)'P');           

            return ms.ToArray();
        }

        public static bool ValidateAndDecodeTransportPacket_Consume(string userIndex, byte[] data, ref List<TransportPacket> packets, ref long SuccessPosition)
        {
            MemoryStream ms = new MemoryStream(data);
            return ValidateAndDecodeTransportPacket_Consume(userIndex, ms, ref packets, ref SuccessPosition);
        }

        public static bool ValidateAndDecodeTransportPacket_Consume(string userIndex, MemoryStream ms, ref List<TransportPacket> packets, ref long SuccessPosition)
        {            
            packets = new List<TransportPacket>();

            SuccessPosition = 0;

            byte[] HEADER = new byte[3];
            byte TYPE = 255;
            byte VERSION = 255;
            byte[] LENGTH = new byte[4];
            byte[] TRAILER = new byte[3];

            while (ms.Position <= ms.Length - 12)
            {
                TransportPacket pack;    

                ms.Read(HEADER, 0, 3);

                if ((HEADER[0] == 'T') && (HEADER[1] == 'N') && (HEADER[2] == 'W'))
                {
                    TYPE = (byte)ms.ReadByte();
                    VERSION = (byte)ms.ReadByte();
                    ms.Read(LENGTH, 0, 4);

                    long DATA_SZ = Utils.GetLengthFromBytes(LENGTH);

                    if (ms.Length - ms.Position >= DATA_SZ + 3)
                    {
                        byte[] RX_DATA = new byte[DATA_SZ];

                        ms.Read(RX_DATA, 0, (int)DATA_SZ);

                        ms.Read(TRAILER, 0, 3);

                        if (!((TRAILER[0] == 'E') && (TRAILER[1] == 'O') && (TRAILER[2] == 'P')))
                        {
                            DisplayUtils.Display("User: " + userIndex + ", ################  CORRUPT TRAILER: 'NO EOP': " + TRAILER[0] + '-' + TRAILER[1] +
                            '-' + TRAILER[2] + '-', DisplayType.Exception);      
                            return false;
                        }

                        SuccessPosition = ms.Position;

                        pack.Type = (TransportPacketType)TYPE;
                        pack.Version = VERSION;
                        pack.Data = RX_DATA;

                        packets.Add(pack);
                    }
                    else
                    {
                       // DisplayUtils.Display("User: " + userIndex + ", INCOMPLETE PACKET: (" + DATA_SZ + " + 3 ) / " + (ms.Length - ms.Position), DisplayType.Exception);
                        return false;
                    }
                }
                else
                {
                    DisplayUtils.Display("User: " + userIndex + ", CORRUPT HEADER: 'NO TNW': " + HEADER[0] + '-' + HEADER[1] +
                        '-' +HEADER[2] + '-', DisplayType.Exception);

                    return false;
                }
            }

            return true;
        }

    }

   
}
