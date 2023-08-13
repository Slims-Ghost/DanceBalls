using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace DanceBalls
{
    internal class Game
    {
        #region Member vars/props
        public GameState GameState { get; private set; }
        public readonly Stopwatch GameClock = new Stopwatch();
        private long TicksSinceLastUpdate = 0;
        private long LastUpdateTicks = 0;
        private float ClientWidth;
        private float ClientHeight;
        public Rectangle ClientBounds;
        public RectangleF ScaleBounds;
        public PointF ScaleCenter;
        public float AspectRatio;
        public float ScaleHeight;
        public float ScaleWidth;
        public float ScaleLeft;
        public float ScaleRight;
        public float ScaleTop;
        public float ScaleBottom;
        public float ScaleFactor;
        private const float ONE_PI = (float)Math.PI;
        private const float TWO_PI = ONE_PI * 2;
        private const float RAD_TO_DEGREES = 180 / ONE_PI;
        private const float DEGREES_TO_RAD = ONE_PI / 180;
        public static readonly string AppRoot = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
        private float rotationEachSecond = ONE_PI;
        public Bumper Bumper { get; set; }
        public ConcurrentDictionary<int, Bogey> Bogeys = new();
        public ConcurrentQueue<Vector2> BumperSpeedHistory = new();
        private const int BumperSpeedQueueLength = 10;
        public static int BogeySequence = 0;
        private Random rand = new();
        private SKColor Color_MajorGridLine = new(0x40, 0x40, 0x70);
        private SKColor Color_MinorGridLine = new(0x10, 0x10, 0x40);
        private SKColor Color_Axis = new(0x40, 0x40, 0x70);
        private SKColor Color_Background = new(0x0, 0x0, 0x14);
        #endregion

        public Game(Rectangle clientBounds) 
        {
            GameState = GameState.Setup;
            //ScaleHeight drives the rest of the Scale dimensions;
            ScaleHeight = 200f;
            SetScale(clientBounds);
            InitBalls();
            SetScale(clientBounds);

            GameState = GameState.Ready;
            GameClock.Start();
        }

        private void InitBalls()
        {
            Bogeys.Clear();

            Bumper = new(this, 0, 0, 13);


            Bogey b1 = new(this, 50, 50, 10);
            b1.Speed = new Vector2(0, -35);
            AddBogey(b1);

            Bogey b2 = new(this, -50, -25, 15);
            b2.Speed = new Vector2(-20, 20);
            AddBogey(b2);

            AddNewBogey();
            AddNewBogey();
            AddNewBogey();

            var bogeyGen = new BogeyGenerator(this);
            bogeyGen.Start();
        }
        public void AddBogey(Bogey bogey)
        {
            var index = Interlocked.Increment(ref BogeySequence);
            Bogeys.TryAdd(index, bogey);
        }

        public void AddNewBogey()
        {
            AddBogey(NewBogey());
        }

        private Bogey NewBogey() 
        {
            float radius = 10 + 5 * (float)rand.NextDouble();
            // Select a random side
            var side = rand.Next(4);
            Vector2 bogeyPosition;
            if (side == 0) // Top
            {
                bogeyPosition = new Vector2(ScaleLeft + ScaleWidth * (float)rand.NextDouble(), ScaleTop + radius - 1);
            }
            else if (side == 1) // Right
            {
                bogeyPosition = new Vector2(ScaleRight + radius - 1, ScaleTop - ScaleHeight * (float)rand.NextDouble());
            }
            else if (side == 2) // Bottom
            {
                bogeyPosition = new Vector2(ScaleLeft + ScaleWidth * (float)rand.NextDouble(), ScaleBottom - radius + 1);
            }
            else // Left
            {
                bogeyPosition = new Vector2(ScaleLeft - radius + 1, ScaleTop - ScaleHeight * (float)rand.NextDouble());
            }
            var centerPosition = new Vector2(ScaleCenter.X, ScaleCenter.Y);
            var speedDirection = (centerPosition - bogeyPosition).Normalize();
            Vector2 bogeySpeed = speedDirection * (30 + 60 * (float)rand.NextDouble());
            return new Bogey(this, bogeyPosition, radius, bogeySpeed);
        }

        public void Advance()
        {
            TicksSinceLastUpdate = LastUpdateTicks == 0 ? 0 : GameClock.ElapsedTicks - LastUpdateTicks;
            var timeSinceLastUpdate = TimeSpan.FromTicks(TicksSinceLastUpdate);

            Vector2 CurrentBumperSpeed;
            if (Bumper.RequestedPosition != Bumper.Position)
            {
                CurrentBumperSpeed = (Bumper.RequestedPosition - Bumper.Position) / (float)timeSinceLastUpdate.TotalSeconds;
                Bumper.Position = Bumper.RequestedPosition;
            }
            else
            {
                CurrentBumperSpeed = default;
            }
            BumperSpeedHistory.Enqueue(CurrentBumperSpeed);
            while(BumperSpeedHistory.Count > BumperSpeedQueueLength) { BumperSpeedHistory.TryDequeue(out var oldBumperSpeed); }
            var totalSpeed = new Vector2();
            foreach( var s in BumperSpeedHistory)
            {
                totalSpeed += s;
            }
            var avgSpeed = totalSpeed / BumperSpeedHistory.Count;
            Bumper.Speed = avgSpeed;
            //float rotation = (float)timeSinceLastUpdate.TotalSeconds * rotationEachSecond;

            //balls[0].Theta = (balls[0].Theta + rotation).NormalizeRadians();
            //balls[1].Theta = (balls[1].Theta + (rotation * 0.83f)).NormalizeRadians();
            var bogeysOOB = new List<int>();
            foreach (var bogeyEntry in Bogeys)
            {
                var bogey = bogeyEntry.Value;
                var origPos = bogey.Position;
                var newPos = bogey.Position + bogey.Speed * (float)timeSinceLastUpdate.TotalSeconds;
                var newBounds = new RectangleF(newPos.X - bogey.Radius, newPos.Y + bogey.Radius, bogey.Diameter, bogey.Diameter);
                if (!ScaleBounds.ScaleIntersects(newBounds))
                {
                    bogeysOOB.Add(bogeyEntry.Key);
                    continue;
                }
                bogey.Position = newPos;
                if (Bumper.IsCollidingWith(bogey))
                {
                    bogey.IsBeingBumped = true;
                    bogey.Position = bogey.PositionOutsideBumper(Bumper);
                    //bogey.Position = origPos;
                    //bogey.Speed = -bogey.Speed;
                    bogey.Speed = bogey.ExitVelocity(Bumper);

                }
                else
                {
                    bogey.IsBeingBumped = false;
                }
            }

            foreach (int bogeyKey in bogeysOOB)
            {
                Bogeys.Remove(bogeyKey, out var _);
            }

            LastUpdateTicks = GameClock.ElapsedTicks;
        }

        private void DrawObjects(SKCanvas c)
        {
            foreach (var ball in Bogeys.Values)
            {
                DrawBall(c, ball);
            }
            DrawBall(c, Bumper);
        }

        private void DrawBall(SKCanvas c, Ball ball)
        {
            var clientBallBounds = ScaleToClient(ball.Bounds);
            c.DrawBitmap(ball.Bitmap, new SKPoint(clientBallBounds.Left, clientBallBounds.Top));
        }

        public void DrawEverything(SKCanvas c)
        {
            c.Clear(Color_Background);
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
            Bumper?.SetScale(ScaleFactor);
            foreach(var bogey in Bogeys.Values) bogey.SetScale(ScaleFactor);
        }

    }
}
