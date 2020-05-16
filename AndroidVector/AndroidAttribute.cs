using System;
using System.Xml.Linq;

namespace AndroidVector
{
    public class AndroidAttribute : XAttribute
    {
        public AndroidAttribute(string name, object value) : base(Namespace.AndroidVector + name, value) { }
    }
}
