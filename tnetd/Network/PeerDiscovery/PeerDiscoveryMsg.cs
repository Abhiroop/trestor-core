using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;
using TNetD.Network.Networking;


namespace TNetD.Network.PeerDiscovery
{
    class PeerDiscoveryMsg
    {
        // TODO: give value a meaning
        public ConcurrentDictionary<Hash, PeerData> knownPeers;



        public byte[] Serialize()
        {
            ProtocolDataType[] PDTs = new ProtocolDataType[knownPeers.Count];
            int i = 0;
            foreach (KeyValuePair<Hash, PeerData> peer in knownPeers)
            {
                PDTs[i] = ProtocolPackager.Pack(peer.Value.Serialize(), 0);
                i++;
            }
            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] data)
        {
            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(data);
            knownPeers = new ConcurrentDictionary<Hash, PeerData>();

            for (int i = 0; i < PDTs.Count; i++)
            {
                byte[] tmpData = new byte[0];
                ProtocolPackager.UnpackByteVector(PDTs[i], 0, ref tmpData);
                if (tmpData.Length > 0)
                {
                    PeerData conn = new PeerData();
                    conn.Deserialize(tmpData);
                    knownPeers[conn.PubKey] = conn;
                }
            }
        }
    }
}
