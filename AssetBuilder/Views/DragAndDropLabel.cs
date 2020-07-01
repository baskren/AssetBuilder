using System;
using Xamarin.Forms;

namespace AssetBuilder
{
    public class DragAndDropLabel : Label
    {
        #region Properties

        #region Placeholder
        /// <summary>
        /// Backing store for DragAndDropLabel.Placeholder property
        /// </summary>
        public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(DragAndDropLabel), default);
        /// <summary>
        /// controls value of DragAndDropLabel.Placeholder property
        /// </summary>
        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }
        #endregion

        #region PlaceholderColor
        /// <summary>
        /// Backing store for DragAndDropLabel.PlaceholderColor property
        /// </summary>
        public static readonly BindableProperty PlaceholderColorProperty = BindableProperty.Create(nameof(PlaceholderColor), typeof(Color), typeof(DragAndDropLabel), default);
        /// <summary>
        /// controls value of DragAndDropLabel.PlaceholderColor property
        /// </summary>
        public Color PlaceholderColor
        {
            get => (Color)GetValue(PlaceholderColorProperty);
            set => SetValue(PlaceholderColorProperty, value);
        }
        #endregion



        #endregion

        public DragAndDropLabel()
        {
        }
    }
}
