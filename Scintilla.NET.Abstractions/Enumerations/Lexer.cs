namespace Scintilla.NET.Abstractions.Enumerations;

/// <summary>
/// Specifies the lexer to use for syntax highlighting in a <see cref="Scintilla" /> control.
/// </summary>
public enum Lexer
{
#if SCINTILLA5
    /// <summary>
    /// The lexer enumeration wasn't found for the specified lexer name.
    /// </summary>
    /// <remarks>Scintilla 5+ does not support lexer type constants.</remarks>
    NotFound = -1,
#endif

    /// <summary>
    /// Lexing is performed by the <see cref="Scintilla" /> control container (host) using
    /// the <see cref="Scintilla.StyleNeeded" /> event.
    /// </summary>
    Container = ScintillaConstants.SCLEX_CONTAINER,

    /// <summary>
    /// No lexing should be performed.
    /// </summary>
    Null = ScintillaConstants.SCLEX_NULL,

    /// <summary>
    /// The Ada (95) language lexer.
    /// </summary>
    Ada = ScintillaConstants.SCLEX_ADA,

    /// <summary>
    /// The assembly language lexer.
    /// </summary>
    Asm = ScintillaConstants.SCLEX_ASM,

    /// <summary>
    /// The batch file lexer.
    /// </summary>
    Batch = ScintillaConstants.SCLEX_BATCH,

    /// <summary>
    /// The Clarion language 
    /// </summary>
    Clw = ScintillaConstants.SCLEX_CLW,

    /// <summary>
    /// The Clarion language No Case 
    /// </summary>
    ClwNoCase = ScintillaConstants.SCLEX_CLWNOCASE,
    /// <summary>
    /// The C language family (C++, C, C#, Java, JavaScript, etc...) lexer.
    /// </summary>
    Cpp = ScintillaConstants.SCLEX_CPP,

    /// <summary>
    /// The Cascading Style Sheets (CSS, SCSS) lexer.
    /// </summary>
    Css = ScintillaConstants.SCLEX_CSS,

    /// <summary>
    /// The Fortran language lexer.
    /// </summary>
    Fortran = ScintillaConstants.SCLEX_FORTRAN,

    /// <summary>
    /// The FreeBASIC language lexer.
    /// </summary>
    FreeBasic = ScintillaConstants.SCLEX_FREEBASIC,

    /// <summary>
    /// The HyperText Markup Language (HTML) lexer.
    /// </summary>
    Html = ScintillaConstants.SCLEX_HTML,

    /// <summary>
    /// JavaScript Object Notation (JSON) lexer.
    /// </summary>
    Json = ScintillaConstants.SCLEX_JSON,

    /// <summary>
    /// The Lisp language lexer.
    /// </summary>
    Lisp = ScintillaConstants.SCLEX_LISP,

    /// <summary>
    /// The Lua scripting language lexer.
    /// </summary>
    Lua = ScintillaConstants.SCLEX_LUA,

    /// <summary>
    /// The Matlab scripting language lexer.
    /// </summary>
    Matlab = ScintillaConstants.SCLEX_MATLAB,

    /// <summary>
    /// The Pascal language lexer.
    /// </summary>
    Pascal = ScintillaConstants.SCLEX_PASCAL,

    /// <summary>
    /// The Perl language lexer.
    /// </summary>
    Perl = ScintillaConstants.SCLEX_PERL,

    /// <summary>
    /// The PHP: Hypertext Preprocessor (PHP) script lexer.
    /// </summary>
    PhpScript = ScintillaConstants.SCLEX_PHPSCRIPT,

    /// <summary>
    /// PowerShell script lexer.
    /// </summary>
    PowerShell = ScintillaConstants.SCLEX_POWERSHELL,

    /// <summary>
    /// Properties file (INI) lexer.
    /// </summary>
    Properties = ScintillaConstants.SCLEX_PROPERTIES,

    /// <summary>
    /// The PureBasic language lexer.
    /// </summary>
    PureBasic = ScintillaConstants.SCLEX_PUREBASIC,

    /// <summary>
    /// The Python language lexer.
    /// </summary>
    Python = ScintillaConstants.SCLEX_PYTHON,

    /// <summary>
    /// The Ruby language lexer.
    /// </summary>
    Ruby = ScintillaConstants.SCLEX_RUBY,

    /// <summary>
    /// The SmallTalk language lexer.
    /// </summary>
    Smalltalk = ScintillaConstants.SCLEX_SMALLTALK,

    /// <summary>
    /// The Structured Query Language (SQL) lexer.
    /// </summary>
    Sql = ScintillaConstants.SCLEX_SQL,

    /// <summary>
    /// The Visual Basic (VB) lexer.
    /// </summary>
    Vb = ScintillaConstants.SCLEX_VB,

    /// <summary>
    /// The Visual Basic Script (VBScript) lexer.
    /// </summary>
    VbScript = ScintillaConstants.SCLEX_VBSCRIPT,

    /// <summary>
    /// The Verilog hardware description language lexer.
    /// </summary>
    Verilog = ScintillaConstants.SCLEX_VERILOG,

    /// <summary>
    /// The Extensible Markup Language (XML) lexer.
    /// </summary>
    Xml = ScintillaConstants.SCLEX_XML,

    /// <summary>
    /// The Blitz (Blitz3D, BlitzMax, etc...) variant of Basic lexer.
    /// </summary>
    BlitzBasic = ScintillaConstants.SCLEX_BLITZBASIC,

    /// <summary>
    /// The Markdown syntax lexer.
    /// </summary>
    Markdown = ScintillaConstants.SCLEX_MARKDOWN,

    /// <summary>
    /// The R programming language lexer.
    /// </summary>
    R = ScintillaConstants.SCLEX_R
}