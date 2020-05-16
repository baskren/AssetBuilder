using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using AndroidVector;

namespace Svg2AndroidVector
{
    public static class PaintConverter
    {
        //TODO: insert gradient properties as post processing setup.
        public static Dictionary<string, string> PaintAttributeMap = new Dictionary<string, string>
        {
            { "fill", "fillColor" },
            { "stroke", "strokeColor" },
        };
        /*
        public static void ConvertPaint(XAttribute svgAttribute, BaseElement avElement, List<string> warnings)
        {
            if (svgAttribute.Parent is XElement svgElement)
            {
                var svgAttributeValue = svgAttribute.Value.Trim();
                if (svgAttribute.Name == "inherit")
                {
                    var parent = svgAttribute.Parent.Parent;
                    while (parent != null)
                    {
                        if (parent.Attribute(svgAttribute.Name) is XAttribute parentAttribute && parentAttribute.Value != "inherit")
                        {
                            svgAttributeValue = parentAttribute.Value;
                            break;
                        }
                    }
                }
                if (svgAttributeValue.StartsWith("url("))
                {
                    if (!svgAttributeValue.StartsWith("url(#"))
                        throw new Exception("Only anchor URLs are supported at this time.");

                    var iri = svgAttributeValue.Substring("url(#".Length).Trim(')').Trim();
                    if (svgElement.GetRoot() is XElement root)
                    {
                        if (root.Descendants(Namespace.Svg + "linearGradient").FirstOrDefault(e=>e.Attribute("id").Value == iri) is XElement svgLinearGradient)
                        {
                            if (ConvertLinearGradient(svgLinearGradient, warnings) is LinearGradient avGradient)
                            {
                                var aapt = new AaptAttr("fill", avGradient);
                                avElement.Add(aapt);
                                return;
                            }
                        }
                        if (root.Descendants(Namespace.Svg + "radialGradient").FirstOrDefault(e => e.Attribute("id").Value == iri) is XElement svgRadialGradient)
                        {
                            if (ConvertRadialGradient(svgRadialGradient, warnings) is RadialGradient avGradient)
                            {
                                var aapt = new AaptAttr("fill", avGradient);
                                avElement.Add(aapt);
                                return;
                            }
                        }
                        warnings.AddWarning("Ignoring gradient because no element found to complete link [" + svgAttributeValue + "].");
                        return;
                    }
                    throw new Exception("Could not find document root");
                }
                else if (PaintAttributeMap.TryGetValue(svgAttribute.Name.ToString(), out string avAttributeName))
                {
                    var (hexColor, opacity) = StyleConverter.GetHexColorAndFloatOpacity(svgAttribute.Value, warnings);
                    avElement.SetAndroidAttributeValue(avAttributeName, hexColor);
                    if (!float.IsNaN(opacity))
                        avElement.SetAndroidAttributeValue("alpha", opacity);
                    return;
                }
                throw new Exception("Somehow, a SVG attribute of [" + svgAttribute.Name + "] was passed to GradientConverter.ConvertPaint.");
            }
            throw new Exception("No parent for paint attribute");
        }
        */
        public static LinearGradient ConvertLinearGradient(XElement svgGradient, List<string> warnings)
        {
            const string typeName = "linearGradient";
            if (svgGradient.Name != Namespace.Svg + typeName)
                throw new ArgumentException("Argument is not the expected <" + typeName + "> SVG element");

            float x1 = 0, y1 = 0, x2 = 0, y2 = 0;
            if (svgGradient.Attribute("x1") is XAttribute x1Attribute)
            {
                var f = x1Attribute.Value.ToPathOffset();
                if (!float.IsNaN(f))
                    x1 = f;
                else
                    warnings.AddWarning("Could not parse x1 parameter of <" + typeName + " id='" + svgGradient.Attribute("id")?.Value + "'>");
            }
            if (svgGradient.Attribute("y1") is XAttribute y1Attribute)
            {
                var f = y1Attribute.Value.ToPathOffset();
                if (!float.IsNaN(f))
                    y1 = f;
                else
                    warnings.AddWarning("Could not parse y1 parameter of <" + typeName + " id='" + svgGradient.Attribute("id")?.Value + "'>");
            }
            if (svgGradient.Attribute("x2") is XAttribute x2Attribute)
            {
                var f = x2Attribute.Value.ToPathOffset();
                if (!float.IsNaN(f))
                    x2 = f;
                else
                    warnings.AddWarning("Could not parse x2 parameter of <" + typeName + " id='" + svgGradient.Attribute("id")?.Value + "'>");
            }
            if (svgGradient.Attribute("y2") is XAttribute y2Attribute)
            {
                var f = y2Attribute.Value.ToPathOffset();
                if (!float.IsNaN(f))
                    y2 = f;
                else
                    warnings.AddWarning("Could not parse y2 parameter of <" + typeName + " id='" + svgGradient.Attribute("id")?.Value + "'>");
            }

            if (x1 == x2 && y1 == y2)
            {
                warnings.AddWarning("Ignoring SVG element because start [" + x1 + "," + y1 + "] and end [" + x2 + "," + y2 + "] points are the same for <" + typeName + " id=\"" + svgGradient.Attribute("id")?.Value + "\".");
                return null;
            }

            var avGradient = new LinearGradient();
            avGradient.SetAndroidAttributeValue("startX", x1);
            avGradient.SetAndroidAttributeValue("startY", y1);
            avGradient.SetAndroidAttributeValue("endX", x2);
            avGradient.SetAndroidAttributeValue("endY", y2);
            avGradient.SetAndroidAttributeValue("type", "linear");
            SetCommonGradientAttributes(svgGradient, avGradient, warnings);

            

            return avGradient;
        }

