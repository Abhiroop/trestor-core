using System;
using System.Collections.Generic;
using System.Globalization;
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
using TNetD.Nodes;

namespace TNetD.UI
{
    /// <summary>
    /// Interaction logic for ConnectionMap.xaml
    /// </summary>
    partial class ConnectionMap : UserControl
    {
        List<Node> nodes = new List<Node>();

        List<Point> nodePositions = new List<Point>();

        bool TryGetNewPoint(out Point point)
        {
            int max_try = 5000;
            point = new Point();

            int BOUNDARY = 40;
            int MIN_DISTANCE = 80;

            int W = 600;// (int)ActualWidth;
            int H = 400;// (int)ActualHeight;

            int randSpaceX = W - BOUNDARY * 2; // 50 pixels from both sides;
            int randSpaceY = H - BOUNDARY * 2;

            while (true)
            {
                // Calculate coordinates.

                int randX = Common.random.Next(0, randSpaceX) + BOUNDARY;
                int randY = Common.random.Next(0, randSpaceY) + BOUNDARY;

                Point newPoint = new Point(randX, randY);

                // Verify distance between points.

                bool passed = true;

                foreach (Point p in nodePositions)
                {
                    double dist = Math.Sqrt(((p.X - newPoint.X) * (p.X - newPoint.X)) + ((p.Y - newPoint.Y) * (p.Y - newPoint.Y)));
                    if (dist < MIN_DISTANCE)
                    {
                        passed = false;
                        break;
                    }
                }

                if (passed)
                {
                    point = newPoint;
                    return true;
                }

                if (max_try++ > 5000) break;
            }

            return false;
        }

        bool NotEnoughAreaToDraw = false;

        internal void InitNodes(List<Node> nodes)
        {
            this.nodes = nodes;

            InitNodePositions(nodes);
        }

        private void InitNodePositions(List<Node> nodes)
        {
            nodePositions.Clear();
            NotEnoughAreaToDraw = false;

            foreach (Node node in nodes)
            {
                Point p;
                if (TryGetNewPoint(out p))
                {
                    nodePositions.Add(p);
                }
                else
                {
                    NotEnoughAreaToDraw = true;
                    break;
                }
            }

            InvalidateVisual();
        }

        public ConnectionMap()
        {
            UseLayoutRounding = true;

            InitializeComponent();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Pen pen = new Pen(Brushes.Black, 1);
            Rect rect = new Rect(20, 20, 50, 60);

            if (NotEnoughAreaToDraw)
            {
                FormattedText ft = new FormattedText("Not Enough Area To Draw, Please Resize",
                    CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Consolas"), 10, Brushes.Red);

                drawingContext.DrawText(ft, new Point(20, 20));

            }
            else
            {
                drawingContext.DrawRectangle(null, pen, rect);

                for (int i = 0; i < nodes.Count; i++)
                {
                    Node node = nodes[i];
                    Point point = nodePositions[i];

                    // Circle to point the center.

                    drawingContext.DrawEllipse(Brushes.Blue, null, point, 10, 10);


                }

            }


        }
    }
}
