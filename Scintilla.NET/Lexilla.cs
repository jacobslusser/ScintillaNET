using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace ScintillaNET;

/// <summary>
/// A class containing methods for interacting with the Lexilla library.
/// </summary>
public class Lexilla
{
    /// <summary>
    /// Initializes the Lexilla.dll library.
    /// </summary>
    /// <param name="lexillaHandle">The handle to the Lexilla.dll file.</param>
    internal Lexilla(IntPtr lexillaHandle)
    {
        const string win32Error = "The Scintilla module has no export for the '{0}' procedure.";

        var lpProcName = nameof(NativeMethods.CreateLexer);

        // Get the Lexilla functions needed to define lexers and create managed callbacks...

        var functionPointer = NativeMethods.GetProcAddress(new HandleRef(this, lexillaHandle), lpProcName);
        if (functionPointer == IntPtr.Zero)
        {
            throw new Win32Exception(string.Format(win32Error, lpProcName),
                new Win32Exception()); // Calls GetLastError
        }

        createLexer = (NativeMethods.CreateLexer) Marshal.GetDelegateForFunctionPointer(
            functionPointer, typeof(NativeMethods.CreateLexer));

        lpProcName = nameof(NativeMethods.GetLexerName);

        functionPointer = NativeMethods.GetProcAddress(new HandleRef(this, lexillaHandle), lpProcName);
        if (functionPointer == IntPtr.Zero)
        {
            throw new Win32Exception(string.Format(win32Error, lpProcName),
                new Win32Exception()); // Calls GetLastError
        }

        getLexerName = (NativeMethods.GetLexerName) Marshal.GetDelegateForFunctionPointer(
            functionPointer, typeof(NativeMethods.GetLexerName));

        lpProcName = nameof(NativeMethods.GetLexerCount);

        functionPointer = NativeMethods.GetProcAddress(new HandleRef(this, lexillaHandle), lpProcName);
        if (functionPointer == IntPtr.Zero)
        {
            throw new Win32Exception(string.Format(win32Error, lpProcName),
                new Win32Exception()); // Calls GetLastError
        }

        getLexerCount = (NativeMethods.GetLexerCount) Marshal.GetDelegateForFunctionPointer(
            functionPointer, typeof(NativeMethods.GetLexerCount));


        lpProcName = nameof(NativeMethods.LexerNameFromID);

        functionPointer = NativeMethods.GetProcAddress(new HandleRef(this, lexillaHandle), lpProcName);
        if (functionPointer == IntPtr.Zero)
        {
            throw new Win32Exception(string.Format(win32Error, lpProcName),
                new Win32Exception()); // Calls GetLastError
        }

        lexerNameFromId = (NativeMethods.LexerNameFromID) Marshal.GetDelegateForFunctionPointer(
            functionPointer, typeof(NativeMethods.LexerNameFromID));

        initialized = true;
    }

    #region Fields

    private static bool initialized;
        
    #endregion

    #region PrivateMethods

    private static void VerifyInitialized()
    {
        if (!initialized)
        {
            _ = Scintilla.GetModulePath();
        }
    }

    #endregion

    #region DllCalls
    private static NativeMethods.GetLexerCount getLexerCount;

    private static NativeMethods.GetLexerName getLexerName;

    private static NativeMethods.CreateLexer createLexer;

    private static NativeMethods.LexerNameFromID lexerNameFromId;
    #endregion

    #region DllWrapping

    /// <summary>
    /// Gets the lexer count in the Lexilla library.
    /// </summary>
    /// <returns>Amount of lexers defined in the Lexilla library.</returns>
    public static int GetLexerCount()
    {
        VerifyInitialized();
        return (int) getLexerCount();
    }

    /// <summary>
    /// Creates a lexer with the specified name.
    /// </summary>
    /// <param name="lexerName">The name of the lexer to create.</param>
    /// <returns>A <see cref="IntPtr"/> containing the lexer interface pointer.</returns>
    public static IntPtr CreateLexer(string lexerName)
    {
        VerifyInitialized();
        return createLexer(lexerName);
    }

    /// <summary>
    /// Gets the name of the lexer specified by an index number.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The name of the lexer if one was found with the specified index; <c>null</c> otherwise.</returns>
    public static string GetLexerName(int index)
    {
        VerifyInitialized();
        var pointer = Marshal.AllocHGlobal(1024);
        try
        {
            getLexerName((UIntPtr) index, pointer, new IntPtr(1024));
            return Marshal.PtrToStringAnsi(pointer);
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    /// <summary>
    /// Returns a lexer name with the specified identifier.
    /// </summary>
    /// <param name="identifier">The lexer identifier.</param>
    /// <returns>The name of the lexer if one was found with the specified identifier; <c>null</c> otherwise.</returns>
    public static string LexerNameFromId(int identifier)
    {
        VerifyInitialized();
        return lexerNameFromId(new IntPtr(identifier));
    }

    /// <summary>
    /// Gets the lexer names contained in the Lexilla library.
    /// </summary>
    /// <returns>An IEnumerable&lt;System.String&gt; value with the lexer names.</returns>
    public static IEnumerable<string> GetLexerNames()
    {
        var count = GetLexerCount();
        for (int i = 0; i < count; i++)
        {
            yield return GetLexerName(i);
        }
    }
    #endregion
}