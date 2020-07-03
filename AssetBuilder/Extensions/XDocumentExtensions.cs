using System;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace AssetBuilder
{
    public static class XDocumentExtensions
    {
        public static async Task<XDocument> LoadAsync(P42.Storage.IStorageFile storageFile)
        {
            if (await storageFile?.ReadAllTextAsync() is string xlmText)
                return XDocument.Parse(xlmText);
            return null;
        }


    }
}
