using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using AndroidVector;

namespace Svg2AndroidVector
{
    public static class Converter
    {

        public static AndroidVector.Vector ConvertSvg(XDocument svgDocument, List<string> warnings)
        {
            var svgElement = svgDocument.Root;
            if (svgDocument.Root.Name != Namespace.Svg + "svg")
                throw new ArgumentException("document must have <svg> root");

            var vector = new AndroidVector.Vector();
            AndroidVector.Group avGroup = vector;
            float width = 0, height = 0;
            if (svgElement.Attribute("width") is XAttribute widthAttribute && AttributeExtensions.TryGetValueInPx(widthAttribute, out width))
                vector.Width = new AndroidVector.UnitizedFloat(width, AndroidVector.Unit.Px);
            else
                widthAttribute = null;
            if (svgElement.Attribute("height") is XAttribute heightAttribute && AttributeExtensions.TryGetValueInPx(heightAttribute, out height))
                vector.Height = new AndroidVector.UnitizedFloat(height, AndroidVector.Unit.Px);
            else
                heightAttribute = null;
            if (svgElement.Attribute("viewBox") is XAttribute viewBoxAttribute)
            {
                var args = viewBoxAttribute.Value.Trim().Split(new char[] { ',', ' ' });
                float x = 0, y = 0;
                if (args.Length > 0)
                    ElementExtensions.TryGetValueInPx(svgElement, args[0], Orientation.Horizontal, out x);
                if (args.Length > 1)
                    ElementExtensions.TryGetValueInPx(svgElement, args[1], Orientation.Vertical, out y);
                if (args.Length > 2 && ElementExtensions.TryGetValueInPx(svgElement, args[2], Orientation.Horizontal, out float portW))
                    vector.ViewportWidth = portW;
                if (args.Length > 3 && ElementExtensions.TryGetValueInPx(svgElement, args[3], Orientation.Vertical, out float portH))
                    vector.ViewportHeight = portH;
                if (x != 0 || y != 0)
                {
                    warnings.AddWarning("SVG viewBox has an x and/or y value.  This may result in part of your image being clipped.  Suggestion: use a vector image editor (like InkScape) to create the document boundaries for your SVG file.");
                    /*
                    avGroup = NestGroup(avGroup);
                    if (x != 0)
                        avGroup.TranslateX = -x;
                    if (y != 0)
                        avGroup.TranslateY = -y;
                    */
                    if (x != 0 || y != 0)
                        vector.SvgTransforms.Add(AndroidVector.Matrix.CreateTranslate(-x, -y));
                }
            }
            else
                viewBoxAttribute = null;

            InsertUseElements(svgElement, warnings);
            ProcessGroupContents(svgElement, avGroup, warnings);

            avGroup.MapGradients();
            avGroup.ApplySvgTransforms();
            avGroup.ApplySvgOpacity();
            avGroup.PurgeDefaults();

            if (viewBoxAttribute is null)
            {
                var bounds = avGroup.GetBounds();
                if (bounds.Width == 0 || bounds.Height == 0)
                {
                    warnings.AddWarning("Calculated width x height of SVG image is " + bounds.Width + " x " + bounds.Height + " AND SVG image does not have 'viewBox' attribute.  Cannot calculated AndroidVector ViewPort.");
                }
                else
                {
                    warnings.AddWarning("SVG element does not contain viewBox attribute.  Making best guess for AndroidVector.ViewPort but you're likely not going to like the result. Suggestion: use a vector image editor (like InkScape) to create the document boundaries for your SVG file. ");
                    if ((widthAttribute == null && heightAttribute == null) || (bounds.Width == 0 || bounds.Height == 0))
                    {
                        vector.ViewportHeight = bounds.Height + bounds.X;
                        vector.ViewportWidth = bounds.Width + bounds.Y;
                    }
                    else if (heightAttribute == null)
                    {
                        vector.ViewportWidth = bounds.Width + bounds.X;
                        var aspect = bounds.Width / bounds.Height;
                        height = vector.Width.Value / aspect;
                        vector.ViewportHeight = height + bounds.Y;
                    }
                    else if (widthAttribute == null)
                    {
                        vector.ViewportHeight = bounds.Height + bounds.Y;
                        var aspect = bounds.Width / bounds.Height;
                        width = vector.Height.Value * aspect;
                        vector.ViewportWidth = width + bounds.X;
                    }
                    else
                    {
                        var contentAspect = bounds.Width / bounds.Height;
                        var providedAspect = width / height;
                        vector.ViewportWidth = bounds.Width + bounds.X;
                        vector.ViewportHeight = bounds.Height + bounds.Y;
                        if (contentAspect > providedAspect)
                            heightAttribute = null;
                        else
                            widthAttribute = null;
                    }
                    if (bounds.Left != 0 || bounds.Top != 0)
                    {
                        var elements = vector.Elements().ToArray();
                        if (elements.Length == 1 && elements[0] is AndroidVector.Group group)
                        {
                            group.TranslateX -= bounds.X;
                            group.TranslateY -= bounds.Y;
                        }
                        else
                        {
                            vector.RemoveNodes();
                            avGroup = NestGroup(vector);
                            foreach (var item in elements)
                                avGroup.Add(item);
                        }
                    }
                }
            }

            if (widthAttribute is null && heightAttribute is null)
            {
                warnings.AddWarning("SVG element does not contain width and/or height attributes.");
                if (widthAttribute is null)
                    vector.Width = new AndroidVector.UnitizedFloat((float)vector.ViewportWidth, AndroidVector.Unit.Px);
                if (heightAttribute is null)
                    vector.Height = new AndroidVector.UnitizedFloat((float)vector.ViewportHeight, AndroidVector.Unit.Px);
            }
            else if (widthAttribute is null)
            {
                var aspect = vector.ViewportWidth / vector.ViewportHeight;
                vector.Width = new AndroidVector.UnitizedFloat((float)(height * aspect), AndroidVector.Unit.Px);
            }
            else if (heightAttribute is null)
            {
                var aspect = vector.ViewportWidth / vector.ViewportHeight;
                vector.Height = new AndroidVector.UnitizedFloat((float)(width / aspect), AndroidVector.Unit.Px);
            }

            return vector;
        }

