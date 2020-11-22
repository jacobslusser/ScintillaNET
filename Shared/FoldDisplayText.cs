using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScintillaNET
{
    /// <summary>
    /// Display options for fold text tags.
    /// </summary>
    public enum FoldDisplayText
    {
        /// <summary>
        /// Do not display the text tags. This is the default.
        /// </summary>
        Hidden = NativeMethods.SC_FOLDDISPLAYTEXT_HIDDEN,

        /// <summary>
        /// Display the text tags.
        /// </summary>
        Standard = NativeMethods.SC_FOLDDISPLAYTEXT_STANDARD,

        /// <summary>
        /// Display the text tags with a box drawn around them.
        /// </summary>
        Boxed = NativeMethods.SC_FOLDDISPLAYTEXT_BOXED
    }
}
