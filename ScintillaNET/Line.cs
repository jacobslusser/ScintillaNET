using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScintillaNET
{
    /// <summary>
    /// Represents a line of text in a <see cref="Scintilla" /> control.
    /// </summary>
    public class Line
    {
        private readonly Scintilla scintilla;

        /// <summary>
        /// Gets the zero-based character position in the document where the line ends (exclusive).
        /// </summary>
        /// <returns>The equivalent of <see cref="StartPosition" /> + <see cref="TextLength" />.</returns>
        public int EndPosition
        {
            get
            {
                return StartPosition + TextLength;
            }
        }

        /// <summary>
        /// Gets the line index.
        /// </summary>
        /// <returns>The zero-based line index within the <see cref="LineCollection" /> that created it.</returns>
        public int Index { get; private set; }

        /// <summary>
        /// Gets the zero-based character position in the document where the line begins.
        /// </summary>
        /// <returns>The document position of the first character in the line.</returns>
        public int StartPosition
        {
            get
            {
                return scintilla.Lines.CharPositionFromLine(Index);
            }
        }

        /// <summary>
        /// Gets the line text.
        /// </summary>
        /// <returns>A string representing the document line.</returns>
        /// <remarks>The returned text includes any end of line characters.</remarks>
        public unsafe string Text
        {
            get
            {
                var start = scintilla.DirectMessage(NativeMethods.SCI_POSITIONFROMLINE, new IntPtr(Index));
                var length = scintilla.DirectMessage(NativeMethods.SCI_LINELENGTH, new IntPtr(Index));
                var ptr = scintilla.DirectMessage(NativeMethods.SCI_GETRANGEPOINTER, start, length);
                if (ptr == IntPtr.Zero)
                    return string.Empty;

                var text = new string((sbyte*)ptr, 0, length.ToInt32(), scintilla.Encoding);
                return text;
            }
        }

        /// <summary>
        /// Gets the length of the line.
        /// </summary>
        /// <returns>The number of characters in the line including any end of line characters.</returns>
        public int TextLength
        {
            get
            {
                return scintilla.Lines.CharLineLength(Index);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Line" /> class.
        /// </summary>
        /// <param name="scintilla">The <see cref="Scintilla" /> control that created this line.</param>
        /// <param name="index">The index of this line within the <see cref="LineCollection" /> that created it.</param>
        public Line(Scintilla scintilla, int index)
        {
            this.scintilla = scintilla;
            Index = index;
        }
    }
}
