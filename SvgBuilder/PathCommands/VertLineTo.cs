using System;

namespace SvgBuilder
{
    public class VertLineTo : BasePathCommand
    {
        public new const char Symbol = 'V';

        public float Y { get; private set; }

        public VertLineTo(float y, bool relative = false) : base(Symbol, relative)
        {
            Y = y;
        }

        public static VertLineTo FromArguments(string s)
        {
            var terms = s.Split(' ');
            if (terms.Length == 2 && char.ToUpper(terms[0][0]) == Symbol)
            {
                var relative = char.IsLower(terms[0][0]);
                if (float.TryParse(terms[1], out float y))
                    return new VertLineTo(y, relative);
            }
            return null;
        }

        public override string ToString()
        {
            return base.ToString() + Y;

        }
    }
}
