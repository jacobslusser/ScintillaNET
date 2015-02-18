# ScintillaNET

ScintillaNET is a Windows Forms control, wrapper, and bindings for the versatile [Scintilla](http://www.scintilla.org/) source editing component.

> "As well as features found in standard text editing components, Scintilla includes features especially useful when editing and debugging source code. These include support for syntax styling, error indicators, code completion and call tips. The selection margin can contain markers like those used in debuggers to indicate breakpoints and the current line. Styling choices are more open than with many editors, allowing the use of proportional fonts, bold and italics, multiple foreground and background colours and multiple fonts." — [scintilla.org](http://www.scintilla.org/)

ScintillaNET can also be used with WPF using the <a href="https://msdn.microsoft.com/en-us/library/ms751761.aspx">WindowsFormsHost</a>.

### Project Status

ScintillaNET is currently in active development and is not considered ready for general use.

## Background

This project is a rewrite of the [ScintillaNET project hosted at CodePlex](http://scintillanet.codeplex.com/) and maintained by myself and others. After many years of contributing to that project I decided to think differently about the API we had created and felt I could make better one if I was willing to go back to a blank canvas. Thus, this project is the spiritual successor to the original ScintillaNET but has been written from scratch.

### First Class Characters

One of the issues that ScintillaNET has historically suffered from is the fact that the native Scintilla control operates on bytes, not characters. Prior versions of ScintillaNET did not account for this, and when you're dealing with Unicode, [one byte doesn't always equal one character](http://www.joelonsoftware.com/articles/Unicode.html). The result was an API that sometimes expected byte offsets and at other times expected character offsets. Sometimes things would work as expected and other times random failures and out-of-range exceptions would occur.

No more. **One of the major focuses of this rewrite was to give ScintillaNET an understanding of Unicode from the ground up.** Every API now consistently works with character based offsets and ranges just like .NET developers expect. Internally we maintain a mapping of character to byte offsets (and vice versa) and do all the translation for you so you never need to worry about it. No more out-of-range exceptions. No more confusion. No more pain. [It just works](http://en.wikipedia.org/wiki/List_of_Apple_Inc._slogans).

### One Library

The second most popular ScintillaNET issue was confusion distributing the ScintillaNET DLL and its native component, the SciLexer DLL. ScintillaNET is a wrapper. Without the SciLexer.dll containing the core Scintilla functionality it is nothing. As a native component, SciLexer.dll has to be compiled separately for 32 and 64-bit versions of Windows. So it was actually three DLLs that developers had to ship with their applications.

This proved a pain point because developers often didn't want to distribute so many libraries or wanted to place them in alternate locations which would break the DLL loading mechanisms used by PInvoke and ScintillaNET. It also causes headaches during design-time in Visual Studio for the same reasons.

To address this ScintillaNET now embeds a 32 and 64-bit version of SciLexer.dll in the ScintillaNET DLL. **Everything you need to run ScintillaNET in one library.** In addition to soothing the pain mentioned above this now makes it possible for us to create a ScintillaNET NuGet package.

### Keeping it Consistent

Another goal of the rewrite was to accept the original Scintilla API for what it is and not try to coerce it into a .NET-style API when it should not or could not be. A good example of this is how ScintillaNET uses indexers to access lines, but not treat them as a .NET collection. Lines in a Scintilla control are not items in a collection. There is no API to Add, Insert, or Remove a line in Scintilla and thus we don't try to create one in ScintillaNET. These deviations from .NET convention are rare, but are done to keep any native Scintilla documentation relevant to the managed wrapper and to avoid situations where trying to force the original API into a more familiar one is more detrimental than helpful.

*NOTE: This is not to say that ScintillaNET cannot add, insert, or remove lines. Those operations, however, are handled as text changes, not line changes.*

## Recipes

1. [Basic Text Retrieval and Modification](#basic-text)
  1. [Retrieving Text](#retrieve-text)
  2. [Insert, Append, Delete](#modify-text)
2. [Zooming](#zooming)
3. [Using a Custom SciLexer.dll](#scilexer)

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

### <a name="zooming"></a>Zooming

Scintilla can increase or decrease the size of the displayed text by a "zoom factor":

```cs
scintilla.ZoomIn(); // Increase
scintilla.ZoomOut(); // Decrease
scintilla.Zoom = 15; // "I like big 'text' and I cannot lie..."
```

*NOTE: The default key bindings set `CTRL+NUMPLUS` and `CTRL+NUMMINUS` to zoom in and zoom out, respectively.*

### <a name="scilexer"></a>Using a Custom SciLexer.dll

This is an advanced topic. On rare occasions you may wish to provide your own build of the SciLexer DLL used by ScintillaNET instead of the one we embed for you. By default ScintillaNET will use the embedded one but you can override the default behavior by calling `SetModulePath` prior to instantiating any Scintilla controls:

```cs
// Call prior to creating any controls
Scintilla.SetModulePath("AltSciLexer.dll");

// Load a control and get the SciLexer.dll info
var scintilla = new Scintilla();
var version = scintilla.GetVersionInfo();
```

The path provide can be an absolute or relative path to SciLexer.dll.

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
