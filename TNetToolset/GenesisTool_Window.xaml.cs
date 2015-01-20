//
// @ Author : Arpan Jati
// @ Date : 14th January 2015

// Contains code to generate the Genesis Ledger Accounts.

using Chaos.NaCl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
using TNetD;
using TNetD.Address;

namespace TNetToolset
{

    /// <summary>
    /// Interaction logic for GenesisTool_Window.xaml
    /// </summary>
    public partial class GenesisTool_Window : Window
    {
        public GenesisTool_Window()
        {
            InitializeComponent();
        }

        // NETWORK Types;
        // 
        // MAIN NET : 14[T]
        // TEST NET : 29[t]
        //
        // Account Types;
        //
        // [ON MAIN]
        // GENESIS    : 201 [G] 
        // VALIDATOR  : 234 [V] 
        // NORMAL     : 217 [N] // THIS WILL BE THE MOST COMMON ONE 
        //                    
        // [ON TEST]          
        // GENESIS    : 25 [g] 
        // VALIDATOR  : 59 [v] 
        // NORMAL     : 40 [n] 

        // GENESIS ACCOUNT FORMAT (.gen_secret) 
        // RANDOMPRIVATE | PUBLIC | NAME | ADDRESS | DESCRIPTION 

        // GENESIS ACCOUNT PUBLIC FORMAT (.gen_public) 
        // PUBLIC | NAME | ADDRESS | DESCRIPTION 

        private void btn_Generate_Click(object sender, RoutedEventArgs e)
        {
            FileStream f_secret = new FileStream("ACCOUNTS.GEN_SECRET", FileMode.Create, FileAccess.Write, FileShare.None);
            StreamWriter sw_secret = new StreamWriter(f_secret);

            FileStream f_public = new FileStream("ACCOUNTS.GEN_PUBLIC", FileMode.Create, FileAccess.Write, FileShare.None);
            StreamWriter sw_public = new StreamWriter(f_public);

            // [WELL, THIS LOOKS LIKE A GOOD RNG
            // MAYBE WE SHOULD GENERATE PRIVATE RANDOM KEYS FOR THE FINAL GENESIS USING OTHER TRNG]

            RNGCryptoServiceProvider rngCSP = new RNGCryptoServiceProvider();

            NetworkType[] Net_Types = new NetworkType[] { NetworkType.MainNet, NetworkType.TestNet };
            AccountType[] Acc_Types_Main = new AccountType[] { AccountType.MainGenesis, AccountType.MainValidator, AccountType.MainNormal };
            AccountType[] Acc_Types_Test = new AccountType[] { AccountType.TestGenesis, AccountType.TestValidator, AccountType.TestNormal };

            AddressFactory af = new AddressFactory();

            af.NetworkType = Net_Types[cb_NetworkType.SelectedIndex];

            af.AccountType = cb_NetworkType.SelectedIndex == 0 ?
                (AccountType)Acc_Types_Main[cb_AccountType.SelectedIndex] :
                (AccountType)Acc_Types_Test[cb_AccountType.SelectedIndex];

            string AccountPrefix = tb_AccountNamePrefix.Text.Trim();
            string DescriptionPrefix = tb_AccountDescriptionPrefix.Text.Trim();

            for (int i = 0; i < int.Parse(tb_NumAccounts.Text); i++)
            {
                byte[] RandomPrivate = new byte[32];

                rngCSP.GetBytes(RandomPrivate);

                byte[] publicKey;
                byte[] expandedPrivateKey;

                Ed25519.KeyPairFromSeed(out publicKey, out expandedPrivateKey, RandomPrivate);

                string PRIVATE = HexUtil.ToString(RandomPrivate);
                string PUBLIC = HexUtil.ToString(publicKey);

                string NAME = AccountPrefix + "_" + i;

                string ADDRESS = AddressFactory.GetAddressString(af.GetAddress(publicKey, NAME));

                string DESCRIPTION = DescriptionPrefix;// +"_" + i;

                string LINE_PUBLIC = PUBLIC + "|" + NAME + "|" + ADDRESS + "|" + DESCRIPTION;

                string LINE_SECRET = PRIVATE + "|" + LINE_PUBLIC;

                sw_secret.WriteLine(LINE_SECRET);
                sw_public.WriteLine(LINE_PUBLIC);
            }

            sw_secret.Close();
            sw_public.Close();
        }

    }
}
