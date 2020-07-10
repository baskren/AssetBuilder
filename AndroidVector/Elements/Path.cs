using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using AndroidVector.Extensions;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using SkiaSharp;

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
            get => GetPropertyAttribute<float>(4f);
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
            if (Attribute(Namespace.AndroidVector + nameof(FillColor).ToCamelCase()) is XAttribute fillAttribute && fillAttribute.Value == "none")
            {
                fillAttribute.Remove();
                FillAlpha = 0;
            }
            if (Attribute(Namespace.AndroidVector + nameof(StrokeColor).ToCamelCase()) is XAttribute strokeAttribute && strokeAttribute.Value == "none")
            {
                strokeAttribute.Remove();
                StrokeAlpha = 0;
            }
        }
        #endregion

        void FillPath(XGraphics gfx, List<PathElement.Base> pathList, XBrush brush)
        {
            var cursor = new XPoint(0, 0);
            var fillPath = new XGraphicsPath
            {
                FillMode = Fill.ToXFillMode()
            };
            PathElement.Base lastItem = null;
            foreach (var item in pathList)
            {
                cursor = item.AddToPath(fillPath, cursor, lastItem);
                lastItem = item;
            }
            gfx.DrawPath(brush, fillPath);
        }

        void StrokePath(XGraphics gfx, List<PathElement.Base> pathList, XBrush brush)
        {
            XPen pen = new XPen(brush, StrokeWidth)
            {
                LineCap = StrokeLineCap.ToXLineCap(),
                LineJoin = StrokeLineJoin.ToXLineJoin(),
                MiterLimit = StrokeMiterLimit,
            };
            StrokePath(gfx, pathList, pen);
        }

        void StrokePath(XGraphics gfx, List<PathElement.Base> pathList, XPen pen)
        {
            var cursor = new XPoint(0, 0);
            var strokePath = new XGraphicsPath();
            PathElement.Base lastItem = null;
            foreach (var item in pathList)
            {
                cursor = item.AddToPath(strokePath, cursor, lastItem);
                lastItem = item;
            }
            gfx.DrawPath(pen, strokePath);
        }

        public override void AddToPdf(XGraphics gfx, List<string> warnings)
        {
            if (PathData?.ToPathList() is List<PathElement.Base> pathList)
            {
                pathList = pathList.ToTransformablePathlist();
                PathData = pathList.ToPathDataText();

                bool fillApplied = false;
                bool strokeApplied = false;
                if (Elements(Namespace.Aapt + "attr") is IEnumerable<XElement> elements && elements.Count() > 0)
                {
                    foreach (var child in elements)
                    {
                        if (child is AaptAttr aaptAttr && aaptAttr.Attribute("name") is XAttribute nameAttribute && (nameAttribute.Value == "android:fillColor" || nameAttribute.Value == "android:strokeColor"))
                        {
                            if (child.Element("gradient") is XElement gradientElement)
                            {
                                ApplyGradient(gfx, gradientElement, pathList, nameAttribute.Value == "android:strokeColor", warnings);
                                if (nameAttribute.Value == "android:strokeColor")
                                    strokeApplied = true;
                                else
                                    fillApplied = true;
                            }
                        }
                    }
                }

                if (!fillApplied && FillColor != Color.Empty && FillAlpha > 0) // && pathList.Any(p=>p is PathElement.ClosePath))
                {
                    XColor fillColor = XColor.FromArgb((int)Math.Round(FillAlpha * 255), FillColor.R, FillColor.G, FillColor.B);
                    XBrush brush = new XSolidBrush(fillColor);
                    FillPath(gfx, pathList, brush);
                }


                if (!strokeApplied && StrokeColor != Color.Empty && StrokeAlpha > 0 && StrokeWidth > 0)
                {
                    XColor penColor = XColor.FromArgb((int)Math.Round(StrokeAlpha * 255), StrokeColor.R, StrokeColor.G, StrokeColor.B);
                    XPen pen = new XPen(penColor, StrokeWidth)
                    {
                        LineCap = StrokeLineCap.ToXLineCap(),
                        LineJoin = StrokeLineJoin.ToXLineJoin(),
                        MiterLimit = StrokeMiterLimit,
                    };
                    StrokePath(gfx, pathList, pen);
                }
            }
        }

        void ApplyGradient(XGraphics gfx, XElement gradientElement, List<PathElement.Base> pathList, bool isStroke, List<string> warnings)
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
                    float xA, xB = 0, yA, yB = 0;
                    foreach (GradientItem item in items)
                    {
                        if (item == items.First())
                        {
                            xA = x1 + (x2 - x1) * -1000;
                            yA = y1 + (y2 - y1) * -1000;
                            xB = x1 + (x2 - x1) * item.Offset;
                            yB = y1 + (y2 - y1) * item.Offset;
                            XBrush brush = new XLinearGradientBrush(new XPoint(xA, yA), new XPoint(xB, yB), item.Color.ToXColor(), item.Color.ToXColor());
                            if (isStroke)
                                StrokePath(gfx, pathList, brush);
                            else
                                FillPath(gfx, pathList, brush);
                        }
                        else
                        {
                            xA = x1 + (x2 - x1) * lastItem.Offset;
                            yA = y1 + (y2 - y1) * lastItem.Offset;
                            xB = x1 + (x2 - x1) * item.Offset;
                            yB = y1 + (y2 - y1) * item.Offset;
                            XBrush brush = new XLinearGradientBrush(new XPoint(xA, yA), new XPoint(xB, yB), lastItem.Color.ToXColor(), item.Color.ToXColor());
                            if (isStroke)
                                StrokePath(gfx, pathList, brush);
                            else
                                FillPath(gfx, pathList, brush);
                            if (item == items.Last())
                            {
                                xA = x1 + (x2 - x1) * item.Offset;
                                yA = y1 + (y2 - y1) * item.Offset;
                                xB = x1 + (x2 - x1) * 1000;
                                yB = y1 + (y2 - y1) * 1000;
                                brush = new XLinearGradientBrush(new XPoint(xA, yA), new XPoint(xB, yB), item.Color.ToXColor(), item.Color.ToXColor());
                                if (isStroke)
                                    StrokePath(gfx, pathList, brush);
                                else
                                    FillPath(gfx, pathList, brush);
                            }
                        }
                        lastItem = item;
                    }
                }
                else
                {
                    XBrush brush = new XLinearGradientBrush(new XPoint(x1 + (x2 - x1) * -1000, y1 + (y2 - y1) * -1000), new XPoint(x1, y1), linearGradient.StartColor.ToXColor(), linearGradient.StartColor.ToXColor());
                    if (isStroke)
                        StrokePath(gfx, pathList, brush);
                    else
                        FillPath(gfx, pathList, brush);
                    brush = new XLinearGradientBrush(new XPoint(x1, y1), new XPoint(x2, y2), linearGradient.StartColor.ToXColor(), linearGradient.EndColor.ToXColor());
                    if (isStroke)
                        StrokePath(gfx, pathList, brush);
                    else
                        FillPath(gfx, pathList, brush);
                    brush = new XLinearGradientBrush(new XPoint(x2, y2), new XPoint(x1 + (x2 - x1) * 1000, y1 + (y2 - y1) * 1000), linearGradient.EndColor.ToXColor(), linearGradient.EndColor.ToXColor());
                    if (isStroke)
                        StrokePath(gfx, pathList, brush);
                    else
                        FillPath(gfx, pathList, brush);
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
                    //float rA = 0, rB = 0;
                    foreach (GradientItem item in items)
                    {
                        if (item == items.First())
                        {
                            if (item.Offset > 0)
                            {
                                XBrush brush = new XRadialGradientBrush(c, 0, r * item.Offset, item.Color.ToXColor(), item.Color.ToXColor());
                                if (isStroke)
                                    StrokePath(gfx, pathList, brush);
                                else
                                    FillPath(gfx, pathList, brush);
                            }
                        }
                        else
                        {
                            XBrush brush = new XRadialGradientBrush(c, r * lastItem.Offset, r * item.Offset, lastItem.Color.ToXColor(), item.Color.ToXColor());
                            if (isStroke)
                                StrokePath(gfx, pathList, brush);
                            else
                                FillPath(gfx, pathList, brush);

                            if (item == items.Last())
                            {
                                brush = new XRadialGradientBrush(c, r * item.Offset, r * 100, item.Color.ToXColor(), item.Color.ToXColor());
                                if (isStroke)
                                    StrokePath(gfx, pathList, brush);
                                else
                                    FillPath(gfx, pathList, brush);
                            }

                        }
                        lastItem = item;
                    }

                }
                else
                {
                    XBrush brush = new XRadialGradientBrush(c, 0, radialGradient.GradientRadius, radialGradient.StartColor.ToXColor(), radialGradient.EndColor.ToXColor());
                    if (isStroke)
                        StrokePath(gfx, pathList, brush);
                    else
                        FillPath(gfx, pathList, brush);
                    brush = new XRadialGradientBrush(c, radialGradient.GradientRadius, radialGradient.GradientRadius * 100, radialGradient.StartColor.ToXColor(), radialGradient.EndColor.ToXColor());
                    if (isStroke)
                        StrokePath(gfx, pathList, brush);
                    else
                        FillPath(gfx, pathList, brush);
                }
            }

        }

        public override void AddToSKCanvas(SKCanvas canvas, bool antiAlias = true)
        {
            if (PathData?.ToPathList() is List<PathElement.Base> pathList)
            {
                var path = SKPath.ParseSvgPathData(PathData);
                path.FillType = Fill.ToSkPathFillType();

                SKPaint fillPaint = null;
                SKPaint strokePaint = null;
                if (Elements(Namespace.Aapt + "attr") is IEnumerable<XElement> elements && elements.Count() > 0)
                {
                    foreach (var child in elements)
                    {
                        if (child is AaptAttr aaptAttr && aaptAttr.Attribute("name") is XAttribute nameAttribute && (nameAttribute.Value == "android:fillColor" || nameAttribute.Value == "android:strokeColor"))
                        {
                            if (child.Element("gradient") is XElement gradientElement)
                            {
                                SKShader shader = null;
                                if (gradientElement is LinearGradient linearGradient)
                                {
                                    var x1 = linearGradient.StartX;
                                    var y1 = linearGradient.StartY;
                                    var x2 = linearGradient.EndX;
                                    var y2 = linearGradient.EndY;

                                    if (linearGradient.Elements("item") is IEnumerable<XElement> children)
                                    {
                                        var items = children.ToList();
                                        items.Sort();
                                        var offsets = new List<float>();
                                        var colors = new List<SKColor>();
                                        foreach (GradientItem item in items)
                                        {
                                            offsets.Add(item.Offset);
                                            colors.Add(item.Color.ToSKColor());
                                        }
                                        shader = SKShader.CreateLinearGradient(
                                                new SKPoint(x1, y1),
                                                new SKPoint(x2, y2),
                                                colors.ToArray(),
                                                offsets.ToArray(),
                                                linearGradient.TileMode.ToSKShaderTileMode());
                                    }
                                    else
                                    {
                                        shader = SKShader.CreateLinearGradient(
                                            new SKPoint(x1, y1),
                                            new SKPoint(x2, y2),
                                            new SKColor[] { linearGradient.StartColor.ToSKColor(), linearGradient.EndColor.ToSKColor() },
                                            new float[] { 0, 1 },
                                            linearGradient.TileMode.ToSKShaderTileMode());
                                    }
                                }
                                else if (gradientElement is RadialGradient radialGradient)
                                {
                                    var cx = radialGradient.CenterX;
                                    var cy = radialGradient.CenterY;
                                    var c = new SKPoint(cx, cy);
                                    var r = radialGradient.GradientRadius;
                                    if (radialGradient.Elements("item") is IEnumerable<XElement> children)
                                    {
                                        var items = children.ToList();
                                        items.Sort();
                                        var offsets = new List<float>();
                                        var colors = new List<SKColor>();
                                        foreach (GradientItem item in items)
                                        {
                                            offsets.Add(item.Offset);
                                            colors.Add(item.Color.ToSKColor());
                                        }
                                        shader = SKShader.CreateRadialGradient(
                                            c,
                                            r,
                                            colors.ToArray(),
                                            offsets.ToArray(),
                                            radialGradient.TileMode.ToSKShaderTileMode()
                                            );
                                    }
                                    else
                                    {
                                        shader = SKShader.CreateRadialGradient(
                                            c,
                                            r,
                                            new SKColor[] { radialGradient.StartColor.ToSKColor(), radialGradient.EndColor.ToSKColor() },
                                            new float[] { 0, 1 },
                                            radialGradient.TileMode.ToSKShaderTileMode()
                                            );
                                    }
                                }

                                if (nameAttribute.Value == "android:fillColor")
                                    fillPaint = new SKPaint
                                    {
                                        Style = SKPaintStyle.Fill,
                                        IsAntialias = antiAlias,
                                        Shader = shader,
                                    };
                                else
                                    strokePaint = new SKPaint
                                    {
                                        Style = SKPaintStyle.Stroke,
                                        IsAntialias = antiAlias,
                                        Shader = shader,
                                        StrokeCap = StrokeLineCap.ToSKStrokeCap(),
                                        StrokeJoin = StrokeLineJoin.ToSKLineJoin(),
                                        StrokeMiter = StrokeMiterLimit,
                                        StrokeWidth = StrokeWidth,
                                    };
                            }
                        }
                    }
                }

                if (fillPaint == null && FillColor != Color.Empty && FillAlpha > 0) // && pathList.Any(p=>p is PathElement.ClosePath))
                {
                    fillPaint = new SKPaint
                    {
                        Style = SKPaintStyle.Fill,
                        IsAntialias = antiAlias,
                        Color = FillColor.ToSKColor().WithAlpha((byte)Math.Round(FillAlpha * 255)),
                    };
                }
                if (fillPaint != null)
                    canvas.DrawPath(path, fillPaint);


                if (strokePaint == null && StrokeColor != Color.Empty && StrokeAlpha > 0 && StrokeWidth > 0)
                {
                    strokePaint = new SKPaint
                    {
                        Style = SKPaintStyle.Stroke,
                        IsAntialias = antiAlias,
                        Color = StrokeColor.ToSKColor().WithAlpha((byte)Math.Round(StrokeAlpha * 255)),
                        StrokeCap = StrokeLineCap.ToSKStrokeCap(),
                        StrokeJoin = StrokeLineJoin.ToSKLineJoin(),
                        StrokeMiter = StrokeMiterLimit,
                        StrokeWidth = StrokeWidth
                    };
                }
                if (strokePaint != null)
                    canvas.DrawPath(path, strokePaint);
            }
        }
    }
}
