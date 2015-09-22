
//
//  @Author: Arpan Jati
//  @Date: September 2015 
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Nodes
{
    class WatchToken
    {
        public Stopwatch stopWatch;
        public Hash token;
        public WatchToken(Stopwatch stopWatch, Hash token)
        {
            this.stopWatch = stopWatch;
            this.token = token;
        }
    }

    class NodeLatency
    {
        NodeState nodeState;
        NodeConfig nodeConfig;

        ConcurrentDictionary<Hash, ConcurrentQueue<double>> latencyMap;

        ConcurrentDictionary<Hash, WatchToken> stopWatches;

        public NodeLatency(NodeConfig nodeConfig, NodeState nodeState)
        {
            this.latencyMap = new ConcurrentDictionary<Hash, ConcurrentQueue<double>>();
            this.stopWatches = new ConcurrentDictionary<Hash, WatchToken>();

            this.nodeState = nodeState;
            this.nodeConfig = nodeConfig;
        }

        public void StartMeasurement(Hash publicKey, Hash token)
        {
            if (stopWatches.ContainsKey(publicKey))
            {                
                stopWatches[publicKey].stopWatch.Restart();
                stopWatches[publicKey].token = token;
            }
            else
            {
                WatchToken watchToken = new WatchToken(new Stopwatch(), token);
                watchToken.stopWatch.Start();

                stopWatches.AddOrUpdate(publicKey, watchToken, (k, v) => watchToken);
            }
        }

        /// <summary>
        /// Automatically adds a latency if the token is correct.
        /// </summary>
        /// <param name="publicKey"></param>
        public void StopMeasurement(Hash publicKey, Hash token)
        {
            if(stopWatches.ContainsKey(publicKey))
            {
                WatchToken watchToken = stopWatches[publicKey];
                if (watchToken.token == token)
                {
                    watchToken.stopWatch.Stop();

                    double latencyMilliseconds = ((double)watchToken.stopWatch.ElapsedTicks / 10000F);

                    watchToken.stopWatch.Reset();

                    WatchToken watchTokenR;
                    stopWatches.TryRemove(publicKey, out watchTokenR);

                    AddLatency(publicKey, latencyMilliseconds);
                    
                    double lat;
                    if(GetAverageLatency(publicKey, out lat))
                    {
                        DisplayUtils.Display("LAT: Node: " + nodeConfig.PublicKey.ToString().Substring(0, 8) + " <-> " +
                        publicKey.ToString().Substring(0, 8) + " : " + lat.ToString("0.000"));
                    }
                }
            }
        }

        void AddLatency(Hash publicKey, double latencyMilliseconds)
        {
            if(latencyMap.ContainsKey(publicKey))
            {
                if (latencyMap[publicKey].Count > Constants.LATENCY_MAX_ELEMENTS)
                {
                    double oldLatency;
                    latencyMap[publicKey].TryDequeue(out oldLatency);
                }

                latencyMap[publicKey].Enqueue(latencyMilliseconds);
            }
            else
            {
                ConcurrentQueue<double> latencies = new ConcurrentQueue<double>();
                latencies.Enqueue(latencyMilliseconds);
                latencyMap.AddOrUpdate(publicKey, latencies, (k, v) => latencies);
            }
        }

        public bool GetAverageLatency(Hash publicKey, out double latencyMilliseconds)
        {
            try
            {
                if (latencyMap.ContainsKey(publicKey))
                {
                    int elements = 0; double total = 0;

                    foreach (var lat in latencyMap[publicKey])
                    {
                        total += lat; elements++;
                    }

                    latencyMilliseconds = (double) total / (double)elements;
                    return true;
                }                
            }
            catch(Exception ex)
            {
                DisplayUtils.Display("Node Latency", ex);
            }

            latencyMilliseconds = 0;

            return false;
        }

        /// <summary>
        /// Remove stale entries.
        /// </summary>
        public void Prune()
        {
            HashSet<Hash> latencyRemovals = new HashSet<Hash>();

            foreach (var node in latencyMap)
            {
                // Remove 'latencies' if it is no longer connected.
                if (!nodeState.ConnectedValidators.ContainsKey(node.Key))
                {
                    latencyRemovals.Add(node.Key);
                }               
            }

            foreach(var latencyRemoval in latencyRemovals)
            {
                ConcurrentQueue<double> latencies;
                latencyMap.TryRemove(latencyRemoval, out latencies);
            }

            HashSet<Hash> watchRemovals = new HashSet<Hash>();

            foreach (var watch in stopWatches)
            {
                // Remove 'watchToken' if it is no longer connected.
                if (!nodeState.ConnectedValidators.ContainsKey(watch.Key))
                {
                    watchRemovals.Add(watch.Key);
                }
            }

            foreach (var watchRemoval in watchRemovals)
            {
                WatchToken watchToken;
                stopWatches.TryRemove(watchRemoval, out watchToken);
            }
        }

        public void Clear()
        {
            latencyMap.Clear();
        }

    }
}
