
// File: IncomingConnectionHander.cs 
// Version: 1.0 
// Date: Jan 7, 2013 | Dec 29, 2014
// Author : Arpan Jati

using Chaos.NaCl;
using Elliptic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using TNetD.Nodes;
using TNetD.Crypto;
using System.Reactive.Linq;

namespace TNetD.Network.Networking
{
    class IncomingConnectionHander
    {
        public event PacketReceivedHandler PacketReceived;

        NodeConfig nodeConfig;

        TcpListener listener;
        static Timer timer;
        static Timer timer_hello;

        List<Hash> KeysGlobal = new List<Hash>();

        public IncomingConnectionHander(int ListenPort, NodeConfig nodeConfig)
        {
            this.nodeConfig = nodeConfig;

            listener = new TcpListener(IPAddress.Any, ListenPort);
            timer = new Timer(TimerCallback_Housekeeping, null, 0, 2000);
            /* timer_hello = new Timer(TimerCallback_Hello, null, 0, 500);*/

            Observable.Interval(TimeSpan.FromMilliseconds(100))
                .Subscribe(async x => await TimerCallback_Hello(x));

            ServicePointManager.DefaultConnectionLimit = 200000;

            // Start a new thread to accept new incoming connections.
            StartListening();
        }

        Queue<NetworkPacketQueueEntry> outgoingQueue = new Queue<NetworkPacketQueueEntry>();

        /// <summary>
        /// Outgoing connections / Keyed on PublicKey
        /// </summary>
        public Dictionary<Hash, IncomingClient> IncomingConnections = new Dictionary<Hash, IncomingClient>();

        /// <summary>
        /// Incoiming Connections Keyed on Identifier.
        /// </summary>
        public Dictionary<Hash, IncomingClient> ThreadList = new Dictionary<Hash, IncomingClient>();

