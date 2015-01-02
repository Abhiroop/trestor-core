using Chaos.NaCl;
using Elliptic;
using TNetD;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using TNetD.Crypto;
using TNetD.Nodes;

namespace TNetD.Network.Networking
{
    public delegate void PacketReceivedHandler(Hash PublicKey, byte[] Data);

    class OutgoingConnection
    {
        NodeSocketData nodeSocketData = default(NodeSocketData);
        //bool Stopall = false;

        ConcurrentQueue<NetworkPacket> outgoingQueue = new ConcurrentQueue<NetworkPacket>();

        public event PacketReceivedHandler PacketReceived;

        public delegate void KeepAlivesHandler(uint Count);
        public event KeepAlivesHandler KeepAlives;
        //Timer timer;
        Random rnd = new Random();

        //string userIndex = "_USER_NAME_PLACEHOLDER_";

        Timer updateTimer;

        public OutgoingConnection(NodeSocketData nodeSocketData)
        {
            this.nodeSocketData = nodeSocketData;
            updateTimer = new Timer(TimerCallback, null, 0, Constants.Network_UpdateFrequencyMS);

            Connect();
        }

        public void EnqueuePacket(NetworkPacket npqe)
        {
            outgoingQueue.Enqueue(npqe);
        }

        private void TimerCallback(Object o)
        {
            try
            {
                if (KeyExchanged)
                {
                    while (outgoingQueue.Count > 0)
                    {
                        NetworkPacket np;
                        if (outgoingQueue.TryDequeue(out np))
                        {
                            SendData(np.Serialize());
                        }
                    }
                }
            }
            catch
            {
                //DisplayUtils.Display("TimerCallback", ex);
            }
        }

        UInt32 PacketCounter = 0;

        public byte[] AuthRandom = new byte[16];

        public byte[] DH_PublicKey;
        public byte[] DH_PrivateKey;

        byte[] ClientIdentifier = new byte[8];

        byte[] TransportKey = new byte[32];
        byte[] AuthenticationKey = new byte[32];

        public bool KeyExchanged = false;

        public bool ThreadKilled = false;

        TcpClient tcpClient = default(TcpClient);

        public void Connect()
        {
            try
            {
                byte[] DH_RandomBytes = new byte[32];

                Constants.rngCsp.GetBytes(DH_RandomBytes);
                Constants.rngCsp.GetBytes(AuthRandom);

                DH_PrivateKey = Curve25519.ClampPrivateKey(DH_RandomBytes);
                DH_PublicKey = Curve25519.GetPublicKey(DH_PrivateKey);

                Hash threadId = new Hash(Utils.GenerateUniqueGUID_Bytes(16));

                tcpClient = new TcpClient();

                if (tcpClient != null)
                    tcpClient.Close();

                tcpClient = new TcpClient();

                tcpClient.ReceiveBufferSize = Constants.PREFS_APP_TCP_BUFFER_SIZE;
                tcpClient.SendBufferSize = Constants.PREFS_APP_TCP_BUFFER_SIZE;

                tcpClient.ReceiveTimeout = 30000;
                tcpClient.SendTimeout = 30000;

                tcpClient.Connect(nodeSocketData.IP, nodeSocketData.ListenPort);


                Thread thr = new Thread(() => { ConnectAndProcess(tcpClient); });
                thr.Start();

            }
            catch (Exception ex)
            {
                if (Constants.NetworkVerbosity >= Verbosity.Errors)
                    DisplayUtils.Display(NodeSocketData.GetString(nodeSocketData) + " : ConnectToServer", ex);
            }
        }

