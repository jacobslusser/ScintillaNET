#region License
/*
MIT License

Copyright(c) 2022 Petteri Kautonen

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Scintilla.NET.Abstractions;
public class ScintillaApiStructs
{
    // http://www.openrce.org/articles/full_view/23
    // It's worth noting that this structure (and the 64-bit version below) represents the ILoader
    // class virtual function table (vtable), NOT the ILoader interface defined in ILexer.h.
    // In this case they are identical because the ILoader class contains only functions.
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ILoaderVTable32
    {
        public ReleaseDelegate Release;
        public AddDataDelegate AddData;
        public ConvertToDocumentDelegate ConvertToDocument;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int ReleaseDelegate(IntPtr self);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int AddDataDelegate(IntPtr self, byte* data, int length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate IntPtr ConvertToDocumentDelegate(IntPtr self);
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ILoaderVTable64
    {
        public ReleaseDelegate Release;
        public AddDataDelegate AddData;
        public ConvertToDocumentDelegate ConvertToDocument;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ReleaseDelegate(IntPtr self);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int AddDataDelegate(IntPtr self, byte* data, int length);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr ConvertToDocumentDelegate(IntPtr self);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Sci_CharacterRange
    {
        public int cpMin;
        public int cpMax;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Sci_NotifyHeader
    {
        public IntPtr hwndFrom;
        public IntPtr idFrom;
        public int code;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Sci_TextRange
    {
        public Sci_CharacterRange chrg;
        public IntPtr lpstrText;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SCNotification
    {
        public Sci_NotifyHeader nmhdr;
        public IntPtr position;
        public int ch;
        public int modifiers;
        public int modificationType;
        public IntPtr text;
        public IntPtr length;
        public IntPtr linesAdded;
        public int message;
        public IntPtr wParam;
        public IntPtr lParam;
        public IntPtr line;
        public int foldLevelNow;
        public int foldLevelPrev;
        public int margin;
        public int listType;
        public int x;
        public int y;
        public int token;
        public IntPtr annotationLinesAdded;
        public int updated;
        public int listCompletionMethod;
    }
}