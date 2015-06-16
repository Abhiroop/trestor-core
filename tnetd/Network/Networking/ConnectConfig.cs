using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Protocol;
using TNetD.Transactions;
using TNetD.Nodes;


namespace TNetD.Network.Networking
{

    class ConnectConfig : ISerializableBase
    {
        /// <summary>
        /// IPv4 Address of the node. [make plans for IPv6]
        /// </summary>
        public string IP;

        /// <summary>
        /// Network Port to connect to.
        /// </summary>
        public int ListenPort;

        /// <summary>
        /// Delay in ms between two consecutive refresh operations.
        /// </summary>
        public int UpdateFrequencyMS;

        public long TimeStamp;





        public ConnectConfig(NodeSocketData socketInfo)
        {
            this.IP = socketInfo.IP;
            this.ListenPort = socketInfo.ListenPort;
            UpdateFrequencyMS = Constants.Network_UpdateFrequencyMS;
        }

        public ConnectConfig(string IP, int port)
        {
            this.IP = IP;
            this.ListenPort = port;
            UpdateFrequencyMS = Constants.Network_UpdateFrequencyMS;
        }

        public ConnectConfig()
        {
            this.IP = "";
            this.ListenPort = 0;
            this.UpdateFrequencyMS = 0;
        }


        public byte[] Serialize()
        {
            ProtocolDataType[] PDTs = new ProtocolDataType[3];

            PDTs[0] = (ProtocolPackager.Pack(IP, 0));
            PDTs[1] = (ProtocolPackager.PackVarint(ListenPort, 1));
            PDTs[2] = (ProtocolPackager.PackVarint(UpdateFrequencyMS, 2));
            PDTs[3] = (ProtocolPackager.PackVarint(TimeStamp, 3));

            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] Data)
        {
            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(Data);


            ProtocolPackager.UnpackString(PDTs[0], 0, ref IP);
            long port = 0, frequency = 0;
            ProtocolPackager.UnpackVarint(PDTs[1], 1, ref port);
            ProtocolPackager.UnpackVarint(PDTs[2], 2, ref frequency);
            ProtocolPackager.UnpackVarint(PDTs[3], 3, ref TimeStamp);
            ListenPort = (int) port;
            UpdateFrequencyMS = (int) frequency;
        }

        /// <summary>
        /// Will check validity of signature for the connection data once implemented
        /// Public key for this signature is the validator's general pub key.
        /// </summary>
        /// <returns></returns>
        public bool CheckValidity(Hash pubkey)
        {
            return true;
        }
    }

}
