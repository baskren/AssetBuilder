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
            var relative = char.IsLower(s[0]);
            var terms = s.Substring(1).Trim().Split(new char[] { ',', ' ' });
            var result = new List<Base>();
            bool isFirst = true;
            for (int i = 0; i < terms.Length;)
            {
                if (float.TryParse(terms[i++], out float pX) &&
                    float.TryParse(terms[i++], out float pY))
                {
                    if (isFirst)
                        result.Add(new MoveTo(pX, pY, relative));
                    else
                        result.Add(new LineTo(pX, pY, relative));
                    isFirst = false;
                }
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
