using System;
using System.Collections.Generic;
using System.Drawing;

namespace AndroidVector.PathElement
{
    public class MoveTo : Base

    {
        public new const char Symbol = 'M';

        public PointF End { get; internal set;  }

        public MoveTo(PointF point, bool relative = false) : base(Symbol, relative)
        {
            End = point;
        }

        public MoveTo(float x, float y, bool relative = false) : base(Symbol, relative)
        {
            End = new PointF(x,y);
        }

        public static List<Base> FromString(string s)
        {
            if (char.ToUpper(s[0]) != Symbol)
                throw new ArgumentException("Invalid use of MoveTo.FromString(" + s + ").");

            int i = 0;
            var v = s.Substring(1).ToFloatList();
            var result = new List<Base>();
            while (v.Count >= i + 2)
            {
                if (i==0)
                    result.Add(new MoveTo(v[i++], v[i++], char.IsLower(s[0])));
                else
                    result.Add(new LineTo(v[i++], v[i++], char.IsLower(s[0])));
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

        public override RectangleF GetBounds()
        {
            return new RectangleF(End.X, End.Y, 0, 0);
        }

    }
}
