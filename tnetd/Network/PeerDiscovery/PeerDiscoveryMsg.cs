﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;


namespace TNetD.Network.PeerDiscovery
{
    class PeerDiscoveryMsg
    {
        // TODO: give value a meaning
        public ConcurrentDictionary<Hash, byte[]> knownPeers;



        public byte[] Serialize()
        {
            ProtocolDataType[] PDTs = new ProtocolDataType[knownPeers.Count];
            int i = 0;
            foreach (KeyValuePair<Hash, byte[]> peer in knownPeers)
            {
                PDTs[i] = (ProtocolPackager.Pack(peer.Key, 0));
                i++;
            }
            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] data)
        {
            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(data);
            knownPeers = new ConcurrentDictionary<Hash, byte[]>();
            for (int i = 0; i < PDTs.Count; i++)
            {
                Hash peer;
                ProtocolPackager.UnpackHash(PDTs[i], 0, out peer);
                knownPeers[peer] = null;
            }
        }
    }
}
