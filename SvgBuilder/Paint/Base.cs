using System;

namespace SvgBuilder.Paint
{
    public abstract class Base
    {
        public string Value { get; private set; }

        public Base(string value)
        {
            Value = value;
        }

        public static Base PaintFromAttributes(string s)
        {
            s = s.Trim().ToLower();
            if (s == "none")
                return new None();
            if (s == "currentcolor")
                return new CurrentColor();
            if (s.StartsWith("uri("))
                return new FuncIri(s);
            return null;
        }
    }
}
