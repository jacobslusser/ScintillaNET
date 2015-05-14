using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScintillaNET
{
    /// <summary>
    /// End-of-line format.
    /// </summary>
    public enum Eol
    {
        /// <summary>
        /// Carriage return, line feed pair "\r\n" (0x0D0A).
        /// </summary>
        CrLf = NativeMethods.SC_EOL_CRLF,

        /// <summary>
        /// Carriage return '\r' (0x0D).
        /// </summary>
        Cr = NativeMethods.SC_EOL_CR,

        /// <summary>
        /// Line feed '\n' (0x0A).
        /// </summary>
        Lf = NativeMethods.SC_EOL_LF
    }
}
