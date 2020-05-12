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

        public GradientItem() : base ("item")
        {
        }
    }
}
