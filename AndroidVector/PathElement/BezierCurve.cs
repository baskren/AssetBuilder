using System;
using System.Collections.Generic;
using System.Drawing;
using AndroidVector.Extensions;
using PdfSharpCore.Drawing;

namespace AndroidVector.PathElement
{
    public class BezierCurve : Base

    {
        public new const char Symbol = 'C';

        public PointF Control1 { get; internal set; }
        public PointF Control2 { get; internal set; }
        public PointF End { get; internal set; }

        public BezierCurve(PointF control1, PointF control2, PointF end, bool relative = false) : base(Symbol, relative)
        {
            Control1 = control1;
            Control2 = control2;
            End = end;
        }

        public BezierCurve(float c1X, float c1Y, float c2X, float c2Y, float endX, float endY, bool relative = false) : base(Symbol, relative)
        {
            Control1 = new PointF(c1X, c1Y);
            Control2 = new PointF(c2X, c2Y);
            End = new PointF(endX, endY);
        }

        public static List<BezierCurve> FromString(string s)
        {
            if (char.ToUpper(s[0]) != Symbol)
                throw new ArgumentException("Invalid use of BezierCurve.FromString(" + s + ").");
            int i = 0;
            var v = s.Substring(1).ToFloatList();
            var result = new List<BezierCurve>();
            while (v.Count >= i + 6)
                result.Add(new BezierCurve(v[i++], v[i++], v[i++], v[i++], v[i++], v[i++], char.IsLower(s[0])));
            return result;
        }

        public override string ToString()
        {
            return base.ToString() +
                Control1.X.ToString("0.###") + "," + Control1.Y.ToString("0.###") + " " +
                Control2.X.ToString("0.###") + "," + Control2.Y.ToString("0.###") + " " +
                End.X.ToString("0.###") + "," + End.Y.ToString("0.###");
        }

        public override SizeF ToAbsolute(SizeF cursor)
        {
            if (IsRelative)
            {
                Control1 += cursor;
                Control2 += cursor;
                End += cursor;
                IsRelative = false;
            }
            return new SizeF(End);
        }

        public override void ApplySvgTransform(Matrix matrix)
        {
            Control1 = matrix.TransformPoint(Control1);
            Control2 = matrix.TransformPoint(Control2);
            End = matrix.TransformPoint(End);
        }

        public override RectangleF GetBounds()
        {
            var left = Math.Min(Control1.X, Math.Min(Control2.X,End.X));
            var right = Math.Max(Control1.X, Math.Max(Control2.X, End.X));
            var top = Math.Min(Control1.Y, Math.Min(Control2.Y, End.Y));
            var bottom = Math.Max(Control1.Y, Math.Max(Control2.Y, End.Y));
            return new RectangleF(left, top, right - left, bottom - top);
        }

        public override XPoint AddToPath(XGraphicsPath path, XPoint cursor, Base lastPathCommand = null)
        {
            path.AddBezier(cursor, Control1.ToXPoint(), Control2.ToXPoint(), End.ToXPoint());
            return End.ToXPoint();
        }
        
    }
}
