//
// @Author: Arpan Jati
// C++ Vesion: @Date: 22th Oct 2014 : To C# : 16th Jan 2015
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Json.JS_Structs;

namespace TNetD.Nodes
{
    /// <summary>
    /// Dynamic state information gathered during the running of the node.
    /// </summary>
    class NodeState
    {
        public ConcurrentBag<Hash> GlobalBlacklistedValidators { get; set; }

        public ConcurrentBag<Hash> GlobalBlacklistedUsers { get; set; }

        public ConcurrentDictionary<Hash, TimeStruct> timeMap { get; set; }

        public ConcurrentBag<Hash> ConnectedValidators { get; set; }

        public long system_time{ get; set; }
        public long network_time{ get; set; }
        
        // //////////////////////
        /*public int ConnectedPeers;
        public int TransactionsProcessed;
        public int TransactionsAccepted;
        public int TransactionsValidated;
        public int RequestsProcessed;
        public int LoadLevel = 1;*/

        public JS_NodeInfo NodeInfo;

        // //////////////////////

        public NodeState()
        {
            GlobalBlacklistedValidators = new ConcurrentBag<Hash>();
            GlobalBlacklistedUsers = new ConcurrentBag<Hash>();
            timeMap = new ConcurrentDictionary<Hash, TimeStruct>();
            
            ConnectedValidators = new ConcurrentBag<Hash>();
            system_time = DateTime.UtcNow.ToFileTimeUtc();
            network_time = DateTime.UtcNow.ToFileTimeUtc();
        }

        public void SetNodeInfo(JS_NodeInfo NodeInfo)
        {
            this.NodeInfo = NodeInfo;
        }
    }
}
