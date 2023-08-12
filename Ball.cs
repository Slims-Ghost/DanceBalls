using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using PointF = Microsoft.Maui.Graphics.PointF;
//using Point = Microsoft.Maui.Graphics.Point;
//using SizeF = Microsoft.Maui.Graphics.SizeF;
//using Size = Microsoft.Maui.Graphics.Size;
//using Color = Microsoft.Maui.Graphics.Color;
namespace DanceBalls
{
    internal class Ball
    {
        public float D { get { return _D; } set { _D = value; SetXY(); } }
        public float Theta { get { return _Theta; } set { _Theta = value; SetXY(); } }
        public float X { get { return _X; } set { _X = value; SetDTheta(); } }
        public float Y { get { return _Y; } set { _Y = value; SetDTheta(); } }
        public float Radius { get; set; }
        public Color Color { get; set; }
        public SKBitmap Bitmap { get; set; }
        public PointF Center { get { return new PointF(X, Y); } }
        public float Diameter { get { return 2 * Radius; } }
        public RectangleF Bounds { get { return new RectangleF(X - Radius, Y + Radius, Diameter, Diameter); } }
        private string ImgPath;
        private float _D;
        private float _Theta;
        private float _X;
        private float _Y;
        private float ScaleFactor;

        //public Ball(float x, float y, float radius, float scaleFactor)
        //{
        //    _X = x;
        //    _Y = y;
        //    Radius= radius;
        //    ScaleFactor = scaleFactor;
        //    Bitmap = new(1, 1);
        //}

        public void SetScale(float ScaleFactor)
        {
            Bitmap = GetBallBitmap(ImgPath, new Size((int)(Radius * 2 * ScaleFactor), (int)(Radius * 2 * ScaleFactor)));
        }

        public Ball(float d, float theta, float radius, string imgPath, float scaleFactor)
        {
            D = d;
            Theta = theta;
            Radius = radius;
            ImgPath = imgPath;
            ScaleFactor = scaleFactor;
            Bitmap = GetBallBitmap(imgPath, new Size((int)(Radius * 2 * ScaleFactor), (int)(Radius * 2 * ScaleFactor)));

        }

        private void SetDTheta()
        {
            _D = (float)Math.Sqrt(X * X + Y * Y);
            _Theta = (float)(Math.Acos(X) + Y < 0 ? Math.PI : 0);
        }

        private void SetXY()
        {
            _X = D * (float)Math.Cos(Theta);
            _Y = D * (float)Math.Sin(Theta);
        }

        private static SKBitmap GetBallBitmap(string imgPath, Size newSize)
        {
            //string imgPath = ;
            string sizedImgPath = imgPath.Replace(".png", "small.png");
            //using FileStream fs = new(imgPath, FileMode.Open);
            Bitmap baseBitmap = (Bitmap)Image.FromFile(imgPath);
            Bitmap sizedBitmap = Helpers.ResizeImage(baseBitmap, newSize.Width, newSize.Height);
            using MemoryStream ms = new();
            //sizedBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            sizedBitmap.Save(sizedImgPath, System.Drawing.Imaging.ImageFormat.Png);
            using FileStream fs = new(sizedImgPath, FileMode.Open);
            var skBitmap = SKBitmap.Decode(fs);
            return skBitmap;
        }
    }
}
