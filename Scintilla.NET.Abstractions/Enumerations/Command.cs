using static Scintilla.NET.Abstractions.ScintillaConstants;

namespace Scintilla.NET.Abstractions.Enumerations;

/// <summary>
/// Actions which can be performed by the application or bound to keys in a <see cref="Scintilla" /> control.
/// </summary>
public enum Command
{
    /// <summary>
    /// When bound to keys performs the standard platform behavior.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Performs no action and when bound to keys prevents them from propagating to the parent window.
    /// </summary>
    Null = SCI_NULL,

    // --------------------------------------------------------------------

    /// <summary>
    /// Moves the caret down one line.
    /// </summary>
    LineDown = SCI_LINEDOWN,

    /// <summary>
    /// Extends the selection down one line.
    /// </summary>
    LineDownExtend = SCI_LINEDOWNEXTEND,

    /// <summary>
    /// Extends the rectangular selection down one line.
    /// </summary>
    LineDownRectExtend = SCI_LINEDOWNRECTEXTEND,

    /// <summary>
    /// Scrolls down one line.
    /// </summary>
    LineScrollDown = SCI_LINESCROLLDOWN,

    // --------------------------------------------------------------------

    /// <summary>
    /// Moves the caret up one line.
    /// </summary>
    LineUp = SCI_LINEUP,

    /// <summary>
    /// Extends the selection up one line.
    /// </summary>
    LineUpExtend = SCI_LINEUPEXTEND,

    /// <summary>
    /// Extends the rectangular selection up one line.
    /// </summary>
    LineUpRectExtend = SCI_LINEUPRECTEXTEND,

    /// <summary>
    /// Scrolls up one line.
    /// </summary>
    LineScrollUp = SCI_LINESCROLLUP,

    // --------------------------------------------------------------------

    /// <summary>
    /// Moves the caret down one paragraph.
    /// </summary>
    ParaDown = SCI_PARADOWN,

    /// <summary>
    /// Extends the selection down one paragraph.
    /// </summary>
    ParaDownExtend = SCI_PARADOWNEXTEND,

    /// <summary>
    /// Moves the caret up one paragraph.
    /// </summary>
    ParaUp = SCI_PARAUP,

    /// <summary>
    /// Extends the selection up one paragraph.
    /// </summary>
    ParaUpExtend = SCI_PARAUPEXTEND,

    // --------------------------------------------------------------------

    /// <summary>
    /// Moves the caret left one character.
    /// </summary>
    CharLeft = SCI_CHARLEFT,

    /// <summary>
    /// Extends the selection left one character.
    /// </summary>
    CharLeftExtend = SCI_CHARLEFTEXTEND,

    /// <summary>
    /// Extends the rectangular selection left one character.
    /// </summary>
    CharLeftRectExtend = SCI_CHARLEFTRECTEXTEND,

    // --------------------------------------------------------------------

    /// <summary>
    /// Moves the caret right one character.
    /// </summary>
    CharRight = SCI_CHARRIGHT,

    /// <summary>
    /// Extends the selection right one character.
    /// </summary>
    CharRightExtend = SCI_CHARRIGHTEXTEND,

    /// <summary>
    /// Extends the rectangular selection right one character.
    /// </summary>
    CharRightRectExtend = SCI_CHARRIGHTRECTEXTEND,
    
    // --------------------------------------------------------------------

    /// <summary>
    /// Moves the caret to the start of the previous word.
    /// </summary>
    WordLeft = SCI_WORDLEFT,

    /// <summary>
    /// Extends the selection to the start of the previous word.
    /// </summary>
    WordLeftExtend = SCI_WORDLEFTEXTEND,

    /// <summary>
    /// Moves the caret to the start of the next word.
    /// </summary>
    WordRight = SCI_WORDRIGHT,

    /// <summary>
    /// Extends the selection to the start of the next word.
    /// </summary>
    WordRightExtend = SCI_WORDRIGHTEXTEND,

    // --------------------------------------------------------------------

    /// <summary>
    /// Moves the caret to the end of the previous word.
    /// </summary>
    WordLeftEnd = SCI_WORDLEFTEND,

    /// <summary>
    /// Extends the selection to the end of the previous word.
    /// </summary>
    WordLeftEndExtend = SCI_WORDLEFTENDEXTEND,

