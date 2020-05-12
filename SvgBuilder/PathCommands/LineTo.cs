using System;
using System.Drawing;

namespace SvgBuilder
{
    public class LineTo : BasePathCommand
    {
        public new const char Symbol = 'L';

        public PointF Point { get; private set; }

        public LineTo(PointF point, bool relative = false) : base(Symbol, relative)
        {
            Point = point;
        }

        public LineTo(float x, float y, bool relative = false) : base(Symbol, relative)
        {
            Point = new PointF(x, y);
        }

        public static LineTo FromArguments(string s)
        {
            var terms = s.Split(' ');
            if (terms.Length == 3 && char.ToUpper(terms[0][0]) == Symbol)
            {
                var relative = char.IsLower(terms[0][0]);
                if (float.TryParse(terms[1], out float pX) &&
                    float.TryParse(terms[2], out float pY))
                    return new LineTo(pX, pY, relative);
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
