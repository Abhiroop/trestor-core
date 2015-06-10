
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

        public LedgerSync(NodeState nodeState, NodeConfig nodeConfig, NetworkPacketSwitch networkPacketSwitch)
        {
            this.nodeState = nodeState;
            this.nodeConfig = nodeConfig;
            this.networkPacketSwitch = networkPacketSwitch;
            this.LedgerTree = nodeState.Ledger.LedgerTree; // Just aliasing.

            this.networkPacketSwitch.LedgerSyncEvent += networkHandler_LedgerSyncEvent;
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

            RootDataResponseMessage rdrm = new RootDataResponseMessage(nodeState.Ledger.LedgerTree.RootNode, ledgerCloseData);
        
            NetworkPacket response = new NetworkPacket(nodeConfig.PublicKey, PacketType.TPT_LSYNC_ROOT_RESPONSE,                
                rdrm.Serialize(), packet.Token);

            networkPacketSwitch.AddToQueue(packet.PublicKeySource, response);
        }

        void HandleRootResponse(NetworkPacket packet)
        {
            // Check that the packet is valid.
            if(networkPacketSwitch.VerifyPendingPacket(packet))
            {
                RootDataResponseMessage rdrm = new RootDataResponseMessage();
                rdrm.Deserialize(packet.Data);
                
                /// Compare with current tree and matchup.
                 
               if(LedgerTree.RootNode.Hash != rdrm.RootHash) // Need to match up child nodes.
               {


               }


            }

        }



    }
}
