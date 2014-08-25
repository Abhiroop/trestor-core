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

namespace TNetWallet.UserControls
{
    /// <summary>
    /// Interaction logic for MoneyBar.xaml
    /// </summary>
    public partial class MoneyBar : UserControl
    {

        public long TotalMoney
        {
            get;
            set;
        }

        public List<long> DestinationParts
        {
            get;
            set;
        }

        public MoneyBar()
        {
            TotalMoney = 100;
            
            InitializeComponent();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            drawingContext.DrawRectangle(null, new Pen(Brushes.AliceBlue, 2), new Rect(0, 0, ActualWidth, Height));


            drawingContext.DrawEllipse(Brushes.Red, new Pen(Brushes.Purple, 2), new Point( ActualWidth / 2, ActualHeight / 2), 10.0, 10.0);
            
        }
    }
}
