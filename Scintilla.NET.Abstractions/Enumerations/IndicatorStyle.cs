namespace Scintilla.NET.Abstractions.Enumerations;

/// <summary>
/// The visual appearance of an indicator.
/// </summary>
public enum IndicatorStyle
{
    /// <summary>
    /// Underlined with a single, straight line.
    /// </summary>
    Plain = ScintillaConstants.INDIC_PLAIN,

    /// <summary>
    /// A squiggly underline. Requires 3 pixels of descender space.
    /// </summary>
    Squiggle = ScintillaConstants.INDIC_SQUIGGLE,

    /// <summary>
    /// A line of small T shapes.
    /// </summary>
    TT = ScintillaConstants.INDIC_TT,

    /// <summary>
    /// Diagonal hatching.
    /// </summary>
    Diagonal = ScintillaConstants.INDIC_DIAGONAL,

    /// <summary>
    /// Strike out.
    /// </summary>
    Strike = ScintillaConstants.INDIC_STRIKE,

    /// <summary>
    /// An indicator with no visual effect.
    /// </summary>
    Hidden = ScintillaConstants.INDIC_HIDDEN,

    /// <summary>
    /// A rectangle around the text.
    /// </summary>
    Box = ScintillaConstants.INDIC_BOX,

    /// <summary>
    /// A rectangle around the text with rounded corners. The rectangle outline and fill transparencies can be adjusted using
    /// <see cref="Indicator.Alpha" /> and <see cref="Indicator.OutlineAlpha" />.
    /// </summary>
    RoundBox = ScintillaConstants.INDIC_ROUNDBOX,

    /// <summary>
    /// A rectangle around the text. The rectangle outline and fill transparencies can be adjusted using
    /// <see cref="Indicator.Alpha" /> and <see cref="Indicator.OutlineAlpha"/>.
    /// </summary>
    StraightBox = ScintillaConstants.INDIC_STRAIGHTBOX,

    /// <summary>
    /// A dashed underline.
    /// </summary>
    Dash = ScintillaConstants.INDIC_DASH,

    /// <summary>
    /// A dotted underline.
    /// </summary>
    Dots = ScintillaConstants.INDIC_DOTS,

    /// <summary>
    /// Similar to <see cref="Squiggle" /> but only using 2 vertical pixels so will fit under small fonts.
    /// </summary>
    SquiggleLow = ScintillaConstants.INDIC_SQUIGGLELOW,

    /// <summary>
    /// A dotted rectangle around the text. The dots transparencies can be adjusted using
    /// <see cref="Indicator.Alpha" /> and <see cref="Indicator.OutlineAlpha" />.
    /// </summary>
    DotBox = ScintillaConstants.INDIC_DOTBOX,

    // PIXMAP

    /// <summary>
    /// A 2-pixel thick underline with 1 pixel insets on either side.
    /// </summary>
    CompositionThick = ScintillaConstants.INDIC_COMPOSITIONTHICK,

    /// <summary>
    /// A 1-pixel thick underline with 1 pixel insets on either side.
    /// </summary>
    CompositionThin = ScintillaConstants.INDIC_COMPOSITIONTHIN,

    /// <summary>
    /// A rectangle around the entire character area. The rectangle outline and fill transparencies can be adjusted using
    /// <see cref="Indicator.Alpha" /> and <see cref="Indicator.OutlineAlpha"/>.
    /// </summary>
    FullBox = ScintillaConstants.INDIC_FULLBOX,

    /// <summary>
    /// An indicator that will change the foreground color of text to the foreground color of the indicator.
    /// </summary>
    TextFore = ScintillaConstants.INDIC_TEXTFORE,

    /// <summary>
    /// A triangle below the start of the indicator range.
    /// </summary>
    Point = ScintillaConstants.INDIC_POINT,

    /// <summary>
    /// A triangle below the center of the first character of the indicator range.
    /// </summary>
    PointCharacter = ScintillaConstants.INDIC_POINTCHARACTER /*,

        /// <summary>
        /// A vertical gradient between a color and alpha at top to fully transparent at bottom.
        /// </summary>
        Gradient = NativeMethods.INDIC_GRADIENT,

        /// <summary>
        /// A vertical gradient with color and alpha in the mid-line fading to fully transparent at top and bottom.
        /// </summary>
        GradientCenter = NativeMethods.INDIC_GRADIENTCENTRE */
}