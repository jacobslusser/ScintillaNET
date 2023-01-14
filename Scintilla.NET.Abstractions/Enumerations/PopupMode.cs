namespace Scintilla.NET.Abstractions.Enumerations;

/// <summary>
/// Behavior of the standard edit control context menu.
/// </summary>
/// <seealso cref="Scintilla.UsePopup(PopupMode)" />
public enum PopupMode
{
    /// <summary>
    /// Never show the default editing menu.
    /// </summary>
    Never = ScintillaConstants.SC_POPUP_NEVER,

    /// <summary>
    /// Show default editing menu if clicking on the control.
    /// </summary>
    All = ScintillaConstants.SC_POPUP_ALL,

    /// <summary>
    /// Show default editing menu only if clicking on text area.
    /// </summary>
    /// <remarks>To receive the <see cref="Scintilla.MarginRightClick" /> event, this value must be used.</remarks>
    /// <seealso cref="Scintilla.MarginRightClick" />
    Text = ScintillaConstants.SC_POPUP_TEXT
}