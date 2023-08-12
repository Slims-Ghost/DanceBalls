using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DanceBalls
{
    internal class Game
    {
        public Bitmap Bitmap { get; set; } = new(1, 1);
        public Stopwatch GameClock { get; set; } = new();
        private Random rand = new Random();
        private Color BackgroundColor;
        private float ClientWidth;
        private float ClientHeight;
        private Rectangle ClientBounds;
        private RectangleF ScaleBounds;
        private PointF ScaleCenter;
        private float AspectRatio;
        private float ScaleHeight;
        private float ScaleWidth;
        private float ScaleLeft;
        private float ScaleRight;
        private float ScaleTop;
        private float ScaleBottom;
        private const float ONE_PI = (float)Math.PI;
        private const float TWO_PI = ONE_PI * 2;
        private const float RAD_TO_DEGREES = 180 / ONE_PI;
        private const float DEGREES_TO_RAD = ONE_PI / 180;
        public object BitmapLocker = new();
        private System.Timers.Timer GameTimer { get; set; } = new();
        public List<Ball> balls= new();
        public Ball b1;
        public Ball b2;
        public long TicksSinceLastUpdate = 0;
        public long LastUpdateTicks = 0;
        public GameState GameState { get; private set; } = GameState.Setup;
        private Bitmap OrbImage = (Bitmap)Image.FromFile(Path.Combine(AppRoot, "orb.png"));
        private static readonly string AppRoot = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
        
        private float rotationEachSecond = ONE_PI / 6;

        public Game(Rectangle clientBounds, Color backgroundColor) 
        {
            GameState = GameState.Setup;
            GameClock.Start();
            SetScale(clientBounds);
            BackgroundColor = backgroundColor;
            GameTimer.Interval = 25;
            GameTimer.AutoReset = false;
            GameTimer.Elapsed += GameTimer_Elapsed;
            InitBalls();
            GameState = GameState.Ready;
            GameTimer.Start();
        }

        public void InitBalls()
        {
            //balls.Clear();

            //b1 = new() { D = 55, Theta = ONE_PI / 4, Radius = 30, Color = Color.Magenta, };
            //var ballSizedImage = Helpers.ResizeImage(OrbImage, ScaleToClient(b1.Bounds));
            //b1.Bitmap = ballSizedImage;
            //balls.Add(b1);

            //b2 = new() { X = -60, Y = -20, Radius = 50, Color = Color.Cyan, };
            //b2.Bitmap = Helpers.ResizeImage(OrbImage, ScaleToClient(b2.Bounds));
            //balls.Add(b2);
        }


        private void GameTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            TicksSinceLastUpdate = LastUpdateTicks == 0 ? 0 : GameClock.ElapsedTicks - LastUpdateTicks;
            var timeSinceLastUpdate = TimeSpan.FromTicks(TicksSinceLastUpdate);

            float rotation = (float)timeSinceLastUpdate.TotalSeconds * rotationEachSecond;
            b1.Theta = (b1.Theta + rotation).NormalizeRadians();
            

            GameTimer.Start();
            LastUpdateTicks = GameClock.ElapsedTicks;
        }

        public async Task DrawAsync()
        {
            await Task.Run(() =>
            {
                lock (BitmapLocker)
                {
                    using var g = Graphics.FromImage(Bitmap);
                    g.Clear(BackgroundColor);
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    DrawGrid(g);
                    DrawObjects(g);
                }
            });
        }

        private void DrawObjects(Graphics g)
        {

            foreach (var ball in balls)
            {
                DrawBall(g, ball);
            }
        }

        private void DrawBall(Graphics g, Ball ball)
        {
            //g.FillEllipse(new SolidBrush(color), ScaleToClient(ball.Bounds));
            if (ball.X == -60) { }
            //g.DrawImageUnscaled(ball.Bitmap!, ScaleToClient(ball.Bounds));
        }

        private void DrawLine(Graphics g, Pen pen, Line line)
        {
            g.DrawLine(pen, ScaleToClient(line.p1), ScaleToClient(line.p2));
        }

        private void DrawLine(Graphics g, Pen pen, PointF p1, PointF p2)
        {
            g.DrawLine(pen, ScaleToClient(p1), ScaleToClient(p2));
        }

        private void DrawLine(Graphics g, Pen pen, float x1, float y1, float x2, float y2)
        {
            g.DrawLine(pen, ScaleToClient(new PointF(x1, y1)), ScaleToClient(new PointF(x2, y2)));
        }

        private void DrawLines(Graphics g, Pen pen, List<PointF> points, bool rounded = false)
        {
            var pColor = Color.DarkGoldenrod;
            var lPen = new Pen(Color.DarkCyan, 3);
            var bPen = new Pen(Color.DarkCyan, 3);
            var smallPen = new Pen(Color.Red, 1);
            var scaledPoints = points.Select(p => ScaleToClient(p)).ToArray();
            if (scaledPoints.Length < 2) return;
            if (scaledPoints.Length == 2)
            {
                g.DrawLine(pen, scaledPoints[0], scaledPoints[1]);
            }
            else if (!rounded)
            {
                g.DrawLines(pen, scaledPoints);
            }
            else
            {
                float radius = 25;
                PointF A, B, C, P, Q;
                Line AB, BC;

                A = points[0];
                B = points[1];
                AB = new(A, B);
                //DrawLine(g, smallPen, A, B);
                P = AB.GetPointAtDistance(Math.Min(radius, AB.Length / 2));
                DrawLine(g, lPen, A, P);
                for (int i = 2; i < points.Count; i++)
                {
                    //DrawPoint(g, pColor, B);
                    C = points[i];
                    BC = new(B, C);
                    Q = BC.GetPointAtDistance(Math.Min(radius, BC.Length / 2));
                    DrawBezier(g, bPen, P, B, B, Q);

                    P = BC.GetPointAtDistance(Math.Min(radius, BC.Length / 2), false);
                    DrawLine(g, lPen, Q, P);

                    //DrawPoint(g, pColor, P);
                    //DrawPoint(g, pColor, Q);

                    A = B;
                    B = C;
                    AB = new(A, B);
                    //DrawLine(g, smallPen, A, B);
                }
                B = points[^2];
                C = points[^1];
                BC = new(B, C);
                //DrawLine(g, smallPen, B, C);
                Q = BC.GetPointAtDistance(Math.Min(radius, BC.Length / 2));
                DrawLine(g, lPen, Q, C);
            }
        }

        private void DrawPoint(Graphics g, Color color, PointF p)
        {
            var pointSize = 4;
            var rect = new RectangleF(p.X - pointSize / 2, p.Y + pointSize / 2, pointSize, pointSize);
            g.FillRectangle(new SolidBrush(color), ScaleToClient(rect));
        }

        private void DrawBezier(Graphics g, Pen pen, PointF p1, PointF p2, PointF p3, PointF p4)
        {
            g.DrawBezier(pen, ScaleToClient(p1), ScaleToClient(p2), ScaleToClient(p3), ScaleToClient(p4));
        }

        private void DrawGrid(Graphics g)
        {
            float gridSpacing_Major = 50;
            float gridSpacing_Minor = 10;

            var gridPen_Major = new Pen(Color.FromArgb(0x40, 0x40, 0x50), 1) { DashPattern = new float[] { 1, 4 } };
            var gridPen_Minor = new Pen(Color.FromArgb(0x1f, 0x1f, 0x58), 1) { DashPattern = new float[] { 1, 4 } };
            var axesPen = new Pen(Color.FromArgb(0x40, 0x40, 0x50), 2) { DashPattern = new float[] { 3, 8 } };

            for (float x = 0; x >= ScaleLeft; x -= gridSpacing_Minor) DrawLine(g, (x % gridSpacing_Major == 0) ? gridPen_Major : gridPen_Minor, new PointF(x, ScaleTop), new PointF(x, ScaleBottom));
            for (float x = 0; x <= ScaleRight; x += gridSpacing_Minor) DrawLine(g, (x % gridSpacing_Major == 0) ? gridPen_Major : gridPen_Minor, new PointF(x, ScaleTop), new PointF(x, ScaleBottom));
            for (float y = 0; y <= ScaleTop; y += gridSpacing_Minor) DrawLine(g, (y % gridSpacing_Major == 0) ? gridPen_Major : gridPen_Minor, new PointF(ScaleLeft, y), new PointF(ScaleRight, y));
            for (float y = 0; y >= ScaleBottom; y -= gridSpacing_Minor) DrawLine(g, (y % gridSpacing_Major == 0) ? gridPen_Major : gridPen_Minor, new PointF(ScaleLeft, y), new PointF(ScaleRight, y));
            DrawLine(g, axesPen, new PointF(0, ScaleTop), new PointF(0, ScaleBottom));
            DrawLine(g, axesPen, new PointF(ScaleLeft, 0), new PointF(ScaleRight, 0));
        }

        public Point ScaleToClient(PointF p)
        {
            //if (ClientWidth == 1000) { }
            if (ScaleWidth / ClientWidth != ScaleHeight / ClientHeight) { }
            var pClient = new Point((int)Math.Round((p.X - ScaleLeft) / ScaleWidth * ClientWidth), (int)Math.Round(-(p.Y - ScaleTop) / ScaleHeight * ClientHeight));
            return pClient;
        }

        public Size ScaleToClient(SizeF scaleSize)
        {
            var sizeClient = new Size((int)(scaleSize.Width / ScaleWidth * ClientWidth), (int)(scaleSize.Height / ScaleHeight * ClientHeight));
            return sizeClient;
        }

        public Rectangle ScaleToClient(RectangleF scaleRect)
        {
            var locClient = ScaleToClient(new PointF(scaleRect.Left, scaleRect.Top));
            var sizeClient = ScaleToClient(new SizeF(scaleRect.Width, scaleRect.Height));
            Rectangle rect = new Rectangle(locClient, sizeClient);
            return rect;
        }

        public PointF ClientToScale(Point p)
        {
            var pScale = new PointF((((float)p.X - ClientBounds.Left) / ClientBounds.Width * ScaleWidth) + ScaleLeft, ScaleTop - (((float)p.Y - ClientBounds.Top) / ClientBounds.Height * ScaleHeight));
            return pScale;
        }

        public void SetScale(Rectangle clientBounds)
        {
            ClientBounds = clientBounds;
            lock(BitmapLocker)
            {
                Bitmap?.Dispose();
                Bitmap = new(ClientBounds.Width, ClientBounds.Height);
            }

            //ScaleHeight drives the rest of the Scale dimensions;
            ScaleHeight = 200f;
            ScaleCenter = new PointF(0f, 0f);
            AspectRatio = (float)ClientBounds.Width / ClientBounds.Height;
            ScaleWidth = ScaleHeight * AspectRatio;
            ScaleBounds = new RectangleF(ScaleCenter.X - (ScaleWidth / 2), ScaleCenter.Y + (ScaleHeight / 2), ScaleWidth, ScaleHeight);
            ScaleLeft = -ScaleWidth / 2;
            ScaleRight = ScaleWidth / 2;
            ScaleTop = ScaleHeight / 2;
            ScaleBottom = -ScaleHeight / 2;
            ClientWidth = ClientBounds.Width;
            ClientHeight = ClientBounds.Height;
        }
    }
}
