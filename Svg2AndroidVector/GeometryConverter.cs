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
            if (svgElement.Name != Namespace.Svg + typeName)
                throw new ArgumentException("Argument is not the expected <"+typeName+"> SVG element");

            var clipGroup = ClipConverter.ConvertClipPathAttribute(svgElement, avGroup, warnings);
            var avPath = new Path();
            CommonAttributes.ProcessAttributes(svgElement, avPath, null, warnings);

            if (clipGroup != avGroup)
            {
                CommonAttributes.SetTransforms(svgElement, clipGroup, warnings);
                clipGroup.Add(avPath);
            }
            else
            {
                CommonAttributes.SetTransforms(svgElement, avPath, warnings);
                avGroup.Add(avPath);
            }
        }

        public static void AddRect(XElement svgElement, Group avGroup, List<string> warnings)
        {
            if (ConvertElementToPathData(svgElement, warnings) is string pathData)
            {
                var clipGroup = ClipConverter.ConvertClipPathAttribute(svgElement, avGroup, warnings);
                var avPath = new Path();
                var ignoreAttributes = new List<string> { "x", "y", "width", "height", "rx", "ry" };
                CommonAttributes.ProcessAttributes(svgElement, avPath,ignoreAttributes, warnings);
                avPath.SetAndroidAttributeValue("pathData", pathData);

                if (clipGroup != avGroup)
                {
                    CommonAttributes.SetTransforms(svgElement, clipGroup, warnings);
                    clipGroup.Add(avPath);
                }
                else
                {
                    CommonAttributes.SetTransforms(svgElement, avPath, warnings);
                    avGroup.Add(avPath);
                }
            }
        }

        public static void AddCircle(XElement svgElement, Group avGroup, List<string> warnings)
        {
            if (ConvertElementToPathData(svgElement, warnings) is string pathData)
            {
                var clipGroup = ClipConverter.ConvertClipPathAttribute(svgElement, avGroup, warnings);
                var avPath = new Path();
                var ignoreAttributes = new List<string> { "cx", "cy", "r" };
                CommonAttributes.ProcessAttributes(svgElement, avPath,ignoreAttributes, warnings);
                avPath.SetAndroidAttributeValue("pathData", pathData);

                if (clipGroup != avGroup)
                {
                    CommonAttributes.SetTransforms(svgElement, clipGroup, warnings);
                    clipGroup.Add(avPath);
                }
                else
                {
                    CommonAttributes.SetTransforms(svgElement, avPath, warnings);
                    avGroup.Add(avPath);
                }
            }
        }

        public static void AddEllipse(XElement svgElement, Group avGroup, List<string> warnings)
        {
            if (ConvertEllipseElementToPathData(svgElement, warnings) is string pathData)
            {
                var clipGroup = ClipConverter.ConvertClipPathAttribute(svgElement, avGroup, warnings);
                var avPath = new Path();
                var ignoreAttributes = new List<string> { "cx", "cy", "rx", "ry" };
                CommonAttributes.ProcessAttributes(svgElement, avPath,ignoreAttributes, warnings);
                avPath.SetAndroidAttributeValue("pathData", pathData);

                if (clipGroup != avGroup)
                {
                    CommonAttributes.SetTransforms(svgElement, clipGroup, warnings);
                    clipGroup.Add(avPath);
                }
                else
                {
                    CommonAttributes.SetTransforms(svgElement, avPath, warnings);
                    avGroup.Add(avPath);
                }
            }
        }

        public static void AddLine(XElement svgElement, Group avGroup, List<string> warnings)
        {
            if (ConvertLineElementToPathData(svgElement, warnings) is string pathData)
            {
                var clipGroup = ClipConverter.ConvertClipPathAttribute(svgElement, avGroup, warnings);
                var avPath = new Path();
                var ignoreAttributes = new List<string> { "x1", "x2", "y1", "y2", "fill", "fill-opacity", "fill-rule" };
                CommonAttributes.ProcessAttributes(svgElement, avPath,ignoreAttributes, warnings);
                avPath.SetAndroidAttributeValue("pathData", pathData);

                if (clipGroup != avGroup)
                {
                    CommonAttributes.SetTransforms(svgElement, clipGroup, warnings);
                    clipGroup.Add(avPath);
                }
                else
                {
                    CommonAttributes.SetTransforms(svgElement, avPath, warnings);
                    avGroup.Add(avPath);
                }
            }
        }

        public static void AddPolyline(XElement svgElement, Group avGroup, List<string> warnings)
        {
            if (ConvertPolylineElementToPathData(svgElement, warnings) is string pathData)
            {
                var clipGroup = ClipConverter.ConvertClipPathAttribute(svgElement, avGroup, warnings);
                var avPath = new Path();
                var ignoreAttributes = new List<string> { "points", "fill", "fill-opacity", "fill-rule" };
                CommonAttributes.ProcessAttributes(svgElement, avPath,ignoreAttributes, warnings);
                avPath.SetAndroidAttributeValue("pathData", pathData);

                if (clipGroup != avGroup)
                {
                    CommonAttributes.SetTransforms(svgElement, clipGroup, warnings);
                    clipGroup.Add(avPath);
                }
                else
                {
                    CommonAttributes.SetTransforms(svgElement, avPath, warnings);
                    avGroup.Add(avPath);
                }
            }
        }

        public static void AddPolygon(XElement svgElement, Group avGroup, List<string> warnings)
        {
            if (ConvertPolygonElementToPathData(svgElement, warnings) is string pathData)
            {
                var clipGroup = ClipConverter.ConvertClipPathAttribute(svgElement, avGroup, warnings);
                var avPath = new Path();
                var ignoreAttributes = new List<string> { "points" };
                CommonAttributes.ProcessAttributes(svgElement, avPath, ignoreAttributes, warnings);
                avPath.SetAndroidAttributeValue("pathData", pathData);

                if (clipGroup != avGroup)
                {
                    CommonAttributes.SetTransforms(svgElement, clipGroup, warnings);
                    clipGroup.Add(avPath);
                }
                else
                {
                    CommonAttributes.SetTransforms(svgElement, avPath, warnings);
                    avGroup.Add(avPath);
                }
            }
        }

        public static string ConvertElementToPathData(XElement svgShape, List<string> warnings)
        {
            if (svgShape.Name == Namespace.Svg + "path")
                return svgShape.Attribute("d").Value;
            if (svgShape.Name == Namespace.Svg + "rect")
                return ConvertRectElementToPathData(svgShape);
            if (svgShape.Name == Namespace.Svg + "circle")
                return ConvertCircleElementToPathData(svgShape, warnings);
            if (svgShape.Name == Namespace.Svg + "ellipse")
                return ConvertEllipseElementToPathData(svgShape, warnings);
            if (svgShape.Name == Namespace.Svg + "line")
                return ConvertLineElementToPathData(svgShape, warnings);
            if (svgShape.Name == Namespace.Svg + "polyline")
                return ConvertPolylineElementToPathData(svgShape, warnings);
            if (svgShape.Name == Namespace.Svg + "polygon")
                return ConvertPolygonElementToPathData(svgShape, warnings);
            warnings.AddWarning("Ignoring <" + svgShape?.Name + " id='" + svgShape.Attribute("id")?.Value + "'> because is not recognized shape");
            return null;
        }

        public static string ConvertCssShapeToPathData(XElement element, string cssShapeText, List<string> warnings)
        {
            if (string.IsNullOrWhiteSpace(cssShapeText))
                return null;
            cssShapeText = cssShapeText.Trim().Trim(')');
            var parts = cssShapeText.Split('(');
            if (parts.Length <  1)
            {
                warnings.AddWarning("Ignoring CssShape '"+cssShapeText+"' <" + element?.Name + " id='" + element.Attribute("id")?.Value + "'> because is not recognized shape");
                return null;
            }
            var cmd = parts[0];
            switch (cmd)
            {
                case "inset":
                    {
                        var values = ElementExtensions.ToFloatList(element, (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[1])) ? "0% 100% 100% 0%" : parts[1]);
                        if (values.Count == 4)
                            return RectToPathData(values[3], values[0], values[1] - values[3], values[2] - values[0]);
                        else if (values.Count == 5)
                            return RectToPathData(values[3], values[0], values[1] - values[3], values[2] - values[0], values[4], values[4]);
                        else
                        {
                            warnings.AddWarning("Ignoring CssShape '" + cssShapeText + "' in <" + element?.Name + " id='" + element.Attribute("id")?.Value + "'> because it doesn't have the right number of arguments.");
                            return null;
                        }
                    }
                case "circle":
                    {
                        var radius = ElementExtensions.ToFloatList(element, "100%")[0];
                        var center = ElementExtensions.ToFloatList(element, "50% 50%");
                        if (parts.Length > 1)
                        {
                            parts = parts[1].Split("at");
                            if (ElementExtensions.ToFloatList(element, parts[1]) is List<float> radiusValues && radiusValues.Count > 0)
                                radius = radiusValues[0];
                            else
                                warnings.Add("Could not parse CSS circle radius in '" + cssShapeText + "' in in <" + element?.Name + " id='" + element.Attribute("id")?.Value + "'>.");
                            if (parts.Length > 1)
                            {
                                if (ElementExtensions.ToFloatList(element, parts[2]) is List<float> centerValues && centerValues.Count > 0)
                                    center = new List<float> { centerValues[0], centerValues.Count > 1 ? centerValues[1] : centerValues[0] };
                                else
                                    warnings.Add("Could not parse CSS circle center in '" + cssShapeText + "' in in <" + element?.Name + " id='" + element.Attribute("id")?.Value + "'>.");
                            }
                        }
                        return CirclePathData(center[0], center[1], radius);
                    }
                case "ellipse":
                    {
                        var radius = ElementExtensions.ToFloatList(element, "100% 100%");
                        var center = ElementExtensions.ToFloatList(element, "50% 50%");
                        if (parts.Length > 1)
                        {
                            parts = parts[1].Split("at");
                            if (ElementExtensions.ToFloatList(element, parts[1]) is List<float> radiusValues && radiusValues.Count > 0)
                                radius = new List<float> { radiusValues[0], radiusValues.Count > 1 ? radiusValues[1] : radiusValues[0] };
                            else
                                warnings.Add("Could not parse CSS circle radius in '" + cssShapeText + "' in in <" + element?.Name + " id='" + element.Attribute("id")?.Value + "'>.");
                            if (parts.Length > 1)
                            {
                                if (ElementExtensions.ToFloatList(element, parts[2]) is List<float> centerValues && centerValues.Count > 0)
                                    center = new List<float> { centerValues[0], centerValues.Count > 1 ? centerValues[1] : centerValues[0] };
                                else
                                    warnings.Add("Could not parse CSS circle center in '" + cssShapeText + "' in in <" + element?.Name + " id='" + element.Attribute("id")?.Value + "'>.");
                            }
                        }
                        return EllipsePathData(center[0], center[1], radius[0], radius[1]);
                    }
                case "polygon":
                    {
                        if (parts.Length > 1)
                        {
                            var points = ElementExtensions.ToFloatList(element, parts[1]);
                            if (points.Count > 2)
                                return PolygonToPathData(points);
                        }
                        warnings.AddWarning("Ignoring CssShape '" + cssShapeText + "' in <" + element?.Name + " id='" + element.Attribute("id")?.Value + "'> because it doesn't have the right number of arguments.");
                        return null;
                    }
            }
            return null;
        }

        public static string ConvertRectElementToPathData(XElement svgRect)
        {
            const string typeName = "rect";
            if (svgRect.Name != Namespace.Svg + typeName)
                throw new ArgumentException("Argument is not the expected <"+typeName+"> SVG element");
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
            //if (w <= 0 || h <= 0 || rx <= 0 || ry <= 0)
            //    rx = ry = 0;

            return RectToPathData(x,y,w,h,rx,ry);
        }


        public static string RectToPathData(float x, float y, float w, float h, float rx = 0, float ry = 0)
        {
            if (rx != 0 && ry == 0)
                ry = rx;
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

        public static string ConvertCircleElementToPathData(XElement svgCircle, List<string> warnings)
        {
            const string typeName = "circle";
            if (svgCircle.Name != Namespace.Svg + typeName)
                throw new ArgumentException("Argument is not the expected <"+typeName+"> SVG element");

            float cx = 0, cy = 0, r = 0;
            if (svgCircle.Attribute("cx") is XAttribute xAttribute)
                AttributeExtensions.TryGetValueInPx(xAttribute, out cx);
            if (svgCircle.Attribute("cy") is XAttribute yAttribute)
                AttributeExtensions.TryGetValueInPx(yAttribute, out cy);
            if (svgCircle.Attribute("r") is XAttribute rAttribute)
                AttributeExtensions.TryGetValueInPx(rAttribute, out r);

            if (r <= 0)
            {
                warnings.AddWarning("Ignoring SVG element because radius [" + r + "] for <"+typeName+" id=\"" + svgCircle.Attribute("id")?.Value + "\" <= 0");
                return null;
            }

            return CirclePathData(cx, cy, r);
        }

        public static string CirclePathData(float cx, float cy, float r)
        {
            var pathData = "M" + (cx - r) + "," + cy;
            pathData += " A" + r + "," + r + " 0 0 0 " + (cx + r) + "," + cy;
            pathData += " A" + r + "," + r + " 0 0 0 " + (cx - r) + "," + cy;
            pathData += " Z";
            return pathData;
        }

        public static string ConvertEllipseElementToPathData(XElement svgEllipse, List<string> warnings)
        {
            const string typeName = "ellipse";
            if (svgEllipse.Name != Namespace.Svg + typeName)
                throw new ArgumentException("Argument is not the expected <" + typeName + "> SVG element");

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
                warnings.AddWarning("Ignoring SVG element because x radius [" + rx + "] for <"+typeName+" id=\"" + svgEllipse.Attribute("id")?.Value + "\" <= 0");
                return null;
            }
            if (ry <= 0)
            {
                warnings.AddWarning("Ignoring SVG element because y radius [" + ry + "] for <" + typeName + " id=\"" + svgEllipse.Attribute("id")?.Value + "\" <= 0");
                return null;
            }
            return EllipsePathData(cx, cy, rx, ry);
        }

        public static string EllipsePathData(float cx, float cy, float rx, float ry)
        {
            var pathData = "M" + (cx - rx) + "," + cy;
            pathData += " A" + rx + "," + ry + " 0 0,0 " + (cx + rx) + "," + cy;
            pathData += " A" + rx + "," + ry + " 0 0,0 " + (cx - rx) + "," + cy;
            pathData += " Z";
            return pathData;
        }

        public static string ConvertLineElementToPathData(XElement svgLine, List<string> warnings)
        {
            const string typeName = "line";
            if (svgLine.Name != Namespace.Svg + typeName)
                throw new ArgumentException("Argument is not the expected <" + typeName + "> SVG element");

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
                warnings.AddWarning("Ignoring SVG element because start ["+x1+","+y1+ "] and end [" + x2 + "," + y2 + "] points are the same for <" + typeName + " id=\"" + svgLine.Attribute("id")?.Value + "\".");
                return null;
            }
            return LineToPathData(x1, y1, x2, y2);
        }

        public static string LineToPathData(float x1, float y1, float x2, float y2)
        {
            var pathData = "M" + x1 + "," + y1;
            pathData += " L" + x2 + "," + y2;
            return pathData;
        }

        public static string ConvertPolylineElementToPathData(XElement svgPolyline, List<string> warnings)
        {
            const string typeName = "polyline";
            if (svgPolyline.Name != Namespace.Svg + typeName)
                throw new ArgumentException("argument is not the expected <" + typeName + "> SVG element");

            if (svgPolyline.Attribute("points") is XAttribute pointsAttribute)
            {
                //var points = pointsAttribute.Value.Split(new char[] { ' ', ',' });
                var points = pointsAttribute.Value.ToFloatList();

                if (points.Count < 4)
                {
                    warnings.AddWarning("Ignoring <" + typeName + " id=\"" + svgPolyline.Attribute("id")?.Value + "\" because only ["+points.Count + "] coordinate values were found.");
                    return null;
                }
                if (points.Count % 2 != 0)
                    warnings.AddWarning("Ignoring last point in points attribute for <" + typeName + " id=\"" + svgPolyline.Attribute("id")?.Value + "\" because it is missing Y coordinate.");

                return PolylineToPathData(points);
            }
            else
                warnings.AddWarning("Ignoring SVG element because no points attribute found for <" + typeName + " id=\"" + svgPolyline.Attribute("id")?.Value + "\".");
            return null;
        }

        public static string PolylineToPathData(List<float> points)
        {
            var pathData = "M" + points[0] + "," + points[1];
            for (int i = 4; i <= points.Count; i += 2)
                pathData += " L" + points[i - 2] + "," + points[i - 1];
            return pathData;
        }

        public static string ConvertPolygonElementToPathData(XElement svgPolygon, List<string> warnings)
        {
            const string typeName = "polygon";
            if (svgPolygon.Name != Namespace.Svg + typeName)
                throw new ArgumentException("Argument is not the expected <" + typeName + "> SVG element");

            if (svgPolygon.Attribute("points") is XAttribute pointsAttribute && !string.IsNullOrWhiteSpace(pointsAttribute.Value))
            {
                var points = pointsAttribute.Value.ToFloatList();
                
                //var points = pointsAttribute.Value.Split(new char[] { ' ', ',' });
                if (points.Count < 4)
                {
                    warnings.AddWarning("Ignoring <" + typeName + " id=\"" + svgPolygon.Attribute("id")?.Value + "\" because only [" + points.Count + "] coordinate values were found.");
                    return null;
                }
                if (points.Count % 2 != 0)
                    warnings.AddWarning("Ignoring last point in points attribute for <" + typeName + " id=\"" + svgPolygon.Attribute("id")?.Value + "\" because it is missing Y coordinate.");

                return PolygonToPathData(points);
                
            }
                warnings.AddWarning("Ignoring SVG element because no points attribute found for <" + typeName + " id=\"" + svgPolygon.Attribute("id")?.Value + "\".");
            return null;
        }

        public static string PolygonToPathData(List<float> points)
        {
            var pathData = "M" + points[0] + "," + points[1];
            for (int i = 4; i <= points.Count; i += 2)
                pathData += " L" + points[i - 2] + "," + points[i - 1];
            pathData += " Z";
            return pathData;
        }
    }
}
