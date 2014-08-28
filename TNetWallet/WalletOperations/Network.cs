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
using System.Windows;

namespace TNetWallet.WalletOperations
{
    public class Network
    {
        string _IP = "127.0.0.1";

        public delegate void PacketReceivedHandler(string Type, byte[] Data);
        public event PacketReceivedHandler PacketReceived;

        public delegate void ConnectionErrorHandler();
        public event ConnectionErrorHandler ConnectionError;

        public bool NetworkAlive = false;

        public bool NetworkOK = false;

        TcpClient Client;

        StreamReader Reader;
        StreamWriter Writer;

        public Thread ManagementThread = default(Thread);

        /// <summary>
        /// Create a connection to the server and connect.
        /// </summary>
        /// <param name="IP"></param>
        public Network(string IP)
        {
            _IP = IP;

            ParameterizedThreadStart pts = new ParameterizedThreadStart(NetworkHandler);
            ManagementThread = new Thread(pts);
            ManagementThread.Start();
        }

        void IssueConnectionError()
        {
            NetworkOK = false;

            if (ConnectionError != null)
                ConnectionError();
        }

        void NetworkHandler(object data)
        {
            NetworkAlive = true;

            while (NetworkAlive)
            {
                try
                {
                    Client = new TcpClient();

                    Client.ReceiveBufferSize = 64;
                    Client.SendBufferSize = 64;

                    Client.Connect(_IP, 5050);

                    Reader = new StreamReader(Client.GetStream());
                    Writer = new StreamWriter(Client.GetStream());

                    try
                    {
                        Application.Current.Dispatcher.Invoke(new Action(delegate
                        {
                            ((MainWindow)App.Current.MainWindow).SetNetworkStatus(true);
                        }));
                    }
                    catch {}   

                    NetworkOK = true;

                    while (Client.Connected)
                    {
                        try
                        {
                            string line = Reader.ReadLine();

                            if (line == null)
                            {
                                // NETWORK DISCONNECTED BY REMOTE HOST
                                IssueConnectionError();
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
                            IssueConnectionError();
                            break;
                        }
                    }

                }
                catch
                {
                    IssueConnectionError();
                }

                Thread.Sleep(Constants.ReconnectInterval);
            }

        }

        /// <summary>
        /// COMMAND FORMAT: TYPE : {0 BYTE[] PublicKey}, {1 string Command}, {2 byte[] CommandData},  
        /// </summary>
        /// <param name="PK"></param>
        /// <param name="Command"></param>
        /// <param name="CommandData"></param>
        public void SendCommand(byte[] PK, string Command, byte[] CommandData)
        {
            try
            {
                List<ProtocolDataType> packets = new List<ProtocolDataType>();

                packets.Add(ProtocolPackager.Pack(PK, 0));
                packets.Add(ProtocolPackager.Pack(Command, 1));
                packets.Add(ProtocolPackager.Pack(CommandData, 2));

                byte[] pack = ProtocolPackager.PackRaw(packets);

                Writer.WriteLine(Convert.ToBase64String(pack));

                Writer.Flush();
            }
            catch
            {
                IssueConnectionError();
            }
        }



    }
}
