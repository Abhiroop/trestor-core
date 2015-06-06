using Chaos.NaCl;
using Grapevine.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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
using TNetD.Crypto;
using TNetD.Json.JS_Structs;
using TNetD.Protocol;
using TNetD.Transactions;
using TNetNative;

namespace TNetTest
{
    /// <summary>
    /// Interaction logic for TNetTestWindow.xaml
    /// </summary>
    public partial class TNetTestWindow : Window
    {
        //RESTClient client = new RESTClient("http://54.69.239.153:2015");
        RESTClient client = new RESTClient("http://localhost:2015");
        //RESTClient client = new RESTClient("http://54.200.152.214:2015"); // LIVE

        List<GenesisAccountData> GAD = new List<GenesisAccountData>();

        Queue<string> Log = new Queue<string>();

        void WriteLog(string data)
        {
            Log.Enqueue(data);
        }

        void WriteLog_Internal(string data)
        {
            this.Dispatcher.Invoke(new Action(delegate
            {
                textBlock_StatusLog.Text += "\n" + data;
                if (textBlock_StatusLog.Text.Length > Common.UI_TextBox_Max_Length)
                    textBlock_StatusLog.Text = "";
            }));
        }

        public TNetTestWindow()
        {
            InitializeComponent();

            Common.Initialize();

            GenesisFileParser gfp = new GenesisFileParser("ACCOUNTS.GEN_SECRET");
            gfp.GetAccounts(out GAD);

            Timer tmr = new Timer();
            tmr.Interval = 100;
            tmr.Elapsed += tmr_Elapsed;
            tmr.Start();

            PrivateKeyStore pk = new PrivateKeyStore();
        }

        void tmr_Elapsed(object sender, ElapsedEventArgs e)
        {
            while (Log.Count > 0)
            {
                WriteLog_Internal(Log.Dequeue());
            }
        }

        async Task<Hash> SentTransactionRequest(GenesisAccountData Src, GenesisAccountData Dest)
        {
            byte[] _rand_1 = new byte[32];
            byte[] _rand_2 = new byte[32];

            Common.rngCsp.GetBytes(_rand_1);
            Common.rngCsp.GetBytes(_rand_2);

            long fee = 0; // Common.random.Next(Common.NETWORK_Min_Transaction_Fee, Common.NETWORK_Min_Transaction_Fee * 2);
            long value = /*(long)(10000000000000L * Common.random.NextDouble());*/  Common.random.Next(1000, 50000000);

            byte[] PrivSeedSender = Src.RandomPrivate;
            string SenderName = Src.Name;

            AccountIdentifier identifierSrc = AddressFactory.PrivateKeyToAccount(PrivSeedSender, SenderName, NetworkType.TestNet, AccountType.TestGenesis);

            byte[] PubSrc;
            byte[] PrivSrcExpanded;
            Ed25519.KeyPairFromSeed(out PubSrc, out PrivSrcExpanded, PrivSeedSender);

            /////////////////

            byte[] PrivSeedDest = Dest.RandomPrivate;
            string DestName = Dest.Name;

            AccountIdentifier identifierDest = AddressFactory.PrivateKeyToAccount(PrivSeedDest, DestName, NetworkType.TestNet, AccountType.TestGenesis);

            SingleTransactionFactory stf = new SingleTransactionFactory(identifierSrc, identifierDest, fee, value);

            byte[] dd = stf.GetTransactionData();

            Hash sig = new Hash(Ed25519.Sign(dd, PrivSrcExpanded));

            TransactionContent tc;

            TransactionProcessingResult rslt = stf.Create(sig, out tc);

            if (rslt == TransactionProcessingResult.Accepted)
            {
                TaskFactory tf = new TaskFactory();

                await tf.StartNew(new Action(delegate
                {

                    string SER_DATA = JsonConvert.SerializeObject(new JS_TransactionReply(tc), Common.JsonSerializerSettings);

                    WriteLog("\nSending:" + SER_DATA);

                    RESTRequest request = new RESTRequest("propagate", Grapevine.HttpMethod.POST, Grapevine.ContentType.JSON);

                    request.Payload = SER_DATA;

                    RESTResponse response = client.Execute(request);

                    WriteLog(response.Content + "\nTime:" + response.ElapsedTime + " (ms)\n");



                }));

            }
            else
            {
                WriteLog("INVALID DATA : " + rslt.ToString());
            }

            return (tc != null) ? tc.TransactionID : new Hash();
        }

        List<Hash> TX_IDs = new List<Hash>();

        private async void button_TransactionsStart_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch sw = new Stopwatch();

            TX_IDs.Clear();

            int Count = int.Parse(textBox_Transactions_TransactionCount.Text);

            for (int i = 0; i < Count; i++)
            {
                try
                {
                    // TEST CODE
                    int[] sdIndex = Utils.GenerateNonRepeatingDistribution(GAD.Count, 2);

                    await SentTransactionRequest(GAD[sdIndex[0]], GAD[sdIndex[1]]);

                    //await Task.Delay(500); 

                    //sdIndex[0] = 6250;
                    //TX_IDs.Add(SentTransactionRequest(GAD[sdIndex[0]], GAD[sdIndex[1]]));

                    // RESTResponse response = client.Execute(new RESTRequest("info"));
                    // textBlock_StatusLog.Text += "\n" + response.Content + "\nTime:" + response.ElapsedTime + " (ms)\n";
                }
                catch (Exception ex)
                {
                    WriteLog("TransactionsStart():" + ex.Message);
                }
            }
        }

