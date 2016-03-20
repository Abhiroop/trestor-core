
// @Author: Arpan Jati
// @Date: March 20, 2016

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using TNetD.Transactions;

namespace TNetD.UI
{
    public class DisplayTransactionContentType : INotifyPropertyChanged
    {
        string text = string.Empty;
        Brush textColor = Brushes.Green;
        DisplayType displayType = DisplayType.Info;
        DateTime time;
        
        public TransactionContent TransactionContent = default(TransactionContent);

        public long sequenceNumber = 0;
        public byte[] ledgerHash = new byte[0];

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property. 
        // The CallerMemberName attribute that is applied to the optional propertyName 
        // parameter causes the property name of the caller to be substituted as an argument. 
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public DisplayTransactionContentType()
        {
            text = string.Empty;
            textColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF8ACD3A"));// #FF8ACD3A
            displayType = DisplayType.Info;
        }

        public DisplayTransactionContentType(TransactionContent tc) : this()
        {
            TransactionContent = tc;

            text = tc.TransactionID.ToString().Substring(0, 8) + "\r\n" + (tc.Value/1000000.0).ToString("0.00000");
            //this.transactions = lcd.Transactions;
            //this.totalTransactions = lcd.TotalTransactions;
            // this.textColor = textColor;
            // this.displayType = displayType;
            time = tc.DateTime;
        }

        public string Text
        {
            get
            {
                return text;
            }

            set
            {
                if (value != text)
                {
                    text = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public DateTime Time
        {
            get
            {
                return time;
            }

            set
            {
                if (value != time)
                {
                    time = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Brush TextColor
        {
            get
            {
                return textColor;
            }

            set
            {
                if (value != textColor)
                {
                    textColor = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public DisplayType DisplayType
        {
            get
            {
                return displayType;
            }

            set
            {
                if (value != displayType)
                {
                    displayType = value;
                    NotifyPropertyChanged();
                }
            }
        }


    }
}
