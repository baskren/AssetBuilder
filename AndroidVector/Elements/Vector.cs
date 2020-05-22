using System;
using System.Drawing;
using System.Xml;
using System.Xml.Linq;
using PdfSharpCore.Drawing;

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

        public void ToPdfDocument(string filePath)
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
            AddToPdf(gfx);

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
    }
}
