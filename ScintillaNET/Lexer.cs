using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScintillaNET
{
    /// <summary>
    /// Specifies the lexer to use for syntax highlighting in a <see cref="Scintilla" /> control.
    /// </summary>
    public enum Lexer
    {
        /// <summary>
        /// Lexing is performed by the <see cref="Scintilla" /> control container (host) using
        /// the <see cref="Scintilla.StyleNeeded" /> event.
        /// </summary>
        Container = NativeMethods.SCLEX_CONTAINER,

        /// <summary>
        /// No lexing should be performed.
        /// </summary>
        Null = NativeMethods.SCLEX_NULL,

        /// <summary>
        /// The Python language lexer.
        /// </summary>
        Python = NativeMethods.SCLEX_PYTHON,

        /// <summary>
        /// The C language family (C++, C, C#, Java, JavaScript, etc...) lexer.
        /// </summary>
        Cpp = NativeMethods.SCLEX_CPP,

        /// <summary>
        /// The Cascading Style Sheets (CSS, SCSS) lexer.
        /// </summary>
        Css = NativeMethods.SCLEX_CSS,

        /// <summary>
        /// The Blitz (Blitz3D, BlitzMax, etc...) variant of Basic lexer.
        /// </summary>
        BlitzBasic = NativeMethods.SCLEX_BLITZBASIC
    }
}
