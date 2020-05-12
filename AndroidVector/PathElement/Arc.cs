using System;
using System.Collections.Generic;
using System.Drawing;
using AndroidVector.Extensions;

namespace AndroidVector.PathElement
{
    public class Arc : Base
    {
        public new const char Symbol = 'A';

        public SizeF Radii { get; private set; }

        public float XAxisRotation { get; private set; }

        public bool IsLongArc { get; private set; }

        public bool IsSweep { get; private set; }

        public PointF End { get; internal set; }

        public Arc(float rx, float ry, float xAxisRotation, bool longArc, bool sweep, PointF end, bool relative) : base(Symbol, relative)
        {
            Radii = new SizeF(rx, ry);
            XAxisRotation = xAxisRotation;
            IsLongArc = longArc;
            IsSweep = sweep;
            End = end;
        }

        public Arc(float rx, float ry, float xAxisRotation, bool longArc, bool sweep, float endX, float endY, bool relative) : base(Symbol, relative)
        {
            Radii = new SizeF(rx, ry);
            XAxisRotation = xAxisRotation;
            IsLongArc = longArc;
            IsSweep = sweep;
            End = new PointF(endX, endY);
        }

        public static List<Arc> FromString(string s)
        {
            if (char.ToUpper(s[0]) != Symbol)
                throw new ArgumentException("Invalid use of Arc.FromString(" + s + ").");
            var relative = char.IsLower(s[0]);
            var terms = s.Substring(1).Trim().Split(new char[] { ',', ' ' });
            var result = new List<Arc>();
            for (int i=0;i<terms.Length;)
            {
                if (float.TryParse(terms[i++], out float rx) &&
                    float.TryParse(terms[i++], out float ry) &&
                    float.TryParse(terms[i++], out float rotX) &&
                    int.TryParse(terms[i++], out int longArc) &&
                    int.TryParse(terms[i++], out int sweep) &&
                    float.TryParse(terms[i++], out float endX) &&
                    float.TryParse(terms[i++], out float endY))
                    result.Add(new Arc(rx, ry, rotX, longArc > 0, sweep > 0, endX, endY, relative));
            }
            return result;
        }

        public override string ToString()
        {
            return base.ToString() +
                Radii.Width.ToString("0.###") + "," + Radii.Height.ToString("0.###") + "," + " " +
                XAxisRotation.ToString("0.###") + " " + (IsLongArc ? 1 : 0) + " " + (IsSweep ? 1 : 0) + " " +
                End.X.ToString("0.###") + "," + End.Y.ToString("0.###");
        }

        public override SizeF ToAbsolute(SizeF cursor)
        {
            if (IsRelative)
            {
                End += cursor;
                IsRelative = false;
            }
            return new SizeF(End);
        }



        public List<BezierCurve> ToBezierCurves(SizeF cursor)
        {
            if (IsRelative)
                ToAbsolute(cursor);

            if (Radii.IsEmpty)
                return new List<BezierCurve> { new BezierCurve(new PointF(cursor.Width, cursor.Height), End, End) };

            return ToBezierCurves(new PointF(cursor.Width, cursor.Height), Radii, XAxisRotation, IsLongArc, IsSweep, End, null);
        }

        static List<BezierCurve> ToBezierCurves(PointF start, SizeF r, float angle, bool isLongArc, bool isSweep, PointF end, double[] recursive = null)
        {
            var _120 = MathExtensions.ToRadians(120f);
            var rad = MathExtensions.ToRadians(angle);
            double f1, f2, cx, cy;
            var result = new List<BezierCurve>();

            if (recursive is null)
            {
                var xy = start.Rotate(-rad);
                start.X = xy.X;
                start.Y = xy.Y;
                xy = end.Rotate(-rad);
                end.X = xy.X;
                end.Y = xy.Y;

                var x = (start.X - end.X) / 2;
                var y = (start.Y - end.Y) / 2;
                var h = x * x / (r.Width * r.Width) + y * y / (r.Height * r.Height);
                if (h > 1)
                {
                    h = (float)Math.Sqrt(h);
                    r.Width = h * r.Width;
                    r.Height = h * r.Height;
                }
                var rx2 = r.Width * r.Width;
                var ry2 = r.Height * r.Height;
                var k = (isLongArc == isSweep ? -1 : 1) *
                    Math.Sqrt(Math.Abs((rx2 * ry2 - rx2 * y * y - ry2 * x * x) / (rx2 * y * y + ry2 * x * x)));
                cx = k * r.Width * y / r.Height + (start.X + end.X) / 2;
                cy = k * -r.Height * x / r.Width + (start.Y + end.Y) / 2;
                f1 = Math.Round(Math.Asin((start.Y - cy) / r.Height), 9);
                f2 = Math.Round(Math.Asin((end.Y - cy) / r.Height), 9);

                f1 = start.X < cx ? Math.PI - f1 : f1;
                f2 = end.X < cx ? Math.PI - f2 : f2;
                if (f1 < 0)
                    f1 = Math.PI * 2 + f1;
                if (f2 < 0)
                    f2 = Math.PI * 2 + f2;
                if (isSweep && f1 > f2)
                {
                    f1 -= Math.PI * 2;
                }
                if (!isSweep && f2 > f1)
                {
                    f2 -= Math.PI * 2;
                }
            }
            else
            {
                f1 = recursive[0];
                f2 = recursive[1];
                cx = recursive[2];
                cy = recursive[3];
            }
            var df = f2 - f1;
            if (Math.Abs(df) > _120)
            {
                var f2old = f2;
                var x2old = end.X;
                var y2old = end.Y;
                f2 = f1 + _120 * (isSweep && f2 > f1 ? 1 : -1);
                end.X = (float)(cx + r.Width * Math.Cos(f2));
                end.Y = (float)(cy + r.Height * Math.Sin(f2));
                result = ToBezierCurves(end, r, angle, false, isSweep, new PointF(x2old, y2old), new double[] { f2, f2old, cx, cy });
            }
            df = f2 - f1;
            var c1 = (float)Math.Cos(f1);
            var s1 = (float)Math.Sin(f1);
            var c2 = (float)Math.Cos(f2);
            var s2 = (float)Math.Sin(f2);
            var t = Math.Tan(df / 4);
            var hx = (float)(4 / 3 * r.Width * t);
            var hy = (float)(4 / 3 * r.Height * t);
            var m1 = new PointF(start.X, start.Y);
            var m2 = new PointF(start.X + hx * s1, start.Y - hy * c1 );
            var m3 = new PointF(end.X + hx * s2, end.Y - hy * c2 );
            var m4 = new PointF(end.X, end.Y);
            m2.X = 2 * m1.X - m2.X;
            m2.Y = 2 * m1.Y - m2.Y;

            result.Insert(0, new BezierCurve(m2, m3, m4));

            if (recursive is null)
            {
                foreach (var bezier in result)
                {
                    bezier.Control1.Rotate(rad);
                    bezier.Control2.Rotate(rad);
                    bezier.End.Rotate(rad);
                }
            }
            return result;
        }

    }
}
