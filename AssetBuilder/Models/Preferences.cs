using System;
using Xamarin.Forms;
using Newtonsoft.Json;

namespace AssetBuilder.Models
{
    public class Preferences : P42.Utils.NotifiablePropertyObject
    {
        static Preferences _current;
        public static Preferences Current
        {
            get
            {
                if (_current != null)
                    return _current;
                if (P42.Utils.TextCache.Recall(nameof(Preferences), "AssetBuilder") is string json)
                    _current = Newtonsoft.Json.JsonConvert.DeserializeObject<Preferences>(json);
                else
                    _current = new Preferences();
                _current.PropertyChanged += OnCurrent_PropertyChanged;
                return _current;
            }
        }

        public bool IsDestinationsEnabled => !string.IsNullOrWhiteSpace(IosOProjectFolder) || !string.IsNullOrWhiteSpace(AndroidProjectFolder) || !string.IsNullOrWhiteSpace(UwpProjectFolder);

        public bool IsIconEnabled => (!string.IsNullOrWhiteSpace(SvgIconFile) && IsDestinationsEnabled) || (!string.IsNullOrWhiteSpace(SvgUwpBadgeFile) && !string.IsNullOrWhiteSpace(UwpProjectFolder));

        public bool IsSplashEnabled => (!string.IsNullOrWhiteSpace(SquareSvgSplashImageFile) || !string.IsNullOrEmpty(Rect310SvgSplashImageFile)) && IsDestinationsEnabled;

        private static void OnCurrent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(_current);
            P42.Utils.TextCache.Store(json, nameof(Preferences), "AssetBuilder");
        }

        string _svgIconFile;
        public string SvgIconFile
        {
            get => _svgIconFile;
            set => SetField(ref _svgIconFile, value);
        }

        string _svgUwpBadgeFile;
        public string SvgUwpBadgeFile
        {
            get => _svgUwpBadgeFile;
            set => SetField(ref _svgUwpBadgeFile, value);
        }

        string _iosProjectFolder;
        public string IosOProjectFolder
        {
            get => _iosProjectFolder;
            set => SetField(ref _iosProjectFolder, value);
        }

        string _androidProjectFolder;
        public string AndroidProjectFolder
        {
            get => _androidProjectFolder;
            set => SetField(ref _androidProjectFolder, value);
        }

        string _uwpProjectFolder;
        public string UwpProjectFolder
        {
            get => _uwpProjectFolder;
            set => SetField(ref _uwpProjectFolder, value);
        }

        Color _splashBackgroundColor = Color.White;
        [JsonIgnore]
        public Color SplashBackgroundColor
        {
            get => _splashBackgroundColor;
            set => SetField(ref _splashBackgroundColor, value);
        }

        public string SplashBackgroundColorString
        {
            get => _splashBackgroundColor.ToHex();
            set => _splashBackgroundColor = Color.FromHex(value.Trim('#'));
        }

        Color _iconBackgroundColor = Color.White;
        [JsonIgnore]
        public Color IconBackgroundColor
        {
            get => _iconBackgroundColor;
            set => SetField(ref _iconBackgroundColor, value);
        }

        public string IconBackgroundColorString
        {
            get => _iconBackgroundColor.ToHex();
            set => _iconBackgroundColor = Color.FromHex(value.Trim('#'));
        }

        string _squareSvgSplashImageFile;
        public string SquareSvgSplashImageFile
        {
            get => _squareSvgSplashImageFile;
            set => SetField(ref _squareSvgSplashImageFile, value);
        }

        /*
        string _rect620SvgSplashImageFile;
        public string Rect620SvgSplashImageFile
        {
            get => _rect620SvgSplashImageFile;
            set => SetField(ref _rect620SvgSplashImageFile, value);
        }
        */

        string _rect310SvgSplashImageFile;
        public string Rect310SvgSplashImageFile
        {
            get => _rect310SvgSplashImageFile;
            set => SetField(ref _rect310SvgSplashImageFile, value);
        }

        MobileSplashSource _mobileSplashSource;
        public MobileSplashSource MobileSplashSource
        {
            get => _mobileSplashSource;
            set => SetField(ref _mobileSplashSource, value);
        }
    }
}
