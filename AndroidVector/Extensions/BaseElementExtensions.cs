using System;
using System.Linq;

namespace AndroidVector
{
    public static class BaseElementExtensions
    {
        public static T Copy<T>(this T baseElement) where T : BaseElement, new()
        {
            var copy = new T();
            foreach (var attribute in baseElement.Attributes())
                if (copy.Attribute(attribute.Name) == null)
                    copy.Add(attribute);

            foreach (var element in baseElement.Elements())
            {
                if (element is Vector bElement)
                    copy.Add(bElement.Copy());
                else if (element is SweepGradient sweepGradient)
                    copy.Add(sweepGradient.Copy());
                else if (element is RadialGradient radialGradient)
                    copy.Add(radialGradient.Copy());
                else if (element is Path path)
                    copy.Add(path.Copy());
                else if (element is LinearGradient linearGradient)
                    copy.Add(linearGradient.Copy());
                else if (element is Group group)
                    copy.Add(group.Copy());
                else if (element is ClipPath clipPath)
                    copy.Add(clipPath.Copy());
                else if (element is AaptAttr aaptAttr)
                    copy.Add(aaptAttr.Copy());
                else if (element is GradientItem gradientItem)
                    copy.Add(gradientItem.Copy());
                else
                    throw new Exception("Doh!  Forgot to support Copy on [" + element + "] AndroidVector element!");
            }
            return copy;

        }

    }
}
