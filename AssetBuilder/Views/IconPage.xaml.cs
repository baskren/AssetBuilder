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

namespace AssetBuilder.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class IconPage : ContentPage
    {
        const string XmlHeader = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n";

        public IconPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            Preferences.Current.PropertyChanged += OnPreferencesChanged;

            _iconSvgFileEntry.TextChanged += OnIconSvgFileChanged;
            _iosProjectFolderEntry.TextChanged += OnIosProjectFolderChanged;
            _androidProjectFolderEntry.TextChanged += OnAndroidProjectFolderChanged;
            _uwpProjectFolderEntry.TextChanged += OnUwpProjectFolderChanged;
            _squareSvgLaunchImageEntry.TextChanged += OnSquareSvgLaunchImageEntryChanged;
            _rect310SvgLaunchImageEntry.TextChanged += OnRect310SvgLaunchImageEntryChanged;

            _mobileUseSquareSplashImageCheckBox.CheckedChanged += MobileSourceCheckBoxChanged;
            _mobileUseRect310SplashImageCheckBox.CheckedChanged += MobileSourceCheckBoxChanged;

            _iconSvgFileEntry.Text = Preferences.Current.SvgIconFile;
            _iosProjectFolderEntry.Text = Preferences.Current.IosOProjectFolder;
            _androidProjectFolderEntry.Text = Preferences.Current.AndroidProjectFolder;
            _uwpProjectFolderEntry.Text = Preferences.Current.UwpProjectFolder;
            _splashPageBackgroundColorEntry.Text = Preferences.Current.SplashBackgroundColor.ToHex();
            _iconBackgroundColorEntry.Text = Preferences.Current.IconBackgroundColor.ToHex();
            _squareSvgLaunchImageEntry.Text = Preferences.Current.SquareSvgSplashImageFile;
            _rect310SvgLaunchImageEntry.Text = Preferences.Current.Rect310SvgSplashImageFile;

            UpdateMobileSplashSource();

            UpdateButtonAbility();

            OnPreferencesChanged(this, new PropertyChangedEventArgs(nameof(Preferences.SplashBackgroundColor)));
            OnPreferencesChanged(this, new PropertyChangedEventArgs(nameof(Preferences.IconBackgroundColor)));
            OnPreferencesChanged(this, new PropertyChangedEventArgs(nameof(Preferences.AndroidProjectFolder)));
            OnPreferencesChanged(this, new PropertyChangedEventArgs(nameof(Preferences.IosOProjectFolder)));
            OnPreferencesChanged(this, new PropertyChangedEventArgs(nameof(Preferences.UwpProjectFolder)));
        }

        private void MobileSourceCheckBoxChanged(object sender, CheckedChangedEventArgs e)
        {
            if (_mobileUseSquareSplashImageCheckBox.IsChecked)
                Preferences.Current.MobileSplashSource = MobileSplashSource.Square;
            else if (_mobileUseRect310SplashImageCheckBox.IsChecked)
                Preferences.Current.MobileSplashSource = MobileSplashSource.Rect310;
            //else if (_mobileUseRect620SplashImageCheckBox.IsChecked)
            //    Preferences.Current.MobileSplashSource = MobileSplashSource.Rect620;
            else
                Preferences.Current.MobileSplashSource = MobileSplashSource.None;
        }

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



        #region Entry event Handlers

        #region Project Folders
        string GetProjectFile(string projectFolderPath, Xamarin.Essentials.DevicePlatform platform)
        {
            if (string.IsNullOrWhiteSpace(projectFolderPath))
                return null;
            var osName = platform.ToString();
            if (Directory.GetFiles(projectFolderPath, "*.csproj") is string[] files)
            {
                if (!Directory.Exists(projectFolderPath))
                {
                    DisplayAlert(osName + " Project Folder", "[" + projectFolderPath + "] failed Directory.Exists test.", "ok");
                    return null;
                }
                if (files.Length > 1)
                {
                    DisplayAlert(osName + " Project Folder", "multiple .csproj files found in the "+osName+" project folder", "ok");
                    return  null;
                }
                if (files.Length < 1)
                {
                    DisplayAlert(osName + " Project Folder", "no .csproj file found in the " + osName + " project folder", "ok");
                    return null;
                }
                var fileName = files[0];
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    DisplayAlert(osName + " Project Folder", "invalid filename for " + osName + " .csproj file", "ok");
                    return null;
                }
                if (!File.Exists(fileName))
                {
                    DisplayAlert(osName + " Project Folder", "candidate for " + osName + " project file [" + fileName + "] does not exist.", "ok");
                    return null;
                }
                var text = File.ReadAllText(fileName);
                if (string.IsNullOrWhiteSpace(text))
                {
                    DisplayAlert(osName + " Project Folder", "candidate for " + osName + " project file [" + fileName + "] is empty.", "ok");
                    return null;
                }
                var outputType = "";
                var targetType = "";
                switch (osName)
                {
                    case nameof(Xamarin.Essentials.DevicePlatform.iOS):
                        outputType = "<OutputType>Exe</OutputType>";
                        targetType = "\\Xamarin\\iOS\\Xamarin.iOS.CSharp.targets";
                        break;
                    case nameof(Xamarin.Essentials.DevicePlatform.Android):
                        outputType = "<OutputType>Library</OutputType>";
                        targetType = "\\Xamarin\\Android\\Xamarin.Android.CSharp.targets";
                        break;
                    case nameof(Xamarin.Essentials.DevicePlatform.UWP):
                        outputType = "<OutputType>AppContainerExe</OutputType>";
                        targetType = "Microsoft.Windows.UI.Xaml.CSharp.targets";
                        break;
                    default:
                        DisplayAlert(osName + " Project Folder", "Unsupported DevicePlatform [" + osName + "]", "ok");
                        return null;
                }
                if (!text.Contains(outputType) || !text.Contains(targetType))
                    DisplayAlert(osName + " Project Folder", "This folder's project file [" + fileName + "] doesn't appear to be an "+osName+" executable app project file.", "ok");
                else
                    return fileName;
            }
            return null;
        }

        bool IsSvgFile(string svgFilePath)
        {
            if (string.IsNullOrWhiteSpace(svgFilePath))
                return false;
            if (!File.Exists(svgFilePath))
            {
                DisplayAlert("SVG File", "File [" + svgFilePath + "] failed File.Exists test.", "ok");
                return false;
            }
            var text = File.ReadAllText(svgFilePath);
            if (string.IsNullOrWhiteSpace(text))
            {
                DisplayAlert("SVG File", "File [" + svgFilePath + "] is empty.", "ok");
                return false;
            }
            if (!text.Contains("<svg") || !text.Contains("http://www.w3.org/2000/svg"))
            {
                DisplayAlert("SVG File", "File [" + svgFilePath + "] doesn't appear to have SVG content.", "ok");
                return false;
            }
            return true;
        }

        private void OnIosProjectFolderChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                Preferences.Current.IosOProjectFolder = null;
                return;
            }
            if (GetProjectFile(e.NewTextValue, Xamarin.Essentials.DevicePlatform.iOS) is string fileName)
                Preferences.Current.IosOProjectFolder = e.NewTextValue.Trim();
            else
                _iosProjectFolderEntry.Text = e.OldTextValue;
        }

        private void OnAndroidProjectFolderChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                Preferences.Current.AndroidProjectFolder = null;
                return;
            }
            if (GetProjectFile(e.NewTextValue, Xamarin.Essentials.DevicePlatform.Android) is string fileName)
                Preferences.Current.AndroidProjectFolder = e.NewTextValue.Trim();
            else
                _androidProjectFolderEntry.Text = e.OldTextValue;
        }

        private void OnUwpProjectFolderChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                Preferences.Current.UwpProjectFolder = null;
                return;
            }
            if (GetProjectFile(e.NewTextValue, Xamarin.Essentials.DevicePlatform.UWP) is string fileName)
                Preferences.Current.UwpProjectFolder = e.NewTextValue.Trim();
            else
                _uwpProjectFolderEntry.Text = e.OldTextValue;
        }
        #endregion


        #region SVG Files
        private void OnIconSvgFileChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                Preferences.Current.SvgIconFile = null;
                return;
            }
            if (IsSvgFile(e.NewTextValue))
                Preferences.Current.SvgIconFile = e.NewTextValue;
            else
                _iconSvgFileEntry.Text = e.OldTextValue;
        }

        private void OnSquareSvgLaunchImageEntryChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                Preferences.Current.SquareSvgSplashImageFile = null;
                _mobileUseSquareSplashImageCheckBox.IsEnabled = false;
                _mobileUseSquareSplashImageCheckBox.IsChecked = false;
                return;
            }
            if (IsSvgFile(e.NewTextValue))
            {
                Preferences.Current.SquareSvgSplashImageFile = e.NewTextValue;
                _mobileUseSquareSplashImageCheckBox.IsEnabled = true;
                _mobileUseSquareSplashImageCheckBox.IsChecked = string.IsNullOrWhiteSpace(Preferences.Current.Rect310SvgSplashImageFile); // && string.IsNullOrWhiteSpace(Preferences.Current.Rect620SvgSplashImageFile);
            }
            else
                _squareSvgLaunchImageEntry.Text = e.OldTextValue;
            UpdateMobileSplashSource();
        }

        private void OnRect310SvgLaunchImageEntryChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                Preferences.Current.Rect310SvgSplashImageFile = null;
                _mobileUseRect310SplashImageCheckBox.IsEnabled = false;
                _mobileUseRect310SplashImageCheckBox.IsChecked = false;
                return;
            }
            if (IsSvgFile(e.NewTextValue))
            {
                Preferences.Current.Rect310SvgSplashImageFile = e.NewTextValue;
                _mobileUseRect310SplashImageCheckBox.IsEnabled = true;
                _mobileUseRect310SplashImageCheckBox.IsChecked = string.IsNullOrWhiteSpace(Preferences.Current.SquareSvgSplashImageFile);// && string.IsNullOrWhiteSpace(Preferences.Current.Rect620SvgSplashImageFile);
            }
            else
                _rect310SvgLaunchImageEntry.Text = e.OldTextValue;
        }
        #endregion

        #endregion


        #region methods
        void UpdateButtonAbility()
        {
            _generateIconsButton.IsEnabled = Preferences.Current.IsIconEnabled;
            _generateLaunchImagesButton.IsEnabled = Preferences.Current.IsSplashEnabled;
        }

        bool firstUpdateMobileSplashSource = true;
        void UpdateMobileSplashSource()
        {
            if (!firstUpdateMobileSplashSource)
            {
                if (string.IsNullOrWhiteSpace(Preferences.Current.SquareSvgSplashImageFile))
                {
                    Preferences.Current.SquareSvgSplashImageFile = null;
                    _mobileUseSquareSplashImageCheckBox.IsEnabled = false;
                    _mobileUseSquareSplashImageCheckBox.IsChecked = false;
                }
                if (string.IsNullOrWhiteSpace(Preferences.Current.Rect310SvgSplashImageFile))
                {
                    Preferences.Current.Rect310SvgSplashImageFile = null;
                    _mobileUseRect310SplashImageCheckBox.IsEnabled = false;
                    _mobileUseRect310SplashImageCheckBox.IsChecked = false;
                }
            }

            _mobileUseSquareSplashImageCheckBox.IsChecked = Preferences.Current.MobileSplashSource == MobileSplashSource.Square;
            _mobileUseRect310SplashImageCheckBox.IsChecked = Preferences.Current.MobileSplashSource == MobileSplashSource.Rect310;
        }

        string MobileSplashSvg
        {
            get
            {
                if (Preferences.Current.MobileSplashSource == MobileSplashSource.Square)
                    return Preferences.Current.SquareSvgSplashImageFile;
                else if (Preferences.Current.MobileSplashSource == MobileSplashSource.Rect310)
                    return Preferences.Current.Rect310SvgSplashImageFile;
                else
                    return null;
            }
        }

        async void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
        {
            if (sender is Xamarin.Forms.Frame frame)
            {
                if (frame == _splashPageBackgroundColorFrame)
                    Preferences.Current.SplashBackgroundColor = await ColorPickerDialog.Show(Content as Grid, "Splash Screen Background", Preferences.Current.SplashBackgroundColor, null);
                else if (frame == _iconBackgroundColorFrame)
                    Preferences.Current.IconBackgroundColor = await ColorPickerDialog.Show(Content as Grid, "Icon Background", Preferences.Current.IconBackgroundColor, null);
            }
            else if (sender is AssetBuilder.Label label)
            {
                //                await DisplayAlert(null, label.Placeholder.ToString(), "ok");
                if (label == _androidProjectFolderEntry || label == _iosProjectFolderEntry || label == _uwpProjectFolderEntry)
                {
                    if (await P42.Storage.Pickers.PickSingleFolderAsync()  is P42.Storage.IStorageFolder folder)
                        label.Text = folder.Path;
                }
                else if (await P42.Storage.Pickers.PickSingleFileAsync() is P42.Storage.IStorageFile fileData)
                {
                    label.Text = fileData.Path;
                }
            }
            else if (sender is Xamarin.Forms.Label xfLabel)
            {
                if (xfLabel == _uwpProjectFolderClearButton)
                    _uwpProjectFolderEntry.Text = null;
                else if (xfLabel == _androidProjectFolderClearButton)
                    _androidProjectFolderEntry.Text = null;
                else if (xfLabel == _iosProjectFolderClearButton)
                    _iosProjectFolderEntry.Text = null;
                else if (xfLabel == _iconSvgFileClearButton)
                    _iconSvgFileEntry.Text = null;
                else if (xfLabel == _squareSvgLaunchImageClearButton)
                    _squareSvgLaunchImageEntry.Text = null;
                else if (xfLabel == _rect310SvgLaunchImageClearButton)
                    _rect310SvgLaunchImageEntry.Text = null;
                //else if (xfLabel == _rect620SvgLaunchImageClearButton)
                //    _rect620SvgLaunchImageEntry.Text = null;
            }
        }


        #region Generate Icons
        void OnGenerateIconsButtonClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Preferences.Current.SvgIconFile)
                || !File.Exists(Preferences.Current.SvgIconFile))
            {
                DisplayAlert(null, "Cannot open SVG file [" + Preferences.Current.SvgIconFile + "]", "cancel");
                return;
            }

            var vector = GenerateVectorAndroidIcons(Preferences.Current.SvgIconFile);
            GenerateRasterAndroidIcons(vector);
            GenerateIosIcons(vector);
            GenerateUwpIcons(vector);

            DisplayAlert("Complate", "App Icons have been generated.", "ok");
        }

        AndroidVector.Vector GenerateVectorAndroidIcons(string svgPath)
        {
            if (Svg2.GenerateAndroidVector(svgPath) is (AndroidVector.Vector vector, List<string> warnings))
            {
                if (warnings.Count > 0)
                    DisplayAlert("Warnings", string.Join("\n\n", warnings), "ok");

                vector = vector.AspectClone();
            }
            else
            {
                DisplayAlert("ERROR", "Failed to generate AndroidVector for unknown reason.", "ok");
                vector = null;
            }

            if (!string.IsNullOrWhiteSpace(Preferences.Current.AndroidProjectFolder))
            {
                var resourcesFolder = Path.Combine(Preferences.Current.AndroidProjectFolder, "Resources");
                if (!Directory.Exists(resourcesFolder))
                    Directory.CreateDirectory(resourcesFolder);
                var mipmapFolder = Path.Combine(resourcesFolder, "mipmap-anydpi-v26");
                if (!Directory.Exists(mipmapFolder))
                    Directory.CreateDirectory(mipmapFolder);

                if (GetProjectFile(Preferences.Current.AndroidProjectFolder, Xamarin.Essentials.DevicePlatform.Android) is string projectFilePath)
                {
                    if (XDocument.Load(projectFilePath) is XDocument csprojDoc)
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

                                File.WriteAllText(projectFilePath, XmlHeader + csprojDoc);
                            }
                        }

                    }
                    else
                    {
                        DisplayAlert(null, "Could not load Android .csproj file as an XDocument", "ok");
                        return null;
                    }

                    var valuesFolder = Path.Combine(resourcesFolder, "values");
                    var colorsPath = Path.Combine(valuesFolder, "colors.xml");
                    var colorsDocument = XDocument.Load(colorsPath);
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
                        File.WriteAllText(colorsPath, text);
                    }


                    if (vector != null)
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
                        File.WriteAllText(Path.Combine(mipmapFolder, "launcher_foreground.xml"), tmpVector.ToString());
                    }
                }
            }
            return vector;
        }

        void GenerateRasterAndroidIcons(AndroidVector.Vector vector)
        {
            if (string.IsNullOrWhiteSpace(Preferences.Current.AndroidProjectFolder))
                return;

            if (!Directory.Exists(Preferences.Current.AndroidProjectFolder))
            {
                DisplayAlert(null, "Invaid Android Project Folder", "ok");
                return;
            }
            var dest = Path.Combine(new string[] { Preferences.Current.AndroidProjectFolder, "Resources" });
            if (!Directory.Exists(dest))
            {
                DisplayAlert(null, "Cannot find Android Resources folder [" + dest + "]", "ok");
                return;
            }
            foreach (var folder in Directory.GetDirectories(dest))
            {
                foreach (var path in Directory.GetFiles(folder))
                {
                    if (Path.GetFileName(path).ToLower() == "icon.png")
                    {
                        int size = 0;
                        using (var inputStream = File.OpenRead(path))
                        {
                            var bitmap = SKBitmap.Decode(inputStream);
                            if (bitmap.Width == bitmap.Height && bitmap.Width > 10)
                                size = bitmap.Width;
                        }
                        if (size > 0)
                            vector.ToPng(path, Preferences.Current.IconBackgroundColor, new System.Drawing.Size(size, size));
                    }
                    if (Path.GetFileName(path).ToLower() == "launcher_foreground.png")
                    {
                        int size = 0;
                        using (var inputStream = File.OpenRead(path))
                        {
                            var bitmap = SKBitmap.Decode(inputStream);
                            if (bitmap.Width == bitmap.Height && bitmap.Width > 10)
                                size = bitmap.Width;
                        }
                        if (size > 0)
                            vector.ToPng(path, Preferences.Current.IconBackgroundColor, new System.Drawing.Size(size * 2 / 3, size * 2 / 3), new System.Drawing.Size(size, size));
                    }
                }
            }
        }

        void GenerateIosIcons(AndroidVector.Vector vector)
        {
            if (string.IsNullOrWhiteSpace(Preferences.Current.IosOProjectFolder))
                return;

            if (!Directory.Exists(Preferences.Current.IosOProjectFolder))
            {
                DisplayAlert(null, "Invaid iOS Project Folder", "ok");
                return;
            }
            var dest = Path.Combine(new string[] { Preferences.Current.IosOProjectFolder, "Assets.xcassets", "AppIcon.appiconset" });
            if (!Directory.Exists(dest))
            {
                DisplayAlert(null, "Cannot find iOS icons folder [" + dest + "]", "ok");
                return;
            }

            foreach (var path in Directory.GetFiles(dest))
            {
                if (Path.GetExtension(path).ToLower() == ".png"
                    && Path.GetFileNameWithoutExtension(path).ToLower().StartsWith("icon")
                    && int.TryParse(Path.GetFileNameWithoutExtension(path).Substring(4), out int size)
                    )
                {
                    vector.ToPng(path, Preferences.Current.IconBackgroundColor, new System.Drawing.Size(size, size));
                }
            }
        }

        static Dictionary<string, int> UwpIcons = new Dictionary<string, int>
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

        void GenerateUwpIcons(AndroidVector.Vector vector)
        {
            if (string.IsNullOrWhiteSpace(Preferences.Current.UwpProjectFolder))
                return;

            if (!Directory.Exists(Preferences.Current.UwpProjectFolder))
            {
                DisplayAlert(null, "Invaid UWP Project Folder", "ok");
                return;
            }
            var dest = Path.Combine(new string[] { Preferences.Current.UwpProjectFolder, "Assets" });
            if (!Directory.Exists(dest))
            {
                DisplayAlert(null, "Cannot find UWP Assets folder [" + dest + "]", "ok");
                return;
            }

            foreach (var kvp in UwpIcons)
            {
                var path = Path.Combine(dest,kvp.Key);
                var size = kvp.Value;
                vector.ToPng(path, Preferences.Current.IconBackgroundColor, new System.Drawing.Size(size, size));
            }
        }

        #endregion


        #region Generate Splash Screens and Images
        void OnGenerateLaunchImageButtonClicked(object sender, EventArgs e)
        {
                
            if (string.IsNullOrWhiteSpace(MobileSplashSvg) || !File.Exists(MobileSplashSvg))
            {
                DisplayAlert(null, "Cannot open SVG file [" + MobileSplashSvg + "]", "cancel");
                return;
            }

            GenerateIosSplashScreen(GenerateAndroidSpashImage(GenerateUwpSplashAndLogoImages()));

             DisplayAlert("Complete", "Launch Screens have been generated.", "ok");
        }

        #region Android
        AndroidVector.Vector GenerateAndroidSpashImage(AndroidVector.Vector vector)
        {
            if (vector != null && !string.IsNullOrWhiteSpace(Preferences.Current.AndroidProjectFolder))
            {

                var resourcesFolder = Path.Combine(Preferences.Current.AndroidProjectFolder, "Resources");
                if (!Directory.Exists(resourcesFolder))
                    Directory.CreateDirectory(resourcesFolder);
                var drawableFolder = Path.Combine(resourcesFolder, "drawable");
                if (!Directory.Exists(drawableFolder))
                    Directory.CreateDirectory(drawableFolder);
                var drawable23Folder = Path.Combine(resourcesFolder, "drawable-v23");
                if (!Directory.Exists(drawable23Folder))
                    Directory.CreateDirectory(drawable23Folder);

                var splashActivityFileName = "SplashActivity.cs";
                var splashActivityPath = Path.Combine(Preferences.Current.AndroidProjectFolder, splashActivityFileName);
                if (!File.Exists(splashActivityPath))
                    GetType().Assembly.TryCopyResource("AssetBuilder.Resources." + splashActivityFileName, splashActivityPath);

                var splashBackgroundFileName = "background_splash.xml";
                var splashBackgroundPath = Path.Combine(drawableFolder, splashBackgroundFileName);
                if (!File.Exists(splashBackgroundPath))
                    GetType().Assembly.TryCopyResource("AssetBuilder.Resources.drawable." + splashBackgroundFileName, splashBackgroundPath);
                var v23splashBackgroundPath = Path.Combine(drawable23Folder, splashBackgroundFileName);
                if (!File.Exists(v23splashBackgroundPath))
                    GetType().Assembly.TryCopyResource("AssetBuilder.Resources.drawable-v23." + splashBackgroundFileName, v23splashBackgroundPath);


                File.WriteAllText(Path.Combine(drawable23Folder, "splash_image.xml"), vector.ToString());

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
                    vector.ToPng(Path.Combine(drawableFolder, "splash_image.png"), Color.Transparent, size);
                }
                catch (Exception e)
                {
                    DisplayAlert("SkiaSharp ERROR", "Failed to generate pre-v23 SDK Android splash image (drawable/splash_image.png) because of the following SkiaSharp exception:\n\n" + e.Message, "ok");
                }

                if (GetProjectFile(Preferences.Current.AndroidProjectFolder, Xamarin.Essentials.DevicePlatform.Android) is string projectFilePath)
                {
                    if (XDocument.Load(projectFilePath) is XDocument csprojDoc)
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


                                File.WriteAllText(projectFilePath, XmlHeader + csprojDoc);
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

                                File.WriteAllText(projectFilePath, XmlHeader + csprojDoc);
                            }
                        }
                    }
                    else
                    {
                        DisplayAlert(null, "Could not load Android .csproj file as an XDocument", "ok");
                        return null;
                    }

                    var valuesFolder = Path.Combine(resourcesFolder, "values");
                    var stylesPath = Path.Combine(valuesFolder, "styles.xml");
                    var stylesDocument = XDocument.Load(stylesPath);
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
                            File.WriteAllText(stylesPath, text);
                        }
                    }

                    var colorsPath = Path.Combine(valuesFolder, "colors.xml");
                    var colorsDocument = XDocument.Load(colorsPath);
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
                        File.WriteAllText(colorsPath, text);
                    }

                    //DisplayAlert(null, "Don't forget to set MainLauncher=\"false\" in " + Path.Combine(Preferences.Current.AndroidProjectFolder + "MainActivity.cs") + ".", "ok");
                    string namespaceLine = null;
                    var mainActivityPath = Path.Combine(Preferences.Current.AndroidProjectFolder, "MainActivity.cs");
                    var mainActivityLines = File.ReadAllLines(mainActivityPath);
                    var updatedLines = new List<string>();
                    foreach (var line in mainActivityLines)
                    {
                        if (line.StartsWith("namespace"))
                            namespaceLine = line;
                        updatedLines.Add(line.Replace("MainLauncher = true", "MainLauncher = false"));
                    }
                    File.WriteAllLines(mainActivityPath, updatedLines);

                    var splashActivityLines = File.ReadAllLines(splashActivityPath);
                    updatedLines = new List<string>();
                    foreach (var line in splashActivityLines)
                    {
                        if (line.StartsWith("namespace"))
                            updatedLines.Add(namespaceLine);
                        else
                            updatedLines.Add(line);
                    }
                    File.WriteAllLines(splashActivityPath, updatedLines);
                }
            }
            return vector;
        }
        #endregion

        #region iOS Splash Screen and Image
        void GenerateIosSplashScreen(AndroidVector.Vector vector)
        {
            if (!string.IsNullOrWhiteSpace(Preferences.Current.IosOProjectFolder))
            {
                if (GenerateIosSplashPdf(vector) is string err0)
                {
                    DisplayAlert("Pdf Generation Error", err0, "ok");
                    return;
                }
                if (UpdateIosLaunchScreenStoryboard() is string err1)
                {
                    DisplayAlert("Update iOS LaunchScreen Storyboard Error", err1, "ok");
                    return;
                }

                if (UpdateIosCsproj() is string err2)
                {
                    DisplayAlert("Update iOS *.csproj Error", err2, "ok");
                    return;
                }
            }
        }

        public string GenerateIosSplashPdf(AndroidVector.Vector vector)
        {
            if (vector is null)
                return "AndroidVector was not generated and thus not available to convert to PDF for iOS LaunchImage.";

            if (string.IsNullOrWhiteSpace(Preferences.Current.IosOProjectFolder) || !Directory.Exists(Preferences.Current.IosOProjectFolder))
                return "Invalid iOS Project Folder";

            var destDir = Path.Combine(new string[] { Preferences.Current.IosOProjectFolder, "Assets.xcassets", "Splash.imageset" });
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
                GetType().Assembly.TryCopyResource("AssetBuilder.Resources.Contents.json", Path.Combine(destDir, "Contents.json"));
            }

            var pdfPath = Path.Combine(destDir, "LaunchImage.pdf");
            if (File.Exists(pdfPath))
                File.Delete(pdfPath);

            try
            {
                vector.ToPdfDocument(pdfPath, Preferences.Current.SplashBackgroundColor);
            }
            catch (Exception e)
            {
                return "Could not generate PDF for iOS LaunchImage because of the following AndroidVector.ToPdfDocument error:\n\n" + e.Message;
            }

            return null;
        }

        public string UpdateIosLaunchScreenStoryboard()
        {
            var resourcesFolder = Path.Combine(Preferences.Current.IosOProjectFolder, "Resources");
            var launchScreenPath = Path.Combine(resourcesFolder, "LaunchScreen.storyboard");
            GetType().Assembly.TryCopyResource("AssetBuilder.Resources.LaunchScreen.storyboard", launchScreenPath);

            var document = XDocument.Load("file://" + launchScreenPath);
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
             File.WriteAllText(launchScreenPath, text);
            return null;
        }

        public string UpdateIosCsproj()
        {
            if (GetProjectFile(Preferences.Current.IosOProjectFolder, Xamarin.Essentials.DevicePlatform.iOS) is string projectFilePath)
            {
                if (XDocument.Load(projectFilePath) is XDocument document)
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
                                File.WriteAllText(projectFilePath, text);

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
        static Dictionary<string, System.Drawing.Size> UwpRectangularSplashImages = new Dictionary<string, System.Drawing.Size>
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
        static Dictionary<string, int> UwpSquareLogoImages = new Dictionary<string, int>
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

        AndroidVector.Vector GenerateUwpSplashAndLogoImages()
        {
            AndroidVector.Vector mobileVector = null;
            if (GenerateUwpRectangleSplashAndLogoImages() is AndroidVector.Vector squareVector && Preferences.Current.MobileSplashSource == MobileSplashSource.Rect310)
                mobileVector = squareVector;
            if (GenerateUwpSquareSplashAndLogoImages() is AndroidVector.Vector rectVector && Preferences.Current.MobileSplashSource == MobileSplashSource.Square)
                mobileVector = rectVector;

            if (GetProjectFile(Preferences.Current.UwpProjectFolder, Xamarin.Essentials.DevicePlatform.UWP) is string projectFilePath)
            {
                if (XDocument.Load(projectFilePath) is XDocument document)
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
                            File.WriteAllText(projectFilePath, text);
                            break;
                        }
                    }
                    if (!found)
                        DisplayAlert("UWP Splash Images", "Could not find <ItemGroup>, in UWP's .csproj file, that contains icon, logo, and splash images", "ok");
                }
            }
            return mobileVector;
        }

        AndroidVector.Vector GenerateUwpSquareSplashAndLogoImages()
        {
            if (string.IsNullOrWhiteSpace(Preferences.Current.SquareSvgSplashImageFile))
                return null;

            if (Svg2.GenerateAndroidVector(Preferences.Current.SquareSvgSplashImageFile) is (AndroidVector.Vector vector, List<string> warnings))
            {
                if (warnings.Count > 0 && Preferences.Current.MobileSplashSource != MobileSplashSource.Square)
                    DisplayAlert("Square Splash Image Warnings", string.Join("\n\n", warnings), "ok");

                if (!string.IsNullOrWhiteSpace(Preferences.Current.UwpProjectFolder))
                {
                    var folder = Path.Combine(Preferences.Current.UwpProjectFolder, "Assets");
                    foreach (var kvp in UwpSquareLogoImages)
                        vector.ToPng(Path.Combine(folder, kvp.Key), Color.Transparent, new System.Drawing.Size(kvp.Value, kvp.Value));
                }
            }
            else
            {
                DisplayAlert("ERROR", "Failed to generate UWP Square Splash images for unknown reason.", "ok");
                vector = null;
            }
            return vector;
        }

        AndroidVector.Vector GenerateUwpRectangleSplashAndLogoImages()
        {
            if (string.IsNullOrWhiteSpace(Preferences.Current.Rect310SvgSplashImageFile))
                return null;

            if (Svg2.GenerateAndroidVector(Preferences.Current.Rect310SvgSplashImageFile) is (AndroidVector.Vector vector, List<string> warnings))
            {
                if (warnings.Count > 0 && Preferences.Current.MobileSplashSource != MobileSplashSource.Square)
                    DisplayAlert("Rectangle Splash Image Warnings", string.Join("\n\n", warnings), "ok");

                if (!string.IsNullOrWhiteSpace(Preferences.Current.UwpProjectFolder))
                {
                    var folder = Path.Combine(Preferences.Current.UwpProjectFolder, "Assets");
                    foreach (var kvp in UwpRectangularSplashImages)
                        vector.ToPng(Path.Combine(folder, kvp.Key), Color.Transparent, kvp.Value);
                }
            }
            else
            {
                DisplayAlert("ERROR", "Failed to generate UWP Square Splash images for unknown reason.", "ok");
                vector = null;
            }
            return vector;
        }
        #endregion

        #endregion

        #endregion


    }
}
