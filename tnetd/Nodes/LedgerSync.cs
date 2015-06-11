
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

namespace TNetD.Nodes
{
    class LedgerSync
    {
        private NodeState nodeState;
        private NodeConfig nodeConfig;
        private NetworkPacketSwitch networkPacketSwitch;
        private ListHashTree LedgerTree;
        System.Timers.Timer TimerLedgerSync;

        //Queue<>  

        public LedgerSync(NodeState nodeState, NodeConfig nodeConfig, NetworkPacketSwitch networkPacketSwitch)
        {
            this.nodeState = nodeState;
            this.nodeConfig = nodeConfig;
            this.networkPacketSwitch = networkPacketSwitch;
            this.LedgerTree = nodeState.Ledger.LedgerTree; // Just aliasing.

            this.networkPacketSwitch.LedgerSyncEvent += networkHandler_LedgerSyncEvent;

            TimerLedgerSync = new System.Timers.Timer();
            TimerLedgerSync.Elapsed += TimerLedgerSync_Elapsed;
            TimerLedgerSync.Enabled = true;
            TimerLedgerSync.Interval = nodeConfig.UpdateFrequencyLedgerSyncMS;
            TimerLedgerSync.Start();
        }

        void TimerLedgerSync_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
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

        bool GetRandomTrustedNode(out List<NodeSocketData> randomPeer, int count)
        {
            int peerCount = nodeConfig.GlobalConfiguration.TrustedNodes.Count;
            randomPeer = new List<NodeSocketData>();
            if (peerCount >= count)
            {
                int[] dist = Utils.GenerateNonRepeatingDistribution(peerCount, count);

                foreach (int randomPeerID in dist)
                {
                    randomPeer.Add(nodeConfig.GlobalConfiguration.TrustedNodes.Values.ElementAt(randomPeerID));
                }

                return true;
            }

            return false;
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

                }


            }

        }



    }
}
