using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using P42.SandboxedStorage;
using Xamarin.Forms;

namespace AssetBuilder
{
    public class ProjectFolderPicker : StorageFolderPickerHybridView
    {
        #region Properties

        #region Platform
        /// <summary>
        /// Backing store for ProjectFolderPicker Platform property
        /// </summary>
        public static readonly BindableProperty ProjectPlatformProperty = BindableProperty.Create(nameof(ProjectPlatform), typeof(Xamarin.Essentials.DevicePlatform), typeof(ProjectFolderPicker), default);
        /// <summary>
        /// controls value of .Platform property
        /// </summary>
        public Xamarin.Essentials.DevicePlatform ProjectPlatform
        {
            get => (Xamarin.Essentials.DevicePlatform)GetValue(ProjectPlatformProperty);
            set => SetValue(ProjectPlatformProperty, value);
        }
        #endregion

        #region ProjectFile
        /// <summary>
        /// Backing store for ProjectFolderPicker ProjectFile property
        /// </summary>
        public static readonly BindableProperty ProjectFileProperty = BindableProperty.Create(nameof(ProjectFile), typeof(IStorageFile), typeof(ProjectFolderPicker), default);
        /// <summary>
        /// controls value of .ProjectFile property
        /// </summary>
        public IStorageFile ProjectFile
        {
            get => (IStorageFile)GetValue(ProjectFileProperty);
            set => SetValue(ProjectFileProperty, value);
        }
        #endregion

        #endregion

        #region Fields
        IStorageFolder oldFolder;
        IStorageFile oldProjetFile;
        #endregion


        public ProjectFolderPicker()
        {
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == StorageItemProperty.PropertyName)
            {
                if (StorageFolder is null)
                    oldFolder = StorageFolder;
                else
                {
                    var task = GetProjectFile();
                    task.Wait();
                    if (task.Result is null)
                    {
                        StorageFolder = oldFolder;
                        ProjectFile = oldProjetFile;
                    }
                    else
                    {
                        ProjectFile = task.Result;
                        oldProjetFile = ProjectFile;
                        oldFolder = StorageFolder;
                    }
                }
            }
            base.OnPropertyChanged(propertyName);
            if (propertyName == ProjectPlatformProperty.PropertyName)
                Placeholder = "click to select " + ProjectPlatform + " platform project folder";
        }

        public async Task<IStorageFile> GetProjectFile()
        {
            await Task.Delay(5).ConfigureAwait(false); 

            var osName =  ProjectPlatform.ToString();
            if (StorageFolder is null)
                return null;
            try
            {
                if (await StorageFolder.GetFilesAsync("*.csproj").ConfigureAwait(false) is IReadOnlyList<IStorageFile> projFiles)
                {
                    if (projFiles.Count > 1)
                    {
                        DisplayAlert(osName + " Project Folder", "Multiple .csproj files found in the " + osName + " project folder [" + StorageFolder.Path + "].", "ok");
                        return null;
                    }
                    if (projFiles.Count < 1)
                    {
                        DisplayAlert(osName + " Project Folder", "No .csproj file found in the " + osName + " project folder [" + StorageFolder.Path + "].", "ok");
                        return null;
                    }
                    var file = projFiles[0];
                    if (string.IsNullOrWhiteSpace(file.Path))
                    {
                        DisplayAlert(osName + " Project Folder", "Invalid filename for " + osName + " .csproj file. [" + StorageFolder.Path + "].", "ok");
                        return null;
                    }
                    var text = await file.ReadAllTextAsync();
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        DisplayAlert(osName + " Project Folder", "Candidate for " + osName + " project file [" + file.Path + "], in this folder [" + StorageFolder.Path + "], is empty.", "ok");
                        return null;
                    }
                    string outputType;
                    string targetType;
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
                            DisplayAlert(osName + " Project Folder", "Unsupported DevicePlatform [" + osName + "] in folder [" + StorageFolder.Path + "].", "ok");
                            return null;
                    }
                    if (!text.Contains(outputType) || !text.Contains(targetType))
                        DisplayAlert(osName + " Project Folder", "The project file [" + file.Path + "], in this folder [" + StorageFolder.Path + "], doesn't appear to be an " + osName + " executable app project file.", "ok");
                    else
                        return file;
                }
            }
            catch (Exception e)
            {
                Device.BeginInvokeOnMainThread(()=>Page?.DisplayAlert("Exception", e.Message, "ok"));
            }
            return null;
        }
    }
}
