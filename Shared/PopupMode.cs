using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScintillaNET
{
    /// <summary>
    /// Behavior of the standard edit control context menu.
    /// </summary>
    /// <seealso cref="Scintilla.UsePopup(PopupMode)" />
    public enum PopupMode
    {
        /// <summary>
        /// Never show the default editing menu.
        /// </summary>
        Never = NativeMethods.SC_POPUP_NEVER,

        /// <summary>
        /// Show default editing menu if clicking on the control.
        /// </summary>
        All = NativeMethods.SC_POPUP_ALL,

        /// <summary>
        /// Show default editing menu only if clicking on text area.
        /// </summary>
        /// <remarks>To receive the <see cref="Scintilla.MarginRightClick" /> event, this value must be used.</remarks>
        /// <seealso cref="Scintilla.MarginRightClick" />
        Text = NativeMethods.SC_POPUP_TEXT
    }
}
