using System;
using System.Collections.Generic;
using System.Drawing;
using AndroidVector.Extensions;
using PdfSharpCore.Drawing;

namespace AndroidVector.PathElement
{
    public class ShortcutBezierCurve : Base

    {
        public new const char Symbol = 'S';

        public PointF Control2 { get; internal set; }
        public PointF End { get; internal set; }

        public ShortcutBezierCurve(PointF control2, PointF end, bool relative = false) : base(Symbol, relative)
        {
            Control2 = control2;
            End = end;
        }

        public ShortcutBezierCurve(float c2X, float c2Y, float endX, float endY, bool relative = false) : base(Symbol, relative)
        {
            Control2 = new PointF(c2X, c2Y);
            End = new PointF(endX, endY);
        }

        public static List<ShortcutBezierCurve> FromString(string s)
        {
            if (char.ToUpper(s[0]) != Symbol)
                throw new ArgumentException("Invalid use of ShortcutBezierCurve.FromString(" + s + ").");

            int i = 0;
            var v = s.Substring(1).ToFloatList();
            var result = new List<ShortcutBezierCurve>();
            while (v.Count >= i + 4)
                result.Add(new ShortcutBezierCurve(v[i++], v[i++], v[i++], v[i++], char.IsLower(s[0])));
            return result;
        }

        public override string ToString()
        {
            return base.ToString() +
                Control2.X.ToString("0.###") + "," + Control2.Y.ToString("0.###") + " " +
                End.X.ToString("0.###") + "," + End.Y.ToString("0.###");
        }

        public override SizeF ToAbsolute(SizeF cursor)
        {
            if (IsRelative)
            {
                Control2 += cursor;
                End += cursor;
                IsRelative = false;
            }
            return new SizeF(End);
        }

        public override void ApplySvgTransform(Matrix matrix)
        {
            Control2 = matrix.TransformPoint(Control2);
            End = matrix.TransformPoint(End);
        }

        public override RectangleF GetBounds()
        {
            var left = Math.Min(Control2.X, End.X);
            var right = Math.Max(Control2.X, End.X);
            var top = Math.Min(Control2.Y, End.Y);
            var bottom = Math.Max(Control2.Y, End.Y);
            return new RectangleF(left, top, right - left, bottom - top);
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



            path.AddBezier(cursor, control1, Control2.ToXPoint(), End.ToXPoint());
            return End.ToXPoint();
        }
    }
}