        /// <summary>
        /// Check whether the given PK is already connected.
        /// </summary>
        /// <param name="PublicKey"></param>
        /// <returns></returns>
        public bool IsConnected(Hash PublicKey)
        {
            if (IncomingConnections.ContainsKey(PublicKey))
            {
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Adds the current packet to the outgoing queue for processing.
        /// If the processing fails, the packets are dropped. 
        /// TODO: Try to open a new connection and try again in future.
        /// </summary>
        /// <param name="npqe"></param>
        /// <returns></returns>
        public bool EnqueuePacket(NetworkPacketQueueEntry npqe)
        {
            if (IncomingConnections.ContainsKey(npqe.PublicKey_Dest))
            {
                outgoingQueue.Enqueue(npqe);
                return true;
            }
            else return false;
        }

        private async Task TimerCallback_Hello(long o)
        {
            while (outgoingQueue.Count > 0)
            {
                NetworkPacketQueueEntry npqe = outgoingQueue.Dequeue();

                if (IncomingConnections.ContainsKey(npqe.PublicKey_Dest))
                {
                    await SendData(npqe.Packet.Serialize(), IncomingConnections[npqe.PublicKey_Dest]);
                }
            }


            /*

            long Authed = 0;

            KeysGlobal = new List<Hash>();

            try
            {
                foreach (KeyValuePair<Hash, IncomingClient> kvp in ThreadList)
                {
                    try
                    {
                        if (kvp.Value.thread.IsAlive)
                        {
                            if (kvp.Value.client != null)
                            {
                                if (kvp.Value.client.Connected)
                                {
                                    if (kvp.Value.KeyExchanged)
                                    {
                                        Authed++;
                                        KeysGlobal.Add(kvp.Key);
                                    }

                                    //BinaryWriter writer = new BinaryWriter(kvp.Value.client.GetStream());
                                    //PacketSender.SendTransportPacket(writer, TransportPacketType.KeepAlive, BitConverter.GetBytes(kvp.Value.PacketCounter), ref kvp.Value.PacketCounter);
                                }
                            }
                        }

                    }
                    catch { }
                }
            }
            catch { }

            Constants.SERVER_GLOBAL_CONNS = ThreadList.Count;
            Constants.SERVER_GLOBAL_AUTH_USERS = Authed;
                        
            try
            {
                foreach (KeyValuePair<Hash, IncomingClient> kvp in ThreadList)
                {
                    kvp.Value
                }
            }
            catch { }*/
        }

        private void TimerCallback_Housekeeping(Object o)
        {
            /// Remove Threads

            // Remove bad connections
            try
            {
                List<Hash> threadsForRemoval = new List<Hash>();
                foreach (KeyValuePair<Hash, IncomingClient> kvp in ThreadList)
                {
                    if (kvp.Value.Ended == true)
                    {
                        threadsForRemoval.Add(kvp.Key);
                    }
                }

                foreach (Hash h in threadsForRemoval)
                {
                    ThreadList.Remove(h);
                    DisplayUtils.Display("Removed Thread: " + HexUtil.ToString(h.Hex), DisplayType.Warning);
                }
            }
            catch (System.Exception ex)
            {
                DisplayUtils.Display("TimerCallback()", ex);
            }

        }

        private async void ClientHandler(object oTcpClient)
        {
            IncomingClient iClient = (IncomingClient)oTcpClient;
            TcpClient tcpClient = iClient.client;

            tcpClient.ReceiveTimeout = 15000;
            tcpClient.SendTimeout = 15000;

            NetworkStream reader = tcpClient.GetStream();
            NetworkStream writer = tcpClient.GetStream();

            MemoryStream messageStream = new MemoryStream();
            byte[] inbuffer = new byte[Constants.PREFS_APP_TCP_BUFFER_SIZE];

            List<TransportPacket> mp = new List<TransportPacket>();

            try
            {
                while (tcpClient != null)
                {
                    if (tcpClient.Connected)
                    {
                        bool packetGood = false;

                        if (reader.CanRead)
                        {
                            int bytesRead = 0;
                            do
                            {
                                bytesRead = 0; //int byteCounter = 0;

                                try
                                {
                                    bytesRead = await reader.ReadAsync(inbuffer, 0, inbuffer.Length);

                                    if (bytesRead == 0)
                                    {
                                        break;
                                    }

                                    messageStream.Write(inbuffer, 0, bytesRead);

                                    if (bytesRead > 0)
                                    {
                                        // DisplayUtils.Display("\nBytes : " + bytesRead);

                                        long SuccessPosition = 0;
                                        byte[] content = messageStream.ToArray();
                                        packetGood = PacketCodec.ValidateAndDecodeTransportPacket_Consume(iClient.UserName, content, ref mp, ref SuccessPosition);

                                        //PacketSender.SendTransportPacket(new BinaryWriter(writer), TransportPacketType.KeepAlive, new byte[0], ref iClient.PacketCounter);

                                        if (packetGood)
                                        {
                                            foreach (TransportPacket p in mp)
                                            {
                                                try
                                                {
                                                    Constants.SERVER_GLOBAL_PACKETS++;
                                                    await ProcessIncomingUserInternal(iClient, p);
                                                }
                                                catch (Exception ex)
                                                {
                                                    DisplayUtils.Display("--- ProcessConnection:CNT:" + mp.Count, ex);
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
                                catch (System.Exception ex)
                                {
                                    DisplayUtils.Display("ClientHandler : Read()", ex);
                                }
                            }
                            while (bytesRead > 0);
                        }

                        //Thread.Sleep(50);
                    }
                    else
                        break;
                }
                reader.Close();
            }
            catch (System.Exception ex)
            {
                DisplayUtils.Display("ClientHandler()", ex);
            }

            iClient.Ended = true;
        }

        //System.Diagnostics.Stopwatch sw_timer = new System.Diagnostics.Stopwatch();

        async Task ProcessIncomingUserInternal(IncomingClient iClient, TransportPacket p)
        {
            BinaryReader reader = new BinaryReader(iClient.client.GetStream());
            //BinaryWriter writer = new BinaryWriter(iClient.client.GetStream());

            NetworkStream writer = iClient.client.GetStream();

            //DisplayUtils.Display("Packet Received: " + p.Type);

            switch (p.Type)
            {
                case TransportPacketType.Initialize:

                    iClient.ConnTimeStart = DateTime.UtcNow.Ticks;

                    //DisplayUtils.Display("Init Received", DisplayType.Info);

                    if ((!iClient.WorkProven) && (!iClient.KeyExchanged))
                    {
                        await PacketSender.SendTransportPacket(writer, TransportPacketType.WorkProofRequest, iClient.WorkTask);
                    }

                    break;

                case TransportPacketType.WorkProofKeyResponse:
                    //DisplayUtils.Display("WorkProofResponse Received : " + p.Data.Length, DisplayType.Info);

                    if ((!iClient.WorkProven) && (!iClient.KeyExchanged))
                    {
                        if (p.Data.Length == 60)
                        {
                            Constants.SERVER_GLOBAL_AUTH_PACKETS++;

                            byte[] Proof = new byte[4];
                            byte[] DHClientPublic = new byte[32];
                            byte[] AuthRandom = new byte[24];

                            Array.Copy(p.Data, 0, Proof, 0, 4);
                            Array.Copy(p.Data, 4, DHClientPublic, 0, 32);
                            Array.Copy(p.Data, 36, AuthRandom, 0, 24);

                            iClient.WorkProven = WorkProof.VerifyProof(iClient.WorkTask, Proof, Constants.Difficulty);

                            if (iClient.WorkProven)
                            {
                                //DisplayUtils.Display("Work Proved", DisplayType.Info);
                                Common.rngCsp.GetBytes(iClient.DHRandomBytes);

                                iClient.DHPrivateKey = Curve25519.ClampPrivateKey(iClient.DHRandomBytes);
                                iClient.DHPublicKey = Curve25519.GetPublicKey(iClient.DHPrivateKey);

                                // Generate the shared-secret using the provided client Public Key
                                byte[] sharedSecret = (new SHA512Managed()).ComputeHash(Curve25519.GetSharedSecret(iClient.DHPrivateKey, DHClientPublic));

                                Array.Copy(sharedSecret, iClient.TransportKey, 32);
                                Array.Copy(sharedSecret, 32, iClient.AuthenticationKey, 0, 32);

                                // Sign the data using the Node-Private key, so that the client can know that the connection is secure.
                                // This thwarts MITM attacks.
                                byte[] Client_ServerAuthSignature = nodeConfig.SignDataWithPrivateKey(AuthRandom);

                                // 64 bytes Signature
                                byte[] signPlain = Client_ServerAuthSignature;

                                if (signPlain.Length != 64) throw new Exception("Improbable Assertion failed : 1");

                                // Encrypt the Signature and Identifier using Salsa20
                                byte[] signCrypted = Salsa20.ProcessSalsa20(signPlain, iClient.TransportKey, new byte[8], 0);

                                // EtM -> Encrypt then MAC
                                byte[] signMAC = (new HMACSHA256(iClient.AuthenticationKey)).ComputeHash(signCrypted);

                                // SERVER_PUBLIC[32] || signMAC[32] || signCrypted[64] => 128 bytes
                                byte[] KeysSignature = iClient.DHPublicKey.Concat(signMAC).Concat(signCrypted).ToArray();

                                await PacketSender.SendTransportPacket(writer, TransportPacketType.ServerPublicTransfer, KeysSignature);
                            }
                            else
                            {
                                DisplayUtils.Display("Work Proof invalid : " + p.Data.Length, DisplayType.Exception);
                                await PacketSender.SendTransportPacket(writer, TransportPacketType.InvalidAuthDisconnect, new byte[0]);
                            }
                        }
                        else
                        {
                            DisplayUtils.Display("Invalid Packet Length : " + p.Data.Length, DisplayType.Exception);
                            await PacketSender.SendTransportPacket(writer, TransportPacketType.InvalidAuthDisconnect, new byte[0]);
                        }
                    }

                    break;

                case TransportPacketType.KeyExComplete_1:

                    if ((iClient.WorkProven) && (!iClient.KeyExchanged))
                    {
                        if (p.Data.Length == 128)
                        {
                            // cryptedSigPK_MAC[32] || cryptedSigPK[96] = 128 bytes

                            byte[] cryptedSigPK_MAC = new byte[32];
                            byte[] cryptedSigPK = new byte[96];

                            Array.Copy(p.Data, 0, cryptedSigPK_MAC, 0, 32);
                            Array.Copy(p.Data, 32, cryptedSigPK, 0, 96);

                            byte[] cryptedSigPK_MAC_Expected = (new HMACSHA256(iClient.AuthenticationKey)).ComputeHash(cryptedSigPK);

                            if (CryptoBytes.ConstantTimeEquals(cryptedSigPK_MAC_Expected, cryptedSigPK_MAC))
                            {
                                byte[] workSignPK = Salsa20.ProcessSalsa20(cryptedSigPK, iClient.TransportKey, new byte[8], 0);

                                // workSignPK[96]  = workSign[64] || nodeConfig.PublicKey[32]

                                byte[] workSign = new byte[64];
                                byte[] remotePK = new byte[32];

                                Array.Copy(workSignPK, 0, workSign, 0, 64);
                                Array.Copy(workSignPK, 64, remotePK, 0, 32);

                                bool serverVerified = Ed25519.Verify(workSign, iClient.WorkTask, remotePK);

                                if (serverVerified)
                                {
                                    iClient.PublicKey = new Hash(remotePK);

                                    if (Constants.NetworkVerbosity >= Verbosity.Info)
                                    {
                                        /*DisplayUtils.Display(NodeSocketData.GetString(nodeSocketData) + " : Key Exchanged 1: Shared KEY [TransportKey] : {" +
                                            (HexUtil.ToString(TransportKey)) + "}", DisplayType.Info);*/

                                        DisplayUtils.Display(iClient.PublicKey.ToString() + " Client Authenticated", DisplayType.Info);
                                    }

                                    await PacketSender.SendTransportPacket(writer, TransportPacketType.KeyExComplete_2, new byte[0]);

                                    iClient.KeyExchanged = true;

                                    IncomingConnections.Add(iClient.PublicKey, iClient);

                                    DisplayUtils.Display("Exchange Complete 2, Shared KEY [TransportKey] : " + HexUtil.ToString(iClient.TransportKey), DisplayType.Info);

                                }
                            }
                        }
                    }

                    break;

                case TransportPacketType.DataCrypted:

                    if (iClient.KeyExchanged && p.Data.Length > 28)
                    {
                        Constants.SERVER_GLOBAL_DATA_PACKETS++;

                        byte[] nonce = new byte[8];
                        byte[] hmac = new byte[16];
                        byte[] crypted_data = new byte[p.Data.Length - 24];

                        Array.Copy(p.Data, 0, nonce, 0, 8);
                        Array.Copy(p.Data, 8, hmac, 0, 16);
                        Array.Copy(p.Data, 24, crypted_data, 0, crypted_data.Length);

                        byte[] MAC_EXPECTED = (new HMACSHA256(iClient.AuthenticationKey)).ComputeHash(crypted_data).Take(16).ToArray();

                        if (CryptoBytes.ConstantTimeEquals(MAC_EXPECTED, hmac))
                        {
                            byte[] DeCryptedData = Salsa20.ProcessSalsa20(crypted_data, iClient.TransportKey, nonce, 0);

                            byte[] counter = new byte[4];
                            byte[] rec_data = new byte[p.Data.Length - 28];

                            Array.Copy(DeCryptedData, 0, counter, 0, 4);
                            Array.Copy(DeCryptedData, 4, rec_data, 0, rec_data.Length);

                            Array.Copy(DeCryptedData, 0, counter, 0, 4);
                            Array.Copy(DeCryptedData, 4, rec_data, 0, rec_data.Length);

                            NetworkPacket np = new NetworkPacket();

                            np.Deserialize(rec_data);

                            if (PacketReceived != null)
                                PacketReceived(iClient.PublicKey, np);
                        }
                        else
                        {
                            DisplayUtils.Display("HMAC FAILED : ");
                        }
                    }

                    break;
            }
        }

        async Task SendData(byte[] Data, IncomingClient client)
        {
            try
            {
                byte[] nonce = new byte[8];
                Common.rngCsp.GetBytes(nonce);

                byte[] counter_sender_data = Utils.GetLengthAsBytes((int)client.PacketCounter).Concat(Data).ToArray();

                byte[] CryptedCounterSenderData = Salsa20.ProcessSalsa20(counter_sender_data, client.TransportKey, nonce, 0);

                byte[] HMAC = (new HMACSHA256(client.AuthenticationKey)).ComputeHash(CryptedCounterSenderData).Take(16).ToArray();

                byte[] NONCE_MAC_DATA = nonce.Concat(HMAC).Concat(CryptedCounterSenderData).ToArray();

                await PacketSender.SendTransportPacket(client.client.GetStream(), TransportPacketType.DataCrypted, NONCE_MAC_DATA);
            }
            catch { }
        }

        public void StartListeningInternal()
        {
            try
            {
                listener.Start();

                while (Constants.ApplicationRunning)
                {
                    IncomingClient _inClient = new IncomingClient();
                    _inClient.client = listener.AcceptTcpClient();

                    Constants.SERVER_GLOBAL_TOTAL_ACC_CONNS++;

                    Hash hsh = new Hash(Utils.GenerateUniqueGUID_Bytes(8));
                    _inClient.Identifier = hsh;

                    ThreadList.Add(hsh, _inClient);

                    Task.Run(() => { ClientHandler(_inClient); });

                    /*Thread clientThread = new Thread(new ParameterizedThreadStart(ClientHandler));
                    _inClient.thread = clientThread;
                    clientThread.Start(_inClient);*/
                }
            }
            catch (Exception ex)
            {
                DisplayUtils.Display("Listening", ex);
            }
        }

        public void StopAndExit()
        {
            listener.Stop();

            /* try
             {
                 foreach (KeyValuePair<Hash, IncomingClient> kvp in ThreadList)
                 {
                     if (kvp.Value.thread.IsAlive)
                     {
                         if (kvp.Value.client != null)
                         {
                             try
                             {
                                 kvp.Value.client.Client.Dispose();
                             }
                             catch { }
                         }
                     }
                 }
             }
             catch { }*/
        }

        public void StartListening()
        {
            // Create a thread to start the listen process, everything else is done using async-await.
            ThreadStart ts = new ThreadStart(StartListeningInternal);
            Thread thr = new Thread(ts);
            thr.Start();
        }


    }
}