        void ConnectAndProcess(TcpClient client)
        {
            bool ConnectionDone = false;

            try
            {
                NetworkStream nsRead = client.GetStream();
                NetworkStream nsWrite = client.GetStream();
                BinaryWriter writer = new BinaryWriter(client.GetStream());

                PacketSender.SendTransportPacket(writer, TransportPacketType.Initialize, new byte[0], ref PacketCounter);

                if ((client.Connected && (client != null)))
                {

                    MemoryStream messageStream = new MemoryStream();
                    byte[] inbuffer = new byte[Constants.PREFS_APP_TCP_BUFFER_SIZE];

                    List<TransportPacket> mp = new List<TransportPacket>();
                    while (tcpClient.Connected && (tcpClient != null))
                    {
                        bool packetGood = false;
                        if (nsRead.CanRead)
                        {
                            int bytesRead = 0; //int byteCounter = 0; 
                            do
                            {
                                bytesRead = 0;
                                try
                                {
                                    bytesRead = nsRead.Read(inbuffer, 0, inbuffer.Length);
                                    messageStream.Write(inbuffer, 0, bytesRead);

                                    if (bytesRead == 0)
                                    {
                                        break;
                                    }


                                    if (bytesRead > 0)
                                    {
                                        long SuccessPosition = 0;
                                        byte[] content = messageStream.ToArray();
                                        packetGood = PacketCodec.ValidateAndDecodeTransportPacket_Consume(nodeSocketData.Name, content, ref mp, ref SuccessPosition);

                                        if (packetGood)
                                        {
                                            foreach (TransportPacket p in mp)
                                            {
                                                try
                                                {
                                                    ProcessOutgoingConnectionInternal(tcpClient, p);
                                                }
                                                catch (Exception ex)
                                                {
                                                    DisplayUtils.Display(NodeSocketData.GetString(nodeSocketData) + " : --- ProcessConnection:CNT:" + mp.Count, ex);
                                                }
                                            }
                                        }

                                        byte[] remaining = new byte[content.Length - SuccessPosition];

                                        Array.Copy(content, (int)SuccessPosition, remaining, 0, remaining.Length);

                                        messageStream.Position = 0;
                                        messageStream = new MemoryStream();
                                        messageStream.Write(remaining, 0, remaining.Length);
                                    }

                                }
                                catch
                                {
                                    //DisplayUtils.Display("ClientHandler : Read()", ex);
                                }
                            }
                            while (nsRead.DataAvailable);

                        }

                        Thread.Sleep(75);
                    }
                    writer.Close();
                    //reader.Close();


                    //MemoryStream messageStream = new MemoryStream();
                    //byte[] inbuffer = new byte[ClientUtils.PREFS_APP_TCP_BUFFER_SIZE];

                    //List<TransportPacket> mp = new List<TransportPacket>();
                    //while (client.Connected && (client != null))
                    //{
                    //    bool packetGood = false;
                    //    if (nsRead.CanRead)
                    //    {
                    //        do
                    //        {
                    //            try
                    //            {
                    //                int bytesRead = nsRead.Read(inbuffer, 0, inbuffer.Length);

                    //                //if (bytesRead == 0) // Connection lost
                    //                //{
                    //                //    DisplayUtils.Display("--- Connection lost");
                    //                //}

                    //                messageStream.Write(inbuffer, 0, bytesRead);
                    //                if (bytesRead > 0)
                    //                {
                    //                    if (!nsRead.DataAvailable)
                    //                    {
                    //                        packetGood = PacketCodec.ValidateAndDecodeTransportPacket(messageStream.ToArray(), ref mp);
                    //                    }
                    //                }
                    //            }
                    //            catch (Exception ex)
                    //            {
                    //                //DisplayUtils.Display("ClientHandler : Read()", ex);
                    //            }
                    //        }
                    //        while (nsRead.DataAvailable);
                    //    }

                    //    if (packetGood)
                    //    {
                    //        messageStream.Position = 0;
                    //        messageStream = new MemoryStream();

                    //        foreach (TransportPacket p in mp)
                    //        {
                    //            try
                    //            {
                    //                ProcessOutgoingConnectionInternal(client, p);
                    //            }
                    //            catch (Exception ex)
                    //            {
                    //                if (tc.verbosity >= Verbosity.Errors)
                    //                DisplayUtils.Display("User: " + userIndex + ", --- ProcessOutgoing:CNT:" + mp.Count, ex);
                    //            }
                    //        }
                    //    }
                    //    Thread.Sleep(75);
                    //}
                    //writer.Close();
                    ////reader.Close();
                }
            }
            catch (System.Exception ex)
            {
                if (ConnectionDone == false)
                {
                    if (Constants.NetworkVerbosity >= Verbosity.Errors)
                        DisplayUtils.Display(NodeSocketData.GetString(nodeSocketData) + " : --- Process Outgoing, Not Connect", ex);

                }
                else
                {

                }
            }
            finally
            {
                try
                {
                    client = null;
                }
                catch { }
            }

            if (Constants.NetworkVerbosity >= Verbosity.Warning)
                DisplayUtils.Display(NodeSocketData.GetString(nodeSocketData) + " : Disconnected", DisplayType.Warning);

            ThreadKilled = true;
        }

