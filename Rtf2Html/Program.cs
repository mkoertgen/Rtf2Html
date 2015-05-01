using System.Collections.Generic;
using System.IO;

namespace Rtf2Html
{
    static class Program
    {
        public static void Main(string[] args)
        {
            var rtfInput = GetArg(args, 0, "Document.rtf");

            var rtf = File.ReadAllText(rtfInput);

            //var xaml = RtfToXamlConverter.RtfToXaml(rtf);
            //File.WriteAllText("xaml.xaml", xaml);

            //var rtf2 = RtfToXamlConverter.XamlToRtf(xaml);
            //File.WriteAllText("rtf2.rtf", rtf2);

            //var plainText = RtfToPlaintTextConverter.RtfToPlainText(rtf);
            //File.WriteAllText("text.txt", plainText);

            var htmlOutput = GetArg(args, 1, Path.ChangeExtension(rtfInput, ".html"));
            var contentUriPrefix = Path.GetFileNameWithoutExtension(htmlOutput);
            var htmlResult = RtfToHtmlConverter.RtfToHtml(rtf, contentUriPrefix);
            htmlResult.WriteToFile(htmlOutput);
        }

        private static string GetArg(IList<string> args, int index, string defaultValue)
        {
            if (args == null || args.Count <= index) return defaultValue;
            return args[index];
        }
    }
}