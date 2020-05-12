using System;
using System.Drawing;

namespace SvgBuilder
{
    public class ShortcutQuadraticCurve : BasePathCommand
    {
        public new const char Symbol = 'T';

        public PointF End { get; private set; }

        public ShortcutQuadraticCurve(PointF end, bool relative = false) : base(Symbol, relative)
        {
            End = end;
        }

        public ShortcutQuadraticCurve(float x, float y, bool relative = false) : base(Symbol, relative)
        {
            End = new PointF(x, y);
        }

        public static ShortcutQuadraticCurve FromArguments(string s)
        {
            var terms = s.Split(' ');
            if (terms.Length == 3 && char.ToUpper(terms[0][0]) == Symbol)
            {
                var relative = char.IsLower(terms[0][0]);
                if (float.TryParse(terms[1], out float pX) &&
                    float.TryParse(terms[2], out float pY))
                    return new ShortcutQuadraticCurve(pX, pY, relative);
            }
            return null;
        }

        public override string ToString()
        {
            return base.ToString() +
                End.X + " " + End.Y + " ";
        }
    }
}
