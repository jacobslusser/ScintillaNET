namespace Scintilla.NET.Abstractions.Enumerations;

/// <summary>
/// The visual indicator used on a wrapped line.
/// </summary>
[Flags]
public enum WrapVisualFlags
{
    /// <summary>
    /// No visual indicator is displayed. This the default.
    /// </summary>
    None = ScintillaConstants.SC_WRAPVISUALFLAG_NONE,

    /// <summary>
    /// A visual indicator is displayed at th end of a wrapped subline.
    /// </summary>
    End = ScintillaConstants.SC_WRAPVISUALFLAG_END,

    /// <summary>
    /// A visual indicator is displayed at the beginning of a subline.
    /// The subline is indented by 1 pixel to make room for the display.
    /// </summary>
    Start = ScintillaConstants.SC_WRAPVISUALFLAG_START,

    /// <summary>
    /// A visual indicator is displayed in the number margin.
    /// </summary>
    Margin = ScintillaConstants.SC_WRAPVISUALFLAG_MARGIN
}