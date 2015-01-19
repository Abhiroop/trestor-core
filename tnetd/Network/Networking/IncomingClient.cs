using TNetD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TNetD.Network.Networking
{
    public class IncomingClient
    {
        //public Thread thread;
        public TcpClient client = default(TcpClient);
        public string UserName = "";

        /// <summary>
        /// Remote PublicKey | Verified using ECDSA.
        /// </summary>
        public Hash PublicKey = new Hash();

        public byte[] WorkTask = new byte[24];
        public byte[] WorkProof = new byte[4];

        /// <summary>
        /// Identifier for internal book-keeping
        /// </summary>
        public Hash Identifier = new Hash();

        public bool WorkProven = false; // Step 1

        /// <summary>
        /// True if key-exchange process is complete. 
        /// Also means that the remote authentication also went well.
        /// </summary>
        public bool KeyExchanged = false; // Step 2
        //public bool IsAuthenticated = false;

        public long ConnTimeStart = 0;

        public UInt32 PacketCounter = 0;

        public byte[] DHRandomBytes;
        public byte[] DHPublicKey;
        public byte[] DHPrivateKey;

        /// <summary>
        /// Key using which all the data is encrypted.
        /// </summary>
        public byte[] TransportKey;

        /// <summary>
        /// HMAC Signing key for all the data.
        /// </summary>
        public byte[] AuthenticationKey;

        /// <summary>
        /// Becomes true when the connection is disconnected.
        /// </summary>
        public bool Ended = false;

        public IncomingClient()
        {
            byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToFileTimeUtc());
            byte[] workRand = new byte[16];

            Constants.rngCsp.GetBytes(workRand);

            Array.Copy(workRand, 0, WorkTask, 0, 16);
            Array.Copy(time, 0, WorkTask, 16, 8);

            TransportKey = new byte[32];
            AuthenticationKey = new byte[32];

            DHRandomBytes = new byte[32];
        }
    }
}
