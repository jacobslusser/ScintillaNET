namespace Scintilla.NET.Abstractions.Enumerations;

/// <summary>
/// The line wrapping strategy.
/// </summary>
public enum WrapMode
{
    /// <summary>
    /// Line wrapping is disabled. This is the default.
    /// </summary>
    None = ScintillaConstants.SC_WRAP_NONE,

    /// <summary>
    /// Lines are wrapped on word or style boundaries.
    /// </summary>
    Word = ScintillaConstants.SC_WRAP_WORD,

    /// <summary>
    /// Lines are wrapped between any character.
    /// </summary>
    Char = ScintillaConstants.SC_WRAP_CHAR,

    /// <summary>
    /// Lines are wrapped on whitespace.
    /// </summary>
    Whitespace = ScintillaConstants.SC_WRAP_WHITESPACE
}