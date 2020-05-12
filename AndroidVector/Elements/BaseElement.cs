using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace AndroidVector
{
    public class BaseElement : XElement
    {
        public static XNamespace avNs = "http://schemas.android.com/apk/res/android";
        public static XNamespace aaptNs = "http://schemas.android.com/aapt";

        public List<Matrix> SvgTransforms { get; } = new List<Matrix>();

        public string DisplayStyle { get; set; }

        public new string Name
        {
            get => GetPropertyAttribute<string>();
            set => SetPropertyAttribute(value);
        }


        public BaseElement(XName name) : base(name) { }

        public BaseElement(XName name, object content) : base(name, content) { }

        public BaseElement(XName name, params object[] content) : base(name, content) { }

        public virtual void ApplySvgTransform(Matrix matrix)
        {
            foreach (var element in Elements())
            {
                if (element is BaseElement baseElement)
                    baseElement.ApplySvgTransform(matrix);
            }
        }

        public void ApplySvgTransforms()
        {
            System.Diagnostics.Debug.WriteLine("this ["+this.Name+"] Count =["+this.Elements().Count()+"]");

            foreach (var element in Elements().ToArray())
            {
                System.Diagnostics.Debug.WriteLine("this [" + this.Name + "] element = [" + element.GetType()+"] ["+"]");

                if (element is BaseElement baseElement)
                {
                    System.Diagnostics.Debug.WriteLine("this [" + this.Name + "] baseElement.Name [" + baseElement.Name+"]");

                    if (baseElement.DisplayStyle == "none") //&& baseElement is Path)
                        baseElement.Remove();
                    else
                        baseElement.ApplySvgTransforms();

                }
            }
            foreach (var transform in SvgTransforms)
            {
                ApplySvgTransform(transform);
            }
            SvgTransforms.Clear();
        }

        protected T GetPropertyAttribute<T>([CallerMemberName] string propertyName = null) 
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException(nameof(propertyName));
            if (typeof(T) == typeof(Color))
                throw new ArgumentException("Use GetColorPropertyAttribute");
            if (typeof(T).IsEnum)
                throw new ArgumentException("Use GetEnumPropertyAttribute");

            if (Attribute(avNs + propertyName.ToCamelCase()) is XAttribute attribute)
            {
                var s = attribute.Value;
                try
                {
                    if (TypeDescriptor.GetConverter(typeof(T)) is TypeConverter converter)
                        return (T)converter.ConvertFromString(s);
                }
                catch (NotSupportedException)
                {
                }
            }
            return default;
        }

        protected Color GetColorPropertyAttribute([CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException(nameof(propertyName));

            if (Attribute(avNs + propertyName.ToCamelCase()) is XAttribute attribute)
                return attribute.Value.ToColor();
            return default;
        }


        protected T GetEnumPropertyAttribute<T>([CallerMemberName] string propertyName = null) where T : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException(nameof(propertyName));

            if (Attribute(avNs + propertyName.ToCamelCase()) is XAttribute attribute && typeof(T).IsEnum)
            {
                if (Enum.TryParse<T>(attribute.Value, out T value))
                    return value;
            }
            return default;
        }

        protected void SetPropertyAttribute(object value, [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException(nameof(propertyName));

            if (value is Color color)
                this.SetAndroidAttributeValue(propertyName.ToCamelCase(), color);
            else if (value.GetType().IsEnum)
                this.SetAndroidAttributeValue(propertyName.ToCamelCase(), value.ToString().ToCamelCase());
            else
                this.SetAndroidAttributeValue(propertyName.ToCamelCase(), value);
        }



    }
}
