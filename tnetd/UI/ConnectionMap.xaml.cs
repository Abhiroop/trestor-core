using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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

        Timer updateTimer;

        bool TryGetNewPoint(out Point point)
        {
            int max_try = 5000;
            point = new Point();

            int BOUNDARY = 40;
            int MIN_DISTANCE = 100;

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

        object TimerLock = new object();

        public ConnectionMap()
        {
            UseLayoutRounding = true;

            InitializeComponent();

            updateTimer = new Timer(TimerCallback, null, 0, 1000);
        }

        private void TimerCallback(Object o)
        {
            lock (TimerLock)
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    InvalidateVisual();
                }));
            }
        }
        private void CreateDrawingVisualRectangle(DrawingContext drawingContext, Point start, Point end, bool oneToTwo, bool twoToOne)
        {
            Color startColor = oneToTwo ? Colors.Red : Colors.Blue;
            Color endColor = twoToOne ? Colors.Red : Colors.Blue;

            // Create a rectangle and draw it in the DrawingContext.
            var gradientStopCollection = new GradientStopCollection
                    {
                        new GradientStop(startColor, 0.0),
                        new GradientStop(endColor, 1.0)
                    };

            var brush = new LinearGradientBrush(gradientStopCollection);
            var pen = new Pen(brush, 3.0);

            /*var vector1 = new Vector(start.X, start.Y);
            var vector2 = new Vector(end.X, end.Y);

            if (vector1.Length < vector2.Length)
            {
                brush.StartPoint = new Point(1, 1);
                brush.EndPoint = new Point(0, 0);
            }*/

            drawingContext.DrawLine(pen, start, end);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            try
            {
                if (NotEnoughAreaToDraw)
                {
                    FormattedText ft = new FormattedText("Not Enough Area To Draw, Please Resize",
                        CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Consolas"), 10, Brushes.Red);

                    drawingContext.DrawText(ft, new Point(20, 20));
                }
                else
                {
                    HashSet<HashPair> connections = new HashSet<HashPair>();

                    foreach (var nodeData in nodes)
                    {
                        Hash PK = nodeData.Key;
                        Node node = nodes[PK];

                        if (nodePositions.ContainsKey(PK))
                        {
                            Point point = nodePositions[PK];

                            foreach (var conns in node.nodeState.ConnectedValidators)
                            {
                                HashPair hp = new HashPair(PK, conns.Key);

                                if (!connections.Contains(hp))
                                {
                                    connections.Add(hp);
                                }
                            }
                        }
                    }

                    foreach (var conn in connections)
                    {
                        if (nodePositions.ContainsKey(conn.HexH1) && nodePositions.ContainsKey(conn.HexH1))
                        {
                            Point NP1 = nodePositions[conn.HexH1];
                            Point NP2 = nodePositions[conn.HexH2];

                            if (nodes.ContainsKey(conn.HexH1) && nodes.ContainsKey(conn.HexH2))
                            {
                                bool is1to2Trusted = nodes[conn.HexH1].nodeState.ConnectedValidators[conn.HexH2].IsTrusted;
                                bool is2to1Trusted = nodes[conn.HexH2].nodeState.ConnectedValidators[conn.HexH1].IsTrusted;

                                CreateDrawingVisualRectangle(drawingContext, NP1, NP2, is1to2Trusted, is2to1Trusted);
                            }
                        }
                    }

                    foreach (var v in nodePositions)
                    {
                        // Circle to point the center.
                        drawingContext.DrawEllipse(Brushes.Black, null, v.Value, 10, 10);
                    }

                }
            }
            catch (Exception ex)
            {
                DisplayUtils.Display(ex.Message + "" + ex.StackTrace);
            }
        }

        // //////////////  

    }
}
