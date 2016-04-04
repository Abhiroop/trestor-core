
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
        static Brush greenBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#1F00c87c"));
        static Brush pinkBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#1Fbf384f"));

        static Brush greenTextBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF00c87c"));
        static Brush pinkTextBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFbf384f"));

        static Brush yellowBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#2FFFD700"));
        static Brush lawnGreen_Brush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF62E515"));

        string text = string.Empty;
        Brush textColor = lawnGreen_Brush;
        DisplayType displayType = DisplayType.Info;
        DateTime time;

        public bool IsSource = false;

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
            displayType = DisplayType.Info;
        }

        public DisplayTransactionContentType(TransactionContent tc) : this()
        {
            TransactionContent = tc;

            text = tc.GetSourcesString() + " => " + tc.GetDestinationsString();
            
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
                if (IsSource)
                {
                    return pinkTextBrush;
                }
                else
                {
                    return greenTextBrush;
                }
            }
        }

        public string Money
        {
            get
            {
                return MoneyTools.GetMoneyDisplayString(TransactionContent.Value);
            }
        }

        public Brush MoneyColor
        {
            get
            {
                return MoneyTools.GetMoneyDisplayColor(TransactionContent.Value);
            }
        }

        public Brush BackgroundColor
        {
            get
            {
                if (IsSource)
                {
                    return pinkBrush;
                }
                else
                {
                    return greenBrush;
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
