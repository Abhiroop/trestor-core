using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetWallet.WalletOperations
{
    class TransactionDataSource : ObservableCollection<TransactionData>
    {
        public TransactionDataSource(string incomingTransactionHistory) : base()
        {     
            //get all rows
            string[] rows = incomingTransactionHistory.Split('|');

            for(int i = 0; i<rows.Length; i++)
            {
                 TransactionData td = new TransactionData();
                //get columns here
                string[] col = rows[i].Split(':');

                td.ID = Encoding.UTF8.GetString(Convert.FromBase64String(col[0]));

                td.Sender = Encoding.UTF8.GetString(Convert.FromBase64String(col[1]));

                td.Receiver = Encoding.UTF8.GetString(Convert.FromBase64String(col[2]));

                td.Receiver = Encoding.UTF8.GetString(Convert.FromBase64String(col[3]));

                td.Time = Encoding.UTF8.GetString(Convert.FromBase64String(col[4]));

                td.IsSuccess = Encoding.UTF8.GetString(Convert.FromBase64String(col[5]));

                Add(td);

            }

           // Add(new TransactionData());
        }
        
    }

    class TransactionData
    {  
        public string ID { get; set;}
        public string Sender { get; set;}
        public string Receiver { get; set;}
        public string Amount { get; set;}
        public string Time { get; set;}
        public string IsSuccess { get; set;}
    }
}
