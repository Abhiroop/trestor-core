﻿
// @Author: Arpan Jati
// @Date: Aug 28, 2015

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using TNetD.Transactions;

namespace TNetD.UI
{
    public class DisplayMessageType : INotifyPropertyChanged
    {
        string text = string.Empty;
        Brush textColor = Brushes.Green;
        DisplayType displayType = DisplayType.Info;
        DateTime time;

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

        public DisplayMessageType()
        {
            this.text = string.Empty;
            this.textColor = textColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF8ACD3A"));// #FF8ACD3A 
            this.displayType = DisplayType.Info;
        }

        public DisplayMessageType(string text, Brush textColor, DisplayType displayType, DateTime time) : this()
        {
            this.text = text;
            this.textColor = textColor;
            this.displayType = displayType;
            this.time = time;
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

        public DateTime Time
        {
            get
            {
                return this.time;
            }

            set
            {
                if (value != this.time)
                {
                    this.time = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Brush TextColor
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

        public DisplayType DisplayType
        {
            get
            {
                return this.displayType;
            }

            set
            {
                if (value != this.displayType)
                {
                    this.displayType = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }

   


   
}
