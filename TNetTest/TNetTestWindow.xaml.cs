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
using TNetD.Tree;
using TNetD.UI;
using TNetNative;

namespace TNetTest
{
    /// <summary>
    /// Interaction logic for TNetTestWindow.xaml
    /// </summary>
    public partial class TNetTestWindow : Window
    {
        //RESTClient client = new RESTClient("http://54.69.239.153:2015");
        //RESTClient client = new RESTClient("http://localhost:2015");
        //RESTClient client = new RESTClient("http://54.200.152.214:2015"); // LIVE
        RESTClient client = new RESTClient("http://localhost:44711");

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

            DisplayUtils.DisplayText += DisplayUtils_DisplayText;

            GenesisFileParser gfp = new GenesisFileParser("ACCOUNTS.GEN_SECRET");
            gfp.GetAccounts(out GAD);

            Timer tmr = new Timer();
            tmr.Interval = 100;
            tmr.Elapsed += tmr_Elapsed;
            tmr.Start();

            PrivateKeyStore pk = new PrivateKeyStore();
        }

        private void DisplayUtils_DisplayText(DisplayMessageType displayMessage)
        {
            if (displayMessage.DisplayType >= DisplayType.Debug)
            {
                try
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        displayMessage.Text = displayMessage.Text.Trim();

                        WriteLog( displayMessage.Time.ToLongTimeString() + 
                            " ["+displayMessage.DisplayType + "]: " + displayMessage.Text);
                    }));
                }
                catch { }
            }
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

            Common.SECURE_RNG.GetBytes(_rand_1);
            Common.SECURE_RNG.GetBytes(_rand_2);

            long fee = 0; // Common.random.Next(Common.NETWORK_Min_Transaction_Fee, Common.NETWORK_Min_Transaction_Fee * 2);
            long value = /*(long)(10000000000000L * Common.random.NextDouble());*/  Common.NORMAL_RNG.Next(1000, 50000000);

            byte[] PrivSeedSender = Src.RandomPrivate;
            string SenderName = Src.Name;

            AccountIdentifier identifierSrc = AddressFactory.PrivateKeyToAccount(PrivSeedSender, SenderName, 
                NetworkType.TestNet, AccountType.TestGenesis);

            byte[] PubSrc;
            byte[] PrivSrcExpanded;
            Ed25519.KeyPairFromSeed(out PubSrc, out PrivSrcExpanded, PrivSeedSender);

            /////////////////

            byte[] PrivSeedDest = Dest.RandomPrivate;
            string DestName = Dest.Name;

            AccountIdentifier identifierDest = AddressFactory.PrivateKeyToAccount(PrivSeedDest, DestName, 
                NetworkType.TestNet, AccountType.TestGenesis);

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

                    string SER_DATA = JsonConvert.SerializeObject(new JS_TransactionReply(tc), Common.JSON_SERIALIZER_SETTINGS);

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

            JS_Resp_WorkProofRequest_Outer jwr_ = JsonConvert.DeserializeObject<JS_Resp_WorkProofRequest_Outer>(json, 
                                                    Common.JsonSerializerSettings);

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

            string priv_key = "<SENDER PRIVATE>";
            string sender_name = "<SENDER NAME>";

            byte[] PubSrc;
            byte[] PrivSrcExpanded;
            Ed25519.KeyPairFromSeed(out PubSrc, out PrivSrcExpanded, HexUtil.GetBytes(priv_key));
  
            AccountIdentifier identifierSrc = AddressFactory.PublicKeyToAccount(PubSrc, sender_name, 
                                                NetworkType.MainNet, AccountType.MainGenesis);

            byte[] PK_dest = HexUtil.GetBytes("<DEST PUBLIC>");

            AccountIdentifier identifierDest = AddressFactory.PublicKeyToAccount(PK_dest, "<DEST NAME>", 
                                                NetworkType.MainNet, AccountType.MainNormal);

            SingleTransactionFactory stf = new SingleTransactionFactory(identifierSrc, identifierDest, 0, 100000);

            byte[] dd = stf.GetTransactionData();

            Hash sig = new Hash(Ed25519.Sign(dd, PrivSrcExpanded));

            TransactionContent tc;

            TransactionProcessingResult rslt = stf.Create(sig, out tc);

            if (rslt == TransactionProcessingResult.Accepted)
            {
                TaskFactory tf = new TaskFactory();
            
                string SER_DATA = JsonConvert.SerializeObject(new JS_TransactionReply(tc), Common.JSON_SERIALIZER_SETTINGS);

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

        private void menuItem_File_TestTree_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void menuItem_File_Test_TimeTests_Click(object sender, RoutedEventArgs e)
        {
            TimeTests tt = new TimeTests();
            tt.Print += Tt_Print;
            tt.Execute();               
        }

        private void Tt_Print(string value)
        {            
            textBlock_StatusLog.Text += "\n" + value;
        }

        private void singleTX_Execute_Click(object sender, RoutedEventArgs e)
        {
            var _client = new RESTClient(singleTX_Client.Text);

            string priv_key = singleTX_SenderPrivate.Text;
            string sender_name = singleTX_SenderName.Text;

            string pub_dest = singleTX_DestPublic.Text;
            string dest_name = singleTX_DestName.Text;

            string amt = singleTX_Amount.Text;
            long value = long.Parse(amt);

            byte[] PubSrc;
            byte[] PrivSrcExpanded;
            Ed25519.KeyPairFromSeed(out PubSrc, out PrivSrcExpanded, HexUtil.GetBytes(priv_key));

            AccountIdentifier identifierSrc = AddressFactory.PublicKeyToAccount(PubSrc, sender_name, 
                                                NetworkType.MainNet, AccountType.MainGenesis);

            byte[] PK_dest = HexUtil.GetBytes(pub_dest);

            AccountIdentifier identifierDest = AddressFactory.PublicKeyToAccount(PK_dest, dest_name, 
                                                NetworkType.MainNet, AccountType.MainNormal);

            SingleTransactionFactory stf = new SingleTransactionFactory(identifierSrc, identifierDest, 0, value);

            byte[] dd = stf.GetTransactionData();

            Hash sig = new Hash(Ed25519.Sign(dd, PrivSrcExpanded));

            TransactionContent tc;

            TransactionProcessingResult rslt = stf.Create(sig, out tc);

            if (rslt == TransactionProcessingResult.Accepted)
            {
                TaskFactory tf = new TaskFactory();

                string SER_DATA = JsonConvert.SerializeObject(new JS_TransactionReply(tc), Common.JSON_SERIALIZER_SETTINGS);

                WriteLog("\nSending:" + SER_DATA);

                RESTRequest request = new RESTRequest("propagate", Grapevine.HttpMethod.POST, Grapevine.ContentType.JSON);

                request.Payload = SER_DATA;

                RESTResponse response = _client.Execute(request);

                WriteLog(response.Content + "\nTime:" + response.ElapsedTime + " (ms)\n");
            }
            else
            {
                WriteLog("INVALID DATA : " + rslt.ToString());
            }
        }

        // ///////////////////////////////////////////////////////////////////////////////  

        private void menuItem_Simulation_Start_Click(object sender, RoutedEventArgs e)
        {
            List<Hash> accounts = new List<Hash>();
            SortedDictionary<Hash, int> hh = new SortedDictionary<Hash, int>();

            byte[] N_H = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
                16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 };

            ListHashTree lht = new ListHashTree();

            int ACCOUNTS = 1000;

            long taka = 0;

            DisplayUtils.Display("Adding ... ", DisplayType.Debug);

            for (int i = 0; i < ACCOUNTS; i++)
            {
                N_H[0] = (byte)(i);

                Common.SECURE_RNG.GetBytes(N_H);

                accounts.Add(new Hash(N_H));

                long _taks = Common.NORMAL_RNG.Next(0, 1000000000);

                taka += _taks;

                AccountInfo ai = new AccountInfo(new Hash(N_H), _taks);
                lht.AddUpdate(ai);
            }

            //lht.TraverseNodes();

            //long received_takas = 0;

            // for (int i = 0; i < ACCOUNTS; i++)
            // {
            //     byte[] N_H = { (byte)(i * 3), 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
            //                       16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 };
            //     Hash h = new Hash(N_H);
            //     AccountInfo ai = (AccountInfo)lht[h];
            //     //DisplayUtils.Display("Fetch: " + HexUtil.ToString(ai.GetID().Hex) + "   ---  Money: " + ai.Money);
            //     received_takas += ai.Money;
            // }

            DisplayUtils.Display("Initial Money : " + taka);

            DisplayUtils.Display("\nTraversing ... ");

            long Tres = 0;

            long LeafDataCount = 0;
            long FoundNodes = 0;

            lht.TraverseAllNodes(ref LeafDataCount, ref FoundNodes, (X) =>
            {
                foreach (AccountInfo AI in X)
                {
                    Tres += AI.Money;
                }
                return TreeResponseType.NothingDone;
            });
            
            DisplayUtils.Display("Traversed Money : " + Tres);
            DisplayUtils.Display("Traversed Nodes : " + lht.TraversedNodes);
            DisplayUtils.Display("Traversed Elements : " + lht.TraversedElements);

            int cnt = 0;
            foreach (Hash h in accounts)
            {
                //N_H[5] = (byte)(i * 3);

                if (++cnt > accounts.Count / 2) break;

                lht.DeleteNode(h);
            }

            DisplayUtils.Display("\nTraversing After Delete ... ");
            // lht.TraverseNodes();

            DisplayUtils.Display("Traversed Money : " + lht.TotalMoney);
            DisplayUtils.Display("Traversed Nodes : " + lht.TraversedNodes);
            DisplayUtils.Display("Traversed Elements : " + lht.TraversedElements);
        }
    }
}
