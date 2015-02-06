# ScintillaNET

ScintillaNET is a Windows Forms control, wrapper, and bindings for the versatile [Scintilla](http://www.scintilla.org/) source editing component.

> "As well as features found in standard text editing components, Scintilla includes features especially useful when editing and debugging source code. These include support for syntax styling, error indicators, code completion and call tips. The selection margin can contain markers like those used in debuggers to indicate breakpoints and the current line. Styling choices are more open than with many editors, allowing the use of proportional fonts, bold and italics, multiple foreground and background colours and multiple fonts." -- scintilla.org

ScintillaNET can also be used with WPF using the <a href="https://msdn.microsoft.com/en-us/library/ms751761(v=vs.110).aspx">WindowsFormsHost</a>.

### Project Status

ScintillaNET is currently in active development and is not considered ready for general use.

## Background

This project is a rewrite of the [ScintillaNET project hosted at CodePlex](http://scintillanet.codeplex.com/) and maintained by myself and others. After many years of contributing to that project I decided to think differently about the API we had created and felt I could make better one if I was willing to go back to a blank canvas. Thus, this project is the spiritual successor to the original ScintillaNET but has been written from scratch.

The API in this rewrite is intended to more closely resemble the original Scintilla API but still be familiar to .NET developers. In cases where the Scintilla API and Windows Forms conventions conflict, ScintillaNET will break from the standard Windows Forms conventions to preserve the original Scintilla API as much as possible. This is most evident in the way ScintillaNET uses indexers to access styles and lines, but not treat them as .NET collections. This is intended to make any native Scintilla documentation still relevant to the managed wrapper and to avoid situations where trying to force the original API into a more familiar one is more detrimental than helpful.

## Bytes vs Characters

Scintilla (and by extension ScintillaNET) operates on bytes, not characters. This trips-up many .NET developers who are used to only working with the managed String type. In .NET all strings are UTF-16 characters, and each character is stored in memory as two bytes. Thus the first character in a string is at offset 0 in memory, the second character at offset 2, the third at offset 4, etc.... Each character is 2 bytes of memory and we can always expect the third character to be at the 4th byte in memory.

Scintilla(NET), on the other hand, stores text as UTF-8—where a single character can be represented as one, two, or more bytes of memory. So the third character _could_ be at the 4th offset in memory—if the preceding two characters each required two bytes each, _or_ it could be at the 2nd offset in memory if the preceding two characters only required one byte each. How many bytes of memory a UTF-8 character can occupy depends on the character.

If Scintilla(NET) operated on characters that would make all this exposition superfluous. But it doesn't. For performance and historical reasons the Scintilla(NET) APIs expect byte offsets and ranges, not character offsets and ranges. If you want to replace the the word "World" in "Hello World" you would need to know the byte offset and range of the word "World", _not_ its character offset and range. Sometimes byte and character offsets will be equal to each other, sometimes not. It all depends on the characters being used.

The work of converting a character offset and range to a byte offset and range (and vice versa) is on you. The [Encoding APIs](https://msdn.microsoft.com/en-us/library/system.text.encoding) offered in the .NET framework provide all the necessary tools. In addition, ScintillaNET provides some APIs to help with this.

If any of that is unclear, you should reread the paragraphs above and [take a crash course on Unicode](http://www.joelonsoftware.com/articles/Unicode.html).

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