        private void button_TransactionsVerify_Click(object sender, RoutedEventArgs e)
        {
            /*RESTRequest request = new RESTRequest("request", Grapevine.HttpMethod.GET);

            RESTResponse response = client.Execute(request);

            string json = response.Content;
            
            WriteLog("\n ACC_WORK:" + json + " \n");

            JS_Resp_WorkProofRequest_Outer jwr_ = JsonConvert.DeserializeObject<JS_Resp_WorkProofRequest_Outer>(json, Common.JsonSerializerSettings);

            JS_WorkProofRequest jwr = jwr_.Data;

            byte[] Proof = WorkProof.CalculateProof(jwr.ProofRequest, jwr.Difficulty);

            byte[] AID = new byte[8];

            Common.rngCsp.GetBytes(AID);

            var acc = AddressFactory.CreateNewAccount( "arpan" );

            JS_AccountRegisterRequest jarr = new JS_AccountRegisterRequest(acc.Item1.PublicKey, acc.Item1.Name,
                acc.Item1.AddressData.AddressString, jwr.ProofRequest, Proof);

            RESTRequest registerRequest = new RESTRequest("register", Grapevine.HttpMethod.POST);

            registerRequest.ContentType = Grapevine.ContentType.JSON;
            registerRequest.Payload = JsonConvert.SerializeObject(jarr, Common.JsonSerializerSettings);

            WriteLog("\n PAYLOAD:" + registerRequest.Payload + " \n");

            RESTResponse acc_resp = client.Execute(registerRequest);
            
            WriteLog("\n ACC_RESPONSE:" + acc_resp.Content + " \n");*/


            /*foreach (Hash txid in TX_IDs)
            {
                WriteLog("Sending TxStatus:" + txid.ToString());

                RESTRequest request = new RESTRequest("txstatus", Grapevine.HttpMethod.GET);

                request.AddQuery("id", txid.ToString());

                RESTResponse response = client.Execute(request);

                WriteLog(response.Content + "\nTime:" + response.ElapsedTime + " (ms)\n");

            }*/


            
            byte[] PubSrc;
            byte[] PrivSrcExpanded;
            Ed25519.KeyPairFromSeed(out PubSrc, out PrivSrcExpanded, HexUtil.GetBytes("<PUT PRIVATE KEY HERE>"));

            AccountIdentifier identifierSrc = AddressFactory.PublicKeyToAccount(PubSrc, "<PUT SENDER NAME HERE>", NetworkType.MainNet, AccountType.MainGenesis);
            
            byte[] PK_dest = HexUtil.GetBytes("<PK Destination>");

            AccountIdentifier identifierDest = AddressFactory.PublicKeyToAccount(PK_dest, "<Destination Name>", NetworkType.MainNet, AccountType.MainNormal);

            SingleTransactionFactory stf = new SingleTransactionFactory(identifierSrc, identifierDest, 0, 1000000000);

            byte[] dd = stf.GetTransactionData();

            Hash sig = new Hash(Ed25519.Sign(dd, PrivSrcExpanded));

            TransactionContent tc;

            TransactionProcessingResult rslt = stf.Create(sig, out tc);

            if (rslt == TransactionProcessingResult.Accepted)
            {
                TaskFactory tf = new TaskFactory();
            
                string SER_DATA = JsonConvert.SerializeObject(new JS_TransactionReply(tc), Common.JsonSerializerSettings);

                WriteLog("\nSending:" + SER_DATA);

                RESTRequest request = new RESTRequest("propagate", Grapevine.HttpMethod.POST, Grapevine.ContentType.JSON);

                request.Payload = SER_DATA;

                RESTResponse response = client.Execute(request);

                WriteLog(response.Content + "\nTime:" + response.ElapsedTime + " (ms)\n");


            }
            else
            {
                WriteLog("INVALID DATA : " + rslt.ToString());
            }

            //Name - shila

            //Address - TNpsWwc1H6SuSXEmQLUn2SyTe328xZh2crL

            //Public - 9c263b6310eac539462818484d8db6c828ec6efb4b1d5951d9e5dc036568fdea
            /*
            bool OK = AddressFactory.VerfiyAddress("TNTBzsiitLGe7cN4HDBDjUTVi6Tp3SkVMKb",
                HexUtil.GetBytes("062160b53d81fd7b49a18842e8522e89d864d351fe2a8841369a8d782198d5da"), "ashish");
            
            WriteLog("OKAY:" + OK);*/

        }

        private void button_MEM_HARD_Start_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = new byte[64];
            ReturnTypes ret = Rig2.Compute(ref data, new byte[16], new byte[10], 8, 10);

            WriteLog("\nHASH:" + HexUtil.ToString(data));
        }

        private void button_TransactionsInitialize_Click(object sender, RoutedEventArgs e)
        {
            RESTResponse response = client.Execute(new RESTRequest("info"));
            WriteLog(response.Content);
        }

        private void menuItem_Benchmark_Click(object sender, RoutedEventArgs e)
        {
            Benchmarks bm = new Benchmarks();

            bm.Show();
        }

        private void menuItem_File_TestVarint_Click(object sender, RoutedEventArgs e)
        {
            //Varint2.TestSingle();
        }

    }
}
