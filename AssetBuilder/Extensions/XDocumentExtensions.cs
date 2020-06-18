using System;
using System.Xml.Linq;

namespace AssetBuilder
{
    public static class XDocumentExtensions
    {
        public static XDocument Load(P42.Storage.IStorageFile storageFile)
        {
            if (storageFile?.OpenRead() is System.IO.FileStream stream)
            {
                return XDocument.Load(stream);
            }
            return null;
        }
    }
}
