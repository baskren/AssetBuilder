using System;
namespace AndroidVector
{
    public enum LineCap
    {
        Butt,
        Round,
        Square
    }

    public static class LineCapExtensions
    {
        public static PdfSharpCore.Drawing.XLineCap ToXLineCap(this LineCap lineCap)
        {
            if (lineCap == LineCap.Butt)
                return PdfSharpCore.Drawing.XLineCap.Flat;
            if (lineCap == LineCap.Round)
                return PdfSharpCore.Drawing.XLineCap.Round;
            if (lineCap == LineCap.Square)
                return PdfSharpCore.Drawing.XLineCap.Square;
            throw new Exception("No conversion found from LineCap to XLineCap");
        }
    }
}
