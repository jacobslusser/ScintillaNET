using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScintillaNET
{
    internal static class Helpers
    {
        #region Methods

        public static double AlphaToOpacity(int alpha)
        {
            if (alpha == NativeMethods.SC_ALPHA_TRANSPARENT)
                return 0.0;
            else if (alpha == NativeMethods.SC_ALPHA_NOALPHA)
                return 1.0;
            else
                return alpha / (double)NativeMethods.SC_ALPHA_OPAQUE;
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
                value = min;

            if (value > max)
                return max;

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

        public static unsafe string GetString(IntPtr bytes, int length, Encoding encoding)
        {
            var ptr = (sbyte*)bytes;
            var str = new string(ptr, 0, length, encoding);

            return str;
        }

        public static int OpacityToAlpha(double opacity)
        {
            if (opacity <= 0.0)
                return NativeMethods.SC_ALPHA_TRANSPARENT;
            else if (opacity >= 1.0)
                return NativeMethods.SC_ALPHA_NOALPHA;
            else
                return (int)Math.Round(opacity * NativeMethods.SC_ALPHA_OPAQUE, 0);
        }

        public static void ValidateCollectionIndex(int index, int count)
        {
            if (index < 0 || index >= count)
                throw new ArgumentOutOfRangeException("index", "Index must be non-negative and less than the size of the collection.");
        }

        public static void ValidateDocumentPosition(int pos, int textLength, string paramName)
        {
            if (pos < 0)
                throw new ArgumentOutOfRangeException(paramName, "Value cannot be less than zero.");
            if (pos > textLength)
                throw new ArgumentOutOfRangeException(paramName, "Value cannot exceed document length.");
        }

        #endregion Methods
    }
}
