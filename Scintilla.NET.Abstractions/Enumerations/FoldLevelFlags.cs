namespace Scintilla.NET.Abstractions.Enumerations;

/// <summary>
/// Flags for additional line fold level behavior.
/// </summary>
[Flags]
public enum FoldLevelFlags
{
    /// <summary>
    /// Indicates that the line is blank and should be treated slightly different than its level may indicate;
    /// otherwise, blank lines should generally not be fold points.
    /// </summary>
    White = ScintillaConstants.SC_FOLDLEVELWHITEFLAG,

    /// <summary>
    /// Indicates that the line is a header (fold point).
    /// </summary>
    Header = ScintillaConstants.SC_FOLDLEVELHEADERFLAG
}