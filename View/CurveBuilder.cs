using System;
using System.Collections.Generic;
using System.Windows;

namespace NodeGraph.View
{
    public static class CurveBuilder
    {
        #region Nested type: Curve
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

            public int GetSegmentIndex(Point p, double tolerance)
            {
                var minDist = double.MaxValue;
                Segment closestSegment = null;
                foreach (var segment in segments)
                {
                    var dist = MinDistanceToLine(segment.p0, segment.p3, p);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestSegment = segment;
                    }
                }
                return segments.IndexOf(closestSegment);
            }
            #endregion
        }
        #endregion

        #region Nested type: Segment
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
        #endregion

        #region Fields
        private static readonly double MIN_CONTROL_LENGTH = 50;
        #endregion

        #region Methods
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

        private static double MinDistanceToLine(Point p0, Point p1, Point p)
        {
            // Return minimum distance between line segment vw and point p
            double l2 = LengthSquared(p0, p1);  // i.e. |w-v|^2 -  avoid a sqrt
            if (l2 == 0.0)
                return Distance(p, p0);   // v == w case
            // Consider the line extending the segment, parameterized as v + t (w - v).
            // We find projection of point p onto the line. 
            // It falls where t = [(p-v) . (w-v)] / |w-v|^2
            // We clamp t from [0,1] to handle points outside the segment vw.
            double t = Math.Max(0, Math.Min(1, Dot(Diff(p, p0), Diff(p1, p0)) / l2));
            Point projection = Add(p0, Mult(Diff(p1, p0), t));  // Projection falls on the segment
            return Distance(p, projection);
        }

        private static double Dot(Point v1, Point v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y;
        }

        private static double Distance(Point p0, Point p1)
        {
            var v = Diff(p0, p1);
            return v.Length();
        }

        private static double LengthSquared(Point p0, Point p1)
        {
            var v = Diff(p0, p1);
            return v.X * v.X + v.Y * v.Y;
        }

        public static Curve BuildCurve(Point start, Point end, List<Point> points)
        {
            var curve = new Curve();
            var prev = start;
            var vRight = new Point(1, 0);
            foreach (var cur in points)
            {
                var v = Diff(cur, prev);
                var len = Math.Max(Math.Min(Math.Abs(v.X), Math.Abs(v.Y)), MIN_CONTROL_LENGTH);
                var angle = Angle(v, vRight);
                var vc1 = Rotate(v, -angle);

                vc1 = vc1.Normalize().Mult(len);
                var control1 = Add(prev, vc1);

                var vc2 = Mult(vc1, -1);
                var control2 = Add(cur, vc2);

                var segment = new Segment
                {
                    p0 = prev,
                    p1 = control1,
                    p2 = control2,
                    p3 = cur
                };
                curve.segments.Add(segment);
                prev = cur;
            }
            {
                var v = Diff(end, prev);
                var angle = Angle(v, vRight);
                var vc1 = Rotate(v, -angle);
                var len = Math.Max(Math.Min(Math.Abs(v.X), Math.Abs(v.Y)), MIN_CONTROL_LENGTH);
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