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

namespace Scintilla.NET.Abstractions;

public static class HelpersGeneral
{
    #region Fields

    private static bool registeredFormats;
    private static uint CF_HTML;
    private static uint CF_RTF;
    private static uint CF_LINESELECT;
    private static uint CF_VSLINETAG;

    #endregion Fields

    #region Methods

    public static long CopyTo(this Stream source, Stream destination)
    {
        byte[] buffer = new byte[2048];
        int bytesRead;
        long totalBytes = 0;
        while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
        {
            destination.Write(buffer, 0, bytesRead);
            totalBytes += bytesRead;
        }
        return totalBytes;
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