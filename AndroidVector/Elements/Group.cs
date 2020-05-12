using System;
using System.Collections.Generic;

namespace AndroidVector
{
    public class Group : BaseElement
    {
        public double Rotation
        {
            get => GetPropertyAttribute<double>();
            set => SetPropertyAttribute(value);
        }

        public double PivotX
        {
            get => GetPropertyAttribute<double>();
            set => SetPropertyAttribute(value);
        }

        public double PivotY
        {
            get => GetPropertyAttribute<double>();
            set => SetPropertyAttribute(value);
        }

        public double ScaleX
        {
            get => GetPropertyAttribute<double>();
            set => SetPropertyAttribute(value);
        }

        public double ScaleY
        {
            get => GetPropertyAttribute<double>();
            set => SetPropertyAttribute(value);
        }

        public double TranslateX
        {
            get => GetPropertyAttribute<double>();
            set => SetPropertyAttribute(value);
        }

        public double TranslateY
        {
            get => GetPropertyAttribute<double>();
            set => SetPropertyAttribute(value);
        }

        public Group() : base("group") { }

        protected Group(string name) : base(name) { }

        public Group(object content) : base("group", content) { }

        protected Group(string name, object content) : base(name, content) { }

    }
}
