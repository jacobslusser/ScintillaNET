namespace Scintilla.NET.Abstractions.Enumerations;

/// <summary>
/// Enables virtual space for rectangular selections or in other circumstances or in both.
/// </summary>
/// <remarks>This enumeration has a FlagsAttribute attribute that allows a bitwise combination of its member values.</remarks>
[Flags]
public enum VirtualSpace
{
    /// <summary>
    /// Virtual space is not enabled. This is the default.
    /// </summary>
    None = ScintillaConstants.SCVS_NONE,

    /// <summary>
    /// Virtual space is enabled for rectangular selections.
    /// </summary>
    RectangularSelection = ScintillaConstants.SCVS_RECTANGULARSELECTION,

    /// <summary>
    /// Virtual space is user accessible.
    /// </summary>
    UserAccessible = ScintillaConstants.SCVS_USERACCESSIBLE,

    /// <summary>
    /// Prevents left arrow movement and selection from wrapping to the previous line.
    /// </summary>
    NoWrapLineStart = ScintillaConstants.SCVS_NOWRAPLINESTART
}