using System;
using System.ComponentModel;
using Xamarin.Forms;
using AssetBuilder.Models;
using SkiaSharp;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using P42.Utils;
using Amporis.Xamarin.Forms.ColorPicker;
using System.Collections.Generic;
using System.Threading.Tasks;
using P42.Storage;

namespace AssetBuilder.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        const string XmlHeader = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n";


        #region Construction / Initialization
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            Preferences.Current.PropertyChanged += OnPreferencesChanged;

            _iosProjectFolderPicker.ProjectPlatform = Xamarin.Essentials.DevicePlatform.iOS;
            _androidProjectFolderPicker.ProjectPlatform = Xamarin.Essentials.DevicePlatform.Android;
            _uwpProjectFolderPicker.ProjectPlatform = Xamarin.Essentials.DevicePlatform.UWP;

            _iosProjectFolderPicker.Page = this;
            _androidProjectFolderPicker.Page = this;
            _uwpProjectFolderPicker.Page = this;
            _iconSvgFilePicker.Page = this;
            _squareSvgLaunchImagePicker.Page = this;
            _rect310SvgLaunchImagePicker.Page = this;



            _iosProjectFolderPicker.StorageFolderChanged += OnIosProjectFolderChanged;
            _androidProjectFolderPicker.StorageFolderChanged += OnAndroidProjectFolderChanged;
            _uwpProjectFolderPicker.StorageFolderChanged += OnUwpProjectFolderChanged;

            _iconSvgFilePicker.StorageFileChanged += OnIconSvgFileChanged;
            _squareSvgLaunchImagePicker.StorageFileChanged += OnSquareSvgLaunchImageEntryChanged;
            _rect310SvgLaunchImagePicker.StorageFileChanged += OnRect310SvgLaunchImageEntryChanged;

            _mobileUseSquareSplashImageCheckBox.CheckedChanged += MobileSourceCheckBoxChanged;
            _mobileUseRect310SplashImageCheckBox.CheckedChanged += MobileSourceCheckBoxChanged;

            LoadPreferences();

        }

        void LoadPreferences()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await _iosProjectFolderPicker.SetStorageFolderFromPathAsync(Preferences.Current.IosOProjectFolder);
                await _androidProjectFolderPicker.SetStorageFolderFromPathAsync(Preferences.Current.AndroidProjectFolder);
                await _uwpProjectFolderPicker.SetStorageFolderFromPathAsync(Preferences.Current.UwpProjectFolder);

                _splashPageBackgroundColorEntry.Text = Preferences.Current.SplashBackgroundColor.ToHex();
                _iconBackgroundColorEntry.Text = Preferences.Current.IconBackgroundColor.ToHex();

                await _iconSvgFilePicker.SetStorageFileFromPathAsync(Preferences.Current.SvgIconFile);
                await _squareSvgLaunchImagePicker.SetStorageFileFromPathAsync(Preferences.Current.SquareSvgSplashImageFile);
                await _rect310SvgLaunchImagePicker.SetStorageFileFromPathAsync(Preferences.Current.Rect310SvgSplashImageFile);

                UpdateMobileSplashSource();

                UpdateButtonAbility();

                OnPreferencesChanged(this, new PropertyChangedEventArgs(nameof(Preferences.SplashBackgroundColor)));
                OnPreferencesChanged(this, new PropertyChangedEventArgs(nameof(Preferences.IconBackgroundColor)));
                OnPreferencesChanged(this, new PropertyChangedEventArgs(nameof(Preferences.AndroidProjectFolder)));
                OnPreferencesChanged(this, new PropertyChangedEventArgs(nameof(Preferences.IosOProjectFolder)));
                OnPreferencesChanged(this, new PropertyChangedEventArgs(nameof(Preferences.UwpProjectFolder)));
            });
        }
        #endregion


        #region Preferences Change Handlers
        private void OnPreferencesChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateButtonAbility();

            if (e.PropertyName == nameof(Preferences.SplashBackgroundColor))
            {
                _splashPageBackgroundColorEntry.Text = Preferences.Current.SplashBackgroundColor.ToHex();
                _splashPageBackgroundColorSample.BackgroundColor = Preferences.Current.SplashBackgroundColor;
            }
            else if (e.PropertyName == nameof(Preferences.IconBackgroundColor))
            {
                _iconBackgroundColorEntry.Text = Preferences.Current.IconBackgroundColor.ToHex();
                _iconBackgroundColorSample.BackgroundColor = Preferences.Current.IconBackgroundColor;
            }
            else if (e.PropertyName == nameof(Preferences.MobileSplashSource))
                UpdateMobileSplashSource();
            else if (e.PropertyName == nameof(Preferences.SquareSvgSplashImageFile))
                UpdateMobileSplashSource();
            else if (e.PropertyName == nameof(Preferences.Rect310SvgSplashImageFile))
                UpdateMobileSplashSource();
        }
        #endregion


        #region Entry event Handlers
        private void MobileSourceCheckBoxChanged(object sender, CheckedChangedEventArgs e)
        {
            if (_mobileUseSquareSplashImageCheckBox.IsChecked)
                Preferences.Current.MobileSplashSource = MobileSplashSource.Square;
            else if (_mobileUseRect310SplashImageCheckBox.IsChecked)
                Preferences.Current.MobileSplashSource = MobileSplashSource.Rect310;
            else
                Preferences.Current.MobileSplashSource = MobileSplashSource.None;
        }

        private void OnIosProjectFolderChanged(object sender, StorageFolderChangedEventArgs e)
        {
            Preferences.Current.IosOProjectFolder = e.StorageFolder?.Path;
        }

        private void OnAndroidProjectFolderChanged(object sender, StorageFolderChangedEventArgs e)
        {
            Preferences.Current.AndroidProjectFolder = e.StorageFolder?.Path;
        }

        private void OnUwpProjectFolderChanged(object sender, StorageFolderChangedEventArgs e)
        {
            Preferences.Current.UwpProjectFolder = e.StorageFolder?.Path;
        }

        private void OnIconSvgFileChanged(object sender, StorageFileChangedEventArgs e)
        {
            Preferences.Current.SvgIconFile = e.StorageFile?.Path;
        }

        private void OnSquareSvgLaunchImageEntryChanged(object sender, StorageFileChangedEventArgs e)
        {
            if (e.StorageFile is null)
            {
                Preferences.Current.SquareSvgSplashImageFile = null;
                _mobileUseSquareSplashImageCheckBox.IsEnabled = false;
                _mobileUseSquareSplashImageCheckBox.IsChecked = false;
            }
            else
            {
                Preferences.Current.SquareSvgSplashImageFile = e.StorageFile.Path;
                _mobileUseSquareSplashImageCheckBox.IsEnabled = true;
                _mobileUseSquareSplashImageCheckBox.IsChecked = string.IsNullOrWhiteSpace(Preferences.Current.Rect310SvgSplashImageFile); 
            }
            UpdateMobileSplashSource();
        }

        private void OnRect310SvgLaunchImageEntryChanged(object sender, StorageFileChangedEventArgs e)
        {
            if (e.StorageFile is null)
            {
                Preferences.Current.Rect310SvgSplashImageFile = null;
                _mobileUseRect310SplashImageCheckBox.IsEnabled = false;
                _mobileUseRect310SplashImageCheckBox.IsChecked = false;
            }
            else
            {
                Preferences.Current.Rect310SvgSplashImageFile = e.StorageFile.Path;
                _mobileUseRect310SplashImageCheckBox.IsEnabled = true;
                _mobileUseRect310SplashImageCheckBox.IsChecked = string.IsNullOrWhiteSpace(Preferences.Current.SquareSvgSplashImageFile);
            }
            UpdateMobileSplashSource();
        }

        async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            if (sender is Frame frame)
            {
                if (frame == _splashPageBackgroundColorFrame)
                    Preferences.Current.SplashBackgroundColor = await ColorPickerDialog.Show(Content as Grid, "Splash Screen Background", Preferences.Current.SplashBackgroundColor, null);
                else if (frame == _iconBackgroundColorFrame)
                    Preferences.Current.IconBackgroundColor = await ColorPickerDialog.Show(Content as Grid, "Icon Background", Preferences.Current.IconBackgroundColor, null);
            }
        }

        #endregion


        #region methods


        #region UI Helpers
        ContentView spinnerContentView;
        void ShowSpinner()
        {
            if (spinnerContentView != null)
                return;

            if (this.Content is Grid grid)
            {
                var rows = grid.RowDefinitions.Count;
                var columns = grid.ColumnDefinitions.Count;

                var spinner = new Xamarin.Forms.ActivityIndicator
                {
                    IsEnabled = true,
                    IsRunning = true,
                    Color = Color.White,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                };

                spinnerContentView = new ContentView
                {
                    BackgroundColor = Color.FromRgba(0, 0, 0, 0.5),
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill,
                    Content = spinner
                };

                if (rows > 0)
                    Grid.SetRowSpan(spinnerContentView, rows);
                if (columns > 0)
                    Grid.SetColumnSpan(spinnerContentView, columns);
                grid.Children.Add(spinnerContentView);
            }
        }

        void HideSpinner()
        {
            if (this.Content is Grid grid && spinnerContentView != null)
            {
                grid.Children.Remove(spinnerContentView);
                spinnerContentView = null;
            }
        }

        void UpdateButtonAbility()
        {
            _generateIconsButton.IsEnabled = Preferences.Current.IsIconEnabled;
            _generateLaunchImagesButton.IsEnabled = Preferences.Current.IsSplashEnabled;
        }

        void UpdateMobileSplashSource()
        {
            _mobileUseSquareSplashImageCheckBox.IsEnabled = _squareSvgLaunchImagePicker.StorageFile != null;
            if (_squareSvgLaunchImagePicker.StorageFile == null)
                _mobileUseSquareSplashImageCheckBox.IsChecked = false;

            _mobileUseRect310SplashImageCheckBox.IsEnabled = _rect310SvgLaunchImagePicker.StorageFile != null;
            if (_rect310SvgLaunchImagePicker.StorageFile == null)
                _mobileUseRect310SplashImageCheckBox.IsChecked = false;

            if (!_mobileUseSquareSplashImageCheckBox.IsChecked && !_mobileUseRect310SplashImageCheckBox.IsChecked)
            {
                if (_squareSvgLaunchImagePicker.StorageFile != null)
                    _mobileUseSquareSplashImageCheckBox.IsChecked = true;
                else if (_rect310SvgLaunchImagePicker.StorageFile != null)
                    _mobileUseRect310SplashImageCheckBox.IsChecked = true;
            }
        }
        #endregion


        #region File Helpers
        async Task<IStorageFile> CopyEmbeddedResourceToFolder(string embeddedResourceId, string fileName, IStorageFolder folder)
        {
            if (!await folder.FileExists(fileName))
            {
                if (await folder.CreateFileAsync(fileName) is IStorageFile file)
                {
                    var localVersionPath = await EmbeddedResourceCache.LocalStorageFullPathForEmbeddedResourceAsync(embeddedResourceId, GetType().Assembly);
                    var text = File.ReadAllText(localVersionPath);
                    await file.WriteAllTextAsync(text);
                    return file;
                }
                else
                    await DisplayAlert(null, "Cannot create " + fileName + " in folder [" + folder.Path + "].", "ok");
            }
            return null;

        }
        #endregion


        #region Generate Icons
        async void OnGenerateIconsButtonClicked(object sender, EventArgs e)
        {
            ShowSpinner();
            if (_iconSvgFilePicker.StorageFile is null)
            {
                await DisplayAlert(null, "Cannot open SVG file [" + Preferences.Current.SvgIconFile + "]", "cancel");
                return;
            }
            var vector = await GenerateVectorAndroidIcons();
            await GenerateRasterAndroidIcons(vector);
            await GenerateIosIcons(vector);
            await GenerateUwpIcons(vector);

            await DisplayAlert("Complete", "App Icons have been generated.", "ok");
            HideSpinner();
        }

        async Task<AndroidVector.Vector> GenerateVectorAndroidIcons()
        {
            if (await Svg2.GenerateAndroidVectorAsync(_iconSvgFilePicker.StorageFile) is (AndroidVector.Vector vector, List<string> warnings))
            {
                if (warnings.Count > 0)
                    await DisplayAlert("Warnings", string.Join("\n\n", warnings), "ok");
                vector = vector.AspectClone();
            }
            else
            {
                await DisplayAlert("ERROR", "Failed to generate AndroidVector for unknown reason.", "ok");
                vector = null;
            }

            if (_androidProjectFolderPicker.StorageFolder != null)
            {
                var resourcesFolder = await _androidProjectFolderPicker.StorageFolder.GetOrCreateFolderAsync("Resources");

                if (await _androidProjectFolderPicker.GetProjectFile() is IStorageFile storageFile)
                {
                    if (await XDocumentExtensions.LoadAsync(storageFile) is XDocument csprojDoc)
                    {
                        XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";
                        if (csprojDoc.Descendants(ns + "AndroidResource").FirstOrDefault(e => e.Attribute("Include")?.Value == "Resources\\values\\styles.xml") is XElement firstAndroidResource)
                        {
                            var androidResourceItemGroup = firstAndroidResource.Parent;
                            if (!androidResourceItemGroup.Elements(ns + "AndroidResource").Any(e => e.Attribute("Include")?.Value == "Resources\\mipmap-anydpi-v26\\launcher_foreground.xml"))
                            {
                                var element = new XElement(ns + "AndroidResource");
                                element.SetAttributeValue("Include", "Resources\\mipmap-anydpi-v26\\launcher_foreground.xml");
                                androidResourceItemGroup.Add(element);

                                await StorageFileExtensions.WriteAllTextAsync(storageFile, XmlHeader + csprojDoc);
                            }
                        }

                    }
                    else
                    {
                        await DisplayAlert(null, "Could not load Android .csproj file as an XDocument", "ok");
                        return null;
                    }

                    var valuesFolder = await resourcesFolder?.GetOrCreateFolderAsync( "values");
                    if (await valuesFolder?.GetFileAsync("colors.xml") is IStorageFile colorsFile)
                    {
                        var colorsDocument = await XDocumentExtensions.LoadAsync(colorsFile);
                        var colors = colorsDocument.Root;
                        if (colors.Name.ToString() == "resources")
                        {
                            var color = colors.Elements("color").FirstOrDefault(e => e.Attribute("name") is XAttribute name && name.Value == "launcher_background");
                            if (color is null)
                            {
                                color = new XElement("color");
                                color.SetAttributeValue("name", "launcher_background");
                                colors.Add(color);
                            }
                            color.Value = Preferences.Current.IconBackgroundColor.ToHex();

                            var text = XmlHeader + colorsDocument.ToString();
                            await StorageFileExtensions.WriteAllTextAsync(colorsFile, text);
                        }
                    }
                    else
                        await DisplayAlert("Error", "Could not find values/colors.xml in Android project","ok");


                    if (vector != null)
                    {
                        if (await resourcesFolder?.GetOrCreateFolderAsync("mipmap-anydpi-v26") is IStorageFolder mipmapFolder)
                        {
                            // through pure Android cruelty, we have to do this.  What a mess.
                            var tmpVector = AndroidVector.BaseElementExtensions.Copy(vector);
                            tmpVector.Width = new AndroidVector.UnitizedFloat(117, AndroidVector.Unit.Dp);
                            tmpVector.Height = new AndroidVector.UnitizedFloat(117, AndroidVector.Unit.Dp);
                            tmpVector.ViewportWidth *= 1.5;
                            tmpVector.ViewportHeight *= 1.5;
                            tmpVector.SvgTransforms.Add(AndroidVector.Matrix.CreateTranslate((float)(tmpVector.ViewportWidth / 6), (float)(tmpVector.ViewportHeight / 6)));
                            tmpVector.ApplySvgTransforms();
                            tmpVector.PurgeDefaults();
                            var launcher_foreground = await mipmapFolder.GetOrCreateFileAsync("launcher_foreground.xml");
                            await StorageFileExtensions.WriteAllTextAsync(launcher_foreground, tmpVector.ToString());
                        }
                        else
                            await DisplayAlert("Error", "Could not find or create Resources/mipmap-anydpi-v26 in Android project", "ok");
                    }
                }
            }
            return vector;
        }

        async Task GenerateRasterAndroidIcons(AndroidVector.Vector vector)
        {
            if (_androidProjectFolderPicker.StorageFolder is null)
                return;

            var folders = await _androidProjectFolderPicker.StorageFolder.GetFoldersAsync();
            foreach (var folder in folders)
                System.Diagnostics.Debug.WriteLine("folder: " + folder.Path);

            if (await _androidProjectFolderPicker.StorageFolder.GetFolderAsync("Resources") is IStorageFolder resourcesFolder)
            {
                var resourceFolders = await resourcesFolder.GetFoldersAsync();
                foreach (var targetFolder in resourceFolders)
                {
                    var targetFiles = await targetFolder.GetFilesAsync();
                    foreach (var targetFile in targetFiles)
                    {
                        if (targetFile.Name.ToLower() == "icon.png")
                        {
                            int size = 0;
                            if (await targetFile.ReadAllBytesAsync() is byte[] bytes)
                            {
                                var bitmap = SKBitmap.Decode(bytes);
                                if (bitmap.Width == bitmap.Height && bitmap.Width > 10)
                                    size = bitmap.Width;
                            }
                            else
                            {
                                await DisplayAlert(null, "Cannot read data from icon file [" + targetFile.Path + "] to determine size ... and thus cannot update it with SVG icon artwork", "ok");
                                return;
                            }
                            if (size > 0)
                                await vector.ToPngAsync(targetFile, Preferences.Current.IconBackgroundColor, new System.Drawing.Size(size, size));
                            else
                            {
                                await DisplayAlert(null, "The icon file [" + targetFile.Path + "] was found to be [" + size + "] pixels wide ... and thus cannot update it with SVG icon artwork", "ok");
                                return;
                            }
                        }
                        if (targetFile.Name.ToLower() == "launcher_foreground.png")
                        {
                            int size = 0;
                            if (await targetFile.ReadAllBytesAsync() is byte[] bytes)
                            {
                                var bitmap = SKBitmap.Decode(bytes);
                                if (bitmap.Width == bitmap.Height && bitmap.Width > 10)
                                    size = bitmap.Width;
                            }
                            else
                            {
                                await DisplayAlert(null, "Cannot read data from icon file [" + targetFile.Path + "] to determine size ... and thus cannot update it with SVG icon artwork", "ok");
                                return;
                            }
                            if (size > 0)
                                await vector.ToPngAsync(targetFile, Preferences.Current.IconBackgroundColor, new System.Drawing.Size(size * 2 / 3, size * 2 / 3), new System.Drawing.Size(size, size));
                            else
                            {
                                await DisplayAlert(null, "The icon file [" + targetFile.Path + "] was found to be [" + size + "] pixels wide ... and thus cannot update it with SVG icon artwork", "ok");
                                return;
                            }
                        }
                    }
                }
            }
            else
                await DisplayAlert(null, "Cannot find Android Resources folder in Android Project Folder [" + _androidProjectFolderPicker.StorageFolder.Path + "]", "ok");
        }

        async Task GenerateIosIcons(AndroidVector.Vector vector)
        {
            if (_iosProjectFolderPicker.StorageFolder is null)
                return;

            var xcassettsFolder = await _iosProjectFolderPicker.StorageFolder.GetFolderAsync("Assets.xcassets");
            if (await (xcassettsFolder?.GetFolderAsync("AppIcon.appiconset") ?? Task.FromResult<IStorageFolder>(null)) is IStorageFolder appIconSetFolder)
            {
                var iconFiles = await appIconSetFolder.GetFilesAsync();
                foreach (var iconFile in iconFiles)
                {
                    if (iconFile.FileType.ToLower() == ".png"
                        && iconFile.Name.ToLower().StartsWith("icon")
                        )
                    {
                        int size = 0;
                        if (await iconFile.ReadAllBytesAsync() is byte[] bytes)
                        {
                            var bitmap = SKBitmap.Decode(bytes);
                            size = bitmap.Width;
                        }
                        else
                        {
                            await DisplayAlert(null, "Cannot read data from icon file [" + iconFile.Path + "] to determine size ... and thus cannot update it with SVG icon artwork", "ok");
                            return;
                        }
                        if (size > 0)
                            await vector.ToPngAsync(iconFile, Preferences.Current.IconBackgroundColor, new System.Drawing.Size(size, size));
                        else
                        {
                            await DisplayAlert(null, "The icon file [" + iconFile.Path + "] was found to be ["+size+"] pixels wide ... and thus cannot update it with SVG icon artwork", "ok");
                            return;
                        }
                    }
                }
            }
            else
                await DisplayAlert(null, "Cannot find iOS project's Assets.xcassets/AppIcon.appiconset folder", "ok");

        }

        static readonly Dictionary<string, int> UwpIcons = new Dictionary<string, int>
        {
            { "BadgeLogo.scale-100.png", 24 },
            { "BadgeLogo.scale-125.png", 30 },
            { "BadgeLogo.scale-150.png", 36 },
            { "BadgeLogo.scale-200.png", 48 },
            { "BadgeLogo.scale-400.png", 96 },
            { "LockScreenLogo.scale-200.png", 48 },
            { "Square44x44Logo.scale-100.png", 44 },
            { "Square44x44Logo.scale-125.png", 55 },
            { "Square44x44Logo.scale-150.png", 66 },
            { "Square44x44Logo.scale-200.png", 88 },
            { "Square44x44Logo.scale-400.png", 176 },
            { "Square44x44Logo.altform-unplated_targetsize-16.png", 16 },
            { "Square44x44Logo.altform-unplated_targetsize-24.png", 24 },
            { "Square44x44Logo.altform-unplated_targetsize-32.png", 32 },
            { "Square44x44Logo.altform-unplated_targetsize-48.png", 48 },
            { "Square44x44Logo.altform-unplated_targetsize-256.png", 256 },
            { "Square44x44Logo.targetsize-16.png", 16 },
            { "Square44x44Logo.targetsize-24.png", 24 },
            { "Square44x44Logo.targetsize-32.png", 32 },
            { "Square44x44Logo.targetsize-48.png", 48 },
            { "Square44x44Logo.targetsize-256.png", 256 },
            { "Square71x71Logo.scale-100.png", 71 },
            { "Square71x71Logo.scale-125.png", 89 },
            { "Square71x71Logo.scale-150.png", 107 },
            { "Square71x71Logo.scale-200.png", 142 },
            { "Square71x71Logo.scale-400.png", 284 },
            { "StoreLogo.backup.png", 50 },
            { "StoreLogo.scale-100.png", 50 },
            { "StoreLogo.scale-125.png", 63 },
            { "StoreLogo.scale-150.png", 75 },
            { "StoreLogo.scale-200.png", 100 },
            { "StoreLogo.scale-400.png", 200 },
            { "SmallTile.scale-100.png", 71 },
            { "SmallTile.scale-125.png", 89 },
            { "SmallTile.scale-150.png", 107 },
            { "SmallTile.scale-200.png", 142 },
            { "SmallTile.scale-400.png", 248 },
        };

        async Task GenerateUwpIcons(AndroidVector.Vector vector)
        {
            if (_uwpProjectFolderPicker.StorageFolder is null)
                return;

            if (await _uwpProjectFolderPicker.StorageFolder.GetFolderAsync("Assets") is IStorageFolder assetsFolder)
            {
                foreach (var kvp in UwpIcons)
                {
                    var file = await assetsFolder.GetOrCreateFileAsync(kvp.Key);
                    var size = kvp.Value;
                    await vector.ToPngAsync(file, Preferences.Current.IconBackgroundColor, new System.Drawing.Size(size, size));
                }
            }
            else
                await DisplayAlert(null, "Cannot find UWP Assets folder in UWP project folder [" + _uwpProjectFolderPicker.StorageFolder.Path + "]", "ok");
        }

        #endregion


        #region Generate Splash Screens and Images
        async void OnGenerateLaunchImageButtonClicked(object sender, EventArgs e)
        {
            ShowSpinner();
            if (await GenerateUwpSplashAndLogoImages() is AndroidVector.Vector mobileSplashVector)
            { 
                await GenerateAndroidSpashImage(mobileSplashVector);
                await GenerateIosSplashScreen(mobileSplashVector);
            }
            else
                await DisplayAlert(null, "No mobile splash image has been generated.", "ok");

            await DisplayAlert("Complete", "Launch Screens have been generated.", "ok");
            HideSpinner();
        }

        #region Android
        async Task<AndroidVector.Vector> GenerateAndroidSpashImage(AndroidVector.Vector vector)
        {
            if (vector != null && _androidProjectFolderPicker.StorageFolder is IStorageFolder projectFolder)
            {
                if (!(await projectFolder.GetOrCreateFolderAsync("Resources") is IStorageFolder resourcesFolder))
                {
                    await DisplayAlert(null, "Cannot open or create Resources folder in [" + projectFolder.Path + "]", "ok");
                    return vector;
                }
                if (!(await resourcesFolder.GetOrCreateFolderAsync("drawable") is IStorageFolder drawableFolder))
                {
                    await DisplayAlert(null, "Cannot open or create drawable folder in [" + resourcesFolder.Path + "]", "ok");
                    return vector;
                }
                if (!(await resourcesFolder.GetOrCreateFolderAsync("drawable-v23") is IStorageFolder drawable23Folder))
                {
                    await DisplayAlert(null, "Cannot open or create drawable-v23 folder in [" + resourcesFolder.Path + "]", "ok");
                    return vector;
                }

                var splashActivityFileName = "SplashActivity.cs";
                await CopyEmbeddedResourceToFolder("AssetBuilder.Resources." + splashActivityFileName, splashActivityFileName, _androidProjectFolderPicker.StorageFolder);

                var splashBackgroundFileName = "background_splash.xml";
                await CopyEmbeddedResourceToFolder("AssetBuilder.Resources." + splashBackgroundFileName, splashBackgroundFileName, drawableFolder);
                await CopyEmbeddedResourceToFolder("AssetBuilder.Resources.drawable-v23." + splashBackgroundFileName, splashBackgroundFileName, drawable23Folder);

                if (await drawable23Folder.GetOrCreateFileAsync("splash_image.xml") is IStorageFile splashImageXmlFile)
                    await splashImageXmlFile.WriteAllTextAsync(vector.ToString());
                else
                {
                    await DisplayAlert(null, "Cannot open or create splash_image.xml in [" + drawable23Folder.Path + "]", "ok");
                    return vector;
                }

                try
                {
                    var width = vector.Width.As(AndroidVector.Unit.Dp);
                    var height = vector.Height.As(AndroidVector.Unit.Dp);
                    var size = new System.Drawing.Size((int)Math.Ceiling(width), (int)Math.Ceiling(height));
                    if (width > 300 || height > 300)
                    {
                        var aspect = width / height;
                        if (aspect >= 1)
                            size = new System.Drawing.Size(300, (int)Math.Ceiling(300 / aspect));
                        else
                            size = new System.Drawing.Size((int)Math.Ceiling(300 * aspect), 300);
                    }
                    if (await drawableFolder.GetOrCreateFileAsync("splash_image.png") is IStorageFile splashImagePngFile)
                    {
                        await vector.ToPngAsync(splashImagePngFile, Color.Transparent, size);
                    }
                    else
                    {
                        await DisplayAlert(null, "Cannot open or create splash_image.png in [" + drawableFolder.Path + "]", "ok");
                        return vector;
                    }

                }
                catch (Exception e)
                {
                    await DisplayAlert("SkiaSharp ERROR", "Failed to generate pre-v23 SDK Android splash image (drawable/splash_image.png) because of the following SkiaSharp exception:\n\n" + e.Message, "ok");
                    return vector;
                }

                if (_androidProjectFolderPicker.ProjectFile is IStorageFile projectFile)
                {
                    if (await XDocumentExtensions.LoadAsync(projectFile) is XDocument csprojDoc)
                    {
                        XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";
                        if (csprojDoc.Descendants(ns + "AndroidResource").FirstOrDefault(e => e.Attribute("Include")?.Value == "Resources\\values\\styles.xml") is XElement firstAndroidResource)
                        {
                            var androidResourceItemGroup = firstAndroidResource.Parent;
                            if (!androidResourceItemGroup.Elements(ns + "AndroidResource").Any(e => e.Attribute("Include")?.Value == "Resources\\drawable\\background_splash.xml"))
                            {
                                var element = new XElement(ns + "AndroidResource");
                                element.SetAttributeValue("Include", "Resources\\drawable\\background_splash.xml");
                                androidResourceItemGroup.Add(element);

                                element = new XElement(ns + "AndroidResource");
                                element.SetAttributeValue("Include", "Resources\\drawable\\splash_image.png");
                                androidResourceItemGroup.Add(element);

                                element = new XElement(ns + "AndroidResource");
                                element.SetAttributeValue("Include", "Resources\\drawable-v23\\background_splash.xml");
                                androidResourceItemGroup.Add(element);

                                element = new XElement(ns + "AndroidResource");
                                element.SetAttributeValue("Include", "Resources\\drawable-v23\\splash_image.xml");
                                androidResourceItemGroup.Add(element);

                                await StorageFileExtensions.WriteAllTextAsync(projectFile, XmlHeader + csprojDoc);
                            }
                        }

                        if (csprojDoc.Descendants(ns + "Compile").FirstOrDefault(e => e.Attribute("Include")?.Value == "MainActivity.cs") is XElement mainActivityElement)
                        {
                            var sourceItemGroup = mainActivityElement.Parent;
                            if (!sourceItemGroup.Elements(ns + "Compile").Any(e => e.Attribute("Include")?.Value == "SplashActivity.cs"))
                            {
                                var element = new XElement(ns + "Compile");
                                element.SetAttributeValue("Include", "SplashActivity.cs");
                                sourceItemGroup.Add(element);

                                await StorageFileExtensions.WriteAllTextAsync(projectFile, XmlHeader + csprojDoc);
                            }
                        }
                    }
                    else
                    {
                        await DisplayAlert(null, "Could not load Android .csproj file as an XDocument", "ok");
                        return null;
                    }

                    //var valuesFolder = Path.Combine(resourcesFolder, "values");
                    if (!(await resourcesFolder.GetFolderAsync("values") is IStorageFolder valuesFolder))
                    {
                        await DisplayAlert(null, "Cannot get values folder in [" + resourcesFolder.Path + "]", "ok");
                        return vector;
                    }

                    if (await valuesFolder.GetFileAsync("styles.xml") is IStorageFile stylesFile)
                    {
                        var stylesDocument = await XDocumentExtensions.LoadAsync(stylesFile);
                        var styles = stylesDocument.Root;
                        if (styles.Name.ToString() == "resources")
                        {
                            if (!styles.Elements("style").Any(e => e.Attribute("name")?.Value == "SplashTheme"))
                            {
                                XNamespace ns = "android";
                                var item = new XElement("item", "@drawable/background_splash");
                                item.SetAttributeValue("name", ns + "windowBackground");
                                var style = new XElement("style", item);
                                style.SetAttributeValue("name", "SplashTheme");
                                style.SetAttributeValue("parent", "Theme.AppCompat.NoActionBar");
                                styles.Add(style);

                                var text = XmlHeader + stylesDocument.ToString().Replace("{android}", "android:");
                                await StorageFileExtensions.WriteAllTextAsync(stylesFile, text);
                            }
                        }
                    }
                    else
                    {
                        await DisplayAlert(null, "Cannot open styles.xml file in [" + valuesFolder.Path + "]", "ok");
                        return vector;
                    }

                    if (await valuesFolder.GetFileAsync("colors.xml") is IStorageFile colorsFile)
                    {
                        var colorsDocument = await XDocumentExtensions.LoadAsync(colorsFile);
                        var colors = colorsDocument.Root;
                        if (colors.Name.ToString() == "resources")
                        {
                            var color = colors.Elements("color").FirstOrDefault(e => e.Attribute("name") is XAttribute name && name.Value == "splash_background");
                            if (color is null)
                            {
                                color = new XElement("color");
                                color.SetAttributeValue("name", "splash_background");
                                colors.Add(color);
                            }
                            color.Value = Preferences.Current.SplashBackgroundColor.ToHex();

                            var text = XmlHeader + colorsDocument.ToString();
                            await StorageFileExtensions.WriteAllTextAsync(colorsFile, text);
                        }
                    }
                    else
                    {
                        await DisplayAlert(null, "Cannot open colors.xml file in [" + valuesFolder.Path + "]", "ok");
                        return vector;
                    }

                    if (await _androidProjectFolderPicker.StorageFolder.GetFileAsync("MainActivity.cs") is IStorageFile mainActivityFile)
                    {
                        string namespaceLine = null;
                        var mainActivityLines = await mainActivityFile.ReadAllLinesAsync();
                        var updatedLines = new List<string>();
                        foreach (var line in mainActivityLines)
                        {
                            if (line.StartsWith("namespace"))
                                namespaceLine = line;
                            updatedLines.Add(line.Replace("MainLauncher = true", "MainLauncher = false"));
                        }
                        await mainActivityFile.WriteAllLinesAsync(updatedLines);

                        if (await _androidProjectFolderPicker.StorageFolder.GetFileAsync(splashActivityFileName) is IStorageFile splashActivityFile)
                        {
                            var splashActivityLines = await StorageFileExtensions.ReadAllLinesAsync(splashActivityFile);
                            updatedLines = new List<string>();
                            foreach (var line in splashActivityLines)
                            {
                                if (line.StartsWith("namespace"))
                                    updatedLines.Add(namespaceLine);
                                else
                                    updatedLines.Add(line);
                            }
                            await splashActivityFile.WriteAllLinesAsync(updatedLines);
                        }
                        else
                        {
                            await DisplayAlert(null, "Cannot open "+splashBackgroundFileName+" file in [" + _androidProjectFolderPicker.StorageFolder.Path + "]", "ok");
                            return vector;
                        }
                    }
                    else
                    {
                        await DisplayAlert(null, "Cannot open MainActivity.cs file in [" + _androidProjectFolderPicker.StorageFolder.Path + "]", "ok");
                        return vector;
                    }
                }
            }
            return vector;
        }
        #endregion

        #region iOS Splash Screen and Image
        async Task GenerateIosSplashScreen(AndroidVector.Vector vector)
        {
            if (_iosProjectFolderPicker.StorageFolder != null)
            {
                if (await GenerateIosSplashPdf(vector) is string err0)
                {
                    await DisplayAlert("Pdf Generation Error", err0, "ok");
                    return;
                }
                if (await UpdateIosLaunchScreenStoryboard() is string err1)
                {
                    await DisplayAlert("Update iOS LaunchScreen Storyboard Error", err1, "ok");
                    return;
                }

                if (await UpdateIosCsproj() is string err2)
                {
                    await DisplayAlert("Update iOS *.csproj Error", err2, "ok");
                    return;
                }
            }
        }

        public async Task<string> GenerateIosSplashPdf(AndroidVector.Vector vector)
        {
            if (_iosProjectFolderPicker.StorageFolder is null)
                return null;
            if (vector is null)
                return "AndroidVector was not generated and thus not available to convert to PDF for iOS LaunchImage.";

            if (!(await _iosProjectFolderPicker.StorageFolder.GetOrCreateFolderAsync("Assets.xcassets") is IStorageFolder xcassetsFolder))
                return "Cannot open Assets.xcassets folder in [" + _iosProjectFolderPicker.StorageFolder.Path + "]";

            if (await xcassetsFolder.GetOrCreateFolderAsync("Splash.imageset") is IStorageFolder splashImageSetFolder)
                await CopyEmbeddedResourceToFolder("AssetBuilder.Resources.Contents.json", "Contents.json", splashImageSetFolder);
            else
                return "Cannot open or create Splash.imageset folder in [" + xcassetsFolder.Path + "]";

            var launchImagePdfFileName = "LaunchImage.pdf";
            if (await splashImageSetFolder.GetFileAsync(launchImagePdfFileName) is IStorageFile oldLaunchImagePdfFile)
                await oldLaunchImagePdfFile.DeleteAsync();

            if (await splashImageSetFolder.CreateFileAsync(launchImagePdfFileName) is IStorageFile launchImagePdfFile)
            {
                try
                {
                    var warnings = await vector.ToPdfDocumentAsync(launchImagePdfFile, Preferences.Current.SplashBackgroundColor);
                    if (warnings!=null && warnings.Count > 0)
                        return string.Join("\n\n", warnings);
                }
                catch (Exception e)
                {
                    return "Could not generate PDF for iOS LaunchImage because of the following AndroidVector.ToPdfDocument error:\n\n" + e.Message;
                }
            }
            return null;
        }

        public async Task<string> UpdateIosLaunchScreenStoryboard()
        {
            if (_iosProjectFolderPicker.StorageFolder is null)
                return null;

            if (!(await _iosProjectFolderPicker.StorageFolder.GetOrCreateFolderAsync("Resources") is IStorageFolder resourcesFolder))
                return "Could not get or create Resources folder in iOS project folder ["+_iosProjectFolderPicker.StorageFolder+"].";
            if (!(await resourcesFolder.GetFileAsync("LaunchScreen.storyboard") is IStorageFile launchScreenStoryboardFile))
                launchScreenStoryboardFile = await CopyEmbeddedResourceToFolder("AssetBuilder.Resources.LaunchScreen.storyboard", "LaunchScreen.storyboard", resourcesFolder);

            var document = await XDocumentExtensions.LoadAsync(launchScreenStoryboardFile);
            if (document.Descendants("color") is IEnumerable<XElement> colorElements)
            {
                if (colorElements.FirstOrDefault(e=>e.Attribute("key")?.Value == "backgroundColor") is XElement backgroundColor)
                {
                    backgroundColor.SetAttributeValue("red", Preferences.Current.SplashBackgroundColor.R);
                    backgroundColor.SetAttributeValue("green", Preferences.Current.SplashBackgroundColor.G);
                    backgroundColor.SetAttributeValue("blue", Preferences.Current.SplashBackgroundColor.B);
                    backgroundColor.SetAttributeValue("alpha", Preferences.Current.SplashBackgroundColor.A);
                }
            }

            string xmlHeader = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>\n";
            var text = xmlHeader + document;
            await StorageFileExtensions.WriteAllTextAsync(launchScreenStoryboardFile, text);
            return null;
        }

        async Task<string> UpdateIosCsproj()
        {
            if (_iosProjectFolderPicker.ProjectFile is IStorageFile projectFile)
            {
                if (await XDocumentExtensions.LoadAsync(projectFile) is XDocument document)
                {
                    XNamespace ns = document.Root.Attribute("xmlns").Value;
                    foreach (var itemGroup in document.Descendants(ns + "ItemGroup"))
                    {
                        {
                            if (itemGroup.Element(ns + "InterfaceDefinition") is XElement interfaceDef
                                && interfaceDef.Attribute("Include") is XAttribute includeAttr
                                && (includeAttr.Value == "Resources\\LaunchScreen.storyboard"
                                    || includeAttr.Value == "Resources/LaunchScreen.storyboard")
                                )
                            {
                                if (itemGroup.Elements(ns + "ImageAsset").Attributes("Include").Any(a => a.Value == "Assets.xcassets\\Splash.imageset\\LaunchImage.pdf"))
                                    return null;

                                var imageAsset = new XElement(ns + "ImageAsset",
                                    new XElement(ns + "Visible", false));
                                imageAsset.SetAttributeValue("Include", "Assets.xcassets\\Splash.imageset\\Contents.json");
                                itemGroup.Add(imageAsset);

                                imageAsset = new XElement(ns + "ImageAsset",
                                    new XElement(ns + "Visible", false));
                                imageAsset.SetAttributeValue("Include", "Assets.xcassets\\Splash.imageset\\LaunchImage.pdf");
                                itemGroup.Add(imageAsset);

                                var text = XmlHeader + document;
                                await StorageFileExtensions.WriteAllTextAsync(projectFile, text);

                                return null;
                            }
                        }
                    }
                    return "could not find ItemGroup into which to inject the LaunchScreen image assets";
                }
            }
            return "could not get files from iOS project folder.";
        }
        #endregion

        #region UWP Splash
        static readonly Dictionary<string, System.Drawing.Size> UwpRectangularSplashImages = new Dictionary<string, System.Drawing.Size>
        {
            { "SplashScreen.scale-100.png", new System.Drawing.Size(620, 300) },
            { "SplashScreen.scale-120.png", new System.Drawing.Size(775, 375) },
            { "SplashScreen.scale-150.png", new System.Drawing.Size(930, 450) },
            { "SplashScreen.scale-200.png", new System.Drawing.Size(1240, 600) },
            { "SplashScreen.scale-400.png", new System.Drawing.Size(2480, 1200) },
            { "Wide310x150Logo.scale-100.png", new System.Drawing.Size(310, 150) },
            { "Wide310x150Logo.scale-120.png", new System.Drawing.Size(388, 188) },
            { "Wide310x150Logo.scale-150.png", new System.Drawing.Size(465, 225) },
            { "Wide310x150Logo.scale-200.png", new System.Drawing.Size(620, 300) },
            { "Wide310x150Logo.scale-400.png", new System.Drawing.Size(1240, 600) },
        };
        static readonly Dictionary<string, int> UwpSquareLogoImages = new Dictionary<string, int>
        {
            { "Square150x150Logo.scale-100.png", 150 },
            { "Square150x150Logo.scale-125.png", 188 },
            { "Square150x150Logo.scale-150.png", 225 },
            { "Square150x150Logo.scale-200.png", 300 },
            { "Square150x150Logo.scale-400.png", 600 },
            { "Square310x310Logo.scale-100.png", 310 },
            { "Square310x310Logo.scale-125.png", 388 },
            { "Square310x310Logo.scale-150.png", 465 },
            { "Square310x310Logo.scale-200.png", 620 },
            { "Square310x310Logo.scale-400.png", 1240 },
            { "LargeTile.scale-100.png", 310 },
            { "LargeTile.scale-125.png", 388 },
            { "LargeTile.scale-150.png", 465 },
            { "LargeTile.scale-200.png", 620 },
            { "LargeTile.scale-400.png", 1240 },
        };

        async Task<AndroidVector.Vector> GenerateUwpSplashAndLogoImages()
        {
            AndroidVector.Vector mobileVector = null;
            if (await GenerateUwpRectangleSplashAndLogoImages() is AndroidVector.Vector squareVector && Preferences.Current.MobileSplashSource == MobileSplashSource.Rect310)
                mobileVector = squareVector;
            if (await GenerateUwpSquareSplashAndLogoImages() is AndroidVector.Vector rectVector && Preferences.Current.MobileSplashSource == MobileSplashSource.Square)
                mobileVector = rectVector;

            if (_uwpProjectFolderPicker.ProjectFile is IStorageFile projectFile)
            {
                if (await XDocumentExtensions.LoadAsync(projectFile) is XDocument document)
                {
                    XNamespace ns = document.Root.Attribute("xmlns").Value;
                    var found = false;
                    foreach (var itemGroup in document.Descendants(ns + "ItemGroup"))
                    {
                        if (itemGroup.Elements(ns + "Content").Any(e => e.Attribute("Include").Value == "Assets\\LargeTile.scale-100.png"))
                        {
                            found = true;
                            foreach (var kvp in UwpRectangularSplashImages)
                            {
                                if (!itemGroup.Elements(ns + "Content").Any(e => e.Attribute("Include").Value == "Assets\\" + kvp.Key))
                                    itemGroup.Add(new XElement(ns + "Content", new XAttribute("Include", "Assets\\" + kvp.Key)));
                            }
                            foreach (var kvp in UwpSquareLogoImages)
                            {
                                if (!itemGroup.Elements(ns + "Content").Any(e => e.Attribute("Include").Value == "Assets\\" + kvp.Key))
                                    itemGroup.Add(new XElement(ns + "Content", new XAttribute("Include", "Assets\\" + kvp.Key)));
                            }


                            var text = XmlHeader + document;
                            await StorageFileExtensions.WriteAllTextAsync(projectFile, text);
                            break;
                        }
                    }
                    if (!found)
                        await DisplayAlert("UWP Splash Images", "Could not find <ItemGroup>, in UWP's .csproj file, that contains icon, logo, and splash images", "ok");
                }
            }
            return mobileVector;
        }

        async Task<AndroidVector.Vector> GenerateUwpSquareSplashAndLogoImages()
        {
            if (_squareSvgLaunchImagePicker.StorageFile is null)
                return null;

            if (await Svg2.GenerateAndroidVectorAsync(_squareSvgLaunchImagePicker.StorageFile) is (AndroidVector.Vector vector, List<string> warnings))
            {
                if (warnings.Count > 0 && Preferences.Current.MobileSplashSource != MobileSplashSource.Square)
                    await DisplayAlert("Square Splash Image Warnings", string.Join("\n\n", warnings), "ok");

                if (_uwpProjectFolderPicker.StorageFolder != null)
                {
                    if (await _uwpProjectFolderPicker.StorageFolder.GetOrCreateFolderAsync("Assets") is IStorageFolder assetsFolder)
                    {
                        foreach (var kvp in UwpSquareLogoImages)
                        {
                            var file = await assetsFolder.GetOrCreateFileAsync(kvp.Key);
                            await vector.ToPngAsync(file, Color.Transparent, new System.Drawing.Size(kvp.Value, kvp.Value));
                        }
                    }
                    else
                        await DisplayAlert("ERROR", "Failed to generate UWP Square Splash images because could not find Assets folder in UWP project folder ["+_uwpProjectFolderPicker.StorageFolder.Path+"].", "ok");
                }
            }
            else
            {
                await DisplayAlert("ERROR", "Failed to generate UWP Square Splash images for unknown reason.", "ok");
                vector = null;
            }
            return vector;
        }

        async Task<AndroidVector.Vector> GenerateUwpRectangleSplashAndLogoImages()
        {
            if (_rect310SvgLaunchImagePicker.StorageFile is null)
                return null;


            if (await Svg2.GenerateAndroidVectorAsync(_rect310SvgLaunchImagePicker.StorageFile) is (AndroidVector.Vector vector, List<string> warnings))
            {
                if (warnings.Count > 0 && Preferences.Current.MobileSplashSource != MobileSplashSource.Square)
                    await DisplayAlert("Rectangle Splash Image Warnings", string.Join("\n\n", warnings), "ok");

                if (_uwpProjectFolderPicker.StorageFolder != null)
                {
                    if (await _uwpProjectFolderPicker.StorageFolder.GetOrCreateFolderAsync("Assets") is IStorageFolder assetsFolder)
                    {
                        foreach (var kvp in UwpRectangularSplashImages)
                        { 
                            var file = await assetsFolder.GetOrCreateFileAsync(kvp.Key);
                            await vector.ToPngAsync(file, Color.Transparent, kvp.Value);
                        }
                    }
                    else
                        await DisplayAlert("ERROR", "Failed to generate UWP rectangular Splash images because could not find Assets folder in UWP project folder [" + _uwpProjectFolderPicker.StorageFolder.Path + "].", "ok");
                }

            }
            else
            {
                await DisplayAlert("ERROR", "Failed to generate UWP rectangular Splash images for unknown reason.", "ok");
                vector = null;
            }
            return vector;
        }
        #endregion

        #endregion

        #endregion



    }
}
