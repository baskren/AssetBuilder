using System;
using System.Collections.Generic;
using System.Drawing;

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
            var relative = char.IsLower(s[0]);
            var terms = s.Substring(1).Trim().Split(new char[] { ',', ' ' });
            var result = new List<BezierCurve>();
            for (int i = 0; i < terms.Length;)
            {
                if (float.TryParse(terms[i++], out float c1X) &&
                    float.TryParse(terms[i++], out float c1Y) &&
                    float.TryParse(terms[i++], out float c2X) &&
                    float.TryParse(terms[i++], out float c2Y) &&
                    float.TryParse(terms[i++], out float endX) &&
                    float.TryParse(terms[i++], out float endY))
                    result.Add( new BezierCurve(c1X, c1Y, c2X, c2Y, endX, endY, relative));
            }
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
    }
}