        string MakeIntString(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sb.Append((int)data[i] + ",");
            }
            return sb.ToString();
        }

        void SendData(byte[] Data)
        {
            try
            {
                byte[] nonce = new byte[8];
                Constants.rngCsp.GetBytes(nonce);

                byte[] counter_sender_data = Utils.GetLengthAsBytes((int)PacketCounter).Concat(Data).ToArray();

                byte[] CryptedCounterSenderData = Salsa20.ProcessSalsa20(counter_sender_data, TransportKey, nonce, 0);

                byte[] HMAC = (new HMACSHA256(AuthenticationKey)).ComputeHash(CryptedCounterSenderData).Take(16).ToArray();

                byte[] NONCE_MAC_DATA = nonce.Concat(HMAC).Concat(CryptedCounterSenderData).ToArray();

                BinaryWriter writer = new BinaryWriter(tcpClient.GetStream());

                PacketSender.SendTransportPacket(writer, TransportPacketType.DataCrypted, NONCE_MAC_DATA, ref PacketCounter);

                writer.Flush();

            }
            catch { }
        }

        private void ProcessOutgoingConnectionInternal(TcpClient client, TransportPacket p)
        {
            BinaryReader reader = new BinaryReader(client.GetStream());
            BinaryWriter writer = new BinaryWriter(client.GetStream());

            switch (p.Type) // Decode received messages from Server.
            {

                case TransportPacketType.WorkProofRequest:

                    // INPUT : 24 Bytes of (random + timestamp)
                    // OUTPUT : 4 bytes Proof, 32 Bytes of KeypairPublic, 16 Bytes Random => 52 bytes
                    if (p.Data.Length == 24)
                    {
                        DisplayUtils.Display(NodeSocketData.GetString(nodeSocketData) + " : WorkProofRequest Received : " + p.Data.Length, DisplayType.Info);

                        byte[] Proof = WorkProof.CalculateProof(p.Data, Constants.Difficulty);

                        byte[] rv = new byte[Proof.Length + DH_PublicKey.Length + AuthRandom.Length];
                        Buffer.BlockCopy(Proof, 0, rv, 0, Proof.Length);
                        Buffer.BlockCopy(DH_PublicKey, 0, rv, Proof.Length, DH_PublicKey.Length);
                        Buffer.BlockCopy(AuthRandom, 0, rv, Proof.Length + DH_PublicKey.Length, AuthRandom.Length);

                        PacketSender.SendTransportPacket(writer, TransportPacketType.WorkProofKeyResponse, rv, ref PacketCounter);
                    }

                    break;

                case TransportPacketType.ServerPublicTransfer:

                    // SERVER_PUBLIC[32] || SIGN_MAC[32] || Sign_Crypted[64] => 128 bytes
                    if (p.Data.Length == 128)
                    {
                        byte[] serverPublic = new byte[32];
                        byte[] signMAC = new byte[32];
                        byte[] signCrypted = new byte[64];

                        Array.Copy(p.Data, 0, serverPublic, 0, 32);
                        Array.Copy(p.Data, 32, signMAC, 0, 32);
                        Array.Copy(p.Data, 64, signCrypted, 0, 64);

                        byte[] sharedSecret = (new SHA512Managed()).ComputeHash(Curve25519.GetSharedSecret(DH_PrivateKey, serverPublic));

                        Array.Copy(sharedSecret, TransportKey, 32);
                        Array.Copy(sharedSecret, 32, AuthenticationKey, 0, 32);

                        byte[] SIGN_ID_MAC_EXPECTED = (new HMACSHA256(AuthenticationKey)).ComputeHash(signCrypted);

                        if (CryptoBytes.ConstantTimeEquals(SIGN_ID_MAC_EXPECTED, signMAC))
                        {
                            byte[] Sign_Plain = Salsa20.ProcessSalsa20(signCrypted, TransportKey, new byte[8], 0);

                            // The remote node should be able to identify itself properly. 
                            bool serverVerified = Ed25519.Verify(Sign_Plain, AuthRandom, nodeSocketData.PublicKey.Hex);

                            if (serverVerified)
                            {
                                if (Constants.NetworkVerbosity >= Verbosity.Info)
                                {
                                    DisplayUtils.Display(NodeSocketData.GetString(nodeSocketData) + " : Key Exchanged: Shared KEY [TransportKey] : {" +
                                        (HexUtil.ToString(TransportKey)) + "}", DisplayType.Info);

                                    DisplayUtils.Display(NodeSocketData.GetString(nodeSocketData) + " : Server Authenticated", DisplayType.Info);
                                }

                                PacketSender.SendTransportPacket(writer, TransportPacketType.KeyExComplete, new byte[0], ref PacketCounter);

                                KeyExchanged = true;

                            }
                            else
                            {
                                if (Constants.NetworkVerbosity >= Verbosity.Errors)
                                    DisplayUtils.Display(NodeSocketData.GetString(nodeSocketData) + " : Remote Authentication Failed : Remote Node Could not authenticate itself", DisplayType.AuthFailure);
                            }

                        }
                        else
                        {
                            if (Constants.NetworkVerbosity >= Verbosity.Errors)
                                DisplayUtils.Display(NodeSocketData.GetString(nodeSocketData) + " : Server Authentication Failed : Server Could not authenticate itself : Invalid MAC ", DisplayType.AuthFailure);
                        }
                    }

                    break;

                case TransportPacketType.KeepAlive:

                    if (p.Data.Length == 4)
                    {
                        uint pCounter = BitConverter.ToUInt32(p.Data, 0);
                        if (KeepAlives != null) KeepAlives(pCounter);
                    }

                    break;

                case TransportPacketType.DataCrypted:

                    if (KeyExchanged)
                    {
                        if (p.Data.Length >= 28)
                        {
                            byte[] nonce = new byte[8];
                            byte[] hmac = new byte[16];
                            byte[] crypted_data = new byte[p.Data.Length - 24];

                            Array.Copy(p.Data, 0, nonce, 0, 8);
                            Array.Copy(p.Data, 8, hmac, 0, 16);
                            Array.Copy(p.Data, 24, crypted_data, 0, crypted_data.Length);

                            byte[] MAC_EXPECTED = (new HMACSHA256(AuthenticationKey)).ComputeHash(crypted_data).Take(16).ToArray();

                            if (CryptoBytes.ConstantTimeEquals(MAC_EXPECTED, hmac))
                            {
                                byte[] DeCryptedData = new byte[p.Data.Length - 24];

                                DeCryptedData = Salsa20.ProcessSalsa20(crypted_data, TransportKey, nonce, 0);

                                byte[] counter = new byte[4];
                                byte[] rec_data = new byte[p.Data.Length - 28];

                                Array.Copy(DeCryptedData, 0, counter, 0, 4);
                                Array.Copy(DeCryptedData, 4, rec_data, 0, rec_data.Length);

                                if (PacketReceived != null)
                                    PacketReceived(nodeSocketData.PublicKey, rec_data);

                            }
                            else
                            {
                                if (Constants.NetworkVerbosity >= Verbosity.Errors)
                                    DisplayUtils.Display(NodeSocketData.GetString(nodeSocketData) + " : HMAC FAILED : ");
                            }
                        }
                    }

                    break;

                default:

                    if (Constants.NetworkVerbosity >= Verbosity.Errors)
                        DisplayUtils.Display(NodeSocketData.GetString(nodeSocketData) + " : Unknown Packet received : " + p.Type.ToString());

                    break;
            }
        }

    }
}
