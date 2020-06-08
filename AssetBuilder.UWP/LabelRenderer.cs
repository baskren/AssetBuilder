using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(AssetBuilder.Label), typeof(AssetBuilder.UWP.LabelRenderer))]
namespace AssetBuilder.UWP
{
    class LabelRenderer : Xamarin.Forms.Platform.UWP.LabelRenderer
    {
        bool IsElementOrControlEmpty => Element == null || Control == null;
        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Label> e)
        {
            var control = Control;
            base.OnElementChanged(e);

            if (control != Control)
            {
                if (control != null)
                {
                    control.DragOver -= Control_DragOver;
                    control.DragLeave -= Control_DragLeave;
                    control.Drop -= Control_Drop;
                    control.DragEnter -= Control_DragEnter;
                }
                Control.AllowDrop = true;
                Control.DragOver += Control_DragOver;
                Control.DragLeave += Control_DragLeave;
                Control.Drop += Control_Drop;
                Control.DragEnter += Control_DragEnter;
                UpdatePlaceholder();
            }
        }

        bool _disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                if (Control != null)
                {
                    Control.DragOver -= Control_DragOver;
                    Control.DragLeave -= Control_DragLeave;
                    Control.Drop -= Control_Drop;
                    Control.DragEnter -= Control_DragEnter;
                }
            }
            base.Dispose(disposing);
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

        private void Control_DragEnter(object sender, Windows.UI.Xaml.DragEventArgs e)
        {
            Background = Color.LightGray.ToBrush();

        }

        async void Control_Drop(object sender, Windows.UI.Xaml.DragEventArgs e)
        {
            //throw new NotImplementedException();
            System.Diagnostics.Debug.WriteLine(GetType() + ".Control_Drop");
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                var array = items.ToArray();
                if (items.FirstOrDefault() is StorageFile storageFile)
                    Element.Text = storageFile.Path;
                else if (items.FirstOrDefault() is StorageFolder folder)
                    Element.Text = folder.Path;
            }
            Background = Element.BackgroundColor.ToBrush();
        }

        private void Control_DragLeave(object sender, Windows.UI.Xaml.DragEventArgs e)
        {
            //throw new NotImplementedException();
            System.Diagnostics.Debug.WriteLine(GetType() + ".Control_DragLeave");
            Background = Element.BackgroundColor.ToBrush();
        }

        private void Control_DragOver(object sender, Windows.UI.Xaml.DragEventArgs e)
        {
            //throw new NotImplementedException();
            System.Diagnostics.Debug.WriteLine(GetType() + ".Control_DragOver ");

            e.AcceptedOperation = DataPackageOperation.Copy;

            if (e.DragUIOverride != null)
            {
                e.DragUIOverride.Caption = "Add file";
                e.DragUIOverride.IsContentVisible = true;
            }


        }

        void UpdatePlaceholder()
        {
            if (IsElementOrControlEmpty)
                return;

            if (Element is AssetBuilder.Label element)
            {
                var formatted = (FormattedString)element.Placeholder;

                if (formatted == null)
                    return;

                if (
                    (string.IsNullOrWhiteSpace(Element.Text) || string.IsNullOrWhiteSpace(Element.FormattedText.ToString()) )
                    && !string.IsNullOrWhiteSpace(element.Placeholder.ToString())
                    )
                {
                    var targetColor = element.PlaceholderColor;
                    var isDefault = targetColor.IsDefault;
                    var placeholderColor = Element.IsEnabled && !isDefault ? targetColor : Color.FromRgb(0.7, 0.7, 0.7);
                    var formattedString = element.Placeholder;
                    Control.Text = element.Placeholder.ToString();
                    Control.Foreground = placeholderColor.ToBrush();
                }
                else
                {
                    if (element.TextColor != Color.Default)
                        Control.Foreground = element.TextColor.ToBrush();
                    else
                        Control.ClearValue(TextBlock.ForegroundProperty);
                    Control.Text = element.Text;
                }
            }
        }
    }
}
