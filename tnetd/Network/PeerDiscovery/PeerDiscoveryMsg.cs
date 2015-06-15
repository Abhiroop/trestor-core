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
        public ConcurrentDictionary<Hash, ConnectConfig> knownPeers;



        public byte[] Serialize()
        {
            ProtocolDataType[] PDTs = new ProtocolDataType[2 * knownPeers.Count];
            int i = 0;
            foreach (KeyValuePair<Hash, ConnectConfig> peer in knownPeers)
            {
                PDTs[i] = ProtocolPackager.Pack(peer.Key, 0);
                i++;
                PDTs[i] = ProtocolPackager.Pack(peer.Value.Serialize(), 1);
                i++;
            }
            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] data)
        {
            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(data);
            knownPeers = new ConcurrentDictionary<Hash, ConnectConfig>();

            int i = 0;
            while (i <= PDTs.Count - 2)
            {
                Hash peer;
                ProtocolPackager.UnpackHash(PDTs[i++], 0, out peer);

                byte[] tmpData = new byte[0];
                ProtocolPackager.UnpackByteVector(PDTs[i++], 1, ref tmpData);
                if (tmpData.Length > 0)
                {
                    ConnectConfig conn = new ConnectConfig();
                    conn.Deserialize(tmpData);
                    knownPeers[peer] = conn;
                }
            }
        }
    }
}
