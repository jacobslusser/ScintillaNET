using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Scintilla.NET.Abstractions;
using Scintilla.NET.Abstractions.Enumerations;
using static Scintilla.NET.Abstractions.ScintillaConstants;
using static ScintillaNET.Helpers;

namespace ScintillaNET;
internal class HelperWinForms
{
    
    public static void Copy(Scintilla scintilla, CopyFormat format, bool useSelection, bool allowLine, int startBytePos, int endBytePos)
    {
        // Plain text
        if ((format & CopyFormat.Text) > 0)
        {
            if (useSelection)
            {
                if (allowLine)
                    scintilla.DirectMessage(SCI_COPYALLOWLINE);
                else
                    scintilla.DirectMessage(SCI_COPY);
            }
            else
            {
                scintilla.DirectMessage(SCI_COPYRANGE, new IntPtr(startBytePos), new IntPtr(endBytePos));
            }
        }

        // RTF and/or HTML
        if ((format & (CopyFormat.Rtf | CopyFormat.Html)) > 0)
        {
            // If we ever allow more than UTF-8, this will have to be revisited
            Debug.Assert(scintilla.DirectMessage(SCI_GETCODEPAGE).ToInt32() == SC_CP_UTF8);

            if (!registeredFormats)
            {
                // Register non-standard clipboard formats.
                // Scintilla -> ScintillaWin.cxx
                // NppExport -> HTMLExporter.h
                // NppExport -> RTFExporter.h

                CF_LINESELECT = NativeMethods.RegisterClipboardFormat("MSDEVLineSelect");
                CF_VSLINETAG = NativeMethods.RegisterClipboardFormat("VisualStudioEditorOperationsLineCutCopyClipboardTag");
                Helpers.CF_HTML = NativeMethods.RegisterClipboardFormat("HTML Format");
                CF_RTF = NativeMethods.RegisterClipboardFormat("Rich Text Format");
                registeredFormats = true;
            }

            var lineCopy = false;
            StyleData[] styles = null;
            List<ArraySegment<byte>> styledSegments = null;

            if (useSelection)
            {
                var selIsEmpty = scintilla.DirectMessage(SCI_GETSELECTIONEMPTY) != IntPtr.Zero;
                if (selIsEmpty)
                {
                    if (allowLine)
                    {
                        // Get the current line
                        styledSegments = GetStyledSegments(scintilla, false, true, 0, 0, out styles);
                        lineCopy = true;
                    }
                }
                else
                {
                    // Get every selection
                    styledSegments = GetStyledSegments(scintilla, true, false, 0, 0, out styles);
                }
            }
            else if (startBytePos != endBytePos)
            {
                // User-specified range
                styledSegments = GetStyledSegments(scintilla, false, false, startBytePos, endBytePos, out styles);
            }

            // If we have segments and can open the clipboard
            if (styledSegments != null && styledSegments.Count > 0 && NativeMethods.OpenClipboard(scintilla.Handle))
            {
                if ((format & CopyFormat.Text) == 0)
                {
                    // Do the things default (plain text) processing would normally give us
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
                    CopyRtf(scintilla, styles, styledSegments);

                // HTML
                if ((format & CopyFormat.Html) > 0)
                    CopyHtml(scintilla, styles, styledSegments);

                NativeMethods.CloseClipboard();
            }
        }
    }

    internal static unsafe void CopyRtf(Scintilla scintilla, StyleData[] styles, List<ArraySegment<byte>> styledSegments)
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
                // TODO The twips value calculated seems too small on my computer
            }

