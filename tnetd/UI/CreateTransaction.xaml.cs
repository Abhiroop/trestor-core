using Chaos.NaCl;
using Newtonsoft.Json;
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
using TNetD.Json.JS_Structs;
using TNetD.Transactions;

namespace TNetD.UI
{
    /// <summary>
    /// Interaction logic for CreateTransaction.xaml
    /// </summary>
    public partial class CreateTransaction : Window
    {
        public CreateTransaction()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] PubSrc;
                byte[] PrivExpanded;
                byte[] PrivSeed = HexUtil.GetBytes(tb_SenderPrivate.Text);
                
                byte[] PubDst = HexUtil.GetBytes(tb_DestPublic.Text);

                Ed25519.KeyPairFromSeed(out PubSrc, out PrivExpanded, PrivSeed);

                tb_SenderPublic.Text = HexUtil.ToString(PubSrc);

                long value = long.Parse(tb_Value.Text);
                long fee = long.Parse(tb_Fee.Text);

                SingleTransactionFactory stf = new SingleTransactionFactory(new Hash(PubSrc), new Hash(PubDst), fee, value);

                byte[] dd = stf.GetTransactionData();

                Hash sig = new Hash(Ed25519.Sign(dd, PrivExpanded));

                TransactionContent tc;

                if (stf.Create(sig, out tc) == TransactionProcessingResult.Accepted)
                {
                    if (check_Json.IsChecked.Value)
                    {
                        tb_TX_Hex.Text = JsonConvert.SerializeObject(new JS_TransactionReply(tc), Common.jss);
                    }
                    else
                    {
                        tb_TX_Hex.Text = HexUtil.ToString(tc.Serialize());
                    }
                }
                else
                {
                    tb_TX_Hex.Text = "INVALID DATA !!!";
                }
            }
            catch
            {
                tb_TX_Hex.Text = "Exception ocurred !!!";
            }

        }
    }
}
