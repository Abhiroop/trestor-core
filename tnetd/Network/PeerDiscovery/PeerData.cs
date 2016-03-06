
// Author : Stephan Verbuecheln
// Date: June 2015

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;
using TNetD.Transactions;
using TNetD.Nodes;
using Chaos.NaCl;


namespace TNetD.Network.PeerDiscovery
{
    class PeerData : ISerializableBase
    {
        public string Name;
        public Hash PubKey;

        public string IP;
        public int ListenPort;

        public long TimeStamp;
        public Hash Signature;

        public PeerData(NodeSocketData socketInfo, NodeState nodeState, NodeConfig nodeConfig)
        {
            this.IP = socketInfo.IP;
            this.ListenPort = socketInfo.ListenPort;
            this.PubKey = socketInfo.PublicKey;
            this.Name = socketInfo.Name;
            TimeStamp = nodeState.NetworkTime;
            Signature = new Hash(nodeConfig.SignDataWithPrivateKey(signableData()));
        }

        public PeerData()
        {

        }

        public void Init()
        {
            Name = "";
            PubKey = new Hash();
            IP = "";
            ListenPort = 0;
            TimeStamp = 0;
            Signature = new Hash();
        }

        public byte[] Serialize()
        {
            ProtocolDataType[] PDTs = new ProtocolDataType[6];

            PDTs[0] = (ProtocolPackager.Pack(Name, 0));
            PDTs[1] = (ProtocolPackager.Pack(PubKey, 1));
            PDTs[2] = (ProtocolPackager.Pack(IP, 2));
            PDTs[3] = (ProtocolPackager.PackVarint(ListenPort, 3));
            PDTs[4] = (ProtocolPackager.PackVarint(TimeStamp, 4));
            PDTs[5] = (ProtocolPackager.Pack(Signature, 5));

            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] Data)
        {
            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(Data);

            if (PDTs.Count == 6)
            {
                long port = 0;
                ProtocolPackager.UnpackString(PDTs[0], 0, ref Name);
                ProtocolPackager.UnpackHash(PDTs[1], 1, out PubKey);
                ProtocolPackager.UnpackString(PDTs[2], 2, ref IP);
                ProtocolPackager.UnpackVarint(PDTs[3], 3, ref port);
                ProtocolPackager.UnpackVarint(PDTs[4], 4, ref TimeStamp);
                ProtocolPackager.UnpackHash(PDTs[5], 5, out Signature);
                ListenPort = (int)port;
            }
        }

        private byte[] signableData()
        {
            List<byte> data = new List<byte>();
            data.AddRange(Utils.Encoding88591.GetBytes(Name));
            data.AddRange(PubKey.Hex);
            data.AddRange(Utils.Encoding88591.GetBytes(IP));
            data.AddRange(Conversions.Int32ToVector(ListenPort));
            data.AddRange(Conversions.Int64ToVector(TimeStamp));

            return data.ToArray();
        }

        /// <summary>
        /// Will check validity of signature for the connection data once implemented
        /// Public key for this signature is the validator's general pub key.
        /// </summary>
        /// <returns></returns>
        public bool CheckValidity()
        {
            byte[] data = signableData();
            return Ed25519.Verify(Signature.Hex, data, PubKey.Hex);
        }
    }

}
