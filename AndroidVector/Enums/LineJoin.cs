using System;
namespace AndroidVector
{
    public enum LineJoin
    {
        Miter,
        Round,
        Bevel
    }

    public static class LineJoinExtensions
    {
        public static PdfSharpCore.Drawing.XLineJoin ToXLineJoin(this LineJoin lineJoin)
        {
            if (lineJoin == LineJoin.Bevel)
                return PdfSharpCore.Drawing.XLineJoin.Bevel;
            if (lineJoin == LineJoin.Miter)
                return PdfSharpCore.Drawing.XLineJoin.Miter;
            if (lineJoin == LineJoin.Round)
                return PdfSharpCore.Drawing.XLineJoin.Round;
            throw new Exception("Cannot convert LineJoin to XLineJoin.");
        }

        public static SkiaSharp.SKStrokeJoin ToSKLineJoin(this LineJoin lineJoin)
        {
            if (lineJoin == LineJoin.Bevel)
                return SkiaSharp.SKStrokeJoin.Bevel;
            if (lineJoin == LineJoin.Miter)
                return SkiaSharp.SKStrokeJoin.Miter;
            if (lineJoin == LineJoin.Round)
                return SkiaSharp.SKStrokeJoin.Round;
            throw new Exception("Cannot convert LineJoin to XLineJoin.");
        }

    }
}
