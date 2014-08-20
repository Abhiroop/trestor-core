using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TNetWallet
{
    /// <summary>
    /// Interaction logic for TitledTextBox.xaml
    /// </summary>
    public partial class TitledTextBox : UserControl
    {
        public string TitleText
        {
            get
            {
                return textBlock_Title.Text;
            }

            set
            {
                textBlock_Title.Text = value;
            }
        }

        char _pChar = '●';

        bool _passBox = false;

        public char PasswordChar
        {
            get { return _pChar; }
            set { _pChar = value;
            passwordBox_Content.PasswordChar = _pChar;
            }
        }

        void DoTypeThingy()
        {
            if (_passBox)
            {
                textBox_Content.Visibility = System.Windows.Visibility.Hidden;
                passwordBox_Content.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                textBox_Content.Visibility = System.Windows.Visibility.Visible;
                passwordBox_Content.Visibility = System.Windows.Visibility.Hidden;
            }
        }
               
        public bool IsPasswordBox
        {
            get { return _passBox;}
            set
            {
                _passBox = value;
                DoTypeThingy();
            }
        }

        public string Text
        {
            get
            {
                return textBox_Content.Text;
            }

            set
            {
                textBox_Content.Text = value;
            }
        }

        public TitledTextBox()
        {
            InitializeComponent();
            DoTypeThingy();
        }

        private void textBox_Content_GotFocus(object sender, RoutedEventArgs e)
        {
            textBlock_Title.Foreground = (SolidColorBrush)Application.Current.Resources["TrestorGreenBrush"];
        }

        private void textBox_Content_LostFocus(object sender, RoutedEventArgs e)
        {
            textBlock_Title.Foreground = (SolidColorBrush)Application.Current.Resources["TrestorGrayBrush"];
        }       
    }
}
