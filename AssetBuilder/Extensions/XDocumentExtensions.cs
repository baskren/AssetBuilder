using System;
using System.Xml;
using System.Xml.Linq;

namespace AssetBuilder
{
    public static class XDocumentExtensions
    {
        public static XDocument Load(P42.Storage.IStorageFile storageFile)
        {
            if (storageFile?.ReadAllText() is string xlmText)
                return XDocument.Parse(xlmText);
            return null;
        }


    }
}
