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
using TNetD.Address;
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
                byte[] PrivSeedSender = HexUtil.GetBytes(tb_SenderPrivate.Text);  
                string SenderName = tb_SenderName.Text.Trim();

                AccountIdentifier identifierSrc = AddressFactory.PrivateKeyToAccount(PrivSeedSender, SenderName);
                
                byte[] PubSrc;
                byte[] PrivSrcExpanded;
                Ed25519.KeyPairFromSeed(out PubSrc, out PrivSrcExpanded, PrivSeedSender);
                
                /////////////////
                
                byte[] PrivSeedDest = HexUtil.GetBytes(tb_DestPrivate.Text);
                string DestName = tb_DestName.Text.Trim();

                AccountIdentifier identifierDest = AddressFactory.PrivateKeyToAccount(PrivSeedDest, DestName);
                
                long value = long.Parse(tb_Value.Text);
                long fee = long.Parse(tb_Fee.Text);

                SingleTransactionFactory stf = new SingleTransactionFactory(identifierSrc, identifierDest, fee, value);

                byte[] dd = stf.GetTransactionData();

                Hash sig = new Hash(Ed25519.Sign(dd, PrivSrcExpanded));

                TransactionContent tc;

                TransactionProcessingResult rslt = stf.Create(sig, out tc);

                if (rslt == TransactionProcessingResult.Accepted)
                {
                    if (check_Json.IsChecked.Value)
                    {
                        tb_TX_Hex.Text = JsonConvert.SerializeObject(new JS_TransactionReply(tc), Common.JSON_SERIALIZER_SETTINGS);
                    }
                    else
                    {
                        tb_TX_Hex.Text = HexUtil.ToString(tc.Serialize());
                    }
                }
                else
                {
                    tb_TX_Hex.Text = "INVALID DATA : " + rslt.ToString();
                }
            }
            catch
            {
                tb_TX_Hex.Text = "Exception ocurred !!!";
            }

        }

        private void btn_Rand_Click(object sender, RoutedEventArgs e)
        {
            byte [] _rand_1 = new byte[32];
            byte [] _rand_2 = new byte[32];

            Common.SECURE_RNG.GetBytes(_rand_1);
            Common.SECURE_RNG.GetBytes(_rand_2);

            tb_SenderPrivate.Text = HexUtil.ToString(_rand_1);
            tb_DestPrivate.Text = HexUtil.ToString(_rand_2);

            tb_Fee.Text = Common.NORMAL_RNG.Next(Common.FIN_MIN_TRANSACTION_FEE, Common.FIN_MIN_TRANSACTION_FEE * 2) + "";

            tb_Value.Text = Common.NORMAL_RNG.Next(1000, 50000000) + "";


        }
    }
}
