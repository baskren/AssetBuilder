using System;
using System.Xml.Linq;

namespace Svg2AndroidVector
{
    public static class AttributeExtensions
    {
        public static bool TryGetValueInPx(XAttribute attribute, out float value)
        {
            value = float.NaN;
            var text = attribute?.Value;
            if (string.IsNullOrWhiteSpace(text))
                return false;

            var valueText = "";
            string unit = null;
            foreach (var c in text.Trim())
            {
                if (char.IsLetter(c))
                    unit += c;
                else if (string.IsNullOrWhiteSpace(unit))
                    valueText += c;
            }
            if (string.IsNullOrWhiteSpace(unit))
                return float.TryParse(valueText, out value);
            var orientation = Orientation.Unknown;
            if (text.Contains('%'))
            {
                switch(attribute.Name.ToString())
                {
                    case "x":
                    case "cx":
                    case "rx":
                    case "x1":
                    case "x2":
                    case "width":
                        orientation = Orientation.Horizontal;
                        break;
                    case "y":
                    case "cy":
                    case "ry":
                    case "y1":
                    case "y2":
                    case "height":
                        orientation = Orientation.Vertical;
                        break;
                    default:
                        throw new ArgumentException("unexpected % in unit [" + attribute.Name + "] in element <" + attribute.Parent?.Name + " id='" + attribute.Parent?.Attribute("id")?.Value + "'>");
                }
            }
            return TryGetValueInPx(attribute.Parent, text, orientation, out value);
        }

        public static bool TryGetValueInPx(XElement element, string text, Orientation orientation, out float value)
        {
            value = float.NaN;
            if (string.IsNullOrWhiteSpace(text))
                return false;

            var valueText = "";
            string unit = null;
            foreach (var c in text.Trim())
            {
                if (char.IsLetter(c))
                    unit += c;
                else if (string.IsNullOrWhiteSpace(unit))
                    valueText += c;
            }
            if (float.TryParse(valueText, out value))
            {
                if (!string.IsNullOrWhiteSpace(unit))
                {
                    switch (unit)
                    {
                        case "%":
                            if (orientation == Orientation.Horizontal)
                                return ElementExtensions.TryGetSvgElementAttributeValueInPx(element.Parent, "width",out value);
                            else
                                return ElementExtensions.TryGetSvgElementAttributeValueInPx(element.Parent, "height", out value);
                        case "in":
                            value *= 96;
                            break;
                        case "mm":
                            value *= 96 / 25.4f;
                            break;
                        case "pc":
                            value *= 96 / 6.0f;
                            break;
                        case "cm":
                            value *= 96 / 2.54f;
                            break;
                        case "pt":
                            value *= 96 / 72f;
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
    }
}
