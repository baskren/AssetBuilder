using System;
namespace AndroidVector
{
    public static class MathExtensions
    {
        public static float ToRadians(float degrees)
            => (float)(Math.PI * degrees / 180f);

        public static double ToRadians(double degrees)
            => Math.PI * degrees / 180.0;

        public static decimal ToRadians(decimal degrees)
            => (decimal)(Math.PI * (double)degrees / 180.0);

        public static float ToDegrees(float radians)
            => (float)(180 * radians / Math.PI);

        public static double ToDegrees(double radians)
            => 180 * radians / Math.PI;

        public static decimal ToDegrees(decimal radians)
            => (decimal)(180 * (double)radians / Math.PI);
    }
}
