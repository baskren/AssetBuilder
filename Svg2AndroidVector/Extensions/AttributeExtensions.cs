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
                if (char.IsLetter(c) && char.ToLower(c) != 'e')
                    unit += c;
                else if (string.IsNullOrWhiteSpace(unit))
                    valueText += c;
            }
            if (string.IsNullOrWhiteSpace(unit))
                return float.TryParse(valueText, out value);
            var orientation = Orientation.Unknown;
            if (text.Contains("%"))
            {
                switch (attribute.Name.ToString())
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
            return ElementExtensions.TryGetValueInPx(attribute.Parent, text, orientation, out value);
        }
    }
}
