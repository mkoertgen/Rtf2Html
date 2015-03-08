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
            return (MemoryStream)TextEditorCopyPaste_ConvertRtfToXaml.Invoke(null, new object[] { rtfContent });
        }

        private static readonly Type TextEditorCopyPastType = Assembly.GetAssembly(typeof(FrameworkElement))
            .GetType("System.Windows.Documents.TextEditorCopyPaste");
        // ReSharper disable once InconsistentNaming
        private static readonly MethodInfo TextEditorCopyPaste_ConvertRtfToXaml = TextEditorCopyPastType
            .GetMethod("ConvertRtfToXaml", BindingFlags.Static | BindingFlags.NonPublic);
    }
}
