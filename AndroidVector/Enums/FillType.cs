using System;
using PdfSharpCore.Drawing;

namespace AndroidVector
{
    public enum FillType
    {
        NonZero,
        EvenOdd
    }

    public static class FillTypeExtensions
    {
        public static SkiaSharp.SKPathFillType ToSkPathFillType(this FillType fillType)
                        => fillType == FillType.EvenOdd ? SkiaSharp.SKPathFillType.EvenOdd : SkiaSharp.SKPathFillType.Winding;

        public static XFillMode ToXFillMode(this FillType fillType)
            => fillType == FillType.EvenOdd ? XFillMode.Alternate : XFillMode.Winding;
    }

}
