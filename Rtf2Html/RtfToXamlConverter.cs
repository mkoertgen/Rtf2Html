using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Rtf2Html
{
    static class RtfToXamlConverter
    {
        public static bool RtfContainsImage(string rtfText)
        {
            // cf.: http://www.biblioscape.com/rtf15_spec.htm#Heading49
            return !String.IsNullOrWhiteSpace(rtfText) && rtfText.Contains(@"\pict");
        }

        /// <summary>
        /// Converts the specified RTF string into a Xaml package.
        /// </summary>
        /// <param name="rtfContent">The RTF content to convert to XAML</param>
        /// <returns>A zipped stream containing a full xaml package; or null</returns>
        public static MemoryStream RtfToXamlPackage(string rtfContent)
        {
            if (String.IsNullOrWhiteSpace(rtfContent)) return null;
            return (MemoryStream)TextEditorCopyPaste_ConvertRtfToXaml
                .Invoke(null, new object[] { rtfContent });
        }

        /// <summary>
        /// Converts the specified Xaml content string into an RTF string.
        /// </summary>
        public static string XamlToRtf(string xamlContent, Stream wpfContainerMemory = null)
        {
            if (String.IsNullOrWhiteSpace(xamlContent)) return String.Empty;
            return (string)TextEditorCopyPaste_ConvertXamlToRtf
                .Invoke(null, new object[] { xamlContent, wpfContainerMemory });
        }

        /// <summary>
        /// Converts the specified RTF string into a Xaml Flow document.
        /// </summary>
        /// <param name="rtfContent">The RTF content to convert to XAML</param>
        /// <returns>A Xaml string</returns>
        /// <remarks>Images and other content in the RTF are lost in the resulting Xaml content.</remarks>
        public static string RtfToXaml(string rtfContent)
        {
            if (String.IsNullOrWhiteSpace(rtfContent)) return String.Empty;
            return (string)XamlRtfConverterType_ConvertRtfToXaml
                .Invoke(XamlRtfConverter, new object[] { rtfContent });
        }

        private static readonly Assembly PresentationFrameworkAssembly = Assembly.GetAssembly(typeof(FrameworkElement));
        private static readonly Type TextEditorCopyPastType = PresentationFrameworkAssembly
            .GetType("System.Windows.Documents.TextEditorCopyPaste");

        private static readonly Type XamlRtfConverterType = PresentationFrameworkAssembly
            .GetType("System.Windows.Documents.XamlRtfConverter");

        // ReSharper disable InconsistentNaming
        private static readonly MethodInfo TextEditorCopyPaste_ConvertRtfToXaml = TextEditorCopyPastType
            .GetMethod("ConvertRtfToXaml", BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly MethodInfo TextEditorCopyPaste_ConvertXamlToRtf = TextEditorCopyPastType
            .GetMethod("ConvertXamlToRtf", BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly MethodInfo XamlRtfConverterType_ConvertRtfToXaml = XamlRtfConverterType
            .GetMethod("ConvertRtfToXaml", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly object XamlRtfConverter = Activator.CreateInstance(XamlRtfConverterType, 
            BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
        // ReSharper restore InconsistentNaming
    }
}
