namespace Scintilla.NET.Abstractions.Enumerations;

/// <summary>
/// End-of-line format.
/// </summary>
public enum Eol
{
    /// <summary>
    /// Carriage Return, Line Feed pair "\r\n" (0x0D0A).
    /// </summary>
    CrLf = ScintillaConstants.SC_EOL_CRLF,

    /// <summary>
    /// Carriage Return '\r' (0x0D).
    /// </summary>
    Cr = ScintillaConstants.SC_EOL_CR,

    /// <summary>
    /// Line Feed '\n' (0x0A).
    /// </summary>
    Lf = ScintillaConstants.SC_EOL_LF
}