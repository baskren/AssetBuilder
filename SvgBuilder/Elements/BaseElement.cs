using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace SvgBuilder
{
    public class BaseElement : XElement
    {
        public const string svgNamespaceURI = "http://www.w3.org/2000/svg";
        public const string xlinkNamespaceURI = "http://www.w3.org/1999/xlink";

        protected static readonly XNamespace ns = svgNamespaceURI;
        protected static readonly XNamespace xlinkNs = xlinkNamespaceURI;

        public string Id
        {
            get => GetPropertyAttribute<string>();
            set => SetPropertyAttribute(value);
        }

        public string Class
        {
            get => GetPropertyAttribute<string>();
            set => SetPropertyAttribute(value);
        }

        public string Style
        {
            get => GetPropertyAttribute<string>();
            set => SetPropertyAttribute(value);
        }



        #region Conditional Processing
        /* not yet supported
        public string Xml_Base
        {
            get => GetPropertyAttribute<string>();
            set => SetPropertyAttribute(value);
        }

        public string Xml_Lang
        {
            get => GetPropertyAttribute<string>();
            set => SetPropertyAttribute(value);
        }

        public Space Xml_Space
        {
            get => GetEnumPropertyAttribute<Space>();
            set => SetPropertyAttribute(value);
        }

        public string RequiredExtensions
        {
            get => GetPropertyAttribute<string>();
            set => SetPropertyAttribute(value);
        }

        public string RequiredFeatures
        {
            get => GetPropertyAttribute<string>();
            set => SetPropertyAttribute(value);
        }

        public string SystemLanguage
        {
            get => GetPropertyAttribute<string>();
            set => SetPropertyAttribute(value);
        }
        */
        #endregion

        #region Graphical Events 
        public string Onactivate
        {
            get => GetPropertyAttribute<string>();
            set => SetPropertyAttribute(value);
        }

        public string Onclick
        {
            get => GetPropertyAttribute<string>();
            set => SetPropertyAttribute(value);
        }

        public string Onfocusin
        {
            get => GetPropertyAttribute<string>();
            set => SetPropertyAttribute(value);
        }

        public string Onfocusout
        {
            get => GetPropertyAttribute<string>();
            set => SetPropertyAttribute(value);
        }

        public string Onload
        {
            get => GetPropertyAttribute<string>();
            set => SetPropertyAttribute(value);
        }

        public string Onmousedown
        {
            get => GetPropertyAttribute<string>();
            set => SetPropertyAttribute(value);
        }

        public string Onmousemove
        {
            get => GetPropertyAttribute<string>();
            set => SetPropertyAttribute(value);
        }

        public string Onmouseover
        {
            get => GetPropertyAttribute<string>();
            set => SetPropertyAttribute(value);
        }

        public string Onmouseup
        {
            get => GetPropertyAttribute<string>();
            set => SetPropertyAttribute(value);
        }

        #endregion


        #region Painting

        #endregion

        public BaseElement(string name) : base(name)
        {
        }


        XName AttributeNameForPropertyName(string propertyName)
        {
            if (propertyName.StartsWith("Xml_"))
                return XNamespace.Xml + propertyName.Substring(4).ToCamelCase();
            if (propertyName.StartsWith("Svg_"))
                return ns + propertyName.Substring(4).ToCamelCase();
            if (propertyName.StartsWith("Xlink_"))
                return xlinkNs + propertyName.Substring(6).ToCamelCase();
            return propertyName.ToCamelCase();
        }

        protected T GetPropertyAttribute<T>([CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException(nameof(propertyName));
            if (typeof(T) == typeof(Color))
                throw new ArgumentException("Use GetColorPropertyAttribute");
            if (typeof(T).IsEnum)
                throw new ArgumentException("Use GetEnumPropertyAttribute");

            XName xName = propertyName.ToCamelCase();


            if (Attribute(AttributeNameForPropertyName(propertyName)) is XAttribute attribute)
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

            if (Attribute(AttributeNameForPropertyName(propertyName)) is XAttribute attribute)
                return attribute.Value.ToColor();
            return default;
        }


        protected T GetEnumPropertyAttribute<T>([CallerMemberName] string propertyName = null) where T : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException(nameof(propertyName));

            if (Attribute(AttributeNameForPropertyName(propertyName)) is XAttribute attribute && typeof(T).IsEnum)
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
                SetAttributeValue(AttributeNameForPropertyName(propertyName), color);
            else if (value.GetType().IsEnum)
                SetAttributeValue(AttributeNameForPropertyName(propertyName), value.ToString().ToCamelCase());
            else
                SetAttributeValue(AttributeNameForPropertyName(propertyName), value);
        }

    }
}
