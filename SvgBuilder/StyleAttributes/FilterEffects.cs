using System;

namespace SvgBuilder.StyleAttributes
{
    public class EnableBackground : Base<string>
    {
        public const string Symbol = "enable-background";
        public EnableBackground(string value) : base(Symbol, value) { }
        public static EnableBackground FromAttributes(string s)
            => new EnableBackground(s);
    }

    public class Filter : Base<string>
    {
        public const string Symbol = "filter";
        public Filter(string value) : base(Symbol, value) { }
        public static Filter FromAttribures(string s)
            => new Filter(s);
    }

    public class FloodColor : Base<string>
    {
        public const string Symbol = "flood-color";
        public FloodColor(string value) : base(Symbol, value) { }
        public static FloodColor FromAttributes(string s)
            => new FloodColor(s);
    }

    public class FloodOpacity : Base<string>
    {
        public const string Symbol = "flood-opacity";
        public FloodOpacity(string value) : base(Symbol, value) { }
        public static FloodOpacity FromAttributes(string s)
            => new FloodOpacity(s);
    }

    public class LightingColor : Base<string>
    {
        public const string Symbol = "lighting-color";
        public LightingColor(string value) : base(Symbol, value) { }
        public static LightingColor FromAttributes(string s)
            => new LightingColor(s);
    }
}
