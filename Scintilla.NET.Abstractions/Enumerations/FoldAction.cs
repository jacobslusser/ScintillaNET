namespace Scintilla.NET.Abstractions.Enumerations;

/// <summary>
/// Fold actions.
/// </summary>
public enum FoldAction
{
    /// <summary>
    /// Contract the fold.
    /// </summary>
    Contract = ScintillaConstants.SC_FOLDACTION_CONTRACT,

    /// <summary>
    /// Expand the fold.
    /// </summary>
    Expand = ScintillaConstants.SC_FOLDACTION_EXPAND,

    /// <summary>
    /// Toggle between contracted and expanded.
    /// </summary>
    Toggle = ScintillaConstants.SC_FOLDACTION_TOGGLE
}