    /// <summary>
    /// Moves the caret to the end of the next word.
    /// </summary>
    WordRightEnd = SCI_WORDRIGHTEND,

    /// <summary>
    /// Extends the selection to the end of the next word.
    /// </summary>
    WordRightEndExtend = SCI_WORDRIGHTENDEXTEND,

    // --------------------------------------------------------------------

    /// <summary>
    /// Moves the caret to the previous word segment (case change or underscore).
    /// </summary>
    WordPartLeft = SCI_WORDPARTLEFT,

    /// <summary>
    /// Extends the selection to the previous word segment (case change or underscore).
    /// </summary>
    WordPartLeftExtend = SCI_WORDPARTLEFTEXTEND,

    /// <summary>
    /// Moves the caret to the next word segment (case change or underscore).
    /// </summary>
    WordPartRight = SCI_WORDPARTRIGHT,

    /// <summary>
    /// Extends the selection to the next word segment (case change or underscore).
    /// </summary>
    WordPartRightExtend = SCI_WORDPARTRIGHTEXTEND,

    // --------------------------------------------------------------------

    /// <summary>
    /// Moves the caret to the start of the line.
    /// </summary>
    Home = SCI_HOME,

    /// <summary>
    /// Extends the selection to the start of the line.
    /// </summary>
    HomeExtend = SCI_HOMEEXTEND,

    /// <summary>
    /// Extends the rectangular selection to the start of the line.
    /// </summary>
    HomeRectExtend = SCI_HOMERECTEXTEND,

    // --------------------------------------------------------------------

    /// <summary>
    /// Moves the caret to the start of the display line.
    /// </summary>
    HomeDisplay = SCI_HOMEDISPLAY,

    /// <summary>
    /// Extends the selection to the start of the display line.
    /// </summary>
    HomeDisplayExtend = SCI_HOMEDISPLAYEXTEND,

    /// <summary>
    /// Moves the caret to the start of the display or document line.
    /// </summary>
    HomeWrap = SCI_HOMEWRAP,

    /// <summary>
    /// Extends the selection to the start of the display or document line.
    /// </summary>
    HomeWrapExtend = SCI_HOMEWRAPEXTEND,

    // --------------------------------------------------------------------

    /// <summary>
    /// Moves the caret to the first non-whitespace character of the line.
    /// </summary>
    VcHome = SCI_VCHOME,

    /// <summary>
    /// Extends the selection to the first non-whitespace character of the line.
    /// </summary>
    VcHomeExtend = SCI_VCHOMEEXTEND,

    /// <summary>
    /// Extends the rectangular selection to the first non-whitespace character of the line.
    /// </summary>
    VcHomeRectExtend = SCI_VCHOMERECTEXTEND,

    // --------------------------------------------------------------------

    /// <summary>
    /// Moves the caret to the first non-whitespace character of the display or document line.
    /// </summary>
    VcHomeWrap = SCI_VCHOMEWRAP,

    /// <summary>
    /// Extends the selection to the first non-whitespace character of the display or document line.
    /// </summary>
    VcHomeWrapExtend = SCI_VCHOMEWRAPEXTEND,

    /// <summary>
    /// Moves the caret to the first non-whitespace character of the display line.
    /// </summary>
    VcHomeDisplay = SCI_VCHOMEDISPLAY,

    /// <summary>
    /// Extends the selection to the first non-whitespace character of the display line.
    /// </summary>
    VcHomeDisplayExtend = SCI_VCHOMEDISPLAYEXTEND,

    // --------------------------------------------------------------------

    /// <summary>
    /// Moves the caret to the end of the document line.
    /// </summary>
    LineEnd = SCI_LINEEND,

    /// <summary>
    /// Extends the selection to the end of the document line.
    /// </summary>
    LineEndExtend = SCI_LINEENDEXTEND,

    /// <summary>
    /// Extends the rectangular selection to the end of the document line.
    /// </summary>
    LineEndRectExtend = SCI_LINEENDRECTEXTEND,

    // --------------------------------------------------------------------

    /// <summary>
    /// Moves the caret to the end of the display line.
    /// </summary>
    LineEndDisplay = SCI_LINEENDDISPLAY,

    /// <summary>
    /// Extends the selection to the end of the display line.
    /// </summary>
    LineEndDisplayExtend = SCI_LINEENDDISPLAYEXTEND,

