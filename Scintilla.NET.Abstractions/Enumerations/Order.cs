namespace Scintilla.NET.Abstractions.Enumerations;

/// <summary>
/// The sorting order for autocompletion lists.
/// </summary>
public enum Order
{
    /// <summary>
    /// Requires that an autocompletion lists be sorted in alphabetical order. This is the default.
    /// </summary>
    Presorted = ScintillaConstants.SC_ORDER_PRESORTED,

    /// <summary>
    /// Instructs a <see cref="Scintilla" /> control to perform an alphabetical sort of autocompletion lists.
    /// </summary>
    PerformSort = ScintillaConstants.SC_ORDER_PERFORMSORT,

    /// <summary>
    /// User-defined order.
    /// </summary>
    Custom = ScintillaConstants.SC_ORDER_CUSTOM
}