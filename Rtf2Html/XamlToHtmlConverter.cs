// adapted from https://github.com/mmanela/MarkupConverter

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;

namespace Rtf2Html
{
    class XamlToHtmlConverter
    {
        private ZipArchive _zip;
        private XmlTextReader _xamlReader;
        private HtmlResult _htmlResult;
        private string _contentUriPrefix = String.Empty;

        public bool AsFullDocument { get; set; }

        public string ContentUriPrefix
        {
            get { return _contentUriPrefix; }
            set
            {
                _contentUriPrefix = value ?? String.Empty;
                if (!String.IsNullOrWhiteSpace(_contentUriPrefix) && !_contentUriPrefix.EndsWith("/"))
                    _contentUriPrefix += "/";
            }
        }

        public XamlToHtmlConverter() { AsFullDocument = true; }

        public HtmlResult ConvertXamlToHtml(MemoryStream xamlPackageStream)
        {
            using (_zip = new ZipArchive(xamlPackageStream))
            {
                var entry = _zip.GetEntry(@"Xaml/Document.xaml");
                using (var stream = entry.Open())
                using (var reader = new StreamReader(stream))
                {
                    var xaml = reader.ReadToEnd();
                    _htmlResult = new HtmlResult();
                    ConvertXamlToHtml(xaml);
                    return _htmlResult;
                }
            }
        }

        private void ConvertXamlToHtml(string xamlString)
        {
            var safeXaml = WrapIntoFlowDocument(xamlString);

            _xamlReader = new XmlTextReader(new StringReader(safeXaml));
            var htmlStringBuilder = new StringBuilder(100);
            var htmlWriter = new XmlTextWriter(new StringWriter(htmlStringBuilder));

            if (!WriteFlowDocument(htmlWriter))
                return;
            _htmlResult.Html = htmlStringBuilder.ToString();
        }

        private static string WrapIntoFlowDocument(string xamlString)
        {
            if (String.IsNullOrWhiteSpace(xamlString)) return String.Empty;
            if (xamlString.StartsWith("<FlowDocument")) return xamlString;
            return
                String.Concat(
                    "<FlowDocument xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">",
                    xamlString,
                    "</FlowDocument>");
        }

        private bool WriteFlowDocument(XmlTextWriter htmlWriter)
        {
            // Xaml content is empty - nothing to convert
            if (!ReadNextToken())
                return false;

            // Root FlowDocument element is missing
            if (_xamlReader.NodeType != XmlNodeType.Element || _xamlReader.Name != "FlowDocument")
                return false;

            // Create a buffer StringBuilder for collecting css properties for inline STYLE attributes
            // on every element level (it will be re-initialized on every level).
            var inlineStyle = new StringBuilder();


            if (AsFullDocument)
            {
                htmlWriter.WriteStartElement("html");
                
                WriteHead(htmlWriter);

                htmlWriter.WriteStartElement("body");
            }

            WriteFormattingProperties(htmlWriter, inlineStyle);
            WriteElementContent(htmlWriter, inlineStyle);

            if (AsFullDocument)
            {
                htmlWriter.WriteEndElement();
                htmlWriter.WriteEndElement();
            }

            return true;
        }

        private static void WriteHead(XmlTextWriter htmlWriter)
        {
            htmlWriter.WriteStartElement("head");
            
            htmlWriter.WriteStartElement("meta");
            htmlWriter.WriteAttributeString("http-equiv", "Content-Type");
            htmlWriter.WriteAttributeString("content", "text/html; charset=UTF-8");
            htmlWriter.WriteEndElement();
            
            htmlWriter.WriteEndElement();
        }

