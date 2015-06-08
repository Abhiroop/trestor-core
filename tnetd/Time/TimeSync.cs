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
        private ConcurrentDictionary<Hash, TimeStruct> timeMap;

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
            timeMap = new ConcurrentDictionary<Hash, TimeStruct>();
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
            Print("start syncing");
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
                timeMap.AddOrUpdate(peer, ts, (ok, ov) => ts);

                // sending
                NetworkPacket packet = new NetworkPacket(nodeConfig.PublicKey, PacketType.TPT_TIMESYNC_REQUEST, message, ts.token);
                networkHandler.AddToQueue(peer, packet);
            }

            // wait
            Print("waiting");
            Thread.Sleep(TIME_TO_SLEEP);

            // process responses
            List<long> diffs = new List<long>();
            foreach (KeyValuePair<Hash, TimeStruct> entry in timeMap)
                diffs.Add(entry.Value.timeDifference);
            Print("received " + diffs.Count + " responses");
            if (diffs.Count > 0)
            {
                long diff = computeMedianDelay(diffs);
                Print("median diff of " + diff);
                return diff;
            }
            else
            {
                return 0;
            }
        }






        /// <summary>
        /// Takes one encoded request and sends an encoded response
        /// </summary>
        /// <param name="packet"></param>
        private void requestHandler(NetworkPacket packet)
        {
            TimeSyncRqMsg request = new TimeSyncRqMsg();
            request.Deserialize(packet.Data);

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

            TimeStruct ts = timeMap[sender];
            if (ts.token == packet.Token)
            {
                ts.receivedTime = nodeState.SystemTime;
                ts.TimeFromValidator = response.responderTime;
                long delay = (ts.receivedTime - ts.sendTime) / 2;
                ts.timeDifference = ts.TimeFromValidator - delay - ts.sendTime;
            }
            timeMap.AddOrUpdate(sender, ts, (ok, ov) => ts);
        }



        /// <summary>
        /// Compute median diff
        /// </summary>
        /// <param name="diffs">List of delays</param>
        /// <returns>Median of delays</returns>
        private long computeMedianDelay(List<long> diffs)
        {
            diffs.Sort();
            int l = diffs.Count;
            if ((l % 2) != 0)
                return diffs[l / 2];
            else
                return diffs[l / 2 - 1] + diffs[l / 2];
        }
    }
}










/*
// create a TimeStruct, set system time and add it to nodeState.TimeMap
public void SendTimeRequest()
{
    Hash[] connected = nodeState.ConnectedValidators.ToArray();

    foreach (Hash validator in connected)
    {
        Hash token = TNetUtils.GenerateNewToken();

        if (!nodeState.TimeMap.ContainsKey(validator))
        {
            TimeStruct ts = new TimeStruct();
            ts.sendTime = nodeState.SystemTime;
            ts.receivedTime = 0;
            ts.TimeFromValidator = 0;
            ts.timeDifference = 0;
            ts.token = token;
            nodeState.TimeMap[validator] = ts; 
            //nodeState.TimeMap.AddOrUpdate(validator, ts, (k,v) => ts);
        }
    }
}

public long GetGlobalAvgTime()
{
    List<long> timeVector = new List<long>();

    foreach (KeyValuePair<long, long> kvp in timeMachine)
    {
//                long myTime_i = kvp.Key;
//              long otherTime_i = kvp.Value;

        long offset = (nodeState.SystemTime - kvp.Key);
        long adjustedTime = offset + kvp.Value;

        timeVector.Add(adjustedTime);
    }

    long timeSum = nodeState.SystemTime;

    int total = timeVector.Count;

    for (int i = 0; i < total; i++)
    {
        timeSum += timeVector[i];
    }

    long avgTime = timeSum / (total + 1);
    return avgTime;
}


//if everything is ok then 0
//else 1
public bool SetTime(Hash PublicKey, Hash token, long time)
{
    if (nodeState.TimeMap.ContainsKey(PublicKey))
    {
        TimeStruct ts;
        nodeState.TimeMap.TryGetValue(PublicKey, out ts);
        Hash _token = ts.token;
        if (token != _token)
            return false;

        ts.receivedTime = nodeState.SystemTime;

        Int64 RTT_one_way = (nodeState.SystemTime - ts.sendTime) / 2;
        Int64 RTT_corrrected_time = time + RTT_one_way;

        ts.TimeFromValidator = RTT_corrrected_time;
        ts.timeDifference = (RTT_corrrected_time - nodeState.SystemTime);

        return true;
    }
    return false;
}

public long CalculateAvgTime()
{
    long diff = 0;
    int counter = 0;

    foreach (KeyValuePair<Hash, TimeStruct> ts in nodeState.TimeMap)
    {
        counter++;
        diff += ts.Value.timeDifference;
    }
    return (diff / counter);
}

*/
