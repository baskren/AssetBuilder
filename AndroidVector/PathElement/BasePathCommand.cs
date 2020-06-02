using System;
using System.Drawing;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace AndroidVector.PathElement
{
    public class Base

    {
        public char Symbol { get; private set; }

        public bool IsRelative { get; internal set; }

        internal Base(char symbol, bool relative)
        {
            Symbol = char.ToUpper(symbol);
            IsRelative = relative;
        }

        public bool ValidFirstToken(string s)
        {
            s = s.Trim();
            return char.ToUpper(s[0]) == char.ToUpper(Symbol);
        }

        public override string ToString()
        {
            return (IsRelative ? char.ToLower(Symbol) : char.ToUpper(Symbol)).ToString();
        }

        public virtual SizeF ToAbsolute(SizeF cursor)
            => throw new NotImplementedException();

        public virtual void ApplySvgTransform(Matrix matrix)
            => throw new NotImplementedException();

        public virtual RectangleF GetBounds()
            => throw new NotImplementedException();

        public virtual XPoint AddToPath(XGraphicsPath path, XPoint cursor, Base lastPathCommand = null)
            => throw new NotImplementedException();

    }
}
