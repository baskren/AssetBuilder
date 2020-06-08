using System;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace AssetBuilder
{
    public class Label : Xamarin.Forms.Label
    {
        #region Placeholder
        /// <summary>
        /// Backing store for Label Placeholder property
        /// </summary>
        public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(nameof(Placeholder), typeof(FormattedString), typeof(Label), default);
        /// <summary>
        /// controls value of .Placeholder property
        /// </summary>
        public FormattedString Placeholder
        {
            get => (FormattedString)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }
        #endregion

        #region PlaceholderColor
        /// <summary>
        /// Backing store for Label PlaceholderColor property
        /// </summary>
        public static readonly BindableProperty PlaceholderColorProperty = BindableProperty.Create(nameof(PlaceholderColor), typeof(Color), typeof(Label), default);
        /// <summary>
        /// controls value of .PlaceholderColor property
        /// </summary>
        public Color PlaceholderColor
        {
            get => (Color)GetValue(PlaceholderColorProperty);
            set => SetValue(PlaceholderColorProperty, value);
        }
        #endregion

        public event EventHandler<TextChangedEventArgs> TextChanged;

        string oldText;

        public Label()
        {
        }


        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == TextProperty.PropertyName)
            {
                TextChanged?.Invoke(this, new TextChangedEventArgs(oldText, Text));
                oldText = Text;
            }
        }
    }
}
