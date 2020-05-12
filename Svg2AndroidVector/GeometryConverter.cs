using System;
using System.Collections.Generic;
using System.Xml.Linq;
using AndroidVector;

namespace Svg2AndroidVector
{
    public static class GeometryConverter
    {

        public static void AddPath(XElement svgElement, Group avGroup, List<string> warnings)
        {
            const string typeName = "path";
            if (svgElement.Name != NameSpace.Svg + typeName)
                throw new ArgumentException("argument is not the expected <"+typeName+"> SVG element");

            avGroup = ClipConverter.ConvertClipPathAttribute(svgElement, avGroup, warnings);
            var avPath = new Path();
            var attributeMap = new Dictionary<string, string>(CommonAttributes.AttributeMap)
            {
                { "d", "pathData" }
            };
            CommonAttributes.ProcessAttributes(svgElement, avPath, attributeMap, null, warnings);
            avGroup.Add(avPath);
        }

        public static void AddRect(XElement svgElement, Group avGroup, List<string> warnings)
        {
            if (ConvertToPathData(svgElement, warnings) is string pathData)
            {
                avGroup = ClipConverter.ConvertClipPathAttribute(svgElement, avGroup, warnings);
                var avPath = new Path();
                var ignoreAttributes = new List<string> { "x", "y", "width", "height", "rx", "ry" };
                CommonAttributes.ProcessAttributes(svgElement, avPath, CommonAttributes.AttributeMap, ignoreAttributes, warnings);
                avPath.SetAndroidAttributeValue("pathData", pathData);
                avGroup.Add(avPath);
            }
        }

        public static void AddCircle(XElement svgElement, Group avGroup, List<string> warnings)
        {
            if (ConvertToPathData(svgElement, warnings) is string pathData)
            {
                avGroup = ClipConverter.ConvertClipPathAttribute(svgElement, avGroup, warnings);
                var avPath = new Path();
                var ignoreAttributes = new List<string> { "cx", "cy", "r" };
                CommonAttributes.ProcessAttributes(svgElement, avPath, CommonAttributes.AttributeMap, ignoreAttributes, warnings);
                avPath.SetAndroidAttributeValue("pathData", pathData);
                avGroup.Add(avPath);
            }
        }

        public static void AddEllipse(XElement svgElement, Group avGroup, List<string> warnings)
        {
            if (ConvertSvgEllipseToPathData(svgElement, warnings) is string pathData)
            {
                avGroup = ClipConverter.ConvertClipPathAttribute(svgElement, avGroup, warnings);
                var avPath = new Path();
                var ignoreAttributes = new List<string> { "cx", "cy", "rx", "ry" };
                CommonAttributes.ProcessAttributes(svgElement, avPath, CommonAttributes.AttributeMap, ignoreAttributes, warnings);
                avPath.SetAndroidAttributeValue("pathData", pathData);
                avGroup.Add(avPath);
            }
        }

        public static void AddLine(XElement svgElement, Group avGroup, List<string> warnings)
        {
            if (ConvertSvgLineToPathData(svgElement, warnings) is string pathData)
            {
                avGroup = ClipConverter.ConvertClipPathAttribute(svgElement, avGroup, warnings);
                var avPath = new Path();
                var ignoreAttributes = new List<string> { "x1", "x2", "y1", "y2", "fill", "fill-opacity", "fill-rule" };
                CommonAttributes.ProcessAttributes(svgElement, avPath, CommonAttributes.AttributeMap, ignoreAttributes, warnings);
                avPath.SetAndroidAttributeValue("pathData", pathData);
                avGroup.Add(avPath);
            }
        }

        public static void AddPolyline(XElement svgElement, Group avGroup, List<string> warnings)
        {
            if (ConvertSvgPolylineToPathData(svgElement, warnings) is string pathData)
            {
                avGroup = ClipConverter.ConvertClipPathAttribute(svgElement, avGroup, warnings);
                var avPath = new Path();
                var ignoreAttributes = new List<string> { "points", "fill", "fill-opacity", "fill-rule" };
                CommonAttributes.ProcessAttributes(svgElement, avPath, CommonAttributes.AttributeMap, ignoreAttributes, warnings);
                avPath.SetAndroidAttributeValue("pathData", pathData);
                avGroup.Add(avPath);
            }
        }

        public static void AddPolygon(XElement svgElement, Group avGroup, List<string> warnings)
        {
            if (ConvertSvgPolygonToPathData(svgElement, warnings) is string pathData)
            {
                avGroup = ClipConverter.ConvertClipPathAttribute(svgElement, avGroup, warnings);
                var avPath = new Path();
                var ignoreAttributes = new List<string> { "points" };
                CommonAttributes.ProcessAttributes(svgElement, avPath, CommonAttributes.AttributeMap, ignoreAttributes, warnings);
                avPath.SetAndroidAttributeValue("pathData", pathData);
                avGroup.Add(avPath);
            }
        }

