
// @Author : Arpan Jati
// @Date: Jan 2015 | June 2015

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNetD.Nodes;

namespace TNetD.Network.Networking
{
    public enum TransportPacketType
    {
        Initialize = 0, Control = 1, WorkProofRequest = 2, WorkProofKeyResponse = 3, ServerPublicTransfer = 4,
        KeyExComplete_1 = 5, KeyExComplete_2 = 6, DataCrypted = 7, Stream = 8, StreamCrypted = 9,
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

    public struct PendingNetworkRequest
    {
        public long Time;
        public Hash PublicKey;
        public PacketType ResponseType;
        public PendingNetworkRequest(long Time, Hash PublicKey, PacketType ResponseType)
        {
            this.Time = Time;
            this.PublicKey = PublicKey;
            this.ResponseType = ResponseType;
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
        
        TPT_CONS_STATE = 0x30, 
        TPT_CONS_MERGE_REQUEST = 0x31,
        TPT_CONS_MERGE_RESPONSE = 0x32,
        TPT_CONS_MERGE_TX_FETCH_REQUEST = 0x33,
        TPT_CONS_MERGE_TX_FETCH_RESPONSE = 0x34,
        TPT_CONS_VOTE_REQUEST = 0x35,
        TPT_CONS_VOTE_RESPONSE = 0x36,
        TPT_CONS_CONFIRM_REQUEST = 0x37,
        TPT_CONS_CONFIRM_RESPONSE = 0x38,   

        TPT_LSYNC_ROOT_REQUEST = 0x40,
        TPT_LSYNC_ROOT_RESPONSE = 0x41,
        TPT_LSYNC_NODE_REQUEST = 0x42,
        TPT_LSYNC_NODE_RESPONSE = 0x43,
        TPT_LSYNC_LEAF_REQUEST = 0x44,
        TPT_LSYNC_LEAF_REQUEST_ALL = 0x45,
        TPT_LSYNC_LEAF_RESPONSE = 0x46,
        
        /// <summary>
        /// Request transaction data for a range of Ledger Sequence Numbers
        /// FORMAT: TransactionSyncRequest
        /// </summary>
        TPT_TX_SYNC_FETCH_REQUEST = 0x50,

        /// <summary>
        /// Response to a Transaction Data Request [TransactionSyncResponse[]]
        /// </summary>
        TPT_TX_SYNC_FETCH_RESPONSE = 0x51,

        /// <summary>
        /// Request transaction ID's for a given Ledger Sequence Number 
        /// </summary>
        TPT_TX_SYNC_ID_REQUEST = 0x52,

        /// <summary>
        /// Response to a Transactio Data ID Request
        /// </summary>
        TPT_TX_SYNC_ID_RESPONSE = 0x53,

        /// <summary>
        /// Queries for current ledger sequence and TransactionCount
        /// </summary>
        TPT_TX_SYNC_QUERY_REQUEST = 0x54,

        /// <summary>
        /// Response for TPT_TX_SYNC_QUERY_CURRENT 
        /// </summary>
        TPT_TX_SYNC_QUERY_RESPONSE = 0x55,

        /// <summary>
        /// Queries for CLOSEHISTORY
        /// </summary>
        TPT_TX_SYNC_CLOSEHISTORY_REQUEST = 0x56,

        /// <summary>
        /// Response for TPT_TX_SYNC_CLOSEHISTORY_REQUEST 
        /// </summary>
        TPT_TX_SYNC_CLOSEHISTORY_RESPONSE = 0x57,

        TPT_TIMESYNC_REQUEST = 0x60,
        TPT_TIMESYNC_RESPONSE = 0x61,

        TPT_PEER_DISCOVERY_INIT = 0x70,
        TPT_PEER_DISCOVERY_RESPONSE = 0x71

    };

}
