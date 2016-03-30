/*
 @Author: Arpan Jati
 @Date: Jan 2015
 */

using Chaos.NaCl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TNetD.Address;
using TNetD.Json.JS_Structs;
using TNetD.Network.Networking;

namespace TNetD.Nodes
{
    /// <summary>
    /// Initial configuration information for the node.
    /// Keeps everything needed to start the node.
    /// </summary>
    class NodeConfig
    {
        #region Locals

        public int UpdateFrequencyMS;
        public int UpdateFrequencyPacketProcessMS;
        public int UpdateFrequencyConsensusMS;
        public int UpdateFrequencyLedgerSyncMS;
        public int UpdateFrequencyLedgerSyncMS_Root;
        public int ListenPortProtocol;
        public int ListenPortRPC;
        public int NodeID { get; private set; }
        public string WorkDirectory;
        public string Name;
        public string Email;
        public string Organisation;
        public string Platform;

        public Hash PublicKey;

        public string Path_AccountDB;
        public string Path_TransactionDB;
        public string Path_BannedNamesDB;
        public string Path_CloseHistoryDB;

        /// <summary>
        /// Saves the node datails as a JSON file.
        /// This is loaded when the app starts.
        /// </summary>
        public string Path_NodeDetails;

        public NetworkConfig NetworkConfig = new NetworkConfig();
        public Dictionary<Hash, NodeSocketData> TrustedNodes;
        
        byte[] masterNodeRandom;
        byte[] masterPrivateKeyExpanded;

        string Path_TrustedNodes;

        INIFile iniFile = default(INIFile);

        //////////////

        #endregion

        public NodeConfig(int NodeID)
        {
            WorkDirectory = "NODE_" + NodeID;

            TrustedNodes = new Dictionary<Hash, NodeSocketData>();

            Path_TrustedNodes = WorkDirectory + "\\TrustedNodes.ini";
            Path_NodeDetails = WorkDirectory + "\\NodeDetails.json";

            LoadTrustedNodes();

            DisplayUtils.Display(" Node " + NodeID + " | Loaded " + TrustedNodes.Count + " trusted nodes");

            this.NodeID = NodeID;

            if (!Directory.Exists(WorkDirectory))
            {
                Directory.CreateDirectory(WorkDirectory);
            }

            iniFile = new INIFile(WorkDirectory + "\\NodeConfig.ini");

            masterNodeRandom = GetNodePrivateRandom(iniFile);

            DisplayUtils.Display(" Node " + NodeID + " | Private Key     : " + HexUtil.ToString(masterNodeRandom));

            byte[] _PublicKey;

            Ed25519.KeyPairFromSeed(out _PublicKey, out masterPrivateKeyExpanded, masterNodeRandom);

            PublicKey = new Hash(_PublicKey);

            DisplayUtils.Display(" Node " + NodeID + " | Public Key      : " + PublicKey);

            ListenPortProtocol = GetListenPortProtocol();

            DisplayUtils.Display(" Node " + NodeID + " | Listen Protocol : " + ListenPortProtocol);

            ListenPortRPC = GetListenPortRPC();

            DisplayUtils.Display(" Node " + NodeID + " | Listen RPC      : " + ListenPortRPC);

            Path_AccountDB = GetAccountDBPath();

            DisplayUtils.Display(" Node " + NodeID + " | Acct DB Path    : " + Path_AccountDB);

            Path_TransactionDB = GetTransactionDBPath();

            DisplayUtils.Display(" Node " + NodeID + " | Trxn DB Path    : " + Path_TransactionDB);

            // // // // // // // // //  Class Initializations // // // // // // // // // // // //

            NetworkConfig = new NetworkConfig(ListenPortProtocol);

            // // // // // // // // // // // // // // // // // // // // // // // // // // //

            //TODO: MAKE THESE LOCAL INI VARIABLES
            UpdateFrequencyMS = Constants.Node_UpdateFrequencyMS;
            UpdateFrequencyConsensusMS = Constants.Node_UpdateFrequencyConsensusMS;
            UpdateFrequencyPacketProcessMS = Constants.Node_UpdateFrequencyPacketProcessMS;
            UpdateFrequencyLedgerSyncMS = Constants.Node_UpdateFrequencyLedgerSyncMS;
            UpdateFrequencyLedgerSyncMS_Root = Constants.Node_UpdateFrequencyLedgerSyncMS_Root;

            Path_BannedNamesDB = GetInitString("PersistentDatabase", "BannedNamesDB", WorkDirectory + "\\BannedNames.sqlite3");
            DisplayUtils.Display(" Node " + NodeID + " | Banned Names DB Path    : " + Path_BannedNamesDB);

            Path_CloseHistoryDB = GetInitString("PersistentDatabase", "CloseHistoryDB", WorkDirectory + "\\CloseHistory.sqlite3");
            DisplayUtils.Display(" Node " + NodeID + " | Close History DB Path    : " + Path_CloseHistoryDB);

            // /////////////////////

            Organisation = GetInitString("Info", "Organisation", "_unspecified_");
            Email = GetInitString("Info", "Email", "_unspecified_");
            Platform = GetInitString("Info", "Platform", "_unspecified_");

            byte[] RAND_PART = new byte[8];

            Common.SECURE_RNG.GetBytes(RAND_PART);

            // TODO: FIX THIS, TAKE THIS FROM, DB
            Name = GetInitString("Info", "Name", "name" + HexUtil.ToString(RAND_PART).ToLowerInvariant());
        }

