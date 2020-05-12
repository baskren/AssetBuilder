using System;

namespace SvgBuilder
{
    public class BasePathCommand
    {
        public char Symbol { get; private set; }

        public bool Relative { get; private set; }

        internal BasePathCommand(char symbol, bool relative = false)
        {
            Symbol = char.ToUpper(symbol);
            Relative = relative;
        }

        public bool ValidFirstToken(string s)
        {
            s = s.Trim();
            return char.ToUpper(s[0]) == char.ToUpper(Symbol);
        }

        public override string ToString()
        {
            return (Relative ? char.ToLower(Symbol) : char.ToUpper(Symbol)) + " ";
        }
    }
}