        public static float ToPathOffset(this string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                if (text.Contains('%'))
                {
                    text = text.Substring(0, text.IndexOf('%'));
                    if (float.TryParse(text, out float result))
                        return result / 100f;
                }
                else if (float.TryParse(text, out float result))
                    return result;
            }
            return float.NaN;
        }


        public static RadialGradient ConvertRadialGradient(XElement svgGradient, List<string> warnings)
        {
            const string typeName = "radialGradient";
            if (svgGradient.Name != Namespace.Svg + typeName)
                throw new ArgumentException("Argument is not the expected <" + typeName + "> SVG element");

            float cx = 0, cy = 0, r = 0;
            if (svgGradient.Attribute("cx") is XAttribute cxAttribute)
            {
                var f = cxAttribute.Value.ToPathOffset();
                if (!float.IsNaN(f))
                    cx = f;
                else
                    warnings.AddWarning("Could not parse cx parameter of <" + typeName + " id='" + svgGradient.Attribute("id")?.Value + "'>");
            }
            if (svgGradient.Attribute("cy") is XAttribute cyAttribute)
            {
                var f = cyAttribute.Value.ToPathOffset();
                if (!float.IsNaN(f))
                    cy = f;
                else
                    warnings.AddWarning("Could not parse cy parameter of <" + typeName + " id='" + svgGradient.Attribute("id")?.Value + "'>");
            }
            if (svgGradient.Attribute("fx") is XAttribute)
                warnings.AddWarning("Ignoring 'fx' attribute in SVG " + typeName + " because it is not mappable to AndroidVector.");
            if (svgGradient.Attribute("fy") is XAttribute)
                warnings.AddWarning("Ignoring 'fy' attribute in SVG " + typeName + " because it is not mappable to AndroidVector.");
            if (svgGradient.Attribute("r") is XAttribute rAttribute)
            {
                var f = rAttribute.Value.ToPathOffset();
                if (!float.IsNaN(f))
                    r = f;
                else
                    warnings.AddWarning("Could not parse r parameter of <" + typeName + " id='" + svgGradient.Attribute("id")?.Value + "'>");
            }

            if (r <= 0)
            {
                //warnings.AddWarning("Ignoring SVG " + typeName + " element because radius <= 0 in element <" + typeName + " id=\"" + svgGradient.Attribute("id")?.Value + "\".");
                warnings.AddWarning("Could not find r (radius) arguement in element <" + typeName + " id=\"" + svgGradient.Attribute("id")?.Value + "\".   Assuming 0.5");
                r = 0.5f;
                //return null;
            }

            var avGradient = new RadialGradient();
            avGradient.SetAndroidAttributeValue("centerX", cx);
            avGradient.SetAndroidAttributeValue("centerY", cy);
            avGradient.SetAndroidAttributeValue("gradientRadius", r);
            avGradient.SetAndroidAttributeValue("type", "linear");
            SetCommonGradientAttributes(svgGradient, avGradient, warnings);
            return avGradient;
        }

