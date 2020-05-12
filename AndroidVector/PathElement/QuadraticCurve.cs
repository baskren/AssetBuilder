using System;
using System.Collections.Generic;
using System.Drawing;

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
            var relative = char.IsLower(s[0]);
            var terms = s.Substring(1).Trim().Split(new char[] { ',', ' ' });
            var result = new List<QuadraticCurve>();
            for (int i = 0; i < terms.Length;)
            {
                if (float.TryParse(terms[i++], out float c1X) &&
                    float.TryParse(terms[i++], out float c1Y) &&
                    float.TryParse(terms[i++], out float endX) &&
                    float.TryParse(terms[i++], out float endY))
                    result.Add( new QuadraticCurve(c1X, c1Y, endX, endY, relative));
            }
            return result;
        }

        public override string ToString()
        {
            return base.ToString() +
                Control1.X.ToString("0.###") + "," + Control1.Y.ToString("0.###") + " " +
                End.X.ToString("0.###") + "," + End.Y.ToString("0.###");
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
    }
}
