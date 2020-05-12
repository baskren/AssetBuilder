using System;
namespace AndroidVector
{
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
    }
}
