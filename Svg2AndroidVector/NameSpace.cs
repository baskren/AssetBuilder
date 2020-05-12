using System;
using System.Xml.Linq;

namespace Svg2AndroidVector
{
    public static class NameSpace
    {
        const string SvgPath = "http://www.w3.org/2000/svg";
        public static readonly XNamespace Svg = SvgPath;
    }
}
