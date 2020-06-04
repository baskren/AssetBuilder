using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using AndroidVector;
using P42.Utils;

namespace AssetBuilder
{
    public class Svg2 
    {

        public static (Vector vector, List<string> warnings) GenerateAndroidVector(string svgPath)
        {
            if (string.IsNullOrWhiteSpace(svgPath) || !File.Exists(svgPath))
                return  (null, new List<string> { "svgPath is null, empty, whitespace, or does not exist as a file" });

            if (File.ReadAllText(svgPath) is string text)
            {
                Dictionary<string, string> entities = new Dictionary<string, string>();
                int index = 0;
                while ((index = text.IndexOf("<!ENTITY ", index))>0)
                {
                    index += "<!ENTITY ".Length;
                    var name = text.SubstringWithTerminator(index, '\"');
                    index += name.Length + 1;
                    name = name.Trim();

                    var content = text.SubstringWithTerminator(index, '\"');

                    index += content.Length;
                    content.Trim('\"');
                    content = content.Replace('\'', '\"');

                    entities.Add(name, content);
                }

                if (entities.Count > 0)
                {
                    foreach (var kvp in entities)
                        text = text.Replace("&" + kvp.Key + ";", kvp.Value);

                    svgPath = System.IO.Path.GetTempFileName();
                    File.WriteAllText(svgPath, text);
                }
            }

            var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, IgnoreProcessingInstructions=true,  };
            XmlReader reader = XmlReader.Create(svgPath, settings);
            XDocument xdoc = XDocument.Load(reader);

            var warnings = new List<string>();
            var vector = Svg2AndroidVector.Converter.ConvertSvg(xdoc, warnings);

            var width = vector.Width.As(Unit.Dp);
            var height = vector.Height.As(Unit.Dp);

            if (width * height == 0)
                return (null, new List<string> { "ERROR: SVG has zero width or height." });

            return (vector, warnings);
        }


    }
}
