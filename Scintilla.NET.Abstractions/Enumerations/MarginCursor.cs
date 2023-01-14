namespace Scintilla.NET.Abstractions.Enumerations;

/// <summary>
/// The display of a cursor when over a margin.
/// </summary>
public enum MarginCursor
{
    /// <summary>
    /// A normal arrow.
    /// </summary>
    Arrow = ScintillaConstants.SC_CURSORARROW,

    /// <summary>
    /// A reversed arrow.
    /// </summary>
    ReverseArrow = ScintillaConstants.SC_CURSORREVERSEARROW
}