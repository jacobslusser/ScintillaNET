using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScintillaNET
{
    /// <summary>
    /// Provides data for the <see cref="Scintilla.AutoCSelection" /> event.
    /// </summary>
    public class AutoCSelectionEventArgs : EventArgs
    {
        private readonly Scintilla scintilla;
        private readonly IntPtr textPtr;
        private readonly int bytePosition;
        private int? position;
        private string text;

        /// <summary>
        /// Gets the start position of the word being completed.
        /// </summary>
        /// <returns>The zero-based document position of the word being completed.</returns>
        public int Position
        {
            get
            {
                if (position == null)
                    position = scintilla.Lines.ByteToCharPosition(bytePosition);

                return (int)position;
            }
        }

        /// <summary>
        /// Gets the text of the selected autocompletion item.
        /// </summary>
        /// <returns>The selected autocompletion item text.</returns>
        public unsafe string Text
        {
            get
            {
                if (text == null)
                {
                    var len = 0;
                    while (((byte*)textPtr)[len] != 0)
                        len++;

                    text = Helpers.GetString(textPtr, len, scintilla.Encoding);
                }

                return text;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoCSelectionEventArgs" /> class.
        /// </summary>
        /// <param name="scintilla">The <see cref="Scintilla" /> control that generated this event.</param>
        /// <param name="bytePosition">The zero-based byte position within the document of the word being completed.</param>
        /// <param name="text">A pointer to the selected autocompletion text.</param>
        public AutoCSelectionEventArgs(Scintilla scintilla, int bytePosition, IntPtr text)
        {
            this.scintilla = scintilla;
            this.bytePosition = bytePosition;
            this.textPtr = text;
        }
    }
}
