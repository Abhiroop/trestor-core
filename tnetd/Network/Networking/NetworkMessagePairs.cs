
// @Author : Arpan Jati
// @Date: June 2015

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Network.Networking
{
    public class NetworkMessagePairs
    {
        public readonly Dictionary<PacketType, PacketType> Pairs = new Dictionary<PacketType, PacketType>();

        public HashSet<PacketType> Requests = new HashSet<PacketType>();
        public HashSet<PacketType> Responses = new HashSet<PacketType>();

        public NetworkMessagePairs()
        {
            Pairs.Add(PacketType.TPT_CONS_MERGE_REQUEST, PacketType.TPT_CONS_MERGE_RESPONSE);
            Pairs.Add(PacketType.TPT_CONS_MERGE_TX_FETCH_REQUEST, PacketType.TPT_CONS_MERGE_TX_FETCH_RESPONSE);
            Pairs.Add(PacketType.TPT_CONS_BALLOT_REQUEST, PacketType.TPT_CONS_BALLOT_RESPONSE);
            Pairs.Add(PacketType.TPT_CONS_BALLOT_AGREE_REQUEST, PacketType.TPT_CONS_BALLOT_AGREE_RESPONSE);

            Pairs.Add(PacketType.TPT_LSYNC_ROOT_REQUEST, PacketType.TPT_LSYNC_ROOT_RESPONSE);
            Pairs.Add(PacketType.TPT_LSYNC_NODE_REQUEST, PacketType.TPT_LSYNC_NODE_RESPONSE);
            Pairs.Add(PacketType.TPT_LSYNC_LEAF_REQUEST, PacketType.TPT_LSYNC_LEAF_RESPONSE);
            Pairs.Add(PacketType.TPT_LSYNC_LEAF_REQUEST_ALL, PacketType.TPT_LSYNC_LEAF_RESPONSE);

            Pairs.Add(PacketType.TPT_TIMESYNC_REQUEST, PacketType.TPT_TIMESYNC_RESPONSE);
            Pairs.Add(PacketType.TPT_PEER_DISCOVERY_INIT, PacketType.TPT_PEER_DISCOVERY_RESPONSE);

            foreach (var v in Pairs)
            {
                Requests.Add(v.Key);
                Responses.Add(v.Value);
            }
        }
    }
}
