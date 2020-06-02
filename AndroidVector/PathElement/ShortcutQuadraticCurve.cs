using System;
using System.Collections.Generic;
using System.Drawing;
using AndroidVector.Extensions;
using PdfSharpCore.Drawing;
using SkiaSharp;

namespace AndroidVector.PathElement
{
    public class ShortcutQuadraticCurve : Base

    {
        public new const char Symbol = 'T';

        public PointF End { get; internal set; }

        internal PointF Control2 { get; set; }

        public ShortcutQuadraticCurve(PointF end, bool relative = false) : base(Symbol, relative)
        {
            End = end;
        }

        public ShortcutQuadraticCurve(float x, float y, bool relative = false) : base(Symbol, relative)
        {
            End = new PointF(x, y);
        }

        public static List<ShortcutQuadraticCurve> FromString(string s)
        {
            if (char.ToUpper(s[0]) != Symbol)
                throw new ArgumentException("Invalid use of ShortcutQuadraticCurve.FromString(" + s + ").");

            int i = 0;
            var v = s.Substring(1).ToFloatList();
            var result = new List<ShortcutQuadraticCurve>();
            while (v.Count >= i + 2)
                result.Add(new ShortcutQuadraticCurve(v[i++], v[i++], char.IsLower(s[0])));
            return result;
        }

        public override string ToString()
        {
            return base.ToString() +
                End.X.ToString("0.###") + "," + End.Y.ToString("0.###");
        }

        public override SizeF ToAbsolute(SizeF cursor)
        {
            if (IsRelative)
            {
                End += cursor;
                IsRelative = false;
            }
            return new SizeF(End);
        }

        public override void ApplySvgTransform(Matrix matrix)
        {
            End = matrix.TransformPoint(End);
        }

        public override RectangleF GetBounds()
        {
            return new RectangleF(End.X, End.Y, 0, 0);
        }

        public override XPoint AddToPath(XGraphicsPath path, XPoint cursor, Base lastPathCommand = null)
        {
            var previousControlPoint = new PointF((float)cursor.X, (float)cursor.Y);
            if (lastPathCommand is BezierCurve bezierCurve)
                previousControlPoint = bezierCurve.Control2;
            else if (lastPathCommand is QuadraticCurve quadraticCurve)
                previousControlPoint = quadraticCurve.Control1;
            else if (lastPathCommand is ShortcutBezierCurve shortcutBezierCurve)
                previousControlPoint = shortcutBezierCurve.Control2;
            else if (lastPathCommand is ShortcutQuadraticCurve shortcutQuadraticCurve)
                previousControlPoint = shortcutQuadraticCurve.Control2;
            var deltaX = cursor.X - previousControlPoint.X;
            var deltaY = cursor.Y - previousControlPoint.Y;
            var control1 = new XPoint(cursor.X + deltaX, cursor.Y + deltaY);

            var cp1x = cursor.X + 2.0 / 3.0 * (control1.X - cursor.X);
            var cp1y = cursor.Y + 2.0 / 3.0 * (control1.Y - cursor.Y);

            var cp2x = End.X + 2.0 / 3.0 * (control1.X - End.X);
            var cp2y = End.Y + 2.0 / 3.0 * (control1.Y - End.Y);

            path.AddBezier(cursor, new XPoint(cp1x, cp1y), new XPoint(cp2x, cp2y), End.ToXPoint());
            //path.AddBezier(cursor, control1, control1, End.ToXPoint());

            // for reference by future ShortcutQuadraticCurve elements.
            Control2 = new PointF((float)control1.X, (float)control1.Y);

            return End.ToXPoint();
        }


    }
}
