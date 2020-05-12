using System;

namespace SvgBuilder.StyleAttributes
{
    public class Direction : Base<Enums.Direction>
    {
        public const string Symbol = "direction";
        public Direction(Enums.Direction value) : base(Symbol, value) { }

        public static Direction FromAttributes(string s)
            => EnumWrapperInstanceAttributes<Direction, Enums.Direction>(s);
    }

    public class LetterSpacing : Base<string>
    {
        public const string Symbol = "letter-spacing";
        public LetterSpacing(string value) : base(Symbol, value) { }

        public static LetterSpacing FromAttributes(string s)
            => new LetterSpacing(s);
    }

    public class TextDecoration : Base<Enums.TextDecoration>
    {
        public const string Symbol = "text-decoration";
        public TextDecoration(Enums.TextDecoration value) : base(Symbol, value) { }

        public static TextDecoration FromAttributes(string s)
            => EnumWrapperInstanceAttributes<TextDecoration, Enums.TextDecoration>(s);
    }

    public class UnicodeBidi : Base<Enums.UnicodeBidi>
    {
        public const string Symbol = "unicode-bidi";
        public UnicodeBidi(Enums.UnicodeBidi value) : base(Symbol, value) { }

        public static UnicodeBidi FromAttributes(string s)
            => EnumWrapperInstanceAttributes<UnicodeBidi, Enums.UnicodeBidi>(s);
    }

    public class WordSpacing : Base<string>
    {
        public const string Symbol = "word-spacing";
        public WordSpacing(string value) : base(Symbol, value) { }

        public static WordSpacing FromAttributes(string s)
            => new WordSpacing(s);
    }

    public class TextRendering : Base<Enums.TextRendering>
    {
        public const string Symbol = "stroke-linejoin";
        public TextRendering(Enums.TextRendering value) : base(Symbol, value) { }
        public static TextRendering FromAttributes(string s)
            => EnumWrapperInstanceAttributes<TextRendering, Enums.TextRendering>(s);
    }

    public class AlignmnentBaseline : Base<Enums.BaselineAignment>
    {
        public const string Symbol = "alignment-baseline";
        public AlignmnentBaseline(Enums.BaselineAignment value) : base(Symbol, value) { }
        public static AlignmnentBaseline FromAttributes(string s)
            => EnumWrapperInstanceAttributes<AlignmnentBaseline, Enums.BaselineAignment>(s);
    }

    public class BaselineShift : Base<string>
    {
        public const string Symbol = "baseline-shift";
        public BaselineShift(string value) : base(Symbol, value) { }
        public static BaselineShift FromAttributes(string s)
            => new BaselineShift(s);
    }

    public class DominantBaseline : Base<Enums.DominantBaseline>
    {
        public const string Symbol = "dominant-baseline";
        public DominantBaseline(Enums.DominantBaseline value) : base(Symbol, value) { }
        public static DominantBaseline FromAttributes(string s)
            => EnumWrapperInstanceAttributes<DominantBaseline, Enums.DominantBaseline>(s);
    }

    public class GlyphOrientationHorizontal : Base<string>
    {
        public const string Symbol = "glyph-orientation-horizontal";
        public GlyphOrientationHorizontal(string value) : base(Symbol, value) { }
        public static GlyphOrientationHorizontal FromAttributes(string s)
            => new GlyphOrientationHorizontal(s);
    }

    public class GlyphOrientationVertical : Base<string>
    {
        public const string Symbol = "glyph-orientation-vertical";
        public GlyphOrientationVertical(string value) : base(Symbol, value) { }
        public static GlyphOrientationVertical FromAttributes(string s)
            => new GlyphOrientationVertical(s);
    }

    public class Kerning : Base<string>
    {
        public const string Symbol = "kerning";
        public Kerning(string value) : base(Symbol, value) { }
        public static Kerning FromAttributes(string s)
            => new Kerning(s);
    }

    public class TextAnchor : Base<Enums.Anchor>
    {
        public const string Symbol = "text-anchor";
        public TextAnchor(Enums.Anchor value) : base(Symbol, value) { }
        public static TextAnchor FromAttributes(string s)
            => EnumWrapperInstanceAttributes<TextAnchor, Enums.Anchor>(s);
    }

    public class WritingMode : Base<Enums.WritingMode>
    {
        public const string Symbol = "writing-mode";
        public WritingMode(Enums.WritingMode value) : base(Symbol, value) { }
        public static WritingMode FromAttributes(string s)
            => EnumWrapperInstanceAttributes<WritingMode, Enums.WritingMode>(s);
    }
}
