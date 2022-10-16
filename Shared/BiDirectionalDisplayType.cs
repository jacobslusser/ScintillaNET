namespace ScintillaNET;

/// <summary>
/// The display type for the <see cref="Scintilla.BiDirectionality"/> property.
/// </summary>
public enum BiDirectionalDisplayType
{
    /// <summary>
    /// The bi-directional display type is disabled.
    /// </summary>
    Disabled = NativeMethods.SC_BIDIRECTIONAL_DISABLED,

    /// <summary>
    /// The bi-directional display type is left-to-right.
    /// </summary>
    LeftToRight = NativeMethods.SC_BIDIRECTIONAL_L2R,

    /// <summary>
    /// The bi-directional display type is right-to-left.
    /// </summary>
    RightToLeft = NativeMethods.SC_BIDIRECTIONAL_R2L
}