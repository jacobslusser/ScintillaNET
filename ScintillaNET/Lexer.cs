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
        /// The Ada lexer.
        /// </summary>
        Ada = NativeMethods.SCLEX_ADA,

        /// <summary>
        /// The Assembly lexer.
        /// </summary>
        Assembly = NativeMethods.SCLEX_ASM,

        /// <summary>
        /// The Batch lexer.
        /// </summary>
        Batch = NativeMethods.SCLEX_BATCH,

        /// <summary>
        /// The C# (C++) lexer.
        /// </summary>
        CSharp = NativeMethods.SCLEX_CPP,

        /// <summary>
        /// The C language family (C++, C, C#, Java, JavaScript, etc...) lexer.
        /// </summary>
        CPP = NativeMethods.SCLEX_CPP,

        /// <summary>
        /// The Cascading Style Sheets (CSS, SCSS) lexer.
        /// </summary>
        CSS = NativeMethods.SCLEX_CSS,

        /// <summary>
        /// The Fortran lexer.
        /// </summary>
        Fortran = NativeMethods.SCLEX_FORTRAN,

        /// <summary>
        /// The HTML lexer.
        /// </summary>
        HTML = NativeMethods.SCLEX_HTML,

        /// <summary>
        /// The Java (C++) lexer.
        /// </summary>
        Java = NativeMethods.SCLEX_CPP,

        /// <summary>
        /// The JavaScript (C++) lexer.
        /// </summary>
        JavaScript = NativeMethods.SCLEX_CPP,

        /// <summary>
        /// The Lisp lexer.
        /// </summary>
        Lisp = NativeMethods.SCLEX_LISP,

        /// <summary>
        /// The Lua scripting language lexer.
        /// </summary>
        Lua = NativeMethods.SCLEX_LUA,

        /// <summary>
        /// The Pascal lexer.
        /// </summary>
        Pascal = NativeMethods.SCLEX_PASCAL,

        /// <summary>
        /// The Perl lexer.
        /// </summary>
        Perl = NativeMethods.SCLEX_PERL,

        /// <summary>
        /// The PHP lexer.
        /// </summary>
        PHP = NativeMethods.SCLEX_PHPSCRIPT,

        /// <summary>
        /// The PostgreSQL lexer.
        /// </summary>
        PostgreSQL = NativeMethods.SCLEX_SQL,

        /// <summary>
        /// The Python language lexer.
        /// </summary>
        Python = NativeMethods.SCLEX_PYTHON,

        /// <summary>
        /// The Ruby lexer.
        /// </summary>
        Ruby = NativeMethods.SCLEX_RUBY,

        /// <summary>
        /// The SmallTalk lexer.
        /// </summary>
        SmallTalk = NativeMethods.SCLEX_SMALLTALK,

        /// <summary>
        /// The Structured Query Language (SQL) lexer.
        /// </summary>
        SQL = NativeMethods.SCLEX_SQL,

        /// <summary>
        /// The VB lexer.
        /// </summary>
        VB = NativeMethods.SCLEX_VB,

        /// <summary>
        /// The Extensible Markup Language (XML) lexer.
        /// </summary>
        XML = NativeMethods.SCLEX_XML,

        /// <summary>
        /// The YAML (XML) lexer.
        /// </summary>
        YAML = NativeMethods.SCLEX_XML,
        
        /// <summary>
        /// The Blitz (Blitz3D, BlitzMax, etc...) variant of Basic lexer.
        /// </summary>
        BlitzBasic = NativeMethods.SCLEX_BLITZBASIC,
                 

        /// <summary>
        /// The Markdown syntax lexer.
        /// </summary>
        Markdown = NativeMethods.SCLEX_MARKDOWN,

        /// <summary>
        /// The R programming language lexer.
        /// </summary>
        R = NativeMethods.SCLEX_R

        
    }
}
