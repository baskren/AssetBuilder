/*
using System;
using System.ComponentModel;
using System.Linq;
using AppKit;
using Foundation;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(AssetBuilder.DragAndDropLabel), typeof(AssetBuilder.Mac.DragAndDropLabelRenderer))]
namespace AssetBuilder.Mac
{
    public class DragAndDropLabelRenderer : Xamarin.Forms.Platform.MacOS.LabelRenderer
    {
		bool IsElementOrControlEmpty => Element == null || Control == null;

		public DragAndDropLabelRenderer() : base()
        {
            RegisterForDraggedTypes(new string[] { "NSFilenamesPboardType" });
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Label> e)
        {
            base.OnElementChanged(e);
			if (e.NewElement != null)
			{
				UpdatePlaceholder();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
			if (e.PropertyName == Entry.PlaceholderProperty.PropertyName ||
				e.PropertyName == Entry.PlaceholderColorProperty.PropertyName)
				UpdatePlaceholder();
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdatePlaceholder();

			base.OnElementPropertyChanged(sender, e);
        }

        public override NSDragOperation DraggingEntered(NSDraggingInfo sender)
		{
			var sourceDragMask = sender.DraggingSourceOperationMask;
			var pboard = sender.DraggingPasteboard;

			Control.BackgroundColor = Color.LightGray.ToNSColor();

			if (pboard.Types.Any(s => s == "NSFilenamesPboardType"))
			{
				if ((sourceDragMask & NSDragOperation.Link) == NSDragOperation.Link)
					return NSDragOperation.Link;
				else if ((sourceDragMask & NSDragOperation.Copy) == NSDragOperation.Copy)
					return NSDragOperation.Copy;
			}
			return NSDragOperation.None;
		}

		public override void DraggingExited(NSDraggingInfo sender)
		{
			Control.BackgroundColor = Element.BackgroundColor.ToNSColor();
		}

		public override bool PerformDragOperation(NSDraggingInfo sender)
		{
			var sourceDragMask = sender.DraggingSourceOperationMask;
			var pboard = sender.DraggingPasteboard;

			if (pboard.Types.Any(s => s == "NSFilenamesPboardType"))
			{
				var files = pboard.GetPropertyListForType("NSFilenamesPboardType") as NSArray;
				for (nuint i = 0; i < files.Count; i++)
				{
					var file = files.GetItem<NSString>(i);
					Element.Text = file.ToString();
				}
				return true;
			}
			return false;
		}

		public override void ConcludeDragOperation(NSDraggingInfo sender)
		{
			Control.BackgroundColor = Element.BackgroundColor.ToNSColor();
		}


		void UpdatePlaceholder()
		{
			if (IsElementOrControlEmpty)
				return;

			if (Element is AssetBuilder.DragAndDropLabel element)
			{
				var formatted = (FormattedString)element.Placeholder;

				if (formatted == null)
					return;

				var targetColor = element.PlaceholderColor;

				// Placeholder default color is 70% gray
				// https://developer.apple.com/library/prerelease/ios/documentation/UIKit/Reference/UITextField_Class/index.html#//apple_ref/occ/instp/UITextField/placeholder

				var color = Element.IsEnabled && !targetColor.IsDefault ? targetColor : ColorExtensions.SeventyPercentGrey.ToColor();

				Control.PlaceholderAttributedString = formatted.ToAttributed(Element, color, Element.HorizontalTextAlignment);
				//Control.PlaceholderAttributedString = Xamarin.Forms.Platform.MacOS.FormattedStringExtensions.ToAttributed(formatted, Element.ToNSFont(), color);
			}
		}

	}
}
*/