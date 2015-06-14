
// @Author : Arpan Jati
// @Date: 8th June 2015

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Network;
using TNetD.Network.Networking;
using TNetD.Transactions;
using TNetD.Tree;

/*
 * FSM States
 * 
 * ST_GOOD
 * ST_ROOT_FETCH
 * ST_DATA_FETCH
 */

namespace TNetD.Nodes
{
    public enum LedgerSyncStateTypes { ST_GOOD, ST_ROOT_FETCH, ST_DATA_FETCH };

    class LedgerSync
    {
        bool Enable = false;

        private readonly int NODE_REQUEST_COUNT = 20;

        object LedgerSyncLock = new object();

        NodeState nodeState;
        NodeConfig nodeConfig;
        NetworkPacketSwitch networkPacketSwitch;
        ListHashTree LedgerTree;
        System.Timers.Timer TimerLedgerSync;

        LedgerSyncStateTypes LedgerState;

        Queue<NodeDataEntity> PendingNodesToBeFetched = new Queue<NodeDataEntity>();
        Queue<Hash> NodeFetchQueue = new Queue<Hash>();

        public LedgerSync(NodeState nodeState, NodeConfig nodeConfig, NetworkPacketSwitch networkPacketSwitch)
        {
            this.nodeState = nodeState;
            this.nodeConfig = nodeConfig;
            this.networkPacketSwitch = networkPacketSwitch;
            this.LedgerTree = nodeState.Ledger.LedgerTree; // Just aliasing.

            LedgerState = LedgerSyncStateTypes.ST_GOOD;

            this.networkPacketSwitch.LedgerSyncEvent += networkHandler_LedgerSyncEvent;

            TimerLedgerSync = new System.Timers.Timer();
            if (Enable) TimerLedgerSync.Elapsed += TimerLedgerSync_Elapsed;
            TimerLedgerSync.Enabled = true;
            TimerLedgerSync.Interval = nodeConfig.UpdateFrequencyLedgerSyncMS;
            TimerLedgerSync.Start();
        }

        void TimerLedgerSync_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (LedgerSyncLock)
            {
                switch (LedgerState)
                {
                    case LedgerSyncStateTypes.ST_GOOD:

                        if (PendingNodesToBeFetched.Count == 0)
                        {
                            handle_ST_ROOT();
                        }
                        else
                        {
                            LedgerState = LedgerSyncStateTypes.ST_DATA_FETCH;
                            handle_ST_DATA_FETCH();                            
                        }

                        break;

                    case LedgerSyncStateTypes.ST_DATA_FETCH:
                        handle_ST_DATA_FETCH();
                        break;
                }
            }
        }

        /// <summary>
        /// All is well, let's get a random trusted peer and ask for the current root.
        /// </summary>
        void handle_ST_ROOT()
        {
            List<NodeSocketData> nsds;
            // THINK: HOW MANY NODES TO CONNECT TO ??
            if (nodeConfig.GetRandomTrustedNode(out nsds, 1))
            {
                foreach (NodeSocketData nsd in nsds)
                {
                    NetworkPacket request = new NetworkPacket(nodeConfig.PublicKey, PacketType.TPT_LSYNC_ROOT_REQUEST,
                        new byte[0], TNetUtils.GenerateNewToken());

                    networkPacketSwitch.AddToQueue(nsd.PublicKey, request);
                }
            }
        }

        void DebugPrint(string Text, DisplayType type)
        {
            DisplayUtils.Display(Text, type);
        }

        void FetchRemoteNode(Hash addressNibbles, byte childIndex)
        {
            List<byte> bytes = (addressNibbles.Hex.ToList());
            bytes.Add(childIndex);
            Hash fetchAddress = new Hash(bytes.ToArray());
            NodeFetchQueue.Enqueue(fetchAddress);
        }

