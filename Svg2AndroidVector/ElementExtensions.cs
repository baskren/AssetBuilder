using System.Xml.Linq;

namespace Svg2AndroidVector
{
    public static class ElementExtensions
    {
        public static string InheritedAttributeValue(this XElement xElement, XAttribute attribute)
        {
            foreach (var ancestor in xElement.Ancestors())
            {
                if (ancestor.Attribute(attribute.Name) is XAttribute candidate && candidate.Value != "inherit")
                    return candidate.Value;
            }
            return null;
        }

        public static XElement GetRoot(this XElement element)
        {
            do
            {
                if (element.Parent == null)
                    return element;
                element = element?.Parent;
            }
            while (element != null);
            return null;
        }

        public static bool TryGetSvgElementAttributeValueInPx(XElement element, string attributeName, out float value)
        {
            value = -1;
            if (element == null)
                return false;
            if (element.Name == NameSpace.Svg  + "svg"
                && element.Attribute(attributeName) is XAttribute widthAttribute
                && AttributeExtensions.TryGetValueInPx(widthAttribute, out value))
                return true;
            else
            {
                do
                {
                    element = element.Parent;
                } while (element != null && element.Name != NameSpace.Svg  + "svg");
                return TryGetSvgElementAttributeValueInPx(element, attributeName, out value);
            }
        }

        public static void FallbackToParentAttribute(this XElement child, string attributeName)
        {
            if (!(child.Attribute(attributeName) is XAttribute) && child.Parent.Attribute(attributeName) is XAttribute attribute)
                child.SetAttributeValue(attributeName, attribute.Value);
        }

        public static void FallbackToInheritAttribute(this XElement child, string attributeName)
        {
            if (!(child.Attribute(attributeName) is XAttribute))
                child.SetAttributeValue(attributeName, "inherit");
        }

    }
}
