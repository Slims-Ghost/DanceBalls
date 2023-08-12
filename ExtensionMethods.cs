using System.Numerics;

namespace DanceBalls
{
    internal static class DanceBalls
    {
        public static bool Between(this int num, int min, int max)
        {
            return num >= min && num <= max;
        }

        public static Point Add(this Point operand1, Point operand2)
        {
            return new Point(operand1.X + operand2.X, operand1.Y + operand2.Y);
        }

        public static Point Subtract(this Point operand1, Point operand2)
        {
            return new Point(operand1.X - operand2.X, operand1.Y - operand2.Y);
        }

        public static float DistanceFrom(this Point operand1, Point operand2)
        {
            return (new Point(operand1.X - operand2.X, operand1.Y - operand2.Y)).Magnitude();
        }

        public static float Magnitude(this Point point)
        {
            return (float)Math.Sqrt(point.X * point.X + point.Y * point.Y);
        }

        public static Point Copy(this Point point)
        {
            return new Point(point.X, point.Y);
        }

        public static PointF Add(this PointF operand1, PointF operand2)
        {
            return new PointF(operand1.X + operand2.X, operand1.Y + operand2.Y);
        }

        public static PointF Subtract(this PointF operand1, PointF operand2)
        {
            return new PointF(operand1.X - operand2.X, operand1.Y - operand2.Y);
        }

        public static PointF HalfwayTo(this PointF operand1, PointF operand2)
        {
            return new PointF(operand1.X + operand2.X / 2f, operand1.Y + operand2.Y / 2f);
        }

        public static float DistanceFrom(this PointF operand1, PointF operand2)
        {
            return (new PointF(operand1.X - operand2.X, operand1.Y - operand2.Y)).Magnitude();
        }

        public static float DistanceFrom(this PointF operand1, float X, float Y)
        {
            return (new PointF(operand1.X - X, operand1.Y - Y)).Magnitude();
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
