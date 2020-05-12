using System;
using System.Collections.Generic;

namespace SvgBuilder.StyleAttributes
{
    public class Clip : Base<string>
    {
        public const string Symbol = "clip";
        public Clip(string value) : base(Symbol, value) { }
        public static Clip FromAttributes(string s)
            => new Clip(s);
    }

    public class ClipPath : Base<List<BasePathCommand>>
    {
        public const string Symbol = "clip-path";
        public ClipPath(List<BasePathCommand> commands) : base(Symbol, commands) { }
        public static ClipPath FromAttributres(string s)
        {
            if (s.ToPathCommands() is List<BasePathCommand> commands)
                return new ClipPath(commands);
            return null;
        }
    }

    public class ClipRule : Base<Enums.Rule>
    {
        public const string Symbol = "clip-rule";
        public ClipRule(Enums.Rule rule) : base(Symbol, rule) { }
        public static ClipRule FromAttributes(string s)
            => EnumWrapperInstanceAttributes<ClipRule, Enums.Rule>(s);
    }

    public class Mask : Base<string>
    {
        public const string Symbol = "mask";
        public Mask(string value) : base(Symbol, value) { }
        public static Mask FromAttributes(string s)
            => new Mask(s);
    }

    public class Opacity : Base<float>
    {
        public const string Symbol = "opaticy";
        public Opacity(float value) : base(Symbol, value) { }
        public static Opacity FromAttributes(string s)
        {
            if (float.TryParse(s, out float value))
                return new Opacity(value);
            return null;
        }
    }
}
