using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using AndroidVector.Extensions;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

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
            get => GetColorPropertyAttribute(Color.Black);
            set => SetPropertyAttribute(value);
        }

        public Color StrokeColor
        {
            get => GetColorPropertyAttribute(Color.Empty);
            set => SetPropertyAttribute(value);
        }

        public float StrokeWidth
        {
            get => GetPropertyAttribute<float>();
            set => SetPropertyAttribute(value);
        }

        public float StrokeAlpha
        {
            get => GetAlphaPropertyAttribute();
            set => SetPropertyAttribute(value);
        }

        public float FillAlpha
        {
            get => GetAlphaPropertyAttribute();
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
            get => GetEnumPropertyAttribute<FillType>(default, "FillType");
            set => SetPropertyAttribute(value, "FillType");
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

        void FillPath(XGraphics gfx, List<PathElement.Base> pathList, XBrush fill)
        {
            var cursor = new XPoint(0, 0);
            var fillPath = new XGraphicsPath
            {
                FillMode = Fill == FillType.EvenOdd ? XFillMode.Alternate : XFillMode.Winding
            };
            PathElement.Base lastItem = null;
            foreach (var item in pathList)
            {
                cursor = item.AddToPath(fillPath, cursor, lastItem);
                lastItem = item;
            }
            gfx.DrawPath(fill, fillPath);
        }

        public override void AddToPdf(XGraphics gfx)
        {

            if (StrokeColor.A == 255 && StrokeColor.R == 255 && StrokeColor.G == 255 && StrokeColor.B == 255)
                System.Diagnostics.Debug.WriteLine("Path");


            if (PathData?.ToPathList() is List<PathElement.Base> pathList)
            {
                pathList = pathList.ToTransformablePathlist();
                PathData = pathList.ToPathDataText();


                if (Elements(Namespace.Aapt + "attr") is IEnumerable<XElement> elements && elements.Count() > 0)
                {
                    foreach (var child in elements)
                    {
                        if (child is AaptAttr aaptAttr && aaptAttr.Attribute("name") is XAttribute nameAttribute && nameAttribute.Value == "android:fillColor")
                        {
                            if (child.Element("gradient") is XElement gradientElement)
                            {
                                if (gradientElement is LinearGradient linearGradient)
                                {
                                    var x1 = linearGradient.StartX;
                                    var y1 = linearGradient.StartY;
                                    var x2 = linearGradient.EndX;
                                    var y2 = linearGradient.EndY;

                                    if (linearGradient.Elements("item") is IEnumerable<XElement> children)
                                    {
                                        GradientItem lastItem = null;
                                        var items = children.ToList();
                                        items.Sort();
                                        float xA, xB=0, yA, yB=0;
                                        foreach (GradientItem item in items)
                                        {
                                            if (item == items.First())
                                            {
                                                xA = x1 + (x2 - x1) * -1000;
                                                yA = y1 + (y2 - y1) * -1000;
                                                xB = x1 + (x2 - x1) * item.Offset;
                                                yB = y1 + (y2 - y1) * item.Offset;
                                                XBrush fill = new XLinearGradientBrush(new XPoint(xA, yA), new XPoint(xB, yB), item.Color.ToXColor(), item.Color.ToXColor());
                                                FillPath(gfx, pathList, fill);
                                            }
                                            else
                                            {
                                                xA = x1 + (x2 - x1) * lastItem.Offset;
                                                yA = y1 + (y2 - y1) * lastItem.Offset;
                                                xB = x1 + (x2 - x1) * item.Offset;
                                                yB = y1 + (y2 - y1) * item.Offset;
                                                XBrush fill = new XLinearGradientBrush(new XPoint(xA, yA), new XPoint(xB, yB), lastItem.Color.ToXColor(), item.Color.ToXColor());
                                                FillPath(gfx, pathList, fill);
                                                if (item == items.Last())
                                                {
                                                    xA = x1 + (x2 - x1) * item.Offset; 
                                                    yA = y1 + (y2 - y1) * item.Offset;
                                                    xB = x1 + (x2 - x1) * 1000;
                                                    yB = y1 + (y2 - y1) * 1000;
                                                    fill = new XLinearGradientBrush(new XPoint(xA, yA), new XPoint(xB, yB), item.Color.ToXColor(), item.Color.ToXColor());
                                                    FillPath(gfx, pathList, fill);
                                                }
                                            }
                                            lastItem = item;
                                        }
                                    }
                                    else
                                    {
                                        XBrush fill = new XLinearGradientBrush(new XPoint(x1 + (x2 - x1) * -1000, y1 + (y2 - y1) * -1000), new XPoint(x1, y1), linearGradient.StartColor.ToXColor(), linearGradient.StartColor.ToXColor());
                                        FillPath(gfx, pathList, fill);
                                        fill = new XLinearGradientBrush(new XPoint(x1, y1), new XPoint(x2, y2), linearGradient.StartColor.ToXColor(), linearGradient.EndColor.ToXColor());
                                        FillPath(gfx, pathList, fill);
                                        fill = new XLinearGradientBrush(new XPoint(x2, y2), new XPoint(x1 + (x2 - x1) * 1000, y1 + (y2 - y1) * 1000), linearGradient.EndColor.ToXColor(), linearGradient.EndColor.ToXColor());
                                        FillPath(gfx, pathList, fill);
                                    }
                                }
                                else if (gradientElement is RadialGradient radialGradient)
                                {
                                    var cx = radialGradient.CenterX;
                                    var cy = radialGradient.CenterY;
                                    var c = new XPoint(cx, cy);
                                    var r = radialGradient.GradientRadius;
                                    if (radialGradient.Elements("item") is IEnumerable<XElement> children)
                                    {
                                        GradientItem lastItem = null;
                                        var items = children.ToList();
                                        items.Sort();
                                        float rA = 0, rB = 0;
                                        foreach (GradientItem item in items)
                                        {
                                            if (item == items.First())
                                            {
                                                if (item.Offset > 0)
                                                {
                                                    XBrush fill = new XRadialGradientBrush(c, 0,  r * item.Offset, item.Color.ToXColor(), item.Color.ToXColor());
                                                    FillPath(gfx, pathList, fill);
                                                }
                                            }
                                            else
                                            {
                                                XBrush fill = new XRadialGradientBrush(c, r * lastItem.Offset, r * item.Offset, lastItem.Color.ToXColor(), item.Color.ToXColor());
                                                FillPath(gfx, pathList, fill);
                                                
                                                if (item == items.Last())
                                                {
                                                    fill = new XRadialGradientBrush(c, r * item.Offset, r * 100, item.Color.ToXColor(), item.Color.ToXColor());
                                                    FillPath(gfx, pathList, fill);
                                                }
                                                
                                            }
                                            lastItem = item;
                                        }

                                    }
                                    else
                                    {
                                        XBrush fill = new XRadialGradientBrush(c, 0, radialGradient.GradientRadius, radialGradient.StartColor.ToXColor(), radialGradient.EndColor.ToXColor());
                                        FillPath(gfx, pathList, fill);
                                        fill = new XRadialGradientBrush(c, radialGradient.GradientRadius, radialGradient.GradientRadius * 100, radialGradient.StartColor.ToXColor(), radialGradient.EndColor.ToXColor());
                                        FillPath(gfx, pathList, fill);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (FillColor != Color.Empty && FillAlpha > 0) // && pathList.Any(p=>p is PathElement.ClosePath))
                {
                    XColor fillColor = XColor.FromArgb((int)Math.Round(FillAlpha * 255), FillColor.R, FillColor.G, FillColor.B);
                    XBrush fill = new XSolidBrush(fillColor);
                    FillPath(gfx, pathList, fill);
                }

                if (StrokeColor != Color.Empty && StrokeAlpha > 0 && StrokeWidth > 0)
                {
                    var cursor = new XPoint(0, 0);
                    XColor penColor = XColor.FromArgb((int)Math.Round(StrokeAlpha * 255), StrokeColor.R, StrokeColor.G, StrokeColor.B);
                    XPen pen = new XPen(penColor, StrokeWidth)
                    {
                        LineCap = StrokeLineCap.ToXLineCap(),
                        LineJoin = StrokeLineJoin.ToXLineJoin(),
                        MiterLimit = StrokeMiterLimit
                    };
                    var strokePath = new XGraphicsPath();
                    PathElement.Base lastItem = null;
                    //XPoint lastMovePoint = cursor;
                    foreach (var item in pathList)
                    {
                        //if (item is PathElement.ClosePath)
                        //    strokePath.AddLine(cursor, lastMovePoint);
                        //else if (item is PathElement.MoveTo moveTo)
                        //{
                        //    strokePath.CloseFigure();
                        //    gfx.DrawPath(pen, strokePath);
                        //    strokePath = new XGraphicsPath();
                        //    strokePath.StartFigure();
                        //}
                        cursor = item.AddToPath(strokePath, cursor, lastItem);
                        lastItem = item;
                        //if (item is PathElement.MoveTo)
                        //    lastMovePoint = cursor;
                    }
                    gfx.DrawPath(pen, strokePath);
                }
            }
        }
    }
}
