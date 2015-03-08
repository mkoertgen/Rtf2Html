using System.Collections.Generic;
using System.IO;

namespace Rtf2Html
{
    static class Program
    {
        public static void Main(string[] args)
        {
            var rtf = File.ReadAllText( GetArg(args, 0, "rtf2.rtf") );
            var result = RtfToHtmlConverter.RtfToHtml(rtf);
            result.WriteToFile(GetArg(args, 1, "rtf2.html"));
        }

        private static string GetArg(IList<string> args, int index, string defaultValue)
        {
            if (args == null || args.Count <= index) return defaultValue;
            return args[index];
        }
    }
}