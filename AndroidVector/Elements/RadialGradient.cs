using System;
using System.Drawing;
using System.Xml.Linq;

namespace AndroidVector
{
    public class RadialGradient : BaseGradient
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

        public float GradientRadius
        {
            get => GetPropertyAttribute<float>();
            set => SetPropertyAttribute(value);
        }

        public RadialGradient()
        {
            this.SetAndroidAttributeValue("type", "radial");
        }

        public override void ApplySvgTransform(Matrix matrix)
        {
            base.ApplySvgTransform(matrix);

            var center = new PointF(CenterX, CenterY);
            var newCenter = matrix.TransformPoint(center);
            CenterX = newCenter.X;
            CenterY = newCenter.Y;

            var radius = new PointF(center.X + GradientRadius, center.Y);
            radius = matrix.TransformPoint(radius);
            var dx = radius.X - CenterX;
            var dy = radius.Y - CenterY;
            var length = (float)Math.Sqrt(dx * dx + dy * dy);

            GradientRadius = length;
        }

        bool hasGradientsBeenMapped;
        public override void MapGradients()
        {
            if (hasGradientsBeenMapped || UserSpaceUnits)
                return;
            if (Parent is AaptAttr aaptElement)
            {
                if (Parent.Parent is Group)
                    aaptElement.Remove();
                else if (Parent.Parent is BaseElement geometryElement)
                {
                    var bounds = geometryElement.GetBounds();
                    if (this.AndroidAttribute("centerX") is XAttribute)
                        CenterX = bounds.Width * CenterX + bounds.Left;
                    else
                        CenterX = bounds.Width * 0.5f + bounds.Left;

                    if (this.AndroidAttribute("centerY") is XAttribute)
                        CenterY = bounds.Height * CenterY + bounds.Top;
                    else
                        CenterY = bounds.Height * 0.5f + bounds.Top;

                    if (this.AndroidAttribute("gradientRadius") is XAttribute)
                        GradientRadius = Math.Min(bounds.Width, bounds.Height) * GradientRadius;
                    else
                        GradientRadius = Math.Min(bounds.Width, bounds.Height) * 0.5f;

                    hasGradientsBeenMapped = true;
                }
            }
        }

    }
}
