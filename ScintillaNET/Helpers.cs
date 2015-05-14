using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

        private static bool registeredFormats;
        private static uint CF_HTML;
        private static uint CF_RTF;
        private static uint CF_LINESELECT;
        private static uint CF_VSLINETAG;

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

        public static void Copy(Scintilla scintilla, CopyFormat format, bool allowLine)
        {
            var defaultProcessing = false;

            // Text (default processing)
            if ((format & CopyFormat.Text) > 0)
            {
                defaultProcessing = true;

                if (allowLine)
                    scintilla.DirectMessage(NativeMethods.SCI_COPYALLOWLINE);
                else
                    scintilla.DirectMessage(NativeMethods.SCI_COPY);
            }

            // RTF or HTML (custom processing)
            if ((format & (CopyFormat.Rtf | CopyFormat.Html)) > 0)
            {
                if (!registeredFormats)
                {
                    CF_LINESELECT = NativeMethods.RegisterClipboardFormat("MSDEVLineSelect");
                    CF_VSLINETAG = NativeMethods.RegisterClipboardFormat("VisualStudioEditorOperationsLineCutCopyClipboardTag");
                    CF_HTML = NativeMethods.RegisterClipboardFormat("HTML Format");
                    CF_RTF = NativeMethods.RegisterClipboardFormat("Rich Text Format");
                    registeredFormats = true;
                }

                StyleData[] styles = null;
                List<ArraySegment<byte>> styledSelections = null;

                var lineCopy = false;
                var selIsEmpty = scintilla.DirectMessage(NativeMethods.SCI_GETSELECTIONEMPTY) != IntPtr.Zero;
                if (selIsEmpty)
                {
                    if (allowLine)
                    {
                        // Get the current line
                        var mainSelection = scintilla.DirectMessage(NativeMethods.SCI_GETMAINSELECTION).ToInt32();
                        var mainCaretPos = scintilla.DirectMessage(NativeMethods.SCI_GETSELECTIONNCARET, new IntPtr(mainSelection)).ToInt32();
                        var lineIndex = scintilla.DirectMessage(NativeMethods.SCI_LINEFROMPOSITION, new IntPtr(mainCaretPos)).ToInt32();
                        var lineStartBytePos = scintilla.DirectMessage(NativeMethods.SCI_POSITIONFROMLINE, new IntPtr(lineIndex)).ToInt32();
                        var lineLength = scintilla.DirectMessage(NativeMethods.SCI_POSITIONFROMLINE, new IntPtr(lineIndex)).ToInt32();

                        styledSelections = GetStyledSelections(scintilla, true, lineStartBytePos, (lineStartBytePos + lineLength), out styles);
                        lineCopy = true;
                    }
                }
                else
                {
                    // Get every selection
                    styledSelections = GetStyledSelections(scintilla, false, 0, 0, out styles);
                }

                // If we have selections and can open the clipboard
                if (styledSelections != null && NativeMethods.OpenClipboard(scintilla.Handle))
                {
                    if (!defaultProcessing)
                    {
                        NativeMethods.EmptyClipboard();

                        if (lineCopy)
                        {
                            // Clipboard tags
                            NativeMethods.SetClipboardData(CF_LINESELECT, IntPtr.Zero);
                            NativeMethods.SetClipboardData(CF_VSLINETAG, IntPtr.Zero);
                        }
                    }

                    // RTF
                    if ((format & CopyFormat.Rtf) > 0)
                        CopyRtf(scintilla, styles, styledSelections);

                    // HTML
                    if ((format & CopyFormat.Html) > 0)
                        CopyHtml(scintilla, styles, styledSelections);

                    NativeMethods.CloseClipboard();
                }
            }
        }

        private static unsafe void CopyHtml(Scintilla scintilla, StyleData[] usedStyles, List<ArraySegment<byte>> styledSelections)
        {
        }


        private static unsafe void CopyRtf(Scintilla scintilla, StyleData[] styles, List<ArraySegment<byte>> styledSelections)
        {
            // NppExport -> NppExport.cpp
            // NppExport -> RTFExporter.cpp
            // http://en.wikipedia.org/wiki/Rich_Text_Format
            // https://msdn.microsoft.com/en-us/library/windows/desktop/ms649013.aspx
            // http://forums.codeguru.com/showthread.php?242982-Converting-pixels-to-twips
            // http://en.wikipedia.org/wiki/UTF-8

            try
            {
                // Calculate twips per space
                int twips;
                var fontStyle = FontStyle.Regular;
                if (styles[Style.Default].Weight >= 700)
                    fontStyle |= FontStyle.Bold;
                if (styles[Style.Default].Italic != 0)
                    fontStyle |= FontStyle.Italic;
                if (styles[Style.Default].Underline != 0)
                    fontStyle |= FontStyle.Underline;

                using (var graphics = scintilla.CreateGraphics())
                using (var font = new Font(styles[Style.Default].FontName, styles[Style.Default].SizeF, fontStyle))
                {
                    var width = graphics.MeasureString(" ", font).Width;
                    twips = (int)((width / graphics.DpiX) * 1440);
                }

                // Write RTF
                using (var ms = new NativeMemoryStream(styledSelections.Sum(s => s.Count)))
                using (var tw = new StreamWriter(ms, Encoding.ASCII))
                {
                    var ansicpg = "";
                    var codePage = scintilla.DirectMessage(NativeMethods.SCI_GETCODEPAGE).ToInt32();
                    if (codePage != NativeMethods.SC_CP_UTF8)
                        ansicpg = @"\ansicpg" + codePage; // Experimental

                    var tabWidth = scintilla.DirectMessage(NativeMethods.SCI_GETTABWIDTH).ToInt32();
                    var deftab = tabWidth * twips;

                    tw.WriteLine(@"{{\rtf1\ansi{0}\deff0\deftab{1}", ansicpg, deftab);
                    tw.Flush();

                    // Build the font table
                    tw.Write(@"{\fonttbl");
                    tw.Write(@"{{\f0 {0};}}", styles[Style.Default].FontName);
                    var fontIndex = 1;
                    for (int i = 0; i < styles.Length; i++)
                    {
                        if (i == Style.Default)
                            continue;

                        if (styles[i].Used)
                        {
                            // Not a completely unique list, but close enough
                            if (styles[i].FontName != styles[Style.Default].FontName)
                            {
                                styles[i].FontIndex = fontIndex++;
                                tw.Write(@"{{\f{0} {1};}}", styles[i].FontIndex, styles[i].FontName);
                            }
                        }
                    }
                    tw.WriteLine("}"); // fonttbl
                    tw.Flush();

                    // Build the color table
                    tw.Write(@"{\colortbl;");
                    tw.Write(@"\red{0}\green{1}\blue{1};", (styles[Style.Default].ForeColor >> 0) & 0xFF, (styles[Style.Default].ForeColor >> 8) & 0xFF, (styles[Style.Default].ForeColor >> 16) & 0xFF);
                    tw.Write(@"\red{0}\green{1}\blue{1};", (styles[Style.Default].BackColor >> 0) & 0xFF, (styles[Style.Default].BackColor >> 8) & 0xFF, (styles[Style.Default].BackColor >> 16) & 0xFF);
                    styles[Style.Default].BackColorIndex = 1;

                    var colorIndex = 0;
                    for (int i = 0; i < styles.Length; i++)
                    {
                        if (i == Style.Default)
                            continue;

                        if (styles[i].Used)
                        {
                            if (styles[i].ForeColor != styles[Style.Default].ForeColor)
                            {
                                styles[i].ForeColorIndex = colorIndex++;
                                tw.Write(@"\red{0}\green{1}\blue{1};", (styles[i].ForeColor >> 0) & 0xFF, (styles[i].ForeColor >> 8) & 0xFF, (styles[i].ForeColor >> 16) & 0xFF);
                            }
                            else
                            {
                                styles[i].ForeColorIndex = 0;
                            }

                            if (styles[i].BackColor != styles[Style.Default].BackColor)
                            {
                                styles[i].BackColorIndex = colorIndex++;
                                tw.Write(@"\red{0}\green{1}\blue{1};", (styles[i].BackColor >> 0) & 0xFF, (styles[i].BackColor >> 8) & 0xFF, (styles[i].BackColor >> 16) & 0xFF);
                            }
                            else
                            {
                                styles[i].BackColorIndex = 1;
                            }
                        }
                    }
                    tw.WriteLine("}"); // colortbl
                    tw.Flush();

                    var lastStyle = Style.Default;
                    tw.Write(@"\f{0}\fs{1}\cf{2}\cb{3}\chshdng0\chcbpat{3} ", styles[Style.Default].FontIndex, (int)(styles[Style.Default].SizeF * 2), styles[Style.Default].ForeColorIndex, styles[Style.Default].BackColorIndex);
                    foreach (var selection in styledSelections)
                    {
                        var endOffset = selection.Offset + selection.Count;
                        for (int i = selection.Offset; i < endOffset; i += 2)
                        {
                            var ch = selection.Array[i];
                            var style = selection.Array[i + 1];

                            if (lastStyle != style)
                            {
                                if (styles[lastStyle].FontIndex != styles[style].FontIndex)
                                    tw.Write(@"\f{0}", styles[style].FontIndex);
                                if (styles[lastStyle].SizeF != styles[style].SizeF)
                                    tw.Write(@"\fs{0}", (int)(styles[style].SizeF * 2));
                                if (styles[lastStyle].ForeColorIndex != styles[style].ForeColorIndex)
                                    tw.Write(@"\cf{0}", styles[style].ForeColorIndex);
                                if (styles[lastStyle].BackColorIndex != styles[style].BackColorIndex)
                                    tw.Write(@"\cb{0}\chshdng0\chcbpat{0}", styles[style].BackColorIndex);
                                if (styles[lastStyle].Italic != styles[style].Italic)
                                    tw.Write(@"\i{0}", styles[style].Italic != 0 ? "" : "0");
                                if (styles[lastStyle].Underline != styles[style].Underline)
                                    tw.Write(@"\ul{0}", styles[style].Underline != 0 ? "" : "0");
                                if (styles[lastStyle].Weight != styles[style].Weight)
                                {
                                    if (styles[style].Weight >= 700 && styles[lastStyle].Weight < 700)
                                        tw.Write(@"\b");
                                    else if (styles[style].Weight < 700 && styles[lastStyle].Weight >= 700)
                                        tw.Write(@"\b0");
                                }

                                lastStyle = style;
                                tw.Write(" "); // Delimiter
                            }

                            switch (ch)
                            {
                                case (byte)'{':
                                    tw.Write(@"\{");
                                    break;

                                case (byte)'}':
                                    tw.Write(@"\}");
                                    break;

                                case (byte)'\\':
                                    tw.Write(@"\\");
                                    break;

                                case (byte)'\t':
                                    tw.Write(@"\tab ");
                                    break;

                                case (byte)'\r':
                                    if (i + 2 < endOffset)
                                    {
                                        if (selection.Array[i + 2] == (byte)'\n')
                                            i += 2;
                                    }
                                    goto case (byte)'\n';

                                case 0xC2:
                                    if (i + 2 < endOffset)
                                    {
                                        if (selection.Array[i + 2] == 0x85) // NEL \u0085
                                        {
                                            i += 2;
                                            goto case (byte)'\n';
                                        }
                                    }
                                    goto default;

                                case 0xE2:
                                    if (i + 4 < endOffset)
                                    {
                                        if (selection.Array[i + 2] == 0x80 && selection.Array[i + 4] == 0xA8) // LS \u2028
                                        {
                                            i += 4;
                                            goto case (byte)'\n';
                                        }
                                        else if (selection.Array[i + 2] == 0x80 && selection.Array[i + 4] == 0xA9) // PS \u2029
                                        {
                                            i += 4;
                                            goto case (byte)'\n';
                                        }
                                    }
                                    goto default;

                                case (byte)'\n':
                                    tw.WriteLine(@"\par");
                                    break;

                                default:
                                    if (ch == 0)
                                    {
                                        // Scintilla behavior is to allow control characters except for
                                        // NUL which will cause the Clipboard to truncate the string.
                                        tw.Write(" ");
                                        break;
                                    }

                                    if (ch > 0x7F && codePage == NativeMethods.SC_CP_UTF8)
                                    {
                                        int unicode = 0;
                                        if (ch < 0xE0 && i + 2 < endOffset)
                                        {
                                            i += 2;
                                            unicode |= ((0x1F & ch) << 6);
                                            unicode |= (0x3F & selection.Array[i + 2]);
                                            tw.Write(@"\u{0}", unicode);
                                            break;
                                        }
                                        else if (ch < 0xF0 && i + 4 < endOffset)
                                        {
                                            i += 4;
                                            unicode |= ((0xF & ch) << 12);
                                            unicode |= ((0x3F & selection.Array[i + 2]) << 6);
                                            unicode |= (0x3F & selection.Array[i + 4]);
                                            tw.Write(@"\u{0}", unicode);
                                            break;
                                        }
                                        else if (ch < 0xF8 && i + 6 < endOffset)
                                        {
                                        }
                                    }

                                    // Codepage char
                                    tw.Write((char)ch);
                                    break;
                            }
                        }
                    }

                    tw.WriteLine("}"); // rtf1
                    tw.Flush();

                    Debug.WriteLine(GetString(ms.Pointer, (int)ms.Length, Encoding.ASCII));
                    if (NativeMethods.SetClipboardData(CF_RTF, ms.Pointer) != IntPtr.Zero)
                        ms.FreeOnDispose = false; // Clipboard will free memory
                }
            }
            catch (Exception ex)
            {
                // Yes, we swallow any exceptions. That may seem like code smell but this matches
                // the behavior of the Clipboard class, Windows Forms controls, and native Scintilla.
                Debug.Fail(ex.Message, ex.ToString());
            }
        }

        private static unsafe List<ArraySegment<byte>> GetStyledSelections(Scintilla scintilla, bool allowLine, int lineStartBytePos, int lineEndBytePos, out StyleData[] styles)
        {
            var selections = new List<ArraySegment<byte>>();
            if (allowLine)
            {
                var styledText = GetStyledText(scintilla, lineStartBytePos, lineEndBytePos, true);
                selections.Add(styledText);
            }
            else
            {
                var ranges = new List<Tuple<int, int>>();
                var selCount = scintilla.DirectMessage(NativeMethods.SCI_GETSELECTIONS).ToInt32();
                for (int i = 0; i < selCount; i++)
                {
                    var selStartBytePos = scintilla.DirectMessage(NativeMethods.SCI_GETSELECTIONNSTART, new IntPtr(i)).ToInt32();
                    var selEndBytePos = scintilla.DirectMessage(NativeMethods.SCI_GETSELECTIONNEND, new IntPtr(i)).ToInt32();

                    ranges.Add(Tuple.Create(selStartBytePos, selEndBytePos));
                }

                var selIsRect = scintilla.DirectMessage(NativeMethods.SCI_SELECTIONISRECTANGLE) != IntPtr.Zero;
                if (selIsRect)
                {
                    // Sort top to bottom
                    ranges.OrderBy(r => r.Item1);
                }

                foreach (var range in ranges)
                {
                    var styledText = GetStyledText(scintilla, range.Item1, range.Item2, selIsRect);
                    selections.Add(styledText);
                }
            }

            // Build a list of (used) styles
            styles = new StyleData[NativeMethods.STYLE_MAX + 1];

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

            foreach (var sel in selections)
            {
                for (int i = 0; i < sel.Count; i += 2)
                {
                    var style = sel.Array[i + 1];
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
            }

            return selections;
        }

        private static unsafe ArraySegment<byte> GetStyledText(Scintilla scintilla, int startBytePos, int endBytePos, bool normalizeLineBreaks)
        {
            var byteLength = (endBytePos - startBytePos);
            var buffer = new byte[(byteLength * 2) + 2];
            fixed (byte* bp = buffer)
            {
                NativeMethods.Sci_TextRange* tr = stackalloc NativeMethods.Sci_TextRange[1];
                tr->chrg.cpMin = startBytePos;
                tr->chrg.cpMax = endBytePos;
                tr->lpstrText = new IntPtr(bp);

                scintilla.DirectMessage(NativeMethods.SCI_GETSTYLEDTEXT, IntPtr.Zero, new IntPtr(tr));
                byteLength *= 2;
            }

            if (normalizeLineBreaks)
            {
                if (byteLength >= 2 && buffer[byteLength - 2] == (byte)'\r')
                {
                    // LF \u000A
                    buffer[byteLength] = (byte)'\n';
                    buffer[byteLength + 1] = buffer[byteLength - 1];
                    byteLength += 2; // Write over the \0\0
                }
                else if (byteLength >= 2 && buffer[byteLength - 2] == (byte)'\n')
                {
                    // CR \u000D
                    if (byteLength == 2 || (buffer[byteLength - 4] != (byte)'\r'))
                    {
                        buffer[byteLength - 2] = (byte)'\r';
                        buffer[byteLength] = (byte)'\n';
                        buffer[byteLength + 1] = buffer[byteLength - 1];
                        byteLength += 2; // Write over the \0\0
                    }
                }
                else if (byteLength >= 4 && buffer[byteLength - 4] == 0xC2 && buffer[byteLength - 2] == 0x85)
                {
                    // NEL \u0085
                    buffer[byteLength - 4] = (byte)'\r';
                    buffer[byteLength - 2] = (byte)'\n';
                }
                else if (byteLength >= 6 && buffer[byteLength - 6] == 0xE2 && buffer[byteLength - 4] == 0x80 && buffer[byteLength - 2] == 0xA8)
                {
                    // LS \u2028
                    buffer[byteLength - 6] = (byte)'\r';
                    buffer[byteLength - 4] = (byte)'\n';
                    byteLength -= 2;
                }
                else if (byteLength >= 6 && buffer[byteLength - 6] == 0xE2 && buffer[byteLength - 4] == 0x80 && buffer[byteLength - 2] == 0xA9)
                {
                    // PS \u2029
                    buffer[byteLength - 6] = (byte)'\r';
                    buffer[byteLength - 4] = (byte)'\n';
                    byteLength -= 2;
                }
            }

            return new ArraySegment<byte>(buffer, 0, byteLength);
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

        public static int TranslateKeys(Keys keys)
        {
            int keyCode;

            // For some reason Scintilla uses different values for these keys...
            switch (keys & Keys.KeyCode)
            {
                case Keys.Down:
                    keyCode = NativeMethods.SCK_DOWN;
                    break;
                case Keys.Up:
                    keyCode = NativeMethods.SCK_UP;
                    break;
                case Keys.Left:
                    keyCode = NativeMethods.SCK_LEFT;
                    break;
                case Keys.Right:
                    keyCode = NativeMethods.SCK_RIGHT;
                    break;
                case Keys.Home:
                    keyCode = NativeMethods.SCK_HOME;
                    break;
                case Keys.End:
                    keyCode = NativeMethods.SCK_END;
                    break;
                case Keys.Prior:
                    keyCode = NativeMethods.SCK_PRIOR;
                    break;
                case Keys.Next:
                    keyCode = NativeMethods.SCK_NEXT;
                    break;
                case Keys.Delete:
                    keyCode = NativeMethods.SCK_DELETE;
                    break;
                case Keys.Insert:
                    keyCode = NativeMethods.SCK_INSERT;
                    break;
                case Keys.Escape:
                    keyCode = NativeMethods.SCK_ESCAPE;
                    break;
                case Keys.Back:
                    keyCode = NativeMethods.SCK_BACK;
                    break;
                case Keys.Tab:
                    keyCode = NativeMethods.SCK_TAB;
                    break;
                case Keys.Return:
                    keyCode = NativeMethods.SCK_RETURN;
                    break;
                case Keys.Add:
                    keyCode = NativeMethods.SCK_ADD;
                    break;
                case Keys.Subtract:
                    keyCode = NativeMethods.SCK_SUBTRACT;
                    break;
                case Keys.Divide:
                    keyCode = NativeMethods.SCK_DIVIDE;
                    break;
                case Keys.LWin:
                    keyCode = NativeMethods.SCK_WIN;
                    break;
                case Keys.RWin:
                    keyCode = NativeMethods.SCK_RWIN;
                    break;
                case Keys.Apps:
                    keyCode = NativeMethods.SCK_MENU;
                    break;
                case Keys.Oem2:
                    keyCode = (byte)'/';
                    break;
                case Keys.Oem3:
                    keyCode = (byte)'`';
                    break;
                case Keys.Oem4:
                    keyCode = '[';
                    break;
                case Keys.Oem5:
                    keyCode = '\\';
                    break;
                case Keys.Oem6:
                    keyCode = ']';
                    break;
                default:
                    keyCode = (int)(keys & Keys.KeyCode);
                    break;
            }

            // No translation necessary for the modifiers. Just add them back in.
            var keyDefinition = keyCode | (int)(keys & Keys.Modifiers);
            return keyDefinition;
        }

        #endregion Methods

        #region Types

        private struct StyleData
        {
            public bool Used;
            public string FontName;
            public int FontIndex; // RTF Only
            public float SizeF;
            public int Weight;
            public int Italic;
            public int Underline;
            public int BackColor;
            public int BackColorIndex; // RTF Only
            public int ForeColor;
            public int ForeColorIndex; // RTF Only
            public int Case; // HTML only
            public int Visible; // HTML only
        }

        #endregion Types
    }
}
