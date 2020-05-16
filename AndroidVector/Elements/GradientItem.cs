using System;
using System.Drawing;

namespace AndroidVector
{
    public class GradientItem : BaseElement
    {
        public float Offset
        {
            get => GetPropertyAttribute<float>();
            set => SetPropertyAttribute(value);
        }

        public Color Color
        {
            get => GetColorPropertyAttribute();
            set => SetPropertyAttribute(value);
        }

        public GradientItem(float offset, Color color) : base ("item")
        {
            Offset = offset;
            Color = color;
        }

        public GradientItem(string offset, string color) : base ("item")
        {
            SetPropertyAttribute(offset, nameof(Offset));
            SetPropertyAttribute(color, nameof(Color));
        }
    }
}
