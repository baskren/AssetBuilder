﻿using System;
using System.Drawing;

namespace AndroidVector.PathElement
{
    public class VertLineTo : Base

    {
        public new const char Symbol = 'V';

        public float Y { get; internal set; }

        public VertLineTo(float y, bool relative = false) : base(Symbol, relative)
        {
            Y = y;
        }

        public static VertLineTo FromString(string s)
        {
            if (char.ToUpper(s[0]) != Symbol)
                throw new ArgumentException("Invalid use of VertLineTo.FromString(" + s + ").");

            var relative = char.IsLower(s[0]);
            int i = 0;
            var v = s.Substring(1).ToFloatList();
            float result = 0;
            while (v.Count >= i + 1)
                result += v[i++];
            if (result != 0 || !relative)
                return new VertLineTo(result, char.IsLower(s[0]));
            return null;
        }

        public LineTo ToLine(SizeF cursor)
        {
            if (IsRelative)
            {
                Y += cursor.Height;
                IsRelative = false;
            }
            return new LineTo(cursor.Width, Y);
        }
        public override string ToString()
        {
            return base.ToString() + Y.ToString("0.###");

        }
    }
}
