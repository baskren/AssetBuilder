using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using P42.Storage;
using Xamarin.Forms;

namespace AssetBuilder
{
    public abstract class StorageItemPicker : Frame, IDisposable
    {
        #region Properties

        #region Page
        /// <summary>
        /// Backing store for SvgFilePicker Page property
        /// </summary>
        public static readonly BindableProperty PageProperty = BindableProperty.Create(nameof(Page), typeof(Page), typeof(StorageItemPicker), default);
        /// <summary>
        /// controls value of .Page property
        /// </summary>
        public Page Page
        {
            get => (Page)GetValue(PageProperty);
            set => SetValue(PageProperty, value);
        }
        #endregion


        #region TextColor
        /// <summary>
        /// Backing store for StorageFilePicker TextColor property
        /// </summary>
        public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(StorageItemPicker), default);
        /// <summary>
        /// controls value of .TextColor property
        /// </summary>
        public Color TextColor
        {
            get => (Color)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }
        #endregion


        #region Placeholder
        /// <summary>
        /// Backing store for Label Placeholder property
        /// </summary>
        public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(StorageItemPicker), default);
        /// <summary>
        /// controls value of .Placeholder property
        /// </summary>
        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }
        #endregion

        #region PlaceholderColor
        /// <summary>
        /// Backing store for Label PlaceholderColor property
        /// </summary>
        public static readonly BindableProperty PlaceholderColorProperty = BindableProperty.Create(nameof(PlaceholderColor), typeof(Color), typeof(StorageItemPicker), Color.DarkGray);
        /// <summary>
        /// controls value of .PlaceholderColor property
        /// </summary>
        public Color PlaceholderColor
        {
            get => (Color)GetValue(PlaceholderColorProperty);
            set => SetValue(PlaceholderColorProperty, value);
        }
        #endregion

        #region StorageItem
        /// <summary>
        /// Backing store for StorageItemPicker StorageItem property
        /// </summary>
        public static readonly BindableProperty StorageItemProperty = BindableProperty.Create(nameof(StorageItem), typeof(IStorageItem), typeof(StorageItemPicker), default);
        /// <summary>
        /// controls value of .StorageItem property
        /// </summary>
        public IStorageItem StorageItem
        {
            get => (IStorageItem)GetValue(StorageItemProperty);
            set => SetValue(StorageItemProperty, value);
        }
        #endregion


        #endregion

        #region VisualElements
        Grid grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = 40 }
            }
        };

        Label pathLabel = new Label
        {
            VerticalOptions = LayoutOptions.Fill,
            HorizontalOptions = LayoutOptions.Fill,
            HorizontalTextAlignment = TextAlignment.Start,
            VerticalTextAlignment = TextAlignment.Start,
        };

        Label clearLabel = new Label
        {
            Text = "ⓧ",
            VerticalTextAlignment = TextAlignment.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Fill,
            HorizontalOptions = LayoutOptions.Fill
        };
        #endregion


        #region Fields
        TapGestureRecognizer pathTap, clearTap;
        #endregion


        #region construction / disposal
        internal StorageItemPicker()
        {
            BackgroundColor = Color.White;
            BorderColor = Color.DarkGray;
            CornerRadius = 4;
            Margin = 0;
            HasShadow = false;
            Padding = 2;

            grid.Children.Add(pathLabel);
            grid.Children.Add(clearLabel, 1, 0);

            Content = grid;

            pathTap = new TapGestureRecognizer();
            pathTap.Tapped += PathTap_Tapped;
            pathLabel.GestureRecognizers.Add(pathTap);

            clearTap = new TapGestureRecognizer();
            clearTap.Tapped += ClearTap_Tapped;
            clearLabel.GestureRecognizers.Add(clearTap);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _disposed = true;
                pathTap.Tapped -= PathTap_Tapped;
                clearTap.Tapped -= ClearTap_Tapped;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion


        #region Methods
        void ClearTap_Tapped(object sender, EventArgs e)
        {
            StorageItem = null;
        }

        protected virtual async void PathTap_Tapped(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == PlaceholderProperty.PropertyName)
                UpdateLabel();
            else if (propertyName == StorageItemProperty.PropertyName)
                UpdateLabel();
        }

        void UpdateLabel()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (StorageItem is null)
                {
                    pathLabel.Text = Placeholder;
                    pathLabel.TextColor = PlaceholderColor == default
                        ? TextColor
                        : PlaceholderColor;
                }
                else
                {
                    pathLabel.Text = StorageItem.Path;
                    pathLabel.TextColor = TextColor;
                }
            });
        }

        protected void DisplayAlert(string title, string text, string buttonText)
        {
            Device.BeginInvokeOnMainThread(async () => await (Page?.DisplayAlert(title, text, buttonText) ?? Task.CompletedTask));
        }
        #endregion

    }
}
