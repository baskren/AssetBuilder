using System;
using System.Drawing;

namespace AndroidVector.PathElement
{
    public class ClosePath : Base

    {
        public new const char Symbol = 'Z';

        public ClosePath(bool relative = false) : base(Symbol, relative)
        {
        }

        public override void ApplySvgTransform(Matrix matrix) { }

        public override SizeF ToAbsolute(SizeF start)
        {
            IsRelative = false;
            return start;
        }
    }
}
