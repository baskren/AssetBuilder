using System;
namespace AndroidVector
{
    public enum TileMode
    {
        Clamp,
        Repeat,
        Mirror
    }

    public static class TileModeExtensions
    {
        public static SkiaSharp.SKShaderTileMode ToSKShaderTileMode(this TileMode tileMode)
        {
            if (tileMode == TileMode.Clamp)
                return SkiaSharp.SKShaderTileMode.Clamp;
            if (tileMode == TileMode.Mirror)
                return SkiaSharp.SKShaderTileMode.Mirror;
            if (tileMode == TileMode.Repeat)
                return SkiaSharp.SKShaderTileMode.Repeat;
            throw new Exception("AndroidVector TileMode cannot be converted to SkiaSharp.SKShaderTileMode");
        }
    }


}
