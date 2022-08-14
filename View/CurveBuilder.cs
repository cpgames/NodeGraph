using System;
using System.Collections.Generic;
using System.Windows;

namespace NodeGraph.View
{
    public class Segment
    {
        #region Fields
        public Point p0;
        public Point p1;
        public Point p2;
        public Point p3;
        #endregion

        #region Methods
        public override string ToString()
        {
            return $"M{(int)p0.X},{(int)p0.Y} C{(int)p1.X},{(int)p1.Y} {(int)p2.X},{(int)p2.Y} {(int)p3.X},{(int)p3.Y}";
        }
        #endregion
    }

    public class Curve
    {
        #region Fields
        public List<Segment> segments = new List<Segment>();
        #endregion

        #region Methods
        public override string ToString()
        {
            var str = string.Empty;
            for (var index = 0; index < segments.Count - 1; index++)
            {
                var segment = segments[index];
                str += segment + " ";
            }
            str += segments[segments.Count - 1].ToString();
            return str;
        }
        #endregion
    }

    public static class CurveBuilder
    {
        #region Fields
        private static readonly double TENSION = 0.5;
        #endregion

        #region Methods
        private static double Dot(Point p1, Point p2)
        {
            return p1.X * p2.X + p1.Y * p2.Y;
        }

        private static double Length(this Point p)
        {
            return Math.Sqrt(p.X * p.X + p.Y * p.Y);
        }

        private static Point Diff(Point p1, Point p2)
        {
            return new Point(p1.X - p2.X, p1.Y - p2.Y);
        }

        public static Point Add(Point p1, Point p2)
        {
            return new Point(p1.X + p2.X, p1.Y + p2.Y);
        }

        private static Point Mult(this Point p1, double n)
        {
            return new Point(p1.X * n, p1.Y * n);
        }

        //private static double Angle(Point p1, Point p2)
        //{
        //    var dot = Dot(p1, p2);
        //    var l1 = p1.Length();
        //    var l2 = p2.Length();
        //    return Math.Acos(dot / l1 / l2);
        //}

        private static double Angle(Point p1, Point p2)
        {
            p1 = p1.Normalize();
            p2 = p2.Normalize();
            var atanA = Math.Atan2(p1.X, p1.Y);
            var atanB = Math.Atan2(p2.X, p2.Y);
            var a = atanA - atanB;
            return a;
        }

        private static Point Rotate(Point p, double a)
        {
            a *= -1; //clockwise is positive
            var x1 = p.X * Math.Cos(a) - p.Y * Math.Sin(a);
            var y1 = p.Y * Math.Cos(a) + p.X * Math.Sin(a);
            return new Point(x1, y1);
        }

        private static Point Normalize(this Point p)
        {
            var length = p.Length();
            var x = p.X / length;
            var y = p.Y / length;
            return new Point(x, y);
        }

        public static Curve BuildCurve(Point start, Point end, List<Point> points)
        {
            var curve = new Curve();
            var prev = start;
            var next = points.Count > 0 ? points[0] : end;
            var vStart = Diff(next, prev);
            var vRight = new Point(1, 0);
            var anglePrev = Angle(vStart, vRight);
            for (var index = 0; index < points.Count; index++)
            {
                var a = prev;
                var b = index < points.Count-1 ? points[index+1] : end;
                var c = points[index];
                var ac = Diff(c, a);
                var ca = Diff(a, c);
                var cb = Diff(b, c);
                var len = ac.Length() * TENSION;

                var vc1 = Rotate(ac, -anglePrev);
                
                vc1 = vc1.Normalize().Mult(len);
                var control1 = Add(a, vc1);

                var dA = Math.Abs(Angle(ca, cb));
                var angle = (Math.PI-dA) / 2;
                if (b.X > c.X || b.Y > c.Y)
                    angle *= -1;
                var vc2 = Rotate(ca, angle);
                vc2 = vc2.Normalize().Mult(len);
                var control2 = Add(c, vc2);

                var segment = new Segment
                {
                    p0 = a,
                    p1 = control1,
                    p2 = control2,
                    p3 = c
                };
                curve.segments.Add(segment);
                anglePrev = angle;
                prev = c;
            }
            {
                var ac = Diff(end, prev);
                var vc1 = Rotate(ac, -anglePrev);

                var len = ac.Length() * TENSION;
                vc1 = vc1.Normalize().Mult(len);
                var control1 = Add(prev, vc1);
                var control2 = new Point(end.X - len, end.Y);
                var segment = new Segment
                {
                    p0 = prev,
                    p1 = control1,
                    p2 = control2,
                    p3 = end
                };
                curve.segments.Add(segment);
            }

            return curve;
        }
        #endregion
    }
}