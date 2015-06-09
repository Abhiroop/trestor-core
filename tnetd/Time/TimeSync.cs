using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using TNetD.Nodes;
using TNetD.Network;
using TNetD.Network.Networking;


namespace TNetD.Time
{
    class TimeSync
    {
        struct RequestStruct
        {
            public long senderTime;
            public Hash token;
        }

        struct ResponseStruct
        {
            public long sentTime;
            public long responderTime;
            public long receivedTime;
            public Hash token;
            public long diff;
        }

        private NodeState nodeState;
        private NodeConfig nodeConfig;
        private NetworkHandler networkHandler;
        private ConcurrentDictionary<Hash, RequestStruct> sentRequests;
        private ConcurrentDictionary<Hash, ResponseStruct> collectedResponses;



        private void Print(String message)
        {
            DisplayUtils.Display(" Node " + nodeConfig.NodeID + " | TimeSync: " + message);
        }



        public TimeSync(NodeState nodeState, NodeConfig nodeConfig, NetworkHandler networkHandler)
        {
            this.nodeState = nodeState;
            this.nodeConfig = nodeConfig;
            this.networkHandler = networkHandler;
            networkHandler.TimeSyncEvent += networkHandler_TimeSyncEvent;
            collectedResponses = new ConcurrentDictionary<Hash, ResponseStruct>();
        }



        void networkHandler_TimeSyncEvent(NetworkPacket packet)
        {
            switch (packet.Type)
            {
                case PacketType.TPT_TIMESYNC_REQUEST:
                    requestHandler(packet);
                    break;
                case PacketType.TPT_TIMESYNC_RESPONSE:
                    responseHandler(packet);
                    break;
            }
        }




        /// <summary>
        /// Requests system time from all connected peers, collects responses after 5 seconds
        /// and computes the median
        /// </summary>
        /// <returns>Median diff of all responses</returns>
        public long SyncTime()
        {
            // process responses
            List<long> diffs = new List<long>();
            foreach (KeyValuePair<Hash, ResponseStruct> entry in collectedResponses)
                diffs.Add(entry.Value.diff);
            diffs.Add(0);
            collectedResponses = new ConcurrentDictionary<Hash, ResponseStruct>();

            long diff = computeAverage(diffs);
            double display = ((double)diff) / 10000000;
            DateTime st = DateTime.FromFileTimeUtc(nodeState.SystemTime);
            DateTime nt = DateTime.FromFileTimeUtc(nodeState.NetworkTime);

            Print(diffs.Count + " resp; diff " + display/*.ToString("0.000")*/ + "; \tst: " + st.ToLongTimeString() + "; \tnt: " + nt.ToLongTimeString());


            //send new requests
            //Print("start syncing with " + nodeState.ConnectedValidators.Count + " peers");
            sentRequests = new ConcurrentDictionary<Hash, RequestStruct>();
            foreach (Hash peer in nodeState.ConnectedValidators)
            {
                // save locally
                RequestStruct rs = new RequestStruct();
                rs.senderTime = nodeState.SystemTime;
                rs.token = TNetUtils.GenerateNewToken();
                sentRequests.AddOrUpdate(peer, rs, (ok, ov) => rs);

                // send message
                TimeSyncRqMsg request = new TimeSyncRqMsg();
                request.senderTime = nodeState.SystemTime;
                byte[] message = request.Serialize();
                NetworkPacket packet = new NetworkPacket(nodeConfig.PublicKey, PacketType.TPT_TIMESYNC_REQUEST, message, rs.token);
                networkHandler.AddToQueue(peer, packet);
            }

            return diff;
        }






        /// <summary>
        /// Takes one encoded request and sends an encoded response
        /// </summary>
        /// <param name="packet"></param>
        private void requestHandler(NetworkPacket packet)
        {
            // unpacking request
            TimeSyncRqMsg request = new TimeSyncRqMsg();
            request.Deserialize(packet.Data);

            // sending response
            TimeSyncRsMsg response = new TimeSyncRsMsg();
            response.senderTime = request.senderTime;
            response.responderTime = nodeState.SystemTime;
            byte[] data = response.Serialize();
            Hash token = packet.Token;
            NetworkPacket respacket = new NetworkPacket(nodeConfig.PublicKey, PacketType.TPT_TIMESYNC_RESPONSE, data, token);
            networkHandler.AddToQueue(packet.PublicKeySource, respacket);
        }


        /// <summary>
        /// Takes one encoded response, computes delay and diff and stores it
        /// </summary>
        /// <param name="packet"></param>
        private void responseHandler(NetworkPacket packet)
        {
            // unpacking response
            TimeSyncRsMsg response = new TimeSyncRsMsg();
            response.Deserialize(packet.Data);
            Hash sender = packet.PublicKeySource;

            // if never sent request to this peer, drop packet
            if (!sentRequests.Keys.Contains(sender))
                return;

            // store response and calculated results
            if (sentRequests[sender].token == packet.Token)
            {
                ResponseStruct rs = new ResponseStruct();
                rs.sentTime = response.senderTime;
                rs.token = packet.Token;
                rs.receivedTime = nodeState.SystemTime;
                rs.responderTime = response.responderTime;
                long delay = (rs.receivedTime - rs.sentTime) / 2;
                rs.diff = rs.responderTime - delay - rs.sentTime;
                collectedResponses.AddOrUpdate(sender, rs, (ok, ov) => rs);
            }
        }



        /// <summary>
        /// Compute median for a list of values
        /// </summary>
        /// <param name="values">List of values</param>
        /// <returns>Median of delays</returns>
        private long computeMedian(List<long> values)
        {
            values.Sort();
            int l = values.Count;
            if ((l % 2) != 0)
                return values[l / 2];
            else
                return values[l / 2 - 1] + values[l / 2];
        }

        /// <summary>
        /// Compute median for a list of values
        /// </summary>
        /// <param name="values">List of values</param>
        /// <returns>Median of delays</returns>
        private long computeAverage(List<long> values)
        {
            long sum = 0;
            foreach (long v in values)
                sum += v;
            return sum / values.Count;
        }
    }
}
