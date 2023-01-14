namespace Scintilla.NET.Abstractions.Enumerations;

/// <summary>
/// Specifies how tab characters are drawn when whitespace is visible.
/// </summary>
public enum TabDrawMode
{
    /// <summary>
    /// The default mode of an arrow stretching until the tabstop.
    /// </summary>
    LongArrow = ScintillaConstants.SCTD_LONGARROW,

    /// <summary>
    /// A horizontal line stretching until the tabstop.
    /// </summary>
    Strikeout = ScintillaConstants.SCTD_STRIKEOUT
}