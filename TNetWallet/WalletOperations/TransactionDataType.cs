using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetWallet.WalletOperations
{
    class TransactionDataSource : ObservableCollection<TransactionHistoryType>
    {
        public TransactionDataSource(string incomingTransactionHistory) : base()
        {     
            //get all rows
            string[] rows = incomingTransactionHistory.Split('|');

            for(int i = 0; i<rows.Length; i++)
            {
                 TransactionHistoryType td = new TransactionHistoryType();
                //get columns here
                string[] col = rows[i].Split(':');

                td._ID = Encoding.UTF8.GetString(Convert.FromBase64String(col[0]));

                td._Sender = Encoding.UTF8.GetString(Convert.FromBase64String(col[1]));

                td._Receiver = Encoding.UTF8.GetString(Convert.FromBase64String(col[2]));

                td._Receiver = Encoding.UTF8.GetString(Convert.FromBase64String(col[3]));

                td._Time = Encoding.UTF8.GetString(Convert.FromBase64String(col[4]));

                td._IsSuccess = Encoding.UTF8.GetString(Convert.FromBase64String(col[5]));

                Add(td);

            }

           // Add(new TransactionData());
        }
        
    }

    class TransactionHistoryType
    {
        public string _ID;
        public string _Sender;
        public string _Receiver;
        public string _Amount;
        public string _Time;
        public string _IsSuccess;

        public string ID { get { return _ID; } }
        public string Sender { get { return _Sender; } }
        public string Receiver { get { return _Receiver; } }
        public string Amount { get { return _Amount; } }
        public string Time { get { return _Time; } }
        public string IsSuccess { get { return _IsSuccess; } }
        
        public TransactionHistoryType()
        {

        }

        public TransactionHistoryType(string ID, string Sender, string Receiver, string Amount, string Time, string IsSuccess)
        {
            _ID = ID;
            _Sender = Sender;
            _Receiver = Receiver;
            _Amount = Amount;
            _Amount = Amount;
            _Time = Time;
            _IsSuccess = IsSuccess;
        }
    }
}
