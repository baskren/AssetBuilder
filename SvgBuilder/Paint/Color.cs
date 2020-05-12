using System;
using System.Drawing;

namespace SvgBuilder.Paint
{
    public class Color : Base
    {
        public Color(string value) : base(value)
        {
        }

        public static Color FromArguments(string s)
        {
            if (s.ToColor() is System.Drawing.Color color)
                return new Color(s);
            return null;
        }
    }
}
