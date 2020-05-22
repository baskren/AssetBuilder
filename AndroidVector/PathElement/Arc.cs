using System;
using System.Collections.Generic;
using System.Drawing;
using AndroidVector.Extensions;
using PdfSharpCore.Drawing;

namespace AndroidVector.PathElement
{
    public class Arc : Base
    {
        public new const char Symbol = 'A';

        public SizeF Radii { get; private set; }

        public float XAxisRotation { get; private set; }

        public bool IsLongArc { get; private set; }

        public bool IsSweep { get; private set; }

        public PointF End { get; internal set; }

        public Arc(float rx, float ry, float xAxisRotation, bool longArc, bool sweep, PointF end, bool relative) : base(Symbol, relative)
        {
            Radii = new SizeF(rx, ry);
            XAxisRotation = xAxisRotation;
            IsLongArc = longArc;
            IsSweep = sweep;
            End = end;
        }

        public Arc(float rx, float ry, float xAxisRotation, bool longArc, bool sweep, float endX, float endY, bool relative) : base(Symbol, relative)
        {
            Radii = new SizeF(rx, ry);
            XAxisRotation = xAxisRotation;
            IsLongArc = longArc;
            IsSweep = sweep;
            End = new PointF(endX, endY);
        }

        public static List<Arc> FromString(string s)
        {
            if (char.ToUpper(s[0]) != Symbol)
                throw new ArgumentException("Invalid use of Arc.FromString(" + s + ").");

            int i = 0;
            var v = s.Substring(1).ToFloatList();
            var result = new List<Arc>();
            while (v.Count >= i + 7)
                result.Add(new Arc(v[i++], v[i++], v[i++], v[i++] > 0, v[i++] > 0, v[i++], v[i++], char.IsLower(s[0])));
            return result;
        }

        public override string ToString()
        {
            return base.ToString() +
                Radii.Width.ToString("0.###") + "," + Radii.Height.ToString("0.###") + " " +
                XAxisRotation.ToString("0.###") + " " + (IsLongArc ? 1 : 0) + " " + (IsSweep ? 1 : 0) + " " +
                End.X.ToString("0.###") + "," + End.Y.ToString("0.###");
        }

        public override SizeF ToAbsolute(SizeF cursor)
        {
            Radii = new SizeF(Math.Abs(Radii.Width), Math.Abs(Radii.Height));
            if (IsRelative)
            {
                End += cursor;
                IsRelative = false;
            }
            return new SizeF(End);
        }


        public override void ApplySvgTransform(Matrix matrix)
        {
            var rx = Radii.Width * matrix.ScaleX;
            var ry = Radii.Height * matrix.ScaleY;
            Radii = new SizeF(rx, ry);

            XAxisRotation += matrix.RotationDegrees;

            End = matrix.TransformPoint(End);
        }

        public override RectangleF GetBounds()
        {
            return new RectangleF(End.X - Radii.Width, End.Y - Radii.Height, 2 * Radii.Width,  2 * Radii.Height);
        }

        public override XPoint AddToPath(XGraphicsPath path, XPoint cursor, Base lastPathCommand = null)
        {
            if (Radii.Width == 0 || Radii.Height == 0)
            {
                path.AddLine(cursor, End.ToXPoint());
                return End.ToXPoint();
            }
            if (End.X == cursor.X && End.Y == cursor.Y)
                return cursor;

            path.AddArc(cursor, End.ToXPoint(), Radii.ToXSize(), XAxisRotation, IsLongArc, IsSweep ? XSweepDirection.Clockwise : XSweepDirection.Counterclockwise);
            return End.ToXPoint();
        }

