using OpenTK.Graphics.ES11;
using System.Numerics;

namespace DanceBalls
{
    internal static class DanceBalls
    {

        public static Vector2 ExitVelocity (this Ball o1, Ball o2)
        {
            var n = (o2.Position - o1.Position).Normalize();
            var t = new Vector2(-n.Y, n.X);
            var v1n_b = n.Dot(o1.Speed);
            var v1t_b = t.Dot(o1.Speed);
            var v2n_b = n.Dot(o2.Speed);


            var v1t_a = v1t_b * t;
            var v1n_a = (-v1n_b * n) + (v2n_b * n);
            var v1_a = v1t_a + v1n_a;
            return v1_a;
        }

        public static Vector2 PositionOutsideBumper(this Bogey bogey, Bumper bumper)
        {
            var n = (bogey.Position - bumper.Position).Normalize();
            var farEnough = (n * (bogey.Radius + bumper.Radius * 1.01f));
            var pos = bumper.Position + farEnough;
            return pos;
        }

        public static bool ScaleIntersects(this RectangleF rect1, RectangleF rect2)
        {
            //var x = rect1.ScaleContains(rect2.Top, rect2.Left);

            var x = (
                rect1.ScaleContains(rect2.Left, rect2.Top) || 
                rect1.ScaleContains(rect2.Right, rect2.Top - rect2.Height) ||
                rect2.ScaleContains(rect1.Left, rect1.Top) ||
                rect2.ScaleContains(rect1.Right, rect2.Top - rect2.Height)
                );
            //if (!x) { }
            return x;
        }

        public static bool ScaleContains(this RectangleF rect, float x, float y)
        {
            return x.Between(rect.Left, rect.Right) && y.Between(rect.Top - rect.Height, rect.Top);
        }

        public static Vector2 Normalize(this Vector2 v)
        {
            return new Vector2(v.X / v.Length(), v.Y / v.Length());
        }

        public static float Dot(this Vector2 v1, Vector2 v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y;
        }

        public static bool Contains(this RectangleF rect, Vector2 vec)
        {
            return rect.Contains(vec.X, vec.Y);
        }

        public static bool Between(this int num, int min, int max)
        {
            return num >= min && num <= max;
        }

        public static bool Between(this float num, float min, float max)
        {
            return num >= min && num <= max;
        }

        public static Point Add(this Point p1, Point p2)
        {
            return new Point(p1.X + p2.X, p1.Y + p2.Y);
        }

        public static Point Subtract(this Point p1, Point p2)
        {
            return new Point(p1.X - p2.X, p1.Y - p2.Y);
        }

        public static float DistanceFrom(this Point p1, Point p2)
        {
            return (new Point(p1.X - p2.X, p1.Y - p2.Y)).Magnitude();
        }

        public static float Magnitude(this Point point)
        {
            return (float)Math.Sqrt(point.X * point.X + point.Y * point.Y);
        }

        public static Point Copy(this Point point)
        {
            return new Point(point.X, point.Y);
        }

        public static PointF Add(this PointF p1, PointF p2)
        {
            return new PointF(p1.X + p2.X, p1.Y + p2.Y);
        }

        public static PointF Subtract(this PointF p1, PointF p2)
        {
            return new PointF(p1.X - p2.X, p1.Y - p2.Y);
        }

        public static PointF HalfwayTo(this PointF p1, PointF p2)
        {
            return new PointF(p1.X + p2.X / 2f, p1.Y + p2.Y / 2f);
        }

        public static float DistanceFrom(this PointF p1, PointF p2)
        {
            return (new PointF(p1.X - p2.X, p1.Y - p2.Y)).Magnitude();
        }

        public static float DistanceFrom(this PointF p1, float X, float Y)
        {
            return (new PointF(p1.X - X, p1.Y - Y)).Magnitude();
        }

        public static float Magnitude(this PointF PointF)
        {
            return (float)Math.Sqrt(PointF.X * PointF.X + PointF.Y * PointF.Y);
        }

        public static PointF Copy(this PointF point)
        {
            return new PointF(point.X, point.Y);
        }

        public static string ToCompactString(this PointF point)
        {
            return $"({point.X:n2}, {point.Y:n2})";
        }

        public static string ToCompactString(this Point point)
        {
            return $"({point.X}, {point.Y})";
        }

        public static PointF ToPointF(this Vector2 vector)
        {
            return new PointF(vector.X, vector.Y);
        }

        public static Point Subtract(this Point p1, Point? p2)
        {
            if (p2 == null) return p1;
            return new Point(p1.X - p2.Value.X, p1.Y - p2.Value.Y);
        }

        public static Point? Subtract(this Point? p1, Point? p2)
        {
            if (p1 == null) return null;
            if (p2 == null) return p1;
            return new Point(p1.Value.X - p2.Value.X, p1.Value.Y - p2.Value.Y);
        }

        public static Point Scale(this Point p1, float scaleFactor)
        {
            return new Point((int)(p1.X * scaleFactor), (int)(p1.Y * scaleFactor));
        }

        public static float NormalizeDegrees(this float angleInDegrees)
        {
            angleInDegrees %= 360;
            if (angleInDegrees > 180) angleInDegrees -= 360;
            if (angleInDegrees < -180) angleInDegrees += 360;
            return angleInDegrees;
        }

        public static float NormalizeRadians(this float angleInRadians)
        {
            angleInRadians %= ((float)Math.PI * 2f);
            if (angleInRadians > (float)Math.PI) angleInRadians -= ((float)Math.PI * 2f);
            if (angleInRadians < -(float)Math.PI) angleInRadians += ((float)Math.PI * 2f);
            return angleInRadians;
        }
    }
}
