using System;
using System.Drawing;
using AndroidVector.Extensions;

namespace AndroidVector
{
    public class Path : BaseElement
    {
        #region Properties
        public string PathData
        {
            get => GetPropertyAttribute<string>();
            set => SetPropertyAttribute(value);
        }

        public Color FillColor
        {
            get => GetColorPropertyAttribute();
            set => SetPropertyAttribute(value);
        }

        public Color StrokeColor
        {
            get => GetColorPropertyAttribute();
            set => SetPropertyAttribute(value);
        }

        public float StrokeWidth
        {
            get => GetPropertyAttribute<float>();
            set => SetPropertyAttribute(value);
        }

        public float StrokeAlpha
        {
            get => GetPropertyAttribute<float>();
            set => SetPropertyAttribute(value);
        }

        public float FillAlpha
        {
            get => GetPropertyAttribute<float>();
            set => SetPropertyAttribute(value);
        }

        public float TrimPathStart
        {
            get => GetPropertyAttribute<float>();
            set => SetPropertyAttribute(value);
        }

        public float TrimPathEnd
        {
            get => GetPropertyAttribute<float>();
            set => SetPropertyAttribute(value);
        }

        public float TrimPathOffset
        {
            get => GetPropertyAttribute<float>();
            set => SetPropertyAttribute(value);
        }

        public LineCap StrokeLineCap
        {
            get => GetEnumPropertyAttribute<LineCap>();
            set => SetPropertyAttribute(value);
        }

        public LineJoin StrokeLineJoin
        {
            get => GetEnumPropertyAttribute<LineJoin>();
            set => SetPropertyAttribute(value);
        }

        public float StrokeMiterLimit
        {
            get => GetPropertyAttribute<float>();
            set => SetPropertyAttribute(value);
        }

        public FillType Fill
        {
            get => GetEnumPropertyAttribute<FillType>();
            set => SetPropertyAttribute(value);
        }
        #endregion


        #region Constructor
        public Path() : base("path")
        {
        }
        #endregion


        #region Bounds
        public override RectangleF GetBounds()
        {
            var pathList = PathData.ToPathList();
            pathList = pathList.ToTransformablePathlist();
            var left = float.NaN;
            var right = float.NaN;
            var top = float.NaN;
            var bottom = float.NaN;
            foreach (var item in pathList)
            {
                var b = item.GetBounds();
                if (!float.IsNaN(b.Left))
                {
                    if (float.IsNaN(left))
                    {
                        left = b.Left - StrokeWidth;
                        right = b.Right + StrokeWidth;
                        top = b.Top - StrokeWidth;
                        bottom = b.Bottom + StrokeWidth;
                    }
                    else
                    {
                        left = Math.Min(left, b.Left - StrokeWidth);
                        top = Math.Min(top, b.Top - StrokeWidth);
                        right = Math.Max(right, b.Right + StrokeWidth);
                        bottom = Math.Max(bottom, b.Bottom + StrokeWidth);
                    }
                }
            }
            return new RectangleF(left, top, float.IsNaN(left) ? float.NaN : right - left, float.IsNaN(left) ? float.NaN : bottom - top);
        }
        #endregion

        #region Svg Transform handler
        public override void ApplySvgTransform(Matrix matrix)
        {
            base.ApplySvgTransform(matrix);
            if (string.IsNullOrWhiteSpace(PathData))
                Remove();
            else
            {
                var pathList = PathData.ToPathList();
                pathList = pathList.ToTransformablePathlist();
                foreach (var item in pathList)
                    item.ApplySvgTransform(matrix);
                PathData = pathList.ToPathDataText();

                StrokeWidth *= (Math.Abs(matrix.ScaleX) + Math.Abs(matrix.ScaleY)) / 2f;
            }
        }

        public override void ApplySvgOpacity(float parentOpacity = 1)
        {
            FillAlpha *= parentOpacity * SvgOpacity;
            StrokeAlpha *= parentOpacity * SvgOpacity;
            SvgOpacity = 1;
        }

        public override void PurgeDefaults()
        {
            if (StrokeWidth == 0)
                this.SetAndroidAttributeValue("strokeWidth", null);
            if (StrokeAlpha == 1)
                this.SetAndroidAttributeValue("strokeAlpha", null);
            if (FillAlpha == 1)
                this.SetAndroidAttributeValue("fillAlpha", null);
            if (TrimPathStart == 0)
                this.SetAndroidAttributeValue("trimPathStart", null);
            if (TrimPathEnd == 1)
                this.SetAndroidAttributeValue("trimPathStart", null);
            if (TrimPathOffset == 0)
                this.SetAndroidAttributeValue("trimPathOffset", null);
            if (StrokeLineCap == LineCap.Butt)
                this.SetAndroidAttributeValue("strokeLineCap", null);
            if (StrokeLineJoin == LineJoin.Miter)
                this.SetAndroidAttributeValue("strokeLineJoin", null);
            if (StrokeMiterLimit == 4)
                this.SetAndroidAttributeValue("strokeMiterLimit", null);
            if (Fill == FillType.NonZero)
                this.SetAndroidAttributeValue("fillType", null);
        }
        #endregion
    }
}
