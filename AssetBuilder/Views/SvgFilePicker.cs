using System;
using System.Runtime.CompilerServices;
using P42.Storage;
using Xamarin.Forms;

namespace AssetBuilder
{
    public class SvgFilePicker : StorageFilePicker
    {
        #region Page
        /// <summary>
        /// Backing store for SvgFilePicker Page property
        /// </summary>
        public static readonly BindableProperty PageProperty = BindableProperty.Create(nameof(Page), typeof(Page), typeof(SvgFilePicker), default);
        /// <summary>
        /// controls value of .Page property
        /// </summary>
        public Page Page
        {
            get => (Page)GetValue(PageProperty);
            set => SetValue(PageProperty, value);
        }
        #endregion

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
                var text = StorageFile.ReadAllText();
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
            }
        }

    }
}
