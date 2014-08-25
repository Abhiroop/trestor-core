using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
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
        public delegate void TextChangedHandler();
        public event TextChangedHandler PassTextChanged;

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

        Color passQuality = Colors.Transparent;

        public Color PasswordQuality
        {
            get
            {
                return passQuality;
            }
        }


        char _pChar = '●';

        bool _passBox = false;

        public char PasswordChar
        {
            get { return _pChar; }
            set
            {
                _pChar = value;
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
            get { return _passBox; }
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
                if (_passBox)
                    return BAD_ConvertToUnsecureString(passwordBox_Content.SecurePassword);
                else return textBox_Content.Text;
            }

            set
            {
                if (_passBox)
                    throw new Exception("Changing Passowrd not allowed.");
                else
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

        public int getNum(string st)
        {
            int counter = 0;
            for (int i = 0; i < st.Length; i++)
            {
                char c = st[i];
                if ((byte)c >= 48 && (byte)c <= 57)
                    counter++;
            }
            return counter;
        }

        public int getSpChar(string st)
        {
            int counter = 0;
            for (int i = 0; i < st.Length; i++)
            {
                char c = st[i];
                if ((byte)c >= 33 && (byte)c <= 47)
                    counter++;
            }
            return counter;
        }

        public static string BAD_ConvertToUnsecureString(SecureString securePassword)
        {
            IntPtr unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
            var s = Marshal.PtrToStringUni(unmanagedString);
            Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            return s;
        }

        private void passwordBox_Content_KeyUp(object sender, KeyEventArgs e)
        {
            SecureString _pass = passwordBox_Content.SecurePassword;

            string pass = BAD_ConvertToUnsecureString(_pass);

            if (pass.Length < 8)
            {
                passQuality = Colors.Red;
            }
            if (pass.Length > 8 && getNum(pass) < 1)
            {
                passQuality = Colors.OrangeRed;
            }

            if (pass.Length > 8 && getSpChar(pass) < 2)
            {
                passQuality = Colors.Orange;
            }

            if (pass.Length >= 8 && getSpChar(pass) >= 1 && getNum(pass) > 1)
            {
                passQuality = Colors.Green;
            }

            if (PassTextChanged != null) PassTextChanged();
        }
    }
}