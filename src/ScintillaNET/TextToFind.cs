#region Using Directives

using System;
using System.Runtime.InteropServices;

#endregion Using Directives


namespace ScintillaNET
{
    /// <summary>
    /// The start position, end position, text to find as part of the FindText function.
    /// Returns the position of matching text.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TextToFind
    {
        /// <summary>
        /// Range to search. You can search backwards by setting chrg.cpMax less than chrg.cpMin.
        /// </summary>
        public CharacterRange chrg;
        /// <summary>
        /// The search pattern (zero terminated)
        /// </summary>
        public IntPtr lpstrText;
        /// <summary>
        /// Returned as the position of matching text
        /// </summary>
        public CharacterRange chrgText;
    }
}
