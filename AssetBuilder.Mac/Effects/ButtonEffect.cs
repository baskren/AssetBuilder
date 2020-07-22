using System;
using AppKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly:ExportEffect(typeof(AssetBuilder.Mac.ButtonEffect), nameof(AssetBuilder.Mac.ButtonEffect))]
namespace AssetBuilder.Mac
{
    public class ButtonEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            if (Control is NSButton nsButton)
            {
                nsButton.BezelStyle = NSBezelStyle.RoundRect;
                nsButton.Bordered = false;
                nsButton.WantsLayer = true;
                nsButton.Layer.BackgroundColor = Xamarin.Forms.Color.Transparent.ToCGColor();
                nsButton.FocusRingType = NSFocusRingType.None;
            }
        }


        protected override void OnDetached()
        {
        }
    }
}
