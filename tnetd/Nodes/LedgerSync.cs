﻿
// @Author : Arpan Jati
// @Date: 8th June 2015

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Network;
using TNetD.Network.Networking;
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
        private readonly int NODE_REQUEST_COUNT = 20;

        object LedgerSyncLock = new object();

        NodeState nodeState;
        NodeConfig nodeConfig;
        NetworkPacketSwitch networkPacketSwitch;
        ListHashTree LedgerTree;
        System.Timers.Timer TimerLedgerSync;

        LedgerSyncStateTypes LedgerState;

        Queue<NodeDataResponse> PendingNodesToBeFetched = new Queue<NodeDataResponse>();

        public LedgerSync(NodeState nodeState, NodeConfig nodeConfig, NetworkPacketSwitch networkPacketSwitch)
        {
            this.nodeState = nodeState;
            this.nodeConfig = nodeConfig;
            this.networkPacketSwitch = networkPacketSwitch;
            this.LedgerTree = nodeState.Ledger.LedgerTree; // Just aliasing.

            LedgerState = LedgerSyncStateTypes.ST_GOOD;

            this.networkPacketSwitch.LedgerSyncEvent += networkHandler_LedgerSyncEvent;

            TimerLedgerSync = new System.Timers.Timer();
            TimerLedgerSync.Elapsed += TimerLedgerSync_Elapsed;
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
                        handle_ST_GOOD();
                        break;

                    //case LedgerSyncStateTypes.ST_ROOT_FETCH:
                      //  handle_ST_ROOT_FETCH();
                       // break;

                    case LedgerSyncStateTypes.ST_DATA_FETCH:
                        handle_ST_DATA_FETCH();
                        break;
                }
            }
        }

        /// <summary>
        /// All is well, let's get a random trusted peer and ask for the current root.
        /// </summary>
        void handle_ST_GOOD()
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

        //void handle_ST_ROOT_FETCH()
        //{
        //
        //}

        void handle_ST_DATA_FETCH()
        {
            int totalOrderedNodes = 0;
            int totalOrderedLeaves = 0;

            while ((PendingNodesToBeFetched.Count > 0) && 
                (totalOrderedNodes < Common.LSYNC_MAX_ORDERED_NODES) && 
                (totalOrderedLeaves < Common.LSYNC_MAX_ORDERED_LEAVES))
            {
                NodeDataResponse ndr = PendingNodesToBeFetched.Dequeue();

                if(ndr.LeafCount <= Common.LSYNC_MAX_LEAVES_TO_FETCH)
                {
                    // Fetch all nodes below

                    
                }
                else
                {
                    // Fetch selective nodes
                    

                }
            }
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
            }
        }

        void HandleRootRequest(NetworkPacket packet)
        {
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
                    LedgerState = LedgerSyncStateTypes.ST_DATA_FETCH;

                    for (int i = 0; i < 16; i++)
                    {
                        NodeDataResponse remoteChild = rdrm.Children[i];
                        ListTreeNode currentChild = LedgerTree.RootNode.Children[i];

                        if (remoteChild != null)
                        {
                            if(currentChild == null) 
                            {
                                // Download all the data below the node.
                                // Needs to be handled properly, as it may have millions of nodes.

                                if (PendingNodesToBeFetched.Count < Common.LSYNC_MAX_PENDING_QUEUE_LENGTH)
                                    PendingNodesToBeFetched.Enqueue(remoteChild);
                            }
                            else
                            {
                                if(remoteChild.NodeHash != currentChild.Hash)
                                {
                                    if (PendingNodesToBeFetched.Count < Common.LSYNC_MAX_PENDING_QUEUE_LENGTH)
                                        PendingNodesToBeFetched.Enqueue(remoteChild);
                                }

                                /*for (int j = 0; j < 16; j++)
                                {
                                    ListTreeNode currentChild_2 = currentChild.Children[j];
                                    Hash remoteChild_2 = remoteChild.Children[i];
                                    if(remoteChild_2 != currentChild_2.Hash)
                                    {
                                        PendingNodes.Enqueue(remoteChild);
                                    }
                                }*/
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

            }

        }



    }
}