        public static void InsertUseElements(XElement svgElement, List<string> warnings)
        {
            foreach (var child in svgElement.Elements().ToArray())
            {
                if (child.Name == Namespace.Svg + "use")
                {
                    if (child.Attribute(Namespace.xlinkNs + "href") is XAttribute hrefAttribute)
                    {
                        if (hrefAttribute.Value.StartsWith("#"))
                        {
                            var href = hrefAttribute.Value.Substring(1).Trim(')').Trim();
                            var root = svgElement.GetRoot();
                            var tranform = new XAttribute("transform", "");
                            if (root.Descendants().Where(e => e.Attribute("id")?.Value == href).FirstOrDefault() is XElement targetElement)
                            {
                                var copies = root.Descendants().Where(e => e.Attribute("id")?.Value.StartsWith(href + "_") ?? false).ToArray();
                                var index = 0;
                                if (copies.Length > 0)
                                {
                                    foreach (var copy in copies)
                                    {
                                        if (copy.Attribute("id") is XAttribute idAttribute && int.TryParse(idAttribute.Value.Substring((href + "_").Length), out int value))
                                            if (value > index)
                                                index = value;
                                    }
                                }
                                var newTarget = new XElement(targetElement);
                                newTarget.SetAttributeValue("id", href + "_" + (index + 1));
                                child.AddAfterSelf(newTarget);
                                //child.ReplaceWith(newTarget);

                                //string x = null, y = null;
                                foreach (var useAttribute in child.Attributes())
                                {
                                    if (useAttribute.Name == Namespace.xlinkNs + "href")
                                    {

                                    }
                                    else if (useAttribute.Name == "x" || useAttribute.Name == "y" || useAttribute.Name == "width" || useAttribute.Name == "height")
                                        SetLinearAttribute(useAttribute.Name, child, newTarget, warnings);
                                    else if (useAttribute.Name == "transform")
                                    {
                                        if (newTarget.Attribute("transform") is XAttribute targetAttribute)
                                            newTarget.SetAttributeValue("transform", targetAttribute.Value + " " + useAttribute.Value);
                                        else
                                            newTarget.SetAttributeValue("transform", useAttribute.Value);
                                    }
                                    else if (useAttribute.Name == "style")
                                    {
                                        if (newTarget.Attribute("style") is XAttribute targetAttribute)
                                            newTarget.SetAttributeValue("style", useAttribute.Value + (useAttribute.Value.EndsWith(";") ? null : ";") + targetAttribute.Value);
                                        else
                                            newTarget.SetAttributeValue("style", useAttribute.Value);
                                    }
                                    else if (useAttribute.Name == "class")
                                    {
                                        if (newTarget.Attribute("class") is XAttribute targetAttribute)
                                            newTarget.SetAttributeValue("class", targetAttribute.Value + " " + useAttribute.Value);
                                        else
                                            newTarget.SetAttributeValue("class", useAttribute.Value);
                                    }
                                    else if (useAttribute.Name == "id")
                                    { }
                                    else if (newTarget.Attribute(useAttribute.Name) is XAttribute unknownAttribute && unknownAttribute.Value != "inherit")
                                        warnings.AddWarning("Ignoring unanticipated <use> attribute " + useAttribute + " because it was expected to be found [" + unknownAttribute + "] in the target element.");
                                    else
                                        newTarget.SetAttributeValue(useAttribute.Name, useAttribute.Value);
                                }

                                InsertUseElements(newTarget , warnings);
                            }
                            else
                                warnings.AddWarning("Ignoring <use id='" + svgElement.Attribute("id")?.Value + "' xlink:href='" + hrefAttribute.Value + "' because could not find element referenced by xlink:href attribute.");
                        }
                        else
                            warnings.AddWarning("Ignoring <use id='" + child.Attribute("id")?.Value + "'> because the xlink:href attribute (" + hrefAttribute.Value + ") is not a local anchor URL.");
                    }
                    else
                        warnings.AddWarning("Ignoring <use id='" + child.Attribute("id")?.Value + "'> because it does not have an xlink:href attribute.");
                }
                else if (child.Name == Namespace.Svg + "title" ||
                         child.Name == Namespace.Svg + "script" ||
                         child.Name == Namespace.Svg + "metadata" ||
                         child.Name == Namespace.Svg + "animate")
                    child.Remove();
                else if (child.Name == Namespace.Svg + "defs")
                { }
                else if (child.HasElements)
                    InsertUseElements(child, warnings);
            }
        }

