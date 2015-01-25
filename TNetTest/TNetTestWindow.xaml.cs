using Chaos.NaCl;
using Grapevine.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using TNetD;
using TNetD.Address;
using TNetD.Json.JS_Structs;
using TNetD.Transactions;

namespace TNetTest
{
    /// <summary>
    /// Interaction logic for TNetTestWindow.xaml
    /// </summary>
    public partial class TNetTestWindow : Window
    {
        RESTClient client = new RESTClient("http://localhost:2015");

        List<GenesisAccountData> GAD = new List<GenesisAccountData>();

        public TNetTestWindow()
        {
            InitializeComponent();

            Common.Initialize();

            RESTResponse response = client.Execute(new RESTRequest("info"));
            textBlock_StatusLog.Text += "" + response.Content;

            GenesisFileParser gfp = new GenesisFileParser("ACCOUNTS.GEN_SECRET");
            gfp.GetAccounts(out GAD);
        }
        
        void SentTransactionRequest(GenesisAccountData Src, GenesisAccountData Dest)
        {
            byte[] _rand_1 = new byte[32];
            byte[] _rand_2 = new byte[32];

            Common.rngCsp.GetBytes(_rand_1);
            Common.rngCsp.GetBytes(_rand_2);

            long fee = 10000; // Common.random.Next(Common.NETWORK_Min_Transaction_Fee, Common.NETWORK_Min_Transaction_Fee * 2);
            long value = Common.random.Next(1000, 50000000);

            byte[] PrivSeedSender = Src.RandomPrivate;
            string SenderName = Src.Name;

            AccountIdentifier identifierSrc = AddressFactory.PrivateKeyToAccount(PrivSeedSender, SenderName);

            byte[] PubSrc;
            byte[] PrivSrcExpanded;
            Ed25519.KeyPairFromSeed(out PubSrc, out PrivSrcExpanded, PrivSeedSender);

            /////////////////

            byte[] PrivSeedDest = Dest.RandomPrivate;
            string DestName = Dest.Name;

            AccountIdentifier identifierDest = AddressFactory.PrivateKeyToAccount(PrivSeedDest, DestName);

            SingleTransactionFactory stf = new SingleTransactionFactory(identifierSrc, identifierDest, fee, value);

            byte[] dd = stf.GetTransactionData();

            Hash sig = new Hash(Ed25519.Sign(dd, PrivSrcExpanded));

            TransactionContent tc;

            TransactionProcessingResult rslt = stf.Create(sig, out tc);

            if (rslt == TransactionProcessingResult.Accepted)
            {
                string SER_DATA = JsonConvert.SerializeObject(new JS_TransactionReply(tc), Common.JsonSerializerSettings);

                textBlock_StatusLog.Text += "\nSending:" + SER_DATA;

                RESTRequest request = new RESTRequest("propagate", Grapevine.HttpMethod.POST, Grapevine.ContentType.JSON);

                request.Payload = SER_DATA;

                RESTResponse response = client.Execute(request);

                textBlock_StatusLog.Text += "\n" + response.Content + "\nTime:" + response.ElapsedTime + " (ms)\n";
            }
            else
            {
                textBlock_StatusLog.Text += "\n INVALID DATA : " + rslt.ToString();
            }
        }
        
        private void button_TransactionsStart_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch sw = new Stopwatch();

            for (int i = 0; i < 100; i++)
            {
                // TEST CODE
                int[] sdIndex = Utils.GenerateNonRepeatingDistribution(GAD.Count, 2);

                //sdIndex[0] = 6250;

                SentTransactionRequest(GAD[sdIndex[0]], GAD[sdIndex[1]]);


                //RESTResponse response = client.Execute(new RESTRequest("info"));
                //textBlock_StatusLog.Text += "\n" + response.Content + "\nTime:" + response.ElapsedTime + " (ms)\n";
            }

        }
    }
}
