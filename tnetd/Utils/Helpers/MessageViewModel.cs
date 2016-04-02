
// @Author: Arpan Jati
// @Date: 30 August 2015

using System.Collections.ObjectModel;
using TNetD.Transactions;
using TNetD.UI;

namespace TNetD.Helpers
{
    class MessageViewModel
    {
        ObservableCollection<DisplayMessageType> logMessages = new ObservableCollection<DisplayMessageType>();
        ObservableCollection<DisplayMessageType> statusMessages = new ObservableCollection<DisplayMessageType>();
        
        public ObservableCollection<DisplayMessageType> LogMessages
        {
            get
            {
                return logMessages;
            }
        }

        public ObservableCollection<DisplayMessageType> StatusMessages
        {
            get
            {
                return statusMessages;
            }
        }

        public void ProcessSkips()
        {
            try {

                if (statusMessages.Count > 2000)
                {
                    for (int i = 0; i < 200; i++)
                        statusMessages.RemoveAt(0);                    
                }

                if (logMessages.Count > 2000)
                {
                    for (int i = 0; i < 200; i++)
                        logMessages.RemoveAt(0);
                }
            }
            catch { }              
        }
    }
    
    class TransactionViewModel
    {
        ObservableCollection<DisplayLedgerCloseType> lcsData = new ObservableCollection<DisplayLedgerCloseType>();
        ObservableCollection<DisplayTransactionContentType> txData = new ObservableCollection<DisplayTransactionContentType>();

        public void Clear()
        {
            lcsData.Clear();
            txData.Clear();
        }

        public ObservableCollection<DisplayLedgerCloseType> LedgerCloseData
        {
            get
            {
                return lcsData;
            }
        }

        public ObservableCollection<DisplayTransactionContentType> TransactionData
        {
            get
            {
                return txData;
            }
        }       
    }

    class AccountViewModel
    {
        ObservableCollection<DisplayAccountInfoType> accountData = new ObservableCollection<DisplayAccountInfoType>();

        public void Clear()
        {
            accountData.Clear();           
        }

        public ObservableCollection<DisplayAccountInfoType> Accounts
        {
            get
            {
                return accountData;
            }
        }
    }
}
