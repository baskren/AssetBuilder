using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using P42.Utils;

namespace AssetBuilder
{
    public class Svg2 
    {

        public static List<string> GenerateAndroidVector(string svgPath, string destPath)
        {
            if (string.IsNullOrWhiteSpace(svgPath) || !File.Exists(svgPath))
                return new List<string> { "svgPath is null, empty, whitespace, or does not exist as a file" };

            string xmlHeader = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n";

            var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, IgnoreProcessingInstructions=true,  };
            XmlReader reader = XmlReader.Create(svgPath, settings);
            XDocument xdoc = XDocument.Load(reader);

            //XDocument xdoc = XDocument.Load(svgPath);

            var warnings = new List<string>();
            var vector = Svg2AndroidVector.Converter.ConvertSvg(xdoc, warnings);
            var text = xmlHeader + vector;

            File.WriteAllText(destPath, text);
            return warnings;
        }


        const string InkscapePath = "/Applications/Inkscape.app/Contents/MacOS/inkscape";
        public static string GeneratePdf(string svgPath, string destPath)
        {
            if (string.IsNullOrWhiteSpace(svgPath) || !File.Exists(svgPath))
                return "svgPath is null, empty, whitespace, or does not exist as a file";

            if (!File.Exists(InkscapePath))
                return "Inkscape cannot be found.  You may need to install Inkscape";

            int timeout = 10000;
            using var process = new System.Diagnostics.Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = InkscapePath;
            process.StartInfo.Arguments = $"{svgPath} --export-area-drawing --without-gui --export-file={destPath}";
            process.StartInfo.CreateNoWindow = false;
            process.Start();
            process.WaitForExit(timeout);

            if (process.HasExited)
            {
                if (process.ExitCode == 0)
                    return null;
            }
            else
                process.Kill();
            return "GeneratePdf did not complete successfully.";
        }
    }
}
