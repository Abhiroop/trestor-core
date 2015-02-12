using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Network;
using TNetD.Network.Networking;
using TNetD.Nodes;
using TNetD.PersistentStore;
using TNetD.SyncFramework.Packets;

namespace TNetD.SyncFramework
{
    class TransactionStreamHandler
    {
        SecureNetwork network = default(SecureNetwork);
        NodeState nodeState = default(NodeState);

        public TransactionStreamHandler(SecureNetwork network, NodeState nodeState)
        {
            this.network = network;
            this.nodeState = nodeState;
        }

        public void HandlePacket(NetworkPacket packet)
        {
            try
            {
                switch (packet.Type)
                {
                    case PacketType.TPT_TX_SYNC_REQUEST:
                        HandleSyncRequest(packet);
                        break;

                    case PacketType.TPT_TX_SYNC_RESPONSE:

                        break;

                    case PacketType.TPT_TX_SYNC_REQUEST_ID:

                        break;

                    case PacketType.TPT_TX_SYNC_RESPONSE_ID:

                        break;

                    case PacketType.TPT_TX_SYNC_QUERY_CURRENT:

                        break;

                    case PacketType.TPT_TX_SYNC_RESPONSE_CURRENT:

                        break;
                }
            }
            catch(Exception ex)
            {
                DisplayUtils.Display("TransactionStreamHandler.HandlePacket", ex);
            }          
        }

        void HandleSyncRequest(NetworkPacket packet)
        {
            TransactionSyncRequest TSR = new TransactionSyncRequest();
            TSR.Deserialize(packet.Data);

            if(nodeState.NodeInfo.LastLedgerInfo.SequenceNumber >= (TSR.StartSequenceNumber + TSR.Length))
            {


            }

            

            
        }

    }
}
