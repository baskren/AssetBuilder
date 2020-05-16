using System;
using System.Xml.Linq;

namespace Svg2AndroidVector
{
    public static class Namespace
    {
        const string SvgPath = "http://www.w3.org/2000/svg";
        public static readonly XNamespace Svg = SvgPath;

        const string XlinkNamespacePath = "http://www.w3.org/1999/xlink";
        public static readonly XNamespace xlinkNs = XlinkNamespacePath;


    }
}