        public List<Base> ToSmallArcs(SizeF cursor)
        {
            if (End.X == cursor.Width && End.Y == cursor.Height)
                return new List<Base>();
            if (Radii.Width == 0 || Radii.Height == 0)
                return new List<Base> { new LineTo(End) };

            if (IsRelative)
                throw new Exception("Should have already converted to Absolute");
            Radii = new SizeF(Math.Abs(Radii.Width), Math.Abs(Radii.Height));

            // Convert to CenterPoint Parameters
            double x1 = cursor.Width;
            double y1 = cursor.Height;
            double x2 = End.X;
            double y2 = End.Y;
            bool fA = IsLongArc;
            bool fS = IsSweep;
            double rx = Radii.Width;
            double ry = Radii.Height;
            double φ = XAxisRotation * Math.PI / 180.0;

            double dx2 = (x1 - x2) / 2.0;
            double dy2 = (y1 - y2) / 2.0;

            double x1_ = Math.Cos(φ) * dx2 + Math.Sin(φ) * dy2;
            double y1_ = -Math.Sin(φ) * dx2 + Math.Cos(φ) * dy2;

            var rxs = rx * rx;
            var rys = ry * ry;
            var x1_s = x1_ * x1_;
            var y1_s = y1_ * y1_;

            var Λ = x1_s / rxs + y1_s / rys;

            if (Λ > 1)
            {
                var s = Math.Sqrt(Λ);
                rx *= s;
                ry *= s;
                rxs = rx * rx;
                rys = ry * ry;
            }

            var f = CenterFactor(rxs, rys, x1_s, y1_s) * (fA==fS ? -1 : 1);

            var cx_ = f * rx * y1_ / ry;
            var cy_ = f * -ry * x1_ / rx;

            double cx = Math.Cos(φ) * cx_ - Math.Sin(φ) * cy_ + (x1 + x2) / 2.0;
            double cy = Math.Sin(φ) * cx_ + Math.Cos(φ) * cy_ + (y1 + y2) / 2.0;

            var xA_ = (x1_ - cx_) / rx;
            var yA_ = (y1_ - cy_) / ry;
            double θ = AngleBetweenVectors(1, 0, xA_, yA_);

            var xB_ = (- x1_ - cx_) / rx;
            var yB_ = (- y1_ - cy_) / ry;
            double Δθ = AngleBetweenVectors(xA_, yA_, xB_, yB_) % (Math.PI * 2);

            if (!fS && Δθ > 0)
                Δθ -= Math.PI * 2;
            else if (fS && Δθ < 0)
                Δθ += Math.PI * 2;

            //if (Math.Abs(Δθ) <= Math.PI / 2)
            //    return new List<Base> { this };

            var result = new List<Base>();
            
            bool last = false;
            while (!last)
            {

                var δθ = Math.Sign(Δθ) * Math.PI / 2;
                if (Math.Abs(Δθ) <= Math.PI / 2)
                {
                    δθ = Δθ;
                    last = true;
                }

                var p1 = PointOnArc(cx, cy, rx, ry, φ, θ);
                var p2 = PointOnArc(cx, cy, rx, ry, φ, θ + δθ);
                
                var η = Math.Tan(δθ / 2);

                var α = (float)(Math.Sin(δθ) * (Math.Sqrt(4 + 3 * η * η) - 1) / 3);
                var m1 = SlopeVectorOnArc(cx, cy, rx, ry, φ, θ);
                var q1 = new PointF(p1.X + α * m1.X, p1.Y + α * m1.Y);

                var m2 = SlopeVectorOnArc(cx, cy, rx, ry, φ, θ + δθ);
                var q2 = new PointF(p2.X - α * m2.X, p2.Y - α * m2.Y);

                var bezier = new BezierCurve(q1, q2, p2);
                result.Add(bezier);

                // pdfSharp arc is broken!
                //var arc = new Arc((float)rx, (float)ry, XAxisRotation, false, IsSweep, (float)p2.X, (float)p2.Y, false);
                //result.Add(arc);

                Δθ -= δθ;
                θ += δθ;
            }
            
            return result;
        }


        PointF SlopeVectorOnArc(double cx, double cy, double rx, double ry, double φ, double θ)
        {
            return new PointF((float)(-rx * Math.Cos(φ) * Math.Sin(θ) - ry * Math.Sin(φ) * Math.Cos(θ)),
                              (float)(-rx * Math.Sin(φ) * Math.Sin(θ) + ry * Math.Cos(φ) * Math.Cos(θ)));
        }


        PointF PointOnArc(double cx, double cy, double rx, double ry, double φ, double θ)
        {
            var x1 = (float)(Math.Cos(φ) * rx * Math.Cos(θ) - Math.Sin(φ) * ry * Math.Sin(θ) + cx);
            var y1 = (float)(Math.Sin(φ) * rx * Math.Cos(θ) + Math.Cos(φ) * ry * Math.Sin(θ) + cy);
            return new PointF(x1, y1);
        }

        double CenterFactor(double rxs, double rys, double x1_s, double y1_s)
        {
            var dq = rxs * y1_s + rys * x1_s;
            var num = rxs * rys - dq;
            return Math.Sqrt(Math.Max(0, num / dq));
        }

        double AngleBetweenVectors(double x1, double y1, double x2, double y2)
        {
            var dotProduct = x1 * x2 + y1 * y2;
            var mag1 = Math.Sqrt(x1 * x1 + y1 * y1);
            var mag2 = Math.Sqrt(x2 * x2 + y2 * y2);

            var sign = (x1 * y2 - y1 * x2) < 0 ? -1 : 1;
            var runByDiagonal = Math.Max(-1, Math.Min(1, dotProduct / (mag1 * mag2)));

            var α = Math.Acos( runByDiagonal ) * sign;
            return α;
        }

    }
}
