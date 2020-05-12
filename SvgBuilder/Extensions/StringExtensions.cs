using System;
using System.Collections.Generic;

namespace SvgBuilder
{
    public static class StringExtensions
    {
        public static string ToCamelCase(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return s;
            return (char.ToLower(s[0]) + s.Substring(1)).Replace('_','-');
        }

        public static string ToPascalCase(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return s;
            return (char.ToUpper(s[0]) + s.Substring(1)).Replace('-','_');
        }

        public static List<BasePathCommand> ToPathCommands(this string s)
        {
            s = s.Trim();
            var strings = new List<string>();
            int start = 0;
            for (int i=1;i <= s.Length;i++)
            {
                if (i== s.Length || char.IsLetter(s[i]))
                {
                    strings.Add(s[start..(i-1)]);
                    start = i;
                }
            }

            var result = new List<BasePathCommand>();
            foreach (var str in strings)
            {
                var cmd = char.ToUpper(str[0]);
                switch(cmd)
                {
                    case Arc.Symbol:
                        if (Arc.FromArguments(str) is Arc arc)
                            result.Add(arc);
                        else
                            throw new Exception("Unrecognized Path Command : " + str);
                        break;
                    case BezierCurve.Symbol:
                        if (BezierCurve.FromArguments(str) is BezierCurve bezierCurve)
                            result.Add(bezierCurve);
                        else
                            throw new Exception("Unrecognized Path Command : " + str);
                        break;
                    case ClosePath.Symbol:
                        result.Add(new ClosePath());
                        break;
                    case HorzLineTo.Symbol:
                        if (HorzLineTo.FromArguments(str) is HorzLineTo horzLineTo)
                            result.Add(horzLineTo);
                        else
                            throw new Exception("Unrecognized Path Command : " + str);
                        break;
                    case LineTo.Symbol:
                        if (LineTo.FromArguments(str) is LineTo lineTo)
                            result.Add(lineTo);
                        else
                            throw new Exception("Unrecognized Path Command : " + str);
                        break;
                    case MoveTo.Symbol:
                        if (MoveTo.FromArguments(str) is MoveTo moveTo)
                            result.Add(moveTo);
                        else
                            throw new Exception("Unrecognized Path Command : " + str);
                        break;
                    case QuadraticCurve.Symbol:
                        if (QuadraticCurve.FromArguments(str) is QuadraticCurve quadraticCurve)
                            result.Add(quadraticCurve);
                        else
                            throw new Exception("Unrecognized Path Command : " + str);
                        break;
                    case ShortcutBezierCurve.Symbol:
                        if (ShortcutBezierCurve.FromArguments(str) is ShortcutBezierCurve shortcutBezierCurve)
                            result.Add(shortcutBezierCurve);
                        else
                            throw new Exception("Unrecognized Path Command : " + str);
                        break;
                    case ShortcutQuadraticCurve.Symbol:
                        if (ShortcutQuadraticCurve.FromArguments(str) is ShortcutQuadraticCurve shortcutQuadraticCurve)
                            result.Add(shortcutQuadraticCurve);
                        else
                            throw new Exception("Unrecognized Path Command : " + str);
                        break;
                    case VertLineTo.Symbol:
                        if (VertLineTo.FromArguments(str) is VertLineTo vertLineTo)
                            result.Add(vertLineTo);
                        else
                            throw new Exception("Unrecognized Path Command : " + str);
                        break;
                    default:
                        throw new Exception("Unrecognized Path Command : " + str);
                }
            }
            return result;
        }
    }
}
