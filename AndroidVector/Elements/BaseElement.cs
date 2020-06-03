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
        #region Properties
        public List<Matrix> SvgTransforms { get; } = new List<Matrix>();

        public string SvgDisplayStyle { get; set; }

        float _svgOpacity = 1;
        public float SvgOpacity
        {
            get => _svgOpacity;
            set
            {
                _svgOpacity = value;
                _svgOpacity = Math.Min(1, Math.Max(0, _svgOpacity));
            } 
        } 

        public new string Name
        {
            get => GetPropertyAttribute<string>();
            set => SetPropertyAttribute(value);
        }
        #endregion


        #region Constructors

        public BaseElement(XName name) : base(name) { }

        public BaseElement(XName name, object content) : base(name, content) { }

        public BaseElement(XName name, params object[] content) : base(name, content) { }

        #endregion


        #region Bounds
        public virtual RectangleF GetBounds()
        {
            //if (SvgTransforms.Any())
            //    throw new Exception("Cannot get bounds before SVG Transforms have been applied.");

            float left = float.NaN, right = float.NaN, top = float.NaN, bottom = float.NaN;
            foreach (var element in Elements())
            {
                if (element is BaseElement baseElement)
                {
                    var b = baseElement.GetBounds();
                    if (!float.IsNaN(b.Left))
                    {
                        if (float.IsNaN(left))
                        {
                            left = b.Left;
                            right = b.Right;
                            top = b.Top;
                            bottom = b.Bottom;
                        }
                        else
                        {
                            left = Math.Min(left, b.Left);
                            top = Math.Min(top, b.Top);
                            right = Math.Max(right, b.Right);
                            bottom = Math.Max(bottom, b.Bottom);
                        }
                    }
                }
            }
            return new RectangleF(left, top, float.IsNaN(left) ? float.NaN : right - left, float.IsNaN(left) ? float.NaN : bottom - top);
        }
        #endregion


        #region SvgTransform Handlers
        public virtual void ApplySvgTransform(Matrix matrix)
        {
            foreach (var element in Elements().ToArray())
            {
                if (element is BaseElement baseElement)
                    baseElement.ApplySvgTransform(matrix);
            }
        }

        public void ApplySvgTransforms()
        {
            foreach (var element in Elements().ToArray())
            {
                if (element is BaseElement baseElement)
                {
                    if (baseElement.SvgDisplayStyle == "none") //&& baseElement is Path)
                        baseElement.Remove();
                    else
                        baseElement.ApplySvgTransforms();
                }
            }
            SvgTransforms.Reverse();
            if (SvgTransforms.Count == 0)
                ApplySvgTransform(new Matrix(1, 0, 0, 1, 0, 0));
            foreach (var transform in SvgTransforms)
            {
                ApplySvgTransform(transform);
            }
            SvgTransforms.Clear();
        }

        public virtual void ApplySvgOpacity(float parentOpacity = 1)
        {
            foreach (var element in Elements().ToArray())
            {
                if (element is BaseElement baseElement)
                    baseElement.ApplySvgOpacity(parentOpacity * SvgOpacity);
            }
            SvgOpacity = 1;
        }

        public virtual void PurgeDefaults()
        {
            foreach (var element in Elements().ToArray())
            {
                if (element is BaseElement baseElement)
                    baseElement.PurgeDefaults();
            }
            this.SetAndroidAttributeValue("strokeWidth", null);
            this.SetAndroidAttributeValue("strokeAlpha", null);
            this.SetAndroidAttributeValue("fillAlpha", null);
            this.SetAndroidAttributeValue("trimPathStart", null);
            this.SetAndroidAttributeValue("trimPathStart", null);
            this.SetAndroidAttributeValue("trimPathOffset", null);
            this.SetAndroidAttributeValue("strokeLineCap", null);
            this.SetAndroidAttributeValue("strokeLineJoin", null);
            this.SetAndroidAttributeValue("strokeMiterLimit", null);
            this.SetAndroidAttributeValue("fillType", null);
            this.SetAndroidAttributeValue("strokeColor", null);
            this.SetAndroidAttributeValue("fillColor", null);
        }

        public virtual void MapGradients()
        {
            foreach (var child in Elements().ToArray())
                if (child is BaseElement baseElement)
                    baseElement.MapGradients();
        }

        #endregion


        #region Property to Attribute connector
        protected T GetPropertyAttribute<T>(T defaultValue = default, [CallerMemberName] string propertyName = null) 
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException(nameof(propertyName));
            if (typeof(T) == typeof(Color))
                throw new ArgumentException("Use GetColorPropertyAttribute");
            if (typeof(T).IsEnum)
                throw new ArgumentException("Use GetEnumPropertyAttribute");

            if (Attribute(Namespace.AndroidVector + propertyName.ToCamelCase()) is XAttribute attribute)
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
            return defaultValue;
        }

        protected Color GetColorPropertyAttribute(Color defaultColor = default, [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException(nameof(propertyName));

            if (Attribute(Namespace.AndroidVector + propertyName.ToCamelCase()) is XAttribute attribute)
                return attribute.Value.ToColor();
            return defaultColor;
        }

        protected float GetAlphaPropertyAttribute([CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException(nameof(propertyName));

            if (Attribute(Namespace.AndroidVector + propertyName.ToCamelCase()) is XAttribute attribute && float.TryParse(attribute.Value, out float result))
                return result;
            return 1;
        }

        protected T GetEnumPropertyAttribute<T>(T defaultValue = default, [CallerMemberName] string propertyName = null) where T : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException(nameof(propertyName));

            if (Attribute(Namespace.AndroidVector + propertyName.ToCamelCase()) is XAttribute attribute && typeof(T).IsEnum)
            {
                if (Enum.TryParse<T>(attribute.Value.ToPascalCase(), out T value))
                    return value;
            }
            return defaultValue;
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
        #endregion


        public virtual void AddToPdf(PdfSharpCore.Drawing.XGraphics gfx, List<string> warnings)
        {
            if (SvgOpacity != 1)
                throw new Exception("Cannot convert to PDF before ApplySvgOpacity has been called.");

            if (SvgTransforms.Count > 0)
                throw new Exception("Cannot convert to PDF before ApplySvgTransforms has been called.");

            gfx.Save();
            foreach (var element in Elements().ToArray())
            {
                if (element is BaseElement baseElement)
                    baseElement.AddToPdf(gfx, warnings);
            }
            gfx.Restore();
        }

        public virtual void AddToSKCanvas(SkiaSharp.SKCanvas canvas)
        {
            if (SvgOpacity != 1)
                throw new Exception("Cannot convert to PDF before ApplySvgOpacity has been called.");

            if (SvgTransforms.Count > 0)
                throw new Exception("Cannot convert to PDF before ApplySvgTransforms has been called.");

            canvas.Save();
            foreach (var element in Elements().ToArray())
            {
                if (element is BaseElement baseElement)
                    baseElement.AddToSKCanvas(canvas);
            }
            canvas.Restore();
        }
    }
}