        void ProcessPendingRemoteFetches()
        {
            long chunk_size = Common.LSYNC_MAX_REQUESTED_NODES / 2;

            while (NodeFetchQueue.Count > 0)
            {
                int count = 0;
                NodeInfoRequest nir = new NodeInfoRequest();
                while ((NodeFetchQueue.Count > 0) && (count++ < chunk_size))
                {
                    nir.Add(NodeFetchQueue.Dequeue());
                }

                // Create a packet and send.
                List<NodeSocketData> nsds;

                // A single random trusted node is okay for fetching data.
                if (nodeConfig.GetRandomTrustedNode(out nsds, 1))
                {
                    NetworkPacket request = new NetworkPacket(nodeConfig.PublicKey, PacketType.TPT_LSYNC_NODE_REQUEST,
                                nir.Serialize(), TNetUtils.GenerateNewToken());

                    networkPacketSwitch.AddToQueue(nsds[0].PublicKey, request);

                    //DebugPrint("Requesting " + nir.TotalRequestedNodes + " nodes from, " + nsds[0].PublicKey + " ME: " + nodeConfig.PublicKey, DisplayType.ImportantInfo);
                }
            }
        }

        void handle_ST_DATA_FETCH()
        {
            long totalOrderedNodes = 0;
            long totalOrderedLeaves = 0;

            while ((PendingNodesToBeFetched.Count > 0) &&
                (totalOrderedNodes < Common.LSYNC_MAX_ORDERED_NODES) &&
                (totalOrderedLeaves < Common.LSYNC_MAX_ORDERED_LEAVES))
            {
                NodeDataEntity nde = PendingNodesToBeFetched.Dequeue();

                if (nde.LeafCount <= Common.LSYNC_MAX_LEAVES_TO_FETCH)
                {
                    // Fetch all nodes below
                    List<NodeSocketData> nsds;

                    // A single random trusted node is okay for fetching data.
                    if (nodeConfig.GetRandomTrustedNode(out nsds, 1))
                    {
                        AllLeafDataRequest aldr = new AllLeafDataRequest(nde);

                        NetworkPacket request = new NetworkPacket(nodeConfig.PublicKey, PacketType.TPT_LSYNC_LEAF_REQUEST_ALL,
                            aldr.Serialize(), TNetUtils.GenerateNewToken());

                        networkPacketSwitch.AddToQueue(nsds[0].PublicKey, request);

                        totalOrderedLeaves += aldr.TotalRequestedLeaves;
                    }

                   // DebugPrint("Fetch Normal All Nodes Below", DisplayType.ImportantInfo);
                }
                else
                {
                    // Fetch selective nodes
                    //DebugPrint("Fetch Selective Nodes", DisplayType.ImportantInfo);

                    ListTreeNode currentNode;
                    if (LedgerTree.TraverseToNode(nde.AddressNibbles, out currentNode) == TraverseResult.Success)
                    {
                        if (currentNode.Hash != nde.NodeHash)
                        {
                            for (int i = 0; i < 16; i++)
                            {
                                Hash remoteChildHash = nde.Children[i];
                                ListTreeNode currentChild = currentNode.Children[i];

                                if (PendingNodesToBeFetched.Count > Common.LSYNC_MAX_PENDING_QUEUE_LENGTH) break;

                                if (remoteChildHash != null)
                                {
                                    if (currentChild == null)
                                    {
                                        FetchRemoteNode(nde.AddressNibbles, (byte)i); totalOrderedNodes++;
                                    }
                                    else
                                    {
                                        if (remoteChildHash != currentChild.Hash)
                                        {
                                            FetchRemoteNode(nde.AddressNibbles, (byte)i); totalOrderedNodes++;
                                        }
                                    }
                                }
                                else
                                {
                                    //DebugPrint("REMOTE NULL !!", DisplayType.ImportantInfo);

                                    // HANDLE CASE FOR THE REMOTE HAVING NO NODE WHEN WE HAVE
                                    // VERIFY WITH OTHERS AND DELETE
                                    // ONLY NEEDED IF THE TRUSTED NODES ARE SENDING BAD DATA
                                    // SHOULD BE IMPLEMENTED BEFORE FINAL NETWORK COMPLETION
                                }
                            }
                        }
                    }
                    else
                    {
                        // ORDER ALL NODES BELOW. Probably in the initial condition.
                        for (int i = 0; i < 16; i++)
                        {
                            FetchRemoteNode(nde.AddressNibbles, (byte)i); totalOrderedNodes++;
                        }
                    }
                }
            }

            ProcessPendingRemoteFetches();

            if (PendingNodesToBeFetched.Count == 0) LedgerState = LedgerSyncStateTypes.ST_GOOD;

        }

