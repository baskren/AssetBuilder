using System;
using System.Drawing;

namespace SvgBuilder
{
    public class BezierCurve : BasePathCommand
    {
        public new const char Symbol = 'C';

        public PointF Control1 { get; private set; }
        public PointF Control2 { get; private set; }
        public PointF End { get; private set; }

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

        public static BezierCurve FromArguments(string s)
        {
            var terms = s.Split(' ');
            if (terms.Length == 7 && char.ToUpper(terms[0][0]) == Symbol)
            { 
                var relative = char.IsLower(terms[0][0]);
                if (float.TryParse(terms[1], out float c1X) &&
                    float.TryParse(terms[2], out float c1Y) &&
                    float.TryParse(terms[3], out float c2X) &&
                    float.TryParse(terms[4], out float c2Y) &&
                    float.TryParse(terms[5], out float endX) &&
                    float.TryParse(terms[6], out float endY))
                    return new BezierCurve(c1X, c1Y, c2X, c2Y, endX, endY, relative);
            }
            return null;
        }

        public override string ToString()
        {
            return base.ToString() +
                Control1.X + " " + Control1.Y + " " +
                Control2.X + " " + Control2.Y + " " +
                End.X + " " + End.Y + " ";
        }
    }
}
