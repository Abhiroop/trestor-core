using Blake2Sharp;
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

namespace TNetD.UI
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

        enum BM { BLAKE2, SHA512, SHA256, SHA1, MD5 };


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

    }
}
