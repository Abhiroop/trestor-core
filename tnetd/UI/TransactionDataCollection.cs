using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNetD.Transactions;

namespace TNetD.UI
{
    public class TransactionEntityData //: INotifyPropertyChanged//IObservable<TransactionContent>
    {
        public string PublicKey { get; set; }
        public string Amount { get; set; }
        public bool IsSource { get; set; }

        public TransactionEntityData(TransactionEntity entity, bool isSource)
        {
            PublicKey = HexUtil.ToString(entity.PublicKey);
            Amount = entity.Amount.ToString();
            IsSource = isSource;
        }
        // public event PropertyChangedEventHandler PropertyChanged;
    }

    public class TransactionData //: INotifyPropertyChanged//IObservable<TransactionContent>
    {
        public TransactionEntityData [] Entity { get; set; }

        public string TransactionID { get; set; }

        public string TimeStamp { get; set; }

        public long Amount { get; set; }

        // public event PropertyChangedEventHandler PropertyChanged;

        public TransactionData(TransactionContent transactionContent) //TransactionEntityData source, TransactionEntityData dest, string txid)
        {
            List<TransactionEntityData> _Entity = new List<TransactionEntityData>();

            long _Money = 0;

            foreach (TransactionEntity ent in transactionContent.Sources)
            {
                _Entity.Add(new TransactionEntityData(ent, true));

                _Money += ent.Amount;
            }

            Amount = _Money;

            foreach (TransactionEntity ent in transactionContent.Destinations)
            {
                _Entity.Add(new TransactionEntityData(ent, false));
            }

            Entity = _Entity.ToArray();

            TransactionID = transactionContent.TransactionID.ToString();

            TimeStamp = DateTime.FromFileTimeUtc(transactionContent.Timestamp).ToString();
        }

    }

}
