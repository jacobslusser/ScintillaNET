namespace Scintilla.NET.Abstractions.Enumerations;

/// <summary>
/// The possible casing styles of a style.
/// </summary>
public enum StyleCase
{
    /// <summary>
    /// Display the text normally.
    /// </summary>
    Mixed = ScintillaConstants.SC_CASE_MIXED,

    /// <summary>
    /// Display the text in upper case.
    /// </summary>
    Upper = ScintillaConstants.SC_CASE_UPPER,

    /// <summary>
    /// Display the text in lower case.
    /// </summary>
    Lower = ScintillaConstants.SC_CASE_LOWER,

    /// <summary>
    /// Display the text in camel case.
    /// </summary>
    Camel = ScintillaConstants.SC_CASE_CAMEL
}