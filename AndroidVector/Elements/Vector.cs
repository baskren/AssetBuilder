using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using PdfSharpCore.Drawing;
using SkiaSharp;

namespace AndroidVector
{
    public class Vector : Group
    {
        public UnitizedFloat Width
        {
            get => GetPropertyAttribute<UnitizedFloat>();
            set => SetPropertyAttribute(value);
        }

        public UnitizedFloat Height
        {
            get => GetPropertyAttribute<UnitizedFloat>();
            set => SetPropertyAttribute(value);
        }

        public double ViewportWidth
        {
            get => GetPropertyAttribute<double>();
            set => SetPropertyAttribute(value);
        }

        public double ViewportHeight
        {
            get => GetPropertyAttribute<double>();
            set => SetPropertyAttribute(value);
        }

        public Color Tint
        {
            get => GetColorPropertyAttribute();
            set => SetPropertyAttribute(value);
        }

        public TintMode TintMode
        {
            get => GetEnumPropertyAttribute<TintMode>();
            set => SetPropertyAttribute(value);
        }

        public bool AutoMirrored
        {
            get => GetPropertyAttribute<bool>();
            set => SetPropertyAttribute(value);
        }

        public double Alpha
        {
            get => GetPropertyAttribute<double>();
            set => SetPropertyAttribute(value);
        }

        public Vector() : base("vector")
        {
            Add(new XAttribute(XNamespace.Xmlns + "android", Namespace.AndroidVector));
            Add(new XAttribute(XNamespace.Xmlns + "aapt", Namespace.Aapt));
        }


        public string ToPdfDocument(string filePath)
        {
            var doc = new PdfSharpCore.Pdf.PdfDocument();
            if (doc.Version < 14)
                doc.Version = 14;
            doc.Info.Title = "LaunchImage PDF";
            doc.Info.Author = "Xamarin.AssetBuilder";
            doc.Info.Subject = "LaunchImage PDF built with Xamarin.AssetBuilder.";
            doc.Info.Keywords = "iOS LaunchImage Xamarin";
            PdfSharpCore.Pdf.PdfPage page = doc.AddPage();

            page.Width = Width.As(Unit.Pt);
            page.Height = Height.As(Unit.Pt);

            XGraphics gfx = XGraphics.FromPdfPage(page);
            gfx.ScaleTransform(page.Width / ViewportWidth, page.Height / ViewportHeight);
            var warnings = new List<string>();
            AddToPdf(gfx, warnings);

            /*
            XPen pen = new XPen(XColors.Navy, Math.PI);
            pen.DashStyle = XDashStyle.Dash;
            var color = XColor.FromName("DarkGoldenrod");
            //color.A = 0.5;
            XSolidBrush brush = new XSolidBrush(color);

            XGraphicsPath path = new XGraphicsPath();
            //path.AddMove(100,100);
            path.AddArc(new XPoint(100,   0), new XPoint(200, 100), new XSize(100, 100), 0, false, XSweepDirection.Clockwise);
            path.AddArc(new XPoint(200, 100), new XPoint(100, 200), new XSize(100, 100), 0, false, XSweepDirection.Clockwise);
            path.AddArc(new XPoint(100, 200), new XPoint(  0, 100), new XSize(100, 100), 0, false, XSweepDirection.Clockwise);
            path.AddArc(new XPoint(  0, 100), new XPoint(100,   0), new XSize(100, 100), 0, false, XSweepDirection.Clockwise);
            //path.AddLine(10, 120, 50, 60);
            //path.AddArc(50, 20, 110, 80, 180, 180);
            //path.AddArc(new XPoint(50, 60), new XPoint(160, 60), new XSize(55, 40), 0, true, XSweepDirection.Clockwise);
            //path.AddLine(160, 60, 220, 100);
            path.CloseFigure();
            gfx.DrawPath(pen, brush, path);
            */

            doc.Save(filePath);
        }

        public void ToPng(string path, Color backgroundColor,  Size imageSize = default, Size bitmapSize = default)
        {
             if (imageSize == default)
                imageSize = new Size((int)Width.As(Unit.Dp), (int)Height.As(Unit.Dp));
            if (bitmapSize == default)
                bitmapSize = imageSize;
            using (var bitmap = new SKBitmap(bitmapSize.Width, bitmapSize.Height))
            {
                using (var canvas = new SKCanvas(bitmap))
                {
                    var r = backgroundColor.R;
                    var g = backgroundColor.G;
                    var b = backgroundColor.B;
                    var a = backgroundColor.A;
                    canvas.Clear(new SKColor(r, g, b, a));
                    canvas.Translate(bitmapSize.Width / 2 - imageSize.Width / 2, bitmapSize.Height / 2 - imageSize.Height / 2);
                    canvas.Scale((float)(imageSize.Width / ViewportWidth), (float)(imageSize.Height / ViewportHeight));
                    AddToSKCanvas(canvas);

                    using (var image = SKImage.FromBitmap(bitmap))
                    {
                        var skdata = image.Encode(SKEncodedImageFormat.Png, 50);
                        using (var stream = System.IO.File.OpenWrite(path))
                        {
                            skdata.SaveTo(stream);
                        }
                    }
                }
            }
        }

        public override string ToString()
         {
            string xmlHeader = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n";
            var text = xmlHeader + base.ToString();
            return text;
        }

    }
}
