# Rtf2Html
Convert Rtf via Xaml to Html including images. The code is adapted from [Matthew Manelas Codesample](http://matthewmanela.com/blog/converting-between-rtf-to-html-and-html-to-rtf/) with a few tweaks:

1. Usage of [`RichTextBox`](http://msdn.microsoft.com/en-us/library/system.windows.controls.richtextbox.aspx) is avoided so we do not need to work around [STA](https://msdn.microsoft.com/en-us/library/windows/desktop/ms680112%28v=vs.85%29.aspx) issues. Instead we use [`TextEditorCopyPaste.ConvertRtfToXaml`](http://referencesource.microsoft.com/#PresentationFramework/Framework/System/windows/Documents/TextEditorCopyPaste.cs,560) via reflection. This is basically the same as using `TextRangeBase.Save(..., DataFormats.XamlPackage)` with `RichTextBox` but feels a bit cleaner to me.
2. Added support for processing the zipped `XamlPackage` stream including images. Requires [`ZipArchive`](https://msdn.microsoft.com/de-de/library/system.io.compression.ziparchive(v=vs.110).aspx) from `System.IO.Compression` in .NET 4.5    

Simple conversion between `Rtf` and `Xaml` is also included by just exposing Microsofts internal implementations.