using System;
using System.Collections.Generic;

namespace SvgBuilder.StyleAttributes
{
    public class ColorInterpolation : Base<string>
    {
        public const string Symbol = "color-interpolation";
        public ColorInterpolation(string value) : base(Symbol, value) { }
        public static ColorInterpolation FromAttributes(string s)
            => new ColorInterpolation(s);
    }

    public class ColorInterpolationFilters : Base<string>
    {
        public const string Symbol = "color-interpolation-filters";
        public ColorInterpolationFilters(string value) : base(Symbol, value) { }
        public static ColorInterpolationFilters FromAttributes(string s)
            => new ColorInterpolationFilters(s);
    }

    public class ColorProfile : Base<string>
    {
        public const string Symbol = "color-profile";
        public ColorProfile(string value) : base(Symbol, value) { }
        public static ColorProfile FromAttributres(string s)
            => new ColorProfile(s);
    }

    public class ColorRendering : Base<Enums.Rendering>
    {
        public const string Symbol = "color-rendering";
        public ColorRendering(Enums.Rendering colorRendering) : base(Symbol, colorRendering) { }
        public static ColorRendering FromAttributes(string s)
            => EnumWrapperInstanceAttributes<ColorRendering, Enums.Rendering>(s);
    }

    public class Fill : Base<Paint.Base>
    {
        public const string Symbol = "fill";
        public Fill(Paint.Base paint) : base(Symbol, paint) { }
        public static Fill FromAttributes(string s)
        {
            if (Paint.Base.PaintFromAttributes(s) is Paint.Base paint)
                return new Fill(paint);
            return null;
        }
    }

    public class FillOpacity : Base<float>
    {
        public const string Symbol = "fill-opacity";
        public FillOpacity(float value) : base(Symbol, value) { }
        public static FillOpacity FromAttributes(string s)
        {
            if (float.TryParse(s, out float value))
                return new FillOpacity(value);
            return null;
        }
    }

    public class FillRule : Base<Enums.Rule>
    {
        public const string Symbol = "fill-rule";
        public FillRule(Enums.Rule value) : base(Symbol,value) { }
        public static FillRule FromAttributes(string s)
            => EnumWrapperInstanceAttributes<FillRule, Enums.Rule>(s);
    }

    public class ImageRendering : Base<Enums.Rendering>
    {
        public const string Symbol = "image-rendering";
        public ImageRendering(Enums.Rendering value) : base(Symbol, value) { }
        public static ImageRendering FromAttributes(string s)
            => EnumWrapperInstanceAttributes<ImageRendering, Enums.Rendering>(s);
    }

    public class Marker : Base<string>
    {
        public const string Symbol = "marker";
        public Marker(string value) : base(Symbol, value) { }
        public static Marker FromAttributes(string s)
            => new Marker(s);
    }

    public class MarkerEnd : Base<string>
    {
        public const string Symbol = "marker-end";
        public MarkerEnd(string value) : base(Symbol, value) { }
        public static MarkerEnd FromAttributes(string s)
            => new MarkerEnd(s);
    }

    public class MarkerMid : Base<string>
    {
        public const string Symbol = "marker-mid";
        public MarkerMid(string value) : base(Symbol, value) { }
        public static MarkerMid FromAttributes(string s)
            => new MarkerMid(s);
    }

    public class MarkerStart : Base<string>
    {
        public const string Symbol = "marker-start";
        public MarkerStart(string value) : base(Symbol, value) { }
        public static MarkerStart FromAttributes(string s)
            => new MarkerStart(s);
    }

    public class ShapeRendering : Base<Enums.ShapeRendering>
    {
        public const string Symbol = "shape-rendering";
        public ShapeRendering(Enums.ShapeRendering value) : base(Symbol, value) { }
        public static ShapeRendering FromAttributes(string s)
            => EnumWrapperInstanceAttributes<ShapeRendering, Enums.ShapeRendering>(s);
    }

    public class Stroke : Base<Paint.Base>
    {
        public const string Symbol = "stroke";
        public Stroke(Paint.Base value) : base(Symbol, value) { }
        public static Stroke FromAttributes(string s)
        {
            if (Paint.Base.PaintFromAttributes(s) is Paint.Base paint)
                return new Stroke(paint);
            return null;
        }
    }

    public class StrokeDashArray : Base<string>
    {
        public const string Symbol = "stroke-dasharray";
        public StrokeDashArray(string value) : base(Symbol, value) { }
        public static StrokeDashArray FromAttributes(string s)
            => new StrokeDashArray(s);
    }

    public class StrokeDashOffset : Base<string>
    {
        public const string Symbol = "stroke-dashoffset";
        public StrokeDashOffset(string value) : base(Symbol, value) { }
        public static StrokeDashOffset FromAttributes(string s)
            => new StrokeDashOffset(s);
    }

    public class StrokeLineCap : Base<Enums.LineCap>
    {
        public const string Symbol = "stroke-linecap";
        public StrokeLineCap(Enums.LineCap value) : base(Symbol, value) { }
        public static StrokeLineCap FromAttributes(string s)
            => EnumWrapperInstanceAttributes<StrokeLineCap, Enums.LineCap>(s);
    }

    public class StrokeLineJoin : Base<Enums.LineJoin>
    {
        public const string Symbol = "stroke-linejoin";
        public StrokeLineJoin(Enums.LineJoin value) : base(Symbol, value) { }
        public static StrokeLineJoin FromAttributes(string s)
            => EnumWrapperInstanceAttributes<StrokeLineJoin, Enums.LineJoin>(s);
    }

    public class StrokeMiterLimit : Base<string>
    {
        public const string Symbol = "stroke-miterlimit";
        public StrokeMiterLimit(string value) : base(Symbol, value) { }
        public static StrokeMiterLimit FromAttributes(string s)
            => new StrokeMiterLimit(s);
    }

    public class StrokeOpacity : Base<string>
    {
        public const string Symbol = "stroke-opacity";
        public StrokeOpacity(string value) : base(Symbol, value) { }
        public static StrokeOpacity FromAttributes(string s)
            => new StrokeOpacity(s);
    }

    public class StrokeWidth : Base<string>
    {
        public const string Symbol = "stroke-width";
        public StrokeWidth(string value) : base(Symbol, value) { }
        public static StrokeWidth FromAttributes(string s)
            => new StrokeWidth(s);
    }
}
