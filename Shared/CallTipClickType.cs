namespace ScintillaNET;

/// <summary>
/// The type of a call tip click.
/// </summary>
public enum CallTipClickType
{
    /// <summary>
    /// The call tip was clicked elsewhere; not the up or down arrows.
    /// </summary>
    Elsewhere = 0,

    /// <summary>
    /// The call tip up arrow was clicked.
    /// </summary>
    UpArrow = 1,

    /// <summary>
    /// The call tip down arrow was clicked.
    /// </summary>
    DownArrow = 2,
}