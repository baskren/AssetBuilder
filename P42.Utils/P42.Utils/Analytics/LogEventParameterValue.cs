using System;
namespace P42.Utils
{
    /*
    public class LogEventParameterValue
    {
        internal Type _type;
        internal string _string;
        internal bool _bool;
        internal char _char;
        internal short _short;
        internal int _int;
        internal long _long;
        internal float _float;
        internal double _double;

        public LogEventParameterValue(string value)
        {
            _type = typeof(string);
            _string = value;
        }

        public LogEventParameterValue(bool value)
        {
            _type = typeof(bool);
            _bool = value;
        }

        public LogEventParameterValue(char value)
        {
            _type = typeof(char);
            _char = value;
        }

        public LogEventParameterValue(short value)
        {
            _type = typeof(short);
            _short = value;
        }

        public LogEventParameterValue(int value)
        {
            _type = typeof(int);
            _int = value;
        }

        public LogEventParameterValue(long value)
        {
            _type = typeof(long);
            _long = value;
        }

        public LogEventParameterValue(float value)
        {
            _type = typeof(float);
            _float = value;
        }

        public LogEventParameterValue(double value)
        {
            _type = typeof(double);
            _double = value;
        }

        public override string ToString()
        {
            if (_type == typeof(bool))
                return _bool.ToString();
            if (_type == typeof(char))
                return _char.ToString();
            if (_type == typeof(short))
                return _short.ToString();
            if (_type == typeof(int))
                return _int.ToString();
            if (_type == typeof(long))
                return _long.ToString();
            if (_type == typeof(float))
                return _float.ToString();
            return _double.ToString();
        }

        public static implicit operator LogEventParameterValue(bool value)
        {
            return new LogEventParameterValue(value);
        }

        public static implicit operator LogEventParameterValue(char value)
        {
            return new LogEventParameterValue(value);
        }

        public static implicit operator LogEventParameterValue(short value)
        {
            return new LogEventParameterValue(value);
        }
        public static implicit operator LogEventParameterValue(int value)
        {
            return new LogEventParameterValue(value);
        }

        public static implicit operator LogEventParameterValue(long value)
        {
            return new LogEventParameterValue(value);
        }
        public static implicit operator LogEventParameterValue(float value)
        {
            return new LogEventParameterValue(value);
        }

        public static implicit operator LogEventParameterValue(double value)
        {
            return new LogEventParameterValue(value);
        }

        public static implicit operator LogEventParameterValue(string value)
        {
            return new LogEventParameterValue(value);
        }

        public static implicit operator bool(LogEventParameterValue value)
        {
            if (value._type != typeof(bool)) throw new InvalidCastException();
            return value._bool;
        }

        public static implicit operator char(LogEventParameterValue value)
        {
            if (value._type != typeof(char)) throw new InvalidCastException();
            return value._char;
        }

        public static implicit operator short(LogEventParameterValue value)
        {
            if (value._type != typeof(short)) throw new InvalidCastException();
            return value._short;
        }
        public static implicit operator int(LogEventParameterValue value)
        {
            if (value._type != typeof(int)) throw new InvalidCastException();
            return value._int;
        }

        public static implicit operator long(LogEventParameterValue value)
        {
            if (value._type != typeof(long)) throw new InvalidCastException();
            return value._long;
        }
        public static implicit operator float(LogEventParameterValue value)
        {
            if (value._type != typeof(float)) throw new InvalidCastException();
            return value._float;
        }

        public static implicit operator double(LogEventParameterValue value)
        {
            if (value._type != typeof(double)) throw new InvalidCastException();
            return value._double;
        }

    }
    */
}
