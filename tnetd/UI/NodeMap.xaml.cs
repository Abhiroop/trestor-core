
// Maps the nodes during a simulation using all the multiple nodes in a set.

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

namespace TNetD.UI
{
    /// <summary>
    /// Interaction logic for NodeMap.xaml
    /// </summary>
    public partial class NodeMap : UserControl
    {
        public NodeMap()
        {
            UseLayoutRounding = true;

            InitializeComponent();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Pen pen = new Pen(Brushes.Black, 1);
            Rect rect = new Rect(20, 20, 50, 60);

            drawingContext.DrawRectangle(null, pen, rect);
            //drawingContext.Pop();
        }
    }
}
