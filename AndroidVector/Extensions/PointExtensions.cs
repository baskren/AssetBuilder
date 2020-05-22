using System;
using System.Drawing;
using PdfSharpCore.Drawing;

namespace AndroidVector.Extensions
{
    public static class PointExtensions
    {
        public static PointF Rotate(this PointF point, float radians)
        {
            var x = (float)(point.X * Math.Cos(radians) - point.Y * Math.Sin(radians));
            var y = (float)(point.X * Math.Sin(radians) + point.Y * Math.Cos(radians));
            return new PointF(x, y);
        }

        public static XPoint ToXPoint(this PointF pointF)
            => new XPoint(pointF.X, pointF.Y);

        public static XSize ToXSize(this SizeF sizeF)
            => new XSize(sizeF.Width, sizeF.Height);
    }
}
