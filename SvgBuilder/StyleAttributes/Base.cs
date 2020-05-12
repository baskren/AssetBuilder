using System;
using System.Collections.Generic;

namespace SvgBuilder.StyleAttributes
{
    public class Base<T>
    {
        public static Dictionary<string, Type> SymbolTypeMap = new Dictionary<string, Type>
        {
            { Clip.Symbol, typeof(Clip) },
            { ClipPath.Symbol, typeof(ClipPath) },
            { ClipRule.Symbol, typeof(ClipRule) },
            { Mask.Symbol, typeof(Mask) },
            { Opacity.Symbol, typeof(Opacity) },
            { ColorInterpolation.Symbol, typeof(ColorInterpolation) },
            { ColorInterpolationFilters.Symbol, typeof(ColorInterpolationFilters) },
            { ColorProfile.Symbol, typeof(ColorProfile) },
            { ColorRendering.Symbol, typeof(ColorRendering) },
            { Fill.Symbol, typeof(Fill) },
            { FillOpacity.Symbol, typeof(FillOpacity) },
            { FillRule.Symbol, typeof(FillRule) },
            { ImageRendering.Symbol, typeof(ImageRendering) },
            { Marker.Symbol, typeof(Marker) },
            { MarkerEnd.Symbol, typeof(MarkerEnd) },
            { MarkerMid.Symbol, typeof(MarkerMid) },
            { MarkerStart.Symbol, typeof(MarkerStart) },
            { ShapeRendering.Symbol, typeof(ShapeRendering) },
            { Stroke.Symbol, typeof(Stroke) },
            { StrokeDashArray.Symbol, typeof(StrokeDashArray) },
            { StrokeDashOffset.Symbol, typeof(StrokeDashOffset) },
            { StrokeLineCap.Symbol, typeof(StrokeLineCap) },
            { StrokeLineJoin.Symbol, typeof(StrokeLineJoin) },
            { StrokeMiterLimit.Symbol, typeof(StrokeMiterLimit) },
            { StrokeOpacity.Symbol, typeof(StrokeOpacity) },
            { StrokeWidth.Symbol, typeof(StrokeWidth) },
            { EnableBackground.Symbol, typeof(EnableBackground) },
            { Filter.Symbol, typeof(Filter) },
            { FloodColor.Symbol, typeof(FloodColor) },
            { FloodOpacity.Symbol, typeof(FloodOpacity) },
            { LightingColor.Symbol, typeof(LightingColor) },
            { Font.Symbol, typeof(Font) },
            { FontFamily.Symbol, typeof(FontFamily) },
            { FontSize.Symbol, typeof(FontSize) },
            { FontSizeAdjust.Symbol, typeof(FontSizeAdjust) },
            { FontStretch.Symbol, typeof(FontStretch) },
            { FontStyle.Symbol, typeof(FontStyle) },
            { FontVariant.Symbol, typeof(FontVariant) },
            { FontWeight.Symbol, typeof(FontWeight) },
            { StopColor.Symbol, typeof(StopColor) },
            { StopOpacity.Symbol, typeof(StopOpacity) },
            { Direction.Symbol, typeof(Direction) },
            { LetterSpacing.Symbol, typeof(LetterSpacing) },
            { TextDecoration.Symbol, typeof(TextDecoration) },
            { UnicodeBidi.Symbol, typeof(UnicodeBidi) },
            { WordSpacing.Symbol, typeof(WordSpacing) },
            { TextRendering.Symbol, typeof(TextRendering) },
            { AlignmnentBaseline.Symbol, typeof(AlignmnentBaseline) },
            { BaselineShift.Symbol, typeof(BaselineShift) },
            { DominantBaseline.Symbol, typeof(DominantBaseline) },
            { GlyphOrientationHorizontal.Symbol, typeof(GlyphOrientationHorizontal) },
            { GlyphOrientationVertical.Symbol, typeof(GlyphOrientationVertical) },
            { Kerning.Symbol, typeof(Kerning) },
            { TextAnchor.Symbol, typeof(TextAnchor) },
            { WritingMode.Symbol, typeof(WritingMode) },
        };

        public string Name { get; private set; }
        public T Value { get; protected set; }

        protected Base(string name) { }

        public Base(string name, T value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString().ToCamelCase();
        }

        public static TStyleAttribute EnumWrapperInstanceAttributes<TStyleAttribute, TEnum>(string s) where TStyleAttribute : Base<TEnum> where TEnum : struct, Enum
        {
            if (Enum.TryParse<TEnum>(s.ToPascalCase(), out TEnum value))
                return (TStyleAttribute)Activator.CreateInstance(typeof(TStyleAttribute), new object[] { value });
            return null;
        }

    }
}
