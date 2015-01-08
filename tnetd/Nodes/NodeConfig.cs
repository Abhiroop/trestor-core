using Chaos.NaCl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TNetD.Network.Networking;

namespace TNetD.Nodes
{
    class NodeConfig
    {
        public int UpdateFrequencyMS;

        public int ListenPortProtocol;

        public int ListenPortRPC;

        public int NodeID;

        public string WorkDirectory;

        public Hash PublicKey;
               
        public string Path_AccountDB;

        public string Path_TransactionDB;

        public string Name;

        public NetworkConfig NetworkConfig = new NetworkConfig();

        public GlobalConfiguration GlobalConfiguration;
        
        byte[] masterNodeRandom;
        byte[] masterPrivateKeyExpanded;

        INIFile iniFile = default(INIFile);

       // RPCRequestHandler RpcRequestHandler;

        public NodeConfig(int NodeID, GlobalConfiguration GlobalConfiguration)
        {
            //this.RpcRequestHandler = rpcRequestHandler;

            WorkDirectory = /*AppDomain.CurrentDomain.BaseDirectory +*/ "NODE_" + NodeID;

            // TODO: FIX THIS, TAKE THIS FROM, DB
            Name = WorkDirectory;

            this.NodeID = NodeID;
            this.GlobalConfiguration = GlobalConfiguration;

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

            UpdateFrequencyMS = Constants.Node_UpdateFrequencyMS;
        }

        /// <summary>
        /// Signs the provided data using Ed25519 using the Master PrivateKey
        /// </summary>
        /// <param name="Data">Data to be signed using the Private Key</param>
        /// <returns>64 byte Signature.</returns>
        public byte [] SignDataWithPrivateKey(byte [] data)
        {
            return Ed25519.Sign(data, masterPrivateKeyExpanded);
        }
        
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

            _ListenPortProtocol = Constants.random.Next(32768, 65000);

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

            _ListenPortRPC = Constants.random.Next(32768, 65000);

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
                Constants.rngCsp.GetBytes(_RAND);

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


    }
}
