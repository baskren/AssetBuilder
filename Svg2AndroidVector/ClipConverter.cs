using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

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
            if (svgSvgElement.Name != NameSpace.Svg + svgTypeName)
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
                        if (args.Length < 1 || !AttributeExtensions.TryGetValueInPx(svgSvgElement, args[0], Orientation.Horizontal, out float x))
                        {
                            warnings.Add("Ignoring <svg clip='auto'> because 'viewBox' does not contain x coordinate.");
                            return;
                        }
                        if (args.Length < 2 || !AttributeExtensions.TryGetValueInPx(svgSvgElement, args[1], Orientation.Vertical, out float y))
                        {
                            warnings.Add("Ignoring <svg clip='auto'> because 'viewBox' does not contain y coordinate.");
                            return;
                        }
                        if (args.Length < 3 || !AttributeExtensions.TryGetValueInPx(svgSvgElement, args[2], Orientation.Horizontal, out float w))
                        {
                            warnings.Add("Ignoring <svg clip='auto'> because 'viewBox' does not contain width.");
                            return;
                        }
                        if (args.Length < 4 || !AttributeExtensions.TryGetValueInPx(svgSvgElement, args[3], Orientation.Vertical, out float h))
                        {
                            warnings.Add("Ignoring <svg clip='auto'> because 'viewBox' does not contain height.");
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
                        warnings.Add("Ignoring <svg clip='auto'> because 'viewBox' attribute is not present.");
                        return;
                    }
                }
                else if (clipAttribute.Value.StartsWith("rect("))
                {
                    // top, right, bottom, left
                    var args = clipAttribute.Value.Split(new char[] { ' ', ',', '(', ')' });
                    if (args.Length < 2 || !AttributeExtensions.TryGetValueInPx(svgSvgElement, args[1], Orientation.Vertical, out float top))
                    {
                        warnings.Add("Ignoring <svg clip='rect(...)'> because 'rect' does not contain top coordinate.");
                        return;
                    }
                    if (args.Length < 3 || !AttributeExtensions.TryGetValueInPx(svgSvgElement, args[2], Orientation.Horizontal, out float right))
                    {
                        warnings.Add("Ignoring <svg clip='rect(...)'> because 'rect' does not contain right coordinate.");
                        return;
                    }
                    if (args.Length < 4 || !AttributeExtensions.TryGetValueInPx(svgSvgElement, args[3], Orientation.Vertical, out float bottom))
                    {
                        warnings.Add("Ignoring <svg clip='rect(...)'> because 'rect' does not contain bottom coordinate.");
                        return;
                    }
                    if (args.Length < 5 || !AttributeExtensions.TryGetValueInPx(svgSvgElement, args[4], Orientation.Horizontal, out float left))
                    {
                        warnings.Add("Ignoring <svg clip='rect(...)'> because 'rect' does not contain left coordinate.");
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
                    warnings.Add("Ignoring <svg clip='"+clipAttribute.Value+"'> because '"+clipAttribute.Value+"' is an unexpected attribute value.");
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
                        if (root.Descendants(NameSpace.Svg + "clipPath").FirstOrDefault(e=>e.Attribute("id").Value == iri) is XElement svgClipPathElement)
                        {
                            AndroidVector.Group result = avGroup is AndroidVector.Group ? avGroup : new AndroidVector.Group();
                            result = svgClipPathElement.ConvertClipPathAttribute(result, warnings);
                            foreach (var child in svgClipPathElement.Elements())
                            {
                                if (GeometryConverter.ConvertToPathData(child, warnings) is string pathData)
                                    result.Add(new AndroidVector.ClipPath(pathData));
                            }
                            return result;
                        }
                        warnings.Add("No element found to complete clipPath link to " + svgAttributeValue + ".");
                        return avGroup;
                    }
                    throw new Exception("could not find document root");
                }
                //result.Add(new AndroidVector.ClipPath(clipPathAttribute.Value));
                
            }
            return avGroup;
        }

    }
}
