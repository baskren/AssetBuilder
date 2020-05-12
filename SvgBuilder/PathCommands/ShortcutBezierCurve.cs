using System;
using System.Drawing;

namespace SvgBuilder
{
    public class ShortcutBezierCurve : BasePathCommand
    {
        public new const char Symbol = 'S';

        public PointF Control2 { get; private set; }
        public PointF End { get; private set; }

        public ShortcutBezierCurve(PointF control2, PointF end, bool relative = false) : base(Symbol, relative)
        {
            Control2 = control2;
            End = end;
        }

        public ShortcutBezierCurve(float c2X, float c2Y, float endX, float endY, bool relative = false) : base(Symbol, relative)
        {
            Control2 = new PointF(c2X, c2Y);
            End = new PointF(endX, endY);
        }

        public static ShortcutBezierCurve FromArguments(string s)
        {
            var terms = s.Split(' ');
            if (terms.Length == 5 && char.ToUpper(terms[0][0]) == Symbol)
            {
                var relative = char.IsLower(terms[0][0]);
                if (float.TryParse(terms[1], out float c2X) &&
                    float.TryParse(terms[2], out float c2Y) &&
                    float.TryParse(terms[3], out float endX) &&
                    float.TryParse(terms[4], out float endY))
                    return new ShortcutBezierCurve(c2X, c2Y, endX, endY, relative);
            }
            return null;
        }

        public override string ToString()
        {
            return base.ToString() +
                Control2.X + " " + Control2.Y + " " +
                End.X + " " + End.Y + " ";
        }
    }
}
