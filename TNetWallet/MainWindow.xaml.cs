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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TNetWallet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mainFrame.Content = App.HomeScreen;
        }

        private void button_Exit_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void button_Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                if (e.ClickCount == 2)
                {
                    //AdjustWindowSize();
                }
                else
                {
                    Application.Current.MainWindow.DragMove();
                }
        }

        private void image_Settings_MouseDown(object sender, MouseButtonEventArgs e)
        {
            settingsMenu.PlacementTarget = this;
            settingsMenu.IsOpen = true;
        }
        
        private void image_Home_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mainFrame.Content = App.HomeScreen;
        }

        private void image_Transaction_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void image_Transaction_MouseUp_1(object sender, MouseButtonEventArgs e)
        {

        }

        private void image_Users_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void image_Refresh_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

    }
}
