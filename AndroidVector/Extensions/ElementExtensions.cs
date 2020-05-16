using System;
using System.Xml.Linq;

namespace AndroidVector
{
    public static class ElementExtensions
    {
        public static void SetAndroidAttributeValue(this XElement xElement, string name, object value)
            => xElement.SetAttributeValue(Namespace.AndroidVector + name, value);

        public static XAttribute AndroidAttribute(this XElement xElement, string name)
            => xElement.Attribute(Namespace.AndroidVector + name);

    }
}
