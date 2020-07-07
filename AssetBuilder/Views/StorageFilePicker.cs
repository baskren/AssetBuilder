using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using P42.SandboxedStorage;
/*
namespace AssetBuilder
{
    public class StorageFilePicker : StorageItemPicker
    {
        #region Properties

        #region StorageFile
        /// <summary>
        /// controls value of StorageFile property
        /// </summary>
        public IStorageFile StorageFile
        {
            get => (IStorageFile)GetValue(StorageItemProperty);
            set => SetValue(StorageItemProperty, value);
        }
        #endregion


        #endregion


        public event EventHandler<StorageFileChangedEventArgs> StorageFileChanged;


        public async Task<bool> SetStorageFileFromPathAsync(string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && await StorageFileExtensions.GetFileFromPathAsync(path) is IStorageFile file)
            {
                StorageFile = file;
                return true;
            }
            return false;
        }

        protected override async void PathTap_Tapped(object sender, EventArgs e)
        {
            if (await Pickers.PickSingleFileAsync() is IStorageFile storageFile)
                StorageFile = storageFile;
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == StorageItemProperty.PropertyName)
                StorageFileChanged?.Invoke(this, new StorageFileChangedEventArgs(StorageFile));
        }

    }

    public class StorageFileChangedEventArgs : EventArgs
    {
        public IStorageFile StorageFile { get; private set; }

        public StorageFileChangedEventArgs(IStorageFile storageFile)
            => StorageFile = storageFile;
    }
}
*/
