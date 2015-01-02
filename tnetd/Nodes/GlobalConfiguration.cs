
// File: GlobalConfiguration.cs 
// Version: 1.0 
// Date: Jan 2, 2015
// Author : Arpan Jati

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TNetD.Nodes
{
    struct NodeSocketData
    {
        public Hash PublicKey;
        public int ListenPort;
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
            return "Node : " + nsd.Name + " | " + nsd.PublicKey;
        }
    }

    class GlobalConfiguration
    {
        public Dictionary<Hash, NodeSocketData> TrustedNodes;

        public GlobalConfiguration()
        {
            TrustedNodes = new Dictionary<Hash, NodeSocketData>();

            LoadTrustedNodes();
        }

        void LoadTrustedNodes()
        {
            if (File.Exists(Constants.File_TrustedNodes))
            {
                StreamReader sr = new StreamReader(Constants.File_TrustedNodes);

                TrustedNodes.Clear();

                //
                // Example String : [Public Key]  [IP] [ListenPort] [Name] 
                // EFA31D61AFD22C60065776AD58462D095C21689A9FFD07746E928084F5AB1CC0 127.0.0.1 64683 Node_0
                //

                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine().Trim();

                    if (!line.Contains(";")) // Semi-colon not allowed.
                    {
                        string[] parts = line.Split(' '); // Space is the separator.

                        if (parts.Length >= 3) // Name field is optional.
                        {
                            try
                            {
                                string _pk = parts[0].Trim();

                                byte[] _PK = HexUtil.GetBytes(_pk);

                                if (_PK.Length == 32) // must have 32 bytes                                
                                {
                                    string _ip = parts[1].Trim();
                                    int _port = int.Parse(parts[2].Trim());
                                    string _name = "";
                                    if (parts.Length >= 4) // Name field is present.
                                    {
                                        _name = parts[3].Trim();
                                    }

                                    Hash PK_Hash = new Hash(_PK);

                                    TrustedNodes.Add(PK_Hash, new NodeSocketData(PK_Hash, _port, _ip, _name));
                                }
                            }
                            catch { }
                        }
                    }
                }
            }

        }


    }
}
