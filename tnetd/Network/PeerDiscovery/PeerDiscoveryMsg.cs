
// Author : Stephan Verbuecheln
// Date: June 2015

using System.Collections.Concurrent;
using TNetD.Protocol;

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
            foreach (var peer in knownPeers)
            {
                PDTs[i] = ProtocolPackager.Pack(peer.Value.Serialize(), 0);
                i++;
            }
            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] data)
        {
            knownPeers = new ConcurrentDictionary<Hash, PeerData>();

            var PDTs = ProtocolPackager.UnPackRaw(data);

            foreach (var PDT in PDTs)
            {
                byte[] tmpData = new byte[0];
                ProtocolPackager.UnpackByteVector(PDT, 0, out tmpData);
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
