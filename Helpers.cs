using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DanceBalls
{
    internal static partial class Helpers
    {
        public static SKBitmap ArrayToImage(byte[,,] pixelArray)
        {
            int width = pixelArray.GetLength(1);
            int height = pixelArray.GetLength(0);

            uint[] pixelValues = new uint[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte alpha = 255;
                    byte red = pixelArray[y, x, 0];
                    byte green = pixelArray[y, x, 1];
                    byte blue = pixelArray[y, x, 2];
                    uint pixelValue = (uint)red + (uint)(green << 8) + (uint)(blue << 16) + (uint)(alpha << 24);
                    pixelValues[y * width + x] = pixelValue;
                }
            }

            SKBitmap bitmap = new();
            GCHandle gcHandle = GCHandle.Alloc(pixelValues, GCHandleType.Pinned);
            SKImageInfo info = new(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);

            IntPtr ptr = gcHandle.AddrOfPinnedObject();
            int rowBytes = info.RowBytes;
            bitmap.InstallPixels(info, ptr, rowBytes, delegate { gcHandle.Free(); });

            return bitmap;
        }

        public static byte[,,] ImageToArray(SKBitmap bmp)
        {
            ReadOnlySpan<byte> spn = bmp.GetPixelSpan();
            byte[,,] pixelValues = new byte[bmp.Height, bmp.Width, 3];
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    int offset = (y * bmp.Width + x) * bmp.BytesPerPixel;
                    pixelValues[y, x, 0] = spn[offset + 2];
                    pixelValues[y, x, 1] = spn[offset + 1];
                    pixelValues[y, x, 2] = spn[offset + 0];
                }
            }
            return pixelValues;
        }

        public static float FnBoundedLinear(float x, float min, float max, bool positive = true)
        {
            if ((positive && x >= max) || (!positive && x <= min)) return 1;
            if ((positive && x <= min) || (!positive && x >= max)) return 0;
            if (min == max) return x == min ? 1 : 0;
            var numerator = x - min;
            var denominator = max - min;
            var quotient = numerator / denominator;
            return positive ? quotient : 1 - quotient;
        }

        public static Rectangle GetBestFit(Size containerSize, Size sizeToFit, int margin = 0)
        {
            int allowedWidth;
            int allowedHeight;
            var sizeToFitRatio = (float)sizeToFit.Width / sizeToFit.Height;
            var containerRatio = (float)containerSize.Width / containerSize.Height;
            if (containerRatio < sizeToFitRatio)
            {
                allowedWidth = containerSize.Width - (margin * 2);
                allowedHeight = (int)(allowedWidth * (sizeToFitRatio < 1 ? sizeToFitRatio : 1 / sizeToFitRatio));
            }
            else
            {
                allowedHeight = containerSize.Height - (margin * 2);
                allowedWidth = (int)(allowedHeight * (sizeToFitRatio < 1 ? sizeToFitRatio : 1 / sizeToFitRatio));
            }
            var bestFitSize = new Size(allowedWidth, allowedHeight);
            var bestFitLocation = new Point((containerSize.Width - bestFitSize.Width) / 2, (containerSize.Height - bestFitSize.Height) / 2);
            return new Rectangle(bestFitLocation, bestFitSize);
        }
        public static string GetMethodName()
        {
            return (new StackTrace()).GetFrame(1)!.GetMethod()!.Name;
        }

        public static void SetImageResolution(string imagePath, int resolution)
        {
            using var bitmap = (Bitmap)Image.FromFile(imagePath);
            var imgFormat = bitmap.RawFormat;
            using var newBitmap = new Bitmap(bitmap);
            bitmap.Dispose();
            newBitmap.SetResolution(resolution, resolution);
            newBitmap.Save(imagePath, imgFormat);
        }

        public static Bitmap ResizeImage(Image image, RectangleF rectangle)
        {
            return ResizeImage(image, (int)rectangle.Width, (int)rectangle.Height);
        }

        public static Bitmap ResizeImage(Image image, Rectangle rectangle)
        {
            return ResizeImage(image, rectangle.Width, rectangle.Height);
        }

        public static Bitmap ResizeImage(Image image, Size size)
        {
            return ResizeImage(image, size.Width, size.Height);
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using var graphics = Graphics.FromImage(destImage);
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using var wrapMode = new ImageAttributes();
            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
            graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);

            return destImage;
        }

        public static void BitBlt(Graphics g, Bitmap bmp)
        {
            IntPtr targetDC = g.GetHdc();
            IntPtr sourceDC = CreateCompatibleDC(targetDC);
            IntPtr sourceBitmap = bmp.GetHbitmap();
            IntPtr originalBitmap = SelectObject(sourceDC, sourceBitmap);
            BitBlt(targetDC, 0, 0, bmp.Width, bmp.Height, sourceDC, 0, 0, TernaryRasterOperations.SRCCOPY);
            SelectObject(sourceDC, originalBitmap);
            DeleteObject(originalBitmap);
            DeleteObject(sourceBitmap);
            DeleteDC(sourceDC);
            g.ReleaseHdc(targetDC);
        }

        [LibraryImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hObjSource, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

        [LibraryImport("gdi32.dll", SetLastError = true)]
        public static partial IntPtr CreateCompatibleDC(IntPtr hdc);

        [LibraryImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool DeleteDC(IntPtr hdc);

        [LibraryImport("gdi32.dll", SetLastError = true)]
        public static partial IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [LibraryImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool DeleteObject(IntPtr hObject);

        public enum TernaryRasterOperations : uint
        {
            SRCCOPY = 0x00CC0020,
            SRCPAINT = 0x00EE0086,
            SRCAND = 0x008800C6,
            SRCINVERT = 0x00660046,
            SRCERASE = 0x00440328,
            NOTSRCCOPY = 0x00330008,
            NOTSRCERASE = 0x001100A6,
            MERGECOPY = 0x00C000CA,
            MERGEPAINT = 0x00BB0226,
            PATCOPY = 0x00F00021,
            PATPAINT = 0x00FB0A09,
            PATINVERT = 0x005A0049,
            DSTINVERT = 0x00550009,
            BLACKNESS = 0x00000042,
            WHITENESS = 0x00FF0062,
            CAPTUREBLT = 0x40000000
        }
    }
}
