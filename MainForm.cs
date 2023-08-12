using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Numerics;
using System.Windows.Forms;

namespace DanceBalls
{
    public partial class MainForm : Form
    {
        private System.Windows.Forms.Timer timer1 = new();
        private Stopwatch sw = new Stopwatch();
        private bool AlreadyDrawing;
        private long TickCount = 0;
        private readonly Game game;

        public MainForm()
        {
            InitializeComponent();

            game = new(pictureBox1.ClientRectangle, pictureBox1.BackColor);

            timer1.Interval = 10;
            timer1.Tick += Timer1_Tick;
            sw.Start();
            timer1.Enabled = true;
        }

        private void Timer1_Tick(object? sender, EventArgs e)
        {
            TickCount++;
            if (AlreadyDrawing) return;
            AlreadyDrawing = true;
            Draw();
            AlreadyDrawing = false;

            if (TickCount % 50 == 1)
            {
                var totalMs = sw.ElapsedMilliseconds;
                var refreshRate = (float)TickCount / totalMs * 1000;
                lblStats.Text = $"Refresh Rate: {refreshRate:N0} Hz";
                lblStats.Left = pictureBox1.Right - lblStats.Width;
            }
        }

        private static (float, float, float, float, float[], int[]) GetStats(List<float> someFloats)
        {
            float avg = someFloats.Average();
            float min = someFloats.Min();
            float max = someFloats.Max();

            int buckets = 10;
            float bucketSize = (max - min) / buckets;
            int[] bucketCounts = new int[buckets];
            for (int i = 0; i < buckets; i++)
            {
                float low = min + i * bucketSize;
                float high = low + bucketSize;
                bucketCounts[i] = someFloats.Where(x => x >= low && (x < high || i == buckets - 1)).Count();
            }

            var array = someFloats.ToArray();
            Array.Sort(array);
            float n = array.Length;
            var deciles = new float[10] {
                array[(int)(0.1f*n)],
                array[(int)(0.2f*n)],
                array[(int)(0.3f*n)],
                array[(int)(0.4f*n)],
                array[(int)(0.5f*n)],
                array[(int)(0.6f*n)],
                array[(int)(0.7f*n)],
                array[(int)(0.8f*n)],
                array[(int)(0.9f*n)],
                array[(int)(n - 1)],
            };
            float sumOfSquaresOfDifferences = someFloats.Select(val => (val - avg) * (val - avg)).Sum();
            float stddev = (float)Math.Sqrt(sumOfSquaresOfDifferences / someFloats.Count);
            return (avg, stddev, min, max, deciles, bucketCounts);
        }

        private async void Draw()
        {
            await game.DrawAsync();
            lock (game.BitmapLocker)
            {
                if (pictureBox1.IsDisposed) return;
                if (game.GameState != GameState.Ready) return;
                using var pbGraphics = pictureBox1.CreateGraphics();
                Helpers.BitBlt(pbGraphics, game.Bitmap);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            if (!AlreadyDrawing) timer1.Enabled = true;
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            HandleResize();
        }

        private void HandleResize()
        {
            game.SetScale(pictureBox1.ClientRectangle);
            lblStats.Left = pictureBox1.Right - lblStats.Width;
        }
    }
}