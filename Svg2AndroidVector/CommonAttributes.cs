using System.Collections.Generic;
using System.Xml.Linq;
using AndroidVector;

namespace Svg2AndroidVector
{
    public class CommonAttributes
    {
        static readonly List<string> IgnoreAttributes = new List<string>()
        {
            "class",
            "clip-path",
            "transform",
            "width",
            "height"
        };

        static CommonAttributes()
        {
            IgnoreAttributes.AddRange(StyleConverter.IgnoreAttributeMap);
            IgnoreAttributes.AddRange(StyleConverter.AttributeMap.Keys);
        }

        public static void ProcessAttributes(XElement svgElement, BaseElement avElement, List<string> ignoreAttributes, List<string> warnings)
        {
            StyleConverter.ProcessStyleAttributes(svgElement, avElement, warnings);

            ignoreAttributes ??= new List<string>();
            ignoreAttributes.AddRange(IgnoreAttributes);

            foreach (var attribute in svgElement.Attributes())
            {
                if (!ignoreAttributes.Contains(attribute.Name.ToString()) &&
                    attribute.Name.Namespace == XNamespace.None &&
                    !attribute.Name.ToString().StartsWith("aria-")
                    )
                {
                    if (attribute.Name == "id")
                        avElement.Name = attribute.Value;
                    else if (attribute.Name == "d")
                        avElement.SetAndroidAttributeValue("pathData", attribute.Value);
                    else
                        warnings.AddWarning("Ignoring attribute [" + attribute.ToString() + "] in SVG <" + svgElement.Name + " id=\"" + svgElement.Attribute("id")?.Value + "\"> because it is unrecognized.");
                }
            }
        }

        // must be handled before converting an element
        public static void SetTransforms(XElement svgElement, BaseElement avElement, List<string> warnings)
        {
            string transformsText = null;
            if (svgElement.Attribute("transform") is XAttribute transformAttribute)
                transformsText = transformAttribute.Value;
            else if (svgElement.Attribute("gradientTransform") is XAttribute gradientTransformAttribute)
                transformsText = gradientTransformAttribute.Value;
            if (!string.IsNullOrWhiteSpace(transformsText))
            {
                var transforms = transformsText.Split(')');
                foreach (var transform in transforms)
                {
                    if (!string.IsNullOrWhiteSpace(transform))
                    {
                        var parts = transform.Trim().Split('(');
                        //var tokens = transform.Trim().Split(new char[] { '(', ' ', ',' });
                        var type = parts[0].Trim();
                        var values = parts[1].ToFloatList();
                        switch (type)
                        {
                            case "translate":
                                {
                                    if (values.Count == 1)
                                        avElement.SvgTransforms.Add(Matrix.CreateTranslate(values[0], 0));
                                    else if (values.Count == 2)
                                        avElement.SvgTransforms.Add(Matrix.CreateTranslate(values[0], values[1]));
                                    else
                                        warnings.AddWarning("Ignoring " + type + " transform because invalid number of tokens in " + type + " transform [" + transform + "] in <" + svgElement.Name + " id=\"" + svgElement.Attribute("id")?.Value + "\">");
                                }
                                break;
                            case "scale":
                                {
                                    if (values.Count == 1)
                                        avElement.SvgTransforms.Add(Matrix.CreateScale(values[0]));
                                    else if (values.Count == 2)
                                        avElement.SvgTransforms.Add(Matrix.CreateScale(values[0], values[1]));
                                    else
                                        warnings.AddWarning("Ignoring " + type + " transform because invalid number of tokens in " + type + " transform [" + transform + "] in <" + svgElement.Name + " id=\"" + svgElement.Attribute("id")?.Value + "\">");
                                }
                                break;
                            case "rotate":
                                {
                                    if (values.Count == 1)
                                        avElement.SvgTransforms.Add(Matrix.CreateRotateDegrees(values[0]));
                                    else if (values.Count == 3)
                                        avElement.SvgTransforms.Add(Matrix.CreateRotateDegrees(values[0], new System.Drawing.PointF(values[1], values[2])));
                                    else
                                        warnings.AddWarning("Ignoring " + type + " transform because invalid number of tokens in " + type + " transform [" + transform + "] in <" + svgElement.Name + " id=\"" + svgElement.Attribute("id")?.Value + "\">");
                                }
                                break;
                            case "skewX":
                                {
                                    if (values.Count == 1)
                                        avElement.SvgTransforms.Add(Matrix.CreateSkewDegrees(values[0], 0));
                                    else
                                        warnings.AddWarning("Ignoring " + type + " transform because invalid number of tokens in " + type + " transform [" + transform + "] in <" + svgElement.Name + " id=\"" + svgElement.Attribute("id")?.Value + "\">");
                                }
                                break;
                            case "skewY":
                                {
                                    if (values.Count == 1)
                                        avElement.SvgTransforms.Add(Matrix.CreateSkewDegrees(0, values[0]));
                                    else
                                        warnings.AddWarning("Ignoring " + type + " transform because invalid number of tokens in " + type + " transform [" + transform + "] in <" + svgElement.Name + " id=\"" + svgElement.Attribute("id")?.Value + "\">");
                                }
                                break;
                            case "matrix":
                                {
                                    if (values.Count == 6)
                                        avElement.SvgTransforms.Add(new Matrix(values[0], values[1], values[2], values[3], values[4], values[5]));
                                    else
                                        warnings.AddWarning("Ignoring " + type + " transform: invalid number of tokens in " + type + " transform [" + transform + "] in <" + svgElement.Name + " id=\"" + svgElement.Attribute("id")?.Value + "\">");
                                }
                                break;
                            case "default":
                                warnings.AddWarning("Ignoring transform [" + transform + "] in <" + svgElement.Name + " id=\"" + svgElement.Attribute("id")?.Value + "\"> because it is not recogized.");
                                break;
                        }
                    }
                }
            }
        }



    }

}
