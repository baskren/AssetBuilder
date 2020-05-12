using System;
using System.Drawing;

namespace AndroidVector.PathElement
{
    public class HorzLineTo : Base

    {
        public new const char Symbol = 'H';

        public float X { get; internal set; }

        public HorzLineTo(float x, bool relative = false) : base(Symbol, relative)
        {
            X = x;
        }

        public static HorzLineTo FromString(string s)
        {
            if (char.ToUpper(s[0]) != Symbol)
                throw new ArgumentException("Invalid use of HorzLineTo.FromString(" + s + ").");
            var relative = char.IsLower(s[0]);
            var terms = s.Substring(1).Trim().Split(new char[] { ',', ' ' });
            float result = 0;
            foreach (var term in terms)
            {
                if (float.TryParse(term, out float x))
                    result += x;
            }
            if (result != 0 || !relative)
                return new HorzLineTo(result, relative);
            return null;
        }

        public LineTo ToLine(SizeF cursor)
        {
            if (IsRelative)
            {
                X += cursor.Width;
                IsRelative = false;
            }
            return new LineTo(X, cursor.Height);
        }

        public override string ToString()
        {
            return base.ToString() + X.ToString("0.###");
        }
    }
}
