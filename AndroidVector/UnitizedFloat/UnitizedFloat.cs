using System;
using System.ComponentModel;
using System.Globalization;

namespace AndroidVector
{
    [TypeConverter(typeof(UnitizedFloatConverter))]
    public class UnitizedFloat
    {

        public float Value { get; set; }

        public Unit Unit { get; set; }

        public UnitizedFloat(float value, Unit unit)
        {
            Value = value;
            Unit = unit;
        }

        public override string ToString()
        {
            return Value + Unit.ToString().ToCamelCase();
        }

        public static UnitizedFloat FromString(string s)
        {
            //var result = new UnitizedFloat();
            var unitResult = Unit.Dp;
            s = s.Trim();
            foreach (var unit in (Unit[])Enum.GetValues(typeof(Unit)))
            {
                var name = unit.ToString().ToLower();
                if (s.EndsWith(name))
                {
                    unitResult = unit;
                    s = s.Substring(0, s.IndexOf(name));
                    break;
                }
            }
            if (float.TryParse(s, out float value))
                return new UnitizedFloat(value, unitResult);
            return null;
        }
    }

    [TypeConverter(typeof(UnitizedFloatConverter))]
    internal class UnitizedFloatConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string s)
            {
                return UnitizedFloat.FromString(s);
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is UnitizedFloat unitizedFloat)
            {
                return unitizedFloat.ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
