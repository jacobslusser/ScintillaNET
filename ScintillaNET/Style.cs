using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ScintillaNET
{
    /// <summary>
    /// A style definition in a <see cref="Scintilla" /> control.
    /// </summary>
    public class Style
    {
        #region Constants

        /// <summary>
        /// Default style index. This style is used to define properties that all styles receive when calling <see cref="Scintilla.StyleClearAll" />.
        /// </summary>
        public const int Default = NativeMethods.STYLE_DEFAULT;

        /// <summary>
        /// Line number style index. This style is used for text in line number margins. The background color of this style also
        /// sets the background color for all margins that do not have any folding mask set.
        /// </summary>
        public const int LineNumber = NativeMethods.STYLE_LINENUMBER;

        /// <summary>
        /// Call tip style index. Only font name, size, foreground color, background color, and character set attributes
        /// can be used when displaying a call tip.
        /// </summary>
        public const int CallTip = NativeMethods.STYLE_CALLTIP;

        /// <summary>
        /// Indent guide style index. This style is used to specify the foreground and background colors of <see cref="Scintilla.IndentationGuides" />.
        /// </summary>
        public const int IndentGuide = NativeMethods.STYLE_INDENTGUIDE;

        /// <summary>
        /// Brace highlighting style index. This style is used on a brace character when set with the <see cref="BraceHighlight" /> method
        /// or the indentation guide when used with the <see cref="HighlightGuide" /> property.
        /// </summary>
        public const int BraceLight = NativeMethods.STYLE_BRACELIGHT;

        /// <summary>
        /// Bad brace style index. This style is used on an unmatched brace character when set with the <see cref="BraceBadLight" /> method.
        /// </summary>
        public const int BraceBad = NativeMethods.STYLE_BRACEBAD;

        #endregion Constants

        #region Fields

        private readonly Scintilla scintilla;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets the background color of the style.
        /// </summary>
        /// <returns>A Color object representing the style background color. The default is White.</returns>
        /// <remarks>Alpha color values are ignored.</remarks>
        public Color BackColor
        {
            get
            {
                var color = scintilla.DirectMessage(NativeMethods.SCI_STYLEGETBACK, new IntPtr(Index), IntPtr.Zero).ToInt32();
                return ColorTranslator.FromWin32(color);
            }
            set
            {
                if (value.IsEmpty)
                    value = Color.White;

                var color = ColorTranslator.ToWin32(value);
                scintilla.DirectMessage(NativeMethods.SCI_STYLESETBACK, new IntPtr(Index), new IntPtr(color));
            }
        }

        /// <summary>
        /// Gets or sets whether the style font is bold.
        /// </summary>
        /// <returns>true if bold; otherwise, false. The default is false.</returns>
        /// <remarks>Setting this property affects the <see cref="Weight" /> property.</remarks>
        public bool Bold
        {
            get
            {
                return scintilla.DirectMessage(NativeMethods.SCI_STYLEGETBOLD, new IntPtr(Index), IntPtr.Zero) != IntPtr.Zero;
            }
            set
            {
                var bold = (value ? new IntPtr(1) : IntPtr.Zero);
                scintilla.DirectMessage(NativeMethods.SCI_STYLESETBOLD, new IntPtr(Index), bold);
            }
        }

        /// <summary>
        /// Gets or sets the casing used to display the styled text.
        /// </summary>
        /// <returns>One of the <see cref="StyleCase" /> enum values. The default is <see cref="StyleCase.Mixed" />.</returns>
        /// <remarks>This does not affect how text is stored, only displayed.</remarks>
        public StyleCase Case
        {
            get
            {
                var @case = scintilla.DirectMessage(NativeMethods.SCI_STYLEGETCASE, new IntPtr(Index), IntPtr.Zero).ToInt32();
                return (StyleCase)@case;
            }
            set
            {
                // Just an excuse to use @... syntax
                var @case = (int)value;
                scintilla.DirectMessage(NativeMethods.SCI_STYLESETCASE, new IntPtr(Index), new IntPtr(@case));
            }
        }

        /// <summary>
        /// Gets or sets whether the remainder of the line is filled with the <see cref="BackColor" />
        /// when this style is used on the last character of a line.
        /// </summary>
        /// <returns>true to fill the line; otherwise, false. The default is false.</returns>
        public bool FillLine
        {
            get
            {
                return scintilla.DirectMessage(NativeMethods.SCI_STYLEGETEOLFILLED, new IntPtr(Index), IntPtr.Zero) != IntPtr.Zero;
            }
            set
            {
                var fillLine = (value ? new IntPtr(1) : IntPtr.Zero);
                scintilla.DirectMessage(NativeMethods.SCI_STYLESETEOLFILLED, new IntPtr(Index), fillLine);
            }
        }

        /// <summary>
        /// Gets or sets the style font name.
        /// </summary>
        /// <returns>The style font name. The default is Verdana.</returns>
        /// <remarks>Scintilla caches fonts by name so font names and casing should be consistent.</remarks>
        public string Font
        {
            get
            {
                var length = scintilla.DirectMessage(NativeMethods.SCI_STYLEGETFONT, new IntPtr(Index), IntPtr.Zero).ToInt32();
                var font = new byte[length];
                unsafe
                {
                    fixed (byte* bp = font)
                        scintilla.DirectMessage(NativeMethods.SCI_STYLEGETFONT, new IntPtr(Index), new IntPtr(bp));
                }

                var name = Encoding.Default.GetString(font, 0, length);
                return name;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    value = "Verdana";

                // As best I can tell, Scintilla is using the LOGFONTA structure for loading
                // and saving fonts. Meaning we need to convert our font name to ANSI.
                var font = Helpers.GetBytes(value, Encoding.Default, true);
                unsafe
                {
                    fixed (byte* bp = font)
                        scintilla.DirectMessage(NativeMethods.SCI_STYLESETFONT, new IntPtr(Index), new IntPtr(bp));
                }
            }
        }

        /// <summary>
        /// Gets or sets the foreground color of the style.
        /// </summary>
        /// <returns>A Color object representing the style foreground color. The default is Black.</returns>
        /// <remarks>Alpha color values are ignored.</remarks>
        public Color ForeColor
        {
            get
            {
                var color = scintilla.DirectMessage(NativeMethods.SCI_STYLEGETFORE, new IntPtr(Index), IntPtr.Zero).ToInt32();
                return ColorTranslator.FromWin32(color);
            }
            set
            {
                if (value.IsEmpty)
                    value = Color.Black;

                var color = ColorTranslator.ToWin32(value);
                scintilla.DirectMessage(NativeMethods.SCI_STYLESETFORE, new IntPtr(Index), new IntPtr(color));
            }
        }

        /// <summary>
        /// Gets or sets whether hovering the mouse over the style text exhibits hyperlink behavior.
        /// </summary>
        /// <returns>true to use hyperlink behavior; otherwise, false. The default is false.</returns>
        public bool Hotspot
        {
            get
            {
                return scintilla.DirectMessage(NativeMethods.SCI_STYLEGETHOTSPOT, new IntPtr(Index), IntPtr.Zero) != IntPtr.Zero;
            }
            set
            {
                var hotspot = (value ? new IntPtr(1) : IntPtr.Zero);
                scintilla.DirectMessage(NativeMethods.SCI_STYLESETHOTSPOT, new IntPtr(Index), hotspot);
            }
        }

        /// <summary>
        /// Gets the zero-based style definition index.
        /// </summary>
        /// <returns>The style definition index within the <see cref="StyleCollection" />.</returns>
        public int Index { get; private set; }

        /// <summary>
        /// Gets or sets whether the style font is italic.
        /// </summary>
        /// <returns>true if italic; otherwise, false. The default is false.</returns>
        public bool Italic
        {
            get
            {
                return scintilla.DirectMessage(NativeMethods.SCI_STYLEGETITALIC, new IntPtr(Index), IntPtr.Zero) != IntPtr.Zero;
            }
            set
            {
                var italic = (value ? new IntPtr(1) : IntPtr.Zero);
                scintilla.DirectMessage(NativeMethods.SCI_STYLESETITALIC, new IntPtr(Index), italic);
            }
        }

        /// <summary>
        /// Gets or sets the size of the style font in points.
        /// </summary>
        /// <returns>The size of the style font as a whole number of points. The default is 8.</returns>
        public int Size
        {
            get
            {
                return scintilla.DirectMessage(NativeMethods.SCI_STYLEGETSIZE, new IntPtr(Index), IntPtr.Zero).ToInt32();
            }
            set
            {
                scintilla.DirectMessage(NativeMethods.SCI_STYLESETSIZE, new IntPtr(Index), new IntPtr(value));
            }
        }

        /// <summary>
        /// Gets or sets the size of the style font in fractoinal points.
        /// </summary>
        /// <returns>The size of the style font in fractional number of points. The default is 8.</returns>
        public float SizeF
        {
            get
            {
                var fraction = scintilla.DirectMessage(NativeMethods.SCI_STYLEGETSIZEFRACTIONAL, new IntPtr(Index), IntPtr.Zero).ToInt32();
                return (float)fraction / NativeMethods.SC_FONT_SIZE_MULTIPLIER;
            }
            set
            {
                var fraction = (int)(value * NativeMethods.SC_FONT_SIZE_MULTIPLIER);
                scintilla.DirectMessage(NativeMethods.SCI_STYLESETSIZEFRACTIONAL, new IntPtr(Index), new IntPtr(fraction));
            }
        }

        /// <summary>
        /// Gets or sets whether the style is underlined.
        /// </summary>
        /// <returns>true if underlined; otherwise, false. The default is false.</returns>
        public bool Underline
        {
            get
            {
                return scintilla.DirectMessage(NativeMethods.SCI_STYLEGETUNDERLINE, new IntPtr(Index), IntPtr.Zero) != IntPtr.Zero;
            }
            set
            {
                var underline = (value ? new IntPtr(1) : IntPtr.Zero);
                scintilla.DirectMessage(NativeMethods.SCI_STYLESETUNDERLINE, new IntPtr(Index), underline);
            }
        }

        /// <summary>
        /// Gets or sets whether the style text is visible.
        /// </summary>
        /// <returns>true to display the style text; otherwise, false. The default is true.</returns>
        public bool Visible
        {
            get
            {
                return scintilla.DirectMessage(NativeMethods.SCI_STYLEGETVISIBLE, new IntPtr(Index), IntPtr.Zero) != IntPtr.Zero;
            }
            set
            {
                var visible = (value ? new IntPtr(1) : IntPtr.Zero);
                scintilla.DirectMessage(NativeMethods.SCI_STYLESETVISIBLE, new IntPtr(Index), visible);
            }
        }

        /// <summary>
        /// Gets or sets the style font weight.
        /// </summary>
        /// <returns>The font weight. The default is 400.</returns>
        /// <remarks>Setting this property affects the <see cref="Bold" /> property.</remarks>
        public int Weight
        {
            get
            {
                return scintilla.DirectMessage(NativeMethods.SCI_STYLEGETWEIGHT, new IntPtr(Index), IntPtr.Zero).ToInt32();
            }
            set
            {
                scintilla.DirectMessage(NativeMethods.SCI_STYLESETWEIGHT, new IntPtr(Index), new IntPtr(value));
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instances of the <see cref="Style" /> class.
        /// </summary>
        /// <param name="scintilla">The <see cref="Scintilla" /> control that created this style.</param>
        /// <param name="index">The index of this style within the <see cref="StyleCollection" /> that created it.</param>
        public Style(Scintilla scintilla, int index)
        {
            this.scintilla = scintilla;
            Index = index;
        }

        #endregion Constructors

        #region Python

        /// <summary>
        /// Style constants for use with the <see cref="Lexer.Python" /> lexer.
        /// </summary>
        public static class Python
        {
            /// <summary>
            /// Default (whitespace) style index.
            /// </summary>
            public const int Default = NativeMethods.SCE_P_DEFAULT;

            /// <summary>
            /// Line comment style index.
            /// </summary>
            public const int CommentLine = NativeMethods.SCE_P_COMMENTLINE;

            /// <summary>
            /// Number style index.
            /// </summary>
            public const int Number = NativeMethods.SCE_P_NUMBER;

            /// <summary>
            /// String style index.
            /// </summary>
            public const int String = NativeMethods.SCE_P_STRING;

            /// <summary>
            /// Single-quote style index.
            /// </summary>
            public const int Character = NativeMethods.SCE_P_CHARACTER;

            /// <summary>
            /// Keyword style index.
            /// </summary>
            public const int Word = NativeMethods.SCE_P_WORD;

            /// <summary>
            /// Triple single-quote style index.
            /// </summary>
            public const int Triple = NativeMethods.SCE_P_TRIPLE;

            /// <summary>
            /// Triple double-quote style index.
            /// </summary>
            public const int TripleDouble = NativeMethods.SCE_P_TRIPLEDOUBLE;

            /// <summary>
            /// Class name style index.
            /// </summary>
            public const int ClassName = NativeMethods.SCE_P_CLASSNAME;

            /// <summary>
            /// Function or method name style index.
            /// </summary>
            public const int DefName = NativeMethods.SCE_P_DEFNAME;

            /// <summary>
            /// Operator style index.
            /// </summary>
            public const int Operator = NativeMethods.SCE_P_OPERATOR;

            /// <summary>
            /// Identifier style index.
            /// </summary>
            public const int Identifier = NativeMethods.SCE_P_IDENTIFIER;

            /// <summary>
            /// Block comment style index.
            /// </summary>
            public const int CommentBlock = NativeMethods.SCE_P_COMMENTBLOCK;

            /// <summary>
            /// Unclosed string EOL style index.
            /// </summary>
            public const int StringEol = NativeMethods.SCE_P_STRINGEOL;

            /// <summary>
            /// Keyword style 2 index.
            /// </summary>
            public const int Word2 = NativeMethods.SCE_P_WORD2;

            /// <summary>
            /// Decorator style index.
            /// </summary>
            public const int Decorator = NativeMethods.SCE_P_DECORATOR;
        }

        #endregion Python

        #region Cpp

        /// <summary>
        /// Style constants for use with the <see cref="Lexer.Cpp" /> lexer.
        /// </summary>
        public static class Cpp
        {
            /// <summary>
            /// Default (whitespace) style index.
            /// </summary>
            public const int Default = NativeMethods.SCE_C_DEFAULT;

            /// <summary>
            /// Comment style index.
            /// </summary>
            public const int Comment = NativeMethods.SCE_C_COMMENT;

            /// <summary>
            /// Line comment style index.
            /// </summary>
            public const int CommentLine = NativeMethods.SCE_C_COMMENTLINE;

            /// <summary>
            /// Documentation comment style index.
            /// </summary>
            public const int CommentDoc = NativeMethods.SCE_C_COMMENTDOC;

            /// <summary>
            /// Number style index.
            /// </summary>
            public const int Number = NativeMethods.SCE_C_NUMBER;

            /// <summary>
            /// Keyword style index.
            /// </summary>
            public const int Word = NativeMethods.SCE_C_WORD;

            /// <summary>
            /// Double-quoted string style index.
            /// </summary>
            public const int String = NativeMethods.SCE_C_STRING;

            /// <summary>
            /// Single-quoted string style index.
            /// </summary>
            public const int Character = NativeMethods.SCE_C_CHARACTER;

            /// <summary>
            /// UUID style index.
            /// </summary>
            public const int Uuid = NativeMethods.SCE_C_UUID;

            /// <summary>
            /// Preprocessor style index.
            /// </summary>
            public const int Preprocessor = NativeMethods.SCE_C_PREPROCESSOR;

            /// <summary>
            /// Operator style index.
            /// </summary>
            public const int Operator = NativeMethods.SCE_C_OPERATOR;

            /// <summary>
            /// Identifier style index.
            /// </summary>
            public const int Identifier = NativeMethods.SCE_C_IDENTIFIER;

            /// <summary>
            /// Unclosed string EOL style index.
            /// </summary>
            public const int StringEol = NativeMethods.SCE_C_STRINGEOL;

            /// <summary>
            /// Verbatim string style index.
            /// </summary>
            public const int Verbatim = NativeMethods.SCE_C_VERBATIM;

            /// <summary>
            /// Regular expression style index.
            /// </summary>
            public const int Regex = NativeMethods.SCE_C_REGEX;

            /// <summary>
            /// Documentation comment line style index.
            /// </summary>
            public const int CommentLineDoc = NativeMethods.SCE_C_COMMENTLINEDOC;

            /// <summary>
            /// Keyword style 2 index.
            /// </summary>
            public const int Word2 = NativeMethods.SCE_C_WORD2;

            /// <summary>
            /// Comment keyword style index.
            /// </summary>
            public const int CommentDocKeyword = NativeMethods.SCE_C_COMMENTDOCKEYWORD;

            /// <summary>
            /// Comment keyword error style index.
            /// </summary>
            public const int CommentDocKeywordError = NativeMethods.SCE_C_COMMENTDOCKEYWORDERROR;

            /// <summary>
            /// Global class style index.
            /// </summary>
            public const int GlobalClass = NativeMethods.SCE_C_GLOBALCLASS;

            /// <summary>
            /// Raw string style index.
            /// </summary>
            public const int StringRaw = NativeMethods.SCE_C_STRINGRAW;

            /// <summary>
            /// Triple-quoted string style index.
            /// </summary>
            public const int TripleVerbatim = NativeMethods.SCE_C_TRIPLEVERBATIM;

            /// <summary>
            /// Hash-quoted string style index.
            /// </summary>
            public const int HashQuotedString = NativeMethods.SCE_C_HASHQUOTEDSTRING;

            /// <summary>
            /// Preprocessor comment style index.
            /// </summary>
            public const int PreprocessorComment = NativeMethods.SCE_C_PREPROCESSORCOMMENT;

            /// <summary>
            /// Preprocessor documentation comment style index.
            /// </summary>
            public const int PreprocessorCommentDoc = NativeMethods.SCE_C_PREPROCESSORCOMMENTDOC;

            /// <summary>
            /// User-defined literal style index.
            /// </summary>
            public const int UserLiteral = NativeMethods.SCE_C_USERLITERAL;

            /// <summary>
            /// Task marker style index.
            /// </summary>
            public const int TaskMarker = NativeMethods.SCE_C_TASKMARKER;

            /// <summary>
            /// Escape sequence style index.
            /// </summary>
            public const int EscapeSequence = NativeMethods.SCE_C_ESCAPESEQUENCE;
        }

        #endregion Cpp

        #region Css

        /// <summary>
        /// Style constants for use with the <see cref="Lexer.Css" /> lexer.
        /// </summary>
        public static class Css
        {
            /// <summary>
            /// Default (whitespace) style index.
            /// </summary>
            public const int Default = NativeMethods.SCE_CSS_DEFAULT;

            /// <summary>
            /// Tag style index.
            /// </summary>
            public const int Tag = NativeMethods.SCE_CSS_TAG;

            /// <summary>
            /// Class style index.
            /// </summary>
            public const int Class = NativeMethods.SCE_CSS_CLASS;

            /// <summary>
            /// Pseudo class style index.
            /// </summary>
            public const int PseudoClass = NativeMethods.SCE_CSS_PSEUDOCLASS;

            /// <summary>
            /// Unknown pseudo class style index.
            /// </summary>
            public const int UnknownPseudoClass = NativeMethods.SCE_CSS_UNKNOWN_PSEUDOCLASS;

            /// <summary>
            /// Operator style index.
            /// </summary>
            public const int Operator = NativeMethods.SCE_CSS_OPERATOR;

            /// <summary>
            /// Identifier style index.
            /// </summary>
            public const int Identifier = NativeMethods.SCE_CSS_IDENTIFIER;

            /// <summary>
            /// Unknown identifier style index.
            /// </summary>
            public const int UnknownIdentifier = NativeMethods.SCE_CSS_UNKNOWN_IDENTIFIER;

            /// <summary>
            /// Value style index.
            /// </summary>
            public const int Value = NativeMethods.SCE_CSS_VALUE;

            /// <summary>
            /// Comment style index.
            /// </summary>
            public const int Comment = NativeMethods.SCE_CSS_COMMENT;

            /// <summary>
            /// ID style index.
            /// </summary>
            public const int Id = NativeMethods.SCE_CSS_ID;

            /// <summary>
            /// Important style index.
            /// </summary>
            public const int Important = NativeMethods.SCE_CSS_IMPORTANT;

            /// <summary>
            /// Directive style index.
            /// </summary>
            public const int Directive = NativeMethods.SCE_CSS_DIRECTIVE;

            /// <summary>
            /// Double-quoted string style index.
            /// </summary>
            public const int DoubleString = NativeMethods.SCE_CSS_DOUBLESTRING;

            /// <summary>
            /// Single-quoted string style index.
            /// </summary>
            public const int SingleString = NativeMethods.SCE_CSS_SINGLESTRING;

            /// <summary>
            /// Identifier style 2 index.
            /// </summary>
            public const int Identifier2 = NativeMethods.SCE_CSS_IDENTIFIER2;

            /// <summary>
            /// Attribute style index.
            /// </summary>
            public const int Attribute = NativeMethods.SCE_CSS_ATTRIBUTE;

            /// <summary>
            /// Identifier style 3 index.
            /// </summary>
            public const int Identifier3 = NativeMethods.SCE_CSS_IDENTIFIER3;

            /// <summary>
            /// Pseudo element style index.
            /// </summary>
            public const int PseudoElement = NativeMethods.SCE_CSS_PSEUDOELEMENT;

            /// <summary>
            /// Extended identifier style index.
            /// </summary>
            public const int ExtendedIdentifier = NativeMethods.SCE_CSS_EXTENDED_IDENTIFIER;

            /// <summary>
            /// Extended pseudo class style index.
            /// </summary>
            public const int ExtendedPseudoClass = NativeMethods.SCE_CSS_EXTENDED_PSEUDOCLASS;

            /// <summary>
            /// Extended pseudo element style index.
            /// </summary>
            public const int ExtendedPseudoElement = NativeMethods.SCE_CSS_EXTENDED_PSEUDOELEMENT;

            /// <summary>
            /// Media style index.
            /// </summary>
            public const int Media = NativeMethods.SCE_CSS_MEDIA;

            /// <summary>
            /// Variable style index.
            /// </summary>
            public const int Variable = NativeMethods.SCE_CSS_VARIABLE;
        }

        #endregion Css

        #region Basic

        /// <summary>
        /// Style constants for use with the <see cref="Lexer.BlitzBasic" /> lexer.
        /// </summary>
        public static class Basic
        {
            /// <summary>
            /// Default (whitespace) style index.
            /// </summary>
            public const int Default = NativeMethods.SCE_CSS_DEFAULT;

            /// <summary>
            /// Comment style index.
            /// </summary>
            public const int Comment = NativeMethods.SCE_B_COMMENT;

            /// <summary>
            /// Number style index.
            /// </summary>
            public const int Number = NativeMethods.SCE_B_NUMBER;

            /// <summary>
            /// Keyword style index.
            /// </summary>
            public const int Keyword = NativeMethods.SCE_B_KEYWORD;

            /// <summary>
            /// String style index.
            /// </summary>
            public const int String = NativeMethods.SCE_B_STRING;

            /// <summary>
            /// Preprocessor style index.
            /// </summary>
            public const int Preprocessor = NativeMethods.SCE_B_PREPROCESSOR;

            /// <summary>
            /// Operator style index.
            /// </summary>
            public const int Operator = NativeMethods.SCE_B_OPERATOR;

            /// <summary>
            /// Identifier style index.
            /// </summary>
            public const int Identifier = NativeMethods.SCE_B_IDENTIFIER;

            /// <summary>
            /// Date style index.
            /// </summary>
            public const int Date = NativeMethods.SCE_B_DATE;

            /// <summary>
            /// Unclosed string EOL style index.
            /// </summary>
            public const int StringEol = NativeMethods.SCE_B_STRINGEOL;

            /// <summary>
            /// Keyword list 2 style index.
            /// </summary>
            public const int Keyword2 = NativeMethods.SCE_B_KEYWORD2;

            /// <summary>
            /// Keyword list 3 style index.
            /// </summary>
            public const int Keyword3 = NativeMethods.SCE_B_KEYWORD3;

            /// <summary>
            /// Keyword list 4 style index.
            /// </summary>
            public const int Keyword4 = NativeMethods.SCE_B_KEYWORD4;

            /// <summary>
            /// Constant style index.
            /// </summary>
            public const int Constant = NativeMethods.SCE_B_CONSTANT;

            /// <summary>
            /// Inline assembler style index.
            /// </summary>
            public const int Asm = NativeMethods.SCE_B_ASM;

            /// <summary>
            /// Label style index.
            /// </summary>
            public const int Label = NativeMethods.SCE_B_LABEL;

            /// <summary>
            /// Error style index.
            /// </summary>
            public const int Error = NativeMethods.SCE_B_ERROR;

            /// <summary>
            /// Hexadecimal number style index.
            /// </summary>
            public const int HexNumber = NativeMethods.SCE_B_HEXNUMBER;

            /// <summary>
            /// Binary number style index.
            /// </summary>
            public const int BinNumber = NativeMethods.SCE_B_BINNUMBER;

            /// <summary>
            /// Block comment style index.
            /// </summary>
            public const int CommentBlock = NativeMethods.SCE_B_COMMENTBLOCK;

            /// <summary>
            /// Documentation line style index.
            /// </summary>
            public const int DocLine = NativeMethods.SCE_B_DOCLINE;

            /// <summary>
            /// Documentation block style index.
            /// </summary>
            public const int DocBlock = NativeMethods.SCE_B_DOCBLOCK;

            /// <summary>
            /// Documentation keyword style index.
            /// </summary>
            public const int DocKeyword = NativeMethods.SCE_B_DOCKEYWORD;
        }

        #endregion Basic
    }
}
