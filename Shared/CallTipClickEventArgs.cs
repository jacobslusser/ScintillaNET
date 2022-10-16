using System;

namespace ScintillaNET;

/// <summary>
/// Provides data for the <see cref="Scintilla.CallTipClick" /> event.
/// </summary>
public class CallTipClickEventArgs: EventArgs
{
    private readonly Scintilla scintilla;

    /// <summary>
    /// Gets the type of the call tip click.
    /// </summary>
    public CallTipClickType CallTipClickType { get; internal set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DwellEventArgs" /> class.
    /// </summary>
    /// <param name="scintilla">The <see cref="Scintilla" /> control that generated this event.</param>
    /// /// <param name="callTipClickType">Type of the call tip click.</param>
    public CallTipClickEventArgs(Scintilla scintilla, CallTipClickType callTipClickType)
    {
        this.scintilla = scintilla;
        CallTipClickType = callTipClickType;
    }
}