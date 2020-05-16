using System;
using System.Drawing;
using System.Xml;
using System.Xml.Linq;

namespace AndroidVector
{
    public class Vector : Group
    {
        public UnitizedFloat Width
        {
            get => GetPropertyAttribute<UnitizedFloat>();
            set => SetPropertyAttribute(value);
        }

        public UnitizedFloat Height
        {
            get => GetPropertyAttribute<UnitizedFloat>();
            set => SetPropertyAttribute(value);
        }

        public double ViewportWidth
        {
            get => GetPropertyAttribute<double>();
            set => SetPropertyAttribute(value);
        }

        public double ViewportHeight
        {
            get => GetPropertyAttribute<double>();
            set => SetPropertyAttribute(value);
        }

        public Color Tint
        {
            get => GetColorPropertyAttribute();
            set => SetPropertyAttribute(value);
        }

        public TintMode TintMode
        {
            get => GetEnumPropertyAttribute<TintMode>();
            set => SetPropertyAttribute(value);
        }

        public bool AutoMirrored
        {
            get => GetPropertyAttribute<bool>();
            set => SetPropertyAttribute(value);
        }

        public double Alpha
        {
            get => GetPropertyAttribute<double>();
            set => SetPropertyAttribute(value);
        }

        public Vector() : base("vector")
        {
            Add(new XAttribute(XNamespace.Xmlns + "android", Namespace.AndroidVector));
            Add(new XAttribute(XNamespace.Xmlns + "aapt", Namespace.Aapt));
        }
    }
}
