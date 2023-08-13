using OpenTK;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace DanceBalls
{
    public partial class SkiaToo : Form
    {
        #region Member vars/props
        private readonly System.Timers.Timer GameTimer = new();
        private bool IsMouseDown { get; set; } = false;
        private Point MouseDownLocation { get; set; }
        private Point MouseDownOffset { get; set; }
        private Rectangle ClientBounds;
        private long TimerTickCount = 0;
        private float ClockMsAtLastTick = 0;
        private bool AlreadyDrawing;
        private float FrameRateNow;
        private float FrameRateTarget = 60;
        private float FrameRateActual = 0;
        private int InitialTimerInterval = 14;
        public ConcurrentQueue<float> FrameRateHistory = new();
        private const int FrameRateQueueLength = 10;
        private Game Game;
        #endregion

        public SkiaToo()
        {
            InitializeComponent();

            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(Screen.AllScreens.Select(s => s.DeviceName).ToArray());
            //var desiredScreen = Screen.AllScreens.FirstOrDefault(s => s.DeviceName.EndsWith("44"));
            //if (desiredScreen != null) Location = desiredScreen.WorkingArea.Location.Add(new Point(20, 20));
            if (Screen.AllScreens.Any(s => s.WorkingArea.Left < -1500))
            {
                Location = new Point(-1500, 200);
                WindowState = FormWindowState.Maximized;
            }

            ClientBounds = new Rectangle(0, 0, skglControl1.Width, skglControl1.Height);
            Game = new(ClientBounds);

            GameTimer.Interval = InitialTimerInterval;
            GameTimer.SynchronizingObject = this;
            GameTimer.Elapsed += Timer_Elapsed;
            GameTimer.Start();
        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            TimerTickCount++;
            if (AlreadyDrawing) return;
            AlreadyDrawing = true;
            GameTimer.Stop();

            Game.Advance();
            skglControl1.Invalidate();

            AlreadyDrawing = false;
            if (TimerTickCount % 40 == 1)
            {
                var totalMs = (float)Game.GameClock.Elapsed.TotalMilliseconds;
                //var ElapsedMsSinceLastTick = (float)Game.GameClock.Elapsed.TotalMilliseconds - ClockMsAtLastTick;
                //ClockMsAtLastTick = (float)Game.GameClock.Elapsed.TotalMilliseconds;
                //FrameRateNow = 1000 / ElapsedMsSinceLastTick;
                
                //var frameRateDelta = FrameRateTarget - avgFrameRate;
                FrameRateActual = TimerTickCount / totalMs * 1000;

                FrameRateHistory.Enqueue(FrameRateActual);
                while (FrameRateHistory.Count > FrameRateQueueLength) { FrameRateHistory.TryDequeue(out var _); }
                var avgFrameRate = FrameRateHistory.Average(r => r);
                this.Text = $"{this.Name} - {nameof(avgFrameRate)}: {avgFrameRate:N2}";

                var frameRateDelta = FrameRateTarget - FrameRateActual;
                if (frameRateDelta > 0) // slower than desired
                    GameTimer.Interval = Math.Max(1, GameTimer.Interval - 1);
                else if (frameRateDelta < -1) // faster then desired
                    GameTimer.Interval = Math.Min(60, GameTimer.Interval + 1);
                lblStats.Text = $"Refresh Rate: {FrameRateActual:N2}fps, Interval: {GameTimer.Interval}ms";
                lblStats.Left = skglControl1.Right - lblStats.Width;
            }
            //if (Game.Bogeys.Any(b => b.Value.IsBeingBumped))
            //{
            //    this.Text = this.Name + " !!BUMP!! ";
            //}
            //else
            //{
            //    this.Text = this.Name + " - BogeyCount: " + Game.Bogeys.Count;
            //}
            //this.Text = this.Name + "Bumper Speed: " + Game.Bumper.Speed.ToString();
            GameTimer.Start();
        }

        private void skglControl1_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintGLSurfaceEventArgs e)
        {
            Game.DrawEverything(e.Surface.Canvas);
        }

        private void SkiaToo_SizeChanged(object sender, EventArgs e)
        {
            ClientBounds = new Rectangle(0, 0, skglControl1.Width, skglControl1.Height);
            Game.SetScale(ClientBounds);
            lblStats.Left = skglControl1.Right - lblStats.Width;
            skglControl1.Invalidate();
        }

        private void skglControl1_MouseDown(object sender, MouseEventArgs e)
        {
            if (Game.GameState != GameState.Ready) return;
            IsMouseDown = true;
            MouseDownLocation = e.Location;
            if (Game.ScaleToClient(Game.Bumper.Center).DistanceFrom(MouseDownLocation) < Game.Bumper.Radius * Game.ScaleFactor)
            {
                Game.Bumper.IsActive = true;
                //this.Text = this.Name + " - Bumper IS active!";
                MouseDownOffset = MouseDownLocation.Subtract(Game.ScaleToClient(Game.Bumper.Center));
            }
            else
            {
                //this.Text = this.Name + " - Bumper is NOT active.";
            }
        }

        private void skglControl1_MouseUp(object sender, MouseEventArgs e)
        {
            CancelMouseDown();
        }

        private void CancelMouseDown()
        {
            IsMouseDown = false;
            Game.Bumper.IsActive = false;
            //this.Text = this.Name + " - Bumper is NOT active.";
            MouseDownLocation = new Point(0, 0);
            MouseDownOffset = new Point(0, 0);
        }

        private void skglControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsMouseDown) return;
            if (!ClientBounds.Contains(e.Location))
            {
                CancelMouseDown();
                return;
            }
            if (Game.Bumper.IsActive)
            {
                Game.Bumper.RequestMoveTo(e.Location, MouseDownOffset);
                //log.DebugAsync($"Current Loc: {p.SelectedPiece.Location}");
                //var newLocation = e.Location.Subtract(MouseOffsetForSelectedPiece);
                //var newBounds = new Rectangle(newLocation, p.SelectedPiece.Size);
                //if (p.Bounds.Contains(newBounds))
                //{
                //    p.SelectedPiece.SetLocation(newLocation);
                //    //log.DebugAsync($"Piece is IN rectangle. New location: {p.SelectedPiece.Location}");

                //}
                //else
                //{
                //    //log.DebugAsync($"Piece is NOT in rectangle.");
                //    if (newBounds.Left < p.Bounds.Left) newLocation.X = p.Bounds.Left;
                //    if (newBounds.Right > p.Bounds.Right) newLocation.X = p.Bounds.Right - p.SelectedPiece.Bounds.Width;
                //    if (newBounds.Top < p.Bounds.Top) newLocation.Y = p.Bounds.Top;
                //    if (newBounds.Bottom > p.Bounds.Bottom) newLocation.Y = p.Bounds.Bottom - p.SelectedPiece.Bounds.Height;
                //    if (newLocation != p.SelectedPiece.Location)
                //    {
                //        p.SelectedPiece.SetLocation(newLocation);
                //        MouseOffsetForSelectedPiece = e.Location.Subtract(p.SelectedPiece.Location);
                //        //log.DebugAsync($"New Location: {p.SelectedPiece.Location}");

                //    }
                //}
            }

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var index = comboBox1.SelectedIndex;
            var selectedScreen = Screen.AllScreens[index];
            this.Location = selectedScreen.WorkingArea.Location;
        }
    }
}
