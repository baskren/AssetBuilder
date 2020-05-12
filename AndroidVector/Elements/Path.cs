using System;
using System.Drawing;
using AndroidVector.Extensions;

namespace AndroidVector
{
    public class Path : BaseElement
    {
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

        public Path() : base("path")
        {
        }

        public override void ApplySvgTransform(Matrix matrix)
        {
            base.ApplySvgTransform(matrix);
            var pathList = PathData.ToPathList();
            pathList = pathList.ToTransformablePathlist();
            foreach (var item in pathList)
                item.ApplySvgTransform(matrix);
            PathData = pathList.ToPathDataText();
        }
    }
}
