using System;
using AppKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace AssetBuilder.Mac
{
    public static class ColorExtensions
    {
		internal static readonly NSColor Black = NSColor.Black;
		internal static readonly NSColor SeventyPercentGrey = NSColor.FromRgba(0.7f, 0.7f, 0.7f, 1);
		internal static readonly NSColor LabelColor = NSColor.Black;
		internal static readonly NSColor AccentColor = Color.FromRgba(50, 79, 133, 255).ToNSColor();
	}
}
