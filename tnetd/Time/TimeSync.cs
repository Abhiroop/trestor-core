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
        private SecureNetwork network;
        private ConcurrentDictionary<Hash, TimeStruct> timeMap;

        // time to sleep after sending out time sync requests
        // in milliseconds
        private readonly int TIME_TO_SLEEP = 5000;




        public TimeSync(NodeState nodeState, NodeConfig nodeConfig, SecureNetwork network)
        {
            this.nodeState = nodeState;
            this.nodeConfig = nodeConfig;
            this.network = network;
            timeMap = new ConcurrentDictionary<Hash, TimeStruct>();
        }



        /*
         * requests time from all connected peers
         * collects responses
         * computes and returns the average of diffs
         */
        public long SyncTime()
        {
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
                NetworkPacketQueueEntry npqe = new NetworkPacketQueueEntry(peer, packet);
                network.AddToQueue(npqe);
            }

            // wait
            Thread.Sleep(TIME_TO_SLEEP);

            // process responses
            List<long> diffs = new List<long>();
            foreach (KeyValuePair<Hash, TimeStruct> entry in timeMap)
            {
                diffs.Add(entry.Value.timeDifference);
            }
            return computeMedianDelay(diffs);
        }



        /*
         * Message handler
         */
        public void MsgHandler(NetworkPacket packet)
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



        /*
         * Takes one encoded request and sends an encoded response
         */
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
            NetworkPacket respacket = new NetworkPacket(packet.PublicKey_Src, PacketType.TPT_TIMESYNC_RESPONSE, data, token);
            NetworkPacketQueueEntry npqe = new NetworkPacketQueueEntry(packet.PublicKey_Src, respacket);
            network.AddToQueue(npqe);
        }



        /*
         * Takes one encoded response and registers it
         */
        private void responseHandler(NetworkPacket packet)
        {
            TimeSyncRsMsg response = new TimeSyncRsMsg();
            response.Deserialize(packet.Data);

            Hash sender = packet.PublicKey_Src;

            TimeStruct ts = timeMap[sender];
            if (ts.token == packet.Token)
            {
                ts.receivedTime = nodeState.SystemTime;
                ts.timeDifference = ts.receivedTime - ts.sendTime;
            }
            timeMap.AddOrUpdate(sender, ts, (ok, ov) => ts);
        }



        /// <summary>
        /// Compute median delay
        /// </summary>
        /// <param name="delays">List of delays</param>
        /// <returns>Median of delays</returns>
        private long computeMedianDelay(List<long> delays)
        {
            delays.Sort();
            int l = delays.Count;
            if (l % 2 != 0)
                return delays[l / 2];
            else
                return delays[l / 2 - 1] + delays[l / 2];
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
