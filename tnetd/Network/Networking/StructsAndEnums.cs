﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNetD.Nodes;

namespace TNetD.Network.Networking
{
    public enum TransportPacketType
    {
        Initialize = 0, Control = 1, WorkProofRequest = 2, WorkProofKeyResponse = 3, ServerPublicTransfer = 4, 
        KeyExComplete_1 = 5, KeyExComplete_2 = 6,  DataCrypted = 7, Stream = 8, StreamCrypted = 9, 
        InvalidAuthDisconnect = 10, KeepAlive = 11
    };

    public struct TransportPacket
    {
        /// <summary>
        /// Type of the packet, the interpretation will be based on the Type
        /// </summary>
        public TransportPacketType Type;

        /// <summary>
        /// Incremental version for the TransportPacket, Starts from 1;
        /// </summary>
        public byte Version;

        /// <summary>
        /// Data can be any length below max_value(signed int)
        /// </summary>
        public byte[] Data;

        public TransportPacket(byte[] Data, TransportPacketType Type, byte Version)
        {
            this.Data = Data;
            this.Type = Type;
            this.Version = Version;
        }

    }

    /////////////////////////////////////
    
    class ConnectConfig
    {
        /// <summary>
        /// IPv4 Address of the node. [make plans for IPv6]
        /// </summary>
        public string IP;

        /// <summary>
        /// Network Port to connect to.
        /// </summary>
        public int ListenPort;

        /// <summary>
        /// Delay in ms between two consecutive refresh operations.
        /// </summary>
        public int UpdateFrequencyMS;

        public ConnectConfig(NodeSocketData socketInfo)
        {
            this.IP = socketInfo.IP;
            this.ListenPort = socketInfo.ListenPort;
            UpdateFrequencyMS = Constants.Network_UpdateFrequencyMS;
        }

        public ConnectConfig(string IP, int Port)
        {
            this.IP = IP;
            this.ListenPort = Port;
            UpdateFrequencyMS = Constants.Network_UpdateFrequencyMS;
        }
    }

    class NetworkConfig
    {
        /// <summary>
        /// Network Port to listen to.
        /// </summary>
        public int ListenPort;

        /// <summary>
        /// Delay in ms between two consecutive refresh operations.
        /// </summary>
        public int UpdateFrequencyMS;

        /// <summary>
        /// Configuration for IncomingConnectionHandler
        /// </summary>
        /// <param name="ListenPort">Network Port to listen to. </param>
        public NetworkConfig(int ListenPort)
        {
            this.ListenPort = ListenPort;
            UpdateFrequencyMS = Constants.Network_UpdateFrequencyMS;
        }

        public NetworkConfig()
            : this(Constants.Network_DefaultListenPort)
        {

        }
    }

    struct SocketInfo
    {
        public string IP;
        public int Port;
        public SocketInfo(string IP, int Port)
        {
            this.IP = IP;
            this.Port = Port;
        }
    }

    public enum NetworkResult { Sent, Received, Queued, ConnFailed, Disconnected };

    /// <summary>
    /// Packet Type (Application Data Layer)
    /// Make sure that the C++ version matches this.
    /// </summary>
    public enum PacketType
    {
        TPT_NOTHING = 0x00,
        TPT_HELLO = 0x01, TPT_DISCONNECT = 0x02, TPT_KEEPALIVE = 0x03,
        TPT_KEY_EXCHANGE_1 = 0x10, TPT_KEY_EXCHANGE_2 = 0x11, TPT_KEY_EXCHANGE_DONE = 0x12,

        // TPT_TRANS_REQUEST : DATA [TransactionContent] or a single TransactionContent request.
        TPT_TRANS_REQUEST = 0x20,

        TPT_TRANS_FORWARDING = 0x21,

        TPT_CONS_STATE = 0x30, TPT_CONS_CURRENT_SET = 0x31, TPT_CONS_REQUEST_TC_TX = 0x32, TPT_CONS_RESP_TC_TX = 0x33,

        TPT_CONS_VOTES = 0x34, TPT_CONS_TIME_SYNC = 0x35, TPT_CONS_DOUBLESPENDERS = 0x36,

        TPT_LSYNC_FETCH_ROOT = 0x40, TPT_LSYNC_FETCH_LAYER_DATA = 0x41,
        TPT_LSYNC_REPLY_ROOT = 0x42, TPT_LSYNC_REPLY_LAYER_DATA = 0x43

    };



}
