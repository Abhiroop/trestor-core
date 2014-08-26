using Chaos.NaCl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetCommon.Protocol;
using TNetWallet.CryptoUtility;

namespace TNetWallet.WalletOperations
{
    class Transactions
    {

        byte [] SenderPublicKey;
        byte [] ReceiverPublicKey;
        long Transaction;
        byte[] signature = new byte[64];

        private byte[] dataToSend = new byte[64];
        //byte[] signedData;

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

            byte [] privateKey = pkm.PrivateKey;
           
            signature = Ed25519.Sign(dataToSend, privateKey);

            _pack = MakeTransactionPacket();
        }

        private string MakeTransactionPacket()
        {
           /* StringBuilder sb = new StringBuilder();
            sb.Append(Convert.ToBase64String(SenderPublicKey));
            sb.Append("|");
            sb.Append(Convert.ToBase64String(ReceiverPublicKey));
            sb.Append("|");
            sb.Append(Transaction.ToString());
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString()));*/

            List<ProtocolDataType> packets = new List<ProtocolDataType>();

            packets.Add(ProtocolPackager.Pack(SenderPublicKey, 0));
            packets.Add(ProtocolPackager.Pack(ReceiverPublicKey, 1));
            packets.Add(ProtocolPackager.Pack(Transaction, 2));
            packets.Add(ProtocolPackager.Pack(signature, 3));

            byte[] pack = ProtocolPackager.PackRaw(packets);

            return  Convert.ToBase64String(pack);
      
        }

    }
}
