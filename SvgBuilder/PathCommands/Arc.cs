using System;
using System.Drawing;

namespace SvgBuilder
{
    public class Arc : BasePathCommand
    {
        public new const char Symbol = 'A';

        public SizeF Radii { get; private set; }

        public float XAxisRotation { get; private set; }

        public bool LongArc { get; private set; }

        public bool Sweep { get; private set; }

        public PointF End { get; private set; }

        public Arc(float rx, float ry, float xAxisRotation, bool longArc, bool sweep, PointF end, bool relative) : base(Symbol, relative)
        {
            Radii = new SizeF(rx, ry);
            XAxisRotation = xAxisRotation;
            LongArc = longArc;
            Sweep = sweep;
            End = end;
        }

        public Arc(float rx, float ry, float xAxisRotation, bool longArc, bool sweep, float endX, float endY, bool relative) : base(Symbol, relative)
        {
            Radii = new SizeF(rx, ry);
            XAxisRotation = xAxisRotation;
            LongArc = longArc;
            Sweep = sweep;
            End = new PointF(endX, endY);
        }

        public static Arc FromArguments(string s)
        {
            var terms = s.Split(' ');
            if (terms.Length == 8 && char.ToUpper(terms[0][0]) == Symbol)
            {
                var relative = char.IsLower(terms[0][0]);
                if (float.TryParse(terms[1], out float rx) &&
                    float.TryParse(terms[2], out float ry) &&
                    float.TryParse(terms[3], out float rotX) &&
                    int.TryParse(terms[4], out int longArc) &&
                    int.TryParse(terms[5], out int sweep) &&
                    float.TryParse(terms[6], out float endX) &&
                    float.TryParse(terms[7], out float endY))
                    return new Arc(rx, ry, rotX, longArc > 0, sweep > 0, endX, endY, relative);
            }
            return null;
        }

        public override string ToString()
        {
            return base.ToString() +
                Radii.Width + " " + Radii.Height + " " +
                XAxisRotation + " " +
                (LongArc ? 1 : 0) + " " + (Sweep ? 1 : 0) + " " +
                End.X + " " + End.Y;
        }
    }
}
