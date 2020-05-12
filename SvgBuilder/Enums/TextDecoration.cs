using System;
namespace SvgBuilder.Enums
{
    [Flags]
    public enum TextDecoration
    {
        Inherit = 0,
        None = 1,
        Underline = 2,
        Overline = 4,
        Line_through = 8,
        Blink = 16
    }
}
