using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScintillaNET
{
    /// <summary>
    /// Specifies how tab characters are drawn when whitespace is visible.
    /// </summary>
    public enum TabDrawMode
    {
        /// <summary>
        /// The default mode of an arrow stretching until the tabstop.
        /// </summary>
        LongArrow = NativeMethods.SCTD_LONGARROW,

        /// <summary>
        /// A horizontal line stretching until the tabstop.
        /// </summary>
        Strikeout = NativeMethods.SCTD_STRIKEOUT
    }
}
