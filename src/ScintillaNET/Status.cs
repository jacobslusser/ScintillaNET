using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScintillaNET
{
    /// <summary>
    /// Possible status codes returned by the <see cref="Scintilla.Status" /> property.
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// No failures.
        /// </summary>
        Ok = NativeMethods.SC_STATUS_OK,

        /// <summary>
        /// Generic failure.
        /// </summary>
        Failure = NativeMethods.SC_STATUS_FAILURE,

        /// <summary>
        /// Memory is exhausted.
        /// </summary>
        BadAlloc = NativeMethods.SC_STATUS_BADALLOC,

        /// <summary>
        /// Regular expression is invalid.
        /// </summary>
        WarnRegex = NativeMethods.SC_STATUS_WARN_REGEX
    }
}