        public static string ConvertToPathData(XElement svgShape, List<string> warnings)
        {
            if (svgShape.Name == NameSpace.Svg + "path")
                return svgShape.Attribute("d").Value;
            if (svgShape.Name == NameSpace.Svg + "rect")
                return ConvertSvgRectToPathData(svgShape);
            if (svgShape.Name == NameSpace.Svg + "circle")
                return ConvertSvgCircleToPathData(svgShape, warnings);
            if (svgShape.Name == NameSpace.Svg + "ellipse")
                return ConvertSvgEllipseToPathData(svgShape, warnings);
            if (svgShape.Name == NameSpace.Svg + "line")
                return ConvertSvgLineToPathData(svgShape, warnings);
            if (svgShape.Name == NameSpace.Svg + "polyline")
                return ConvertSvgPolylineToPathData(svgShape, warnings);
            if (svgShape.Name == NameSpace.Svg + "polygon")
                return ConvertSvgPolygonToPathData(svgShape, warnings);
            warnings.Add("<" + svgShape?.Name + " id='" + svgShape.Attribute("id")?.Value + "'> is not recognized shape");
            return null;
        }

        public static string ConvertSvgRectToPathData(XElement svgRect)
        {
            const string typeName = "rect";
            if (svgRect.Name != NameSpace.Svg + typeName)
                throw new ArgumentException("argument is not the expected <"+typeName+"> SVG element");
            float x = 0, y = 0, w = 0, h = 0, rx = 0, ry = 0;
            if (svgRect.Attribute("x") is XAttribute xAttribute)
                AttributeExtensions.TryGetValueInPx(xAttribute, out x);
            if (svgRect.Attribute("y") is XAttribute yAttribute)
                AttributeExtensions.TryGetValueInPx(yAttribute, out y);
            if (svgRect.Attribute("width") is XAttribute wAttribute)
                AttributeExtensions.TryGetValueInPx(wAttribute, out w);
            if (svgRect.Attribute("height") is XAttribute hAttribute)
                AttributeExtensions.TryGetValueInPx(hAttribute, out h);
            if (svgRect.Attribute("rx") is XAttribute rxAttribute)
                AttributeExtensions.TryGetValueInPx(rxAttribute, out rx);
            if (svgRect.Attribute("ry") is XAttribute ryAttribute)
                AttributeExtensions.TryGetValueInPx(ryAttribute, out ry);

            if (w <= 0)
                w = 0;
            if (h <= 0)
                h = 0;
            if (w <= 0 || h <= 0 || rx <= 0 || ry <= 0)
                rx = ry = 0;

            var pathData = "M" + (x + rx) + "," + y;
            if (ry > 0)
                pathData += " a" + rx + "," + ry + " 0 0,0 " + (-rx) + "," + ry;
            pathData += " V" + (y + h - ry);
            if (rx > 0)
                pathData += " a" + rx + "," + ry + " 0 0,0 " + rx + "," + ry;
            pathData += " H" + (x + w - rx);
            if (rx > 0)
                pathData += " a" + rx + "," + ry + " 0 0,0 " + rx + "," + (-ry);
            pathData += " V" + (y + ry);
            if (rx > 0)
                pathData += " a" + rx + "," + ry + " 0 0,0 " + (-rx) + "," + (-ry);
            pathData += " Z";

            return pathData;
        }

        public static string ConvertSvgCircleToPathData(XElement svgCircle, List<string> warnings)
        {
            const string typeName = "circle";
            if (svgCircle.Name != NameSpace.Svg + typeName)
                throw new ArgumentException("argument is not the expected <"+typeName+"> SVG element");

            float cx = 0, cy = 0, r = 0;
            if (svgCircle.Attribute("cx") is XAttribute xAttribute)
                AttributeExtensions.TryGetValueInPx(xAttribute, out cx);
            if (svgCircle.Attribute("cy") is XAttribute yAttribute)
                AttributeExtensions.TryGetValueInPx(yAttribute, out cy);
            if (svgCircle.Attribute("r") is XAttribute rAttribute)
                AttributeExtensions.TryGetValueInPx(rAttribute, out r);

            if (r <= 0)
            {
                warnings.Add("ignoring SVG element because radius [" + r + "] for <"+typeName+" id=\"" + svgCircle.Attribute("id")?.Value + "\" <= 0");
                return null;
            }

            var pathData = "M" + (cx - r) + "," + cy;
            pathData += " A" + r + "," + r + " 0 0,0 " + (cx + r) + "," + cy;
            pathData += " A" + r + "," + r + " 0 0,0 " + (cx - r) + "," + cy;
            pathData += " Z";

            return pathData;
        }

