using System;
using System.Runtime.CompilerServices;
using P42.SandboxedStorage;
using Xamarin.Forms;

namespace AssetBuilder
{
    public class SvgFilePicker : StorageFilePickerHybridView
    {
        #region Fields
        IStorageFile oldFile;
        #endregion

        public SvgFilePicker()
        {
            Placeholder = "click to select SVG file";
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == StorageItemProperty.PropertyName)
            {
                Xamarin.Forms.Device.BeginInvokeOnMainThread(async () =>
                {
                    var text = await StorageFile.ReadAllTextAsync();
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        Page?.DisplayAlert("SVG File", "File [" + StorageFile.Path + "] is empty.", "ok");
                        StorageFile = oldFile;
                    }
                    if (!text.Contains("<svg") || !text.Contains("http://www.w3.org/2000/svg"))
                    {
                        Page?.DisplayAlert("SVG File", "File [" + StorageFile.Path + "] doesn't appear to have SVG content.", "ok");
                        StorageFile = oldFile;
                    }
                    oldFile = StorageFile;
                });
            }
        }

    }
}
