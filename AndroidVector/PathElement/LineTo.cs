using System;
using System.Collections.Generic;
using System.Drawing;

namespace AndroidVector.PathElement
{
    public class LineTo : Base

    {
        public new const char Symbol = 'L';

        public PointF End { get; internal set; }

        public LineTo(PointF point, bool relative = false) : base(Symbol, relative)
        {
            End = point;
        }

        public LineTo(float x, float y, bool relative = false) : base(Symbol, relative)
        {
            End = new PointF(x, y);
        }

        public static List<LineTo> FromString(string s)
        {
            if (char.ToUpper(s[0]) != Symbol)
                throw new ArgumentException("Invalid use of LineTo.FromString(" + s + ").");

            int i = 0;
            var v = s.Substring(1).ToFloatList();
            var result = new List<LineTo>();
            while (v.Count >= i + 2)
                result.Add(new LineTo(v[i++], v[i++], char.IsLower(s[0])));
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

    }
}
