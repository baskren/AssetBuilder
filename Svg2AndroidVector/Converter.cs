using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Svg2AndroidVector
{
    public static class Converter
    {

        public static AndroidVector.Vector ConvertSvg(XDocument svgDocument, List<string> warnings)
        {
            var svgElement = svgDocument.Root;
            if (svgDocument.Root.Name != NameSpace.Svg + "svg")
                throw new ArgumentException("document must have <svg> root");

            var vector = new AndroidVector.Vector();
            AndroidVector.Group avGroup = vector;
            if (svgElement.Attribute("width") is XAttribute widthAttribute && AttributeExtensions.TryGetValueInPx(widthAttribute, out float width))
                vector.Width = new AndroidVector.UnitizedFloat(width, AndroidVector.Unit.Px);
            if (svgElement.Attribute("height") is XAttribute heightAttribute && AttributeExtensions.TryGetValueInPx(heightAttribute, out float height))
                vector.Height = new AndroidVector.UnitizedFloat(height, AndroidVector.Unit.Px);
            if (svgElement.Attribute("viewBox") is XAttribute xAttribute)
            {
                var args = xAttribute.Value.Trim().Split(new char[] { ',', ' ' });
                float x = 0, y = 0;
                if (args.Length > 0)
                    AttributeExtensions.TryGetValueInPx(svgElement, args[0], Orientation.Horizontal, out x);
                if (args.Length > 1)
                    AttributeExtensions.TryGetValueInPx(svgElement, args[1], Orientation.Vertical, out y);
                if (args.Length > 2 && AttributeExtensions.TryGetValueInPx(svgElement, args[2], Orientation.Horizontal, out float portW))
                    vector.ViewportWidth = portW;
                if (args.Length > 3 && AttributeExtensions.TryGetValueInPx(svgElement, args[3], Orientation.Vertical, out float portH))
                    vector.ViewportHeight = portH;
                if (x != 0 || y != 0)
                {
                    avGroup = NestGroup(avGroup);
                    if (x != 0)
                        avGroup.TranslateX = x;
                    if (y != 0)
                        avGroup.TranslateY = y;
                }
            }

            ProcessGroupContents(svgElement, avGroup, warnings);

            avGroup.ApplySvgTransforms();

            return vector;
        }

        public static AndroidVector.Group AddGroup(XElement svgElement, AndroidVector.Group avGroup, List<string> warnings)
        {
            const string typeName = "g";
            if (svgElement.Name != NameSpace.Svg + typeName)
                throw new ArgumentException("argument is not the expected <" + typeName + "> SVG element");

            avGroup = NestGroup(avGroup);
            avGroup = ClipConverter.ConvertClipPathAttribute(svgElement, avGroup, warnings);
            CommonAttributes.ProcessAttributes(svgElement, avGroup, CommonAttributes.AttributeMap, null, warnings);
            ProcessGroupContents(svgElement, avGroup, warnings);
            return avGroup;
        }

        public static void AddUse(XElement svgElement, AndroidVector.Group avGroup, List<string> warnings)
        {
            const string typeName = "use";
            if (svgElement.Name != NameSpace.Svg + typeName)
                throw new ArgumentException("argument is not the expected <" + typeName + "> SVG element");

            XNamespace xlinkNs = "http://www.w3.org/1999/xlink";

            if (svgElement.Attribute(xlinkNs + "href") is XAttribute hrefAttribute)
            {
                if (!hrefAttribute.Value.StartsWith("#"))
                {
                    warnings.Add("ignoring <use id='" + svgElement.Attribute("id")?.Value + "' xlink:href='" + hrefAttribute.Value + "' because xlink:href attribute is not a local anchor.");
                    return;
                }
                var href = hrefAttribute.Value.Trim(new char[] { '#', ' ' });
                var root = svgElement.GetRoot();
                if (root.Descendants().Where(e => e.Attribute("id")?.Value == href).FirstOrDefault() is XElement useElement)
                {
                    avGroup = new AndroidVector.Group(avGroup);
                    if (useElement.Attribute("x") is XAttribute xAttribute && AttributeExtensions.TryGetValueInPx(xAttribute, out float x))
                        avGroup.SetAttributeValue(AndroidVector.BaseElement.avNs + "translateX", x);
                    if (useElement.Attribute("y") is XAttribute yAttribute && AttributeExtensions.TryGetValueInPx(yAttribute, out float y))
                        avGroup.SetAttributeValue(AndroidVector.BaseElement.avNs + "translateY", y);
                    avGroup = ClipConverter.ConvertClipPathAttribute(svgElement, avGroup, warnings);
                    CommonAttributes.ProcessAttributes(svgElement, avGroup, CommonAttributes.AttributeMap, new List<string> { "x", "y" }, warnings);
                    ProcessGroupContents(svgElement, avGroup, warnings);
                    return;
                }
                warnings.Add("ignoring <use id='" + svgElement.Attribute("id")?.Value + "' xlink:href='" + hrefAttribute.Value + "' because could not find element referenced by xlink:href attribute.");
                return;
            }
            warnings.Add("ignoring <use id='" + svgElement.Attribute("id")?.Value + "'> because cannot find xlink:href attribute.");
        }

        public static void AddSvg(XElement svgElement, AndroidVector.Group avGroup, List<string> warnings)
        {
            const string typeName = "svg";
            if (svgElement.Name != NameSpace.Svg + typeName)
                throw new ArgumentException("argument is not the expected <" + typeName + "> SVG element");

            avGroup = NestGroup(avGroup);

            AttributeExtensions.TryGetValueInPx(svgElement.Attribute("width"), out float e_w);
            AttributeExtensions.TryGetValueInPx(svgElement.Attribute("height"), out float e_h);

            var align = "xMidYMid";
            var meetOrSlice = "meet";
            if (svgElement.Attribute("preserveAspectRatio") is XAttribute preserveAspectAttribute)
            {
                var split = preserveAspectAttribute.Value.Trim().Split(' ');
                align = split[0].Trim();
                meetOrSlice = split[1].Trim();
            }
            if (svgElement.Attribute("viewBox") is XAttribute viewBoxAttribute)
            {
                var args = viewBoxAttribute.Value.Trim().Split(new char[] { ',', ' ' });
                if (args.Length == 4)
                {
                    if (AttributeExtensions.TryGetValueInPx(svgElement, args[0], Orientation.Horizontal, out float vb_x))
                    {
                        if (AttributeExtensions.TryGetValueInPx(svgElement, args[1], Orientation.Vertical, out float vb_y))
                        {
                            if (AttributeExtensions.TryGetValueInPx(svgElement, args[2], Orientation.Horizontal, out float vb_w) && vb_w != 0)
                            {
                                if (AttributeExtensions.TryGetValueInPx(svgElement, args[3], Orientation.Horizontal, out float vb_h) && vb_h != 0)
                                {
                                    if (float.IsNaN(e_w) ||e_w <= 0)
                                        e_w = vb_w;
                                    if (float.IsNaN(e_h) || e_h <= 0)
                                        e_h = vb_h;
                                    var scale_x = e_w / vb_w;
                                    var scale_y = e_h / vb_h;
                                    if (align != "none")
                                    {
                                        if (meetOrSlice == "meet")
                                            scale_x = scale_y = Math.Min(scale_x, scale_y);
                                        else if (meetOrSlice == "slice")
                                            scale_x = scale_y = Math.Max(scale_x, scale_y);
                                        var translate_x = -(vb_x * scale_x) + (e_w - vb_w * scale_x) / (align.Contains("xMid") ?  2f : 1f );
                                        var translate_y = -(vb_y * scale_y) + (e_h - vb_h * scale_y) / (align.Contains("yMid") ? 2f : 1f);

                                        avGroup.SvgTransforms.Add(AndroidVector.Matrix.CreateTranslate(translate_x, translate_y));
                                        avGroup.SvgTransforms.Add(AndroidVector.Matrix.CreateScale(scale_x, scale_y));

                                    }
                                }
                                else
                                    warnings.Add("ignoring <"+typeName+" id='" + svgElement.Attribute("id")?.Value + "'  viewBox [" + viewBoxAttribute.Value + "] because could not parse port Height argument.");
                            }
                            else
                                warnings.Add("ignoring <" + typeName + " id='" + svgElement.Attribute("id")?.Value + "'  viewBox [" + viewBoxAttribute.Value + "] because could not parse prot Width argument.");
                        }
                        else
                            warnings.Add("ignoring <" + typeName + " id='" + svgElement.Attribute("id")?.Value + "' viewBox [" + viewBoxAttribute.Value + "] because could not parse Y argument.");
                    }
                    else
                        warnings.Add("ignoring <" + typeName + " id='" + svgElement.Attribute("id")?.Value + "'  viewBox [" + viewBoxAttribute.Value + "] because could not parse X argument.");
                }
                else
                    warnings.Add("ignoring <" + typeName + " id='" + svgElement.Attribute("id")?.Value + "'  viewBox ["+ viewBoxAttribute.Value+ "] because wrong number of arguments.");
                }
            else
                warnings.Add("could not find <" + typeName + " id='" + svgElement.Attribute("id")?.Value + "'  viewBox attribute.");

            avGroup = ClipConverter.ConvertClipPathAttribute(svgElement, avGroup, warnings);
            CommonAttributes.ProcessAttributes(svgElement, avGroup, CommonAttributes.AttributeMap, new List<string> { "viewBox" }, warnings);
            ProcessGroupContents(svgElement, avGroup, warnings);
            return;
        }

        public static void ProcessGroupContents(XElement svgElement, AndroidVector.Group avGroup, List<string> warnings)
        {
            foreach (var child in svgElement.Elements())
            {
                if (child.Name == NameSpace.Svg + "path")
                    GeometryConverter.AddPath(child, avGroup, warnings);
                else if (child.Name == NameSpace.Svg + "rect")
                    GeometryConverter.AddRect(child, avGroup, warnings);
                else if (child.Name == NameSpace.Svg + "circle")
                    GeometryConverter.AddCircle(child, avGroup, warnings);
                else if (child.Name == NameSpace.Svg + "ellipse")
                    GeometryConverter.AddEllipse(child, avGroup, warnings);
                else if (child.Name == NameSpace.Svg + "line")
                    GeometryConverter.AddLine(child, avGroup, warnings);
                else if (child.Name == NameSpace.Svg + "polyline")
                    GeometryConverter.AddPolyline(child, avGroup, warnings);
                else if (child.Name == NameSpace.Svg + "polygon")
                    GeometryConverter.AddPolygon(child, avGroup, warnings);
                else if (child.Name == NameSpace.Svg + "g")
                {
                    if (child.HasElements)
                        AddGroup(child, avGroup, warnings);
                }
                else if (child.Name == NameSpace.Svg + "use")
                    AddUse(child, avGroup, warnings);
                else if (child.Name == NameSpace.Svg + "svg")
                {
                    if (child.HasElements)
                        AddSvg(child, avGroup, warnings);
                }
                else if (child.Name == NameSpace.Svg + "style")
                {
                    if (child.Attribute("type") is XAttribute typeAttribute && typeAttribute.Value != "text/css")
                        warnings.Add("Unexpected <style> type attribute [" + typeAttribute.Value + "]");
                    else
                        StyleConverter.AddCssStyles(child, warnings);
                }
                else if (child.Name == NameSpace.Svg + "defs")
                { }
                else if (child.Name == NameSpace.Svg + "metadata")
                { }
                else if (child.Name == NameSpace.Svg + "text")
                {
                    if (child.HasElements &&
                        (!(child.Attribute("style") is XAttribute styleAttribute) ||
                          !styleAttribute.Value.Contains("display:none"))
                          )
                    {
                        warnings.Add("ignoring unsupported element <" + child.Name + " id='" + child.Attribute("id")?.Value + "'>");
                    }
                }
                else
                {
                    if (child.Name.ToString().StartsWith("{" + NameSpace.Svg + "}"))
                        warnings.Add("ignoring unsupported element <" + child.Name + " id='" + child.Attribute("id")?.Value + "'>");
                }
            }
        }

        public static AndroidVector.Group NestGroup(AndroidVector.Group group)
        {
            var result = new AndroidVector.Group();
            group?.Add(result);
            return result;
        }


    }
}
