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





        public ConnectConfig(NodeSocketData socketInfo)
        {
            this.IP = socketInfo.IP;
            this.ListenPort = socketInfo.ListenPort;
            UpdateFrequencyMS = Constants.Network_UpdateFrequencyMS;
        }

        public ConnectConfig(string IP, int Port)
        {
            this.IP = IP;
            this.ListenPort = Port;
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
            PDTs[1] = (ProtocolPackager.Pack(ListenPort, 0));
            PDTs[2] = (ProtocolPackager.Pack(UpdateFrequencyMS, 0));

            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] Data)
        {
            List<ProtocolDataType> PDTs = ProtocolPackager.UnPackRaw(Data);


            ProtocolPackager.UnpackString(PDTs[0], 0, ref IP);
            ProtocolPackager.UnpackInt32(PDTs[1], 0, ref ListenPort);
            ProtocolPackager.UnpackInt32(PDTs[2], 0, ref UpdateFrequencyMS);
        }
    }

}