        private void WriteFormattingProperties(XmlTextWriter htmlWriter, StringBuilder inlineStyle)
        {
            Debug.Assert(_xamlReader.NodeType == XmlNodeType.Element);

            // Clear string builder for the inline style
            inlineStyle.Remove(0, inlineStyle.Length);

            if (!_xamlReader.HasAttributes)
                return;

            var borderSet = false;

            while (_xamlReader.MoveToNextAttribute())
            {
                string css = null;

                switch (_xamlReader.Name)
                {
                    // Character fomatting properties
                    // ------------------------------
                    case "Background":
                        css = "background-color:" + ParseXamlColor(_xamlReader.Value) + ";";
                        break;
                    case "FontFamily":
                        css = "font-family:" + _xamlReader.Value + ";";
                        break;
                    case "FontStyle":
                        css = "font-style:" + _xamlReader.Value.ToLower() + ";";
                        break;
                    case "FontWeight":
                        css = "font-weight:" + _xamlReader.Value.ToLower() + ";";
                        break;
                    case "FontStretch":
                        break;
                    case "FontSize":
                        css = "font-size:" + _xamlReader.Value + ";";
                        break;
                    case "Foreground":
                        css = "color:" + ParseXamlColor(_xamlReader.Value) + ";";
                        break;
                    case "TextDecorations":
                        css = _xamlReader.Value.ToLower() == "strikethrough" ? "text-decoration:line-through;" : "text-decoration:underline;";
                        break;
                    case "TextEffects":
                        break;
                    case "Emphasis":
                        break;
                    case "StandardLigatures":
                        break;
                    case "Variants":
                        break;
                    case "Capitals":
                        break;
                    case "Fraction":
                        break;

                    // Paragraph formatting properties
                    // -------------------------------
                    case "Padding":
                        css = "padding:" + ParseXamlThickness(_xamlReader.Value) + ";";
                        break;
                    case "Margin":
                        css = "margin:" + ParseXamlThickness(_xamlReader.Value) + ";";
                        break;
                    case "BorderThickness":
                        css = "border-width:" + ParseXamlThickness(_xamlReader.Value) + ";";
                        borderSet = true;
                        break;
                    case "BorderBrush":
                        css = "border-color:" + ParseXamlColor(_xamlReader.Value) + ";";
                        borderSet = true;
                        break;
                    case "LineHeight":
                        break;
                    case "TextIndent":
                        css = "text-indent:" + _xamlReader.Value + ";";
                        break;
                    case "TextAlignment":
                        css = "text-align:" + _xamlReader.Value + ";";
                        break;
                    case "IsKeptTogether":
                        break;
                    case "IsKeptWithNext":
                        break;
                    case "ColumnBreakBefore":
                        break;
                    case "PageBreakBefore":
                        break;
                    case "FlowDirection":
                        break;

                    // Table/Image attributes
                    // ----------------
                    case "Width":
                        css = "width:" + _xamlReader.Value + ";";
                        break;
                    case "Height":
                        css = "height:" + _xamlReader.Value + ";";
                        break;
                    case "ColumnSpan":
                        htmlWriter.WriteAttributeString("colspan", _xamlReader.Value);
                        break;
                    case "RowSpan":
                        htmlWriter.WriteAttributeString("rowspan", _xamlReader.Value);
                        break;

                    // Hyperlink Attributes
                    case "NavigateUri":
                        htmlWriter.WriteAttributeString("href", _xamlReader.Value);
                        break;

                    case "TargetName":
                        htmlWriter.WriteAttributeString("target", _xamlReader.Value);
                        break;
                }

                if (css != null)
                    inlineStyle.Append(css);
            }

            if (borderSet)
                inlineStyle.Append("border-style:solid;mso-element:para-border-div;");

            // Return the xamlReader back to element level
            _xamlReader.MoveToElement();
            Debug.Assert(_xamlReader.NodeType == XmlNodeType.Element);
        }

        private static string ParseXamlColor(string color)
        {
            // Remove transparency value
            if (color.StartsWith("#"))
                color = "#" + color.Substring(3);
            return color;
        }

        private static string ParseXamlThickness(string thickness)
        {
            var values = thickness.Split(',');
            for (int i = 0; i < values.Length; i++)
            {
                double value;
                if (double.TryParse(values[i], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value))
                    values[i] = Math.Ceiling(value).ToString(CultureInfo.InvariantCulture);
                else
                    values[i] = "1";
            }

            string cssThickness;
            switch (values.Length)
            {
                case 1:
                    cssThickness = thickness;
                    break;
                case 2:
                    cssThickness = values[1] + " " + values[0];
                    break;
                case 4:
                    cssThickness = values[1] + " " + values[2] + " " + values[3] + " " + values[0];
                    break;
                default:
                    cssThickness = values[0];
                    break;
            }

            return cssThickness;
        }

