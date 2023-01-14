namespace Scintilla.NET.Abstractions.Enumerations;

/// <summary>
/// The symbol displayed by a <see cref="Marker" />
/// </summary>
public enum MarkerSymbol
{
    /// <summary>
    /// A circle. This symbol is typically used to indicate a breakpoint.
    /// </summary>
    Circle = ScintillaConstants.SC_MARK_CIRCLE,

    /// <summary>
    /// A rectangel with rounded edges.
    /// </summary>
    RoundRect = ScintillaConstants.SC_MARK_ROUNDRECT,

    /// <summary>
    /// An arrow (triangle) pointing right.
    /// </summary>
    Arrow = ScintillaConstants.SC_MARK_ARROW,

    /// <summary>
    /// A rectangle that is wider than it is tall.
    /// </summary>
    SmallRect = ScintillaConstants.SC_MARK_SMALLRECT,

    /// <summary>
    /// An arrow and tail pointing right. This symbol is typically used to indicate the current line of execution.
    /// </summary>
    ShortArrow = ScintillaConstants.SC_MARK_SHORTARROW,

    /// <summary>
    /// An invisible symbol useful for tracking the movement of lines.
    /// </summary>
    Empty = ScintillaConstants.SC_MARK_EMPTY,

    /// <summary>
    /// An arrow (triangle) pointing down.
    /// </summary>
    ArrowDown = ScintillaConstants.SC_MARK_ARROWDOWN,

    /// <summary>
    /// A minus (-) symbol.
    /// </summary>
    Minus = ScintillaConstants.SC_MARK_MINUS,

    /// <summary>
    /// A plus (+) symbol.
    /// </summary>
    Plus = ScintillaConstants.SC_MARK_PLUS,

    /// <summary>
    /// A thin vertical line. This symbol is typically used on the middle line of an expanded fold block.
    /// </summary>
    VLine = ScintillaConstants.SC_MARK_VLINE,

    /// <summary>
    /// A thin 'L' shaped line. This symbol is typically used on the last line of an expanded fold block.
    /// </summary>
    LCorner = ScintillaConstants.SC_MARK_LCORNER,

    /// <summary>
    /// A thin 't' shaped line. This symbol is typically used on the last line of an expanded nested fold block.
    /// </summary>
    TCorner = ScintillaConstants.SC_MARK_TCORNER,

    /// <summary>
    /// A plus (+) symbol with surrounding box. This symbol is typically used on the first line of a collapsed fold block.
    /// </summary>
    BoxPlus = ScintillaConstants.SC_MARK_BOXPLUS,

    /// <summary>
    /// A plus (+) symbol with surrounding box and thin vertical line. This symbol is typically used on the first line of a collapsed nested fold block.
    /// </summary>
    BoxPlusConnected = ScintillaConstants.SC_MARK_BOXPLUSCONNECTED,

    /// <summary>
    /// A minus (-) symbol with surrounding box. This symbol is typically used on the first line of an expanded fold block.
    /// </summary>
    BoxMinus = ScintillaConstants.SC_MARK_BOXMINUS,

    /// <summary>
    /// A minus (-) symbol with surrounding box and thin vertical line. This symbol is typically used on the first line of an expanded nested fold block.
    /// </summary>
    BoxMinusConnected = ScintillaConstants.SC_MARK_BOXMINUSCONNECTED,

    /// <summary>
    /// Similar to a <see cref="LCorner" />, but curved.
    /// </summary>
    LCornerCurve = ScintillaConstants.SC_MARK_LCORNERCURVE,

    /// <summary>
    /// Similar to a <see cref="TCorner" />, but curved.
    /// </summary>
    TCornerCurve = ScintillaConstants.SC_MARK_TCORNERCURVE,

    /// <summary>
    /// Similar to a <see cref="BoxPlus" /> but surrounded by a circle.
    /// </summary>
    CirclePlus = ScintillaConstants.SC_MARK_CIRCLEPLUS,

    /// <summary>
    /// Similar to a <see cref="BoxPlusConnected" />, but surrounded by a circle.
    /// </summary>
    CirclePlusConnected = ScintillaConstants.SC_MARK_CIRCLEPLUSCONNECTED,

    /// <summary>
    /// Similar to a <see cref="BoxMinus" />, but surrounded by a circle.
    /// </summary>
    CircleMinus = ScintillaConstants.SC_MARK_CIRCLEMINUS,

    /// <summary>
    /// Similar to a <see cref="BoxMinusConnected" />, but surrounded by a circle.
    /// </summary>
    CircleMinusConnected = ScintillaConstants.SC_MARK_CIRCLEMINUSCONNECTED,

    /// <summary>
    /// A special marker that displays no symbol but will affect the background color of the line.
    /// </summary>
    Background = ScintillaConstants.SC_MARK_BACKGROUND,

    /// <summary>
    /// Three dots (ellipsis).
    /// </summary>
    DotDotDot = ScintillaConstants.SC_MARK_DOTDOTDOT,

    /// <summary>
    /// Three bracket style arrows.
    /// </summary>
    Arrows = ScintillaConstants.SC_MARK_ARROWS,

    // PixMap = ScintillaConstants.SC_MARK_PIXMAP,

    /// <summary>
    /// A rectangle occupying the entire marker space.
    /// </summary>
    FullRect = ScintillaConstants.SC_MARK_FULLRECT,

    /// <summary>
    /// A rectangle occupying only the left edge of the marker space.
    /// </summary>
    LeftRect = ScintillaConstants.SC_MARK_LEFTRECT,

    /// <summary>
    /// A special marker left available to plugins.
    /// </summary>
    Available = ScintillaConstants.SC_MARK_AVAILABLE,

    /// <summary>
    /// A special marker that displays no symbol but will underline the current line text.
    /// </summary>
    Underline = ScintillaConstants.SC_MARK_UNDERLINE,

    /// <summary>
    /// A user-defined image. Images can be set using the <see cref="Marker.DefineRgbaImage" /> method.
    /// </summary>
    RgbaImage = ScintillaConstants.SC_MARK_RGBAIMAGE,

    /// <summary>
    /// A left-rotated bookmark.
    /// </summary>
    Bookmark = ScintillaConstants.SC_MARK_BOOKMARK,

    // Character = ScintillaConstants.SC_MARK_CHARACTER
}