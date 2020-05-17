using System;
using System.Drawing;
using System.Xml.Linq;

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
            this.SetAndroidAttributeValue("type", "linear");
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
                    if (aaptElement.Attribute("name") is XAttribute nameAttribute)
                    {
                        if (nameAttribute.Value.Contains("fillColor"))
                            geometryElement.SetAttributeValue(Namespace.AndroidVector + "fillColor", null);
                        else if (nameAttribute.Value.Contains("strokeColor"))
                            geometryElement.SetAttributeValue(Namespace.AndroidVector + "strokeColor", null);
                    }
                    var bounds = geometryElement.GetBounds();
                    if (this.AndroidAttribute("startX") is XAttribute)
                        StartX = bounds.Width * StartX + bounds.Left;
                    else
                        StartX = bounds.Left;
                    if (this.AndroidAttribute("endX") is XAttribute)
                        EndX = bounds.Width * EndX + bounds.Left;
                    else
                        EndX = bounds.Right;

                    if (this.AndroidAttribute("startY") is XAttribute)
                        StartY = bounds.Height * StartY + bounds.Top;
                    else
                        StartY = bounds.Top;
                    if (this.AndroidAttribute("endY") is XAttribute)
                        EndY = bounds.Height * EndY + bounds.Top;
                    else
                        EndY = bounds.Top;

                    hasGradientsBeenMapped = true;
                }
            }
        }
    }
}
