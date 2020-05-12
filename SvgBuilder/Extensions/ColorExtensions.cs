using System;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using SvgBuilder.Paint;

namespace SvgBuilder
{
    public static class ColorExtensions
    {
        public static System.Drawing.Color ToSystemDrawingColor(this SvgBuilder.Paint.Color svgColor)
            => svgColor.Value.ToColor();

        public static SvgBuilder.Paint.Color ToSvgPaintColor(this System.Drawing.Color color)
            => SvgBuilder.Paint.Color.FromArguments("#"+color.R.ToString("X2")+color.G.ToString("X2")+color.B.ToString("X2"));
        

        public static System.Drawing.Color ToColor(this string s)
        {
            if (s.StartsWith("#"))
                return FromHexString(s);

            var rg = new Regex("[rgbRGB]{3}");
            if (rg.Match(s).Success)
                return FromRGBString(s);

            return default;
        }

        static System.Drawing.Color FromHexString(string s)
        {
            int r, g, b;
            s = s.Substring(1);

            if (s.Length == 3)
            {
                r = int.Parse(s.Substring(0, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                g = int.Parse(s.Substring(1, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                b = int.Parse(s.Substring(2, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                r += r * 16;
                g += g * 16;
                b += b * 16;
                return System.Drawing.Color.FromArgb(r, g, b);
            }
            else if (s.Length == 6)
            {
                r = int.Parse(s.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                g = int.Parse(s.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                b = int.Parse(s.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                return System.Drawing.Color.FromArgb(r, g, b);
            }
            else
            {
                throw new ArgumentException("Invalid SvgColor", s);
            }
        }

        static System.Drawing.Color FromRGBString(string s)
        {
#pragma warning disable CC0021 // Use nameof
            int r, g, b;
            var rg = new Regex(@"[rgbRGB ]+\( *(?<r>\d+)[, ]+(?<g>\d+)[, ]+(?<b>\d+) *\)");
            var m = rg.Match(s);
            if (m.Success)
            {
                r = int.Parse(m.Groups["r"].Captures[0].Value, CultureInfo.InvariantCulture);
                g = int.Parse(m.Groups["g"].Captures[0].Value, CultureInfo.InvariantCulture);
                b = int.Parse(m.Groups["b"].Captures[0].Value, CultureInfo.InvariantCulture);

                return System.Drawing.Color.FromArgb(r, g, b);
            }

            rg = new Regex(@"[rgbRGB ]+\( *(?<r>\d+)%[, ]+(?<g>\d+)%[, ]+(?<b>\d+)% *\)");
            m = rg.Match(s);
            if (m.Success)
            {
                r = int.Parse(m.Groups["r"].Captures[0].Value, CultureInfo.InvariantCulture) * 255 / 100;
                g = int.Parse(m.Groups["g"].Captures[0].Value, CultureInfo.InvariantCulture) * 255 / 100;
                b = int.Parse(m.Groups["b"].Captures[0].Value, CultureInfo.InvariantCulture) * 255 / 100;

                return System.Drawing.Color.FromArgb(r, g, b);
            }
#pragma warning restore CC0021 // Use nameof

            throw new ArgumentException("Invalid SvgColor", s);
        }


    }
}
