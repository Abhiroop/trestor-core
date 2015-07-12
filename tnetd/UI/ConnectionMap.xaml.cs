using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using System.Windows.Media.Effects;
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

        /// <summary>
        /// Normal distance between two points (Euclidean)
        /// </summary>
        /// <param name="P1">First Point</param>
        /// <param name="P2">Second Point</param>
        /// <returns></returns>
        double DistanceBetweenPoints(Point P1, Point P2)
        {
            double y2_y1 = P2.Y - P1.Y;
            double x2_x1 = P2.X - P1.X;

            return Math.Sqrt((y2_y1 * y2_y1) + (x2_x1 * x2_x1));
        }

        /// <summary>
        /// Calculates the distance between a line and a point. The line is defined by P1 and P2. The Point is P0.
        /// </summary>
        /// <param name="P1">First point of the Line</param>
        /// <param name="P2">Second point of the Line</param>
        /// <param name="P0">The Point</param>
        /// <returns></returns>
        double DistanceBetweenLineAndPoint(Point P1, Point P2, Point P0)
        {
            double ASq = ((P2.Y - P1.Y) * P0.X) - ((P2.X - P1.X) * P0.Y) +
                (P2.X * P1.Y) - (P2.Y * P1.X);

            return Math.Abs(ASq) / (DistanceBetweenPoints(P1, P2));
        }

        bool TryGetNewPoint(out Point point)
        {
            int maxTryCount = 0;
            point = new Point();

            int BOUNDARY = 40;
            int MIN_DISTANCE = 100;
            int MIN_LINE_POINT_DISTANCE = 50;
            int MAX_TRY = 500;


            int W = 800;// (int)ActualWidth;
            int H = 600;// (int)ActualHeight;

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
                    double dist = DistanceBetweenPoints(p.Value, newPoint);
                    if (dist < MIN_DISTANCE)
                    {
                        passed = false;
                        break;
                    }
                }

                if (passed)
                {
                    int posCount = nodePositions.Count;

                    var k = nodePositions.Keys.ToList();

                    for (int i = 0; i < posCount; i++)
                    {
                        for (int j = i; j < posCount; j++)
                        {
                            Point P1 = nodePositions[k[i]];
                            Point P2 = nodePositions[k[j]];

                            double line_pointDist = DistanceBetweenLineAndPoint(P1, P2, newPoint);
                            if (line_pointDist < MIN_LINE_POINT_DISTANCE)
                            {
                                DisplayUtils.Display("Line point BREAK", DisplayType.Warning);
                                passed = false;
                                break;
                            }
                        }
                    }

                }

                if (passed)
                {
                    point = newPoint;
                    return true;
                }

                if (maxTryCount++ > MAX_TRY)
                {
                    break;
                }
            }

            return false;
        }

        bool NotEnoughAreaToDraw = false;

        internal void InitNodes(List<Node> _nodes)
        {
            lock (DrawLock)
            {
                nodes.Clear();
                nodePositions.Clear();

                foreach (Node node in _nodes)
                {
                    nodes.Add(node.PublicKey, node);
                }

                InitNodePositions();
            }
        }

        private void InitNodePositions()
        {
            nodePositions.Clear();
            NotEnoughAreaToDraw = false;

            int maxTryScratchCount = 0;
            int MAX_TRY_SCRATCH = 100;

            while (true)
            {
                foreach (var node in nodes)
                {
                    Point p;
                    if (TryGetNewPoint(out p))
                    {
                        nodePositions.Add(node.Key, p);
                    }
                }

                if (nodePositions.Count == nodes.Count)
                {
                    break;
                }
                else
                {
                    nodePositions.Clear();

                    if (maxTryScratchCount++ > MAX_TRY_SCRATCH)
                    {
                        NotEnoughAreaToDraw = true;
                        break;
                    }
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
                try
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        InvalidateVisual();
                    }));
                }
                catch { }                
            }
        }
        private object DrawLock = new object();

        private void CreateDrawingVisualRectangle(DrawingContext drawingContext, Point origin, Point start, Point end, bool oneToTwo, bool twoToOne, bool isOutgoing)
        {
            // Color : outgoing : BlueLight/LawnGreen(trusted)
            //         incoming : Red/Magenta

            Color outgoing = Colors.LightBlue;
            Color trustedOutgoing = Colors.LawnGreen;
            Color incoming = Colors.Red;
            Color trustedIncoming = Colors.Magenta;

            Color startColor = isOutgoing ? (oneToTwo ? trustedOutgoing : outgoing) : (oneToTwo ? trustedIncoming : incoming);

            Color endColor = (!isOutgoing) ? (twoToOne ? trustedOutgoing : outgoing) : (twoToOne ? trustedIncoming : incoming);

            // Create a rectangle and draw it in the DrawingContext.
            var gradientStopCollection = new GradientStopCollection
                    {
                        new GradientStop(startColor, 0.0),
                        new GradientStop(endColor, 1.0)
                    };

            var brush = new LinearGradientBrush(gradientStopCollection);
            var pen = new Pen(brush, 3.0);

            var vector1 = new Vector(start.X, start.Y);
            var vector2 = new Vector(end.X, end.Y);

            if (vector1.Length < vector2.Length)
            {
                brush.StartPoint = new Point(1, 1);
                brush.EndPoint = new Point(0, 0);
            }

            start.X += origin.X;
            start.Y += origin.Y;
              
            /*LineGeometry g = new LineGeometry();
            g.StartPoint = start;
            g.EndPoint = end;

            DrawingVisual dv = new DrawingVisual();


            var layer = new DrawingGroup();
            using (var lcontext = layer.Open())
            {
                lcontext.DrawGeometry(brush, pen, g);
            }

            var be = new BlurEffect
            {
                Radius = 3.0,
                KernelType = KernelType.Gaussian,
                RenderingBias = RenderingBias.Quality
            };

            //layer.SetValue(, be); //new DropShadowBitmapEffect { Color = Colors.Black, ShadowDepth = 3, Opacity = 0.5 };
            drawingContext.PushEffect(new BlurBitmapEffect(), null);

            drawingContext.DrawDrawing(layer);
            drawingContext.Pop();*/

            drawingContext.DrawLine(pen, start, end);
        }

        class Placeholder : FrameworkElement
        {
            Action<DrawingContext> action;
            public Placeholder(Action<DrawingContext> action)
            {
                this.action = action;
            }
            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);
                action(drawingContext);
            }
        }

        public static void RenderBlurred(DrawingContext dc, int width, int height, Rect targetRect, double blurRadius, Action<DrawingContext> action)
        {
            Rect elementRect = new Rect(0, 0, width, height);
            Placeholder element = new Placeholder(action)
            {
                Width = width,
                Height = height,
                Effect = new BlurEffect() { Radius = blurRadius }
            };
            element.Arrange(elementRect);
            RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Default);
            rtb.Render(element);
            dc.DrawImage(rtb, targetRect);
        }


        protected override void OnRender(DrawingContext drawingContext)
        {
            lock (DrawLock)
            {
                try
                {
                    //drawingContext.

                    if (NotEnoughAreaToDraw)
                    {
                        FormattedText ft = new FormattedText("Not Enough Area To Draw, Please Resize",
                            CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Consolas"), 10, Brushes.Red);

                        drawingContext.DrawText(ft, new Point(20, 20));
                    }
                    else
                    {
                        Point nodeGraphOrigin = new Point(0, 0);

                        DrawNodeGraph(drawingContext, nodeGraphOrigin);

                    }
                }
                catch (Exception ex)
                {
                    DisplayUtils.Display(ex.Message + "" + ex.StackTrace);
                }
            }
        }

        private void DrawNodeGraph(DrawingContext drawingContext, Point nodeGraphOrigin)
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
                        bool isOutgoing = nodes[conn.HexH1].nodeState.ConnectedValidators[conn.HexH2].Direction == ConnectionDirection.Outgoing;
                        bool is2to1Trusted = nodes[conn.HexH2].nodeState.ConnectedValidators[conn.HexH1].IsTrusted;

                        CreateDrawingVisualRectangle(drawingContext, nodeGraphOrigin, NP1, NP2, is1to2Trusted, is2to1Trusted, isOutgoing);
                    }
                }
            }

            foreach (var np in nodePositions)
            {
                Point v = np.Value;
                v.X += nodeGraphOrigin.X;
                v.Y += nodeGraphOrigin.Y;

                // Circle to point the center.
                drawingContext.DrawEllipse(Brushes.LightGray, null, v, 15, 15);
                /*RenderBlurred(drawingContext, 35, 35, new Rect(v, new Size(35, 35)), 10, 
                    dc => dc.DrawRectangle(new SolidColorBrush(Colors.Transparent), new Pen(Brushes.Blue, 3), new Rect(0, 0, 35, 35)));*/

                if (nodes.ContainsKey(np.Key))
                {
                    Node nd = nodes[np.Key];

                    FormattedText ft = new FormattedText("" + nd.nodeConfig.NodeID,
                        CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Courier New"), 12, Brushes.Black);

                    Point textPoint = v;
                    textPoint.X -= 5;
                    textPoint.Y -= 5;

                    drawingContext.DrawText(ft, textPoint);
                }

            }
        }

        // //////////////  

    }
}
