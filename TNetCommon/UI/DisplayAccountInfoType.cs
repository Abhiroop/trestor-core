
// @Author: Arpan Jati
// @Date: April 2, 2016

using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using TNetD.Transactions;

namespace TNetD.UI
{
    public class DisplayAccountInfoType : INotifyPropertyChanged
    {
        static Brush backBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#1F8ACD3A"));
        static Brush blueBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#1F0081c9"));
        static Brush greenBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#1F00c87c"));
        static Brush pinkBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#1Fbf384f"));
        static Brush yellowBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#2FFFD700"));
        
        public AccountInfo AccountInfo = default(AccountInfo);

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

        public DisplayAccountInfoType()
        {

        }
        
        public DisplayAccountInfoType(AccountInfo accountInfo) : this()
        {
            this.AccountInfo = accountInfo;
        }

        public string PublicKey
        {
            get { return AccountInfo.PublicKey.ToString(); }
        }

        public string Money
        {
            get
            {
                return MoneyTools.GetMoneyDisplayString (AccountInfo.Money);
            }
        }

        public Brush MoneyColor
        {
            get
            {
                return MoneyTools.GetMoneyDisplayColor(AccountInfo.Money);
            }
        }

        public string Name
        {
            get { return AccountInfo.Name; }
        }

        public string Address
        {
            get { return AccountInfo.GetAddress(); }
        }

        public string AccountState
        {
            get { return AccountInfo.AccountState.ToString(); }
        }

        public string NetworkType
        {
            get { return AccountInfo.NetworkType.ToString(); }
        }

        public string AccountType
        {
            get { return AccountInfo.AccountType.ToString(); }
        }

        public string Time
        {
            get
            {
                var dt = DateTime.FromFileTimeUtc(AccountInfo.LastTransactionTime);

                return dt.ToShortDateString() + " (" + (int)((DateTime.UtcNow - dt).TotalDays) + " days)";
            }
        }

        public Brush BackgroundColor
        {
            get
            {
                if(AccountInfo.Money >= 10000000000 && !(AccountInfo.AccountType == TNetD.Address.AccountType.MainGenesis ||
                    AccountInfo.AccountType == TNetD.Address.AccountType.TestGenesis) )
                {
                    return yellowBrush;
                }

                if (AccountInfo.AccountType == TNetD.Address.AccountType.MainGenesis ||
                    AccountInfo.AccountType == TNetD.Address.AccountType.TestGenesis)
                {
                    return greenBrush;
                }
                else if (AccountInfo.AccountType == TNetD.Address.AccountType.MainValidator ||
                    AccountInfo.AccountType == TNetD.Address.AccountType.TestValidator)
                {
                    return pinkBrush;
                }
                else if (AccountInfo.AccountType == TNetD.Address.AccountType.MainNormal ||
                    AccountInfo.AccountType == TNetD.Address.AccountType.TestNormal)
                {
                    return blueBrush;
                }
                else
                {
                    return backBrush;
                }
            }
        }
    }
}
