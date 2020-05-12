using System;
using System.Drawing;

namespace AndroidVector
{
    public class SweepGradient : BaseGradient
    {
        public float CenterX
        {
            get => GetPropertyAttribute<float>();
            set => SetPropertyAttribute(value);
        }

        public float CenterY
        {
            get => GetPropertyAttribute<float>();
            set => SetPropertyAttribute(value);
        }

        public SweepGradient()
        {
        }

        public override void ApplySvgTransform(Matrix matrix)
        {
            base.ApplySvgTransform(matrix);
            var center = new PointF(CenterX, CenterY);
            center = matrix.TransformPoint(center);
            CenterX = center.X;
            CenterY = center.Y;
        }
    }
}
