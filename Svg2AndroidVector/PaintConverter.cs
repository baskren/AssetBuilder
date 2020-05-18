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

        static XElement GetGradientHrefElement(XElement svgGradient, List<string> warnings)
        {
            XElement hrefElement = null;
            if (svgGradient.Attribute(Namespace.xlinkNs + "href") is XAttribute hrefAttribute)
            {
                if (hrefAttribute.Value.StartsWith("#"))
                {
                    var iri = hrefAttribute.Value.Substring(1).Trim(')').Trim();
                    if (svgGradient.GetRoot() is XElement root)
                    {
                        if (root.Descendants().FirstOrDefault(e => e.Attribute("id")?.Value == iri) is XElement hrefGradient)
                            hrefElement = hrefGradient;
                        else
                            warnings.AddWarning("Ignoring gradient because no element of same type found to complete link [" + hrefAttribute + "].");
                    }
                    else
                        throw new Exception("Could not find document root");
                }
                else
                    warnings.AddWarning("Only anchor URLs are supported at this time.");
            }
            return hrefElement;
        }

        public static LinearGradient ConvertLinearGradient(XElement svgGradient, List<string> warnings)
        {
            const string typeName = "linearGradient";
            if (svgGradient.Name != Namespace.Svg + typeName)
                throw new ArgumentException("Argument is not the expected <" + typeName + "> SVG element");

            var hrefElement = GetGradientHrefElement(svgGradient, warnings);

            float x1 = 0, y1 = 0, x2 = 1, y2 = 0;
            if (GetAttribute("x1", svgGradient, hrefElement) is XAttribute x1Attribute)
            {
                var f = x1Attribute.Value.ToPathOffset();
                if (!float.IsNaN(f))
                    x1 = f;
                else
                    warnings.AddWarning("Could not parse x1 parameter of <" + typeName + " id='" + svgGradient.Attribute("id")?.Value + "'>");
            }
            if (GetAttribute("y1", svgGradient, hrefElement) is XAttribute y1Attribute)
            {
                var f = y1Attribute.Value.ToPathOffset();
                if (!float.IsNaN(f))
                    y1 = f;
                else
                    warnings.AddWarning("Could not parse y1 parameter of <" + typeName + " id='" + svgGradient.Attribute("id")?.Value + "'>");
            }
            if (GetAttribute("x2", svgGradient, hrefElement) is XAttribute x2Attribute)
            {
                var f = x2Attribute.Value.ToPathOffset();
                if (!float.IsNaN(f))
                    x2 = f;
                else
                    warnings.AddWarning("Could not parse x2 parameter of <" + typeName + " id='" + svgGradient.Attribute("id")?.Value + "'>");
            }
            if (GetAttribute("y2", svgGradient, hrefElement) is XAttribute y2Attribute)
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
            SetCommonGradientAttributes(svgGradient, hrefElement, avGradient, warnings);

            

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

            var hrefElement = GetGradientHrefElement(svgGradient, warnings);

            float cx = 0, cy = 0, r = 0;
            if (GetAttribute("cx", svgGradient, hrefElement) is XAttribute cxAttribute)
            {
                var f = cxAttribute.Value.ToPathOffset();
                if (!float.IsNaN(f))
                    cx = f;
                else
                    warnings.AddWarning("Could not parse cx parameter of <" + typeName + " id='" + svgGradient.Attribute("id")?.Value + "'>");
            }

            if (GetAttribute("cy", svgGradient, hrefElement) is XAttribute cyAttribute)
            {
                var f = cyAttribute.Value.ToPathOffset();
                if (!float.IsNaN(f))
                    cy = f;
                else
                    warnings.AddWarning("Could not parse cy parameter of <" + typeName + " id='" + svgGradient.Attribute("id")?.Value + "'>");
            }
            if (svgGradient.Attribute("fx") is XAttribute fxAttribute)
            {
                var f = fxAttribute.Value.ToPathOffset();
                if (!float.IsNaN(f))
                {
                    cx = f;
                    warnings.AddWarning("Found 'fx' attribute in SVG " + typeName + ".  Emulating AndroidStudio VectorAsset Generator by replacing 'rx' with 'fx'.");
                }
            }
            if (svgGradient.Attribute("fy") is XAttribute fyAttribute)
            {
                var f = fyAttribute.Value.ToPathOffset();
                if (!float.IsNaN(f))
                {
                    cy = f;
                    warnings.AddWarning("Found 'fy' attribute in SVG " + typeName + ".  Emulating AndroidStudio VectorAsset Generator by replacing 'ry' with 'fy'.");
                }
            }
            if (GetAttribute("r", svgGradient, hrefElement) is XAttribute rAttribute)
            {
                var f = rAttribute.Value.ToPathOffset();
                if (!float.IsNaN(f))
                    r = f;
                else
                    warnings.AddWarning("Could not parse r parameter of <" + typeName + " id='" + svgGradient.Attribute("id")?.Value + "'>");
            }

            if (r <= 0)
                r = 0.5f;

            var avGradient = new RadialGradient();
            avGradient.SetAndroidAttributeValue("centerX", cx);
            avGradient.SetAndroidAttributeValue("centerY", cy);
            avGradient.SetAndroidAttributeValue("gradientRadius", r);
            SetCommonGradientAttributes(svgGradient, hrefElement, avGradient, warnings);
            return avGradient;
        }

        static XAttribute GetAttribute(string name, XElement source, XElement href)
        {
            XAttribute attribute = null;
            if (source.Attribute(name) is XAttribute sourceAttribute)
                attribute = sourceAttribute;
            else if (href!=null && href.Attribute(name) is XAttribute hrefAttribute)
                attribute = hrefAttribute;
            return attribute;
        }

        static void SetCommonGradientAttributes(XElement svgGradient, XElement hrefElement, BaseGradient avGradient, List<string> warnings)
        {
            var typeName = svgGradient.Name;

            if (GetAttribute("gradientUnits", svgGradient, hrefElement) is XAttribute unitsAttribute)
                avGradient.UserSpaceUnits = unitsAttribute.Value == "userSpaceOnUse";

            if (GetAttribute("gradientTransform", svgGradient, hrefElement) is XAttribute transformAttribute)
                CommonAttributes.SetTransforms(svgGradient, avGradient, warnings);

            if (GetAttribute("spreadMethod", svgGradient, hrefElement) is XAttribute spreadMethodAttribute)
            {
                switch (spreadMethodAttribute.Value)
                {
                    case "pad":
                        avGradient.SetAndroidAttributeValue("tileMode", "clamp");
                        break;
                    case "reflect":
                        avGradient.SetAndroidAttributeValue("tileMode", "mirror");
                        break;
                    case "repeat":
                        avGradient.SetAndroidAttributeValue("tileMode", "repeat");
                        break;
                    default:
                        warnings.AddWarning("Ignoring 'spreadMethod' attribute in SVG " + typeName + " because value '" + spreadMethodAttribute.Value + "' is not supported.");
                        break;
                }
            }

            var stops = hrefElement?.Elements(Namespace.Svg + "stop").ToList() ?? new List<XElement>();
            stops.AddRange(svgGradient.Elements(Namespace.Svg + "stop"));
           
            foreach (var stop in stops)
            {
                var color = Color.Black;
                var styles = new Dictionary<string, string>();

                if (stop.Attribute("style") is XAttribute styleAttribute)
                {
                    var parts = styleAttribute.Value.Split(';');
                    foreach (var style in parts)
                    {
                        var styleParts = style.Split(':');
                        if (styleParts.Length > 1)
                            styles.Add(styleParts[0], styleParts[1]);
                    }
                }

                if (stop.Attribute("stop-color") is XAttribute colorAttribute)
                    color = colorAttribute.Value.ToColor();
                else if (styles.TryGetValue("stop-color", out string colorText))
                    color = colorText.ToColor();

                string opacityText = null;
                if (stop.Attribute("stop-opacity") is XAttribute opacityAttribute)
                    opacityText = opacityAttribute.Value;
                else if (styles.ContainsKey("stop-opacity"))
                    //styles.TryGetValue("stop-opacity", out opacityText);
                    opacityText = styles["stop-opacity"];

                if (!string.IsNullOrWhiteSpace(opacityText))
                {
                    bool percent = false;
                    if (opacityText.EndsWith('%'))
                    {
                        percent = true;
                        opacityText.Trim('%');
                    }
                    if (float.TryParse(opacityText, out float value))
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


                string offsetText = "0";
                if (stop.Attribute("offset") is XAttribute offsetAttribute)
                    offsetText = offsetAttribute.Value;
                else if (styles.ContainsKey("offset"))
                    offsetText = styles["offset"];
                avGradient.Add(new GradientItem(offsetText, color.ToHexString()));
            }


        }
    }
}
