namespace Scintilla.NET.Abstractions.Enumerations;

/// <summary>
/// Possible status codes returned by the <see cref="Scintilla.Status" /> property.
/// </summary>
public enum Status
{
    /// <summary>
    /// No failures.
    /// </summary>
    Ok = ScintillaConstants.SC_STATUS_OK,

    /// <summary>
    /// Generic failure.
    /// </summary>
    Failure = ScintillaConstants.SC_STATUS_FAILURE,

    /// <summary>
    /// Memory is exhausted.
    /// </summary>
    BadAlloc = ScintillaConstants.SC_STATUS_BADALLOC,

    /// <summary>
    /// Regular expression is invalid.
    /// </summary>
    WarnRegex = ScintillaConstants.SC_STATUS_WARN_REGEX
}