using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DanceBalls
{
    internal class Ball
    {
        public Vector2 Position { get; set; }
        public Vector2 Speed { get; set; }
        public Vector2 Acceleration { get; set; }
        public float Radius { get; set; }
        public Color Color { get; set; }
        public SKBitmap Bitmap { get; set; }
        private readonly object BitmapLocker = new();
        public float X { get { return Position.X; } }
        public float Y { get { return Position.Y; } }
        public PointF Center { get { return new PointF(X, Y); } }
        public float Diameter { get { return 2 * Radius; } }
        public RectangleF Bounds { get { return new RectangleF(X - Radius, Y + Radius, Diameter, Diameter); } }
        protected Game Game { get; set; }
        protected string ImgPath;
        protected float ScaleFactor;

        public void SetScale(float scaleFactor)
        {
            if (scaleFactor == 0 || ImgPath == null) return;
            Bitmap?.Dispose();
            Bitmap = GetBallBitmap(ImgPath, new Size((int)(Radius * 2 * scaleFactor), (int)(Radius * 2 * scaleFactor)));
        }

        public Ball(Game game, float x, float y, float radius, string imgPath)
            : this(game, new Vector2(x, y), radius, imgPath)
        { }

        public Ball(Game game, Vector2 position, float radius, string imgPath)
        {
            Game = game;
            Position = position;
            Radius = radius;
            ImgPath = imgPath;
            SetScale(Game.ScaleFactor);
        }

        public Ball(Game game, Vector2 position, float radius, Vector2 speed, string imgPath)
            : this(game, position, radius, imgPath)
        {
            Speed = speed;
        }

        public bool IsCollidingWith(Ball otherBall)
        {
            return (Center.DistanceFrom(otherBall.Center) < Radius + otherBall.Radius);
        }

        public static bool WouldCollideWith(PointF position, float radius, Ball ball)
        {
            return (position.DistanceFrom(ball.Center) < radius + ball.Radius);
        }

        private SKBitmap GetBallBitmap(string imgPath, Size newSize)
        {
            lock (BitmapLocker)
            {
                Bitmap baseBitmap = (Bitmap)Image.FromFile(imgPath);
                Bitmap sizedBitmap = Helpers.ResizeImage(baseBitmap, newSize.Width, newSize.Height);
                //using MemoryStream ms = new();
                //sizedBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                //var skBitmap = SKBitmap.Decode(ms);

                string sizedImgPath = imgPath.Replace(".png", "small.png");
                try
                {
                    sizedBitmap.Save(sizedImgPath, System.Drawing.Imaging.ImageFormat.Png);
                }
                catch (Exception ex)
                {
                    throw;
                }
                using FileStream fs = new(sizedImgPath, FileMode.Open);
                var skBitmap = SKBitmap.Decode(fs);

                return skBitmap;
            }
        }
    }
}
