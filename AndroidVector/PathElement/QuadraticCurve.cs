using System;
using System.Collections.Generic;
using System.Drawing;
using AndroidVector.Extensions;
using PdfSharpCore.Drawing;

namespace AndroidVector.PathElement
{
    public class QuadraticCurve : Base

    {
        public new const char Symbol = 'Q';

        public PointF Control1 { get; internal set; }
        public PointF End { get; internal set; }

        public QuadraticCurve(PointF control1, PointF end, bool relative = false) : base(Symbol, relative)
        {
            Control1 = control1;
            End = end;
        }

        public QuadraticCurve(float c1X, float c1Y, float endX, float endY, bool relative = false) : base(Symbol, relative)
        {
            Control1 = new PointF(c1X, c1Y);
            End = new PointF(endX, endY);
        }

        public static List<QuadraticCurve> FromString(string s)
        {
            if (char.ToUpper(s[0]) != Symbol)
                throw new ArgumentException("Invalid use of QuadraticCurve.FromString(" + s + ").");

            int i = 0;
            var v = s.Substring(1).ToFloatList();
            var result = new List<QuadraticCurve>();
            while (v.Count >= i + 4)
                result.Add(new QuadraticCurve(v[i++], v[i++], v[i++], v[i++], char.IsLower(s[0])));
            return result;
        }

        public override string ToString()
        {
            return base.ToString() +
                Control1.X.ToString("0.###") + "," + Control1.Y.ToString("0.###") + " " +
                End.X.ToString("0.###") + "," + End.Y.ToString("0.###");
        }

        public BezierCurve ToBezierCurve(SizeF cursor)
        {
            var cp1x = (float)(cursor.Width + 2.0 / 3.0 * (Control1.X - cursor.Width));
            var cp1y = (float)(cursor.Height + 2.0 / 3.0 * (Control1.Y - cursor.Height));

            var cp2x = (float)(End.X + 2.0 / 3.0 * (Control1.X - End.X));
            var cp2y = (float)(End.Y + 2.0 / 3.0 * (Control1.Y - End.Y));

            return new BezierCurve(new PointF(cp1x, cp1y), new PointF(cp2x, cp2y), End);
        }

        public override SizeF ToAbsolute(SizeF cursor)
        {
            if (IsRelative)
            {
                Control1 += cursor;
                End += cursor;
                IsRelative = false;
            }
            return new SizeF(End);
        }

        public override void ApplySvgTransform(Matrix matrix)
        {
            Control1 = matrix.TransformPoint(Control1);
            End = matrix.TransformPoint(End);
        }

        public override RectangleF GetBounds()
        {
            var left = Math.Min(Control1.X, End.X);
            var right = Math.Max(Control1.X, End.X);
            var top = Math.Min(Control1.Y, End.Y);
            var bottom = Math.Max(Control1.Y, End.Y);
            return new RectangleF(left, top, right - left, bottom - top);
        }

        public override XPoint AddToPath(XGraphicsPath path, XPoint cursor, Base lastPathCommand = null)
        {
            var cp1x = cursor.X + 2.0 / 3.0 * (Control1.X - cursor.X);
            var cp1y = cursor.Y + 2.0 / 3.0 * (Control1.Y - cursor.Y);

            var cp2x = End.X + 2.0 / 3.0 * (Control1.X - End.X);
            var cp2y = End.Y + 2.0 / 3.0 * (Control1.Y - End.Y);


            path.AddBezier(cursor, new XPoint(cp1x, cp1y), new XPoint(cp2x, cp2y), End.ToXPoint());
            return End.ToXPoint();
        }
    }
}