        void networkHandler_LedgerSyncEvent(NetworkPacket packet)
        {
            switch (packet.Type)
            {
                case PacketType.TPT_LSYNC_ROOT_REQUEST:
                    HandleRootRequest(packet);
                    break;

                case PacketType.TPT_LSYNC_ROOT_RESPONSE:
                    HandleRootResponse(packet);
                    break;

                case PacketType.TPT_LSYNC_LEAF_REQUEST_ALL:
                    HandleLeafRequestAll(packet);
                    break;

                case PacketType.TPT_LSYNC_LEAF_RESPONSE:
                    HandleLeafResponse(packet);
                    break;

                case PacketType.TPT_LSYNC_NODE_REQUEST:
                    HandleNodeRequest(packet);
                    break;

                case PacketType.TPT_LSYNC_NODE_RESPONSE:
                    HandleNodeResponse(packet);
                    break;
            }
        }

        void HandleNodeRequest(NetworkPacket packet)
        {
            NodeInfoRequest nir = new NodeInfoRequest();
            nir.Deserialize(packet.Data);

           // DebugPrint("NodeRequest from " + packet.PublicKeySource + " Nodes : " + nir.TotalRequestedNodes, DisplayType.Warning);

            if ((Common.LSYNC_MAX_REQUESTED_NODES >= nir.TotalRequestedNodes) &&
                (nir.TotalRequestedNodes == nir.RequestedNodesAdresses.Count))
            {
                NodeInfoResponse responseData = new NodeInfoResponse();

                foreach (Hash nodeAddress in nir.RequestedNodesAdresses)
                {
                    ListTreeNode ltn;
                    if (LedgerTree.TraverseToNode(nodeAddress, out ltn) == TraverseResult.Success)
                    {
                        responseData.Add(new NodeDataEntity(ltn));
                    }
                }

                NetworkPacket response = new NetworkPacket(nodeConfig.PublicKey, PacketType.TPT_LSYNC_NODE_RESPONSE,
                   responseData.Serialize(), packet.Token);

                networkPacketSwitch.AddToQueue(packet.PublicKeySource, response);
            }
        }

        void HandleNodeResponse(NetworkPacket packet)
        {
            // Check that the packet is valid.
            if (networkPacketSwitch.VerifyPendingPacket(packet))
            {
                NodeInfoResponse nir = new NodeInfoResponse();
                nir.Deserialize(packet.Data);

               // DebugPrint("NodeResponse from " + packet.PublicKeySource + " : " + packet.Data.Length + " Bytes, Nodes : " + nir.TotalRequestedNodes, DisplayType.Warning);

                foreach (NodeDataEntity nde in nir.RequestedNodes)
                {
                    PendingNodesToBeFetched.Enqueue(nde);
                }
            }
            else
            {
                DebugPrint("Packet VER FAILED : HandleNodeResponse().", DisplayType.Warning);
            }
        }

        void HandleLeafRequestAll(NetworkPacket packet)
        {
            AllLeafDataRequest aldr = new AllLeafDataRequest();
            aldr.Deserialize(packet.Data);

           // DebugPrint("LEAF REQUEST All : " + aldr.TotalRequestedLeaves + " NODES : " + packet.Data.Length + " Bytes", DisplayType.ImportantInfo);

            if (aldr.TotalRequestedLeaves <= Common.LSYNC_MAX_LEAVES_TO_FETCH)
            {
                ListTreeNode node;

                if (LedgerTree.TraverseToNode(aldr.AddressNibbles, out node) == TraverseResult.Success)
                {
                    List<LeafDataType> leaves = new List<LeafDataType>();

                    LedgerTree.GetAllLeavesUnderNode(Common.LSYNC_MAX_LEAVES_TO_FETCH, node, ref leaves);

                    LeafAccountDataResponse ladr = new LeafAccountDataResponse();

                    foreach (LeafDataType ldt in leaves)
                    {
                        AccountInfo ai = (AccountInfo)ldt;
                        ladr.Add(ai);
                    }

                    NetworkPacket response = new NetworkPacket(nodeConfig.PublicKey, PacketType.TPT_LSYNC_LEAF_RESPONSE,
                            ladr.Serialize(), packet.Token);

                    networkPacketSwitch.AddToQueue(packet.PublicKeySource, response);

                    //DebugPrint("SENT LEAF RESPONSE : " + ladr.LeafCount + " Leaves ... " + response.Data.Length + " Bytes", DisplayType.CodeAssertionFailed);
                }
            }
        }

