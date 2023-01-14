namespace Scintilla.NET.Abstractions.Enumerations;

/// <summary>
/// Lexer property types.
/// </summary>
public enum PropertyType
{
    /// <summary>
    /// A Boolean property. This is the default.
    /// </summary>
    Boolean = ScintillaConstants.SC_TYPE_BOOLEAN,

    /// <summary>
    /// An integer property.
    /// </summary>
    Integer = ScintillaConstants.SC_TYPE_INTEGER,

    /// <summary>
    /// A string property.
    /// </summary>
    String = ScintillaConstants.SC_TYPE_STRING
}