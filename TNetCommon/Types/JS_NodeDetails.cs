
// @Author : Arpan Jati
// @Date: March 2016

using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using TNetD.Protocol;

namespace TNetD.Json.JS_Structs
{
    public class JS_NodeDetails : JS_Response, ISerializableBase
    {
        [JsonIgnore]
        public int ConnectedPeers = 1;
        public int LoadLevel = 1;

        public long TransactionsProcessed = 0;
        public long TransactionsAccepted = 0;
        public long TransactionsVerified = 0;
        public long TransactionsValidated = 0;
        public long RequestsProcessed = 0;

        public long NetworkPacketsOut = 0;
        public long NetworkPacketsIn = 0;

        public long AccountCreationRequests = 0;
        public long TotalAccounts = 0;

        public long ProofOfWorkQueueLength = 0;

        public DateTime SystemTime = DateTime.UtcNow;
        public DateTime NetworkTime = DateTime.UtcNow;

        public JS_NodeDetails()
        {
        }

        public JS_NodeDetails(DateTime SystemTime, DateTime NetworkTime)
        {
            this.SystemTime = SystemTime;
            this.NetworkTime = NetworkTime;
        }

        public JS_Resp GetResponse()
        {
            return new JS_Resp(RPCStatus.Success, this);
        }

        public byte[] Serialize()
        {
            var PDTs = new List<ProtocolDataType>();

            PDTs.Add(ProtocolPackager.Pack(ConnectedPeers, 0));
            PDTs.Add(ProtocolPackager.Pack(LoadLevel, 1));

            PDTs.Add(ProtocolPackager.Pack(TransactionsProcessed, 2));
            PDTs.Add(ProtocolPackager.Pack(TransactionsAccepted, 3));
            PDTs.Add(ProtocolPackager.Pack(TransactionsVerified, 4));
            PDTs.Add(ProtocolPackager.Pack(TransactionsValidated, 5));
            PDTs.Add(ProtocolPackager.Pack(RequestsProcessed, 6));

            PDTs.Add(ProtocolPackager.Pack(NetworkPacketsOut, 7));
            PDTs.Add(ProtocolPackager.Pack(NetworkPacketsIn, 8));

            PDTs.Add(ProtocolPackager.Pack(AccountCreationRequests, 9));
            PDTs.Add(ProtocolPackager.Pack(TotalAccounts, 10));
            PDTs.Add(ProtocolPackager.Pack(ProofOfWorkQueueLength, 11));

            PDTs.Add(ProtocolPackager.Pack(SystemTime.ToFileTimeUtc(), 12));
            PDTs.Add(ProtocolPackager.Pack(NetworkTime.ToFileTimeUtc(), 13));

            return ProtocolPackager.PackRaw(PDTs);
        }

        public void Deserialize(string json)
        {
            JS_NodeDetails nd = (JS_NodeDetails)JsonConvert.DeserializeObject(json,
                            typeof(JS_NodeDetails), Common.JSON_SERIALIZER_SETTINGS);

            ConnectedPeers = nd.ConnectedPeers;
            LoadLevel = nd.LoadLevel;

            TransactionsProcessed = nd.TransactionsProcessed;
            TransactionsAccepted = nd.TransactionsAccepted;
            TransactionsVerified = nd.TransactionsVerified;
            TransactionsValidated = nd.TransactionsValidated;
            RequestsProcessed = nd.RequestsProcessed;

            NetworkPacketsOut = nd.NetworkPacketsOut;
            NetworkPacketsIn = nd.NetworkPacketsIn;

            AccountCreationRequests = nd.AccountCreationRequests;
            TotalAccounts = nd.TotalAccounts;
            ProofOfWorkQueueLength = nd.ProofOfWorkQueueLength;

            SystemTime = nd.SystemTime;
            NetworkTime = nd.NetworkTime;
        }

        public void Deserialize(byte[] data)
        {
            foreach (var PDT in ProtocolPackager.UnPackRaw(data))
            {
                switch (PDT.NameType)
                {
                    case 0:
                        ProtocolPackager.UnpackInt32(PDT, 0, ref ConnectedPeers);
                        break;

                    case 1:
                        ProtocolPackager.UnpackInt32(PDT, 1, ref LoadLevel);
                        break;

                    case 2:
                        ProtocolPackager.UnpackInt64(PDT, 2, ref TransactionsProcessed);
                        break;

                    case 3:
                        ProtocolPackager.UnpackInt64(PDT, 3, ref TransactionsAccepted);
                        break;

                    case 4:
                        ProtocolPackager.UnpackInt64(PDT, 4, ref TransactionsVerified);
                        break;

                    case 5:
                        ProtocolPackager.UnpackInt64(PDT, 5, ref TransactionsValidated);
                        break;

                    case 6:
                        ProtocolPackager.UnpackInt64(PDT, 6, ref RequestsProcessed);
                        break;

                    case 7:
                        ProtocolPackager.UnpackInt64(PDT, 7, ref NetworkPacketsOut);
                        break;

                    case 8:
                        ProtocolPackager.UnpackInt64(PDT, 8, ref NetworkPacketsIn);
                        break;

                    case 9:
                        ProtocolPackager.UnpackInt64(PDT, 9, ref AccountCreationRequests);
                        break;

                    case 10:
                        ProtocolPackager.UnpackInt64(PDT, 10, ref TotalAccounts);
                        break;

                    case 11:
                        ProtocolPackager.UnpackInt64(PDT, 11, ref ProofOfWorkQueueLength);
                        break;

                    case 12:
                        long _stsTime = 0;
                        ProtocolPackager.UnpackInt64(PDT, 12, ref _stsTime);
                        SystemTime = DateTime.FromFileTimeUtc(_stsTime);
                        break;

                    case 13:
                        long _netTime = 0;
                        ProtocolPackager.UnpackInt64(PDT, 13, ref _netTime);
                        NetworkTime = DateTime.FromFileTimeUtc(_netTime);
                        break;
                }
            }
        }
    }
}
