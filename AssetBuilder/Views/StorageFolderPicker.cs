using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using P42.Storage;

namespace AssetBuilder
{
    public class StorageFolderPicker : StorageItemPicker
    {
        #region Properties

        #region StorageFile
        /// <summary>
        /// controls value of StorageFolder property
        /// </summary>
        public IStorageFolder StorageFolder
        {
            get => (IStorageFolder)GetValue(StorageItemProperty);
            set => SetValue(StorageItemProperty, value);
        }
        #endregion


        #endregion

        public event EventHandler<StorageFolderChangedEventArgs> StorageFolderChanged;



        public async Task<bool> FolderFromPathAsync(string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && await StorageFolderExtensions.GetFolderFromPathAsync(path) is IStorageFolder folder)
            {
                StorageFolder = folder;
                return true;
            }
            return false;
        }

        protected override async void PathTap_Tapped(object sender, EventArgs e)
        {
            if (await Pickers.PickSingleFolderAsync() is IStorageFolder storageFolder)
                StorageFolder = storageFolder;
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == StorageItemProperty.PropertyName)
                StorageFolderChanged?.Invoke(this, new StorageFolderChangedEventArgs(StorageFolder));
        }

    }

    public class StorageFolderChangedEventArgs : EventArgs
    {
        public IStorageFolder StorageFolder { get; private set; }

        public StorageFolderChangedEventArgs(IStorageFolder storageFolder)
            => StorageFolder = storageFolder;
    }
}
