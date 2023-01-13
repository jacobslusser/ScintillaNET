using System;
using static Scintilla.NET.Abstractions.ScintillaApiStructs;

namespace ScintillaNET;

// For internal use only
public sealed class SCNotificationEventArgs : EventArgs
{
    public SCNotification SCNotification { get; private set; }

    public SCNotificationEventArgs(SCNotification scn)
    {
        this.SCNotification = scn;
    }
}