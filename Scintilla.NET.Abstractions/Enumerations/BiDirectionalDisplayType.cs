namespace Scintilla.NET.Abstractions.Enumerations;

/// <summary>
/// The display type for the <c>Scintilla.BiDirectionality</c> property.
/// </summary>
public enum BiDirectionalDisplayType
{
    /// <summary>
    /// The bi-directional display type is disabled.
    /// </summary>
    Disabled = ScintillaConstants.SC_BIDIRECTIONAL_DISABLED,

    /// <summary>
    /// The bi-directional display type is left-to-right.
    /// </summary>
    LeftToRight = ScintillaConstants.SC_BIDIRECTIONAL_L2R,

    /// <summary>
    /// The bi-directional display type is right-to-left.
    /// </summary>
    RightToLeft = ScintillaConstants.SC_BIDIRECTIONAL_R2L
}