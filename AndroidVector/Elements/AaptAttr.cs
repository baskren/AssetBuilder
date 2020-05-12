using System;
namespace AndroidVector
{
    public class AaptAttr : BaseElement
    {
        public AaptAttr(string attributeName, object content) : base(aaptNs + "attr", content)
        {
            SetAttributeValue("name", "android:" + attributeName);
        }

    }
}
