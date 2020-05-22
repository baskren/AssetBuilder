using System;
using System.Drawing;
using PdfSharpCore.Drawing;

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

        public override RectangleF GetBounds()
        {
            return new RectangleF(float.NaN, float.NaN, float.NaN, float.NaN);
        }

        public override XPoint AddToPath(XGraphicsPath path, XPoint cursor, Base lastPathCommand = null)
        {
            path.CloseFigure();
            return cursor;
        }
    }
}