        static void SetLinearAttribute(XName name, XElement useElement, XElement targetElement, List<string> warnings)
        {
            if (useElement.Attribute(name) is XAttribute useAttribute)
            {
                if (targetElement.Attribute(name) is XAttribute targetAttribute)
                {
                    var useText = useAttribute.Value.Trim();
                    bool usePercent = false;
                    if (useText.EndsWith("%"))
                    {
                        usePercent = true;
                        useText.Trim('%');
                    }
                    if (float.TryParse(useText, out float useValue))
                    {
                        if (usePercent)
                            useValue /= 100;
                        var targetText = targetAttribute.Value.Trim();
                        bool targetPercent = false;
                        if (targetText.EndsWith("%"))
                        {
                            targetPercent = true;
                            targetText.Trim('%');
                        }
                        if (float.TryParse(targetText, out float targetValue))
                        {
                            if (targetPercent)
                                targetValue /= 100;
                            if (usePercent || targetPercent)
                            {
                                var result = useValue * targetValue;
                                if (usePercent && targetPercent)
                                    result *= 100;
                                targetElement.SetAttributeValue(name, result + "%");
                            }
                            else
                            {
                                targetElement.SetAttributeValue(name, useValue + targetValue);
                            }
                        }
                        else
                            warnings.AddWarning("Ignoring "+name+"='" + targetAttribute.Value + "' argument in use target <" + targetElement.Name + " id='" + targetElement.Attribute("id")?.Value + "'> because could not parse it.");
                    }
                    else
                        warnings.AddWarning("Ignoring " + name + "='" + useAttribute.Value + "' argument in <use xlink:href='" + useElement.Attribute(Namespace.xlinkNs+"href")?.Value + "'> because could not parse it.");

                }
                else
                    targetElement.SetAttributeValue(name, useAttribute.Value);
            }
        }

