using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Helpers;
using TNetD.Network;
using TNetD.Network.Networking;
using TNetD.Nodes;

namespace TNetD.Tests
{
    class PacketLogger
    {
        object writeLock = new object();

        public bool LoggingEnabled { get; set; } = false;

        NodeConfig nodeConfig = default(NodeConfig);

        // TODO: MAKE PRIVATE : AND FAST
        NodeState nodeState = default(NodeState);

        FileStream logger = default(FileStream);

        TextWriter tr = default(TextWriter);

        public Stopwatch sw;

        public PacketLogger(NodeConfig nodeConfig, NodeState nodeState)
        {
            this.nodeConfig = nodeConfig;
            this.nodeState = nodeState;

            logger = new FileStream(nodeConfig.WorkDirectory + "\\" + "voteLog_" + nodeConfig.NodeID + ".log", FileMode.Create);
            tr = new StreamWriter(logger);           
        }

        /*public void Initialize()
        {
            //sw = new Stopwatch();
            //tr.WriteLine("Logging Start At : " + DateTime.UtcNow.ToFileTimeUtc());
            //sw.Start();
        }*/

        public void LogSend(Hash publicKeyDestination, NetworkPacket packet, int k)
        {

            if (LoggingEnabled)
            {

                lock (writeLock)
                {
                    switch (packet.Type)
                    {
                        // LOG VOTING

                        case PacketType.TPT_CONS_MERGE_REQUEST:
                        case PacketType.TPT_CONS_MERGE_RESPONSE:
                        case PacketType.TPT_CONS_MERGE_TX_FETCH_REQUEST:
                        case PacketType.TPT_CONS_MERGE_TX_FETCH_RESPONSE:
                        case PacketType.TPT_CONS_SYNC_REQUEST:
                        case PacketType.TPT_CONS_SYNC_RESPONSE:
                        case PacketType.TPT_CONS_VOTE_REQUEST:
                        case PacketType.TPT_CONS_VOTE_RESPONSE:
                        case PacketType.TPT_CONS_CONFIRM_REQUEST:
                        case PacketType.TPT_CONS_CONFIRM_RESPONSE:

                            StringBuilder sb = new StringBuilder();

                            sb.Append("S"+k+"," + packet.Type + ",");
                            sb.Append(packet.Token + ",");
                            sb.Append(sw.ElapsedTicks + ",");
                            sb.Append(packet.PublicKeySource + ",");
                            sb.Append(publicKeyDestination + ",");

                            tr.WriteLine(sb.ToString());

                            break;
                    }
                }
            }
        }

        public void LogReceive(NetworkPacket packet)
        {
            if (LoggingEnabled)
            {
                lock (writeLock)
                {
                    switch (packet.Type)
                    {
                        // LOG VOTING

                        case PacketType.TPT_CONS_MERGE_REQUEST:
                        case PacketType.TPT_CONS_MERGE_RESPONSE:
                        case PacketType.TPT_CONS_MERGE_TX_FETCH_REQUEST:
                        case PacketType.TPT_CONS_MERGE_TX_FETCH_RESPONSE:
                        case PacketType.TPT_CONS_SYNC_REQUEST:
                        case PacketType.TPT_CONS_SYNC_RESPONSE:
                        case PacketType.TPT_CONS_VOTE_REQUEST:
                        case PacketType.TPT_CONS_VOTE_RESPONSE:
                        case PacketType.TPT_CONS_CONFIRM_REQUEST:
                        case PacketType.TPT_CONS_CONFIRM_RESPONSE:

                            StringBuilder sb = new StringBuilder();

                            sb.Append("R," + packet.Type + ",");
                            sb.Append(packet.Token + ",");
                            sb.Append(sw.ElapsedTicks + ",");
                            sb.Append(packet.PublicKeySource + ",");
                            sb.Append(nodeConfig.PublicKey + ","); // ME

                            tr.WriteLine(sb.ToString());

                            break;
                    }
                }
            }
        }
        public void LogMap(System.Collections.Concurrent.ConcurrentDictionary<Hash, Consensus.ConsensusStates> map)
        {

            lock (writeLock)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var entry in map)
                {
                    sb.Append(HighResolutionDateTime.UtcNow + " - ");
                    sb.Append(entry.Key + "-" + entry.Value + "\r\n");
                }

                tr.WriteLine(sb.ToString());
            }

        }

        public void LogFinish(string v)
        {

            StringBuilder sb = new StringBuilder();
            sb.Append(HighResolutionDateTime.UtcNow + " - ");
            sb.Append(v);
            tr.WriteLine(sb.ToString());
        }
    }
        
}
