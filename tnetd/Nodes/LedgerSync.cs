
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
        object LedgerSyncLock = new object();

        NodeState nodeState;
        NodeConfig nodeConfig;
        NetworkPacketSwitch networkPacketSwitch;
        ListHashTree LedgerTree;
        System.Timers.Timer TimerLedgerSync;

        LedgerSyncStateTypes LedgerState;

        Queue<NodeDataResponse> PendingNodes = new Queue<NodeDataResponse>();

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
            nodeState.PersistentCloseHistory.GetLastRowData(out ledgerCloseData);

            RootDataResponse rdrm = new RootDataResponse(LedgerTree.RootNode, ledgerCloseData);

            NetworkPacket response = new NetworkPacket(nodeConfig.PublicKey, PacketType.TPT_LSYNC_ROOT_RESPONSE,
                rdrm.Serialize(), packet.Token);

            networkPacketSwitch.AddToQueue(packet.PublicKeySource, response);
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


                        }


                    }



                }

            }

        }



    }
}