        public static AndroidVector.Group AddGroup(XElement svgElement, AndroidVector.Group avGroup, List<string> warnings)
        {
            string typeName = svgElement.Name.LocalName;
            if (typeName != "g" && typeName != "a")
                throw new ArgumentException("found <" + typeName + "> SVG element but was expected <g> or <a>.");

            avGroup = NestGroup(avGroup);
            avGroup = ClipConverter.ConvertClipPathAttribute(svgElement, avGroup, warnings);
            CommonAttributes.SetTransforms(svgElement, avGroup, warnings);
            CommonAttributes.ProcessAttributes(svgElement, avGroup, null, warnings);
            ProcessGroupContents(svgElement, avGroup, warnings);
            return avGroup;
        }

        /*
        public static void AddUse(XElement svgElement, AndroidVector.Group avGroup, List<string> warnings)
        {
            const string typeName = "use";
            if (svgElement.Name != Namespace.Svg + typeName)
                throw new ArgumentException("argument is not the expected <" + typeName + "> SVG element");


            if (svgElement.Attribute(Namespace.xlinkNs + "href") is XAttribute hrefAttribute)
            {
                if (!hrefAttribute.Value.StartsWith("#"))
                {
                    warnings.AddWarning("Ignoring <use id='" + svgElement.Attribute("id")?.Value + "' xlink:href='" + hrefAttribute.Value + "' because xlink:href attribute is not a local anchor.");
                    return;
                }
                var href = hrefAttribute.Value.Trim(new char[] { '#', ' ' });
                var root = svgElement.GetRoot();
                if (root.Descendants().Where(e => e.Attribute("id")?.Value == href).FirstOrDefault() is XElement useElement)
                {
                    avGroup = NestGroup(avGroup);
                    if (useElement.Attribute("x") is XAttribute xAttribute && AttributeExtensions.TryGetValueInPx(xAttribute, out float x))
                        avGroup.SetAttributeValue(AndroidVector.Namespace.AndroidVector + "translateX", x);
                    if (useElement.Attribute("y") is XAttribute yAttribute && AttributeExtensions.TryGetValueInPx(yAttribute, out float y))
                        avGroup.SetAttributeValue(AndroidVector.Namespace.AndroidVector + "translateY", y);
                    avGroup = ClipConverter.ConvertClipPathAttribute(svgElement, avGroup, warnings);
                    CommonAttributes.SetTransforms(svgElement, avGroup, warnings);
                    CommonAttributes.ProcessAttributes(svgElement, avGroup, new List<string> { "x", "y" }, warnings);
                    ProcessGroupContents(svgElement, avGroup, warnings);
                    return;
                }
                warnings.AddWarning("Ignoring <use id='" + svgElement.Attribute("id")?.Value + "' xlink:href='" + hrefAttribute.Value + "' because could not find element referenced by xlink:href attribute.");
                return;
            }
            warnings.AddWarning("Ignoring <use id='" + svgElement.Attribute("id")?.Value + "'> because cannot find xlink:href attribute.");
        }
        */

        public static void AddSvg(XElement svgElement, AndroidVector.Group avGroup, List<string> warnings)
        {
            const string typeName = "svg";
            if (svgElement.Name != Namespace.Svg + typeName)
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
                    if (ElementExtensions.TryGetValueInPx(svgElement, args[0], Orientation.Horizontal, out float vb_x))
                    {
                        if (ElementExtensions.TryGetValueInPx(svgElement, args[1], Orientation.Vertical, out float vb_y))
                        {
                            if (ElementExtensions.TryGetValueInPx(svgElement, args[2], Orientation.Horizontal, out float vb_w) && vb_w != 0)
                            {
                                if (ElementExtensions.TryGetValueInPx(svgElement, args[3], Orientation.Horizontal, out float vb_h) && vb_h != 0)
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
                                    warnings.AddWarning("Ignoring <"+typeName+" id='" + svgElement.Attribute("id")?.Value + "'  viewBox [" + viewBoxAttribute.Value + "] because could not parse port Height argument.");
                            }
                            else
                                warnings.AddWarning("Ignoring <" + typeName + " id='" + svgElement.Attribute("id")?.Value + "'  viewBox [" + viewBoxAttribute.Value + "] because could not parse prot Width argument.");
                        }
                        else
                            warnings.AddWarning("Ignoring <" + typeName + " id='" + svgElement.Attribute("id")?.Value + "' viewBox [" + viewBoxAttribute.Value + "] because could not parse Y argument.");
                    }
                    else
                        warnings.AddWarning("Ignoring <" + typeName + " id='" + svgElement.Attribute("id")?.Value + "'  viewBox [" + viewBoxAttribute.Value + "] because could not parse X argument.");
                }
                else
                    warnings.AddWarning("Ignoring <" + typeName + " id='" + svgElement.Attribute("id")?.Value + "'  viewBox ["+ viewBoxAttribute.Value+ "] because wrong number of arguments.");
                }
            else
                warnings.AddWarning("Could not find a viewBox attribute in <" + typeName + " id='" + svgElement.Attribute("id")?.Value + "'>.");

