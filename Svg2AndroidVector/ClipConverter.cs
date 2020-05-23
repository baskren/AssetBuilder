using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using AndroidVector;

namespace Svg2AndroidVector
{
    public static class ClipConverter
    {
        /// <summary>
        /// Creates AndroidVector.Clip child  from Clip attribute of SVG element
        /// </summary>
        /// <param name="svgSvgElement"></param>
        /// <param name="avVector"></param>
        /// <param name="warnings"></param>
        /// <returns></returns>
        public static void ConvertClipAttribute(XElement svgSvgElement, AndroidVector.BaseElement av, List<string> warnings)
        {
            const string svgTypeName = "svg";
            if (svgSvgElement.Name != Namespace.Svg + svgTypeName)
                throw new ArgumentException("Only applicable to SVG <svg> element");

            if (av.Name != "vector" && av.Name != "group")
                throw new ArgumentException("Only applicable to Android <vector> and <group> elements.");

            if (av.Attribute("clip") is XAttribute clipAttribute)
            {
                if (clipAttribute.Value == "auto")
                {
                    if (svgSvgElement.Attribute("viewBox") is XAttribute svgViewBoxAttribute)
                    {
                        var args = svgViewBoxAttribute.Value.Split(new char[] { ' ', ',' });
                        if (args.Length < 1 || !ElementExtensions.TryGetValueInPx(svgSvgElement, args[0], Orientation.Horizontal, out float x))
                        {
                            warnings.AddWarning("Ignoring <svg clip='auto'> because 'viewBox' does not contain x coordinate.");
                            return;
                        }
                        if (args.Length < 2 || !ElementExtensions.TryGetValueInPx(svgSvgElement, args[1], Orientation.Vertical, out float y))
                        {
                            warnings.AddWarning("Ignoring <svg clip='auto'> because 'viewBox' does not contain y coordinate.");
                            return;
                        }
                        if (args.Length < 3 || !ElementExtensions.TryGetValueInPx(svgSvgElement, args[2], Orientation.Horizontal, out float w))
                        {
                            warnings.AddWarning("Ignoring <svg clip='auto'> because 'viewBox' does not contain width.");
                            return;
                        }
                        if (args.Length < 4 || !ElementExtensions.TryGetValueInPx(svgSvgElement, args[3], Orientation.Vertical, out float h))
                        {
                            warnings.AddWarning("Ignoring <svg clip='auto'> because 'viewBox' does not contain height.");
                            return;
                        }
                        string pathData = " M " + x + "," + y;
                        pathData += " v " + h;
                        pathData += " h " + w;
                        pathData += " v " + (-h);
                        pathData += " z ";
                        av.Add(new XElement("clip-path", new AndroidVector.AndroidAttribute("pathData", pathData)));
                    }
                    else
                    {
                        warnings.AddWarning("Ignoring <svg clip='auto'> because 'viewBox' attribute is not present.");
                        return;
                    }
                }
                else if (clipAttribute.Value.StartsWith("rect("))
                {
                    // top, right, bottom, left
                    var args = clipAttribute.Value.Split(new char[] { ' ', ',', '(', ')' });
                    if (args.Length < 2 || !ElementExtensions.TryGetValueInPx(svgSvgElement, args[1], Orientation.Vertical, out float top))
                    {
                        warnings.AddWarning("Ignoring <svg clip='rect(...)'> because 'rect' does not contain top coordinate.");
                        return;
                    }
                    if (args.Length < 3 || !ElementExtensions.TryGetValueInPx(svgSvgElement, args[2], Orientation.Horizontal, out float right))
                    {
                        warnings.AddWarning("Ignoring <svg clip='rect(...)'> because 'rect' does not contain right coordinate.");
                        return;
                    }
                    if (args.Length < 4 || !ElementExtensions.TryGetValueInPx(svgSvgElement, args[3], Orientation.Vertical, out float bottom))
                    {
                        warnings.AddWarning("Ignoring <svg clip='rect(...)'> because 'rect' does not contain bottom coordinate.");
                        return;
                    }
                    if (args.Length < 5 || !ElementExtensions.TryGetValueInPx(svgSvgElement, args[4], Orientation.Horizontal, out float left))
                    {
                        warnings.AddWarning("Ignoring <svg clip='rect(...)'> because 'rect' does not contain left coordinate.");
                        return;
                    }
                    string pathData = " M " + top + "," + left;
                    pathData += " V " + bottom;
                    pathData += " H " + right;
                    pathData += " V " + top;
                    pathData += " Z ";
                    av.Add(new XElement("clip-path", new AndroidVector.AndroidAttribute("pathData", pathData)));
                }
                else
                {
                    warnings.AddWarning("Ignoring <svg clip='"+clipAttribute.Value+"'> because '"+clipAttribute.Value+"' is an unexpected attribute value.");
                    return;
                }
            }
        }


