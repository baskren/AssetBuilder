﻿using System;
using System.Collections.Generic;
using System.Drawing;
using AndroidVector.Extensions;
using PdfSharpCore.Drawing;

namespace AndroidVector.PathElement
{
    public class QuadraticCurve : Base

    {
        public new const char Symbol = 'Q';

        public PointF Control1 { get; internal set; }
        public PointF End { get; internal set; }

        public QuadraticCurve(PointF control1, PointF end, bool relative = false) : base(Symbol, relative)
        {
            Control1 = control1;
            End = end;
        }

        public QuadraticCurve(float c1X, float c1Y, float endX, float endY, bool relative = false) : base(Symbol, relative)
        {
            Control1 = new PointF(c1X, c1Y);
            End = new PointF(endX, endY);
        }

        public static List<QuadraticCurve> FromString(string s)
        {
            if (char.ToUpper(s[0]) != Symbol)
                throw new ArgumentException("Invalid use of QuadraticCurve.FromString(" + s + ").");

            int i = 0;
            var v = s.Substring(1).ToFloatList();
            var result = new List<QuadraticCurve>();
            while (v.Count >= i + 4)
                result.Add(new QuadraticCurve(v[i++], v[i++], v[i++], v[i++], char.IsLower(s[0])));
            return result;
        }

        public override string ToString()
        {
            return base.ToString() +
                Control1.X.ToString("0.###") + "," + Control1.Y.ToString("0.###") + " " +
                End.X.ToString("0.###") + "," + End.Y.ToString("0.###");
        }

        public override SizeF ToAbsolute(SizeF cursor)
        {
            if (IsRelative)
            {
                Control1 += cursor;
                End += cursor;
                IsRelative = false;
            }
            return new SizeF(End);
        }

        public override void ApplySvgTransform(Matrix matrix)
        {
            Control1 = matrix.TransformPoint(Control1);
            End = matrix.TransformPoint(End);
        }

        public override RectangleF GetBounds()
        {
            var left = Math.Min(Control1.X, End.X);
            var right = Math.Max(Control1.X, End.X);
            var top = Math.Min(Control1.Y, End.Y);
            var bottom = Math.Max(Control1.Y, End.Y);
            return new RectangleF(left, top, right - left, bottom - top);
        }

        public override XPoint AddToPath(XGraphicsPath path, XPoint cursor, Base lastPathCommand = null)
        {
            path.AddBezier(cursor, Control1.ToXPoint(), Control1.ToXPoint(), End.ToXPoint());
            return End.ToXPoint();
        }
    }
}