    /// <summary>
    /// Moves the caret to the end of the display or document line.
    /// </summary>
    LineEndWrap = SCI_LINEENDWRAP,

    /// <summary>
    /// Extends the selection to the end of the display or document line.
    /// </summary>
    LineEndWrapExtend = SCI_LINEENDWRAPEXTEND,

    // --------------------------------------------------------------------

    /// <summary>
    /// Moves the caret to the start of the document.
    /// </summary>
    DocumentStart = SCI_DOCUMENTSTART,

    /// <summary>
    /// Extends the selection to the start of the document.
    /// </summary>
    DocumentStartExtend = SCI_DOCUMENTSTARTEXTEND,

    /// <summary>
    /// Moves the caret to the end of the document.
    /// </summary>
    DocumentEnd = SCI_DOCUMENTEND,

    /// <summary>
    /// Extends the selection to the end of the document.
    /// </summary>
    DocumentEndExtend = SCI_DOCUMENTENDEXTEND,

    // --------------------------------------------------------------------

    /// <summary>
    /// Moves the caret up one page.
    /// </summary>
    PageUp = SCI_PAGEUP,

    /// <summary>
    /// Extends the selection up one page.
    /// </summary>
    PageUpExtend = SCI_PAGEUPEXTEND,

    /// <summary>
    /// Extends the rectangular selection up one page.
    /// </summary>
    PageUpRectExtend = SCI_PAGEUPRECTEXTEND,

    // --------------------------------------------------------------------

    /// <summary>
    /// Moves the caret down one page.
    /// </summary>
    PageDown = SCI_PAGEDOWN,

    /// <summary>
    /// Extends the selection down one page.
    /// </summary>
    PageDownExtend = SCI_PAGEDOWNEXTEND,

    /// <summary>
    /// Extends the rectangular selection down one page.
    /// </summary>
    PageDownRectExtend = SCI_PAGEDOWNRECTEXTEND,

    // --------------------------------------------------------------------

    /// <summary>
    /// Moves the caret up one window or page.
    /// </summary>
    StutteredPageUp = SCI_STUTTEREDPAGEUP,

    /// <summary>
    /// Extends the selection up one window or page.
    /// </summary>
    StutteredPageUpExtend = SCI_STUTTEREDPAGEUPEXTEND,

    // --------------------------------------------------------------------

    /// <summary>
    /// Moves the caret down one window or page.
    /// </summary>
    StutteredPageDown = SCI_STUTTEREDPAGEDOWN,

    /// <summary>
    /// Extends the selection down one window or page.
    /// </summary>
    StutteredPageDownExtend = SCI_STUTTEREDPAGEDOWNEXTEND,

    // --------------------------------------------------------------------

    /// <summary>
    /// Deletes the character left of the caret.
    /// </summary>
    DeleteBack = SCI_DELETEBACK,

    /// <summary>
    /// Deletes the character (excluding line breaks) left of the caret.
    /// </summary>
    DeleteBackNotLine = SCI_DELETEBACKNOTLINE,

    // --------------------------------------------------------------------

    /// <summary>
    /// Deletes from the caret to the start of the previous word.
    /// </summary>
    DelWordLeft = SCI_DELWORDLEFT,

    /// <summary>
    /// Deletes from the caret to the start of the next word.
    /// </summary>
    DelWordRight = SCI_DELWORDRIGHT,

    /// <summary>
    /// Deletes from the caret to the end of the next word.
    /// </summary>
    DelWordRightEnd = SCI_DELWORDRIGHTEND,

    // --------------------------------------------------------------------

    /// <summary>
    /// Deletes the characters left of the caret to the start of the line.
    /// </summary>
    DelLineLeft = SCI_DELLINELEFT,

    /// <summary>
    /// Deletes the characters right of the caret to the start of the line.
    /// </summary>
    DelLineRight = SCI_DELLINERIGHT,

    /// <summary>
    /// Deletes the current line.
    /// </summary>
    LineDelete = SCI_LINEDELETE,

    // --------------------------------------------------------------------

    /// <summary>
    /// Removes the current line and places it on the clipboard.
    /// </summary>
    LineCut = SCI_LINECUT,

    /// <summary>
    /// Copies the current line and places it on the clipboard.
    /// </summary>
    LineCopy = SCI_LINECOPY,

