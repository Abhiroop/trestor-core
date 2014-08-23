using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetWallet.WalletOperations
{
    class Transactions
    {

        byte [] SenderPublicKey;
        byte [] ReceiverPublicKey;
        long Transaction;

        string _pack = "";

        public string Packet
        {
            get { return _pack; }
        }

        public Transactions(byte [] SenderPublicKey,byte [] ReceiverPublicKey,long Transaction)
        {
            this.SenderPublicKey = SenderPublicKey;
            this.ReceiverPublicKey = ReceiverPublicKey;
            this.Transaction = Transaction;

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
