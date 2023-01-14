namespace Scintilla.NET.Abstractions.Enumerations;

/// <summary>
/// Visibility and location of annotations in a <see cref="Scintilla" /> control
/// </summary>
public enum Annotation
{
    /// <summary>
    /// Annotations are not displayed. This is the default.
    /// </summary>
    Hidden = ScintillaConstants.ANNOTATION_HIDDEN,

    /// <summary>
    /// Annotations are drawn left justified with no adornment.
    /// </summary>
    Standard = ScintillaConstants.ANNOTATION_STANDARD,

    /// <summary>
    /// Annotations are indented to match the text and are surrounded by a box.
    /// </summary>
    Boxed = ScintillaConstants.ANNOTATION_BOXED,

    /// <summary>
    /// Annotations are indented to match the text.
    /// </summary>
    Indented = ScintillaConstants.ANNOTATION_INDENTED
}