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
        public Thread thread;
        public TcpClient client = default(TcpClient);
        public string UserName = "";
        public byte[] WorkTask = new byte[24];
        public byte[] WorkProof = new byte[4];

        public Hash Identifier = new Hash();

        public bool WorkProven = false; // Step 1
        public bool KeyExchanged = false; // Step 2
        public bool IsAuthenticated = false; // Step 3

        public long ConnTimeStart = 0;

        public UInt32 PacketCounter = 0;

        public byte[] serverPublicKey;
        public byte[] serverPrivateKey;
        //public byte[] serverSymmKey;

        public byte[] serverRandomBytes;
        public byte[] TransportKey;
        public byte[] AuthenticationKey;

        public IncomingClient()
        {
            byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToFileTimeUtc());
            byte[] workRand = new byte[16];

            Constants.rngCsp.GetBytes(workRand);

            Buffer.BlockCopy(workRand, 0, WorkTask, 0, 16);
            Buffer.BlockCopy(time, 0, WorkTask, 16, 8);

            // This rng part should be moved to, after work proof.

            serverRandomBytes = new byte[32];
            TransportKey = new byte[32];
            AuthenticationKey = new byte[32];

            Constants.rngCsp.GetBytes(serverRandomBytes);

            //Constants.rngCsp.GetBytes(TransportKey);
            //Constants.rngCsp.GetBytes(AuthenticationKey);
        }
    }
}