        /// <summary>
        /// Returns a string correcponding to the NodeID
        /// </summary>
        /// <returns></returns>
        public string ID()
        {
            return "Node_" + NodeID;
        }

        void LoadTrustedNodes()
        {
            if (File.Exists(Path_TrustedNodes))
            {
                StreamReader sr = new StreamReader(Path_TrustedNodes);

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

        /// <summary>
        /// Signs the provided data using Ed25519 using the Master PrivateKey
        /// </summary>
        /// <param name="Data">Data to be signed using the Private Key</param>
        /// <returns>64 byte Signature.</returns>
        public byte[] SignDataWithPrivateKey(byte[] data)
        {
            return Ed25519.Sign(data, masterPrivateKeyExpanded);
        }

        public void SignEntity(ISignableBase entity)
        {
            entity.UpdateSignature(Ed25519.Sign(entity.GetSignatureData(), masterPrivateKeyExpanded));
        }

        #region INI Methods

        int GetListenPortProtocol()
        {
            int _ListenPortProtocol = -1;

            try
            {
                string __ListenPort = iniFile.IniReadValue("Network", "ListenPortProtocol");
                _ListenPortProtocol = int.Parse(__ListenPort);
            }
            catch { }

            if (_ListenPortProtocol != -1)
            {
                return _ListenPortProtocol;
            }

            // Get new port

            _ListenPortProtocol = Common.NORMAL_RNG.Next(32768, 65000);

            iniFile.IniWriteValue("Network", "ListenPortProtocol", _ListenPortProtocol.ToString());

            _ListenPortProtocol = -1;

            try
            {
                string __ListenPort = iniFile.IniReadValue("Network", "ListenPortProtocol");
                _ListenPortProtocol = int.Parse(__ListenPort);

                DisplayUtils.Display(" Node " + NodeID + " | Using randomly generated ListenPortProtocol : " + _ListenPortProtocol);
            }
            catch { }

            if (_ListenPortProtocol != -1)
            {
                return _ListenPortProtocol;
            }
            else
            {
                throw new Exception("Cannot write Network/ListenPortProtocol to config file.");
            }
        }

        int GetListenPortRPC()
        {
            int _ListenPortRPC = -1;

            try
            {
                string __ListenPort = iniFile.IniReadValue("Network", "ListenPortRPC");
                _ListenPortRPC = int.Parse(__ListenPort);
            }
            catch { }

            if (_ListenPortRPC != -1)
            {
                return _ListenPortRPC;
            }

            // Get new port

            _ListenPortRPC = Common.NORMAL_RNG.Next(32768, 65000);

            iniFile.IniWriteValue("Network", "ListenPortRPC", _ListenPortRPC.ToString());

            _ListenPortRPC = -1;

            try
            {
                string __ListenPortRPC = iniFile.IniReadValue("Network", "ListenPortRPC");
                _ListenPortRPC = int.Parse(__ListenPortRPC);

                DisplayUtils.Display(" Node " + NodeID + " | Using randomly generated ListenPortRPC : " + _ListenPortRPC);
            }
            catch { }

            if (_ListenPortRPC != -1)
            {
                return _ListenPortRPC;
            }
            else
            {
                throw new Exception("Cannot write Network/ListenPortRPC to config file.");
            }
        }

        byte[] GetNodePrivateRandom(INIFile _iniFile)
        {
            byte[] _RAND = new byte[0];

            try
            {
                _RAND = HexUtil.GetBytes(_iniFile.IniReadValue("Keys", "PrivateRandom"));
            }
            catch { }

            if (_RAND.Length != 32)
            {
                _RAND = new byte[32];
                Common.SECURE_RNG.GetBytes(_RAND);

                DisplayUtils.Display(" Node " + NodeID + " Creating new PrivateKey : " + HexUtil.ToString(_RAND));
                _iniFile.IniWriteValue("Keys", "PrivateRandom", HexUtil.ToString(_RAND));

                byte[] _PublicKey, _temp;
                Ed25519.KeyPairFromSeed(out _PublicKey, out _temp, _RAND);

                // Just for storing / Not used / Can be used for verification of encrypted / NodePrivateRandom data.
                _iniFile.IniWriteValue("Keys", "PublicKey", HexUtil.ToString(_PublicKey));
            }
            else
            {
                return _RAND;
            }

            // Read newly created rand-value;
            // The duplicate read is to make sure the proper values which are written/updated are indeed used.

            _RAND = new byte[0];

            try
            {
                _RAND = HexUtil.GetBytes(_iniFile.IniReadValue("Keys", "PrivateRandom"));
            }
            catch { }

            if (_RAND.Length != 32)
            {
                throw new Exception("Cannot write Keys/PrivateRandom to config file.");
            }
            else
            {
                return _RAND;
            }
        }

        string GetInitString(string Section, string Key, string Default)
        {
            string _Temp_String = "";

            _Temp_String = iniFile.IniReadValue(Section, Key);

            if (_Temp_String != "")
            {
                return _Temp_String;
            }

            _Temp_String = Default;

            _Temp_String = _Temp_String.Replace("\\\\", "\\");

            iniFile.IniWriteValue(Section, Key, _Temp_String);

            _Temp_String = "";

            _Temp_String = iniFile.IniReadValue(Section, Key);

            if (_Temp_String != "")
            {
                return _Temp_String;
            }
            else
            {
                throw new Exception("Cannot write " + Section + "/" + Key + " to config file.");
            }
        }

        string GetAccountDBPath()
        {
            string _AC_DB_Path = "";

            _AC_DB_Path = iniFile.IniReadValue("PersistentDatabase", "AccountDBPath");

            if (_AC_DB_Path != "")
            {
                return _AC_DB_Path;
            }

            // Get new port

            _AC_DB_Path = WorkDirectory + "\\AccountStore.sqlite3";

            _AC_DB_Path = _AC_DB_Path.Replace("\\\\", "\\");

            iniFile.IniWriteValue("PersistentDatabase", "AccountDBPath", _AC_DB_Path);

            _AC_DB_Path = "";

            _AC_DB_Path = iniFile.IniReadValue("PersistentDatabase", "AccountDBPath");

            if (_AC_DB_Path != "")
            {
                return _AC_DB_Path;
            }
            else
            {
                throw new Exception("Cannot write PersistentDatabase/AccountDBPath to config file.");
            }

        }

        string GetTransactionDBPath()
        {
            string _TX_DB_Path = "";

            _TX_DB_Path = iniFile.IniReadValue("PersistentDatabase", "TransactionDBPath");

            if (_TX_DB_Path != "")
            {
                return _TX_DB_Path;
            }

            // Get new port

            _TX_DB_Path = WorkDirectory + "\\TransactionStore.sqlite3";

            _TX_DB_Path = _TX_DB_Path.Replace("\\\\", "\\");

            iniFile.IniWriteValue("PersistentDatabase", "TransactionDBPath", _TX_DB_Path);

            _TX_DB_Path = "";

            _TX_DB_Path = iniFile.IniReadValue("PersistentDatabase", "TransactionDBPath");

            if (_TX_DB_Path != "")
            {
                return _TX_DB_Path;
            }
            else
            {
                throw new Exception("Cannot write PersistentDatabase/TransactionDBPath to config file.");
            }

        }

        #endregion

        public JS_NodeInfo Get_JS_Info()
        {
            JS_NodeInfo info = new JS_NodeInfo();

            info.Name = Name;
            info.Address = AddressFactory.GetAddressString(AddressFactory.GetAddress(PublicKey.Hex, Name,
                NetworkType.MainNet, AccountType.MainValidator));

            info.Email = Email;
            info.Organisation = Organisation;
            info.Platform = Platform;
            info.PublicKey = PublicKey.Hex;
            info.Version = "0.2 beta-release (dev)";

            return info;
        }

        public bool GetRandomTrustedNode(out List<NodeSocketData> randomPeer, int count)
        {
            int peerCount = TrustedNodes.Count;
            randomPeer = new List<NodeSocketData>();
            if (peerCount >= count)
            {
                int[] dist = Utils.GenerateNonRepeatingDistribution(peerCount, count);

                foreach (int randomPeerID in dist)
                {
                    NodeSocketData nsd = TrustedNodes.Values.ElementAt(randomPeerID);
                    if (nsd.PublicKey != PublicKey)
                        randomPeer.Add(nsd);
                }

                return true;
            }

            return false;
        }

    }
}
