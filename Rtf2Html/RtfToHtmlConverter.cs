namespace Rtf2Html
{
    internal static class RtfToHtmlConverter
    {
        public static HtmlResult RtfToHtml(string rtf, string contentUriPrefix = null, bool asFullDocument = true)
        {
            var xamlStream = RtfToXamlConverter.RtfToXamlPackage(rtf);
            var htmlConverter = new XamlToHtmlConverter
            {
                AsFullDocument = asFullDocument,
                ContentUriPrefix = contentUriPrefix
            };

            return htmlConverter.ConvertXamlToHtml(xamlStream);
        }
    }
}