        void HandleLeafResponse(NetworkPacket packet)
        {
            // Check that the packet is valid.
            if (networkPacketSwitch.VerifyPendingPacket(packet))
            {
                LeafAccountDataResponse ladr = new LeafAccountDataResponse();

                ladr.Deserialize(packet.Data);

                if (ladr.Leaves.Count == ladr.LeafCount)
                {
                    //DebugPrint("YAYY, RECEIVED " + ladr.LeafCount + " LEAVES: " + packet.Data.Length + " Bytes, Adding/Updating to ledger", DisplayType.Warning);

                    foreach (AccountInfo ai in ladr.Leaves)
                    {
                        LedgerTree.AddUpdate(ai);
                    }
                }
                else
                {
                    DebugPrint("Bad Deserialize : HandleLeafResponse().", DisplayType.Warning);
                }
            }
            else
            {
                DebugPrint("Packet VER FAILED : HandleLeafResponse().", DisplayType.Warning);
            }
        }

        void HandleRootRequest(NetworkPacket packet)
        {
            DebugPrint("ROOT DATA REQUESTED BY " + packet.PublicKeySource.ToString(), DisplayType.ImportantInfo);
            LedgerCloseData ledgerCloseData;
            if (nodeState.PersistentCloseHistory.GetLastRowData(out ledgerCloseData))
            {
                RootDataResponse rdrm = new RootDataResponse(LedgerTree.RootNode, ledgerCloseData);

                NetworkPacket response = new NetworkPacket(nodeConfig.PublicKey, PacketType.TPT_LSYNC_ROOT_RESPONSE,
                    rdrm.Serialize(), packet.Token);

                networkPacketSwitch.AddToQueue(packet.PublicKeySource, response);
            }
        }

        void HandleRootResponse(NetworkPacket packet)
        {
            // Check that the packet is valid.
            if (networkPacketSwitch.VerifyPendingPacket(packet))
            {
                RootDataResponse rdrm = new RootDataResponse();
                rdrm.Deserialize(packet.Data);

                /// Compare with current tree and matchup.

                if (LedgerTree.RootNode.Hash != rdrm.RootHash) // Need to match up child nodes.
                {
                    DebugPrint("MISMATCH: RootResponse from " + packet.PublicKeySource + " : " + packet.Data.Length + " Bytes", DisplayType.Warning);

                    LedgerState = LedgerSyncStateTypes.ST_DATA_FETCH;

                    for (int i = 0; i < 16; i++)
                    {
                        NodeDataEntity remoteChild = rdrm.Children[i];
                        ListTreeNode currentChild = LedgerTree.RootNode.Children[i];

                        if (PendingNodesToBeFetched.Count > Common.LSYNC_MAX_PENDING_QUEUE_LENGTH) break;

                        if (remoteChild != null)
                        {
                            if (currentChild == null)
                            {
                                // Download all the data below the node.
                                // Needs to be handled properly, as it may have millions of nodes.

                                PendingNodesToBeFetched.Enqueue(remoteChild);
                            }
                            else
                            {
                                if (remoteChild.NodeHash != currentChild.Hash)
                                {
                                    PendingNodesToBeFetched.Enqueue(remoteChild);
                                }
                            }
                        }
                        else
                        {
                            // HANDLE CASE FOR THE REMOTE HAVING NO NODE WHEN WE HAVE
                            // VERIFY WITH OTHERS AND DELETE
                            // ONLY NEEDED IF THE TRUSTED NODES ARE SENDING BAD DATA
                            // SHOULD BE IMPLEMENTED BEFORE FINAL NETWORK COMPLETION
                        }
                    }
                }
                else
                {
                    DebugPrint("ROOT IS SYNCHRONZED WITH: " + packet.PublicKeySource, DisplayType.ImportantInfo);
                }

            }
            else
            {
                DebugPrint("Packet VER FAILED : HandleRootResponse().", DisplayType.Warning);
            }
        }



    }
}
