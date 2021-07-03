using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScintillaNET
{
    /// <summary>
    /// Possible strategies for styling text using application idle time.
    /// </summary>
    /// <seealso cref="Scintilla.IdleStyling" />
    public enum IdleStyling
    {
        /// <summary>
        /// Syntax styling is performed for all the currently visible text before displaying it.
        /// This is the default.
        /// </summary>
        None = NativeMethods.SC_IDLESTYLING_NONE,

        /// <summary>
        /// A small amount of styling is performed before display and then further styling is performed incrementally in the background as an idle-time task.
        /// This can improve initial display/scroll performance, but may result in the text initially appearing uncolored and then, some time later, it is colored.
        /// </summary>
        ToVisible = NativeMethods.SC_IDLESTYLING_TOVISIBLE,

        /// <summary>
        /// Text after the currently visible portion may be styled as an idle-time task.
        /// This will not improve initial display/scroll performance, but may improve subsequent display/scroll performance.
        /// </summary>
        AfterVisible = NativeMethods.SC_IDLESTYLING_AFTERVISIBLE,

        /// <summary>
        /// Text before and after the current visible text.
        /// This is a combination of <see cref="ToVisible" /> and <see cref="AfterVisible" />.
        /// </summary>
        All = NativeMethods.SC_IDLESTYLING_ALL
    }
}
