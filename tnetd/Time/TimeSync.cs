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
        private NodeState nodeState;
        private NodeConfig nodeConfig;
        private NetworkHandler networkHandler;
        private ConcurrentDictionary<Hash, TimeStruct> collectedRequests;

        // time to sleep after sending out time sync requests
        // in milliseconds
        private readonly int TIME_TO_SLEEP = 5000;

        private void Print(String message)
        {
            DisplayUtils.Display(" Node " + nodeConfig.NodeID + " |\tTimeSync: " + message);
        }

        public TimeSync(NodeState nodeState, NodeConfig nodeConfig, NetworkHandler networkHandler)
        {
            this.nodeState = nodeState;
            this.nodeConfig = nodeConfig;
            this.networkHandler = networkHandler;
            networkHandler.TimeSyncEvent += networkHandler_TimeSyncEvent;
            collectedRequests = new ConcurrentDictionary<Hash, TimeStruct>();
        }

        void networkHandler_TimeSyncEvent(NetworkPacket packet)
        {
            switch (packet.Type)
            {
                case PacketType.TPT_TIMESYNC_REQUEST:
                    Print("request packet received");
                    requestHandler(packet);
                    break;
                case PacketType.TPT_TIMESYNC_RESPONSE:
                    Print("response packet received");
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
            foreach (KeyValuePair<Hash, TimeStruct> entry in collectedRequests)
                diffs.Add(entry.Value.timeDifference);

            long diff = computeMedianDelay(diffs);
            Print("received " + diffs.Count + " responses; median diff of " + diff);


            //send new requests
            Print("start syncing with " + nodeState.ConnectedValidators.Count + " peers");
            foreach (Hash peer in nodeState.ConnectedValidators)
            {
                // prepare message
                TimeSyncRqMsg request = new TimeSyncRqMsg();
                request.senderTime = nodeState.SystemTime;
                byte[] message = request.Serialize();

                // save locally
                TimeStruct ts = new TimeStruct();
                ts.sendTime = request.senderTime;
                ts.token = TNetUtils.GenerateNewToken();
                collectedRequests.AddOrUpdate(peer, ts, (ok, ov) => ts);

                // sending
                NetworkPacket packet = new NetworkPacket(nodeConfig.PublicKey, PacketType.TPT_TIMESYNC_REQUEST, message, ts.token);
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
            TimeSyncRqMsg request = new TimeSyncRqMsg();
            request.Deserialize(packet.Data);

            Print("request sender time" + request.senderTime);

            TimeSyncRsMsg response = new TimeSyncRsMsg();
            response.senderTime = request.senderTime;
            response.responderTime = nodeState.SystemTime;

            // sending response
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
            TimeSyncRsMsg response = new TimeSyncRsMsg();
            response.Deserialize(packet.Data);

            Hash sender = packet.PublicKeySource;

            TimeStruct ts = collectedRequests[sender];
            if (ts.token == packet.Token)
            {
                ts.receivedTime = nodeState.SystemTime;
                ts.TimeFromValidator = response.responderTime;
                long delay = (ts.receivedTime - ts.sendTime) / 2;
                ts.timeDifference = ts.TimeFromValidator - delay - ts.sendTime;
            }
            collectedRequests.AddOrUpdate(sender, ts, (ok, ov) => ts);
        }



        /// <summary>
        /// Compute median diff
        /// </summary>
        /// <param name="diffs">List of delays</param>
        /// <returns>Median of delays</returns>
        private long computeMedianDelay(List<long> diffs)
        {
            if (diffs.Count == 0)
                return 0;
            diffs.Sort();
            int l = diffs.Count;
            if ((l % 2) != 0)
                return diffs[l / 2];
            else
                return diffs[l / 2 - 1] + diffs[l / 2];
        }
    }
}
