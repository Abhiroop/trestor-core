
// @Author : Arpan Jati
// @Date: June 2015

using System.Collections.Generic;

namespace TNetD.Network.Networking
{
    public class NetworkMessagePairs
    {
        // Maps allowed Requests => Responses
        public readonly Dictionary<PacketType, PacketType> Pairs = new Dictionary<PacketType, PacketType>
        {
            [PacketType.TPT_CONS_SYNC_REQUEST] = PacketType.TPT_CONS_SYNC_RESPONSE,
            [PacketType.TPT_CONS_MERGE_REQUEST] = PacketType.TPT_CONS_MERGE_RESPONSE,
            [PacketType.TPT_CONS_MERGE_TX_FETCH_REQUEST] = PacketType.TPT_CONS_MERGE_TX_FETCH_RESPONSE,
            [PacketType.TPT_CONS_VOTE_REQUEST] = PacketType.TPT_CONS_VOTE_RESPONSE,
            [PacketType.TPT_CONS_CONFIRM_REQUEST] = PacketType.TPT_CONS_CONFIRM_RESPONSE,

            [PacketType.TPT_LSYNC_ROOT_REQUEST]= PacketType.TPT_LSYNC_ROOT_RESPONSE,
            [PacketType.TPT_LSYNC_NODE_REQUEST] = PacketType.TPT_LSYNC_NODE_RESPONSE,
            [PacketType.TPT_LSYNC_LEAF_REQUEST] = PacketType.TPT_LSYNC_LEAF_RESPONSE,
            [PacketType.TPT_LSYNC_LEAF_REQUEST_ALL] = PacketType.TPT_LSYNC_LEAF_RESPONSE,

            [PacketType.TPT_TIMESYNC_REQUEST] = PacketType.TPT_TIMESYNC_RESPONSE,
            [PacketType.TPT_PEER_DISCOVERY_REQUEST] = PacketType.TPT_PEER_DISCOVERY_RESPONSE,

            [PacketType.TPT_HEARTBEAT_REQUEST] = PacketType.TPT_HEARTBEAT_RESPONSE
        };

        // Hash Sets for faster lookups.
        public HashSet<PacketType> Requests = new HashSet<PacketType>();
        public HashSet<PacketType> Responses = new HashSet<PacketType>();
        
        public NetworkMessagePairs()
        {
            foreach (var v in Pairs)
            {
                Requests.Add(v.Key);
                Responses.Add(v.Value);
            }
        }
    }
}
