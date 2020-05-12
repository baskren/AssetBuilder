using System;

namespace SvgBuilder.StyleAttributes
{
    public class StopColor : Base<System.Drawing.Color>
    {
        public const string Symbol = "stop-color";
        public StopColor(System.Drawing.Color color) : base(Symbol, color) { }
        public static StopColor FromAttributes(string s)
        {
            if (s.ToColor() is System.Drawing.Color color)
                return new StopColor(color);
            return null;
        }
    }

    public class StopOpacity : Base<float>
    {
        public const string Symbol = "stop-opacity";
        public StopOpacity(float value) : base(Symbol, value) { }
        public static StopOpacity FromAtributes(string s)
        {
            if (float.TryParse(s, out float value))
                return new StopOpacity(value);
            return null;
        }
    }
}
