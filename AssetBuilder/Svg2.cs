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

        public static (Vector vector, List<string> warnings) GenerateAndroidVector(System.IO.Stream stream)
        {
            using (var reader = new System.IO.StreamReader(stream))
            {
                var text = reader.ReadToEnd();
                return GenerateAndroidVector(text);
            }
        }

        public static (Vector vector, List<string> warnings) GenerateAndroidVector(P42.Storage.IStorageFile svgFile)
        {
            if (svgFile.ReadAllText() is string svgText)
                return GenerateAndroidVector(svgText);
            return (null, new List<string> { "Cannot read text from file ["+svgFile.Path+"]" });
        }

        public static (Vector vector, List<string> warnings) GenerateAndroidVector(string svgText)
        {
            if (!string.IsNullOrWhiteSpace(svgText))
            {
                Dictionary<string, string> entities = new Dictionary<string, string>();
                int index = 0;
                while ((index = svgText.IndexOf("<!ENTITY ", index))>0)
                {
                    index += "<!ENTITY ".Length;
                    var name = svgText.SubstringWithTerminator(index, '\"');
                    index += name.Length + 1;
                    name = name.Trim();

                    var content = svgText.SubstringWithTerminator(index, '\"');

                    index += content.Length;
                    content.Trim('\"');
                    content = content.Replace('\'', '\"');

                    entities.Add(name, content);
                }

                if (entities.Count > 0)
                {
                    foreach (var kvp in entities)
                        svgText = svgText.Replace("&" + kvp.Key + ";", kvp.Value);
                }

                var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, IgnoreProcessingInstructions = true, };
                using (var textReader = new StringReader(svgText))
                {
                    XmlReader reader = XmlReader.Create(textReader, settings);
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
            else
                return (null, new List<string> { "svgPath is null, empty, whitespace, or does not exist as a file" });
        }


    }
}
