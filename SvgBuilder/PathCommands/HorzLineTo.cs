using System;
namespace SvgBuilder
{
    public class HorzLineTo : BasePathCommand
    {
        public new const char Symbol = 'H';

        public float X { get; private set; }

        public HorzLineTo(float x, bool relative = false) : base(Symbol, relative)
        {
            X = x;
        }

        public static HorzLineTo FromArguments(string s)
        {
            var terms = s.Split(' ');
            if (terms.Length == 2 && char.ToUpper(terms[0][0]) == Symbol)
            {
                var relative = char.IsLower(terms[0][0]);
                if (float.TryParse(terms[1], out float x))
                    return new HorzLineTo(x, relative);
            }
            return null;
        }

        public override string ToString()
        {
            return base.ToString() + X;
        }
    }
}
