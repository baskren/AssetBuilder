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
            var relative = char.IsLower(s[0]);
            var terms = s.Substring(1).Trim().Split(new char[] { ',', ' ' });
            var result = new List<LineTo>();
            for (int i = 0; i < terms.Length;)
            {
                if (float.TryParse(terms[i++], out float pX) &&
                    float.TryParse(terms[i++], out float pY))
                    result.Add(new LineTo(pX, pY, relative));
            }
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
    }
}
