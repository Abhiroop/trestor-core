
// @Author : Arpan Jati
// @Date: March 2016

using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using TNetD.Protocol;

namespace TNetD.Json.JS_Structs
{
    public class JS_NodeInfo : JS_Response, ISerializableBase
    {
        public byte[] PublicKey;

        public string Name;
        public string Address;
        public string Organisation;
        public string Platform;
        public string Email;
        public string Version;
        public string Network;

        public JS_NodeDetails NodeDetails;
        public JS_LedgerInfo LastLedgerInfo;

        public JS_NodeInfo()
        {
            Network = Common.NETWORK_TYPE.ToString();
            this.NodeDetails = new JS_NodeDetails();
            this.LastLedgerInfo = new JS_LedgerInfo();
        }

        public JS_NodeInfo(JS_NodeDetails NodeDetails, JS_LedgerInfo LastLedgerInfo)
        {
            this.NodeDetails = NodeDetails;
            this.LastLedgerInfo = LastLedgerInfo;
        }

        public JS_Resp GetResponse()
        {
            return new JS_Resp(RPCStatus.Success, this);
        }

        public byte[] Serialize()
        {
            var PDTs = new List<ProtocolDataType>();

            PDTs.Add(ProtocolPackager.Pack(PublicKey, 0));

            PDTs.Add(ProtocolPackager.Pack(Name, 1));
            PDTs.Add(ProtocolPackager.Pack(Address, 2));
            PDTs.Add(ProtocolPackager.Pack(Organisation, 3));
            PDTs.Add(ProtocolPackager.Pack(Platform, 4));
            PDTs.Add(ProtocolPackager.Pack(Email, 5));
            PDTs.Add(ProtocolPackager.Pack(Version, 6));
            PDTs.Add(ProtocolPackager.Pack(Network, 7));

            PDTs.Add(ProtocolPackager.Pack(NodeDetails, 8));
            PDTs.Add(ProtocolPackager.Pack(LastLedgerInfo, 9));

            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(byte[] data)
        {
            foreach (var PDT in ProtocolPackager.UnPackRaw(data))
            {
                switch (PDT.NameType)
                {
                    case 0:
                        ProtocolPackager.UnpackByteVector(PDT, 0, out PublicKey);
                        break;

                    case 1:
                        ProtocolPackager.UnpackString(PDT, 1, ref Name);
                        break;

                    case 2:
                        ProtocolPackager.UnpackString(PDT, 2, ref Address);
                        break;

                    case 3:
                        ProtocolPackager.UnpackString(PDT, 3, ref Organisation);
                        break;

                    case 4:
                        ProtocolPackager.UnpackString(PDT, 4, ref Platform);
                        break;

                    case 5:
                        ProtocolPackager.UnpackString(PDT, 5, ref Email);
                        break;

                    case 6:
                        ProtocolPackager.UnpackString(PDT, 6, ref Version);
                        break;

                    case 7:
                        ProtocolPackager.UnpackString(PDT, 7, ref Network);
                        break;

                    case 8:
                        byte[] _nodeDetails;
                        ProtocolPackager.UnpackByteVector(PDT, 8, out _nodeDetails);
                        NodeDetails.Deserialize(_nodeDetails);
                        break;

                    case 9:
                        byte[] _lastLedgerInfo;
                        ProtocolPackager.UnpackByteVector(PDT, 9, out _lastLedgerInfo);
                        LastLedgerInfo.Deserialize(_lastLedgerInfo);
                        break;

                  
                }
            }
        }
    }
}
