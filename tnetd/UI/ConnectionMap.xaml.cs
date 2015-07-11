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
using TNetD.Network.Networking;
using TNetD.Nodes;

namespace TNetD.UI
{
    /// <summary>
    /// Interaction logic for ConnectionMap.xaml
    /// </summary>
    partial class ConnectionMap : UserControl
    {
        Dictionary<Hash, Node> nodes = new Dictionary<Hash, Node>();

        Dictionary<Hash, Point> nodePositions = new Dictionary<Hash, Point>();

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

                foreach (var p in nodePositions)
                {
                    double dist = Math.Sqrt(((p.Value.X - newPoint.X) * (p.Value.X - newPoint.X)) + ((p.Value.Y - newPoint.Y) * (p.Value.Y - newPoint.Y)));
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

        internal void InitNodes(List<Node> _nodes)
        {
            foreach (Node node in _nodes)
            {
                nodes.Add(node.PublicKey, node);
            }

            InitNodePositions();
        }

        private void InitNodePositions()
        {
            nodePositions.Clear();
            NotEnoughAreaToDraw = false;

            foreach (var node in nodes)
            {
                Point p;
                if (TryGetNewPoint(out p))
                {
                    nodePositions.Add(node.Key, p);
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

                HashSet<HashPair> connections = new HashSet<HashPair>();
                HashSet<HashPair> trustedConnections = new HashSet<HashPair>();

                foreach(var nodeData in nodes)
                {
                    Hash PK = nodeData.Key;
                    Node node = nodes[PK];

                    if(nodePositions.ContainsKey(PK))
                    {
                        Point point = nodePositions[PK];

                        // Circle to point the center.

                        drawingContext.DrawEllipse(Brushes.Blue, null, point, 10, 10);

                        Hash [] conns = node.networkPacketSwitch.GetConnectedNodes(ConnectionListType.All);
                        
                    }
                }
            }
        }

        // //////////////  

    }
}
