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
        public IconPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _iconSvgFileEntry.Text = Preferences.Current.SvgIconFile;
            _iosProjectFolderEntry.Text = Preferences.Current.IosOProjectFolder;
            _androidProjectFolderEntry.Text = Preferences.Current.AndroidProjectFolder;
            _uwpProjectFolderEntry.Text = Preferences.Current.UwpProjectFolder;
            _squareSvgLaunchImageEntry.Text = Preferences.Current.SquareSvgSplashImageFile;
            _splashPageBackgroundColorEntry.Text = Preferences.Current.SplashBackgroundColor.ToHex();
            _iconBackgroundColorEntry.Text = Preferences.Current.IconBackgroundColor.ToHex();
            Preferences.Current.PropertyChanged += OnPreferencesChanged;

            _iconSvgFileEntry.TextChanged += OnIconSvgFileChanged;
            _iosProjectFolderEntry.TextChanged += OnIosProjectFolderChanged;
            _androidProjectFolderEntry.TextChanged += OnAndroidProjectFolderChanged;
            _uwpProjectFolderEntry.TextChanged += OnUwpProjectFolderChanged;
            _squareSvgLaunchImageEntry.TextChanged += OnSquareSvgLaunchImageEntryChanged;

            UpdateButtonAbility();
        }

        private void OnPreferencesChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateButtonAbility();

            if (e.PropertyName == nameof(Preferences.SplashBackgroundColor))
                _splashPageBackgroundColorEntry.Text = Preferences.Current.SplashBackgroundColor.ToHex();
            else if (e.PropertyName == nameof(Preferences.IconBackgroundColor))
                _iconBackgroundColorEntry.Text = Preferences.Current.IconBackgroundColor.ToHex();
        }

        #region Entry event Handlers
        private void OnIosProjectFolderChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                Preferences.Current.IosOProjectFolder = e.NewTextValue;
                return;
            }
            if (Directory.Exists(e.NewTextValue.Trim()))
            {
                Preferences.Current.IosOProjectFolder = e.NewTextValue.Trim();
                return;
            }
            DisplayAlert(null,"["+ e.NewTextValue + "] failed Directory.Exists test.", "ok");
            Preferences.Current.IosOProjectFolder = e.OldTextValue.Trim();
        }

        private void OnAndroidProjectFolderChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                Preferences.Current.AndroidProjectFolder = e.NewTextValue;
                return;
            }
            if (Directory.Exists(e.NewTextValue.Trim()))
            {
                Preferences.Current.AndroidProjectFolder = e.NewTextValue.Trim();
                return;
            }
            DisplayAlert(null, "[" + e.NewTextValue + "] failed Directory.Exists test.", "ok");
            Preferences.Current.AndroidProjectFolder = e.OldTextValue.Trim();
        }

        private void OnUwpProjectFolderChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                Preferences.Current.UwpProjectFolder = e.NewTextValue;
                return;
            }
            if (Directory.Exists(e.NewTextValue.Trim()))
            {
                Preferences.Current.UwpProjectFolder = e.NewTextValue.Trim();
                return;
            }
            DisplayAlert(null, "[" + e.NewTextValue + "] failed Directory.Exists test.", "ok");
            Preferences.Current.UwpProjectFolder = e.OldTextValue.Trim() ;
        }

        private void OnIconSvgFileChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                Preferences.Current.SvgIconFile = e.NewTextValue;
                return;
            }
            if (File.Exists(e.NewTextValue))
            {
                if (e.NewTextValue.ToLower().EndsWith(".svg"))
                {
                    Preferences.Current.SvgIconFile = e.NewTextValue;
                    return;
                }
                else
                    DisplayAlert(null, "file must be SVG", "ok");
            }
            else
                DisplayAlert(null, e.NewTextValue + " is not a file.", "ok");
            Preferences.Current.SvgIconFile = e.OldTextValue;
        }

        private void OnSquareSvgLaunchImageEntryChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                Preferences.Current.SquareSvgSplashImageFile = e.NewTextValue;
                return;
            }
            if (File.Exists(e.NewTextValue))
            {
                if (e.NewTextValue.ToLower().EndsWith(".svg"))
                {
                    Preferences.Current.SquareSvgSplashImageFile = e.NewTextValue;
                    return;
                }
                else
                    DisplayAlert(null, "file must be SVG", "ok");
            }
            else
                DisplayAlert(null, e.NewTextValue + " is not a file.", "ok");
            _squareSvgLaunchImageEntry.Text = e.OldTextValue;
        }

        async void OnSelectSplashBackgroundColorButtonClicked(System.Object sender, System.EventArgs e)
            =>Preferences.Current.SplashBackgroundColor = await ColorPickerDialog.Show(Content as StackLayout, "Splash Screen Background", Preferences.Current.SplashBackgroundColor, null);

        async void OnSelectIconBackgroundColorButtonClicked(System.Object sender, System.EventArgs e)
            => Preferences.Current.IconBackgroundColor = await ColorPickerDialog.Show(Content as StackLayout, "Icon Background", Preferences.Current.IconBackgroundColor, null);
        #endregion


        #region Button Click handlers
        void OnGenerateIconsButtonClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Preferences.Current.SvgIconFile)
                || !File.Exists(Preferences.Current.SvgIconFile))
            {
                DisplayAlert(null, "Cannot open SVG file [" + Preferences.Current.SvgIconFile + "]", "cancel");
                return;
            }

            //var svg = new SkiaSharp.Extended.Svg.SKSvg();
            //var pict = svg.Load(Preferences.Current.SvgIconFile);

            //GenerateRasterAndroidIcons(pict);
            var vector = GenerateVectorAndroidIcons(Preferences.Current.SvgIconFile);
            GenerateRasterAndroidIcons(vector);
            GenerateIosIcons(vector);

            DisplayAlert("Complate", "App Icons have been generated.", "ok");
        }

        void OnGenerateLaunchImageButtonClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Preferences.Current.SquareSvgSplashImageFile)
                || !File.Exists(Preferences.Current.SquareSvgSplashImageFile))
            {
                DisplayAlert(null, "Cannot open SVG file [" + Preferences.Current.SquareSvgSplashImageFile + "]", "cancel");
                return;
            }
            GenerateIosSplashScreen(GenerateAndroidSpashImage());

            DisplayAlert("Complete", "Launch Screens have been generated.", "ok");
        }
        #endregion


        #region methods
        void UpdateButtonAbility()
        {
            _generateIconsButton.IsEnabled = Preferences.Current.IsIconEnabled;
            _generateLaunchImagesButton.IsEnabled = Preferences.Current.IsSplashEnabled;
        }


        //void GenerateIosIcons(SKPicture pict)
        void GenerateIosIcons(AndroidVector.Vector vector)
        {
            if (string.IsNullOrWhiteSpace(Preferences.Current.IosOProjectFolder) || !Directory.Exists(Preferences.Current.IosOProjectFolder))
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

            foreach (var path in Directory.GetFiles(dest) )
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

        AndroidVector.Vector GenerateVectorAndroidIcons(string svgPath)
        {
            string xmlHeader = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n";

            var resourcesFolder = Path.Combine(Preferences.Current.AndroidProjectFolder, "Resources");
            if (!Directory.Exists(resourcesFolder))
                Directory.CreateDirectory(resourcesFolder);
            var mipmapFolder = Path.Combine(resourcesFolder, "mipmap-anydpi-v26");
            if (!Directory.Exists(mipmapFolder))
                Directory.CreateDirectory(mipmapFolder);

            var csprojFiles = Directory.GetFiles(Preferences.Current.AndroidProjectFolder, "*.csproj");
            if (csprojFiles.Length > 1)
            {
                DisplayAlert(null, "More than one Android .csproj file", "ok");
                return null;
            }
            if (csprojFiles.Length < 1)
            {
                DisplayAlert(null, "Could not find Android .csproj file", "ok");
                return null;
            }
            if (XDocument.Load(csprojFiles[0]) is XDocument csprojDoc)
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

                        File.WriteAllText(csprojFiles[0], xmlHeader + csprojDoc);
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

                var text = xmlHeader + colorsDocument.ToString();
                File.WriteAllText(colorsPath, text);
            }

            if (Svg2.GenerateAndroidVector(svgPath) is (AndroidVector.Vector vector, List<string> warnings))
            {
                vector = vector.AspectClone();

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

                if (warnings.Count > 0)
                    DisplayAlert("Warnings", string.Join("\n\n", warnings), "ok");
            }
            else
            {
                DisplayAlert("ERROR", "Failed to generate AndroidVector for unknown reason.", "ok");
                vector = null;
            }

            return vector;
        }

        void GenerateRasterAndroidIcons(AndroidVector.Vector vector)
        {
            if (string.IsNullOrWhiteSpace(Preferences.Current.AndroidProjectFolder) || !Directory.Exists(Preferences.Current.AndroidProjectFolder))
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

        #region Android Splash
        AndroidVector.Vector GenerateAndroidSpashImage()
        {
            string xmlHeader = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n";

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

            var csprojFiles = Directory.GetFiles(Preferences.Current.AndroidProjectFolder, "*.csproj");
            if (csprojFiles.Length > 1)
            {
                DisplayAlert(null, "More than one Android .csproj file","ok");
                return null;
            }
            if (csprojFiles.Length < 1)
            {
                DisplayAlert(null, "Could not find Android .csproj file", "ok");
                return null;
            }
            if (XDocument.Load(csprojFiles[0]) is XDocument csprojDoc)
            {
                XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";
                if (csprojDoc.Descendants(ns +"AndroidResource").FirstOrDefault(e=>e.Attribute("Include")?.Value == "Resources\\values\\styles.xml") is XElement firstAndroidResource)
                {
                    var androidResourceItemGroup = firstAndroidResource.Parent;
                    if (!androidResourceItemGroup.Elements(ns +"AndroidResource").Any(e=>e.Attribute("Include")?.Value == "Resources\\drawable\\background_splash.xml"))
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


                        File.WriteAllText(csprojFiles[0], xmlHeader + csprojDoc);
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

                        File.WriteAllText(csprojFiles[0], xmlHeader + csprojDoc);
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

                    var text = xmlHeader + stylesDocument.ToString().Replace("{android}", "android:");
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

                var text = xmlHeader + colorsDocument.ToString();
                File.WriteAllText(colorsPath, text);
            }

            if (Svg2.GenerateAndroidVector(Preferences.Current.SquareSvgSplashImageFile) is (AndroidVector.Vector vector, List<string> warnings))
            {
                File.WriteAllText(Path.Combine(drawable23Folder, "splash_image.xml"), vector.ToString());

                if (warnings.Count > 0)
                    DisplayAlert("Warnings", string.Join("\n\n", warnings), "ok");
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
            }
            else
            {
                DisplayAlert("ERROR", "Failed to generate AndroidVector for unknown reason.", "ok");
                vector = null;
            }


            //DisplayAlert(null, "Don't forget to set MainLauncher=\"false\" in " + Path.Combine(Preferences.Current.AndroidProjectFolder + "MainActivity.cs") + ".", "ok");
            string namespaceLine = null;
            var mainActivityPath = Path.Combine(Preferences.Current.AndroidProjectFolder, "MainActivity.cs");
            var mainActivityLines= File.ReadAllLines(mainActivityPath);
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

            return vector;
        }
        #endregion

        #region iOS Splash
        void GenerateIosSplashScreen(AndroidVector.Vector vector)
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
            GetType().Assembly.TryCopyResource("AssetBuilder.Resources.LaunchScreen.storyboard", launchScreenPath, true);

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
            var fs = Directory.GetFiles(Preferences.Current.IosOProjectFolder);
            if (Directory.GetFiles(Preferences.Current.IosOProjectFolder, "*.csproj") is string[] files)
            {
                if (files.Length > 1)
                    return "multiple .csproj files found in the iOS project folder";
                if (files.Length < 1)
                    return "no .csproj file found in the iOS project folder";
                var fileName = files[0];
                if (string.IsNullOrWhiteSpace(fileName))
                    return "invalid filename for iOS .csproj file";
                if (!File.Exists(fileName))
                    return "candidate for iOS project file [" + fileName + "] does not exist.";
                if (XDocument.Load(fileName) is XDocument document)
                {
                    XNamespace ns = document.Root.Attribute("xmlns").Value;
                    foreach (var itemGroup in document.Descendants(ns + "ItemGroup"))
                    {
                        {
                            if (itemGroup.Element(ns +"InterfaceDefinition") is XElement interfaceDef
                                && interfaceDef.Attribute("Include") is XAttribute includeAttr
                                && (includeAttr.Value == "Resources\\LaunchScreen.storyboard"
                                    || includeAttr.Value == "Resources/LaunchScreen.storyboard")
                                )
                            {
                                if (itemGroup.Elements(ns + "ImageAsset").Attributes("Include").Any(a => a.Value == "Assets.xcassets\\Splash.imageset\\LaunchImage.pdf"))
                                    return null;

                                var imageAsset = new XElement(ns +"ImageAsset",
                                    new XElement(ns + "Visible", false));
                                imageAsset.SetAttributeValue("Include", "Assets.xcassets\\Splash.imageset\\Contents.json");
                                itemGroup.Add(imageAsset);

                                imageAsset = new XElement(ns +"ImageAsset",
                                    new XElement(ns +"Visible", false));
                                imageAsset.SetAttributeValue("Include", "Assets.xcassets\\Splash.imageset\\LaunchImage.pdf");
                                itemGroup.Add(imageAsset);

                                string xmlHeader = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n";
                                var text = xmlHeader + document;
                                File.WriteAllText(fileName, text);

                                return null;
                            }
                        }
                    }
                    return "could not find ItemGroup into which to inject the LaunchScreen image assets";
                }
                else
                    return "could not load [" + fileName + "] as an XML document";
            }
            return "could not get files from iOS project folder.";
        }
        #endregion

        #endregion


    }
}
