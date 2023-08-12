using Microsoft.Maui.Graphics.Skia;
using Microsoft.Maui.Graphics;
using DanceBalls;
using System.Diagnostics;

using PointF = Microsoft.Maui.Graphics.PointF;
using Point = Microsoft.Maui.Graphics.Point;
using SizeF = Microsoft.Maui.Graphics.SizeF;
using Size = Microsoft.Maui.Graphics.Size;
using SkiaSharp;
using Color = Microsoft.Maui.Graphics.Color;
using OpenTK;

namespace TestMauiGraphics
{
    public partial class MauiTest : Form
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
        #endregion

        public MauiTest()
        {
            InitializeComponent();
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
            Ball b1 = new(50, ONE_PI / 4, 10, Path.Combine(AppRoot, "orb.png"), ScaleFactor);
            Ball b2 = new(50, ONE_PI + (ONE_PI / 4), 10, Path.Combine(AppRoot, "blue orb.png"), ScaleFactor);
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

        private void DrawObjects(ICanvas c)
        {

            c.ResetStroke();
            c.StrokeColor = Color.FromRgb(0x0, 0xD0, 0xD0);
            c.StrokeSize = 3;
            //DrawLine(c, balls[0].Center, balls[1].Center);

            foreach (var ball in balls)
            {
                DrawBall(c, ball);
            }



        }

        private void DrawBall(ICanvas c, Ball ball)
        {
            var clientBallTopLeft = ScaleToClient(new PointF(ball.Center.X - ball.Radius, ball.Center.Y + ball.Radius));
            var clientBallSize = ScaleToClient(new SizeF(ball.Diameter, ball.Diameter));
            c.ResetStroke();
            //c.FillColor = ball.Color;
            //c.StrokeColor = ball.Color;
            c.FillEllipse((float)clientBallTopLeft.X, (float)clientBallTopLeft.Y, (float)clientBallSize.Width, (float)clientBallSize.Height);
        }

        private void DrawEverything(ICanvas c)
        {
            DrawGrid(c);
            DrawObjects(c);
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
                //TickCounts.Enqueue(totalMs);
                //while(TickCounts.Count > 100)
                //{
                //    TickCounts.Dequeue();
                //}
                //var elapsedMs = totalMs - TickCounts.Peek();

                //FrameRateActual = elapsedMs == 0 ? 0 : ((float)TickCounts.Count - 1) / elapsedMs * 1000;
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

        private void skglControl1_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintGLSurfaceEventArgs e)
        {
            ICanvas canvas = new SkiaCanvas() { Canvas = e.Surface.Canvas };

            canvas.FillColor = Color.FromRgb(skglControl1.BackColor.R, skglControl1.BackColor.G, skglControl1.BackColor.B);
            canvas.FillRectangle(0, 0, skglControl1.Width, skglControl1.Height);


            DrawEverything(canvas);

            //canvas.StrokeColor = Colors.White.WithAlpha(.5f);
            //canvas.StrokeSize = 2;
            //for (int i = 0; i < 100; i++)
            //{
            //    float x = Random.Shared.Next(skglControl1.Width);
            //    float y = Random.Shared.Next(skglControl1.Height);
            //    float r = Random.Shared.Next(5, 50);
            //    canvas.DrawCircle(x, y, r);
            //}
        }

        private void DrawGrid(ICanvas c)
        {
            float gridSpacingMajor = 50;
            float gridSpacingMinor = 10;

            // Minor gridlines
            c.StrokeColor = Color.FromRgb(0x10, 0x10, 0x40);
            c.StrokeSize = 1;
            c.StrokeDashPattern = new float[] { 2, 2 };
            for (float x = -gridSpacingMinor; x >= ScaleLeft; x -= gridSpacingMinor) if (x % gridSpacingMajor != 0) DrawLine(c, new PointF(x, ScaleTop), new PointF(x, ScaleBottom));
            for (float x = gridSpacingMinor; x <= ScaleRight; x += gridSpacingMinor) if (x % gridSpacingMajor != 0) DrawLine(c, new PointF(x, ScaleTop), new PointF(x, ScaleBottom));
            for (float y = gridSpacingMinor; y <= ScaleTop; y += gridSpacingMinor) if (y % gridSpacingMajor != 0) DrawLine(c, new PointF(ScaleLeft, y), new PointF(ScaleRight, y));
            for (float y = -gridSpacingMinor; y >= ScaleBottom; y -= gridSpacingMinor) if (y % gridSpacingMajor != 0) DrawLine(c, new PointF(ScaleLeft, y), new PointF(ScaleRight, y));

            // Major gridlines
            c.StrokeColor = Color.FromRgb(0x40, 0x40, 0x70);
            c.StrokeSize = 1;
            c.StrokeDashPattern = new float[] { 2, 2 };
            for (float x = -gridSpacingMajor; x >= ScaleLeft; x -= gridSpacingMajor) DrawLine(c, new PointF(x, ScaleTop), new PointF(x, ScaleBottom));
            for (float x = gridSpacingMajor; x <= ScaleRight; x += gridSpacingMajor) DrawLine(c, new PointF(x, ScaleTop), new PointF(x, ScaleBottom));
            for (float y = gridSpacingMajor; y <= ScaleTop; y += gridSpacingMajor) DrawLine(c, new PointF(ScaleLeft, y), new PointF(ScaleRight, y));
            for (float y = -gridSpacingMajor; y >= ScaleBottom; y -= gridSpacingMajor) DrawLine(c, new PointF(ScaleLeft, y), new PointF(ScaleRight, y));

            // Axes
            c.StrokeColor = Color.FromRgb(0x40, 0x40, 0x70);
            c.StrokeSize = 2;
            c.StrokeDashPattern = new float[] { 6, 4, 2, 4 };
            DrawLine(c, new PointF(0, 0), new PointF(ScaleLeft, 0));
            DrawLine(c, new PointF(0, 0), new PointF(ScaleRight, 0));
            DrawLine(c, new PointF(0, 0), new PointF(0, ScaleTop));
            DrawLine(c, new PointF(0, 0), new PointF(0, ScaleBottom));

            c.ResetStroke();
        }

        private void DrawLine(ICanvas c, PointF p0, PointF p1)
        {
            var sp0 = ScaleToClient(p0);
            var sp1 = ScaleToClient(p1);
            c.DrawLine((float)sp0.X, (float)sp0.Y, (float)sp1.X, (float)sp1.Y);
        }

        private void DrawLine(ICanvas c, Color color, PointF p0, PointF p1)
        {
            c.StrokeColor = color;
            DrawLine(c, p0, p1);
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

        private void skglControl1_SizeChanged(object s, EventArgs e) => skglControl1.Invalidate();
    }
}