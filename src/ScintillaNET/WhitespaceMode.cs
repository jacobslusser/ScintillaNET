﻿namespace ScintillaNET
{
    /// <summary>
    /// Specifies the display mode of whitespace characters.
    /// </summary>
    public enum WhitespaceMode
    {
        /// <summary>
        /// The normal display mode with whitespace displayed as an empty background color.
        /// </summary>
        Invisible = NativeMethods.SCWS_INVISIBLE,

        /// <summary>
        /// Whitespace characters are drawn as dots and arrows.
        /// </summary>
        VisibleAlways = NativeMethods.SCWS_VISIBLEALWAYS,

        /// <summary>
        /// Whitespace used for indentation is displayed normally but after the first visible character,
        /// it is shown as dots and arrows.
        /// </summary>
        VisibleAfterIndent = NativeMethods.SCWS_VISIBLEAFTERINDENT,
    }
}
