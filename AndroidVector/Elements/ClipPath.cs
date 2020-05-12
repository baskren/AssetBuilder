using System;
using AndroidVector.Extensions;

namespace AndroidVector
{
    public class ClipPath : BaseElement
    {
        public string PathData
        {
            get => GetPropertyAttribute<string>();
            set => SetPropertyAttribute(value);
        }

        public ClipPath() : base("clip-path")
        {
        }

        public ClipPath(string value) : this()
        {
            PathData = value;
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
