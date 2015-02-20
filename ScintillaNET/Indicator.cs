using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ScintillaNET
{
    /// <summary>
    /// Represents an indicator in a <see cref="Scintilla" /> control.
    /// </summary>
    public class Indicator
    {
        private readonly Scintilla scintilla;

        /*
        /// <summary>
        /// Given a position within a text range using this indicator, will return
        /// the end position of that range.
        /// </summary>
        /// <param name="position">Any zero-based byte position with the range using this indicator.</param>
        /// <returns>The end position byte index.</returns>
        public int FindEnd(int position)
        {
            return scintilla.DirectMessage(NativeMethods.SCI_INDICATOREND, new IntPtr(Index), new IntPtr(position)).ToInt32();
        }

        /// <summary>
        /// Given a position within a text range using this indicator, will return
        /// the start position of that range.
        /// </summary>
        /// <param name="position">Any zero-based byte position with the range using this indicator.</param>
        /// <returns>The start position byte index.</returns>
        public int FindStart(int position)
        {
            return scintilla.DirectMessage(NativeMethods.SCI_INDICATORSTART, new IntPtr(Index), new IntPtr(position)).ToInt32();
        }
        */

        /// <summary>
        /// Gets or sets the alpha transparency of the indicator.
        /// </summary>
        /// <returns>
        /// The alpha transparency ranging from 0 (completely transparent)
        /// to 255 (no transparency). The default is 30.
        /// </returns>
        public int Alpha
        {
            get
            {
                return scintilla.DirectMessage(NativeMethods.SCI_INDICGETALPHA, new IntPtr(Index)).ToInt32();
            }
            set
            {
                value = Helpers.Clamp(value, 0, 255);
                scintilla.DirectMessage(NativeMethods.SCI_INDICSETALPHA, new IntPtr(Index), new IntPtr(value));
            }
        }

        /// <summary>
        /// Gets or sets the color used to draw an indicator.
        /// </summary>
        /// <returns>The Color used to draw an indicator. The default varies.</returns>
        public Color ForeColor
        {
            get
            {
                var color = scintilla.DirectMessage(NativeMethods.SCI_INDICGETFORE, new IntPtr(Index)).ToInt32();
                return ColorTranslator.FromWin32(color);
            }
            set
            {
                var color = ColorTranslator.ToWin32(value);
                scintilla.DirectMessage(NativeMethods.SCI_INDICSETFORE, new IntPtr(Index), new IntPtr(color));
            }
        }

        /// <summary>
        /// Gets the zero-based indicator index this object represents.
        /// </summary>
        /// <returns>The indicator definition index within the <see cref="IndicatorCollection" />.</returns>
        public int Index { get; private set; }

        /// <summary>
        /// Gets or sets the alpha transparency of the indicator outline.
        /// </summary>
        /// <returns>
        /// The alpha transparency ranging from 0 (completely transparent)
        /// to 255 (no transparency). The default is 50.
        /// </returns>
        public int OutlineAlpha
        {
            get
            {
                return scintilla.DirectMessage(NativeMethods.SCI_INDICGETOUTLINEALPHA, new IntPtr(Index)).ToInt32();
            }
            set
            {
                value = Helpers.Clamp(value, 0, 255);
                scintilla.DirectMessage(NativeMethods.SCI_INDICSETOUTLINEALPHA, new IntPtr(Index), new IntPtr(value));
            }
        }

        /// <summary>
        /// Gets or sets the indicator style.
        /// </summary>
        /// <returns>One of the <see cref="ScintillaNET.IndicatorStyle" /> enumeration values. The default varies.</returns>
        public IndicatorStyle Style
        {
            get
            {
                return (IndicatorStyle)scintilla.DirectMessage(NativeMethods.SCI_INDICGETSTYLE, new IntPtr(Index));
            }
            set
            {
                var style = (int)value;
                scintilla.DirectMessage(NativeMethods.SCI_INDICSETSTYLE, new IntPtr(Index), new IntPtr(style));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Indicator" /> class.
        /// </summary>
        /// <param name="scintilla">The <see cref="Scintilla" /> control that created this indicator.</param>
        /// <param name="index">The index of this style within the <see cref="IndicatorCollection" /> that created it.</param>
        public Indicator(Scintilla scintilla, int index)
        {
            this.scintilla = scintilla;
            Index = index;
        }
    }
}
