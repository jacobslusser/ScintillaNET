using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ScintillaNET
{
    internal static class Helpers
    {
        #region Fields

        private const string HTML_HEADER =
            "Version:0.9\r\n" +
            "StartHTML:00000097\r\n" +
            "EndHTML:00000000\r\n" +
            "StartFragment:00000000\r\n" +
            "EndFragment:00000000\r\n";

        private const int INDEX_START_FRAGMENT = 65;
        private const int INDEX_END_FRAGMENT = 87;
        private const int INDEX_END_HTML = 41;

        private static readonly byte[] header = Encoding.ASCII.GetBytes(HTML_HEADER);

        #endregion Fields

        #region Methods

        public static unsafe byte[] BitmapToArgb(Bitmap image)
        {
            var rect = new Rectangle(0, 0, image.Width, image.Height);

            // Get the bitmap into a 32bpp ARGB format (if it isn't already)
            if (image.PixelFormat != PixelFormat.Format32bppArgb)
            {
                var clone = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
                using (var graphics = Graphics.FromImage(clone))
                    graphics.DrawImage(image, rect);

                image = clone;
            }

            // Convert ARGB to RGBA
            var bitmapData = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var bytes = new byte[bitmapData.Stride * bitmapData.Height];
            fixed (byte* bp = bytes)
            {
                try
                {
                    for (int y = 0; y < bitmapData.Height; y++)
                    {
                        var srcRow = (byte*)bitmapData.Scan0 + (y * bitmapData.Stride);
                        var destRow = bp + (y * bitmapData.Stride);
                        for (int x = 0; x < bitmapData.Width; x++)
                        {
                            // 4 bytes per pixel
                            destRow[x * 4] = srcRow[(x * 4) + 3]; // A
                            destRow[(x * 4) + 1] = srcRow[(x * 4) + 1]; // B
                            destRow[(x * 4) + 2] = srcRow[x * 4]; // G
                            destRow[(x * 4) + 3] = srcRow[(x * 4) + 2]; // R
                        }
                    }
                }
                finally
                {
                    image.UnlockBits(bitmapData);
                }
            }

            return bytes;
        }

        public static unsafe byte[] ByteToCharStyles(byte* styles, byte* text, int length, Encoding encoding)
        {
            // This is used by annotations and margins to get all the styles in one call.
            // It converts an array of styles where each element corresponds to a BYTE
            // to an array of styles where each element corresponds to a CHARACTER.

            var bytePos = 0; // Position within text BYTES and style BYTES (should be the same)
            var charPos = 0; // Position within style CHARACTERS
            var decoder = encoding.GetDecoder();
            var result = new byte[encoding.GetCharCount(text, length)];

            while (bytePos < length)
            {
                if (decoder.GetCharCount(text + bytePos, 1, false) > 0)
                    result[charPos++] = *(styles + bytePos); // New char

                bytePos++;
            }

            return result;
        }

        public static unsafe byte[] CharToByteStyles(byte[] styles, byte* text, int length, Encoding encoding)
        {
            // This is used by annotations and margins to style all the text in one call.
            // It converts an array of styles where each element corresponds to a CHARACTER
            // to an array of styles where each element corresponds to a BYTE.

            var bytePos = 0; // Position within text BYTES and style BYTES (should be the same)
            var charPos = 0; // Position within style CHARACTERS
            var decoder = encoding.GetDecoder();
            var result = new byte[length];

            while (bytePos < length && charPos < styles.Length)
            {
                result[bytePos] = styles[charPos];
                if (decoder.GetCharCount(text + bytePos, 1, false) > 0)
                    charPos++; // Move a char

                bytePos++;
            }

            return result;
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        public static int ClampMin(int value, int min)
        {
            if (value < min)
                return min;

            return value;
        }

        public static unsafe IntPtr ExportAsClipboardHtml(Scintilla scintilla, int byteStartPos, int byteEndPos, bool colorize, out int length)
        {
            // NppExport
            // http://blogs.msdn.com/b/jmstall/archive/2007/01/21/html-clipboard.aspx
            // http://blogs.msdn.com/b/jmstall/archive/2007/01/21/sample-code-html-clipboard.aspx
            // https://msdn.microsoft.com/en-us/library/windows/desktop/ms649015.aspx

            // I'm using native calls for most of this to make it as fast as possible...

            var byteLength = byteEndPos - byteStartPos;

            if (colorize)
                scintilla.DirectMessage(NativeMethods.SCI_COLOURISE, new IntPtr(byteStartPos), new IntPtr(byteEndPos));

            var buffer = new byte[(byteLength * 2) + 2];
            fixed (byte* bp = buffer)
            {
                NativeMethods.Sci_TextRange* tr = stackalloc NativeMethods.Sci_TextRange[1];
                tr->chrg.cpMin = byteStartPos;
                tr->chrg.cpMax = byteEndPos;
                tr->lpstrText = new IntPtr(bp);

                scintilla.DirectMessage(NativeMethods.SCI_GETSTYLEDTEXT, IntPtr.Zero, new IntPtr(tr));
            }

            // Build a list of (used) styles
            var styles = new StyleData[NativeMethods.STYLE_MAX + 1];

            styles[Style.Default].Used = true;
            styles[Style.Default].FontName = scintilla.Styles[Style.Default].Font;
            styles[Style.Default].SizeF = scintilla.Styles[Style.Default].SizeF;
            styles[Style.Default].Weight = scintilla.DirectMessage(NativeMethods.SCI_STYLEGETWEIGHT, new IntPtr(Style.Default), IntPtr.Zero).ToInt32();
            styles[Style.Default].Italic = scintilla.DirectMessage(NativeMethods.SCI_STYLEGETITALIC, new IntPtr(Style.Default), IntPtr.Zero).ToInt32();
            styles[Style.Default].Underline = scintilla.DirectMessage(NativeMethods.SCI_STYLEGETUNDERLINE, new IntPtr(Style.Default), IntPtr.Zero).ToInt32();
            styles[Style.Default].BackColor = scintilla.DirectMessage(NativeMethods.SCI_STYLEGETBACK, new IntPtr(Style.Default), IntPtr.Zero).ToInt32();
            styles[Style.Default].ForeColor = scintilla.DirectMessage(NativeMethods.SCI_STYLEGETFORE, new IntPtr(Style.Default), IntPtr.Zero).ToInt32();
            styles[Style.Default].Case = scintilla.DirectMessage(NativeMethods.SCI_STYLEGETCASE, new IntPtr(Style.Default), IntPtr.Zero).ToInt32();
            styles[Style.Default].Visible = scintilla.DirectMessage(NativeMethods.SCI_STYLEGETVISIBLE, new IntPtr(Style.Default), IntPtr.Zero).ToInt32();

            for (int i = 0; i < byteLength; i++)
            {
                var style = buffer[(i * 2) + 1];
                if (!styles[style].Used)
                {
                    styles[style].Used = true;
                    styles[style].FontName = scintilla.Styles[style].Font;
                    styles[style].SizeF = scintilla.Styles[style].SizeF;
                    styles[style].Weight = scintilla.DirectMessage(NativeMethods.SCI_STYLEGETWEIGHT, new IntPtr(style), IntPtr.Zero).ToInt32();
                    styles[style].Italic = scintilla.DirectMessage(NativeMethods.SCI_STYLEGETITALIC, new IntPtr(style), IntPtr.Zero).ToInt32();
                    styles[style].Underline = scintilla.DirectMessage(NativeMethods.SCI_STYLEGETUNDERLINE, new IntPtr(style), IntPtr.Zero).ToInt32();
                    styles[style].BackColor = scintilla.DirectMessage(NativeMethods.SCI_STYLEGETBACK, new IntPtr(style), IntPtr.Zero).ToInt32();
                    styles[style].ForeColor = scintilla.DirectMessage(NativeMethods.SCI_STYLEGETFORE, new IntPtr(style), IntPtr.Zero).ToInt32();
                    styles[style].Case = scintilla.DirectMessage(NativeMethods.SCI_STYLEGETCASE, new IntPtr(style), IntPtr.Zero).ToInt32();
                    styles[style].Visible = scintilla.DirectMessage(NativeMethods.SCI_STYLEGETVISIBLE, new IntPtr(style), IntPtr.Zero).ToInt32();
                }
            }

            long pos = 0;
            byte[] bytes;
            using (var ms = new NativeMemoryStream(byteLength))
            using (var tw = new StreamWriter(ms, new UTF8Encoding(false)))
            {
                ms.Write(header, 0, header.Length);

                tw.WriteLine("<html>");
                tw.WriteLine("<head>");
                tw.WriteLine(@"<meta charset=""utf-8"" />");
                tw.WriteLine("<title>ScintillaNET Export</title>");
                tw.WriteLine("</head>");
                tw.WriteLine("<body>");
                tw.Flush();

                // Patch header
                pos = ms.Position;
                ms.Seek(INDEX_START_FRAGMENT, SeekOrigin.Begin);
                ms.Write((bytes = Encoding.ASCII.GetBytes(ms.Length.ToString("D8"))), 0, bytes.Length);
                ms.Seek(pos, SeekOrigin.Begin);
                tw.WriteLine("<!--StartFragment -->");

                // Write the styles used.
                // We're doing the style tag in the body to include it in the "fragment".
                tw.WriteLine(@"<style type=""text/css"" scoped="""">");
                tw.Write("div#ScintillaNET {");
                tw.Write(" float: left;");
                tw.Write(" white-space: pre;");
                tw.Write(" line-height: {0}px;", scintilla.DirectMessage(NativeMethods.SCI_TEXTHEIGHT, new IntPtr(0)).ToInt32());
                tw.Write(" background: {0};", ColorTranslator.ToHtml(ColorTranslator.FromWin32(styles[Style.Default].BackColor)));
                tw.WriteLine(" }");

                for (int i = 0; i < styles.Length; i++)
                {
                    if (!styles[i].Used)
                        continue;

                    tw.Write("span.s{0} {{", i);
                    tw.Write(@" font-family: ""{0}"";", styles[i].FontName);
                    tw.Write(" font-size: {0}pt;", styles[i].SizeF);
                    tw.Write(" font-weight: {0};", styles[i].Weight);
                    if (styles[i].Italic != 0)
                        tw.Write(" font-style: italic;");
                    if (styles[i].Underline != 0)
                        tw.Write(" text-decoration: underline;");
                    tw.Write(" background-color: {0};", ColorTranslator.ToHtml(ColorTranslator.FromWin32(styles[i].BackColor)));
                    tw.Write(" color: {0};", ColorTranslator.ToHtml(ColorTranslator.FromWin32(styles[i].ForeColor)));
                    switch ((StyleCase)styles[i].Case)
                    {
                        case StyleCase.Upper:
                            tw.Write(" text-transform: uppercase;");
                            break;
                        case StyleCase.Lower:
                            tw.Write(" text-transform: lowercase;");
                            break;
                    }

                    if (styles[i].Visible == 0)
                        tw.Write(" visibility: hidden;");
                    tw.WriteLine(" }");
                }

                tw.WriteLine("</style>");
                tw.Write(@"<div id=""ScintillaNET"">");
                tw.Flush();

                var tabSize = scintilla.DirectMessage(NativeMethods.SCI_GETTABWIDTH).ToInt32();
                var tab = new string(' ', tabSize);

                byte ch;
                var openSpan = false;
                var lastStyle = -1;
                tw.AutoFlush = true;
                for (int i = 0; i < byteLength; i++)
                {
                    // Print new span if style changes
                    if (buffer[(i * 2) + 1] != lastStyle)
                    {
                        if (openSpan)
                            tw.Write("</span>");

                        lastStyle = buffer[(i * 2) + 1];
                        tw.Write(@"<span class=""s{0}"">", lastStyle);
                        openSpan = true;
                    }

                    ch = buffer[i * 2];
                    switch (ch)
                    {
                        case (byte)'\r':
                        case (byte)'\n':
                            ms.WriteByte(ch);
                            break;

                        case (byte)'<':
                            tw.Write("&lt;");
                            break;

                        case (byte)'>':
                            tw.Write("&gt;");
                            break;

                        case (byte)'&':
                            tw.Write("&amp;");
                            break;

                        case (byte)'\t':
                            tw.Write(tab);
                            break;

                        default:
                            if (ch >= 0x20) // Skip control chars
                                ms.WriteByte(ch);
                            break;
                    }
                }

                tw.AutoFlush = false;
                if (openSpan)
                    tw.Write("</span>");

                tw.WriteLine("</div>");
                tw.Flush();

                // Patch header
                pos = ms.Position;
                ms.Seek(INDEX_END_FRAGMENT, SeekOrigin.Begin);
                ms.Write((bytes = Encoding.ASCII.GetBytes(ms.Length.ToString("D8"))), 0, bytes.Length);
                ms.Seek(pos, SeekOrigin.Begin);
                tw.WriteLine("<!--EndFragment-->");

                tw.WriteLine("</body>");
                tw.WriteLine("</html>");
                tw.Flush();

                // Patch header
                pos = ms.Position;
                ms.Seek(INDEX_END_HTML, SeekOrigin.Begin);
                ms.Write((bytes = Encoding.ASCII.GetBytes(ms.Length.ToString("D8"))), 0, bytes.Length);
                ms.Seek(pos, SeekOrigin.Begin);

                // Terminator
                ms.WriteByte(0);
                length = (int)ms.Length;
                return ms.Pointer;
            }
        }

        public static unsafe byte[] GetBytes(string text, Encoding encoding, bool zeroTerminated)
        {
            if (string.IsNullOrEmpty(text))
                return new byte[0];

            int count = encoding.GetByteCount(text);
            byte[] buffer = new byte[count + (zeroTerminated ? 1 : 0)];

            fixed (byte* bp = buffer)
            fixed (char* ch = text)
            {
                encoding.GetBytes(ch, text.Length, bp, count);
            }

            if (zeroTerminated)
                buffer[buffer.Length - 1] = 0;

            return buffer;
        }

        public static unsafe byte[] GetBytes(char[] text, int length, Encoding encoding, bool zeroTerminated)
        {
            fixed (char* cp = text)
            {
                var count = encoding.GetByteCount(cp, length);
                var buffer = new byte[count + (zeroTerminated ? 1 : 0)];
                fixed (byte* bp = buffer)
                    encoding.GetBytes(cp, length, bp, buffer.Length);

                if (zeroTerminated)
                    buffer[buffer.Length - 1] = 0;

                return buffer;
            }
        }

        public static unsafe string GetString(IntPtr bytes, int length, Encoding encoding)
        {
            var ptr = (sbyte*)bytes;
            var str = new string(ptr, 0, length, encoding);

            return str;
        }

        #endregion Methods

        #region Types

        private struct StyleData
        {
            public bool Used;
            public string FontName;
            public float SizeF;
            public int Weight;
            public int Italic;
            public int Underline;
            public int BackColor;
            public int ForeColor;
            public int Case;
            // public int FillLine;
            public int Visible;
        }

        #endregion Types
    }
}
