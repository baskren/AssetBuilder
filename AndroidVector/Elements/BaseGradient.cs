using System;
using System.Drawing;

namespace AndroidVector
{
    public class BaseGradient : BaseElement
    {
        public Color StartColor
        {
            get => GetColorPropertyAttribute();
            set => SetPropertyAttribute(value);
        }

        public Color CenterColor
        {
            get => GetColorPropertyAttribute();
            set => SetPropertyAttribute(value);
        }

        public Color EndColor
        {
            get => GetColorPropertyAttribute();
            set => SetPropertyAttribute(value);
        }

        public GradientType Type
        {
            get => GetEnumPropertyAttribute<GradientType>();
            set => SetPropertyAttribute(value);
        }

        public TileMode TileMode
        {
            get => GetEnumPropertyAttribute<TileMode>();
            set => SetPropertyAttribute(value);
        }

        public bool UserSpaceUnits { get; set; } = false;

        public BaseGradient() : base("gradient")
        {
        }
    }
}
