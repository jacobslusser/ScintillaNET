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

        public static unsafe byte[] GetBytes(string text, Encoding encoding, bool zeroTerminated)
        {
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

        #endregion Methods
    }
}
