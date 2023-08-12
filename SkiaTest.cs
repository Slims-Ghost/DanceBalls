using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK.Graphics.ES20;
using System.Diagnostics;

namespace DanceBalls
{
    public partial class SkiaTest : Form
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

        public SkiaTest()
        {
            InitializeComponent();

            glControl1.Paint += new PaintEventHandler(PaintGLControl!);
            //ScaleHeight drives the rest of the Scale dimensions;
            ScaleHeight = 200f;
            SetScale(glControl1.Bounds);
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
            //Ball b1 = new(45, ONE_PI / 4, 10, Path.Combine(AppRoot, "orb.png"), ScaleFactor);
            //Ball b2 = new(25, ONE_PI + (ONE_PI / 4), 10, Path.Combine(AppRoot, "blue orb.png"), ScaleFactor);
            //balls.Add(b1);
            //balls.Add(b2);
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

        private void DrawObjects(SKSurface s)
        {

            var connectorPaint = new SKPaint { Color = new SKColor(0, 220, 220), StrokeWidth = 3, };
            //DrawLine(s, connectorPaint, balls[0].Center, balls[1].Center);

            foreach (var ball in balls)
            {
                DrawBall(s, ball);
            }



        }

        private void DrawBall(SKSurface s, Ball ball)
        {
            var clientBallBounds = ScaleToClient(ball.Bounds);
            //s.Canvas.DrawBitmap(ball.Bitmap, new SKPoint(clientBallBounds.Left, clientBallBounds.Top));
        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {

            TickCount++;
            if (AlreadyDrawing) return;
            AlreadyDrawing = true;
            GameTimer.Stop();

            Advance();
            glControl1.Invalidate();

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
                lblStats.Left = glControl1.Right - lblStats.Width;
            }
            GameTimer.Start();
        }

        private void DrawEverything(SKSurface s)
        {
            //DrawGrid(s);
            DrawObjects(s);
        }

        private void PaintGLControl(object sender, PaintEventArgs e)
        {
            // make our surface the same size as the user control
            Control sctl = (Control)sender;
            int width = sctl.Width;
            int height = sctl.Height;

            // setup the Skia surface using OpenGL
            SKColorType colorType = SKColorType.Rgba8888;
            using GRContext contextOpenGL = GRContext.Create(GRBackend.OpenGL, GRGlInterface.CreateNativeGlInterface());
            GL.GetInteger(GetPName.FramebufferBinding, out var framebuffer);
            GRGlFramebufferInfo glInfo = new((uint)framebuffer, colorType.ToGlSizedFormat());
            GL.GetInteger(GetPName.StencilBits, out var stencil);
            using GRBackendRenderTarget renderTarget = new(width, height, contextOpenGL.GetMaxSurfaceSampleCount(colorType), stencil, glInfo);
            using SKSurface surface = SKSurface.Create(contextOpenGL, renderTarget, GRSurfaceOrigin.BottomLeft, colorType);

            // clear the background
            surface.Canvas.Clear(new SKColor(glControl1.BackColor.R, glControl1.BackColor.G, glControl1.BackColor.B));
            // redraw everything
            DrawEverything(surface);

            // Force a display
            surface.Canvas.Flush();
            glControl1.SwapBuffers();

            surface.Dispose();
            renderTarget.Dispose();
            contextOpenGL.Dispose();
        }

        private void DrawGrid(SKSurface s)
        {
            float gridSpacingMajor = 50;
            float gridSpacingMinor = 10;

            var paintMajor = new SKPaint { Color = new SKColor(0x40, 0x40, 0x70), PathEffect = SKPathEffect.CreateDash(new float[] { 2, 2 }, 0) };
            var paintMinor = new SKPaint { Color = new SKColor(0x10, 0x10, 0x40), PathEffect = SKPathEffect.CreateDash(new float[] { 2, 2 }, 0) };
            var paintAxes = new SKPaint { Color = new SKColor(0x40, 0x40, 0x70), StrokeWidth = 2, PathEffect = SKPathEffect.CreateDash(new float[] { 6, 4, 2, 4 }, 0) };

            for (float x = -gridSpacingMinor; x >= ScaleLeft; x -= gridSpacingMinor) DrawLine(s, (x % gridSpacingMajor == 0) ? paintMajor : paintMinor, new PointF(x, ScaleTop), new PointF(x, ScaleBottom));
            for (float x = gridSpacingMinor; x <= ScaleRight; x += gridSpacingMinor) DrawLine(s, (x % gridSpacingMajor == 0) ? paintMajor : paintMinor, new PointF(x, ScaleTop), new PointF(x, ScaleBottom));
            for (float y = gridSpacingMinor; y <= ScaleTop; y += gridSpacingMinor) DrawLine(s, (y % gridSpacingMajor == 0) ? paintMajor : paintMinor, new PointF(ScaleLeft, y), new PointF(ScaleRight, y));
            for (float y = -gridSpacingMinor; y >= ScaleBottom; y -= gridSpacingMinor) DrawLine(s, (y % gridSpacingMajor == 0) ? paintMajor : paintMinor, new PointF(ScaleLeft, y), new PointF(ScaleRight, y));
            DrawLine(s, paintAxes, new PointF(0, ScaleTop), new PointF(0, ScaleBottom));
            DrawLine(s, paintAxes, new PointF(ScaleLeft, 0), new PointF(ScaleRight, 0));
        }

        private void DrawLine(SKSurface s, SKPaint paint, float x0, float y0, float x1, float y1)
        {
            var sp0 = ScaleToClient(new PointF(x0, y0));
            var sp1 = ScaleToClient(new PointF(x1, y1));
            s.Canvas.DrawLine(sp0.X, sp0.Y, sp1.X, sp1.Y, paint);
        }

        private void DrawLine(SKSurface s, SKPaint paint, PointF p0, PointF p1)
        {
            var sp0 = ScaleToClient(p0);
            var sp1 = ScaleToClient(p1);
            s.Canvas.DrawLine(sp0.X, sp0.Y, sp1.X, sp1.Y, paint);
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

        private void glControl1_SizeChanged(object sender, EventArgs e)
        {
            SetScale(glControl1.Bounds);
            lblStats.Left = glControl1.Right - lblStats.Width;
        }

        private void glControl1_Click(object sender, EventArgs e)
        {
            GC.Collect();
        }
    }
}