            avGroup = ClipConverter.ConvertClipPathAttribute(svgElement, avGroup, warnings);
            CommonAttributes.SetTransforms(svgElement, avGroup, warnings);
            CommonAttributes.ProcessAttributes(svgElement, avGroup, new List<string> { "viewBox" }, warnings);
            ProcessGroupContents(svgElement, avGroup, warnings);
            return;
        }

        public static void ProcessGroupContents(XElement svgElement, AndroidVector.Group avGroup, List<string> warnings)
        {
            foreach (var child in svgElement.Elements())
            {
                if (child.Name == Namespace.Svg + "path")
                    GeometryConverter.AddPath(child, avGroup, warnings);
                else if (child.Name == Namespace.Svg + "rect")
                    GeometryConverter.AddRect(child, avGroup, warnings);
                else if (child.Name == Namespace.Svg + "circle")
                    GeometryConverter.AddCircle(child, avGroup, warnings);
                else if (child.Name == Namespace.Svg + "ellipse")
                    GeometryConverter.AddEllipse(child, avGroup, warnings);
                else if (child.Name == Namespace.Svg + "line")
                    GeometryConverter.AddLine(child, avGroup, warnings);
                else if (child.Name == Namespace.Svg + "polyline")
                    GeometryConverter.AddPolyline(child, avGroup, warnings);
                else if (child.Name == Namespace.Svg + "polygon")
                    GeometryConverter.AddPolygon(child, avGroup, warnings);
                else if (child.Name == Namespace.Svg + "g" || child.Name == Namespace.Svg + "a")
                {
                    if (child.HasElements)
                        AddGroup(child, avGroup, warnings);
                }
                else if (child.Name == Namespace.Svg + "svg")
                {
                    if (child.HasElements)
                        AddSvg(child, avGroup, warnings);
                }
                else if (child.Name == Namespace.Svg + "style")
                {
                    if (child.Attribute("type") is XAttribute typeAttribute && typeAttribute.Value != "text/css")
                        warnings.AddWarning("Ignoring style attribute [" + typeAttribute.Value + "] because it is not recognized.");
                    else
                        StyleConverter.StoreCssStyles(child, warnings);
                }
                else if (child.Name == Namespace.Svg + "desc")
                { }
                else if (child.Name == Namespace.Svg + "defs")
                { }
                else if (child.Name == Namespace.Svg + "metadata")
                { }
                else if (child.Name == Namespace.Svg + "animate")
                { }
                else if (child.Name == Namespace.Svg + "script")
                { }
                else if (child.Name == Namespace.Svg + "title")
                { }
                else if (child.Name == Namespace.Svg + "linearGradient")
                { }
                else if (child.Name == Namespace.Svg + "radialGradient")
                { }
                else if (child.Name == Namespace.Svg + "use")
                { }
                else if (child.Name == Namespace.Svg + "text")
                {
                    if ((child.HasElements || !string.IsNullOrWhiteSpace(child.Value)) &&
                        (!(child.Attribute("style") is XAttribute styleAttribute) ||
                          !styleAttribute.Value.Contains("display:none"))
                          )
                    {
                        warnings.AddWarning("Ignoring unsupported element <" + child.Name + " id='" + child.Attribute("id")?.Value + "'>.  Suggestions: Convert text to paths before processing.  In INKSCAPE, this can be done by selecting text and then selecting the PATH > OBJECT TO PATH menu item.");
                    }
                }
                else
                {
                    if (child.Name.ToString().StartsWith("{" + Namespace.Svg + "}"))
                        warnings.AddWarning("Ignoring unsupported element <" + child.Name + " id='" + child.Attribute("id")?.Value + "'>.");
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
