using Blake2Sharp;
using Chaos.NaCl;
using System;
using System.Collections.Generic;
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
using TNetNative;

namespace TNetTest
{
    /// <summary>
    /// Interaction logic for Benchmarks.xaml
    /// </summary>
    public partial class Benchmarks : Window
    {
        public Benchmarks()
        {
            InitializeComponent();
        }

        enum BM { BLAKE2, SHA512, SHA256, SHA1, MD5, Ed25519_Sign, Ed25519_Verify,
            Ed25519_Sign_Native, Ed25519_Verify_Native, Curve25519_DH };


        private void ExecBM(BM bm)
        {
            try
            {
                long length = long.Parse(tb_DataLength.Text);

                byte[] data = new byte[length];

                System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

                timer.Start();

                switch (bm)
                {
                    case BM.MD5:
                        (new MD5Cng()).ComputeHash(data);
                        break;

                    case BM.SHA1:
                        (new SHA1Cng()).ComputeHash(data);
                        break;

                    case BM.SHA256:
                        (new SHA256Cng()).ComputeHash(data);
                        break;

                    case BM.SHA512:
                        (new System.Security.Cryptography.SHA512Cng()).ComputeHash(data);
                        break;

                    case BM.BLAKE2:
                        Blake2B.ComputeHash(data);
                        break;
                }

                timer.Stop();

                float Speed = (float)(((float)length / 1048576) / timer.Elapsed.TotalSeconds);

                tb_Results.AppendText(bm.ToString() + ": " + Speed + " MiB/s : Time : " + timer.Elapsed.TotalMilliseconds + " ms\n");
            }
            catch { }
        }

        private void ExecBM_25519(BM bm)
        {
            try
            {
                //long length = long.Parse(tb_DataLength.Text);

                //byte[] data = new byte[length];

                int Count = 1000;

                System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

                timer.Start();

                switch (bm)
                {
                    case BM.Ed25519_Sign:

                        for (int i = 0; i < 1000; i++)
                        {
                            Ed25519.Sign(new byte[1024], new byte[64]);
                        }

                        break;

                    case BM.Ed25519_Sign_Native:

                        for (int i = 0; i < 1000; i++)
                        {
                            Ed25519_Native.Sign(new byte[1024], new byte[32] ,new byte[64]);
                        }

                        break;

                    case BM.Ed25519_Verify:

                        for (int i = 0; i < 1000; i++)
                        {
                            Ed25519.Verify(new byte[64], new byte[1024], new byte[32]);
                        }

                        break;
                }

                timer.Stop();

                float Speed = (float)(((float)Count) / timer.Elapsed.TotalSeconds);

                tb_Results.AppendText(bm.ToString() + ": " + Speed + " Ops/s : Time : " + timer.Elapsed.TotalMilliseconds + " ms\n");
            }
            catch { }
        }

        private void menu_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void menu_Method_BLAKE2_Click(object sender, RoutedEventArgs e)
        {
            ExecBM(BM.BLAKE2);
        }

        private void menu_Method_SHA512_Click(object sender, RoutedEventArgs e)
        {
            ExecBM(BM.SHA512);
        }

        private void menu_Method_SHA256_Click(object sender, RoutedEventArgs e)
        {
            ExecBM(BM.SHA256);
        }

        private void menu_Method_SHA1_Click(object sender, RoutedEventArgs e)
        {
            ExecBM(BM.SHA1);
        }

        private void menu_Method_MD5_Click(object sender, RoutedEventArgs e)
        {
            ExecBM(BM.MD5);
        }

        private void menu_Method_Ed25519_Sign_Click(object sender, RoutedEventArgs e)
        {
            ExecBM_25519(BM.Ed25519_Sign);
        }

        private void menu_Method_Ed25519_Verify_Click(object sender, RoutedEventArgs e)
        {
            ExecBM_25519(BM.Ed25519_Verify);
        }

        private void menu_Method_Ed25519_Sign_Native_Click(object sender, RoutedEventArgs e)
        {
            ExecBM_25519(BM.Ed25519_Sign_Native);
        }

        private void menu_Method_Ed25519_Verify_Native_Click(object sender, RoutedEventArgs e)
        {
            ExecBM_25519(BM.Ed25519_Verify_Native);
        }

        private void menu_Method_Curve25519_Click(object sender, RoutedEventArgs e)
        {
            ExecBM_25519(BM.Curve25519_DH);
        }

    }
}
