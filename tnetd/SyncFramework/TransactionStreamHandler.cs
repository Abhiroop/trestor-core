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
using TNetD.Transactions;

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
                    case PacketType.TPT_TX_SYNC_FETCH_REQUEST:
                        HandleSyncFetchRequest(packet);
                        break;

                    case PacketType.TPT_TX_SYNC_FETCH_RESPONSE:
                        HandleSyncFetchResponse(packet);
                        break;

                    case PacketType.TPT_TX_SYNC_ID_REQUEST:

                        break;

                    case PacketType.TPT_TX_SYNC_ID_RESPONSE:

                        break;

                    case PacketType.TPT_TX_SYNC_QUERY_REQUEST:

                        HandleSyncQueryRequest(packet);

                        break;

                    case PacketType.TPT_TX_SYNC_QUERY_RESPONSE:
                        HandleSyncQueryResponse(packet);
                        break;

                    case PacketType.TPT_TX_SYNC_CLOSEHISTORY_REQUEST:
                        break;

                    case PacketType.TPT_TX_SYNC_CLOSEHISTORY_RESPONSE:
                        break;


                }
            }
            catch (Exception ex)
            {
                DisplayUtils.Display("TransactionStreamHandler.HandlePacket", ex);
            }
        }

        void HandleSyncQueryRequest(NetworkPacket packet)
        {

            TransactionSyncQueryResponse transactionSyncQueryResponse = new TransactionSyncQueryResponse();

            transactionSyncQueryResponse.LedgerSequence = nodeState.NodeInfo.LastLedgerInfo.SequenceNumber;

            NetworkPacket np = new NetworkPacket(new Hash(nodeState.NodeInfo.PublicKey), PacketType.TPT_TX_SYNC_QUERY_RESPONSE,
                        transactionSyncQueryResponse.Serialize(), packet.Token);

            network.AddToQueue(new NetworkPacketQueueEntry(packet.PublicKeySource, np)); // Send the reply.

        }

        void HandleSyncQueryResponse(NetworkPacket packet)
        {
            TransactionSyncQueryResponse tsqr = new TransactionSyncQueryResponse();
            tsqr.Deserialize(packet.Data);

            // Well, we have received a response from a node. 
            // CRITICAL: Test if we ordered it. Then check if we are behind, if so, send fetch requests.



            // if(nodeState.NodeInfo)

        }

        void HandleSyncFetchRequest(NetworkPacket packet)
        {
            TransactionSyncRequest TSR = new TransactionSyncRequest();
            TSR.Deserialize(packet.Data);

            if (nodeState.NodeInfo.LastLedgerInfo.SequenceNumber >= (TSR.StartSequenceNumber + TSR.Length))
            {
                TransactionSyncResponse transactionSyncResponse = new TransactionSyncResponse();

                if (nodeState.Persistent.TransactionStore.FetchBySequenceNumber(out transactionSyncResponse.TransactionContents,
                    TSR.StartSequenceNumber, TSR.Length) == DBResponse.FetchSuccess)
                {
                    NetworkPacket np = new NetworkPacket(new Hash(nodeState.NodeInfo.PublicKey), PacketType.TPT_TX_SYNC_FETCH_RESPONSE,
                        transactionSyncResponse.Serialize(), packet.Token);

                    network.AddToQueue(new NetworkPacketQueueEntry(packet.PublicKeySource, np)); // Send the reply.
                }

            }
        }

        void HandleSyncFetchResponse(NetworkPacket packet)
        {
            TransactionSyncResponse transactionSyncResponse = new TransactionSyncResponse();
            transactionSyncResponse.Deserialize(packet.Data);

            // Verify the correctness and push to database.
            // CRITICAL. THIS VESION TRUSTS THE SERVER (May not be true in practice)

            nodeState.Persistent.TransactionStore.AddUpdateBatch(transactionSyncResponse.TransactionContents);

            /*foreach (TransactionContentSet transactionContentSet in transactionSyncResponse.TransactionContents)
            {
                // CRITICAL: VERIFY that the transactions are actually valid. Or from fully trusted sources.
                foreach (TransactionContent transactionContent in transactionContentSet.TxContent)
                {
                    try
                    {
                        
            
                    }
                    catch (Exception ex) { DisplayUtils.Display("AddUpdateBatch()", ex); }
                }
            }*/

        }

    }
}
