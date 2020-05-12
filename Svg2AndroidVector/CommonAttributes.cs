using System;
using System.Collections.Generic;
using System.Xml.Linq;
using AndroidVector;

namespace Svg2AndroidVector
{
    public class CommonAttributes
    {
        public static Dictionary<string, string> AttributeMap = new Dictionary<string, string>
        {
            { "id", "name" },
            { "stroke-width", "strokeWidth" },
            { "stroke-opacity", "strokeAlpha" },
            { "fillOpacity", "fillAlpha" },
            //{ "", "trimPathStart"},
            //{ "", "trimPathEnd"},
            //{ "", "trimPathOffset"},
            { "stroke-linecap", "strokeLineCap"},
            { "stroke-linejoin", "strokeLineJoin"},
            { "stroke-miterlimit", "strokeMiterLimit"},
            { "fill-rule", "fillType"},
        };

        public static void ProcessAttributes(XElement svgElement, AndroidVector.BaseElement avElement, Dictionary<string, string> attributeMap, List<string> ignoreAttributes, List<string> warnings)
        {
            if (StyleConverter.CssStyles.TryGetValue(svgElement.Name.LocalName, out string typeStyleText))
                StyleConverter.ProcessStyleValue(svgElement, typeStyleText, avElement, warnings);

            if (svgElement.Attribute("class") is XAttribute classAttribute) 
            {
                if (StyleConverter.CssStyles.TryGetValue("." + classAttribute.Value, out string classStyleText))
                    StyleConverter.ProcessStyleValue(svgElement, classStyleText, avElement, warnings);
                if (StyleConverter.CssStyles.TryGetValue(svgElement.Name.LocalName +"." + classAttribute.Value, out string typeClassStyleText))
                    StyleConverter.ProcessStyleValue(svgElement, typeClassStyleText, avElement, warnings);
            }

            if (svgElement.Attribute("id") is XAttribute idAttribute &&
                !string.IsNullOrWhiteSpace(idAttribute.Value) &&
                StyleConverter.CssStyles.TryGetValue("#"+idAttribute.Value, out string styleText))
                StyleConverter.ProcessStyleValue(svgElement, styleText, avElement, warnings);

            foreach (var attribute in svgElement.Attributes())
            {
                if (ignoreAttributes?.Contains(attribute.Name.ToString()) ?? false)
                    continue;
                if (attribute.Name.ToString().StartsWith('{') && attribute.Name.ToString().Contains('}'))
                    continue;
                if (attribute.Name.ToString().StartsWith("aria-"))
                    continue;
                if (attribute.Name == "class")
                    continue;
                if (attribute.Name == "clip-path")
                    continue;
                if (attribute.Name == "fill" || attribute.Name == "stroke")
                    PaintConverter.ConvertPaint(attribute, avElement, warnings);
                else if (attribute.Name == "style")
                    StyleConverter.ProcessStyleAttribute(attribute, avElement, warnings);
                else if (attribute.Name == "transform")
                    SetTransforms(svgElement, avElement, warnings);
                else if (attributeMap.TryGetValue(attribute.Name.ToString(), out string avAttributeName))
                {
                    var value = attribute.Value;
                    if (attribute.Name == "fill-rule")
                    {
                        switch (value)
                        {
                            case "evenodd":
                                value = "evenOdd";
                                break;
                            case "nonzero":
                                value = "nonZero";
                                break;
                        }
                    }
                    if (attribute.Value != "inherit")
                        avElement.SetAndroidAttributeValue(avAttributeName, value);
                    else if (avElement.InheritedAttributeValue(attribute) is string inheritedValue)
                        avElement.SetAndroidAttributeValue(avAttributeName, inheritedValue);
                    else
                        warnings.Add("unable to find inherited attribute [" + attribute.ToString() + "] in SVG <" + svgElement.Name + " id=\"" + svgElement.Attribute("id").Value + "\">.");
                }
                else
                    warnings.Add("unsupported attribute [" + attribute.ToString() + "] in SVG <" + svgElement.Name + " id=\"" + svgElement.Attribute("id")?.Value + "\">.");
            }
        }