            // Write RTF
            using (var ms = new NativeMemoryStream(styledSegments.Sum(s => s.Count)))
            using (var tw = new StreamWriter(ms, Encoding.ASCII))
            {
                var tabWidth = scintilla.DirectMessage(SCI_GETTABWIDTH).ToInt32();
                var deftab = tabWidth * twips;

                tw.WriteLine(@"{{\rtf1\ansi\deff0\deftab{0}", deftab);
                tw.Flush();

                // Build the font table
                tw.Write(@"{\fonttbl");
                tw.Write(@"{{\f0 {0};}}", styles[Style.Default].FontName);
                var fontIndex = 1;
                for (int i = 0; i < styles.Length; i++)
                {
                    if (!styles[i].Used)
                        continue;

                    if (i == Style.Default)
                        continue;

                    // Not a completely unique list, but close enough
                    if (styles[i].FontName != styles[Style.Default].FontName)
                    {
                        styles[i].FontIndex = fontIndex++;
                        tw.Write(@"{{\f{0} {1};}}", styles[i].FontIndex, styles[i].FontName);
                    }
                }
                tw.WriteLine("}"); // fonttbl
                tw.Flush();

                // Build the color table
                tw.Write(@"{\colortbl");
                tw.Write(@"\red{0}\green{1}\blue{2};", (styles[Style.Default].ForeColor >> 0) & 0xFF, (styles[Style.Default].ForeColor >> 8) & 0xFF, (styles[Style.Default].ForeColor >> 16) & 0xFF);
                tw.Write(@"\red{0}\green{1}\blue{2};", (styles[Style.Default].BackColor >> 0) & 0xFF, (styles[Style.Default].BackColor >> 8) & 0xFF, (styles[Style.Default].BackColor >> 16) & 0xFF);
                styles[Style.Default].ForeColorIndex = 0;
                styles[Style.Default].BackColorIndex = 1;
                var colorIndex = 2;
                for (int i = 0; i < styles.Length; i++)
                {
                    if (!styles[i].Used)
                        continue;

                    if (i == Style.Default)
                        continue;

                    // Not a completely unique list, but close enough
                    if (styles[i].ForeColor != styles[Style.Default].ForeColor)
                    {
                        styles[i].ForeColorIndex = colorIndex++;
                        tw.Write(@"\red{0}\green{1}\blue{2};", (styles[i].ForeColor >> 0) & 0xFF, (styles[i].ForeColor >> 8) & 0xFF, (styles[i].ForeColor >> 16) & 0xFF);
                    }
                    else
                    {
                        styles[i].ForeColorIndex = styles[Style.Default].ForeColorIndex;
                    }

                    if (styles[i].BackColor != styles[Style.Default].BackColor)
                    {
                        styles[i].BackColorIndex = colorIndex++;
                        tw.Write(@"\red{0}\green{1}\blue{2};", (styles[i].BackColor >> 0) & 0xFF, (styles[i].BackColor >> 8) & 0xFF, (styles[i].BackColor >> 16) & 0xFF);
                    }
                    else
                    {
                        styles[i].BackColorIndex = styles[Style.Default].BackColorIndex;
                    }
                }
                tw.WriteLine("}"); // colortbl
                tw.Flush();

                // Start with the default style
                tw.Write(@"\f{0}\fs{1}\cf{2}\chshdng0\chcbpat{3}\cb{3} ", styles[Style.Default].FontIndex, (int)(styles[Style.Default].SizeF * 2), styles[Style.Default].ForeColorIndex, styles[Style.Default].BackColorIndex);
                if (styles[Style.Default].Italic != 0)
                    tw.Write(@"\i");
                if (styles[Style.Default].Underline != 0)
                    tw.Write(@"\ul");
                if (styles[Style.Default].Weight >= 700)
                    tw.Write(@"\b");

                tw.AutoFlush = true;
                var lastStyle = Style.Default;
                var unicodeLineEndings = ((scintilla.DirectMessage(SCI_GETLINEENDTYPESACTIVE).ToInt32() & SC_LINE_END_TYPE_UNICODE) > 0);
                foreach (var seg in styledSegments)
                {
                    var endOffset = seg.Offset + seg.Count;
                    for (int i = seg.Offset; i < endOffset; i += 2)
                    {
                        var ch = seg.Array[i];
                        var style = seg.Array[i + 1];

                        if (lastStyle != style)
                        {
                            // Change the style
                            if (styles[lastStyle].FontIndex != styles[style].FontIndex)
                                tw.Write(@"\f{0}", styles[style].FontIndex);
                            if (styles[lastStyle].SizeF != styles[style].SizeF)
                                tw.Write(@"\fs{0}", (int)(styles[style].SizeF * 2));
                            if (styles[lastStyle].ForeColorIndex != styles[style].ForeColorIndex)
                                tw.Write(@"\cf{0}", styles[style].ForeColorIndex);
                            if (styles[lastStyle].BackColorIndex != styles[style].BackColorIndex)
                                tw.Write(@"\chshdng0\chcbpat{0}\cb{0}", styles[style].BackColorIndex);
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

                            // NOTE: We don't support StyleData.Visible and StyleData.Case in RTF

                            lastStyle = style;
                            tw.Write("\n"); // Delimiter
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
                                    if (seg.Array[i + 2] == (byte)'\n')
                                        i += 2;
                                }

                                // Either way, this is a line break
                                goto case (byte)'\n';

                            case 0xC2:
                                if (unicodeLineEndings && i + 2 < endOffset)
                                {
                                    if (seg.Array[i + 2] == 0x85) // NEL \u0085
                                    {
                                        i += 2;
                                        goto case (byte)'\n';
                                    }
                                }

                                // Not a Unicode line break
                                goto default;

                            case 0xE2:
                                if (unicodeLineEndings && i + 4 < endOffset)
                                {
                                    if (seg.Array[i + 2] == 0x80 && seg.Array[i + 4] == 0xA8) // LS \u2028
                                    {
                                        i += 4;
                                        goto case (byte)'\n';
                                    }
                                    else if (seg.Array[i + 2] == 0x80 && seg.Array[i + 4] == 0xA9) // PS \u2029
                                    {
                                        i += 4;
                                        goto case (byte)'\n';
                                    }
                                }

                                // Not a Unicode line break
                                goto default;

                            case (byte)'\n':
                                // All your line breaks are belong to us
                                tw.WriteLine(@"\par");
                                break;

                            default:

                                if (ch == 0)
                                {
                                    // Scintilla behavior is to allow control characters except for
                                    // NULL which will cause the Clipboard to truncate the string.
                                    tw.Write(" "); // Replace with space
                                    break;
                                }

                                if (ch > 0x7F)
                                {
                                    // Treat as UTF-8 code point
                                    int unicode = 0;
                                    if (ch < 0xE0 && i + 2 < endOffset)
                                    {
                                        unicode |= ((0x1F & ch) << 6);
                                        unicode |= (0x3F & seg.Array[i + 2]);
                                        tw.Write(@"\u{0}?", unicode);
                                        i += 2;
                                        break;
                                    }
                                    else if (ch < 0xF0 && i + 4 < endOffset)
                                    {
                                        unicode |= ((0xF & ch) << 12);
                                        unicode |= ((0x3F & seg.Array[i + 2]) << 6);
                                        unicode |= (0x3F & seg.Array[i + 4]);
                                        tw.Write(@"\u{0}?", unicode);
                                        i += 4;
                                        break;
                                    }
                                    else if (ch < 0xF8 && i + 6 < endOffset)
                                    {
                                        unicode |= ((0x7 & ch) << 18);
                                        unicode |= ((0x3F & seg.Array[i + 2]) << 12);
                                        unicode |= ((0x3F & seg.Array[i + 4]) << 6);
                                        unicode |= (0x3F & seg.Array[i + 6]);
                                        tw.Write(@"\u{0}?", unicode);
                                        i += 6;
                                        break;
                                    }
                                }

                                // Regular ANSI char
                                ms.WriteByte(ch);
                                break;
                        }
                    }
                }

                tw.AutoFlush = false;
                tw.WriteLine("}"); // rtf1
                tw.Flush();

                // Terminator
                ms.WriteByte(0);

                // var str = GetString(ms.Pointer, (int)ms.Length, Encoding.ASCII);
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
}
