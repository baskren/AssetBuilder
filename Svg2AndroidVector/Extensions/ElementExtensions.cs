using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Svg2AndroidVector
{
    public static class ElementExtensions
    {
        public static string InheritedAttributeValue(this XElement xElement, string attributeName)
        {
            foreach (var ancestor in xElement.Ancestors())
            {
                if (ancestor.Attribute(attributeName) is XAttribute candidate && candidate.Value != "inherit")
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

        public static bool TryGetSvgElementAttributeValueInPx(this XElement element, string attributeName, out float value)
        {
            value = -1;
            if (element == null)
                return false;
            if (element.Name == Namespace.Svg  + "svg"
                && element.Attribute(attributeName) is XAttribute attribute
                && AttributeExtensions.TryGetValueInPx(attribute, out value))
                return true;
            else
            {
                do
                {
                    element = element.Parent;
                } while (element != null && element.Name != Namespace.Svg  + "svg");
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

        public static bool TryGetValueInPx(XElement element, string text, Orientation orientation, out float result)
        {
            double value = double.NaN;
            result = float.NaN;
            if (string.IsNullOrWhiteSpace(text))
                return false;

            var valueText = "";
            string unit = null;
            foreach (var c in text.Trim())
            {
                if (char.IsLetter(c) && char.ToLower(c) != 'e')
                    unit += c;
                else if (string.IsNullOrWhiteSpace(unit))
                    valueText += c;
            }
            if (double.TryParse(valueText, out value))
            {
                result = (float)value;
                if (!string.IsNullOrWhiteSpace(unit))
                {
                    switch (unit)
                    {
                        case "%":
                            if (orientation == Orientation.Vertical)
                                return ElementExtensions.TryGetSvgElementAttributeValueInPx(element.Parent, "height", out result);
                            else
                                return ElementExtensions.TryGetSvgElementAttributeValueInPx(element.Parent, "width", out result);
                        case "in":
                            result *= 96;
                            break;
                        case "mm":
                            result *= 96 / 25.4f;
                            break;
                        case "pc":
                            result *= 96 / 6.0f;
                            break;
                        case "cm":
                            result *= 96 / 2.54f;
                            break;
                        case "pt":
                            result *= 96 / 72f;
                            break;
                        case "px":
                            break;
                        default:
                            throw new ArgumentException("unknown unit [" + unit + "]");
                    }
                }

                //unit = "px";
                return true;
            }
            return false;
        }

        public static List<float> ToFloatList(XElement element, string text)
        {
            var result = new List<float>();
            text = text.Trim();
            if (string.IsNullOrWhiteSpace(text))
                return result;
            var newText = text[0].ToString();
            for (int i = 1; i < text.Length; i++)
            {
                if (text[i] == '-' && char.IsDigit(text[i - 1]))
                    newText += " ";
                newText += text[i];
            }
            var tokens = newText.Split(new char[] { ' ', ',' }).ToList();
            tokens.RemoveAll(t => t == "at");
            tokens.RemoveAll(t => t == "round");

            var orientation = Orientation.Horizontal;
            foreach (var token in tokens)
            {
                if (TryGetValueInPx(element, token, orientation, out float value))
                {
                    result.Add(value);
                    if (orientation == Orientation.Horizontal)
                        orientation = Orientation.Vertical;
                    else
                        orientation = Orientation.Horizontal;
                }
            }
            return result;
        }

    }
}