        //TODO: Process clip paths after all of the SVG has been converted (just like gradients)
        public static AndroidVector.Group ConvertClipPathAttribute(this XElement svgElement, AndroidVector.Group avGroup, List<string> warnings)
        {
            if (svgElement.Attribute("clip-path") is XAttribute svgAttribute)
            {
                var svgAttributeValue = svgAttribute.Value.Trim();
                if (svgAttributeValue == "inherit")
                {
                    var parent = svgAttribute.Parent.Parent;
                    while (parent != null)
                    {
                        if (parent.Attribute(svgAttribute.Name) is XAttribute parentAttribute && parentAttribute.Value.Trim() != "inherit")
                        {
                            svgAttributeValue = parentAttribute.Value.Trim();
                            return avGroup;
                        }
                    }
                }

                if (svgElement.Name != Namespace.Svg + "g" || svgElement.Name != Namespace.Svg + "clipPath")
                    avGroup = Converter.NestGroup(avGroup);

                if (svgAttributeValue.StartsWith("url("))
                {
                    if (!svgAttributeValue.StartsWith("url(#"))
                        throw new Exception("Only anchor URLs are supported at this time.");

                    var iri = svgAttributeValue.Substring("url(#".Length).Trim(')').Trim();
                    if (svgElement.GetRoot() is XElement root)
                    {
                        if (root.Descendants(Namespace.Svg + "clipPath").FirstOrDefault(e=>e.Attribute("id").Value == iri) is XElement svgClipPathElement)
                        {
                            //AndroidVector.Group result = avGroup is AndroidVector.Group ? avGroup : new AndroidVector.Group();
                            avGroup = AddClipPathElement(svgClipPathElement, avGroup, warnings);
                            return avGroup;
                        }
                        warnings.AddWarning("Ignoring clip-path because no element found to complete clipPath link to " + svgAttributeValue + ".");
                        return avGroup;
                    }
                    throw new Exception("could not find document root");
                }
                else if (GeometryConverter.ConvertCssShapeToPathData(svgElement, svgAttributeValue, warnings) is string pathData)
                {
                    avGroup.Add(new ClipPath(pathData));
                }
                else
                {
                    warnings.AddWarning("Ignoring clip-path attribute because could not interpret value : " + svgAttributeValue + ".");
                }
            }
            return avGroup;
        }


        public static AndroidVector.Group AddClipPathElement(this XElement svgElement, AndroidVector.Group avGroup, List<string> warnings)
        {
            string typeName = svgElement.Name.LocalName;

            avGroup = ClipConverter.ConvertClipPathAttribute(svgElement, avGroup, warnings);
            CommonAttributes.ProcessAttributes(svgElement, avGroup, null, warnings);
            var avClip = new AndroidVector.ClipPath();
            foreach (var child in svgElement.Elements())
            {
                if (child.Name == Namespace.Svg + "use")
                {
                    float tx = 0, ty = 0;
                    if (child.Attribute("x") is XAttribute xAttribute)
                        AttributeExtensions.TryGetValueInPx(xAttribute, out tx);
                    if (child.Attribute("y") is XAttribute yAttribute)
                        AttributeExtensions.TryGetValueInPx(yAttribute, out ty);
                    if (tx != 0 || ty != 0)
                        avGroup.SvgTransforms.Add(Matrix.CreateTranslate(tx, ty));

                    if (child.Attribute(Namespace.xlinkNs + "href") is XAttribute hrefAttribute)
                    {
                        if (hrefAttribute.Value.StartsWith("#"))
                        {
                            var href = hrefAttribute.Value.Trim(new char[] { '#', ' ' });
                            var root = child.GetRoot();
                            if (root.Descendants().Where(e => e.Attribute("id")?.Value == href).FirstOrDefault() is XElement useElement)
                            {
                                if (useElement.Name == Namespace.Svg + "clipPath")
                                {
                                    avGroup = ClipConverter.ConvertClipPathAttribute(useElement, avGroup, warnings);
                                    CommonAttributes.ProcessAttributes(useElement, avGroup, new List<string> { "x", "y" }, warnings);
                                    AddClipPathElement(useElement, avGroup, warnings);
                                }
                                else if (GeometryConverter.ConvertElementToPathData(useElement, warnings) is string pathData)
                                {
                                    avClip.PathData += pathData;
                                    warnings.AddWarning("A union of two (or more) clippaths has been found.  If these clippaths overlap, you're likely not going to like the result.  Suggestion:  Use a vector image editor (like InkScape) to alter your union of clippaths to be one path, without crossing segments.");
                                }
                                else
                                    warnings.AddWarning("Ignoring <" + useElement.Name.LocalName + " id='" + useElement.Attribute("id")?.Value + "'> inside of <" + child.Name.LocalName + " id='" + child.Attribute("id")?.Value + "' xlink:href='" + hrefAttribute.Value + "' because could not find element referenced by xlink:href attribute.");
                            }
                            else
                            {
                                warnings.AddWarning("Ignoring <use id='" + svgElement.Attribute("id")?.Value + "' xlink:href='" + hrefAttribute.Value + "' because could not find element referenced by xlink:href attribute.");
                            }
                        }
                        else
                        {
                            warnings.AddWarning("Ignoring <use id='" + svgElement.Attribute("id")?.Value + "' xlink:href='" + hrefAttribute.Value + "' because xlink:href attribute is not a local anchor.");
                        }
                    }
                    else
                    {
                        warnings.AddWarning("Ignoring <use id='" + svgElement.Attribute("id")?.Value + "'> because cannot find xlink:href attribute.");
                    }
                    //    var tmpGroup = new AndroidVector.Group();
                    
                }
                else if (GeometryConverter.ConvertElementToPathData(child, warnings) is string pathData)
                    avClip.PathData += pathData;
            }

            if (!string.IsNullOrWhiteSpace(avClip.PathData))
            {
                CommonAttributes.SetTransforms(svgElement, avClip, warnings);
                avGroup.Add(avClip);
            }

            return avGroup;
        }

    }
}
