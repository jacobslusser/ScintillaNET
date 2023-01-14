namespace Scintilla.NET.Abstractions.Enumerations;

/// <summary>
/// The number of phases used when drawing.
/// </summary>
public enum Phases
{
    /// <summary>
    /// Drawing is done in a single phase. This is the fastest but provides no support for kerning.
    /// </summary>
    One = ScintillaConstants.SC_PHASES_ONE,

    /// <summary>
    /// Drawing is done in two phases; the background first and then the text. This is the default.
    /// </summary>
    Two = ScintillaConstants.SC_PHASES_TWO,

    /// <summary>
    /// Drawing is done in multiple phases; once for each feature. This is the slowest but allows
    /// extreme ascenders and descenders to overflow into adjacent lines.
    /// </summary>
    Multiple = ScintillaConstants.SC_PHASES_MULTIPLE
}