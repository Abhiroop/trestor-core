
// @Author: Arpan Jati
// @Date: March 20, 2016

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace TNetD.UI
{
    public class DisplayLedgerCloseType : INotifyPropertyChanged
    {
        string text = string.Empty;
        long transactions = 0;
        long totalTransactions = 0;
        
        Brush textColor = Brushes.Green;
        DisplayType displayType = DisplayType.Info;
        DateTime time;

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

        public DisplayLedgerCloseType()
        {
            text = string.Empty;
            textColor = textColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF8ACD3A"));// #FF8ACD3A
            displayType = DisplayType.Info;
        }

        public DisplayLedgerCloseType(LedgerCloseData lcd) : this()
        {
            /*public long SequenceNumber;
                public byte[] LedgerHash;
                public long Transactions;
                public long TotalTransactions;
                public long CloseTime;*/

            sequenceNumber = lcd.SequenceNumber;
            ledgerHash = lcd.LedgerHash;

            this.text = "" + new Hash(lcd.LedgerHash).ToString() ;
            this.transactions = lcd.Transactions;
            this.totalTransactions = lcd.TotalTransactions;

            // this.textColor = textColor;
            // this.displayType = displayType;
            this.time = DateTime.FromFileTimeUtc(lcd.CloseTime);
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

        public string SequenceNumber
        {
            get
            {
                return "SEQ: " + sequenceNumber + " | " + totalTransactions + "";
            }
        }

        public string Transactions
        {
            get
            {
                return "TX: "+transactions + "";
            }
        }

        public string TotalTransactions
        {
            get
            {
                return "TOT: " + totalTransactions + "";
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
