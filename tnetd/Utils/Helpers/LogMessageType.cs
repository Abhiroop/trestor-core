
// @Author: Arpan Jati
// @Date: Aug 28, 2015

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace TNetD.Helpers
{
    class LogMessageType : INotifyPropertyChanged
    {      
        string text = string.Empty;
        Color textColor = Colors.Green;
        DisplayType messageType = DisplayType.Info;

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property. 
        // The CallerMemberName attribute that is applied to the optional propertyName 
        // parameter causes the property name of the caller to be substituted as an argument. 
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        // The constructor is private to enforce the factory pattern. 
        private LogMessageType()
        {
            text = string.Empty;
            textColor = Colors.Green;
            messageType = DisplayType.Info;
        }
             
        public string Text
        {
            get
            {
                return this.text;
            }

            set
            {
                if (value != this.text)
                {
                    this.text = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Color TextColor
        {
            get
            {
                return this.textColor;
            }

            set
            {
                if (value != this.textColor)
                {
                    this.textColor = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public DisplayType MessageType
        {
            get
            {
                return this.messageType;
            }

            set
            {
                if (value != this.messageType)
                {
                    this.messageType = value;
                    NotifyPropertyChanged();
                }
            }
        }

        
    }
}
