namespace Scintilla.NET.Abstractions.Enumerations;

/// <summary>
/// Configuration options for automatic code folding.
/// </summary>
/// <remarks>This enumeration has a FlagsAttribute attribute that allows a bitwise combination of its member values.</remarks>
[Flags]
public enum AutomaticFold
{
    /// <summary>
    /// Automatic folding is disabled. This is the default.
    /// </summary>
    None = 0,

    /// <summary>
    /// Automatically show lines as needed. The <see cref="Scintilla.NeedShown" /> event is not raised when this value is used.
    /// </summary>
    Show = ScintillaConstants.SC_AUTOMATICFOLD_SHOW,

    /// <summary>
    /// Handle clicks in fold margin automatically. The <see cref="Scintilla.MarginClick" /> event is not raised for folding margins when this value is used.
    /// </summary>
    Click = ScintillaConstants.SC_AUTOMATICFOLD_CLICK,

    /// <summary>
    /// Show lines as needed when the fold structure is changed.
    /// </summary>
    Change = ScintillaConstants.SC_AUTOMATICFOLD_CHANGE
}