        private void WriteElementContent(XmlTextWriter htmlWriter, StringBuilder inlineStyle)
        {
            Debug.Assert(_xamlReader.NodeType == XmlNodeType.Element);

            var elementContentStarted = false;

            if (_xamlReader.IsEmptyElement)
            {
                if (htmlWriter != null && inlineStyle.Length > 0)
                {
                    // Output STYLE attribute and clear inlineStyle buffer.
                    htmlWriter.WriteAttributeString("style", inlineStyle.ToString());
                    inlineStyle.Remove(0, inlineStyle.Length);
                }
            }
            else
            {
                while (ReadNextToken() && _xamlReader.NodeType != XmlNodeType.EndElement)
                {
                    switch (_xamlReader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (_xamlReader.Name.Contains("."))
                                AddComplexProperty(htmlWriter, inlineStyle);
                            else
                            {
                                if (htmlWriter != null && !elementContentStarted && inlineStyle.Length > 0)
                                {
                                    // Output STYLE attribute and clear inlineStyle buffer.
                                    htmlWriter.WriteAttributeString("style", inlineStyle.ToString());
                                    inlineStyle.Remove(0, inlineStyle.Length);
                                }
                                elementContentStarted = true;
                                WriteElement(htmlWriter, inlineStyle);
                            }
                            Debug.Assert(_xamlReader.NodeType == XmlNodeType.EndElement || _xamlReader.NodeType == XmlNodeType.Element && _xamlReader.IsEmptyElement);
                            break;
                        case XmlNodeType.Comment:
                            if (htmlWriter != null)
                            {
                                if (!elementContentStarted && inlineStyle.Length > 0)
                                {
                                    htmlWriter.WriteAttributeString("style", inlineStyle.ToString());
                                    inlineStyle.Remove(0, inlineStyle.Length);
                                }
                                htmlWriter.WriteComment(_xamlReader.Value);
                            }
                            elementContentStarted = true;
                            break;
                        case XmlNodeType.CDATA:
                        case XmlNodeType.Text:
                        case XmlNodeType.SignificantWhitespace:
                            if (htmlWriter != null)
                            {
                                if (!elementContentStarted && inlineStyle.Length > 0)
                                {
                                    htmlWriter.WriteAttributeString("style", inlineStyle.ToString());
                                    inlineStyle.Remove(0, inlineStyle.Length);
                                }
                                htmlWriter.WriteString(_xamlReader.Value);
                            }
                            elementContentStarted = true;
                            break;
                    }
                }

                Debug.Assert(_xamlReader.NodeType == XmlNodeType.EndElement);
            }
        }

        private void AddComplexProperty(XmlTextWriter htmlWriter, StringBuilder inlineStyle)
        {
            Debug.Assert(_xamlReader.NodeType == XmlNodeType.Element);

            if (htmlWriter != null && _xamlReader.Name == "Image.Source")
            {
                if (ReadNextToken() && _xamlReader.Name == "BitmapImage")
                {
                    var imageUri = _xamlReader.GetAttribute("UriSource");
                    if (!String.IsNullOrWhiteSpace(imageUri))
                    {
                        // check new image
                        if (!_htmlResult.Content.ContainsKey(imageUri))
                        {
                            var entryPath = String.Concat("Xaml/", imageUri).Replace("/./", "/");
                            var imageEntry = _zip.GetEntry(entryPath);
                            using (var stream = imageEntry.Open())
                            {
                                var image = ToByteArray(stream);
                                var identicalContent = _htmlResult.Content.FirstOrDefault(kvp => image.SequenceEqual(kvp.Value));
                                var isIdentical = !default(KeyValuePair<string, byte[]>).Equals(identicalContent);
                                if (isIdentical)
                                    imageUri = identicalContent.Key;
                                else
                                {
                                    imageUri = String.Concat(ContentUriPrefix, imageUri).Replace("/./", "/");
                                    _htmlResult.Content[imageUri] = image;
                                }
                            }
                        }

                        // width & height
                        if (inlineStyle != null && inlineStyle.Length > 0)
                        {
                            htmlWriter.WriteAttributeString("style", inlineStyle.ToString());
                            inlineStyle.Remove(0, inlineStyle.Length);
                        }

                        htmlWriter.WriteAttributeString("src", imageUri);

                        while (_xamlReader.NodeType != XmlNodeType.EndElement)
                            ReadNextToken();
                        return;
                    }
                }
            }

            if (inlineStyle != null && _xamlReader.Name.EndsWith(".TextDecorations"))
                inlineStyle.Append("text-decoration:underline;");
            
            // Skip the element representing the complex property
            WriteElementContent(null, null);
        }

