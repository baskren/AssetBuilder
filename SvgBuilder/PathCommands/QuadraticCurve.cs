using System;
using System.Drawing;

namespace SvgBuilder
{
    public class QuadraticCurve : BasePathCommand
    {
        public new const char Symbol = 'Q';

        public PointF Control1 { get; private set; }
        public PointF End { get; private set; }

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

        public static QuadraticCurve FromArguments(string s)
        {
            var terms = s.Split(' ');
            if (terms.Length == 5 && char.ToUpper(terms[0][0]) == Symbol)
            {
                var relative = char.IsLower(terms[0][0]);
                if (float.TryParse(terms[1], out float c1X) &&
                    float.TryParse(terms[2], out float c1Y) &&
                    float.TryParse(terms[3], out float endX) &&
                    float.TryParse(terms[4], out float endY))
                    return new QuadraticCurve(c1X, c1Y, endX, endY, relative);
            }
            return null;
        }

        public override string ToString()
        {
            return base.ToString() +
                Control1.X + " " + Control1.Y + " " +
                End.X + " " + End.Y + " ";
        }
    }
}
