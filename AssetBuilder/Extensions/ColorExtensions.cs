using System;
using System.Collections.Generic;
using System.Text;

namespace AssetBuilder
{
    public static class ColorExtensions
    {
        public static string ToArgbHex(this Xamarin.Forms.Color color)
        {
            uint num = (uint)(color.R * 255.0);
            uint num2 = (uint)(color.G * 255.0);
            uint num3 = (uint)(color.B * 255.0);
            uint num4 = (uint)(color.A * 255.0);
            return $"#{num4:X2}{num:X2}{num2:X2}{num3:X2}";
        }

        public static string ToRgbHex(this Xamarin.Forms.Color color)
        {
            uint num = (uint)(color.R * 255.0);
            uint num2 = (uint)(color.G * 255.0);
            uint num3 = (uint)(color.B * 255.0);
            return $"#{num:X2}{num2:X2}{num3:X2}";
        }

    }
}
