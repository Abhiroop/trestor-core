
// @Author : Arpan Jati
// @Date: 8th June 2015

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Network.Networking;

namespace TNetD.Nodes
{
    class LedgerSyncHandler
    {
        private NodeState nodeState;
        private NodeConfig nodeConfig;
        private NetworkPacketSwitch networkHandler;

        public LedgerSyncHandler(NodeState nodeState, NodeConfig nodeConfig, NetworkPacketSwitch networkHandler)
        {
            this.nodeState = nodeState;
            this.nodeConfig = nodeConfig;
            this.networkHandler = networkHandler;
            this.networkHandler.LedgerSyncEvent += networkHandler_LedgerSyncEvent;
        }

        void networkHandler_LedgerSyncEvent(Network.NetworkPacket packet)
        {
            switch (packet.Type)
            {
                case PacketType.TPT_LSYNC_FETCH_ROOT:
                    
                    break;

                case PacketType.TPT_LSYNC_REPLY_ROOT:
                    
                    break;
            }
        }






    }
}
