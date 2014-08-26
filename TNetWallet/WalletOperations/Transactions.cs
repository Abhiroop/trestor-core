using Chaos.NaCl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetWallet.CryptoUtility;

namespace TNetWallet.WalletOperations
{
    class Transactions
    {

        byte [] SenderPublicKey;
        byte [] ReceiverPublicKey;
        long Transaction;
        //byte[] signature;

        private byte[] dataToSend;
        byte[] signedData;

        string _pack = "";

        public string Packet
        {
            get { return _pack; }
        }

        public Transactions(byte [] SenderPublicKey,byte [] ReceiverPublicKey,long Transaction, PublicKeyManagement pkm)
        {
            this.SenderPublicKey = SenderPublicKey;
            this.ReceiverPublicKey = ReceiverPublicKey;
            this.Transaction = Transaction;

            byte []privateKey = pkm.PrivateKey;
            //make packet here


            //signedData = Ed25519.Sign(dataToSend, privateKey);

            //not required
            _pack = MakeTransactionPacket();
        }

        private string MakeTransactionPacket()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Convert.ToBase64String(SenderPublicKey));
            sb.Append("|");

            sb.Append(Convert.ToBase64String(ReceiverPublicKey));
            sb.Append("|");

            sb.Append(Transaction.ToString());

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString()));
      
        }

    }
}
