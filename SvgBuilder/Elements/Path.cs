using System;
using System.Collections.Generic;

namespace SvgBuilder
{
    public class Path : BaseElement
    {
        public List<BasePathCommand> Data
        {
            get
            {
                if (GetPropertyAttribute<string>() is string str)
                    return str.ToPathCommands();
                return null;
            }
            set
            {
                var str = "";
                foreach (var cmd in value)
                    str += cmd.ToString() + " ";
                SetPropertyAttribute(str);
            }
        }

        public float PathLength
        {
            get => GetPropertyAttribute<float>();
            set => SetPropertyAttribute(value);
        }

        public Path() : base("path")
        {
        }


    }
}
