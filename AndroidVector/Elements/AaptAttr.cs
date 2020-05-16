using System;
namespace AndroidVector
{
    public class AaptAttr : BaseElement
    {
        public AaptAttr(string attributeName, object content) : base(Namespace.Aapt + "attr", content)
        {
            SetAttributeValue("name", "android:" + attributeName);
        }

    }
}
