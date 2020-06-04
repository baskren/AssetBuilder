using System;
namespace AndroidVector
{
    public class AaptAttr : BaseElement
    {
        public AaptAttr() : base(Namespace.Aapt + "attr") { }

        public AaptAttr(string attributeName) : base(Namespace.Aapt + "attr")
        {
            if (!string.IsNullOrWhiteSpace(attributeName))
                SetAttributeValue("name", "android:" + attributeName);
        }

        public AaptAttr(string attributeName, object content) : base(Namespace.Aapt + "attr", content)
        {
            if (!string.IsNullOrWhiteSpace(attributeName))
                SetAttributeValue("name", "android:" + attributeName);
        }
    }
}
