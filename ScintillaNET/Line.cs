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
        /// Gets the line index.
        /// </summary>
        /// <returns>The zero-based line index within the <see cref="LineCollection" /> that created it.</returns>
        public int Index { get; private set; }

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
