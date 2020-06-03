using System;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using PdfSharpCore.Drawing;

namespace AndroidVector
{
    public static class ColorExtensions
    {
        public static string ToHexString(this Color c) => $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}";

        public static Color ToColor(this string s)
        {
            if (s.StartsWith("#"))
                return FromHexString(s);

            var rg = new Regex("[rgbRGB]{3}");
            if (rg.Match(s).Success)
                return FromRGBString(s);

            return default;
        }

        public static Color FromHexString(this string s)
        {
            int a, r, g, b;
            if(s.StartsWith("#"))
                s = s.Substring(1);

            if (s.Length == 3)
            {
                r = int.Parse(s.Substring(0, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                g = int.Parse(s.Substring(1, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                b = int.Parse(s.Substring(2, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                r += r * 16;
                g += g * 16;
                b += b * 16;
                return Color.FromArgb(r, g, b);
            }
            else if (s.Length == 6)
            {
                r = int.Parse(s.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                g = int.Parse(s.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                b = int.Parse(s.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                return Color.FromArgb(r, g, b);
            }
            else if (s.Length == 4)
            {
                a = int.Parse(s.Substring(0, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                r = int.Parse(s.Substring(1, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                g = int.Parse(s.Substring(2, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                b = int.Parse(s.Substring(3, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                a += a * 16;
                r += r * 16;
                g += g * 16;
                b += b * 16;
                return Color.FromArgb(a, r, g, b);
            }
            else if (s.Length == 8)
            {
                a = int.Parse(s.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                r = int.Parse(s.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                g = int.Parse(s.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                b = int.Parse(s.Substring(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                return Color.FromArgb(a, r, g, b);
            }
            else
                throw new ArgumentException($"Invalid hex color string [{s}]");
        }

        public static Color FromRGBString(this string s)
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

                return Color.FromArgb(r, g, b);
            }

            rg = new Regex(@"[rgbRGB ]+\( *(?<r>\d+)%[, ]+(?<g>\d+)%[, ]+(?<b>\d+)% *\)");
            m = rg.Match(s);
            if (m.Success)
            {
                r = int.Parse(m.Groups["r"].Captures[0].Value, CultureInfo.InvariantCulture) * 255 / 100;
                g = int.Parse(m.Groups["g"].Captures[0].Value, CultureInfo.InvariantCulture) * 255 / 100;
                b = int.Parse(m.Groups["b"].Captures[0].Value, CultureInfo.InvariantCulture) * 255 / 100;

                return Color.FromArgb(r, g, b);
            }
#pragma warning restore CC0021 // Use nameof

            throw new ArgumentException("Invalid SvgColor", s);
        }

        public static XColor ToXColor(this Color c)
            => XColor.FromArgb(c.ToArgb());

        public static SkiaSharp.SKColor ToSKColor(this Color c)
            => new SkiaSharp.SKColor(c.R, c.G, c.B, c.A);

    }
}
