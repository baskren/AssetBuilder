using System;
using System.Drawing;

namespace AndroidVector
{
    public class GradientItem : BaseElement, IComparable
    {
        public float Offset
        {
            get => GetPropertyAttribute<float>(1.0f);
            set => SetPropertyAttribute(value);
        }

        public Color Color
        {
            get => GetColorPropertyAttribute(Color.Black);
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

        public int CompareTo(object obj)
        {
            if (obj is GradientItem other)
                return Offset.CompareTo(other.Offset);
            return -1;
        }
    }
}
