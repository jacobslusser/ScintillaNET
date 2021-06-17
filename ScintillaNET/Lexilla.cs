using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace ScintillaNET
{
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
            const string win32error = "The Scintilla module has no export for the '{0}' procedure.";

            var lpProcName = nameof(NativeMethods.CreateLexer);

            // Get the Lexilla functions needed to define lexers...
            var functionPointer = NativeMethods.GetProcAddress(new HandleRef(this, lexillaHandle), lpProcName);
            if (functionPointer == IntPtr.Zero)
            {
                throw new Win32Exception(string.Format(win32error, lpProcName), new Win32Exception()); // Calls GetLastError
            }

            // Create a managed callback
            createLexer = (NativeMethods.CreateLexer)Marshal.GetDelegateForFunctionPointer(
                functionPointer, typeof(NativeMethods.CreateLexer));

            lpProcName = nameof(NativeMethods.GetLexerName);

            // Get the Lexilla functions needed to define lexers...
            functionPointer = NativeMethods.GetProcAddress(new HandleRef(this, lexillaHandle), lpProcName);
            if (functionPointer == IntPtr.Zero)
            {
                throw new Win32Exception(string.Format(win32error, lpProcName), new Win32Exception()); // Calls GetLastError
            }

            // Create a managed callback
            getLexerName = (NativeMethods.GetLexerName)Marshal.GetDelegateForFunctionPointer(
                functionPointer, typeof(NativeMethods.GetLexerName));
        }

        #region DllCalls
        private static NativeMethods.CreateLexer createLexer;

        private static NativeMethods.GetLexerName getLexerName;
        #endregion

        #region DllWrapping
        internal static IntPtr CreateLexer(string lexerName)
        {
            return createLexer(lexerName);
        }

        internal static string GetLexerName(int index)
        {
            var pointer = Marshal.AllocHGlobal(1024);
            try
            {
                getLexerName((UIntPtr) index, pointer, (IntPtr) 1024);
                return Marshal.PtrToStringAnsi(pointer);
            }
            finally
            {
                Marshal.FreeHGlobal(pointer);
            }
        }
        #endregion
    }
}
