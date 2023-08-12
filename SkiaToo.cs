using OpenTK;
using SkiaSharp;
using System.Diagnostics;

namespace DanceBalls
{
    public partial class SkiaToo : Form
    {
        #region Member vars/props
        private readonly Stopwatch GameClock = new Stopwatch();
        private readonly System.Timers.Timer GameTimer = new();
        private bool AlreadyDrawing;
        private long TickCount = 0;
        private long TicksSinceLastUpdate = 0;
        private readonly Queue<long> TickCounts = new Queue<long>();
        private long LastUpdateTicks = 0;
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
        private float ScaleFactor;
        private const float ONE_PI = (float)Math.PI;
        private const float TWO_PI = ONE_PI * 2;
        private const float RAD_TO_DEGREES = 180 / ONE_PI;
        private const float DEGREES_TO_RAD = ONE_PI / 180;
        private static readonly string AppRoot = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
        private float rotationEachSecond = ONE_PI;
        private float FrameRateActual;
        private float FrameRateTarget = 60;
        private int InitialTimerInterval = 14;
        private List<Ball> balls = new();
        private Random rand = new Random();
        private SKColor Color_MajorGridLine = new SKColor(0x40, 0x40, 0x70);
        private SKColor Color_MinorGridLine = new SKColor(0x10, 0x10, 0x40);
        private SKColor Color_Axis = new SKColor(0x40, 0x40, 0x70);
        private SKColor Color_Background = new SKColor(0x0, 0x0, 0x14);
        #endregion

        public SkiaToo()
        {
            InitializeComponent();
            //ScaleHeight drives the rest of the Scale dimensions;
            ScaleHeight = 200f;
            SetScale(skglControl1.Bounds);
            InitBalls();
            GameTimer.Interval = InitialTimerInterval;
            GameTimer.SynchronizingObject = this;
            GameTimer.Elapsed += Timer_Elapsed;
            
            GameClock.Start();
            GameTimer.Start();
        }

        private void InitBalls()
        {
            balls.Clear();
            Ball b1 = new(75, ONE_PI / 4, 10, Path.Combine(AppRoot, "orb.png"), ScaleFactor);
            Ball b2 = new(50, ONE_PI + (ONE_PI / 4), 15, Path.Combine(AppRoot, "blue orb.png"), ScaleFactor);
            balls.Add(b1);
            balls.Add(b2);
        }

        private void Advance()
        {
            TicksSinceLastUpdate = LastUpdateTicks == 0 ? 0 : GameClock.ElapsedTicks - LastUpdateTicks;
            var timeSinceLastUpdate = TimeSpan.FromTicks(TicksSinceLastUpdate);

            float rotation = (float)timeSinceLastUpdate.TotalSeconds * rotationEachSecond;
            balls[0].Theta = (balls[0].Theta + rotation).NormalizeRadians();
            balls[1].Theta = (balls[1].Theta + (rotation * 0.83f)).NormalizeRadians();


            LastUpdateTicks = GameClock.ElapsedTicks;
        }

        private void DrawObjects(SKCanvas c)
        {

            var connectorPaint = new SKPaint { Style = SKPaintStyle.Stroke, Color = new SKColor(0, 220, 220), StrokeWidth = 3, };
            //DrawLine(c, connectorPaint, balls[0].Center, balls[1].Center);

            var p0 = ScaleToClient(balls[0].Center);
            var p1 = ScaleToClient(balls[1].Center);
            var c0 = ScaleToClient(balls[0].Center.Add(new Point(-60, -60)));
            var c1 = ScaleToClient(balls[1].Center.Add(new Point(0, -100)));
            using SKPath path = new();
            path.MoveTo(p0.X, p0.Y);
            path.CubicTo(new SKPoint(c0.X, c0.Y), new SKPoint(c1.X, c1.Y), new SKPoint(p1.X, p1.Y));
            c.DrawPath(path, connectorPaint);

            foreach (var ball in balls)
            {
                DrawBall(c, ball);
            }
        }

