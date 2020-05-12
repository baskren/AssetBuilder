using System;
using System.Xml.Linq;
using System.Collections.Generic;
using AndroidVector;

namespace Svg2AndroidVector
{
    public static class StyleConverter
    {
        public static Dictionary<string, string> CssStyles = new Dictionary<string, string>();

        public static void AddCssStyles(XElement stylesElement, List<string> warnings)
        {
            var text = stylesElement.Value;
            var lines = text.Split(new[] { '\r', '\n' });
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var parts = line.Trim().Split('{');
                    var selectors = parts[0].Split(',');
                    var styles = parts[1].Trim('}').Trim();
                    foreach (var selector in selectors)
                    {
                        if (!string.IsNullOrWhiteSpace(selector))
                        {
                            CssStyles[selector.Trim()] = styles;
                        }
                    }
                }
            }
        }

        public static Dictionary<string, string> AttributeMap = new Dictionary<string, string>
        {
            { "fill", "fillColor" },
            { "fill-rule", "fillType"},
            { "fill-opacity", "fillAlpha" },

            { "stroke", "strokeColor" },
            { "stroke-width", "strokeWidth" },
            { "stroke-opacity", "strokeAlpha" },
            { "stroke-linecap", "strokeLineCap"},
            { "stroke-linejoin", "strokeLineJoin"},
            { "stroke-miterlimit", "strokeMiterLimit"},
        };

        public static List<string> IgnoreAttributeMap = new List<string>
        {
            "font-size",
            "line-height",
            "font-family",
            "-inkscape-font-specification",
            "text-align",
            "white-space",
            "shape-inside",
            "clip-rule"
        };

        public static void ProcessStyleAttribute(XAttribute styleAttribute, BaseElement avElement, List<string> warnings)
        {
            ProcessStyleValue(styleAttribute.Parent, styleAttribute.Value, avElement, warnings);
        }
        public static void ProcessStyleValue(XElement svgElement, string styleText, BaseElement avElement, List<string> warnings)
        {
            if (string.IsNullOrWhiteSpace(styleText))
            {
                if (svgElement.Parent?.Attribute("style") is XAttribute parentStyleAttribute && !string.IsNullOrWhiteSpace(parentStyleAttribute.Value))
                    styleText = parentStyleAttribute.Value;
                else
                {
                    warnings.Add("style text is null, empty or whitespace");
                    return;
                }
            }

            var styleParts = styleText.Split(';');
            foreach (var part in styleParts)
            {
                if (string.IsNullOrWhiteSpace(part))
                    continue;
                var tokens = part.Split(':');
                if (tokens.Length == 2)
                {
                    var cmd = tokens[0].Trim();
                    var value = tokens[1].Trim();
                    if (AttributeMap.TryGetValue(cmd, out string avAtrName))
                    {
                        if (cmd == "fill" || cmd == "stroke")
                        {
                            if (value == "none")
                            {
                                avElement.SetAndroidAttributeValue(avAtrName, null);
                                avElement.SetAndroidAttributeValue(cmd + "Alpha", null);
                            }
                            else
                            {
                                var (hexColor, opacity) = GetHexColorAndFloatOpacity(value, warnings);
                                avElement.SetAndroidAttributeValue(avAtrName, hexColor);
                                if (!float.IsNaN(opacity))
                                    avElement.SetAndroidAttributeValue(cmd + "Alpha", opacity);
                            }
                        }
                        else if (cmd == "stroke-width")
                        {
                            AttributeExtensions.TryGetValueInPx(svgElement, value, Orientation.Unknown, out float strokeWidth);
                            avElement.SetAndroidAttributeValue(avAtrName, strokeWidth);
                        }
                        else if (cmd == "fill-rule")
                        {
                            if (value == "evenodd")
                                avElement.SetAndroidAttributeValue(avAtrName, "evenOdd");
                            else if (value == "nonzero")
                                avElement.SetAndroidAttributeValue(avAtrName, "nonZero");
                            else
                                warnings.Add("unknown SVG style fill-rule [" + value + "] in <" + svgElement?.Name + " id='" + svgElement?.Attribute("id")?.Value + "'> ");
                        }
                        else
                            avElement.SetAndroidAttributeValue(avAtrName, value);
                    }
                    else if (cmd == "display")
                    {
                        avElement.DisplayStyle = value;
                    }
                    else if (!IgnoreAttributeMap.Contains(cmd))
                        warnings.Add("could not find matching Android attribute for style [" + part + "]");
                }
                else
                    warnings.Add("could not parse style [" + part + "]");
            }
        }

        public static (string hexColor, float opacity) GetHexColorAndFloatOpacity(string colorText, List<string>warnings)
        {
            colorText = colorText.Trim();
            if (colorText.StartsWith("rbg"))
            {
                colorText.Trim(')');
                var tokens = colorText.Split(new char[] { '(', ' ', ',' });
                if (tokens.Length < 4)
                {
                    warnings.Add("Could not parse color text [" + colorText + "]");
                    return ("#000", 1);
                }
                string hexColor = "#";
                if (tokens.Length > 3)
                {
                    var rToken = tokens[1].Trim();
                    if (ParseColorNumberToken(rToken) is string rHex)
                        hexColor += rHex;
                    else
                    {
                        warnings.Add("Could not parse red token [" + rToken + "] in color text [" + colorText + "]");
                        return ("#000", 1);
                    }

                    var gToken = tokens[2].Trim();
                    if (ParseColorNumberToken(gToken) is string gHex)
                        hexColor += gHex;
                    else
                    {
                        warnings.Add("Could not parse green token [" + gToken + "] in color text [" + colorText + "]");
                        return ("#000", 1);
                    }

                    var bToken = tokens[3].Trim();
                    if (ParseColorNumberToken(gToken) is string bHex)
                        hexColor += bHex;
                    else
                    {
                        warnings.Add("Could not parse blue token [" + bToken + "] in color text [" + colorText + "]");
                        return ("#000", 1);
                    }

                    if (tokens.Length > 4)
                    {
                        var aToken = tokens[4].Trim();
                        if (aToken.EndsWith('%'))
                        {
                            if (float.TryParse(aToken, out float alpha))
                                return (hexColor, alpha);
                        }
                        else if (float.TryParse(aToken, out float alpha))
                            return (hexColor, alpha / 255f);
                        warnings.Add("Could not parse alpha token [" + aToken + "] in color text [" + colorText + "]");
                    }
                    return (hexColor, float.NaN);
                }
            }

            if (colorText.StartsWith("#"))
            {
                if (colorText.Length ==  4 || colorText.Length == 7)
                    return (colorText, float.NaN);
                if (colorText.Length == 5)
                {
                    var aHex = colorText.Substring(1, 1);
                    var aInt = Convert.ToInt16(aHex);
                    var aFloat = aInt / 15;
                    return ("#" + colorText.Substring(2), aFloat);
                }
                if (colorText.Length == 9)
                {
                    var aHex = colorText.Substring(1, 2);
                    var aInt = Convert.ToInt16(aHex);
                    var aFloat = aInt / 255f;
                    return ("#" + colorText.Substring(3), aFloat);
                }
            }
            else if (colorText == "transparent")
                return ("#000", 0);
            else if (CssColors.TryGetValue(colorText, out string cssHexColor))
                return (cssHexColor, 1);

            warnings.Add("Could not parse color text [" + colorText + "]");
            return ("#000", 1);

        }

        static string ParseColorNumberToken(string token)
        {
            float value;
            if (token.EndsWith('%'))
            {
                if (!float.TryParse(token, out value))
                    return null;
                value *= 255;
            }
            else if (!float.TryParse(token, out value))
                return null;
            int intValue = (int)Math.Round(value);
            return intValue.ToString("X2");
        }

        public static Dictionary<string, string> CssColors = new Dictionary<string, string>
        {
            { "black",  "#000000" },
            { "silver", "#c0c0c0" },
            { "gray",   "#808080" },
            { "white",   "#ffffff" },
            { "maroon",   "#800000" },
            { "red",   "#ff0000" },
            { "purple",   "#800080" },
            { "fuchsia",   "#ff00ff" },
            { "green",   "#008000" },
            { "lime",   "#00ff00" },
            { "olive",   "#808000" },
            { "yellow",   "#ffff00" },
            { "navy",   "#000080" },
            { "blue",   "#0000ff" },
            { "teal",   "#008080" },
            { "aqua",   "#00ffff" },

            { "orange",   "#ffa500" },
            { "aliceblue",   "#f0f8ff" },
            { "antiquewhite",   "#faebd7" },
            { "aquamarine",   "#7fffd4" },
            { "azure",   "#f0ffff" },
            { "beige",   "#f5f5dc" },
            { "bisque",   "#ffe4c4" },
            { "blanchedalmond",   "#ffebcd" },
            { "blueviolet",   "#8a2be2" },
            { "brown",   "#a52a2a" },
            { "burlywood",   "#deb887" },
            { "cadetblue",   "#5f9ea0" },
            { "chartreuse",   "#7fff00" },
            { "chocolate",   "#d2691e" },
            { "coral",   "#ff7f50" },
            { "cornflowerblue",   "#6495ed" },
            { "cornsilk",   "#fff8dc" },
            { "crimson",   "#dc143c" },
            { "cyan",   "#00ffff" },
            { "darkblue",   "#00008b" },
            { "darkcyan",   "#008b8b" },
            { "darkgoldenrod",   "#b8860b" },
            { "darkgray",   "#a9a9a9" },
            { "darkgreen",   "#006400" },
            { "darkgrey",   "#a9a9a9" },
            { "darkkhaki",   "#bdb76b" },
            { "darkmagenta",   "#8b008b" },
            { "darkolivegreen",   "#556b2f" },
            { "darkorange",   "#ff8c00" },
            { "darkorchid",   "#9932cc" },
            { "darkred",   "#8b0000" },
            { "darksalmon",   "#e9967a" },
            { "darkseagreen",   "#8fbc8f" },
            { "darkslateblue",   "#483d8b" },
            { "darkslategray",   "#2f4f4f" },
            { "darkslategrey",   "#2f4f4f" },
            { "darkturquoise",   "#00ced1" },
            { "darkviolet",   "#9400d3" },
            { "deeppink",   "#ff1493" },
            { "deepskyblue",   "#00bfff" },
            { "dimgray",   "#696969" },
            { "dimgrey",   "#696969" },
            { "dodgerblue",   "#1e90ff" },
            { "firebrick",   "#b22222" },
            { "floralwhite",   "#fffaf0" },
            { "forestgreen",   "#228b22" },
            { "gainsboro",   "#dcdcdc" },
            { "ghostwhite",   "#f8f8ff" },
            { "gold",   "#ffd700" },
            { "goldenrod",   "#daa520" },
            { "greenyellow",   "#adff2f" },
            { "grey",   "#808080" },
            { "honeydew",   "#f0fff0" },
            { "hotpink",   "#ff69b4" },
            { "indianred",   "#cd5c5c" },
            { "indigo",   "#4b0082" },
            { "ivory",   "#fffff0" },
            { "khaki",   "#f0e68c" },
            { "lavender",   "#e6e6fa" },
            { "lavenderblush",   "#fff0f5" },
            { "lawngreen",   "#7cfc00" },
            { "lemonchiffon",   "#fffacd" },
            { "lightblue",   "#add8e6" },
            { "lightcoral",   "#f08080" },
            { "lightcyan",   "#e0ffff" },
            { "lightgoldenrodyellow",   "#fafad2" },
            { "lightgray",   "#d3d3d3" },
            { "lightgreen",   "#90ee90" },
            { "lightgrey",   "#d3d3d3" },
            { "lightpink",   "#ffb6c1" },
            { "lightsalmon",   "#ffa07a" },
            { "lightseagreen",   "#20b2aa" },
            { "lightskyblue",   "#87cefa" },
            { "lightslategray",   "#778899" },
            { "lightslategrey",   "#778899" },
            { "lightsteelblue",   "#b0c4de" },
            { "lightyellow",   "#ffffe0" },
            { "limegreen",   "#32cd32" },
            { "linen",   "#faf0e6" },
            { "magenta",   "#ff00ff" },
            { "mediumaquamarine",   "#66cdaa" },
            { "mediumblue",   "#0000cd" },
            { "mediumorchid",   "#ba55d3" },
            { "mediumpurple",   "#9370db" },
            { "mediumseagreen",   "#3cb371" },
            { "mediumslateblue",   "#7b68ee" },
            { "mediumspringgreen",   "#00fa9a" },
            { "mediumvioletred",   "#c71585" },
            { "midnightblue",   "#191970" },
            { "mintcream",   "#f5fffa" },
            { "mistyrose",   "#ffe4e1" },
            { "moccasin",   "#ffe4b5" },
            { "navajowhite",   "#ffdead" },
            { "oldlace",   "#fdf5e6" },
            { "olivedrab",   "#6b8e23" },
            { "orangered",   "#ff4500" },
            { "orchid",   "#da70d6" },
            { "palegoldenrod",   "#eee8aa" },
            { "palegreen",   "#98fb98" },
            { "paleturquoise",   "#afeeee" },
            { "palevioletred",   "#db7093" },
            { "papayawhip",   "#ffefd5" },
            { "peachpuff",   "#ffdab9" },
            { "peru",   "#cd853f" },
            { "pink",   "#ffc0cb" },
            { "plum",   "#dda0ddv" },
            { "powderblue",   "#b0e0e6" },
            { "rosybrown",   "#bc8f8f" },
            { "royalblue",   "#4169e1" },
            { "saddlebrown",   "#8b4513" },
            { "salmon",   "#fa8072" },
            { "sandybrown",   "#f4a460" },
            { "seagreen",   "#2e8b57" },
            { "seashell",   "#fff5ee" },
            { "sienna",   "#a0522d" },
            { "skyblue",   "#87ceeb" },
            { "slateblue",   "#6a5acd" },
            { "slategray",   "#708090" },
            { "slategrey",   "#708090" },
            { "snow",   "#fffafa" },
            { "springgreen",   "#00ff7f" },
            { "steelblue",   "#4682b4" },
            { "tan",   "#d2b48c" },
            { "thistle",   "#d8bfd8" },
            { "tomato",   "#ff6347" },
            { "turquoise",   "#40e0d0" },
            { "violet",   "#ee82ee" },
            { "wheat",   "#f5deb3" },
            { "whitesmoke",   "#f5f5f5" },
            { "yellowgreen",   "#9acd32" },
            { "rebeccapurple",   "#663399" },
        };
    }
}
