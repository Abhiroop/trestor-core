using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Threading;
using System.Threading;
using TNetCommon.Protocol;

namespace TNetWallet.WalletOperations
{
    class Network
    {
        public delegate void PacketReceivedHandler(string Type, byte[] Data);
        public event PacketReceivedHandler PacketReceived;

        public delegate void ConnectionErrorHandler();
        public event ConnectionErrorHandler ConnectionError;

        TcpClient tc;
        //DispatcherTimer dt;
        StreamReader sr;
        StreamWriter sw;

        /// <summary>
        /// Create a connectioon to the server and connect.
        /// </summary>
        /// <param name="IP"></param>
        public Network(string IP)
        {
            try
            {
                tc = new TcpClient();

                tc.ReceiveBufferSize = 64;
                tc.SendBufferSize = 64;

                tc.Connect(IP, 5050);

                //dt = new DispatcherTimer();
                //dt.Interval = new TimeSpan(0, 0, 0, 0, 100);
                sr = new StreamReader(tc.GetStream());
                sw = new StreamWriter(tc.GetStream());

                ParameterizedThreadStart pts = new ParameterizedThreadStart(NetworkHandler);

                Thread thr = new Thread(pts);
                thr.Start(tc);

            }
            catch
            {
                if (ConnectionError != null)
                    ConnectionError();
            }
        }

        void NetworkHandler(object data)
        {
            while (tc.Connected)
            {
                try
                {
                    string line = sr.ReadLine();

                    if (line == null)
                    {
                        if (ConnectionError != null)
                            ConnectionError();

                        break;
                    }

                    string[] parts = line.Split('|');

                    if (parts.Length == 2)
                    {
                        String FLAG = parts[0];
                        String DATA = parts[1];

                        byte[] DECODED = Convert.FromBase64String(DATA);

                        if (PacketReceived != null)
                            PacketReceived(FLAG, DECODED);
                    }
                }
                catch
                {
                    if (ConnectionError != null)
                        ConnectionError();

                    break;
                }
            }
        }

        /*/// <summary>
        /// Send Packet
        /// </summary>
        /// <param name="Packet"></param>
        public void SendPacket(string Packet)
        {
            try
            {
                NetworkStream w = tc.GetStream();
                StreamWriter sw = new StreamWriter(w);
                sw.WriteLine(Packet);
                sw.Flush();
            }
            catch
            {
                if (ConnectionError != null)
                    ConnectionError();
            }
        }*/

        /// <summary>
        /// COMMAND FORMAT: TYPE : {0 BYTE[] PublicKey}, {1 string Command}, {2 byte[] CommandData},  
        /// </summary>
        /// <param name="PK"></param>
        /// <param name="Command"></param>
        /// <param name="CommandData"></param>
        public void SendCommand(byte[] PK, string Command, byte [] CommandData)
        {
            try
            {
                NetworkStream w = tc.GetStream();
                StreamWriter sw = new StreamWriter(w);

                List<ProtocolDataType> packets = new List<ProtocolDataType>();

                packets.Add(ProtocolPackager.Pack(PK,0));
                packets.Add(ProtocolPackager.Pack(Command, 1));
                packets.Add(ProtocolPackager.Pack(CommandData, 2));

                byte [] pack = ProtocolPackager.PackRaw(packets);

                sw.WriteLine(Convert.ToBase64String(pack));

                /*sw.WriteLine("COMMAND|" + Convert.ToBase64String(PK) + "|" +
                    Convert.ToBase64String(Encoding.GetEncoding("ISO8859-1").GetBytes(Command)) + "|" +
                    Convert.ToBase64String(Encoding.GetEncoding("ISO8859-1").GetBytes(CommandData)));*/

                sw.Flush();
            }
            catch
            {
                if (ConnectionError != null)
                    ConnectionError();
            }
        }
    }
}
