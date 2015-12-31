using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Network;
using TNetD.Network.Networking;
using TNetD.Nodes;

namespace TNetD.Tests
{
    class PacketLogger
    {
        NodeConfig nodeConfig = default(NodeConfig);

        // TODO: MAKE PRIVATE : AND FAST
        NodeState nodeState = default(NodeState);

        FileStream logger = default(FileStream);

        TextWriter tr = default(TextWriter);

        public PacketLogger(NodeConfig nodeConfig, NodeState nodeState)
        {
            this.nodeConfig = nodeConfig;
            this.nodeState = nodeState;

            logger = new FileStream(nodeConfig.WorkDirectory + "\\" + "voteLog.log", FileMode.Create);
            tr = new StreamWriter(logger);
        }

        ~PacketLogger()
        {
            logger.Flush();
            logger.Close();
        }

        public async Task LogSend(Hash publicKeyDestination, NetworkPacket packet)
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

                    sb.Append("S," + packet.Type + ",");
                    sb.Append(packet.Token + ",");
                    sb.Append(nodeState.CurrentNetworkTime + ",");
                    sb.Append(packet.PublicKeySource + ",");
                    sb.Append(publicKeyDestination + ",");

                    await tr.WriteLineAsync(sb.ToString()).ConfigureAwait(false);

                    break;

            }

        }

        public async Task LogReceive(NetworkPacket packet)
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
                    sb.Append(nodeState.CurrentNetworkTime + ",");
                    sb.Append(packet.PublicKeySource + ",");
                    sb.Append(nodeConfig.PublicKey + ","); // ME

                    await tr.WriteLineAsync(sb.ToString()).ConfigureAwait(false);

                    break;
            }
        }
    }
        
}
