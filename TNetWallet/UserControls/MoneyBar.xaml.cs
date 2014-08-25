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

            DestinationParts = new List<long>();
            
            InitializeComponent();
        }

        SolidColorBrush[] BrushList = new SolidColorBrush[] { Brushes.LightPink, Brushes.LightYellow, Brushes.LavenderBlush, Brushes.LightSteelBlue };

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            drawingContext.DrawRectangle(null, new Pen(Brushes.Navy, 2), new Rect(0, 0, ActualWidth, Height));

            long PartsTotal=0;
            foreach(long l in DestinationParts)
            {
                PartsTotal += l;
            }

            if(PartsTotal <= TotalMoney)
            {
                double TotalScale = ((double)TotalMoney - (double)PartsTotal) / (double)TotalMoney;

                int ColorCount = 0;
                double WidthIndex = 0;
                foreach (long part in DestinationParts)
                {
                    double PartScale = (((double)part) / (double)TotalMoney);

                    double PartWidth = PartScale * ActualWidth;

                    drawingContext.DrawRectangle(BrushList[(ColorCount++) % BrushList.Length], new Pen(Brushes.Purple, 1), new Rect(WidthIndex, 0, PartWidth, ActualHeight));
          
                    WidthIndex += PartWidth;
                }

                drawingContext.DrawRectangle(Brushes.LightGreen, new Pen(Brushes.Purple, 1), new Rect(WidthIndex, 0, TotalScale * ActualWidth, ActualHeight));
          

            }
            else
            {               
                drawingContext.DrawRectangle(null, new Pen(Brushes.Red, 2), new Rect(0, 0, ActualWidth, Height));
                drawingContext.DrawText(new FormattedText("Overspending", System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Tahoma"), 15, Brushes.Black), new Point(30, ActualHeight / 3));
            }


            //drawingContext.DrawEllipse(Brushes.Red, new Pen(Brushes.Purple, 2), new Point( ActualWidth / 2, ActualHeight / 2), 10.0, 10.0);
            
        }
    }
}
