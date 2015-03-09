using System.Collections.Generic;
using System.IO;

namespace Rtf2Html
{
    static class Program
    {
        public static void Main(string[] args)
        {
            var rtfInput = GetArg(args, 0, "Document.rtf");
            var htmlOutput = GetArg(args, 1, Path.ChangeExtension(rtfInput, ".html"));

            var rtf = File.ReadAllText(rtfInput);

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