        static void SetCommonGradientAttributes(XElement svgGradient, BaseGradient avGradient, List<string> warnings)
        {
            var typeName = svgGradient.Name;

            if (svgGradient.Attribute("gradientUnits") is XAttribute unitsAttribute)
                avGradient.UserSpaceUnits = unitsAttribute.Value == "userSpaceOnUse";

            if (svgGradient.Attribute("gradientTransform") is XAttribute transformAttribute)
                CommonAttributes.SetTransforms(svgGradient, avGradient, warnings);

            if (svgGradient.Attribute("spreadMethod") is XAttribute spreadMethodAttribute)
            {
                switch (spreadMethodAttribute.Value)
                {
                    case "pad":
                        avGradient.SetAndroidAttributeValue("tileMode", "clamp");
                        break;
                    case "reflect":
                        avGradient.SetAndroidAttributeValue("tileMode", "mirrow");
                        break;
                    case "repeat":
                        avGradient.SetAndroidAttributeValue("tileMode", "repeat");
                        break;
                    default:
                        warnings.AddWarning("Ignoring 'spreadMethod' attribute in SVG " + typeName + " because value '" + spreadMethodAttribute.Value + "' is not supported.");
                        break;
                }
            }

            foreach (var stop in svgGradient.Elements(Namespace.Svg + "stop"))
            {
                if (stop.Attribute("stop-color") is XAttribute colorAttribute)
                {
                    var color = colorAttribute.Value.FromHexString();
                    if (stop.Attribute("stop-opacity") is XAttribute opacityAttribute)
                    {
                        var text = opacityAttribute.Value.Trim();
                        bool percent = false;
                        if (text.EndsWith('%'))
                        {
                            percent = true;
                            text.Trim('%');
                        }
                        if (float.TryParse(text, out float value))
                        {
                            if (percent)
                                value /= 100f;
                            int alpha = (int)Math.Round(255 * value);
                            color = Color.FromArgb(alpha, color.R, color.G, color.B);
                        }
                        else
                        {
                            warnings.AddWarning("Ignoring opacity because could not parse stop-opaticy in " + stop + ".");
                        }
                    }
                    if (stop.Attribute("offset") is XAttribute offsetAttribute)
                    {
                        //avGradient.Add(new XElement(Namespace.Svg + "item", new AndroidAttribute("color", colorAttribute.Value), new AndroidAttribute("offset", offsetAttribute.Value)));
                        avGradient.Add(new GradientItem(offsetAttribute.Value, color.ToHexString()));
                    }
                    else
                        warnings.AddWarning("Ignoring " + typeName + " stop because no offset attribute for SVG <stop id='" + stop.Attribute("id")?.Value + "'>.");
                }
                warnings.AddWarning("Ignoring " + typeName + " stop because no stop-color attribute for SVG <stop id='" + stop.Attribute("id")?.Value + "'>.");
            }

        }
    }
}