    /// <summary>
    /// Transposes the current and previous lines.
    /// </summary>
    LineTranspose = SCI_LINETRANSPOSE,

    /// <summary>
    /// Reverses the current line.
    /// </summary>
    LineReverse = SCI_LINEREVERSE,

    /// <summary>
    /// Duplicates the current line.
    /// </summary>
    LineDuplicate = SCI_LINEDUPLICATE,

    // --------------------------------------------------------------------

    /// <summary>
    /// Converts the selection to lowercase.
    /// </summary>
    Lowercase = SCI_LOWERCASE,

    /// <summary>
    /// Converts the selection to uppercase.
    /// </summary>
    Uppercase = SCI_UPPERCASE,

    /// <summary>
    /// Cancels autocompletion, calltip display, and drops any additional selections.
    /// </summary>
    Cancel = SCI_CANCEL,

    /// <summary>
    /// Toggles overtype. See <see cref="Scintilla.Overtype" />.
    /// </summary>
    EditToggleOvertype = SCI_EDITTOGGLEOVERTYPE,

    // --------------------------------------------------------------------

    /// <summary>
    /// Inserts a newline character.
    /// </summary>
    NewLine = SCI_NEWLINE,

    /// <summary>
    /// Inserts a form feed character.
    /// </summary>
    FormFeed = SCI_FORMFEED,

    /// <summary>
    /// Adds a tab (indent) character.
    /// </summary>
    Tab = SCI_TAB,

    /// <summary>
    /// Removes a tab (indent) character from the start of a line.
    /// </summary>
    BackTab = SCI_BACKTAB,

    // --------------------------------------------------------------------

    /// <summary>
    /// Duplicates the current selection.
    /// </summary>
    SelectionDuplicate = SCI_SELECTIONDUPLICATE,

    /// <summary>
    /// Moves the caret vertically to the center of the screen.
    /// </summary>
    VerticalCenterCaret = SCI_VERTICALCENTRECARET,

    // --------------------------------------------------------------------

    /// <summary>
    /// Moves the selected lines up.
    /// </summary>
    MoveSelectedLinesUp = SCI_MOVESELECTEDLINESUP,

    /// <summary>
    /// Moves the selected lines down.
    /// </summary>
    MoveSelectedLinesDown = SCI_MOVESELECTEDLINESDOWN,

    // --------------------------------------------------------------------

    /// <summary>
    /// Scrolls to the start of the document without changing the selection.
    /// </summary>
    ScrollToStart = SCI_SCROLLTOSTART,

    /// <summary>
    /// Scrolls to the end of the document without changing the selection.
    /// </summary>
    ScrollToEnd = SCI_SCROLLTOEND,

    // --------------------------------------------------------------------

    /// <summary>
    /// Command equivalent to <see cref="Scintilla.ZoomIn" />.
    /// </summary>
    ZoomIn = SCI_ZOOMIN,

    /// <summary>
    /// Command equivalent to <see cref="Scintilla.ZoomOut" />.
    /// </summary>
    ZoomOut = SCI_ZOOMOUT,

    /// <summary>
    /// Command equivalent to <see cref="Scintilla.Undo" />.
    /// </summary>
    Undo = SCI_UNDO,

    /// <summary>
    /// Command equivalent to <see cref="Scintilla.Redo" />.
    /// </summary>
    Redo = SCI_REDO,

    /// <summary>
    /// Command equivalent to <see cref="Scintilla.SwapMainAnchorCaret" />
    /// </summary>
    SwapMainAnchorCaret = SCI_SWAPMAINANCHORCARET,

    /// <summary>
    /// Command equivalent to <see cref="Scintilla.RotateSelection" />
    /// </summary>
    RotateSelection = SCI_ROTATESELECTION,

    /// <summary>
    /// Command equivalent to <see cref="Scintilla.MultipleSelectAddNext" />
    /// </summary>
    MultipleSelectAddNext = SCI_MULTIPLESELECTADDNEXT,

    /// <summary>
    /// Command equivalent to <see cref="Scintilla.MultipleSelectAddEach" />
    /// </summary>
    MultipleSelectAddEach = SCI_MULTIPLESELECTADDEACH,

    /// <summary>
    /// Command equivalent to <see cref="Scintilla.SelectAll" />
    /// </summary>
    SelectAll = SCI_SELECTALL
}