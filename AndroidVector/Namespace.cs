using System;
using System.Xml.Linq;

namespace AndroidVector
{
    public class Namespace
    {
        const string AndroidVectorPath = "http://schemas.android.com/apk/res/android";
        public static readonly XNamespace AndroidVector = AndroidVectorPath;

        const string AaptPath = "http://schemas.android.com/aapt";
        public static readonly XNamespace Aapt = AaptPath;


    }
}
