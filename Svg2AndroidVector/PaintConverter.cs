using System;
using System.Collections.Generic;
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
                        if (root.Descendants(NameSpace.Svg + "linearGradient").FirstOrDefault(e=>e.Attribute("id").Value == iri) is XElement svgLinearGradient)
                        {
                            if (ConvertLinearGradient(svgLinearGradient, warnings) is LinearGradient avGradient)
                            {
                                var aapt = new AaptAttr("fill", avGradient);
                                svgElement.Add(aapt);
                                return;
                            }
                        }
                        if (root.Descendants(NameSpace.Svg + "radialGradient").FirstOrDefault(e => e.Attribute("id").Value == iri) is XElement svgRadialGradient)
                        {
                            if (ConvertRadialGradient(svgRadialGradient, warnings) is RadialGradient avGradient)
                            {
                                var aapt = new AaptAttr("fill", avGradient);
                                svgElement.Add(aapt);
                                return;
                            }
                        }
                        warnings.Add("No element found to complete Paint link to " + svgAttributeValue + ".");
                        return;
                    }
                    throw new Exception("could not find document root");
                }
                if (PaintAttributeMap.TryGetValue(svgAttribute.Name.ToString(), out string avAttributeName))
                {
                    var (hexColor, opacity) = StyleConverter.GetHexColorAndFloatOpacity(svgAttribute.Value, warnings);
                    avElement.SetAndroidAttributeValue(avAttributeName, hexColor);
                    if (!float.IsNaN(opacity))
                        avElement.SetAndroidAttributeValue("alpha", opacity);
                    return;
                }
                throw new Exception("somehow, a SVG attribute of [" + svgAttribute.Name + "] was passed to GradientConverter.ConvertPaint.");
            }
            throw new Exception("No parent for paint attribute");
        }

        public static LinearGradient ConvertLinearGradient(XElement svgGradient, List<string> warnings)
        {
            const string typeName = "linearGradient";
            if (svgGradient.Name != NameSpace.Svg + typeName)
                throw new ArgumentException("argument is not the expected <" + typeName + "> SVG element");

            float x1 = 0, y1 = 0, x2 = 0, y2 = 0;
            if (svgGradient.Attribute("x1") is XAttribute x1Attribute)
                AttributeExtensions.TryGetValueInPx(x1Attribute, out x1);
            if (svgGradient.Attribute("y1") is XAttribute y1Attribute)
                AttributeExtensions.TryGetValueInPx(y1Attribute, out y1);
            if (svgGradient.Attribute("x2") is XAttribute x2Attribute)
                AttributeExtensions.TryGetValueInPx(x2Attribute, out x2);
            if (svgGradient.Attribute("y2") is XAttribute y2Attribute)
                AttributeExtensions.TryGetValueInPx(y2Attribute, out y2);

            if (x1 == x2 && y1 == y2)
            {
                warnings.Add("ignoring SVG element because start [" + x1 + "," + y1 + "] and end [" + x2 + "," + y2 + "] points are the same for <" + typeName + " id=\"" + svgGradient.Attribute("id")?.Value + "\".");
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

        public static RadialGradient ConvertRadialGradient(XElement svgGradient, List<string> warnings)
        {
            const string typeName = "radialGradient";
            if (svgGradient.Name != NameSpace.Svg + typeName)
                throw new ArgumentException("argument is not the expected <" + typeName + "> SVG element");

            float cx = 0, cy = 0, r = 0;
            if (svgGradient.Attribute("cx") is XAttribute x1Attribute)
                //float.TryParse(x1Attribute.Value, out cx);
                AttributeExtensions.TryGetValueInPx(x1Attribute, out cx);
            if (svgGradient.Attribute("cy") is XAttribute y1Attribute)
                //float.TryParse(y1Attribute.Value, out cy);
                AttributeExtensions.TryGetValueInPx(y1Attribute, out cy);
            if (svgGradient.Attribute("fx") is XAttribute)
                warnings.Add("ignoring 'fx' attribute in SVG " + typeName + " because it is not supported by AndroidVector.");
            if (svgGradient.Attribute("fy") is XAttribute)
                warnings.Add("ignoring 'fy' attribute in SVG " + typeName + " because it is not supported by AndroidVector.");
            if (svgGradient.Attribute("r") is XAttribute rAttribute)
                AttributeExtensions.TryGetValueInPx(rAttribute, out r);

            if (r <= 0)
            {
                warnings.Add("ignoring SVG " + typeName + " element because radius <= 0 <" + typeName + " id=\"" + svgGradient.Attribute("id")?.Value + "\".");
                return null;
            }

            var avGradient = new RadialGradient();
            avGradient.SetAndroidAttributeValue("centerX", cx);
            avGradient.SetAndroidAttributeValue("centerY", cy);
            avGradient.SetAndroidAttributeValue("gradientRadius", r);
            avGradient.SetAndroidAttributeValue("type", "linear");
            SetCommonGradientAttributes(svgGradient, avGradient, warnings);
            return avGradient;
        }

        static bool SetCommonGradientAttributes(XElement svgGradient, BaseGradient avGradient, List<string> warnings)
        {
            var typeName = svgGradient.Name;

            if (svgGradient.Attribute("gradientUnits") is XAttribute)
                warnings.Add("ignoring 'gradientUnits' attribute in SVG " + typeName + " because it is not understood well enough by me to know if it can be property supported.");

            if (svgGradient.Attribute("gradientTransform") is XAttribute)
                warnings.Add("ignoring 'gradientTransform' attribute in SVG " + typeName + " because it is not understood well enough by me to know if it can be property supported.");

            if (svgGradient.Attribute("id") is XAttribute idAttribute && !string.IsNullOrWhiteSpace(idAttribute.Value))
            {
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
                            warnings.Add("ignoring 'spreadMethod' attribute in SVG " + typeName + " because value '" + spreadMethodAttribute.Value + "' is not supported.");
                            break;
                    }
                }

                foreach (var stop in svgGradient.Elements("stop"))
                {
                    if (stop.Attribute("stop-color") is XAttribute colorAttribute)
                    {
                        if (stop.Attribute("offset") is XAttribute offsetAttribute)
                            avGradient.Add(new XElement("item", new AndroidAttribute("color", colorAttribute.Value), new AndroidAttribute("offset", offsetAttribute.Value)));
                        else
                            warnings.Add("ignoring " + typeName + " stop because no offset attribute for SVG <stop id='" + stop.Attribute("id")?.Value + "'>.");
                    }
                    warnings.Add("ignoring " + typeName + " stop because no stop-color attribute for SVG <stop id='" + stop.Attribute("id")?.Value + "'>.");
                }
                return true;
            }
            warnings.Add("ignoring SVG '" + typeName + "' because invalid id for <" + typeName + " id=\"" + svgGradient.Attribute("id")?.Value + "\".");
            return false;
        }
    }
}
