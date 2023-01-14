namespace Scintilla.NET.Abstractions.Enumerations;

/// <summary>
/// The source of a modification
/// </summary>
public enum ModificationSource
{
    /// <summary>
    /// Modification is the result of a user operation.
    /// </summary>
    User = ScintillaConstants.SC_PERFORMED_USER,

    /// <summary>
    /// Modification is the result of an undo operation.
    /// </summary>
    Undo = ScintillaConstants.SC_PERFORMED_UNDO,

    /// <summary>
    /// Modification is the result of a redo operation.
    /// </summary>
    Redo = ScintillaConstants.SC_PERFORMED_REDO
}