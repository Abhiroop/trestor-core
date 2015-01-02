﻿using Chaos.NaCl;
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

        public int ListenPort;

        public int NodeID;

        public string WorkDirectory;

        public Hash PublicKey;
               
        public string DBPath;

        public NetworkConfig NetworkConfig = new NetworkConfig();

        public GlobalConfiguration GlobalConfiguration;
        
        byte[] masterNodeRandom;
        byte[] masterPrivateKeyExpanded;

        INIFile iniFile = default(INIFile);

        public NodeConfig(int NodeID, GlobalConfiguration GlobalConfiguration)
        {
            WorkDirectory = /*AppDomain.CurrentDomain.BaseDirectory +*/ "NODE_" + NodeID;

            this.NodeID = NodeID;
            this.GlobalConfiguration = GlobalConfiguration;

            if (!Directory.Exists(WorkDirectory))
            {
                Directory.CreateDirectory(WorkDirectory);
            }

            iniFile = new INIFile(WorkDirectory + "\\NodeConfig.ini");
            
            masterNodeRandom = GetNodePrivateRandom(iniFile);

            DisplayUtils.Display(" Node " + NodeID + " | Private Key   : " + HexUtil.ToString(masterNodeRandom));

            byte[] _PublicKey;

            Ed25519.KeyPairFromSeed(out _PublicKey, out masterPrivateKeyExpanded, masterNodeRandom);

            PublicKey = new Hash(_PublicKey);

            DisplayUtils.Display(" Node " + NodeID + " | Public Key    : " + PublicKey);

            ListenPort = GetListenPort();

            DisplayUtils.Display(" Node " + NodeID + " | Listen Port   : " + ListenPort);

            DBPath = GetDBPath();

            DisplayUtils.Display(" Node " + NodeID + " | Database Path : " + DBPath);
            
            // // // // // // // // //  Class Initializations // // // // // // // // // // // //

            NetworkConfig = new NetworkConfig(ListenPort);

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
        
        int GetListenPort()
        {
            int _ListenPort = -1;

            try
            {
                string __ListenPort = iniFile.IniReadValue("Network", "ListenPort");
                _ListenPort = int.Parse(__ListenPort);
            }
            catch { }

            if (_ListenPort != -1)
            {
                return _ListenPort;
            }

            // Get new port

            _ListenPort = Constants.random.Next(32768, 65000);

            iniFile.IniWriteValue("Network", "ListenPort", _ListenPort.ToString());

            _ListenPort = -1;

            try
            {
                string __ListenPort = iniFile.IniReadValue("Network", "ListenPort");
                _ListenPort = int.Parse(__ListenPort);
                

                DisplayUtils.Display(" Node " + NodeID + " | Using randomly generated ListenPort : " + _ListenPort);
            }
            catch { }

            if (_ListenPort != -1)
            {
                return _ListenPort;
            }
            else
            {
                throw new Exception("Cannot write Network/ListenPort to config file.");
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

        string GetDBPath()
        {
            string _DB_Path = "";

            _DB_Path = iniFile.IniReadValue("PersistentDatabase", "DBPath");

            if (_DB_Path != "")
            {
                //if (File.Exists(_DB_Path))
                // {
                return _DB_Path;
                // }
            }

            // Get new port

            _DB_Path = WorkDirectory + "\\AccountStore.sqlite3";

            _DB_Path = _DB_Path.Replace("\\\\", "\\");

            iniFile.IniWriteValue("PersistentDatabase", "DBPath", _DB_Path);

            _DB_Path = "";

            _DB_Path = iniFile.IniReadValue("PersistentDatabase", "DBPath");

            if (_DB_Path != "")
            {
                //if (File.Exists(_DB_Path))
                // {
                return _DB_Path;
                // }
                // else
                //{
                //throw new Exception("PersistentDatabase/DBPath invalid | I/O | Directory permission error.");
                // }
            }
            else
            {
                throw new Exception("Cannot write PersistentDatabase/DBPath to config file.");
            }

        }




    }
}