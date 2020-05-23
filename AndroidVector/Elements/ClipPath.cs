using System;
using System.Collections.Generic;
using AndroidVector.Extensions;
using PdfSharpCore.Drawing;

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

        public override void AddToPdf(XGraphics gfx)
        {
            var cursor = new XPoint(0, 0);
            var clipPath = new XGraphicsPath();
            clipPath.FillMode = XFillMode.Winding;
            PathElement.Base lastItem = null;

            if (PathData?.ToPathList() is List<PathElement.Base> pathList)
            {
                pathList = pathList.ToTransformablePathlist();
                PathData = pathList.ToPathDataText();
                foreach (var item in pathList)
                {
                    cursor = item.AddToPath(clipPath, cursor, lastItem);
                    lastItem = item;
                }

                gfx.IntersectClip(clipPath);
            }
        }
    }
}
