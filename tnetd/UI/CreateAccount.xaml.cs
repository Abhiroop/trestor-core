using Chaos.NaCl;
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
using System.Windows.Shapes;
using TNetD.Address;

namespace TNetD.UI
{
    /// <summary>
    /// Interaction logic for CreateAccount.xaml
    /// </summary>
    public partial class CreateAccount : Window
    {
        public CreateAccount()
        {
            InitializeComponent();
        }

        AddressFactory af = new AddressFactory();

        private void tb_PrivateRandom_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                byte[] _pr = HexUtil.GetBytes(tb_PrivateRandom.Text);

                byte[] PK;
                byte[] EPK;

                Ed25519.KeyPairFromSeed(out PK, out EPK, _pr);

                tb_Public.Text = HexUtil.ToString(PK);
                tb_ExpandedPrivate.Text = HexUtil.ToString(EPK);
            }
            catch
            {
                tb_Public.Text = "Exception !!!";
                tb_ExpandedPrivate.Text = "Exception !!!";
                tb_Address.Text = "Exception !!!";
            }

            try
            {
                byte[] PK = HexUtil.GetBytes(tb_Public.Text);

                byte[] Address = af.GetAddress(PK, tb_Name.Text);

                tb_Address.Text = AddressFactory.GetAddressString(Address);
            }
            catch
            {                
                tb_Address.Text = "Exception !!!";
            }            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // GEN.
            byte [] _pr = new byte[32];
            Common.rngCsp.GetBytes(_pr);

            tb_PrivateRandom.Text = HexUtil.ToString(_pr);
        }
    }
}
