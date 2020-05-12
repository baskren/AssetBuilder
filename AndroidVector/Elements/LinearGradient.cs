using System;
using System.Drawing;

namespace AndroidVector
{
    public class LinearGradient : BaseGradient
    {
        public float StartX
        {
            get => GetPropertyAttribute<float>();
            set => SetPropertyAttribute(value);
        }

        public float StartY
        {
            get => GetPropertyAttribute<float>();
            set => SetPropertyAttribute(value);
        }

        public float EndX
        {
            get => GetPropertyAttribute<float>();
            set => SetPropertyAttribute(value);
        }

        public float EndY
        {
            get => GetPropertyAttribute<float>();
            set => SetPropertyAttribute(value);
        }

        public LinearGradient()
        {
        }

        public override void ApplySvgTransform(Matrix matrix)
        {
            base.ApplySvgTransform(matrix);
            var start = new PointF(StartX, StartY);
            start = matrix.TransformPoint(start);
            StartX = start.X;
            StartY = start.Y;

            var end = new PointF(EndX, EndY);
            end = matrix.TransformPoint(end);
            EndX = end.X;
            EndY = end.Y;
        }
    }
}
