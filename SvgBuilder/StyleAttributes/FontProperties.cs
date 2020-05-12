using System;
namespace SvgBuilder.StyleAttributes
{
    public class Font : Base<string>
    {
        public const string Symbol = "font";
        public Font(string value) : base(Symbol, value) { }

        public static Font FromAttributes(string s)
            => new Font(s);
    }

    public class FontFamily : Base<string>
    {
        public const string Symbol = "font-family";
        public FontFamily(string family) : base(Symbol, family) { }

        public static FontFamily FromAttributes(string s)
            => new FontFamily(s);
    }

    public class FontSize : Base<string>
    {
        public const string Symbol = "font-size";
        public FontSize(string value) : base(Symbol, value) { }

        public static FontSize FromAttributes(string s)
            => new FontSize(s);
    }

    public class FontSizeAdjust : Base<string>
    {
        public const string Symbol = "font-adjust";
        public FontSizeAdjust(string value) : base(Symbol, value) { }

        public static FontSizeAdjust FromAttributes(string s)
            => new FontSizeAdjust(s);
    }

    public class FontStretch : Base<string>
    {
        public const string Symbol = "font-stretch";
        public FontStretch(string value) : base(Symbol, value) { }

        public static FontStretch FromAttributes(string s)
            => new FontStretch(s);
    }

    public class FontStyle : Base<string>
    {
        public const string Symbol = "font-style";
        public FontStyle(string value) : base(Symbol, value) { }

        public static FontStyle FromAttributes(string s)
            => new FontStyle(s);
    }

    public class FontVariant : Base<string>
    {
        public const string Symbol = "font-variant";
        public FontVariant(string value) : base(Symbol, value) { }

        public static FontVariant FromAttributes(string s)
            => new FontVariant(s);
    }

    public class FontWeight : Base<string>
    {
        public const string Symbol = "font-weight";
        public FontWeight(string value) : base(Symbol, value) { }

        public static FontWeight FromAttributes(string s)
            => new FontWeight(s);
    }
}