        // must be handled before converting an element
        public static void SetTransforms(XElement svgElement, BaseElement avElement, List<string> warnings)
        {
            if (svgElement.Attribute("transform") is XAttribute transformAttribute)
            {
                var transforms = transformAttribute.Value.Split(')');
                foreach (var transform in transforms)
                {
                    var tokens = transform.Split(new char[] { '(', ' ', ',' });
                    var type = tokens[0].Trim();
                    switch (type)
                    {
                        case "translate":
                            {
                                if (tokens.Length > 1 && tokens.Length < 4)
                                {
                                    if (AttributeExtensions.TryGetValueInPx(svgElement, tokens[1], Orientation.Horizontal, out float x))
                                    {
                                        float y = 0;
                                        if (tokens.Length > 2)
                                        {
                                            if (!AttributeExtensions.TryGetValueInPx(svgElement, tokens[2], Orientation.Vertical, out y))
                                                warnings.Add("Ignoring Y of "+type+" transform: Could not parse Y token for "+type+ " transform [" + transform + "]  in <" + svgElement.Name + " id=\"" + svgElement.Attribute("id")?.Value + "\">");
                                        }
                                        avElement.SvgTransforms.Add(Matrix.CreateTranslate(x, y));
                                    }
                                    else
                                        warnings.Add("Ignoring " + type + " transform: Could not parse X token for " + type + " transform [" + transform + "]  in <" + svgElement.Name + " id=\"" + svgElement.Attribute("id")?.Value + "\">");
                                }
                                else
                                    warnings.Add("Ignoring " + type + " transform: invalid number of tokens in " + type + " transform [" + transform + "] in <" + svgElement.Name + " id=\"" + svgElement.Attribute("id")?.Value + "\">");
                            }
                            break;
                        case "scale":
                            {
                                if (tokens.Length > 1 && tokens.Length < 4)
                                {
                                    if (float.TryParse(tokens[1], out float x))
                                    {
                                        float y = 0;
                                        if (tokens.Length > 2)
                                        {
                                            if (!float.TryParse(tokens[2], out y))
                                                warnings.Add("Ignoring Y of " + type + " transform: Could not parse Y token for " + type + " transform [" + transform + "]  in <" + svgElement.Name + " id=\"" + svgElement.Attribute("id")?.Value + "\">");
                                        }
                                        avElement.SvgTransforms.Add(Matrix.CreateScale(x, y));
                                    }
                                    else
                                        warnings.Add("Ignoring " + type + " transform: Could not parse X token for " + type + " transform [" + transform + "]  in <" + svgElement.Name + " id=\"" + svgElement.Attribute("id")?.Value + "\">");
                                }
                                else
                                    warnings.Add("Ignoring " + type + " transform: invalid number of tokens in " + type + " transform [" + transform + "] in <" + svgElement.Name + " id=\"" + svgElement.Attribute("id")?.Value + "\">");
                            }
                            break;
                        case "rotate":
                            {
                                if (tokens.Length == 2)
                                {
                                    if (float.TryParse(tokens[1], out float degrees))
                                    {
                                        avElement.SvgTransforms.Add(Matrix.CreateRotateDegrees(degrees));
                                    }
                                    else
                                        warnings.Add("Ignoring " + type + " transform: Could not parse angle token for " + type + " transform [" + transform + "]  in <" + svgElement.Name + " id=\"" + svgElement.Attribute("id")?.Value + "\">");
                                }
                                else
                                    warnings.Add("Ignoring " + type + " transform: invalid number of tokens in " + type + " transform [" + transform + "] in <" + svgElement.Name + " id=\"" + svgElement.Attribute("id")?.Value + "\">");
                            }
                            break;
                        case "skewX":
                            {
                                if (tokens.Length == 2)
                                {
                                    if (float.TryParse(tokens[1], out float degrees))
                                    {
                                        avElement.SvgTransforms.Add(Matrix.CreateSkewDegrees(degrees,0));
                                    }
                                    else
                                        warnings.Add("Ignoring " + type + " transform: Could not parse angle token for " + type + " transform [" + transform + "]  in <" + svgElement.Name + " id=\"" + svgElement.Attribute("id")?.Value + "\">");
                                }
                                else
                                    warnings.Add("Ignoring " + type + " transform: invalid number of tokens in " + type + " transform [" + transform + "] in <" + svgElement.Name + " id=\"" + svgElement.Attribute("id")?.Value + "\">");
                            }
                            break;
                        case "skewY":
                            {
                                if (tokens.Length == 2)
                                {
                                    if (float.TryParse(tokens[1], out float degrees))
                                    {
                                        avElement.SvgTransforms.Add(Matrix.CreateSkewDegrees(0, degrees));
                                    }
                                    else
                                        warnings.Add("Ignoring " + type + " transform: Could not parse angle token for " + type + " transform [" + transform + "]  in <" + svgElement.Name + " id=\"" + svgElement.Attribute("id")?.Value + "\">");
                                }
                                else
                                    warnings.Add("Ignoring " + type + " transform: invalid number of tokens in " + type + " transform [" + transform + "] in <" + svgElement.Name + " id=\"" + svgElement.Attribute("id")?.Value + "\">");
                            }
                            break;
                        case "matrix":
                            {
                                if (tokens.Length == 7)
                                {
                                    if (!float.TryParse(tokens[1], out float m1))
                                    {
                                        warnings.Add("Ignoring " + type + " transform: Could not parse m1 token for " + type + " transform [" + transform + "]  in <" + svgElement.Name + " id=\"" + svgElement.Attribute("id")?.Value + "\">");
                                        break;
                                    }
                                    if (!float.TryParse(tokens[2], out float m2))
                                    {
                                        warnings.Add("Ignoring " + type + " transform: Could not parse m2 token for " + type + " transform [" + transform + "]  in <" + svgElement.Name + " id=\"" + svgElement.Attribute("id")?.Value + "\">");
                                        break;
                                    }
                                    if (!float.TryParse(tokens[3], out float m3))
                                    {
                                        warnings.Add("Ignoring " + type + " transform: Could not parse m3 token for " + type + " transform [" + transform + "]  in <" + svgElement.Name + " id=\"" + svgElement.Attribute("id")?.Value + "\">");
                                        break;
                                    }
                                    if (!float.TryParse(tokens[4], out float m4))
                                    {
                                        warnings.Add("Ignoring " + type + " transform: Could not parse m4 token for " + type + " transform [" + transform + "]  in <" + svgElement.Name + " id=\"" + svgElement.Attribute("id")?.Value + "\">");
                                        break;
                                    }
                                    if (!float.TryParse(tokens[5], out float m5))
                                    {
                                        warnings.Add("Ignoring " + type + " transform: Could not parse m5 token for " + type + " transform [" + transform + "]  in <" + svgElement.Name + " id=\"" + svgElement.Attribute("id")?.Value + "\">");
                                        break;
                                    }
                                    if (!float.TryParse(tokens[6], out float m6))
                                    {
                                        warnings.Add("Ignoring " + type + " transform: Could not parse m6 token for " + type + " transform [" + transform + "]  in <" + svgElement.Name + " id=\"" + svgElement.Attribute("id")?.Value + "\">");
                                        break;
                                    }
                                    avElement.SvgTransforms.Add(new Matrix(m1, m2, m3, m4, m5, m6));
                                }
                                else
                                    warnings.Add("Ignoring " + type + " transform: invalid number of tokens in " + type + " transform [" + transform + "] in <" + svgElement.Name + " id=\"" + svgElement.Attribute("id")?.Value + "\">");
                            }
                            break;
                        case "default":
                            warnings.Add("transform [" + transform + "] not supported : <" + svgElement.Name + " id=\"" + svgElement.Attribute("id")?.Value + "\">");
                            break;
                    }
                }
            }
        }



    }

}
