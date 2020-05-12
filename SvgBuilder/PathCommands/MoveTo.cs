using System;
using System.Drawing;

namespace SvgBuilder
{
    public class MoveTo : BasePathCommand
    {
        public new const char Symbol = 'M';

        public PointF Point { get; private set;  }

        public MoveTo(PointF point, bool relative = false) : base(Symbol, relative)
        {
            Point = point;
        }

        public MoveTo(float x, float y, bool relative = false) : base(Symbol, relative)
        {
            Point = new PointF(x,y);
        }

        public static MoveTo FromArguments(string s)
        {
            var terms = s.Split(' ');
            if (terms.Length == 3 && char.ToUpper(terms[0][0]) == Symbol)
            {
                var relative = char.IsLower(terms[0][0]);
                if (float.TryParse(terms[1], out float pX) &&
                    float.TryParse(terms[2], out float pY))
                    return new MoveTo(pX, pY, relative);
            }
            return null;
        }

        public override string ToString()
        {
            return base.ToString() +
                Point.X + " " + Point.Y;
        }
    }
}