        public static string ConvertSvgEllipseToPathData(XElement svgEllipse, List<string> warnings)
        {
            const string typeName = "circle";
            if (svgEllipse.Name != NameSpace.Svg + typeName)
                throw new ArgumentException("argument is not the expected <" + typeName + "> SVG element");

            float cx = 0, cy = 0, rx = 0, ry = 0;
            if (svgEllipse.Attribute("cx") is XAttribute xAttribute)
                AttributeExtensions.TryGetValueInPx(xAttribute, out cx);
            if (svgEllipse.Attribute("cy") is XAttribute yAttribute)
                AttributeExtensions.TryGetValueInPx(yAttribute, out cy);
            if (svgEllipse.Attribute("rx") is XAttribute rxAttribute)
                AttributeExtensions.TryGetValueInPx(rxAttribute, out rx);
            if (svgEllipse.Attribute("ry") is XAttribute ryAttribute)
                AttributeExtensions.TryGetValueInPx(ryAttribute, out ry);

            if (rx <= 0)
            {
                warnings.Add("ignoring SVG element because x radius [" + rx + "] for <"+typeName+" id=\"" + svgEllipse.Attribute("id")?.Value + "\" <= 0");
                return null;
            }
            if (ry <= 0)
            {
                warnings.Add("ignoring SVG element because y radius [" + ry + "] for <" + typeName + " id=\"" + svgEllipse.Attribute("id")?.Value + "\" <= 0");
                return null;
            }

            var pathData = "M" + (cx - rx) + "," + cy;
            pathData += " A" + rx + "," + ry + " 0 0,0 " + (cx + rx) + "," + cy;
            pathData += " A" + rx + "," + ry + " 0 0,0 " + (cx - rx) + "," + cy;
            pathData += " Z";
            return pathData;
        }

        public static string ConvertSvgLineToPathData(XElement svgLine, List<string> warnings)
        {
            const string typeName = "line";
            if (svgLine.Name != NameSpace.Svg + typeName)
                throw new ArgumentException("argument is not the expected <" + typeName + "> SVG element");

            float x1 = 0, y1 = 0, x2 = 0, y2 = 0;
            if (svgLine.Attribute("x1") is XAttribute x1Attribute)
                AttributeExtensions.TryGetValueInPx(x1Attribute, out x1);
            if (svgLine.Attribute("y1") is XAttribute y1Attribute)
                AttributeExtensions.TryGetValueInPx(y1Attribute, out y1);
            if (svgLine.Attribute("x2") is XAttribute x2Attribute)
                AttributeExtensions.TryGetValueInPx(x2Attribute, out x2);
            if (svgLine.Attribute("y2") is XAttribute y2Attribute)
                AttributeExtensions.TryGetValueInPx(y2Attribute, out y2);

            if (x1 == x2 && y1 == y2)
            {
                warnings.Add("ignoring SVG element because start ["+x1+","+y1+ "] and end [" + x2 + "," + y2 + "] points are the same for <" + typeName + " id=\"" + svgLine.Attribute("id")?.Value + "\".");
                return null;
            }

            var pathData = "M" + x1 + "," + y1;
            pathData += " L" + x2 + "," + y2;
            return pathData;
        }

        public static string ConvertSvgPolylineToPathData(XElement svgPolyline, List<string> warnings)
        {
            const string typeName = "polyline";
            if (svgPolyline.Name != NameSpace.Svg + typeName)
                throw new ArgumentException("argument is not the expected <" + typeName + "> SVG element");

            if (svgPolyline.Attribute("points") is XAttribute pointsAttribute)
            {
                var points = pointsAttribute.Value.Split(new char[] { ' ', ',' });
                if (points.Length < 4)
                {
                    warnings.Add("ignoring <" + typeName + " id=\"" + svgPolyline.Attribute("id")?.Value + "\" because only ["+points.Length+"] coordinate values were found.");
                    return null;
                }
                if (points.Length % 2 == 0)
                    warnings.Add("ignoring last point in points attribute for <" + typeName + " id=\"" + svgPolyline.Attribute("id")?.Value + "\" because it is missing Y coordinate.");

                var pathData = "M" + points[0] +"," + points[1];
                for (int i=4;i <= points.Length; i+=2)
                    pathData += " L" + points[i-2] + "," + points[i-1];

                return pathData;
            }
            else
                warnings.Add("ignoring SVG element because no points attribute found for <" + typeName + " id=\"" + svgPolyline.Attribute("id")?.Value + "\".");
            return null;
        }

        public static string ConvertSvgPolygonToPathData(XElement svgPolygon, List<string> warnings)
        {
            const string typeName = "polygon";
            if (svgPolygon.Name != NameSpace.Svg + typeName)
                throw new ArgumentException("argument is not the expected <" + typeName + "> SVG element");

            if (svgPolygon.Attribute("points") is XAttribute pointsAttribute)
            {
                var points = pointsAttribute.Value.Split(new char[] { ' ', ',' });
                if (points.Length < 4)
                {
                    warnings.Add("ignoring <" + typeName + " id=\"" + svgPolygon.Attribute("id")?.Value + "\" because only [" + points.Length + "] coordinate values were found.");
                    return null;
                }
                if (points.Length % 2 == 0)
                    warnings.Add("ignoring last point in points attribute for <" + typeName + " id=\"" + svgPolygon.Attribute("id")?.Value + "\" because it is missing Y coordinate.");

                var pathData = "M" + points[0] + "," + points[1];
                for (int i = 4; i <= points.Length; i += 2)
                    pathData += " L" + points[i-2] + "," + points[i - 1];
                pathData += " Z";
                return pathData;
            }
                warnings.Add("ignoring SVG element because no points attribute found for <" + typeName + " id=\"" + svgPolygon.Attribute("id")?.Value + "\".");
            return null;
        }
    }
}
