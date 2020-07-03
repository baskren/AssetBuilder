using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
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


        public List<string> ToPdfDocument(System.IO.Stream stream, Color backgroundColor)
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

            var warnings = new List<string>();
            using (XGraphics gfx = XGraphics.FromPdfPage(page))
            {
                gfx.DrawRectangle(new XSolidBrush(backgroundColor.ToXColor()), new XRect(-72, -72, page.Width.Point + 72, page.Height.Point + 72));
                gfx.ScaleTransform(page.Width / ViewportWidth, page.Height / ViewportHeight);
                AddToPdf(gfx, warnings);
            }

            doc.Save(stream);
            return warnings;
        }

        public List<string> ToPdfDocument(string filePath, Color backgroundColor)
        {
            using (var stream = System.IO.File.OpenWrite(filePath))
            {
                return ToPdfDocument(stream, backgroundColor);
            }
        }

        public async Task<List<string>> ToPdfDocumentAsync(P42.Storage.IStorageFile storageFile, Color backgroundColor)
        {
            using (var stream = new System.IO.MemoryStream())
            {
                var results = ToPdfDocument(stream, backgroundColor);
                var bytes = stream.ToArray();
                await storageFile.WriteAllBytesAsync(bytes);
                return results;
            }
        }

        /// <summary>
        /// Generates a PNG from the AndroidVector
        /// </summary>
        /// <param name="stream">where to save the PNG</param>
        /// <param name="backgroundColor">background color for the PNG</param>
        /// <param name="imageSize">size of vector image (centered) in the PNG</param>
        /// <param name="bitmapSize">size of the PNG</param>
        public void ToPng(System.IO.Stream stream, Color backgroundColor, Size imageSize = default, Size bitmapSize = default)
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

                    var hScale = imageSize.Width / ViewportWidth;
                    var vScale = imageSize.Height / ViewportHeight;
                    var scale = (float)Math.Min(hScale, vScale);

                    var scaledWidth = (float)(scale * ViewportWidth);
                    var scaledHeight = (float)(scale * ViewportHeight);

                    canvas.Clear(new SKColor(r, g, b, a));
                    canvas.Translate(bitmapSize.Width / 2 - scaledWidth / 2, bitmapSize.Height / 2 - scaledHeight / 2);
                    canvas.Scale(scale);
                    AddToSKCanvas(canvas);

                    using (var image = SKImage.FromBitmap(bitmap))
                    {
                        var skdata = image.Encode(SKEncodedImageFormat.Png, 50);
                        skdata.SaveTo(stream);
                    }
                }
            }
        }



        /// <summary>
        /// Generates a PNG from the AndroidVector
        /// </summary>
        /// <param name="file">where to save the PNG</param>
        /// <param name="backgroundColor">background color for the PNG</param>
        /// <param name="imageSize">size of vector image (centered) in the PNG</param>
        /// <param name="bitmapSize">size of the PNG</param>
        public async Task ToPngAsync(P42.Storage.IStorageFile file, Color backgroundColor, Size imageSize = default, Size bitmapSize = default)
        {
            using (var stream = new System.IO.MemoryStream())
            {
                ToPng(stream, backgroundColor, imageSize, bitmapSize);
                var bytes = stream.ToArray();
                await file.WriteAllBytesAsync(bytes);
            }
        }

        /// <summary>
        /// Generates a PNG from the AndroidVector
        /// </summary>
        /// <param name="path">where to save the PNG</param>
        /// <param name="backgroundColor">background color for the PNG</param>
        /// <param name="imageSize">size of vector image (centered) in the PNG</param>
        /// <param name="bitmapSize">size of the PNG</param>
        public void ToPng(string path, Color backgroundColor,  Size imageSize = default, Size bitmapSize = default)
        {
            using (var stream = System.IO.File.OpenWrite(path))
            {
                ToPng(stream, backgroundColor, imageSize, bitmapSize);
            }
        }

        public override string ToString()
         {
            string xmlHeader = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n";
            var text = xmlHeader + base.ToString();
            return text;
        }

        public Vector AspectClone(float aspectRatio = 1)
        {
            var vector = this.Copy();

            var width = vector.Width.As(AndroidVector.Unit.Dp);
            var height = vector.Height.As(AndroidVector.Unit.Dp);
            var orginalAspect = width / height;
            if (orginalAspect != aspectRatio)
            {
                var viewportWidth = vector.ViewportWidth;
                var viewportHeight = vector.ViewportHeight;

                if (orginalAspect > aspectRatio)
                {
                    var heightScale = orginalAspect / aspectRatio;
                    var heightMoveScale = (width - height) / 2 / height;
                    vector.Height = new AndroidVector.UnitizedFloat(width / aspectRatio, AndroidVector.Unit.Dp);
                    vector.ViewportHeight *= heightScale;
                    vector.SvgTransforms.Add(AndroidVector.Matrix.CreateTranslate(0, (float)(viewportHeight * heightMoveScale)));
                    vector.ApplySvgTransforms();
                    vector.PurgeDefaults();
                }
                else 
                {
                    var widthScale = aspectRatio / orginalAspect;
                    var widthOffsetScale =  (height - width) / 2 / width;
                    vector.Width = new AndroidVector.UnitizedFloat(height, AndroidVector.Unit.Dp);
                    vector.ViewportWidth *= widthScale;
                    vector.SvgTransforms.Add(AndroidVector.Matrix.CreateTranslate((float)(viewportWidth * widthOffsetScale), 0));
                    vector.ApplySvgTransforms();
                    vector.PurgeDefaults();
                }
            }
            return vector;
        }
    }
}
