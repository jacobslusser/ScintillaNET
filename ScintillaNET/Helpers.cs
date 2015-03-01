using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScintillaNET
{
    internal static class Helpers
    {
        #region Methods

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

        public static unsafe string GetString(IntPtr bytes, int length, Encoding encoding)
        {
            var ptr = (sbyte*)bytes;
            var str = new string(ptr, 0, length, encoding);

            return str;
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
