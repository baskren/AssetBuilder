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
            int i = 0;
            var v = s.Substring(1).ToFloatList();
            float result = 0;
            while (v.Count >= i + 1)
                result += v[i++];
            if (result != 0 || !relative)
                return new HorzLineTo(result, char.IsLower(s[0]));
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
