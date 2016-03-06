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
    public enum ConnectionMapDisplayMode { Voting, LedgerSync, Trust, TimeSync };

    /// <summary>
    /// Interaction logic for ConnectionMap.xaml
    /// </summary>
    partial class ConnectionMap : UserControl
    {
        public ConnectionMapDisplayMode DisplayMode { get; set; }

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
            int MIN_DISTANCE = 200;
            int MIN_LINE_POINT_DISTANCE = 100;
            int MAX_TRY = 500;

            int W = 900;
            int H = 600;

            int randSpaceX = W - BOUNDARY * 2; // 50 pixels from both sides;
            int randSpaceY = H - BOUNDARY * 2;

            while (true)
            {
                // Calculate coordinates.

                int randX = Common.NORMAL_RNG.Next(0, randSpaceX) + BOUNDARY;
                int randY = Common.NORMAL_RNG.Next(0, randSpaceY) + BOUNDARY;

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

            DisplayMode = ConnectionMapDisplayMode.Voting;

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

        private void DrawGradientLine(DrawingContext drawingContext, Point origin, Point start, Point end, bool oneToTwo, bool twoToOne, bool isOutgoing)
        {
            start.X += origin.X;
            start.Y += origin.Y;

            end.X += origin.X;
            end.Y += origin.Y;

            // Color : outgoing : BlueLight/LawnGreen(trusted)
            //         incoming : Red/Magenta

            Color outgoing = Colors.ForestGreen;
            Color trustedOutgoing = Color.FromRgb(204, 255, 51); // Bright Green
            Color incoming = Color.FromRgb(153, 204, 255); // Light Blue
            Color trustedIncoming = Color.FromRgb(223, 255, 126); // Light Green

            Color startColor = isOutgoing ? (oneToTwo ? trustedOutgoing : outgoing) : (oneToTwo ? trustedIncoming : incoming);

            Color endColor = (!isOutgoing) ? (twoToOne ? trustedOutgoing : outgoing) : (twoToOne ? trustedIncoming : incoming);

            //Color startColor = (oneToTwo ? Colors.LawnGreen : Colors.Red);
            //Color endColor = (twoToOne ? Colors.LawnGreen : Colors.Red);

            Pen startPen = new Pen(new SolidColorBrush(startColor), 3);
            Pen endPen = new Pen(new SolidColorBrush(endColor), 3);

            Point mid = new Point((start.X + end.X) / 2, (start.Y + end.Y) / 2);

            drawingContext.DrawLine(startPen, start, mid);
            drawingContext.DrawLine(endPen, mid, end);

            //Color endColor = (!isOutgoing) ? (twoToOne ? trustedOutgoing : outgoing) : (twoToOne ? trustedIncoming : incoming);

            // Create a rectangle and draw it in the DrawingContext.
            /* var gradientStopCollection = new GradientStopCollection
                     {
                         new GradientStop(startColor, 0.0),
                         new GradientStop(endColor, 1.0)
                     };

             double deltaY = end.Y - start.Y;
             double deltaX = end.X - start.X;
            
             double angleInDegrees = Math.Atan2(deltaY , deltaX) * 180 / Math.PI;
            
             var brush = new LinearGradientBrush(gradientStopCollection, angleInDegrees);
             var pen = new Pen(brush, 3.0);
            
             drawingContext.DrawLine(pen, start, end);*/
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

        void PrintText(DrawingContext dc, string text, Point position)
        {
            FormattedText conn_text = new FormattedText(text,
                       CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Courier New"), 12, Brushes.LightCyan);

            dc.DrawText(conn_text, position);
        }

        private string GetNodeDisplayString(Node nd, ConnectionMapDisplayMode mode)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("" + nd.nodeConfig.NodeID + " - " + nd.PublicKey.ToString().Substring(0, 6));

            switch (mode)
            {
                case ConnectionMapDisplayMode.Voting:

                    string rh = nd.LocalLedger.LedgerTree.GetRootHash().ToString();

                    sb.Append(
                        "\nState: " + nd.ConsensusState +
                        "\nRoot: " + ((rh.Length >= 8) ? rh.Substring(0, 8) : "Empty")
                        );

                    break;

                case ConnectionMapDisplayMode.Trust:

                    int Trusted = 0;
                    int Untrusted = 0;
                    int Outgoing = 0;

                    StringBuilder TL = new StringBuilder();

                    foreach (var val in nd.nodeState.ConnectedValidators)
                    {
                        if (val.Value.IsTrusted)
                            TL.Append("" + val.Key.ToString().Substring(0, 4) + ",");

                        Trusted += val.Value.IsTrusted ? 1 : 0;
                        Untrusted += val.Value.IsTrusted ? 0 : 1;

                        Outgoing += val.Value.Direction == ConnectionDirection.Outgoing ? 1 : 0;
                    }

                    sb.Append("\nTrusted: " + Trusted +
                        //"\n" + TL.ToString() +                                            
                                "\nOutgoing: " + Outgoing);
                    break;

                case ConnectionMapDisplayMode.TimeSync:

                    break;

                case ConnectionMapDisplayMode.LedgerSync:

                    break;

            }

            return sb.ToString();
        }

        private void DrawNodeGraph(DrawingContext drawingContext, Point nodeGraphOrigin)
        {
            HashSet<HashPair> connections = new HashSet<HashPair>();

            foreach (var nodeData in nodes)
            {
                foreach (var conns in nodeData.Value.nodeState.ConnectedValidators)
                {
                    HashPair hp = new HashPair(nodeData.Key, conns.Key);

                    if (!connections.Contains(hp))
                    {
                        connections.Add(hp);
                    }
                }
            }

            PrintText(drawingContext, "Connections = " + connections.Count, new Point(30, 30));

            foreach (var conn in connections)
            {
                if (nodePositions.ContainsKey(conn.HexH1) &&
                    nodePositions.ContainsKey(conn.HexH1) &&
                    nodes.ContainsKey(conn.HexH1) &&
                    nodes.ContainsKey(conn.HexH2))
                {
                    Point NP1 = nodePositions[conn.HexH1];
                    Point NP2 = nodePositions[conn.HexH2];

                    Node N1 = nodes[conn.HexH1];
                    Node N2 = nodes[conn.HexH2];

                    bool is1to2Trusted = N1.nodeState.ConnectedValidators[conn.HexH2].IsTrusted;
                    bool isOutgoing = nodes[conn.HexH1].nodeState.ConnectedValidators[conn.HexH2].Direction == ConnectionDirection.Outgoing;
                    bool is2to1Trusted = N2.nodeState.ConnectedValidators[conn.HexH1].IsTrusted;

                    DrawGradientLine(drawingContext, nodeGraphOrigin, NP1, NP2, is1to2Trusted, is2to1Trusted, isOutgoing);

                }
            }

            foreach (var np in nodePositions)
            {
                Point v = np.Value;
                v.X += nodeGraphOrigin.X;
                v.Y += nodeGraphOrigin.Y;

                // Circle to point the center.
                //drawingContext.DrawEllipse(Brushes.LightGray, null, v, 15, 15);

                int rectWidth = 100;
                int rectHeight = 50;

                v.X -= rectWidth / 2;
                v.Y -= rectHeight / 2;

                Rect rectTODraw = new Rect(v, new Size(rectWidth, rectHeight));

                SolidColorBrush scb_blue = new SolidColorBrush(Color.FromRgb(33, 98, 163));

                drawingContext.DrawRoundedRectangle(scb_blue, null, rectTODraw, 5, 5);

                /*RenderBlurred(drawingContext, 35, 35, new Rect(v, new Size(35, 35)), 10, 
                    dc => dc.DrawRectangle(new SolidColorBrush(Colors.Transparent), new Pen(Brushes.Blue, 3), new Rect(0, 0, 35, 35)));*/

                if (nodes.ContainsKey(np.Key))
                {
                    Node nd = nodes[np.Key];

                    FormattedText ft = new FormattedText(GetNodeDisplayString(nd, DisplayMode),
                        CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Courier New"), 10, Brushes.LightCyan);

                    Point textPoint = v;
                    textPoint.X += 5;
                    textPoint.Y += 5;

                    drawingContext.DrawText(ft, textPoint);
                }

            }
        }

        // //////////////  

    }
}
