using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DanceBalls
{
    [DebuggerDisplay("({x1}, {y1}) -> ({x2}, {y2})")]
    internal class Line
    {
        public PointF p1;
        public PointF p2;
        public float Length { get { return p2.DistanceFrom(p1); } }
        public PointF Midpoint { get { return new PointF((x1 + x2) / 2, (y1 + y2) / 2); } }
        public float x1 { get { return p1.X; } }
        public float y1 { get { return p1.Y; } }
        public float x2 { get { return p2.X; } }
        public float y2 { get { return p2.Y; } }
        public float m { get { return GetSlope(); } }
        public float b { get { return y1 - (m * x1); } }
        public float theta { get { return GetAngle(); } }

        public Line(PointF p1, PointF p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }

        public float GetSlope()
        {
            if (x1 == x2) return y1 < y2 ? float.PositiveInfinity : float.NegativeInfinity;
            return (y2 - y1) / (x2 - x1);
        }

        public float GetAngle()
        {
            var angle = (float)Math.Atan(m);
            if (y2 <= y1 && x2 < x1) //Q3
                angle += (float)Math.PI;
            else if (y2 > y1 && x2 < x1) //Q2
                angle += (float)Math.PI;
            angle = angle.NormalizeRadians();
            return angle;
        }

        public PointF GetPointAtDistance(float d, bool fromStart = true)
        {
            float deltaX = (d / (float)Math.Sqrt(1 + m * m)) * (x2 > x1 ? 1 : -1);
            float xd = fromStart ? x1 + deltaX : x2 - deltaX;
            float yd = m * xd + b;
            return new PointF(xd, yd);
        }
    }
}