        private void WriteElement(XmlTextWriter htmlWriter, StringBuilder inlineStyle)
        {
            Debug.Assert(_xamlReader.NodeType == XmlNodeType.Element);

            if (htmlWriter == null)
            {
                // Skipping mode; recurse into the xaml element without any output
                WriteElementContent(null, null);
            }
            else
            {
                string htmlElementName;

                switch (_xamlReader.Name)
                {
                    case "Run":
                    case "Span":
                        htmlElementName = "span";
                        break;
                    case "InlineUIContainer":
                        htmlElementName = "span";
                        break;
                    case "Bold":
                        htmlElementName = "b";
                        break;
                    case "Italic":
                        htmlElementName = "i";
                        break;
                    case "Paragraph":
                        htmlElementName = "p";
                        break;
                    case "BlockUIContainer":
                        htmlElementName = "div";
                        break;
                    case "Section":
                        htmlElementName = "div";
                        break;
                    case "Table":
                        htmlElementName = "table";
                        break;
                    case "TableColumn":
                        htmlElementName = "col";
                        break;
                    case "TableRowGroup":
                        htmlElementName = "tbody";
                        break;
                    case "TableRow":
                        htmlElementName = "tr";
                        break;
                    case "TableCell":
                        htmlElementName = "td";
                        break;
                    case "List":
                        var marker = _xamlReader.GetAttribute("MarkerStyle");
                        if (marker == null || marker == "None" || marker == "Disc" || marker == "Circle" || marker == "Square" || marker == "Box")
                            htmlElementName = "ul";
                        else
                            htmlElementName = "ol";
                        break;
                    case "ListItem":
                        htmlElementName = "li";
                        break;
                    case "Hyperlink":
                        htmlElementName = "a";
                        break;
                    case "Image":
                        htmlElementName = "img";
                        break;
                    default:
                        htmlElementName = null; // Ignore the element
                        break;
                }

                if (htmlElementName != null)
                {
                    htmlWriter.WriteStartElement(htmlElementName);
                    WriteFormattingProperties(htmlWriter, inlineStyle);
                    WriteElementContent(htmlWriter, inlineStyle);
                    htmlWriter.WriteEndElement();
                }
                else
                {
                    // Skip this unrecognized xaml element
                    WriteElementContent(null, null);
                }
            }
        }

        private bool ReadNextToken()
        {
            while (_xamlReader.Read())
            {
                Debug.Assert(_xamlReader.ReadState == ReadState.Interactive, "Reader is expected to be in Interactive state (" + _xamlReader.ReadState + ")");
                switch (_xamlReader.NodeType)
                {
                    case XmlNodeType.Element:
                    case XmlNodeType.EndElement:
                    case XmlNodeType.None:
                    case XmlNodeType.CDATA:
                    case XmlNodeType.Text:
                    case XmlNodeType.SignificantWhitespace:
                        return true;

                    case XmlNodeType.Whitespace:
                        if (_xamlReader.XmlSpace == XmlSpace.Preserve)
                            return true;
                        // ignore insignificant whitespace
                        break;

                    case XmlNodeType.EndEntity:
                    case XmlNodeType.EntityReference:
                        //  Implement entity reading
                        //xamlReader.ResolveEntity();
                        //xamlReader.Read();
                        //ReadChildNodes( parent, parentBaseUri, xamlReader, positionInfo);
                        break; // for now we ignore entities as insignificant stuff

                    case XmlNodeType.Comment:
                        return true;
                }
            }
            return false;
        }

        private static byte[] ToByteArray(Stream stream)
        {
            var memoryStream = stream as MemoryStream;
            if (memoryStream != null) return memoryStream.ToArray();
            using (memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}