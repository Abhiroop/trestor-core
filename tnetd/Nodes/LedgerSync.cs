
// @Author : Arpan Jati
// @Date: 8th June 2015

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Network;
using TNetD.Network.Networking;

namespace TNetD.Nodes
{
    class LedgerSync
    {
        private NodeState nodeState;
        private NodeConfig nodeConfig;
        private NetworkPacketSwitch networkPacketSwitch;

        public LedgerSync(NodeState nodeState, NodeConfig nodeConfig, NetworkPacketSwitch networkPacketSwitch)
        {
            this.nodeState = nodeState;
            this.nodeConfig = nodeConfig;
            this.networkPacketSwitch = networkPacketSwitch;
            this.networkPacketSwitch.LedgerSyncEvent += networkHandler_LedgerSyncEvent;
        }

        void networkHandler_LedgerSyncEvent(NetworkPacket packet)
        {
            switch (packet.Type)
            {
                case PacketType.TPT_LSYNC_FETCH_ROOT:
                    


                    break;

                case PacketType.TPT_LSYNC_REPLY_ROOT:
                    
                    break;
            }
        }

        void HandleFetchRoot(NetworkPacket packet)
        {
            
        }






    }
}
