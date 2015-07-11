
// File: GlobalConfiguration.cs 
// Version: 1.0 
// Date: Jan 2, 2015 | July 2015 Moved to new file.
// Author : Arpan Jati

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Nodes
{
    struct NodeSocketData
    {
        /// <summary>
        /// Public key of the destination node.
        /// </summary>
        public Hash PublicKey;

        /// <summary>
        /// Listen Port of the destination node.
        /// </summary>
        public int ListenPort;

        /// <summary>
        /// IP of the destination node.
        /// </summary>
        public string IP;

        public string Name;

        public NodeSocketData(Hash PublicKey, int ListenPort, string IP, string Name)
        {
            this.PublicKey = PublicKey;
            this.ListenPort = ListenPort;
            this.IP = IP;
            this.Name = Name;
        }

        public static string GetString(NodeSocketData nsd)
        {
            return "Node : " + nsd.Name + " | " + nsd.PublicKey.ToString();//.Substring(0,8);
        }
    }
}
