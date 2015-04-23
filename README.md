# ScintillaNET

ScintillaNET is a Windows Forms control, wrapper, and bindings for the versatile [Scintilla](http://www.scintilla.org/) source code editing component.

> "As well as features found in standard text editing components, Scintilla includes features especially useful when editing and debugging source code. These include support for syntax styling, error indicators, code completion and call tips. The selection margin can contain markers like those used in debuggers to indicate breakpoints and the current line. Styling choices are more open than with many editors, allowing the use of proportional fonts, bold and italics, multiple foreground and background colours and multiple fonts." — [scintilla.org](http://www.scintilla.org/)

ScintillaNET can also be used with WPF using the <a href="https://msdn.microsoft.com/en-us/library/ms751761.aspx">WindowsFormsHost</a>.

### Project Status

ScintillaNET is in active development and nearing 100% API coverage with the core Scintilla component. Beta testers are welcome.

## Background

This project is a rewrite of the [ScintillaNET project hosted at CodePlex](http://scintillanet.codeplex.com/) and maintained by myself and others. After many years of contributing to that project I decided to think differently about the API we had created and felt I could make better one if I was willing to go back to a blank canvas. Thus, this project is the spiritual successor to the original ScintillaNET but has been written from scratch.

### First Class Characters

One of the issues that ScintillaNET has historically suffered from is the fact that the native Scintilla control operates on bytes, not characters. Prior versions of ScintillaNET did not account for this, and when you're dealing with Unicode, [one byte doesn't always equal one character](http://www.joelonsoftware.com/articles/Unicode.html). The result was an API that sometimes expected byte offsets and at other times expected character offsets. Sometimes things would work as expected and other times random failures and out-of-range exceptions would occur.

No more. **One of the major focuses of this rewrite was to give ScintillaNET an understanding of Unicode from the ground up.** Every API now consistently works with character-based offsets and ranges just like .NET developers expect. Internally we maintain a mapping of character to byte offsets (and vice versa) and do all the translation for you so you never need to worry about it. No more out-of-range exceptions. No more confusion. No more pain. [It just works](http://en.wikipedia.org/wiki/List_of_Apple_Inc._slogans).

### One Library

The second most popular ScintillaNET issue was confusion distributing the ScintillaNET DLL and its native component, the SciLexer DLL. ScintillaNET is a wrapper. Without the SciLexer.dll containing the core Scintilla functionality it is nothing. As a native component, SciLexer.dll has to be compiled separately for 32 and 64-bit versions of Windows. So it was actually three DLLs that developers had to ship with their applications.

This proved a pain point because developers often didn't want to distribute so many libraries or wanted to place them in alternate locations which would break the DLL loading mechanisms used by PInvoke and ScintillaNET. It also causes headaches during design-time in Visual Studio for the same reasons.

To address this ScintillaNET now embeds a 32 and 64-bit version of SciLexer.dll in the ScintillaNET DLL. **Everything you need to run ScintillaNET in one library.** In addition to soothing the pain mentioned above this now makes it possible for us to create a ScintillaNET NuGet package.

### Keeping it Consistent

Another goal of the rewrite was to accept the original Scintilla API for what it is and not try to coerce it into a .NET-style API when it should not or could not be. A good example of this is how ScintillaNET uses indexers to access lines, but not treat them as a .NET collection. Lines in a Scintilla control are not items in a collection. There is no API to Add, Insert, or Remove a line in Scintilla and thus we don't try to create one in ScintillaNET. These deviations from .NET convention are rare, but are done to keep any native Scintilla documentation relevant to the managed wrapper and to avoid situations where trying to force the original API into a more familiar one is more detrimental than helpful.

*NOTE: This is not to say that ScintillaNET cannot add, insert, or remove lines. Those operations, however, are handled as text changes, not line changes.*

## Conventions

ScintillaNET is fully documented, but since there has been such an effort made to keep the ScintillaNET API consist with the native Scintilla API we encourage you to continue using [their documentation](http://www.scintilla.org/scintilladoc.html) to fill in any gaps you find in ours.

Generally speaking, their API will map to ours in the following ways:

+ A call that has an associated 'get' and 'set' such as `SCI_GETTEXT` and `SCI_SETTEXT(value)`, will map to a similarly named property such as `Text`.
+ A call that requires a number argument to access an item in a 'collection' such as `SCI_INDICSETFORE(indicatorNumber, ...)` or `SCI_STYLEGETSIZE(styleNumber, ...)`, will be accessed through an indexer such as `Indicators[0].ForeColor` or `Styles[0].Size`.

The native Scintilla control has a habit of clamping input values to within acceptable ranges rather than throwing exceptions and so we've kept that behavior in ScintillaNET. For example, the `GotoPosition` method requires a character `position` argument. If that value is less than zero or past the end of the document it will be clamped to either `0` or the `TextLength` rather than throw an `OutOfRangeException`. This tends to result in less exceptions, but the same desired outcome.

## Recipes

1. [Basic Text Retrieval and Modification](#basic-text)
  1. [Retrieving Text](#retrieve-text)
  2. [Insert, Append, Delete](#modify-text)
2. [Syntax Highlighting](#syntax-highlighting)
  1. [Selecting a Lexer](#lexer)
  2. [Defining Styles](#styles)
  3. [Setting Keywords](#keywords)
  4. [Complete Recipe](#syntax-highlighting-recipe)
3. [Automatic Code Folding](#auto-folding)
4. [Basic Autocompletion](#autocompletion)
5. [Brace Matching](#brace-matching)
6. [Intercepting Inserted Text](#insert-check)
7. [Displaying Line Numbers](#line-numbers)
  1. [Custom Line Numbers](#custom-line-numbers)
8. [Zooming](#zooming)
9. [Updating Dependent Controls](#update-ui)
10. [Find and Highlight Words](#find-highlight)
11. [View Whitespace](#whitespace)
12. [Increase Line Spacing](#line-spacing)
13. [Rectangular/Multiple Selections](#multiple-selections)
14. [Bookmark Lines](#bookmarks)
15. [Key Bindings](#key-bindings)
16. [Documents](#documents)
  1. [Understanding Document Reference Counts](#reference-counting)
  2. [Multiple Views of one Document](#multiple-views)
  3. [Multiple Documents for one View](#multiple-documents)
  3. [Background Loading](#loader)
17. [Using a Custom SciLexer.dll](#scilexer)
18. [Direct Messages](#direct-message) 

### <a name="basic-text"></a>Basic Text Retrieval and Modification

At its most basic level ScintillaNET is a text editor. The following recipes demonstrate basic text I/O.

#### <a name="retrieve-text"></a>Retrieving Text

The `Text` property is the obvious choice if you want to get a string that represents all the text currently in a Scintilla control. Internally the Scintilla control will copy the entire contents of the document into a new string. Depending on the amount of text in the editor, this could be an expensive operation. Scintilla is designed to work efficiently with large amounts of text, but using the `Text` property to retrieve only a substring negates that.

Instead, it's usually best to identify the range of text you're interested in (through search, selection, or some other means) and use the `GetTextRange` method to copy only what you need into a string:

```cs
// Get the first 256 characters of the document
var text = scintilla.GetTextRange(0, Math.Min(256, scintilla.TextLength));
Console.WriteLine(text);
```

#### <a name="modify-text"></a>Insert, Append, and Delete

Modifications usually come in the form of insert, append, and delete operations. As was discussed above, using the `Text` property to make a small change in the document contents is highly inefficient. Instead try one of the following options:

```cs
scintilla.Text = "Hello";
scintilla.AppendText(" World"); // 'Hello' -> 'Hello World'
scintilla.DeleteRange(0, 5); // 'Hello World' -> ' World'
scintilla.InsertText(0, "Goodbye"); // ' World' -> 'Goodbye World'
```

*NOTE: It may help to think of a Scintilla control as a `StringBuilder`.*

### <a name="syntax-highlighting"></a>Syntax Highlighting

Far and away the most popular use for Scintilla is to display and edit source code. Out-of-the-box, Scintilla comes with syntax highlighting support for over 100 different languages. Chances are that the language you want to edit is already supported.

#### <a name="lexer"></a>Selecting a Lexer

A language processor is referred to as a 'lexer' in Scintilla. Without going too much into parser theory, it's important to know that a lexer performs [lexical analysis](http://en.wikipedia.org/wiki/Lexical_analysis) of a language, not [syntatic analysis (parsing)](http://en.wikipedia.org/wiki/Parsing). In short, this means that the language support provided by Scintilla is enough to break the text into tokens and provide syntax highlighting, but not interpret what those tokens mean or whether they form an actual program. The distinction is important because developers using Scintilla often want to know how they can highlight incorrect code. Scintilla doesn't do that. If you want more than basic syntax highlighting, you'll need to couple Scintilla with a parser or even a background compiler.

To inform Scintilla what the current language is you must set the `Lexer` property to the appropriate enum value. In some cases multiple languages share the same `Lexer` enumeration value because these language share the same lexical grammar. For example, the `Cpp` lexer not only provides language support for C++, but all for C, C#, Java, JavaScript and others—because they are lexically similar. In our example we want to do C# syntax highlighting so we'll use the `Cpp` lexer.

```cs
scintilla.Lexer = Lexer.Cpp;
```

#### <a name="styles"></a>Defining Styles

The process of doing syntax highlighting in Scintilla is referred to as styling. When the text is styled, runs of text are assigned a numeric style definition in the `Styles` collection. For example, keywords may be assigned the style definition `1`, while operators may be assigned the definition `2`. It's entire up to the lexer how this is done. Once done, however, you are then free to determine what style `1` or `2` look like. The lexer assigns the styles, but you define the style appearance. To make it easier to know which styles definitions a lexer will use, the `Style` object contains static constants that coincide with each `Lexer` enumeration value. For example, if we were using the `Cpp` lexer and wanted to set the style for single-line comments (//...) we would use the `Style.Cpp.CommentLine` constant to set the appropriate style in the `Styles` collection:

```cs
scintilla.Styles[Style.Cpp.CommentLine].Font = "Consolas";
scintilla.Styles[Style.Cpp.CommentLine].Size = 10;
scintilla.Styles[Style.Cpp.CommentLine].ForeColor = Color.FromArgb(0, 128, 0); // Green
```

To set the string style we would:

```cs
scintilla.Styles[Style.Cpp.String].Font = "Consolas";
scintilla.Styles[Style.Cpp.String].Size = 10;
scintilla.Styles[Style.Cpp.String].ForeColor = Color.FromArgb(163, 21, 21); // Red
```

To set the styles for number tokens we would do the same thing using the `Style.Cpp.Number` constant. For operators, we would use `Style.Cpp.Operator`, and so on.

If you use your imagination you will begin to see how doing this for each possible lexer token could be tedious. There is a lot of repetition. To reduce the amount of code you have to write Scintilla provides a way of setting a single style and then applying its appearance to every style in the collection. The general process is to:

* Reset the `Default` style using `StyleResetDefault`.
* Configure the `Default` style with all common properties.
* Use the `StyleClearAll` method to apply the `Default` style to all styles.
* Set any individual style properties

Using that time saving approach, we can set the appearance of our C# lexer styles like so:

```cs
// Configuring the default style with properties
// we have common to every lexer style saves time.
scintilla.StyleResetDefault();
scintilla.Styles[Style.Default].Font = "Consolas";
scintilla.Styles[Style.Default].Size = 10;
scintilla.StyleClearAll();

// Configure the CPP (C#) lexer styles
scintilla.Styles[Style.Cpp.Default].ForeColor = Color.Silver;
scintilla.Styles[Style.Cpp.Comment].ForeColor = Color.FromArgb(0, 128, 0); // Green
scintilla.Styles[Style.Cpp.CommentLine].ForeColor = Color.FromArgb(0, 128, 0); // Green
scintilla.Styles[Style.Cpp.CommentLineDoc].ForeColor = Color.FromArgb(128, 128, 128); // Gray
scintilla.Styles[Style.Cpp.Number].ForeColor = Color.Olive;
scintilla.Styles[Style.Cpp.Word].ForeColor = Color.Blue;
scintilla.Styles[Style.Cpp.Word2].ForeColor = Color.Blue;
scintilla.Styles[Style.Cpp.String].ForeColor = Color.FromArgb(163, 21, 21); // Red
scintilla.Styles[Style.Cpp.Character].ForeColor = Color.FromArgb(163, 21, 21); // Red
scintilla.Styles[Style.Cpp.Verbatim].ForeColor = Color.FromArgb(163, 21, 21); // Red
scintilla.Styles[Style.Cpp.StringEol].BackColor = Color.Pink;
scintilla.Styles[Style.Cpp.Operator].ForeColor = Color.Purple;
scintilla.Styles[Style.Cpp.Preprocessor].ForeColor = Color.Maroon;
```

#### <a name="keywords"></a>Setting Keywords

The last thing we need to do to provide syntax highlighting is to inform the lexer what the language keywords and identifiers are. Since languages can often add keywords year after year, or because a lexer may sometimes be used for more than one language, it makes sense to make the keyword list configurable.

Since each Scintilla lexer is like a program until itself the number of keyword sets and the definition of each one vary from lexer to lexer. To determine what keyword sets a lexer supports you can call the `DescribeKeywordSets` method. This prints a human readable explanation of how many sets the current `Lexer` supports and what each means:

```cs
scintilla.Lexer = Lexer.Cpp;
Console.WriteLine(scintilla.DescribeKeywordSets());

// Outputs:
// Primary keywords and identifiers
// Secondary keywords and identifiers
// Documentation comment keywords
// Global classes and typedefs
// Preprocessor definitions
// Task marker and error marker keywords
```

Based on the output of `DescribeKeywordSets` I can determine that the first two sets are what I'm interested in for supporting general purpose C# syntax highlighting. To set a set of keywords you call the `SetKeywords` method. What 'primary' and 'secondary' means in the keyword set description is up to a bit of interpretation, but I'll break it down so that primary keywords are C# language keywords and secondary keywords are known .NET types. To set those I would call:

```cs
scintilla.SetKeywords(0, "abstract as base break case catch checked continue default delegate do else event explicit extern false finally fixed for foreach goto if implicit in interface internal is lock namespace new null object operator out override params private protected public readonly ref return sealed sizeof stackalloc switch this throw true try typeof unchecked unsafe using virtual while");
scintilla.SetKeywords(1, "bool byte char class const decimal double enum float int long sbyte short static string struct uint ulong ushort void");
```

*NOTE: Keywords in a keyword set can be separated by any combination of whitespace (space, tab, '\r', '\n') characters.*

#### <a name="syntax-highlighting-recipe"></a>Complete Recipe

The complete recipe below will give you C# syntax highlighting using colors roughly equivalent to the Visual Studio defaults.

```cs
// Configuring the default style with properties
// we have common to every lexer style saves time.
scintilla.StyleResetDefault();
scintilla.Styles[Style.Default].Font = "Consolas";
scintilla.Styles[Style.Default].Size = 10;
scintilla.StyleClearAll();

// Configure the CPP (C#) lexer styles
scintilla.Styles[Style.Cpp.Default].ForeColor = Color.Silver;
scintilla.Styles[Style.Cpp.Comment].ForeColor = Color.FromArgb(0, 128, 0); // Green
scintilla.Styles[Style.Cpp.CommentLine].ForeColor = Color.FromArgb(0, 128, 0); // Green
scintilla.Styles[Style.Cpp.CommentLineDoc].ForeColor = Color.FromArgb(128, 128, 128); // Gray
scintilla.Styles[Style.Cpp.Number].ForeColor = Color.Olive;
scintilla.Styles[Style.Cpp.Word].ForeColor = Color.Blue;
scintilla.Styles[Style.Cpp.Word2].ForeColor = Color.Blue;
scintilla.Styles[Style.Cpp.String].ForeColor = Color.FromArgb(163, 21, 21); // Red
scintilla.Styles[Style.Cpp.Character].ForeColor = Color.FromArgb(163, 21, 21); // Red
scintilla.Styles[Style.Cpp.Verbatim].ForeColor = Color.FromArgb(163, 21, 21); // Red
scintilla.Styles[Style.Cpp.StringEol].BackColor = Color.Pink;
scintilla.Styles[Style.Cpp.Operator].ForeColor = Color.Purple;
scintilla.Styles[Style.Cpp.Preprocessor].ForeColor = Color.Maroon;
scintilla.Lexer = Lexer.Cpp;

// Set the keywords
scintilla.SetKeywords(0, "abstract as base break case catch checked continue default delegate do else event explicit extern false finally fixed for foreach goto if implicit in interface internal is lock namespace new null object operator out override params private protected public readonly ref return sealed sizeof stackalloc switch this throw true try typeof unchecked unsafe using virtual while");
scintilla.SetKeywords(1, "bool byte char class const decimal double enum float int long sbyte short static string struct uint ulong ushort void");
```

### <a name="auto-folding"></a>Automatic Code Folding

Most of the lexers included with Scintilla also support code folding (sometimes called outlining). Depending on the level of control you want, you can handle that manually, automatically, or anywhere in between. The recipe below shows the basics of how to configure a lexer so it will do automatic code folding:

```cs
// Set the lexer
scintilla.Lexer = Lexer.Cpp;

// Instruct the lexer to calculate folding
scintilla.SetProperty("fold", "1");
scintilla.SetProperty("fold.compact", "1");

// Configure a margin to display folding symbols
scintilla.Margins[2].Type = MarginType.Symbol;
scintilla.Margins[2].Mask = Marker.MaskFolders;
scintilla.Margins[2].Sensitive = true;
scintilla.Margins[2].Width = 20;

// Set colors for all folding markers
for (int i = 25; i <= 31; i++)
{
    scintilla.Markers[i].SetForeColor(SystemColors.ControlLightLight);
    scintilla.Markers[i].SetBackColor(SystemColors.ControlDark);
}

// Configure folding markers with respective symbols
scintilla.Markers[Marker.Folder].Symbol = MarkerSymbol.BoxPlus;
scintilla.Markers[Marker.FolderOpen].Symbol = MarkerSymbol.BoxMinus;
scintilla.Markers[Marker.FolderEnd].Symbol = MarkerSymbol.BoxPlusConnected;
scintilla.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
scintilla.Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.BoxMinusConnected;
scintilla.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
scintilla.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

// Enable automatic folding
scintilla.AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change);
```

By default a lexer will not do the additional work of calculating fold points unless you configure it to. With the `SetProperty` method you can pass configuration values to a lexer. The lexer behaviors you can customize—their names and possible values—however, are very poorly documented. Perhaps a future version of ScintillaNET will address this. That being said, virtually every lexer included with Scintilla uses the `"fold"` and `"fold.compact"` properties for enabling folding. Setting these to `"1"` will configure the lexer to calculate fold points and set the appropriate line markers.

By convention margin 2 is used for folding symbols but any margin will work if properly configured. In our sample margin 2 is configured as a `Symbol` margin, `Sensitive` to mouse clicks, and we use the `Marker.MaskFolders` constant to mask the range of possible markers the margin can display to just folding-related markers.

The markers designated for folding are indexes 25 through 31 and equivalent constants are named `Marker.Folder*`. When folding is enabled, Scintilla will automatically use the marker indexes in that range to indicate fold points. You can customize the appearance of those markers any way you wish, but you cannot change which marker indexes Scintilla expects to use for folding.

To customize our markers' appearance we use a short loop to change the default background and foreground colors. We then use the `Marker.Folder*` constants to set an appropriate symbol for each marker.

Finally, we enable automatic code folding with the `AutomaticFold` property. Had we not used automatic folding we would need to handle margin click events and use the folding API to toggle visibility of lines. 

### <a name="autocompletion"></a>Basic Autocompletion

You are responsible for triggering autocompletion, Scintilla won't do it for your. This confuses a lot of developers who believe it should be automatic. But don't worry, it's not difficult.

The easiest way is to monitor the `CharAdded` event—which is triggered each time a character is inserted into the document. Your application logic may then determine if the word being keyed in is a language keyword, an identifier name, or something else, and provide the appropriate list of possible autocompletion words. That's up to you.

At its simplest, this is how it works:

```cs
private void scintilla_CharAdded(object sender, CharAddedEventArgs e)
{
    // Find the word start
    var currentPos = scintilla.CurrentPosition;
    var wordStartPos = scintilla.WordStartPosition(currentPos, true);

    // Display the autocompletion list
    var lenEntered = currentPos - wordStartPos;
    if (lenEntered > 0)
    {
        scintilla.AutoCShow(lenEntered, "abstract as base break case catch checked continue default delegate do else event explicit extern false finally fixed for foreach goto if implicit in interface internal is lock namespace new null object operator out override params private protected public readonly ref return sealed sizeof stackalloc switch this throw true try typeof unchecked unsafe using virtual while");
    }
}
```

When you display an autocompletion list you tell Scintilla how many characters of the word being completed have already been entered. By doing this, Scintilla will narrow down the list of possible completion words and, when the user selects one of those words with the tab or enter key, automatically complete the *rest* of the word and not insert the *entire* word. That's what the code using `WordStartPosition` is doing—figuring out how many characters of the current word have already been entered.

The `if (lenEntered > 0)` check is a way of making sure the user is entering a word and not just typing whitespace. If `wordStartPos` was the same as `currentPos` it would mean our caret is in the middle of whitespace, not a word. Another way to do that would be to check the `CharAddedEventArgs.Char` property.

### <a name="brace-matching"></a>Brace Matching

So you wanna highlight matching braces? No problem. But it doesn't come for free. What may be a brace character in one programming language may be an operator in another. Thus, Scintilla provides facilities for you to highlight matching braces but it doesn't do it for you.

Our basic approach will be this: listen to the `UpdateUI` event to know when the caret changes position, determine if the caret is adjacent to a brace character, find the matching brace character using `BraceMatch`, and then highlight those characters with `BraceHighlight`. This may sound like a lot of work but it's actually pretty easy. Let's start by setting the brace styles:

```cs
scintilla.Styles[Style.BraceLight].BackColor = Color.LightGray;
scintilla.Styles[Style.BraceLight].ForeColor = Color.BlueViolet;
scintilla.Styles[Style.BraceBad].ForeColor = Color.Red;
```

You'll notice that we also set the style for an unmatched brace. We'll indicate those in red.

For the purposes of this recipe we'll assume the following characters are braces: '(', ')', '[', ']', '{', '}', '<', and '>'. This is convenient because that's also what the `BraceMatch` method supports, as you'll see in a minute. To determine if a character is one of the brace characters we want to process we'll use a simple helper method:

```cs
private static bool IsBrace(int c)
{
    switch (c)
    {
        case '(':
        case ')':
        case '[':
        case ']':
        case '{':
        case '}':
        case '<':
        case '>':
            return true;
    }

    return false;
}
```

All that remains now is to handle the `UpdateUI` event:

```cs
int lastCaretPos = 0;

private void scintilla_UpdateUI(object sender, UpdateUIEventArgs e)
{
    // Has the caret changed position?
    var caretPos = scintilla.CurrentPosition;
    if (lastCaretPos != caretPos)
    {
        lastCaretPos = caretPos;
        var bracePos1 = -1;
        var bracePos2 = -1;

        // Is there a brace to the left or right?
        if (caretPos > 0 && IsBrace(scintilla.GetCharAt(caretPos - 1)))
            bracePos1 = (caretPos - 1);
        else if (IsBrace(scintilla.GetCharAt(caretPos)))
            bracePos1 = caretPos;

        if (bracePos1 >= 0)
        {
            // Find the matching brace
            bracePos2 = scintilla.BraceMatch(bracePos1);
            if (bracePos2 == Scintilla.InvalidPosition)
                scintilla.BraceBadLight(bracePos1);
            else
                scintilla.BraceHighlight(bracePos1, bracePos2);
        }
        else
        {
            // Turn off brace matching
            scintilla.BraceHighlight(Scintilla.InvalidPosition, Scintilla.InvalidPosition);
        }
    }
}
```

The `UpdateUI` event can be called for many reasons—not just because the caret moved. So at the top of the handler is a check to see if the caret has moved since the last time the `UpdateUI` event fired. Next we peek at the character before the current caret position and *at* the current caret position to determine if either is a brace character using our simple `IsBrace` helper method. If it is a brace character, `bracePos` will be set and we can then look for the matching brace character. Scintilla makes this easy by providing the `BraceMatch` method which looks for the same brace characters that our `IsBrace` method does and knows whether to scan backwards or forwards depending on the start character. It's also smart enough to skip over nested braces. If you wanted to find a character that the `BraceMatch` method doesn't support you would have to roll your own scanning logic. If a matching brace is found we highlight both brace positions using the `BraceHighlight` method, if not, we highlight the orphaned brace with the `BraceBadLight` method.

That's it for basic brace matching, but we could go one step further. Scintilla also provides the ability to show an indentation guide, which is a vertical line at different indentation levels. This is a good companion feature for brace matching. When we do brace matching we can also highlight the corresponding indentation guide in the `BraceLight` style. To enable indentation guides we use the `IndentationGuides` property:

```cs
scintilla.IndentationGuides = IndentView.LookBoth;
```

To highlight an indentation guide we use the `HighlightGuide` property. The value of this property is the column number of the guide to highlight. Since indentation guides are vertical lines at different indentation levels, it makes sense that we have to tell Scintilla which indentation level should be highlighted by providing it with the column (read level) of the indentation. The column number of a document position can be determined using the `GetColumn` method.

Putting that all together, here is the complete recipe:

```cs
int lastCaretPos = 0;

private void form_Load(object sender, EventArgs e)
{
    scintilla.IndentationGuides = IndentView.LookBoth;
    scintilla.Styles[Style.BraceLight].BackColor = Color.LightGray;
    scintilla.Styles[Style.BraceLight].ForeColor = Color.BlueViolet;
    scintilla.Styles[Style.BraceBad].ForeColor = Color.Red;
}

private static bool IsBrace(int c)
{
    switch (c)
    {
        case '(':
        case ')':
        case '[':
        case ']':
        case '{':
        case '}':
        case '<':
        case '>':
            return true;
    }

    return false;
}

private void scintilla_UpdateUI(object sender, UpdateUIEventArgs e)
{
    // Has the caret changed position?
    var caretPos = scintilla.CurrentPosition;
    if (lastCaretPos != caretPos)
    {
        lastCaretPos = caretPos;
        var bracePos1 = -1;
        var bracePos2 = -1;

        // Is there a brace to the left or right?
        if (caretPos > 0 && IsBrace(scintilla.GetCharAt(caretPos - 1)))
            bracePos1 = (caretPos - 1);
        else if (IsBrace(scintilla.GetCharAt(caretPos)))
            bracePos1 = caretPos;

        if (bracePos1 >= 0)
        {
            // Find the matching brace
            bracePos2 = scintilla.BraceMatch(bracePos1);
            if (bracePos2 == Scintilla.InvalidPosition)
            {
                scintilla.BraceBadLight(bracePos1);
                scintilla.HighlightGuide = 0;
            }
            else
            {
                scintilla.BraceHighlight(bracePos1, bracePos2);
                scintilla.HighlightGuide = scintilla.GetColumn(bracePos1);
            }
        }
        else
        {
            // Turn off brace matching
            scintilla.BraceHighlight(Scintilla.InvalidPosition, Scintilla.InvalidPosition);
            scintilla.HighlightGuide = 0;
        }
    }
}
```

### <a name="insert-check"></a>Intercepting Inserted Text

There are numerous events to inform you of when text has changed. In addition to the `TextChanged` event provided by almost all Windows Forms controls, Scintilla also provides events for `Insert`, `Delete`, `BeforeInsert`, and `BeforeDelete`. By using these events you can trigger other changes in your application.

These events are all read-only, however. Changes made by a user to the text can be observed, but not modified—with one exception. The `InsertCheck` event occurs before text is inserted (and earlier than the `BeforeInsert` event) and is provided for the express purpose of giving you an option to modify the text being inserted. This can be used to simply cancel/prevent unwanted user input. Or in more advanced situations, it could be used to replace user input.

The following code snippet illustrates how you might handle the `InsertCheck` event to transform user input to HTML encode input:

```cs
private void scintilla_InsertCheck(object sender, InsertCheckEventArgs e)
{
    e.Text = WebUtility.HtmlEncode(e.Text);
}
```

### <a name="line-numbers"></a>Displaying Line Numbers

Someone new to Scintilla might wonder why displaying line numbers gets its own recipe when one would assume it's as simple as flipping a single Boolean property. Well it's not. The subject of line numbers touches on the much larger subject of margins. In Scintilla there can be up to five margins (0 through 4) on the left edge of the control, of which, line numbers is just one of those. By convention Scintilla sets the `Margin.Type` property of margin 0 to `MarginType.Number`, making it the de facto line number margin. Any margin can display line numbers though if its `Type` property is set to `MarginType.Number`. Scintilla also hides line numbers by default by setting the `Width` of margin 0 to zero. To display the default line number margin, increase its width:

```cs
scintilla.Margins[0].Width = 16;
```

You'll quickly find, however, that once you reach lines numbers in the 100's range the width of your line number margin is no longer sufficient. Scintilla doesn't automatically increase or decrease the width of a margin—including a line number margin. Why? It goes back to the fact that a margin could display line numbers or it could display something else entirely where dynamically growing and shrinking would be an undesirable trait.

The line number margin can be made to grow or shrink dynamically, it just requires a little extra code your part. In the recipe below we handle the `TextChanged` event so we can know when the number of lines changes. (*There are several other events we could use to determine the content has changed, but `TextChanged` will do just fine.*) Then we measure the width of the last line number in the document (or equivalent number of characters) using the `TextWidth` method. Finally, set the `Width` of the line number margin. Some caching of the calculation is thrown in for good measure since the number of lines will change far less than the `TextChanged` event will fire.

```cs
private int maxLineNumberCharLength;
private void scintilla_TextChanged(object sender, EventArgs e)
{
    // Did the number of characters in the line number display change?
    // i.e. nnn VS nn, or nnnn VS nn, etc...
    var maxLineNumberCharLength = scintilla.Lines.Count.ToString().Length;
    if (maxLineNumberCharLength == this.maxLineNumberCharLength)
        return;

    // Calculate the width required to display the last line number
    // and include some padding for good measure.
    const int padding = 2;
    scintilla.Margins[0].Width = scintilla.TextWidth(Style.LineNumber, new string('9', maxLineNumberCharLength + 1)) + padding;
    this.maxLineNumberCharLength = maxLineNumberCharLength;
}
```

*NOTE: The color of the text displayed in a line number margin can be controlled via the `Style.LineNumber` style definition.*

#### <a name="custom-line-numbers"></a>Custom Line Numbers

Scintilla has no built-in facilities for changing the line number display to some other text or format; however, using a standard text margin (rather than a number margin) we could simulate line numbers using any text or format we'd like.

In this example we'll assume our user is either a savant or masochist and wants line numbers displayed in hexadecimal format. The first step would be to change margin 0 (traditionally use for line numbers) to a right-aligned text margin.

```cs
scintilla.Margins[0].Type = MarginType.RightText;
scintilla.Margins[0].Width = 35;
```

Now we just need to set the margin text for each line to a hexadecimal representation of its line number and keep it up-to-date when the lines change. To monitor changes I'll use the `Insert` and `Delete` events because they include a property indicating the number of lines added or removed. Using the `TextChanged` event wouldn't tell me that (easily). A small optimization I've also done is to update only the lines affected rather than every line. That would be anything *following* the changed line, but not before it—those are still valid.

```cs
private void UpdateLineNumbers(int startingAtLine)
{
    // Starting at the specified line index, update each
    // subsequent line margin text with a hex line number.
    for (int i = startingAtLine; i < scintilla.Lines.Count; i++)
    {
        scintilla.Lines[i].MarginStyle = Style.LineNumber;
        scintilla.Lines[i].MarginText = "0x" + i.ToString("X2");
    }
}

private void scintilla_Insert(object sender, ModificationEventArgs e)
{
    // Only update line numbers if the number of lines changed
    if (e.LinesAdded != 0)
        UpdateLineNumbers(scintilla.LineFromPosition(e.Position));
}

private void scintilla_Delete(object sender, ModificationEventArgs e)
{
    // Only update line numbers if the number of lines changed
    if (e.LinesAdded != 0)
        UpdateLineNumbers(scintilla.LineFromPosition(e.Position));
}
```

### <a name="zooming"></a>Zooming

Scintilla can increase or decrease the size of the displayed text by a "zoom factor":

```cs
scintilla.ZoomIn(); // Increase
scintilla.ZoomOut(); // Decrease
scintilla.Zoom = 15; // "I like big 'text' and I cannot lie..."
```

*NOTE: The default key bindings set `CTRL+NUMPLUS` and `CTRL+NUMMINUS` to zoom in and zoom out, respectively.*

### <a name="update-ui"></a>Updating Dependent Controls

A common feature most developers wish to provide with their Scintilla-based IDEs is to indicate where the caret (i.e. cursor) is at all times by perhaps displaying its location in the status bar. The `UpdateUI` event is well suited to this. It is fired any time there is a change to text content or styling, the selection, or scroll positions and provides a way for identifying which of those changes caused the event to fire. This can be used to update any dependent controls or even synchronize the scrolling of one Scintilla control with another.

To display the current caret position and selection range in the status bar, try:

```cs
private void scintilla_UpdateUI(object sender, UpdateUIEventArgs e)
{
    if ((e.Change & UpdateChange.Selection) > 0)
    {
        // The caret/selection changed
        var currentPos = scintilla.CurrentPosition;
        var anchorPos = scintilla.AnchorPosition;
        toolStripStatusLabel.Text = "Ch: " + currentPos + " Sel: " + Math.Abs(anchorPos - currentPos);
    }
}
```

### <a name="find-highlight"></a>Find and Highlight Words

The following example will find all occurrences of the string specified (case-insensitive) and highlight them with a light-green indicator:

```cs
private void HighlightWord(string text)
{
    // Indicators 0-7 could be in use by a lexer
    // so we'll use indicator 8 to highlight words.
    const int NUM = 8;

    // Remove all uses of our indicator
    scintilla.IndicatorCurrent = NUM;
    scintilla.IndicatorClearRange(0, scintilla.TextLength);

    // Update indicator appearance
    scintilla.Indicators[NUM].Style = IndicatorStyle.StraightBox;
    scintilla.Indicators[NUM].ForeColor = Color.Green;
    scintilla.Indicators[NUM].OutlineAlpha = 50;
    scintilla.Indicators[NUM].Alpha = 30;

    // Search the document
    scintilla.TargetStart = 0;
    scintilla.TargetEnd = scintilla.TextLength;
    scintilla.SearchFlags = SearchFlags.None;
    while (scintilla.SearchInTarget(text) != -1)
    {
        // Mark the search results with the current indicator
        scintilla.IndicatorFillRange(scintilla.TargetStart, scintilla.TargetEnd - scintilla.TargetStart);

        // Search the remainder of the document
        scintilla.TargetStart = scintilla.TargetEnd;
        scintilla.TargetEnd = scintilla.TextLength;
    }
}
```

This example also illustrates the "set-once, run-many" style API that Scintilla is known for. When performing a search, the `TargetStart` and `TargetEnd` properties are set to indicate the search range prior to calling `SearchInTarget`. The indicators API is similar. The `IndicatorCurrent` property is first set and then subsequent calls to `IndicatorClearRange` and `IndicatorFillRange` make use of that value.

*NOTE: Indicators and styles can be used simultaneously.*

### <a name="whitespace"></a>View Whitespace

Scintilla has several properties for controlling the display and color of whitespace (space and tab characters). By default, whitespace is not visible. It can be made visible by setting the `ViewWhitespace` property. Since whitespace can be significant to some programming languages the default behavior is for the current lexer to set the color of whitespace. To override the default behavior the `SetWhitespaceForeColor` and `SetWhitespaceBackColor` methods can be used. To make whitespace visible and always display in an orange color (regardless of the current lexer), try:

```cs
// Display whitespace in orange
scintilla.WhitespaceSize = 2;
scintilla.ViewWhitespace = WhitespaceMode.VisibleAlways;
scintilla.SetWhitespaceForeColor(true, Color.Orange);
```

### <a name="line-spacing"></a>Increase Line Spacing

Feeling like your text is a little too cramped? Having a little extra space between lines can sometimes help the readability of code. In some text editors and IDEs you would need to use a different font which has a larger ascent and/or descent (I'm looking at you Visual Studio). In Scintilla you can increase the text ascent and decent independently of the font using the `ExtraAscent` and `ExtraDescent` properties.

```cs
// Increase line spacing
scintilla.ExtraAscent = 5;
scintilla.ExtraDescent = 5;
```

### <a name="multiple-selections"></a>Rectangular/Multiple Selections

Scintilla supports the now popular ability of making a "rectangular" selection. A user can do this by holding the `ALT` key when dragging the mouse. This feature is a specialized version of what is more generally known a "multiple selections" in Scintilla. To get the most of our rectangular selections you'll likely want to enable the following features at a minimum:

```cs
scintilla.MultipleSelection = true;
scintilla.MouseSelectionRectangularSwitch = true;
scintilla.AdditionalSelectionTyping = true;
scintilla.VirtualSpaceOptions = VirtualSpace.RectangularSelection;
```

Internally Scintilla handles rectangular selections as a list of multiple selection ranges. When there are multiple selections, one range it is said to be the "main selection" and the others are known as "additional selections". These ranges do not need to be contiguous as they are in a rectangular selection. Once multiple selections are enabled with the `MultipleSelection` property, a user can hold the `CTRL` key to select additional, non-continuous ranges of text.

To get information about the current selections you can enumerate the `Scintilla.Selections` property like so:

```cs
foreach (var selection in scintilla.Selections)
{
    // Print the current selection's range
    Debug.WriteLine("Start: " + selection.Start + ", End: " + selection.End);
}
```

Most of the properties on the enumerated `Selection` object have setters if you want to modify the bounds of an existing selection.

Selections are only queried through the `Scintilla.Selections` collection, not created. Selections are created using the `Scintilla.SetSelection` method to make the main selection and the subsequent calls to the `AddSelection` method as many times is needed to add additional selections.

If you intend to programmatically create a rectangular selection you can save yourself the time of calling `SetSelection` and multiple calls to `AddSelection` by using the `RectangularSelection*` properties. These allow you to specify the anchor and caret (and virtual space) positions of the rectangle and Scintilla will do the extra work of converting those to multiple selections.

In much the same way that you can customize the appearance of a single selection using the `SetSelectionForeColor` and `SetSelectionBackColor` methods, you may use the `SetAdditionalSelFore` and `SetAdditionalSelBack` methods to do the same for additional selections. The caret in an additional selection can also be customized using the `AdditionalCaret*` properties.

### <a name="bookmarks"></a>Bookmark Lines

This recipe shows how markers can be used to indicate bookmarked lines and iterate through them. Markers are symbols displayed in the left margins and are typically used for things like showing breakpoints, search results, the current line of execution, or in this case, bookmarks.

In the `Form.Load` event we'll prepare a margin to be used for our bookmarks and we'll configure one of the markers to use the `Bookmark` symbol:

```cs
private const int BOOKMARK_MARGIN = 1; // Conventionally the symbol margin
private const int BOOKMARK_MARKER = 3; // Arbitrary. Any valid index would work.

private void MainForm_Load(object sender, EventArgs e)
{
    var margin = scintilla.Margins[BOOKMARK_MARGIN];
    margin.Width = 16;
    margin.Sensitive = true;
    margin.Type = MarginType.Symbol;
    margin.Mask = Marker.MaskAll;
    margin.Cursor = MarginCursor.Arrow;

    var marker = scintilla.Markers[BOOKMARK_MARKER];
    marker.Symbol = MarkerSymbol.Bookmark;
    marker.SetBackColor(Color.DeepSkyBlue);
    marker.SetForeColor(Color.Black);
}
```

The margin is flagged as `Sensitive` so we can receive mouse click notifications from it (and because this margin can sometimes get emotional). The margin `Mask` is a way of restricting which marker symbols can appear in the margin. For the purposes of this example we'll configure it so that all marker symbols assigned to any given line will be displayed.

By handling the `MarginClick` event we can toggle a bookmark marker for each line:

```cs
private void scintilla_MarginClick(object sender, MarginClickEventArgs e)
{
    if (e.Margin == BOOKMARK_MARGIN)
    {
        // Do we have a marker for this line?
        const uint mask = (1 << BOOKMARK_MARKER);
        var line = scintilla.Lines[scintilla.LineFromPosition(e.Position)];
        if ((line.MarkerGet() & mask) > 0)
        {
            // Remove existing bookmark
            line.MarkerDelete(BOOKMARK_MARKER);
        }
        else
        {
            // Add bookmark
            line.MarkerAdd(BOOKMARK_MARKER);
        }
    }
}
```

The code above makes use of the `MarkerGet` method to get a bitmask indicating which markers are set for the current line. This mask is a 32-bit value where each bit corresponds to one of the 32 marker indexes. If marker `0` were set, bit `0` would be set. If marker `1` where set, bit `1` would be set and so on. To determine if our maker has been set for a line we would want to check if the `1 << 3` bit is set which is what the statement `(line.MarkerGet() & mask) > 0` does.

At this point, if you run the code you should be able to add and remove pretty blue bookmarks from document lines. To let a user jump between bookmarks we'll add 'Next' and 'Previous' buttons and wire-up their click events like this:

```cs
private void buttonPrevious_Click(object sender, EventArgs e)
{
    var line = scintilla.LineFromPosition(scintilla.CurrentPosition);
    var prevLine = scintilla.Lines[--line].MarkerPrevious(1 << BOOKMARK_MARKER);
    if (prevLine != -1)
        scintilla.Lines[prevLine].Goto();
}

private void buttonNext_Click(object sender, EventArgs e)
{
    var line = scintilla.LineFromPosition(scintilla.CurrentPosition);
    var nextLine = scintilla.Lines[++line].MarkerNext(1 << BOOKMARK_MARKER);
    if (nextLine != -1)
        scintilla.Lines[nextLine].Goto();
}
```

### <a name="key-bindings"></a>Key Bindings

When a new `Scintilla` instance is created, there are a number key bindings set by default. All default bindings can be cleared in one fell swoop using the `ClearCmdKeys` method, or individually using the `ClearCmdKey` method.

To disable the use of `CTRL`+`V` as a shortcut for paste:

```cs
scintilla.ClearCmdKey(Keys.Control | Keys.V);
```

An alternative way would be to assign `Command.Null` to the `CTRL`+`V` keys:

```cs
scintilla.AssignCmdKey(Keys.Control | Keys.V, Command.Null);
```

If, for example, you wanted to provide an option in your editor where holding the `CTRL` key would enable a Vi (or Vim) style of caret movement with the `H`, `J`, `K`, and `L` keys:

```cs
scintilla.AssignCmdKey(Keys.Control | Keys.H, Command.CharLeft);
scintilla.AssignCmdKey(Keys.Control | Keys.J, Command.LineDown);
scintilla.AssignCmdKey(Keys.Control | Keys.K, Command.LineUp);
scintilla.AssignCmdKey(Keys.Control | Keys.L, Command.CharRight);
```

*TIP: You can execute a `Command` using the `ExecuteCmd` method without having to bind it to a key combination if you just want to perform the command programmatically.*

### <a name="documents"></a>Documents

*This is an advanced topic.*

Scintilla has limited support for document-control separation, allowing more than one Scintilla control to access the same document (e.g. splitter windows), one control to access multiple documents (tabs), or for loading a document outside of a Scintilla control.

To work with documents requires a firm understanding of how document reference counting works. As has been said before, one of the project goals of the new ScintillaNET was to not shield users from the core Scintilla API and working with `Document` instances, `ILoader` instances, and reference counting is a prime example of that. It requires diligent programming to prevent memory leaks.

#### <a name="reference-counting"></a> Understanding Document Reference Counts

One of the things we must get straight right away is to understand that every `Document` in Scintilla contains a reference count. When a new document is created the reference count is 1. When a document is associated with a Scintilla control or the `Scintilla.AddRefDocument` method is used, that reference count increases. When a document is removed from a Scintilla control or the `Scintilla.ReleaseDocument` method is used, that count is decreased. When the reference count reaches 0 the document will be released and the memory freed. Conversely, if the document count never reaches 0 memory leaks will occur.

So making sure a document reference count reaches 0 is good, however, allowing that to happen when the document is currently in use by a Scintilla control is bad. Don't do that. The universe will implode.

With that out of the way...

#### <a name="multiple-views"></a>Multiple Views of one Document

To share the same document between two Scintilla controls, set their `Document` properties to the same value:

```cs
scintilla2.Document = scintilla1.Document;
```

The `scintilla2` control will now show the same contents as the `scintilla1` control (and vice versa). To break the connection we can force `scintilla2` to drop the current document reference and create a new one by setting the `Document` property to `Document.Empty`:

```cs
scintilla2.Document = Document.Empty;
```

This type of document sharing is relatively free from having to track reference counts. Each time a document is associated with more than one Scintilla control, the document reference count is increased. Each time it is dissociated that reference count is decreased—including when a Scintilla control is disposed. If that was the last association to that document, the reference count of the document would reach 0 and the memory freed.

*NOTE: The `Document.Empty` constant is not an empty document; it is the absence of one (i.e. null).*

#### <a name="multiple-documents"></a>Multiple Documents for one View

Most modern text editors and IDEs support the ability to have multiple tabs open at once. One way to accomplish that would be to have multiple Scintilla controls, each with their own document. To conserve resources, however, you could choose to have only one Scintilla control and select multiple documents in and out of it. Your tabs would then simply be a way to switch between those documents, not display separate controls. This is how [Notepad++](http://notepad-plus-plus.org/) handles multiple documents.

As previously explained, each time a `Document` is unassociated with a Scintilla control its reference count decreases by one. So, if we want to select a new blank document into a Scintilla control, *but not delete the current one*, we need to increase the reference count of the current document *before* we select the new one:

```cs
private void NewDocument()
{
    var document = scintilla.Document;
    scintilla.AddRefDocument(document);

    // Replace the current document with a new one
    scintilla.Document = Document.Empty;
}
```

Said another way, calling `AddRefDocument` will increase the current document reference count from 1 to 2. When the `Document` property is set, the current document is unassociated with the control and its cout is decreased from 2 to 1. Had we not prematurely increased the reference count, it would have gone from 1 to 0 and the document would have been deleted and not available for us to select back into the Scintilla control at a later time.

To switch the current document with an existing one (i.e. switch tabs) works almost the same way. We first increase the current document reference count from 1 to 2 so that when its unassociated with the Scintilla control it would drop from 2 to 1 and not be deleted. When the next (existing) document is selected into the Scintilla control its count will increase from 1 to 2, so we call `ReleaseDocument` after making the association to drop it back to 1 and make Scintilla the sole owner:

```cs
private void SwitchDocument(Document nextDocument)
{
    var prevDocument = scintilla.Document;
    scintilla.AddRefDocument(prevDocument);

    // Replace the current document and make Scintilla the owner
    scintilla.Document = nextDocument;
    scintilla.ReleaseDocument(nextDocument);
}
```

At any time you can use the `AddRefDocument` and `ReleaseDocument` methods to make these kind of adjustments so long as you're mindful of the reference count.

#### <a name="loader"></a>Background Loading

A `Document` can be loaded in a background thread using an `ILoader` instance. The `ILoader` interface contains only three methods: `AddData`, `ConvertToDocument`, and `Release`. To add text to the document call `AddData`. Once done, call `ConvertToDocument` to get the `Document` handle.

I find the `ConvertToDocument` method to have a misleading name. Internally an `ILoader` instance contains a new `Document` instance. The `ConvertToDocument` method simply returns that internal instance. No 'conversion' is taking place.

Once it's understood that the `ILoader` is a wrapper around a `Document` it should be obvious that we need to keep track of its internal document reference count just as we would any other document. When the `ILoader` is created, the internal document has a reference count of 1 (as do all new documents). If for any reason we need to get rid of this document because there was an error loading in the data or we want to cancel the process we would need to decrease that internal document reference count or incur a memory leak.

That is the whole purpose for the `ILoader.Release` method—to decrease the reference count of the internal document. That also explains why we shouldn't call `Release` on the `ILoader` once we've successfully called `ConvertToDocument`. That would decrease the reference count of the document we just got access to.

This is best explained with an example. The method below will use the `ILoader` instance specified to load characters data from a file at the specified path and return the completed document:

```cs
private async Task<Document> LoadFileAsync(ILoader loader, string path, CancellationToken cancellationToken)
{
    try
    {
        using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
        using (var reader = new StreamReader(file))
        {
            var count = 0;
            var buffer = new char[4096];
            while ((count = await reader.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
            {
                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();

                // Add the data to the document
                if (!loader.AddData(buffer, count))
                    throw new IOException("The data could not be added to the loader.");
            }

            return loader.ConvertToDocument();
        }
    }
    catch
    {
        loader.Release();
        throw;
    }
}
```

Data is added through the `AddData` method. This is done on a background thread and is the whole point of using the `ILoader`. If `AddData` returns `false` it means Scintilla encountered an error (out of memory) and we should stop loading. If that or any other error occurs—including cancellation—we're careful to make sure we call `ILoader.Release` in our catch block to drop the reference count of the internal document from 1 to 0 so that it will be deleted.

To use our new `LoadFileAsync` method, we might do something like this:

```cs
private async void button_Click(object sender, EventArgs e)
{
    try
    {
        var loader = scintilla.CreateLoader(256);
        if (loader == null)
            throw new ApplicationException("Unable to create loader.");

        var cts = new CancellationTokenSource();
        var document = await LoadFileAsync(loader, @"your_file_path.txt", cts.Token);
        scintilla.Document = document;

        // Every document starts with a reference count of 1. Assigning it to Scintilla increased that to 2.
        // To let Scintilla control the life of the document, we'll drop it back down to 1.
        scintilla.ReleaseDocument(document);
    }
    catch (OperationCanceledException)
    {
    }
    catch(Exception)
    {
        MessageBox.Show(this, "There was an error loading the file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
```

An `ILoader` is created by calling `Scintilla.CreateLoader`. The length argument of `256` is arbitrary and is meant to be a hint to Scintilla on how much memory it should allocate. If the loader exceeds the initial allocation, it will automatically allocate more so you don't need to be exact.

As noted in the code comments (and ad nauseam above), we must decrease the document reference count once selected into the Scintilla control because the document is born with a reference count of 1. That increased from 1 to 2 when associated with the control. If we're not sure we're going to take ownership back any time soon, it's probably best to make Scintilla the sole owner so we drop it back from 2 to 1.

*NOTE: While `AddData` is meant to be called from a background thread, `Scintilla.CreateLoader`, `Scintilla.Document`, `Scintilla.ReleaseDocument`, etc... are not. Any interaction with Scintilla should always be done on the UI thread.*

### <a name="scilexer"></a>Using a Custom SciLexer.dll

*This is an advanced topic.*

On rare occasions you may wish to provide your own build of the SciLexer DLL used by ScintillaNET instead of the one we embed for you. By default ScintillaNET will use the embedded one but you can override the default behavior by calling `SetModulePath` prior to instantiating any Scintilla controls:

```cs
// Call prior to creating any controls
Scintilla.SetModulePath("AltSciLexer.dll");

// Load a control and get the SciLexer.dll info
var scintilla = new Scintilla();
var version = scintilla.GetVersionInfo();
```

The path provided can be an absolute or relative path to SciLexer.dll.

It goes without saying that the SciLexer DLL embedded with ScintillaNET has been tested with ScintillaNET; yours has not. You run a compatibility risk running a nonstandard DLL.

### <a name="direct-message"></a>Direct Messages

*This is an advanced topic.*

At its heart Scintilla is a Win32 control. The standard convention for passing messages to any Win32 control is via the Windows [SendMessage](https://msdn.microsoft.com/en-us/library/windows/desktop/ms644950.aspx) function. The same is true of Scintilla. If for any reason you want to bypass the managed wrapper we've created with ScintillaNET you can go directly to the underlying Win32 control via the SendMessage function.

Similar to the Windows SendMessage function is the `Scintilla.DirectMessage` method. It has roughly the same signature as the SendMessage function and serves the same purpose; but internally `DirectMessage` bypasses the Windows message loop to offer better performance than SendMessage. This is how ScintillaNET makes calls to the native Scintilla control.

So if you're crazy enough there is nothing to stop you from finding a message constant in the original Scintilla.h C++ header file and use the `DirectMessage` method like this:

```cs
const int SCI_PAGEDOWN = 2322;
scintilla.DirectMessage(SCI_PAGEDOWN, IntPtr.Zero, IntPtr.Zero);
```

The message above will scroll the view down one page.

I have to warn you though in the strongest possible terms that you should never, **never use the `DirectMessage` method unless you are absolutely sure of what you're doing**. In some cases ScintillaNET makes assumptions about the current state of the Scintilla control and if you sidestep ScintillaNET terrible things may happen.
> "Fire and brimstone coming down from the skies! Rivers and seas boiling! Forty years of darkness! Earthquakes, volcanoes... The dead rising from the grave! Human sacrifice, dogs and cats living together... mass hysteria!" — [Ghostbusters (1984)](http://www.imdb.com/title/tt0087332/quotes?item=qt0475882)

You've been warned.

## License

The MIT License (MIT)

Copyright (c) 2015, Jacob Slusser, https://github.com/jacobslusser

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
