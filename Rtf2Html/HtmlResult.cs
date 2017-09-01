using System.Collections.Generic;
using System.IO;

namespace Rtf2Html
{
    internal class HtmlResult
    {
        public string Html { get; set; }
        public Dictionary<string, byte[]> Content { get; }

        public HtmlResult()
        {
            Html = string.Empty;
            Content = new Dictionary<string, byte[]>();
        }

        public void WriteToFile(string fileName)
        {
            File.WriteAllText(fileName, Html);
            var fileInfo = new FileInfo(fileName);
            foreach (var content in Content)
            {
                // GetFullPath converts forward slashes, cf.:
                // http://stackoverflow.com/questions/3144492/how-do-i-get-nets-path-combine-to-convert-forward-slashes-to-backslashes
                var contentFileName = Path.GetFullPath(Path.Combine(fileInfo.DirectoryName, content.Key));
                var contentPath = Path.GetDirectoryName(contentFileName);
                if (!Directory.Exists(contentPath))
                    // ReSharper disable once AssignNullToNotNullAttribute
                    Directory.CreateDirectory(contentPath);
                File.WriteAllBytes(contentFileName, content.Value);
            }
        }
    }
}