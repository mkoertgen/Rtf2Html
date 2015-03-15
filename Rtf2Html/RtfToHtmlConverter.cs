namespace Rtf2Html
{
    static class RtfToHtmlConverter
    {
        public static HtmlResult RtfToHtml(string rtf, string contentUriPrefix = null, bool asFullDocument = true)
        {
            var xamlStream = RtfXamlConverter.RtfToXamlPackage(rtf);
            var htmlConverter = new XamlToHtmlConverter
            {
                AsFullDocument = asFullDocument,
                ContentUriPrefix = contentUriPrefix
            };

            return htmlConverter.ConvertXamlToHtml(xamlStream);
        }
    }
}