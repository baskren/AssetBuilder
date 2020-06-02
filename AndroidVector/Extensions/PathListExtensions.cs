using System;
using System.Collections.Generic;
using System.Drawing;
using AndroidVector.PathElement;

namespace AndroidVector.Extensions
{
    public static class PathListExtensions
    {

        public static List<Base> ToTransformablePathlist(this List<Base> pathList)
        {
            var cursor = SizeF.Empty;
            var newList = new List<Base>();
            SizeF start = SizeF.Empty;
            bool started = false;
            foreach (var element in pathList.ToArray())
            {
                if (element is HorzLineTo horz)
                {
                    var lineTo = horz.ToLine(cursor);
                    if (lineTo.End.X != cursor.Width || lineTo.End.Y != cursor.Height)
                    {
                        newList.Add(lineTo);
                        cursor = new SizeF(lineTo.End);
                    }
                }
                else if (element is VertLineTo vert)
                {
                    var lineTo = vert.ToLine(cursor);
                    if (lineTo.End.X != cursor.Width || lineTo.End.Y != cursor.Height)
                    {
                        newList.Add(lineTo);
                        cursor = new SizeF(lineTo.End);
                    }
                }
                else if (element is ClosePath close)
                {
                    cursor = close.ToAbsolute(start);
                    newList.Add(close);
                    started = false;
                }
                else
                {
                    var newCursor = element.ToAbsolute(cursor);
                    if (newCursor != cursor || !started)
                    {
                        if (!started)
                        {
                            start = newCursor;
                            started = true;
                        }
                        if (element is Arc arc)
                            newList.AddRange(arc.ToSmallArcs(cursor));
                        //else if (element is QuadraticCurve quadratic)
                        //    newList.Add(quadratic.ToBezierCurve(cursor));
                        else
                            newList.Add(element);
                        cursor = newCursor;
                    }
                }
            }
            return newList;
        }

        public static string ToPathDataText(this List<Base> pathList)
        {
            var result = string.Join(" ", pathList);
            return result;
        }

        public static List<Base> ToPathList(this string pathDataText)
        {
            pathDataText = pathDataText.Trim();
            var strings = new List<string>();
            int start = 0;
            for (int i = 1; i <= pathDataText.Length; i++)
            {
                if (i == pathDataText.Length || (char.ToLower(pathDataText[i])!='e' && char.IsLetter(pathDataText[i])))
                {
                    strings.Add(pathDataText.Substring(start, i - start));
                    start = i;
                }
            }

            var result = new List<PathElement.Base>();
            foreach (var str in strings)
            {
                if (string.IsNullOrWhiteSpace(str))
                    continue;
                var cmd = char.ToUpper(str[0]);
                switch (cmd)
                {
                    case Arc.Symbol:
                        if (Arc.FromString(str) is List<Arc> arcList)
                            result.AddRange(arcList);
                        break;
                    case BezierCurve.Symbol:
                        if (BezierCurve.FromString(str) is List<BezierCurve> bezierCurveList)
                            result.AddRange(bezierCurveList);
                        break;
                    case ClosePath.Symbol:
                        result.Add(new ClosePath());
                        break;
                    case HorzLineTo.Symbol:
                        if (HorzLineTo.FromString(str) is HorzLineTo horzLineTo)
                            result.Add(horzLineTo);
                        break;
                    case LineTo.Symbol:
                        if (LineTo.FromString(str) is List<LineTo> lineList)
                            result.AddRange(lineList);
                        break;
                    case MoveTo.Symbol:
                        if (MoveTo.FromString(str) is List<Base> moveAndLineList)
                            result.AddRange(moveAndLineList);
                        break;
                    case QuadraticCurve.Symbol:
                        if (QuadraticCurve.FromString(str) is List<QuadraticCurve> quadraticCurveList)
                            result.AddRange(quadraticCurveList);
                        break;
                    case ShortcutBezierCurve.Symbol:
                        if (ShortcutBezierCurve.FromString(str) is List<ShortcutBezierCurve> shortcutBezierCurveList)
                            result.AddRange(shortcutBezierCurveList);
                        break;
                    case ShortcutQuadraticCurve.Symbol:
                        if (ShortcutQuadraticCurve.FromString(str) is List<ShortcutQuadraticCurve> shortcutQuadraticCurveList)
                            result.AddRange(shortcutQuadraticCurveList);
                        break;
                    case VertLineTo.Symbol:
                        if (VertLineTo.FromString(str) is VertLineTo vertLineTo)
                            result.Add(vertLineTo);
                        break;
                    default:
                        throw new Exception("Unrecognized Path Command : " + str);
                }
            }
            return result;

        }
    }
}
