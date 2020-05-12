using System;

namespace SvgBuilder
{
    public class ClosePath : BasePathCommand
    {
        public new const char Symbol = 'Z';

        public ClosePath(bool relative = false) : base(Symbol, relative)
        {
        }
    }
}