        private void DrawBall(SKCanvas c, Ball ball)
        {
            var clientBallBounds = ScaleToClient(ball.Bounds);
            c.DrawBitmap(ball.Bitmap, new SKPoint(clientBallBounds.Left, clientBallBounds.Top));
        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {

            TickCount++;
            if (AlreadyDrawing) return;
            AlreadyDrawing = true;
            GameTimer.Stop();

            Advance();
            skglControl1.Invalidate();

            AlreadyDrawing = false;
            if (TickCount % 50 == 1)
            {
                var totalMs = GameClock.ElapsedMilliseconds;
                FrameRateActual = (float)TickCount / totalMs * 1000;
                var frameRateDelta = FrameRateTarget - FrameRateActual;
                if (frameRateDelta > 0) // slower than desired
                    GameTimer.Interval = Math.Max(1, GameTimer.Interval - 1);
                else if (frameRateDelta < -1) // faster then desired
                    GameTimer.Interval = Math.Min(60, GameTimer.Interval + 1);
                lblStats.Text = $"Refresh Rate: {FrameRateActual:N2}fps, Interval: {GameTimer.Interval}ms";
                lblStats.Left = skglControl1.Right - lblStats.Width;
            }
            GameTimer.Start();
        }

        private void DrawEverything(SKCanvas c)
        {
            DrawGrid(c);
            DrawObjects(c);
        }

        private void DrawGrid(SKCanvas c)
        {
            float gridSpacingMajor = 50;
            float gridSpacingMinor = 10;

            var paintMajor = new SKPaint { Color = Color_MajorGridLine, PathEffect = SKPathEffect.CreateDash(new float[] { 2, 2 }, 0) };
            var paintMinor = new SKPaint { Color = Color_MinorGridLine, PathEffect = SKPathEffect.CreateDash(new float[] { 2, 2 }, 0) };
            var paintAxes = new SKPaint { Color = Color_Axis, StrokeWidth = 2, PathEffect = SKPathEffect.CreateDash(new float[] { 6, 4, 2, 4 }, 0) };

            for (float x = -gridSpacingMinor; x >= ScaleLeft; x -= gridSpacingMinor) DrawLine(c, (x % gridSpacingMajor == 0) ? paintMajor : paintMinor, new PointF(x, ScaleTop), new PointF(x, ScaleBottom));
            for (float x = gridSpacingMinor; x <= ScaleRight; x += gridSpacingMinor) DrawLine(c, (x % gridSpacingMajor == 0) ? paintMajor : paintMinor, new PointF(x, ScaleTop), new PointF(x, ScaleBottom));
            for (float y = gridSpacingMinor; y <= ScaleTop; y += gridSpacingMinor) DrawLine(c, (y % gridSpacingMajor == 0) ? paintMajor : paintMinor, new PointF(ScaleLeft, y), new PointF(ScaleRight, y));
            for (float y = -gridSpacingMinor; y >= ScaleBottom; y -= gridSpacingMinor) DrawLine(c, (y % gridSpacingMajor == 0) ? paintMajor : paintMinor, new PointF(ScaleLeft, y), new PointF(ScaleRight, y));
            DrawLine(c, paintAxes, new PointF(0, 0), new PointF(0, ScaleTop));
            DrawLine(c, paintAxes, new PointF(0, 0), new PointF(0, ScaleBottom));
            DrawLine(c, paintAxes, new PointF(0, 0), new PointF(ScaleLeft, 0));
            DrawLine(c, paintAxes, new PointF(0, 0), new PointF(ScaleRight, 0));
        }

        private void DrawBezier(SKCanvas c)
        {
            
        }

        private void DrawLine(SKCanvas c, SKPaint paint, float x0, float y0, float x1, float y1)
        {
            var sp0 = ScaleToClient(new PointF(x0, y0));
            var sp1 = ScaleToClient(new PointF(x1, y1));
            c.DrawLine(sp0.X, sp0.Y, sp1.X, sp1.Y, paint);
        }

        private void DrawLine(SKCanvas c, SKPaint paint, PointF p0, PointF p1)
        {
            var sp0 = ScaleToClient(p0);
            var sp1 = ScaleToClient(p1);
            c.DrawLine(sp0.X, sp0.Y, sp1.X, sp1.Y, paint);
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
            ScaleFactor = ClientHeight / ScaleHeight;
            balls.ForEach(b => b.SetScale(ScaleFactor));
        }

        private void skglControl1_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintGLSurfaceEventArgs e)
        {
            var c = e.Surface.Canvas;
            c.Clear(Color_Background);
            DrawEverything(c);
        }

        private void SkiaToo_SizeChanged(object sender, EventArgs e)
        {
            SetScale(skglControl1.Bounds);
            lblStats.Left = skglControl1.Right - lblStats.Width;
            skglControl1.Invalidate();
        }

    }
}
