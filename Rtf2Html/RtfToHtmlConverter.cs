namespace Rtf2Html
{
    internal static class RtfToHtmlConverter
    {
        public static HtmlResult RtfToHtml(string rtf, bool asFullDocument = true)
        {
            var xamlStream = RtfToXamlConverter.RtfToXamlPackage(rtf);
            using (var htmlConverter = new XamlToHtmlConverter(xamlStream, asFullDocument))
                return htmlConverter.ConvertXamlToHtml();
        }
    }
}