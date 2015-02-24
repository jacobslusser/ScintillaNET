using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScintillaNET
{
    /// <summary>
    /// Represents a margin displayed on the left edge of a <see cref="Scintilla" /> control.
    /// </summary>
    public class Margin
    {
        private readonly Scintilla scintilla;

        /// <summary>
        /// Gets or sets the mouse cursor style when over the margin.
        /// </summary>
        /// <returns>One of the <see cref="MarginCursor" /> enumeration values. The default is <see cref="MarginCursor.Arrow" />.</returns>
        public MarginCursor Cursor
        {
            get
            {
                return (MarginCursor)scintilla.DirectMessage(NativeMethods.SCI_GETMARGINCURSORN, new IntPtr(Index));
            }
            set
            {
                var cursor = (int)value;
                scintilla.DirectMessage(NativeMethods.SCI_SETMARGINCURSORN, new IntPtr(Index), new IntPtr(cursor));
            }
        }

        /// <summary>
        /// Gets the zero-based margin index this object represents.
        /// </summary>
        /// <returns>The margin index within the <see cref="MarginCollection" />.</returns>
        public int Index { get; private set; }

        /// <summary>
        /// Gets or sets whether the margin is sensitive to mouse clicks.
        /// </summary>
        /// <returns>true if the margin is sensitive to mouse clicks; otherwise, false. The default is false.</returns>
        /// <seealso cref="Scintilla.MarginClick" />
        public bool Sensitive
        {
            get
            {
                return (scintilla.DirectMessage(NativeMethods.SCI_GETMARGINSENSITIVEN, new IntPtr(Index)) != IntPtr.Zero);
            }
            set
            {
                var sensitive = (value ? new IntPtr(1) : IntPtr.Zero);
                scintilla.DirectMessage(NativeMethods.SCI_SETMARGINSENSITIVEN, new IntPtr(Index), sensitive);
            }
        }

        /// <summary>
        /// Gets or sets the margin type.
        /// </summary>
        /// <returns>One of the <see cref="MarginType" /> enumeration values. The default is <see cref="MarginType.Symbol" />.</returns>
        public MarginType Type
        {
            get
            {
                return (MarginType)(scintilla.DirectMessage(NativeMethods.SCI_GETMARGINTYPEN, new IntPtr(Index)));
            }
            set
            {
                var type = (int)value;
                scintilla.DirectMessage(NativeMethods.SCI_SETMARGINTYPEN, new IntPtr(Index), new IntPtr(type));
            }
        }

        /// <summary>
        /// Gets or sets the width in pixels of the margin.
        /// </summary>
        /// <returns>The width of the margin measured in pixels.</returns>
        /// <remarks>Scintilla assigns various default widths.</remarks>
        public int Width
        {
            get
            {
                return scintilla.DirectMessage(NativeMethods.SCI_GETMARGINWIDTHN, new IntPtr(Index)).ToInt32();
            }
            set
            {
                value = Helpers.ClampMin(value, 0);
                scintilla.DirectMessage(NativeMethods.SCI_SETMARGINWIDTHN, new IntPtr(Index), new IntPtr(value));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Margin" /> class.
        /// </summary>
        /// <param name="scintilla">The <see cref="Scintilla" /> control that created this margin.</param>
        /// <param name="index">The index of this margin within the <see cref="MarginCollection" /> that created it.</param>
        public Margin(Scintilla scintilla, int index)
        {
            this.scintilla = scintilla;
            Index = index;
        }
    }
}
