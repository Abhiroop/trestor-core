using Chaos.NaCl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TNetD.Ledgers;
using TNetD.Transactions;

namespace TNetD
{
    struct CandidateStatus
    {
        public bool Vote;
        public bool Forwarded;
        public CandidateStatus(bool Vote, bool Forwarded)
        {
            this.Vote = Vote;
            this.Forwarded = Forwarded;
        }
    }

    struct TransactionContentPack
    {
        public Hash Source;
        public TransactionContent Transacation;
        public TransactionContentPack(Hash Source, TransactionContent Transacation)
        {
            this.Source = Source;
            this.Transacation = Transacation;
        }
    }

    internal class Node
    {
        public Dictionary<Hash, Node> Connections = new Dictionary<Hash, Node>();

        public Dictionary<Hash, Node> TrustedNodes = new Dictionary<Hash, Node>();

        /// <summary>
        /// Outer Dict is TransactionID, inner is Voter node.
        /// </summary>
        public Dictionary<Hash, Dictionary<Hash, CandidateStatus>> ReceivedCandidates = new Dictionary<Hash, Dictionary<Hash, CandidateStatus>>();


        int ConnectionLimit = 0;
        public int OutTransactionCount;
        public int InCandidatesCount;
        public int InTransactionCount;

        public AccountInfo AI;

        Ledger ledger;

        public Ledger LocalLedger
        {
            get { return ledger; }
        }

        byte[] _PrivateKey;
        byte[] _PublicKey;

        public Hash PublicKey
        {
            get
            {
                return new Hash(_PublicKey);
            }
        }

        Timer Tmr;

        public Node(int ConnectionLimit, Ledger ledger, long Money, int TimerRate)
        {
            this.ConnectionLimit = ConnectionLimit;
            byte[] Seed = new byte[32];
            Constants.rngCsp.GetBytes(Seed);
            Ed25519.KeyPairFromSeed(out _PublicKey, out _PrivateKey, Seed);
            this.ledger = ledger;

            AI = new AccountInfo(PublicKey, Money);

            ledger.AddUserToLedger(AI);

            Tmr = new Timer();
            Tmr.Elapsed += Tmr_Elapsed;
            Tmr.Enabled = true;
            Tmr.Interval = TimerRate;
            Tmr.Start();
        }

        void Tmr_Elapsed(object sender, ElapsedEventArgs e)
        {
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
                if (ledger.AccountExists(PublicKey))
                    return ledger[PublicKey].Money;
                else return -1;
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
