using Chaos.NaCl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TNetD.Ledgers;
using TNetD.Network;
using TNetD.Network.Networking;
using TNetD.PersistentStore;
using TNetD.Transactions;

namespace TNetD.Nodes
{
    internal class Node
    {
        SecureNetwork network = default(SecureNetwork);

        //public Dictionary<Hash, Node> TrustedNodes = new Dictionary<Hash, Node>();

        /// <summary>
        /// Outer Dict is TransactionID, inner is Voter node.
        /// </summary>
        public Dictionary<Hash, Dictionary<Hash, CandidateStatus>> ReceivedCandidates = new Dictionary<Hash, Dictionary<Hash, CandidateStatus>>();

        /// <summary>
        /// A dictionary of Trusted nodes, stored by PublicKey
        /// </summary>
        public Dictionary<Hash, NodeSocketData> TrustedNodes;

        public int OutTransactionCount = 0;
        public int InCandidatesCount;
        public int InTransactionCount;

        public AccountInfo AI;

        IPersistentAccountStore persistentAccountStore;

        Ledger ledger;

        public Ledger LocalLedger
        {
            get { return ledger; }
        }

        public Hash PublicKey
        {
            get
            {
                return nodeConfig.PublicKey;
            }
        }

        Timer Tmr;

        NodeConfig nodeConfig = default(NodeConfig);

        /// <summary>
        /// Initializes a node. Node ID is 0 for most cases.
        /// Only other use is hosting multiple validators from an IP (bad-idea) and simulation.
        /// </summary>
        /// <param name="ID"></param>
        public Node(int ID, GlobalConfiguration globalConfiguration)
        {
            nodeConfig = new NodeConfig(ID, globalConfiguration);

            network = new SecureNetwork(nodeConfig);
            network.PacketReceived += network_PacketReceived;

            TrustedNodes = globalConfiguration.TrustedNodes;

            network.Initialize();

            persistentAccountStore = new SQLiteAccountStore(nodeConfig);

            AI = new AccountInfo(PublicKey, Money);

            ledger = new Ledger(persistentAccountStore);

            ledger.AddUserToLedger(AI);

            Tmr = new Timer();
            Tmr.Elapsed += Tmr_Elapsed;
            Tmr.Enabled = true;
            Tmr.Interval = nodeConfig.UpdateFrequencyMS;
            Tmr.Start();

            // ////////////////////

            // Connect to TrustedNodes

            //List<Task> tasks = new List<Task>();

            foreach (KeyValuePair<Hash, NodeSocketData> kvp in TrustedNodes)
            {
                if (kvp.Key != PublicKey) // Make sure we are not connecting to self !!
                {
                    if (!network.IsConnected(kvp.Key))
                    {
                        SendInitialize(kvp.Key);
                    }
                }
            }
        }

        void network_PacketReceived(Hash publicKey, NetworkPacket packet)
        {
            DisplayUtils.Display(" Packet: " + packet.Type + " | From: " + publicKey + " | Data Length : " + packet.Data.Length );

            //throw new NotImplementedException();
        }

        public void StopNode()
        {
            Constants.ApplicationRunning = false;
            network.Stop();
        }

        void SendInitialize(Hash publicKey)
        {
            //await Task.Delay(Constants.random.Next(500, 1000)); // Wait a random delay before connecting.

            NetworkPacketQueueEntry npqe = new NetworkPacketQueueEntry(publicKey, new NetworkPacket(PublicKey, PacketType.TPT_HELLO, new byte[0]));

            network.AddToQueue(npqe);
        }

        private void Tmr_Elapsed(object sender, ElapsedEventArgs e)
        {
            //await Task.WhenAll(tasks.ToArray());

            

            while (PendingIncomingCandidates.Count > 0)
            {

                // ledger.PushNewCandidate(PendingIncomingCandidates.Pop());
            }

            // Send the transcation to the TrustedNodes
            while (PendingIncomingTransactions.Count > 0)
            {
                // TransactionContent tc = PendingIncomingTransactions.Pop();

                //  ReceivedCandidates
                // ReceivedCandidates
            }
        }


        void CreateArbitraryTransactionAndSendToTrustedNodes()
        {
            //List<TransactionSink> tsks = new List<TransactionSink>();

            //Node destNode = Constants.GlobalNodeList[Constants.random.Next(0, Constants.GlobalNodeList.Count)];

            //if (destNode.PublicKey != PublicKey)
            //{
            //    int Amount = Constants.random.Next(0, (int)(Money / 2));

            //    TransactionSink tsk = new TransactionSink(destNode.PublicKey, Amount);
            //    tsks.Add(tsk);

            //    TransactionContent tco = new TransactionContent(PublicKey, 0, tsks.ToArray(), new byte[0]);

            //    OutTransactionCount++;
            //}
        }

        void InitializeValuesFromGlobalLedger()
        {

        }

        public long Money
        {
            get
            {
                if (ledger != null)
                    if (ledger.AccountExists(PublicKey))
                        return ledger[PublicKey].Money;
                    else return -1;

                return -1;
            }
        }

        Stack<TransactionContentPack> PendingIncomingCandidates = new Stack<TransactionContentPack>();

        Stack<TransactionContentPack> PendingIncomingTransactions = new Stack<TransactionContentPack>();

        /// <summary>
        /// [TO BE CALLED BY OTHER NODES] Sends transactions to destination, only valid ones will be processed.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="Transactions"></param>
        public void SendTransaction(Hash source, TransactionContent Transaction)
        {
            PendingIncomingTransactions.Push(new TransactionContentPack(source, Transaction));
            InTransactionCount++;
        }

        /// <summary>
        /// [TO BE CALLED BY OTHER NODES] Sends candidates to destination [ONLY AFTER > 50% voting], only valid ones will be processed.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="Transactions"></param>
        public void SendCandidates(Hash source, TransactionContent[] Transactions)
        {
            if (TrustedNodes.ContainsKey(source))
            {
                foreach (TransactionContent tc in Transactions)
                {
                    PendingIncomingCandidates.Push(new TransactionContentPack(source, tc));
                    InCandidatesCount++;
                }
            }
        }


    }
}
