using System;
using System.Collections.Generic;
using System.Drawing;

namespace AndroidVector.PathElement
{
    public class ShortcutQuadraticCurve : Base

    {
        public new const char Symbol = 'T';

        public PointF End { get; internal set; }

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
            var relative = char.IsLower(s[0]);
            var terms = s.Substring(1).Trim().Split(new char[] { ',', ' ' });
            var result = new List<ShortcutQuadraticCurve>();
            for (int i = 0; i < terms.Length;)
            {
                if (float.TryParse(terms[i++], out float pX) &&
                    float.TryParse(terms[i++], out float pY))
                    result.Add(new ShortcutQuadraticCurve(pX, pY, relative));
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
