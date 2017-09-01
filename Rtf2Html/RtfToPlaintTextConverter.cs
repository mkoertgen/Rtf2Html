using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;

namespace Rtf2Html
{
    internal static class RtfToPlaintTextConverter
    {
        public static string RtfToPlainText(string rtf, Encoding encoding = null)
        {
            var safeEncoding = encoding ?? Encoding.UTF8;
            // cf.: http://stackoverflow.com/questions/26123907/convert-rtf-string-to-xaml-string
            var doc = new FlowDocument();
            var range = new TextRange(doc.ContentStart, doc.ContentEnd);
            using (var stream = new MemoryStream(safeEncoding.GetBytes(rtf)))
            using (var outStream = new MemoryStream())
            {
                range.Load(stream, DataFormats.Rtf);
                range.Save(outStream, DataFormats.Text);
                outStream.Seek(0, SeekOrigin.Begin);
                return safeEncoding.GetString(outStream.ToArray()).Trim();
            }
        }
    }
}