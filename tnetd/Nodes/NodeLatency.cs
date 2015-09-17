
//
//  @Author: Arpan Jati
//  @Date: September 2015 
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Nodes
{
    class NodeLatency
    {
        NodeState nodeState;

        ConcurrentDictionary<Hash, ConcurrentQueue<double>> latencyMap;

        public NodeLatency(NodeState nodeState)
        {
            latencyMap = new ConcurrentDictionary<Hash, ConcurrentQueue<double>>();
            this.nodeState = nodeState;
        }

        public void AddLatency(Hash publicKey, double latencyMilliseconds)
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
            if(latencyMap.ContainsKey(publicKey))
            {
                int elems=0; double total = 0;
                foreach(var lat in latencyMap[publicKey])
                {
                    total += lat; elems++;
                }
                latencyMilliseconds = total / elems;
                return true;
            }
            latencyMilliseconds = 0;
            return false;
        }

        /// <summary>
        /// Remove stale entries.
        /// </summary>
        public void Prune()
        {
            foreach(var node in nodeState.ConnectedValidators)
            {
                if(!latencyMap.ContainsKey(node.Key))
                {
                    ConcurrentQueue<double> latencies;
                    latencyMap.TryRemove(node.Key, out latencies);
                }                
            }
        }

        public void Clear()
        {
            latencyMap.Clear();
        }

    }
}
