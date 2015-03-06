using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace ScintillaNET
{
    internal static class Helpers
    {
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
    }
}
