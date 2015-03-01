using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ScintillaNET
{
    /// <summary>
    /// Represents a Scintilla editor control.
    /// </summary>
    [Docking(DockingBehavior.Ask)]
    public class Scintilla : Control
    {
        #region Fields

        // Static module data
        private static string modulePath;
        private static IntPtr moduleHandle;
        private static NativeMethods.Scintilla_DirectFunction directFunction;

        // Events
        private static readonly object scNotificationEventKey = new object();
        private static readonly object insertCheckEventKey = new object();
        private static readonly object beforeInsertEventKey = new object();
        private static readonly object beforeDeleteEventKey = new object();
        private static readonly object insertEventKey = new object();
        private static readonly object deleteEventKey = new object();
        private static readonly object updateUIEventKey = new object();
        private static readonly object modifyAttemptEventKey = new object();
        private static readonly object styleNeededEventKey = new object();
        private static readonly object savePointReachedEventKey = new object();
        private static readonly object savePointLeftEventKey = new object();

        // The goods
        private IntPtr sciPtr;
        private BorderStyle borderStyle;

        private int stylingPosition;
        private int stylingBytePosition;

        // Modified event optimization
        private int? cachedPosition = null;
        private string cachedText = null;

        #endregion Fields

        #region Methods

        /// <summary>
        /// Inserts the specified text at the current caret position.
        /// </summary>
        /// <param name="text">The text to insert at the current caret position.</param>
        /// <remarks>The caret position is set to the end of the inserted text, but it is not scrolled into view.</remarks>
        public unsafe void AddText(string text)
        {
            var bytes = Helpers.GetBytes(text ?? string.Empty, Encoding, zeroTerminated: false);
            fixed (byte* bp = bytes)
                DirectMessage(NativeMethods.SCI_ADDTEXT, new IntPtr(bytes.Length), new IntPtr(bp));
        }

        /// <summary>
        /// Adds the specified text to the end of the document.
        /// </summary>
        /// <param name="text">The text to add to the document.</param>
        /// <remarks>The current selection is not changed and the new text is not scrolled into view.</remarks>
        public unsafe void AppendText(string text)
        {
            var bytes = Helpers.GetBytes(text ?? string.Empty, Encoding, zeroTerminated: false);
            fixed (byte* bp = bytes)
                DirectMessage(NativeMethods.SCI_APPENDTEXT, new IntPtr(bytes.Length), new IntPtr(bp));
        }

        /// <summary>
        /// Marks the beginning of a set of actions that should be treated as a single undo action.
        /// </summary>
        /// <remarks>A call to <see cref="BeginUndoAction" /> should be followed by a call to <see cref="EndUndoAction" />.</remarks>
        /// <seealso cref="EndUndoAction" />
        public void BeginUndoAction()
        {
            DirectMessage(NativeMethods.SCI_BEGINUNDOACTION);
        }

        /// <summary>
        /// Removes the selected text from the document.
        /// </summary>
        public void Clear()
        {
            DirectMessage(NativeMethods.SCI_CLEAR);
        }

        /// <summary>
        /// Deletes all document text, unless the document is read-only.
        /// </summary>
        public void ClearAll()
        {
            DirectMessage(NativeMethods.SCI_CLEARALL);
        }

        /// <summary>
        /// Removes all styling from the document and resets the folding state.
        /// </summary>
        public void ClearDocumentStyle()
        {
            DirectMessage(NativeMethods.SCI_CLEARDOCUMENTSTYLE);
        }

        /// <summary>
        /// Copies the selected text from the document and places it on the clipboard.
        /// </summary>
        public void Copy()
        {
            DirectMessage(NativeMethods.SCI_COPY);
        }

        /// <summary>
        /// Copies the selected text from the document and places it on the clipboard.
        /// If the selection is empty the current line is copied.
        /// </summary>
        /// <remarks>
        /// If the selection is empty and the current line copied, an extra "MSDEVLineSelect" marker is added to the
        /// clipboard which is then used in <see cref="Paste" /> to paste the whole line before the current line.
        /// </remarks>
        public void CopyAllowLine()
        {
            DirectMessage(NativeMethods.SCI_COPYALLOWLINE);
        }

        /// <summary>
        /// Copies the specified range of text to the clipboard.
        /// </summary>
        /// <param name="start">The zero-based character position in the document to start copying.</param>
        /// <param name="length">The zero-based character position (exclusive) in the document to stop copying.</param>
        public void CopyRange(int start, int end)
        {
            var textLength = TextLength;
            start = Helpers.Clamp(start, 0, textLength);
            end = Helpers.Clamp(end, 0, textLength);

            // Convert to byte positions
            start = Lines.CharToBytePosition(start);
            end = Lines.CharToBytePosition(end);

            DirectMessage(NativeMethods.SCI_COPYRANGE, new IntPtr(start), new IntPtr(end));
        }

        /// <summary>
        /// Cuts the selected text from the document and places it on the clipboard.
        /// </summary>
        public void Cut()
        {
            DirectMessage(NativeMethods.SCI_CUT);
        }

        /// <summary>
        /// Deletes a range of text from the document.
        /// </summary>
        /// <param name="position">The zero-based character position to start deleting.</param>
        /// <param name="length">The number of characters to delete.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="position" /> or <paramref name="length" /> is less than zero. -or-
        /// The sum of <paramref name="position" /> and <paramref name="length" /> is greater than the document length.
        /// </exception>
        public void DeleteRange(int position, int length)
        {
            var textLength = TextLength;

            if (position < 0)
                throw new ArgumentOutOfRangeException("position", "Position cannot be less than zero.");
            if (position > textLength)
                throw new ArgumentOutOfRangeException("position", "Position cannot exceed document length.");
            if (length < 0)
                throw new ArgumentOutOfRangeException("length", "Length cannot be less than zero.");
            if (position + length > textLength)
                throw new ArgumentOutOfRangeException("length", "Position and length must refer to a range within the document.");

            // Convert to byte position/length
            var byteStartPos = Lines.CharToBytePosition(position);
            var byteEndPos = Lines.CharToBytePosition(position + length);
            DirectMessage(NativeMethods.SCI_DELETERANGE, new IntPtr(byteStartPos), new IntPtr(byteEndPos - byteStartPos));
        }

        /// <summary>
        /// Retreives a description of keyword sets supported by the current <see cref="Lexer" />.
        /// </summary>
        /// <returns>A String describing each keyword set separated by line breaks for the current lexer.</returns>
        public unsafe string DescribeKeywordSets()
        {
            var length = DirectMessage(NativeMethods.SCI_DESCRIBEKEYWORDSETS).ToInt32();
            var bytes = new byte[length + 1];

            fixed (byte* bp = bytes)
                DirectMessage(NativeMethods.SCI_DESCRIBEKEYWORDSETS, IntPtr.Zero, new IntPtr(bp));

            var str = Encoding.ASCII.GetString(bytes, 0, length);
            return str;
        }

        internal IntPtr DirectMessage(int msg)
        {
            return DirectMessage(msg, IntPtr.Zero, IntPtr.Zero);
        }

        internal IntPtr DirectMessage(int msg, IntPtr wParam)
        {
            return DirectMessage(msg, wParam, IntPtr.Zero);
        }

        /// <summary>
        /// Sends the specified message directly to the native Scintilla window,
        /// bypassing any managed APIs.
        /// </summary>
        /// <param name="msg">The message ID.</param>
        /// <param name="wParam">The message <c>wparam</c> field.</param>
        /// <param name="lParam">The message <c>lparam</c> field.</param>
        /// <returns>An <see cref="IntPtr"/> representing the result of the message request.</returns>
        /// <remarks>This API supports the Scintilla infrastructure and is not intended to be used directly from your code.</remarks>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public virtual IntPtr DirectMessage(int msg, IntPtr wParam, IntPtr lParam)
        {
            // If the control handle, ptr, direct function, etc... hasn't been created yet, it will be now.
            var result = DirectMessage(SciPointer, msg, wParam, lParam);
            return result;
        }

        private static IntPtr DirectMessage(IntPtr sciPtr, int msg, IntPtr wParam, IntPtr lParam)
        {
            // Like Win32 SendMessage but directly to Scintilla
            var result = directFunction(sciPtr, msg, wParam, lParam);
            return result;
        }

        /// <summary>
        /// Clears any undo or redo history.
        /// </summary>
        /// <remarks>This will also cause <see cref="SetSavePoint" /> to be called but will not raise the <see cref="SavePointReached" /> event.</remarks>
        public void EmptyUndoBuffer()
        {
            DirectMessage(NativeMethods.SCI_EMPTYUNDOBUFFER);
        }

        /// <summary>
        /// Marks the end of a set of actions that should be treated as a single undo action.
        /// </summary>
        /// <seealso cref="BeginUndoAction" />
        public void EndUndoAction()
        {
            DirectMessage(NativeMethods.SCI_ENDUNDOACTION);
        }

        /// <summary>
        /// Returns the last document position likely to be styled correctly.
        /// </summary>
        /// <returns>The zero-based document position of the last styled character.</returns>
        public int GetEndStyled()
        {
            var pos = DirectMessage(NativeMethods.SCI_GETENDSTYLED).ToInt32();
            return Lines.ByteToCharPosition(pos);
        }

        private static string GetModulePath()
        {
            // UI thread...
            if (modulePath == null)
            {
                // Extract the embedded SciLexer DLL
                // http://stackoverflow.com/a/768429/2073621
                var version = typeof(Scintilla).Assembly.GetName().Version.ToString(3);
                modulePath = Path.Combine(Path.GetTempPath(), "ScintillaNET", version, (IntPtr.Size == 4 ? "x86" : "x64"), "SciLexer.dll");

                if (!File.Exists(modulePath))
                {
                    // http://stackoverflow.com/a/229567/2073621
                    // Synchronize access to the file across processes
                    var guid = ((GuidAttribute)typeof(Scintilla).Assembly.GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value.ToString();
                    var name = string.Format(CultureInfo.InvariantCulture, "Global\\{{{0}}}", guid);
                    using (var mutex = new Mutex(false, name))
                    {
                        var access = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
                        var security = new MutexSecurity();
                        security.AddAccessRule(access);
                        mutex.SetAccessControl(security);

                        var ownsHandle = false;
                        try
                        {
                            try
                            {
                                ownsHandle = mutex.WaitOne(5000, false); // 5 sec
                                if (ownsHandle == false)
                                {
                                    var timeoutMessage = string.Format(CultureInfo.InvariantCulture, "Timeout waiting for exclusive access to '{0}'.", modulePath);
                                    throw new TimeoutException(timeoutMessage);
                                }
                            }
                            catch (AbandonedMutexException)
                            {
                                // Previous process terminated abnormally
                                ownsHandle = true;
                            }

                            // Double-checked (process) lock
                            if (!File.Exists(modulePath))
                            {
                                // Write the embedded file to disk
                                var directory = Path.GetDirectoryName(modulePath);
                                if (!Directory.Exists(directory))
                                    Directory.CreateDirectory(directory);

                                var resource = string.Format(CultureInfo.InvariantCulture, "ScintillaNET.{0}.SciLexer.dll", (IntPtr.Size == 4 ? "x86" : "x64"));
                                var resourceStream = typeof(Scintilla).Assembly.GetManifestResourceStream(resource); // Don't close the resource stream
                                using (var fileStream = File.Create(modulePath))
                                    resourceStream.CopyTo(fileStream);
                            }
                        }
                        finally
                        {
                            if (ownsHandle)
                                mutex.ReleaseMutex();
                        }
                    }
                }
            }

            return modulePath;
        }

        /// <summary>
        /// Gets a range of text from the document.
        /// </summary>
        /// <param name="position">The zero-based starting character position of the range to get.</param>
        /// <param name="length">The number of characters to get.</param>
        /// <returns>A string representing the text range.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="position" /> or <paramref name="length" /> is less than zero. -or-
        /// The sum of <paramref name="position" /> and <paramref name="length" /> is greater than the document length.
        /// </exception>
        public unsafe string GetTextRange(int position, int length)
        {
            var textLength = TextLength;

            if (position < 0)
                throw new ArgumentOutOfRangeException("position", "Position cannot be less than zero.");
            if (position > textLength)
                throw new ArgumentOutOfRangeException("position", "Position cannot exceed document length.");
            if (length < 0)
                throw new ArgumentOutOfRangeException("length", "Length cannot be less than zero.");
            if (position + length > textLength)
                throw new ArgumentOutOfRangeException("length", "Position and length must refer to a range within the document.");

            // Convert to byte position/length
            var byteStartPos = Lines.CharToBytePosition(position);
            var byteEndPos = Lines.CharToBytePosition(position + length);
            var ptr = DirectMessage(NativeMethods.SCI_GETRANGEPOINTER, new IntPtr(byteStartPos), new IntPtr(byteEndPos - byteStartPos));
            if (ptr == IntPtr.Zero)
                return string.Empty;

            var text = new string((sbyte*)ptr, 0, length, Encoding);
            return text;
        }

        /// <summary>
        /// Returns the version information of the native Scintilla library.
        /// </summary>
        /// <returns>An object representing the version information of the native Scintilla library.</returns>
        public FileVersionInfo GetVersionInfo()
        {
            var path = GetModulePath();
            var version = FileVersionInfo.GetVersionInfo(path);

            return version;
        }

        /// <summary>
        /// Inserts text at the specified position.
        /// </summary>
        /// <param name="position">The zero-based character position to insert the text. Specify -1 to use the current caret position.</param>
        /// <param name="text">The text to insert into the document.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="position" /> less than zero and not equal to -1. -or-
        /// <paramref name="position" /> is greater than the document length.
        /// </exception>
        /// <remarks>No scrolling is performed.</remarks>
        public unsafe void InsertText(int position, string text)
        {
            if (position < -1)
                throw new ArgumentOutOfRangeException("position", "Position must be greater or equal to zero, or -1.");

            if (position != -1)
            {
                var textLength = TextLength;
                if (position > textLength)
                    throw new ArgumentOutOfRangeException("position", "Position cannot exceed document length.");

                position = Lines.CharToBytePosition(position);
            }

            fixed (byte* bp = Helpers.GetBytes(text ?? string.Empty, Encoding, zeroTerminated: true))
                DirectMessage(NativeMethods.SCI_INSERTTEXT, new IntPtr(position), new IntPtr(bp));
        }

        /// <summary>
        /// Removes the specified marker from all lines.
        /// </summary>
        /// <param name="marker">The zero-based <see cref="Marker" /> index to remove from all lines, or -1 to remove all markers from all lines.</param>
        public void MarkerDeleteAll(int marker)
        {
            marker = Helpers.Clamp(marker, -1, Markers.Count - 1);
            DirectMessage(NativeMethods.SCI_MARKERDELETEALL, new IntPtr(marker));
        }

        /// <summary>
        /// Searches the document for the marker handle and deletes the marker if found.
        /// </summary>
        /// <param name="markerHandle">The <see cref="MarkerHandle" /> created by a previous call to <see cref="Line.MarkerAdd" /> of the marker to delete.</param>
        public void MarkerDeleteHandle(MarkerHandle markerHandle)
        {
            DirectMessage(NativeMethods.SCI_MARKERDELETEHANDLE, markerHandle.Value);
        }

        /// <summary>
        /// Searches the document for the marker handle and returns the line number containing the marker if found.
        /// </summary>
        /// <param name="markerHandle">The <see cref="MarkerHandle" /> created by a previous call to <see cref="Line.MarkerAdd" /> of the marker to search for.</param>
        /// <returns>If found, the zero-based line index containing the marker; otherwise, -1.</returns>
        public int MarkerLineFromHandle(MarkerHandle markerHandle)
        {
            return DirectMessage(NativeMethods.SCI_MARKERLINEFROMHANDLE, markerHandle.Value).ToInt32();
        }

        /// <summary>
        /// Raises the <see cref="BeforeDelete" /> event.
        /// </summary>
        /// <param name="e">A <see cref="BeforeModificationEventArgs" /> that contains the event data.</param>
        protected virtual void OnBeforeDelete(BeforeModificationEventArgs e)
        {
            var handler = Events[beforeDeleteEventKey] as EventHandler<BeforeModificationEventArgs>;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Raises the <see cref="BeforeInsert" /> event.
        /// </summary>
        /// <param name="e">A <see cref="BeforeModificationEventArgs" /> that contains the event data.</param>
        protected virtual void OnBeforeInsert(BeforeModificationEventArgs e)
        {
            var handler = Events[beforeInsertEventKey] as EventHandler<BeforeModificationEventArgs>;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Raises the <see cref="Delete" /> event.
        /// </summary>
        /// <param name="e">A <see cref="ModificationEventArgs" /> that contains the event data.</param>
        protected virtual void OnDelete(ModificationEventArgs e)
        {
            var handler = Events[deleteEventKey] as EventHandler<ModificationEventArgs>;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Raises the HandleCreated event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnHandleCreated(EventArgs e)
        {
            // Set more intelligent defaults...

            // I would like to see all of my text please
            DirectMessage(NativeMethods.SCI_SETSCROLLWIDTHTRACKING, new IntPtr(1));

            // It's pointless to do any encoding other than UTF-8 in Scintilla
            DirectMessage(NativeMethods.SCI_SETCODEPAGE, new IntPtr(NativeMethods.SC_CP_UTF8));

            // The default tab width of 8 is crazy big
            DirectMessage(NativeMethods.SCI_SETTABWIDTH, new IntPtr(4));

            base.OnHandleCreated(e);
        }

        /// <summary>
        /// Raises the <see cref="Insert" /> event.
        /// </summary>
        /// <param name="e">A <see cref="ModificationEventArgs" /> that contains the event data.</param>
        protected virtual void OnInsert(ModificationEventArgs e)
        {
            var handler = Events[insertEventKey] as EventHandler<ModificationEventArgs>;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Raises the <see cref="InsertCheck" /> event.
        /// </summary>
        /// <param name="e">An <see cref="InsertCheckEventArgs" /> that contains the event data.</param>
        protected virtual void OnInsertCheck(InsertCheckEventArgs e)
        {
            var handler = Events[insertCheckEventKey] as EventHandler<InsertCheckEventArgs>;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Raises the <see cref="ModifyAttempt" /> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs" /> that contains the event data.</param>
        protected virtual void OnModifyAttempt(EventArgs e)
        {
            var handler = Events[modifyAttemptEventKey] as EventHandler<EventArgs>;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Raises the <see cref="SavePointLeft" /> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs" /> that contains the event data.</param>
        protected virtual void OnSavePointLeft(EventArgs e)
        {
            var handler = Events[savePointLeftEventKey] as EventHandler<EventArgs>;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Raises the <see cref="SavePointReached" /> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs" /> that contains the event data.</param>
        protected virtual void OnSavePointReached(EventArgs e)
        {
            var handler = Events[savePointReachedEventKey] as EventHandler<EventArgs>;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Raises the <see cref="StyleNeeded" /> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs" /> that contains the event data.</param>
        protected virtual void OnStyleNeeded(EventArgs e)
        {
            var handler = Events[styleNeededEventKey] as EventHandler<EventArgs>;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Raises the <see cref="UpdateUI" /> event.
        /// </summary>
        /// <param name="e">An <see cref="UpdateUIEventArgs" /> that contains the event data.</param>
        protected virtual void OnUpdateUI(UpdateUIEventArgs e)
        {
            EventHandler<UpdateUIEventArgs> handler = Events[updateUIEventKey] as EventHandler<UpdateUIEventArgs>;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Pastes the contents of the clipboard into the current selection.
        /// </summary>
        public void Paste()
        {
            DirectMessage(NativeMethods.SCI_PASTE);
        }

        /// <summary>
        /// Redoes the effect of an <see cref="Undo" /> operation.
        /// </summary>
        public void Redo()
        {
            DirectMessage(NativeMethods.SCI_REDO);
        }

        /// <summary>
        /// Replaces the current selection with the specified text.
        /// </summary>
        /// <param name="text">The text that should replace the current selection.</param>
        /// <remarks>
        /// If there is not a current selection, the text will be inserted at the current caret position.
        /// Following the operation the caret is placed at the end of the inserted text and scrolled into view.
        /// </remarks>
        public unsafe void ReplaceSelection(string text)
        {
            // TODO I don't like how using a null/empty string does nothing

            fixed (byte* bp = Helpers.GetBytes(text ?? string.Empty, Encoding, zeroTerminated: true))
                DirectMessage(NativeMethods.SCI_REPLACESEL, IntPtr.Zero, new IntPtr(bp));
        }

        private void ScnModified(ref NativeMethods.SCNotification scn)
        {
            // The InsertCheck, BeforeInsert, BeforeDelete, Insert, and Delete events can all potentially require
            // the same conversions: byte to char position, char* to string, etc.... To avoid doing the same work
            // multiple times we share that data between events.

            if ((scn.modificationType & NativeMethods.SC_MOD_INSERTCHECK) > 0)
            {
                var eventArgs = new InsertCheckEventArgs(this, scn.position, scn.length, scn.text);
                OnInsertCheck(eventArgs);

                cachedPosition = eventArgs.CachedPosition;
                cachedText = eventArgs.CachedText;
            }

            const int sourceMask = (NativeMethods.SC_PERFORMED_USER | NativeMethods.SC_PERFORMED_UNDO | NativeMethods.SC_PERFORMED_REDO);

            if ((scn.modificationType & (NativeMethods.SC_MOD_BEFOREDELETE | NativeMethods.SC_MOD_BEFOREINSERT)) > 0)
            {
                var source = (ModificationSource)(scn.modificationType & sourceMask);
                var eventArgs = new BeforeModificationEventArgs(this, source, scn.position, scn.length, scn.text);

                eventArgs.CachedPosition = cachedPosition;
                eventArgs.CachedText = cachedText;

                if ((scn.modificationType & NativeMethods.SC_MOD_BEFOREINSERT) > 0)
                {
                    OnBeforeInsert(eventArgs);
                }
                else
                {
                    OnBeforeDelete(eventArgs);
                }

                cachedPosition = eventArgs.CachedPosition;
                cachedText = eventArgs.CachedText;
            }

            if ((scn.modificationType & (NativeMethods.SC_MOD_DELETETEXT | NativeMethods.SC_MOD_INSERTTEXT)) > 0)
            {
                var source = (ModificationSource)(scn.modificationType & sourceMask);
                var eventArgs = new ModificationEventArgs(this, source, scn.position, scn.length, scn.text, scn.linesAdded);

                eventArgs.CachedPosition = cachedPosition;
                eventArgs.CachedText = cachedText;

                if ((scn.modificationType & NativeMethods.SC_MOD_INSERTTEXT) > 0)
                {
                    OnInsert(eventArgs);
                }
                else
                {
                    OnDelete(eventArgs);
                }

                // Always clear the cache
                cachedPosition = null;
                cachedText = null;

                // For backward compatibility.... Of course this means that we'll raise two
                // TextChanged events for replace (insert/delete) operations, but that's life.
                OnTextChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Scrolls the current position into view, if it is not already visible.
        /// </summary>
        public void ScrollCaret()
        {
            DirectMessage(NativeMethods.SCI_SCROLLCARET);
        }

        /// <summary>
        /// Searches for the first occurrence of the specified pattern in the target defined by <see cref="TargetStart" /> and <see cref="TargetEnd" />.
        /// </summary>
        /// <param name="pattern">The text pattern to search for. The interpretation of the pattern is defined by the <see cref="SearchFlags" />.</param>
        /// <returns>The zero-based start position of the matched text within the document if successful; otherwise, -1.</returns>
        /// <remarks>If successful, the <see cref="TargetStart" /> and <see cref="TargetEnd" /> properties will be updated to start and end positions of the matched text.</remarks>
        public unsafe int SearchInTarget(string pattern)
        {
            int bytePos = 0;
            var bytes = Helpers.GetBytes(pattern ?? string.Empty, Encoding, zeroTerminated: false);
            fixed (byte* bp = bytes)
                bytePos = DirectMessage(NativeMethods.SCI_SEARCHINTARGET, new IntPtr(bytes.Length), new IntPtr(bp)).ToInt32();

            if (bytePos == -1)
                return bytePos;

            return Lines.ByteToCharPosition(bytePos);
        }

        /// <summary>
        /// Updates a keyword set used by the current <see cref="Lexer" />.
        /// </summary>
        /// <param name="set">The zero-based index of the keyword set to update.</param>
        /// <param name="keywords">
        /// A list of keywords pertaining to the current <see cref="Lexer" /> separated by whitespace (space, tab, '\n', '\r') characters.
        /// </param>
        /// <remarks>The keywords specified will be styled according to the current <see cref="Lexer" />.</remarks>
        /// <seealso cref="DescribeKeywordSets" />
        public unsafe void SetKeywords(int set, string keywords)
        {
            set = Helpers.Clamp(set, 0, NativeMethods.KEYWORDSET_MAX);
            var bytes = Helpers.GetBytes(keywords ?? string.Empty, Encoding.ASCII, zeroTerminated: true);

            fixed (byte* bp = bytes)
                DirectMessage(NativeMethods.SCI_SETKEYWORDS, new IntPtr(set), new IntPtr(bp));
        }

        /// <summary>
        /// Sets the application-wide default module path of the native Scintilla library.
        /// </summary>
        /// <param name="modulePath">The native Scintilla module path.</param>
        /// <remarks>
        /// This method must be called prior to the first <see cref="Scintilla" /> control being created.
        /// The <paramref name="modulePath" /> can be relative or absolute.
        /// </remarks>
        public static void SetModulePath(string modulePath)
        {
            if (Scintilla.modulePath == null)
            {
                Scintilla.modulePath = modulePath;
            }
        }

        /// <summary>
        /// Marks the document as unmodified.
        /// </summary>
        /// <seealso cref="Modified" />
        public void SetSavePoint()
        {
            DirectMessage(NativeMethods.SCI_SETSAVEPOINT);
        }

        /// <summary>
        /// Sets a global override to the selection background color.
        /// </summary>
        /// <param name="use">true to override the selection background color; otherwise, false.</param>
        /// <param name="color">The global selection background color.</param>
        /// <seealso cref="SetSelectionForeColor" />
        public void SetSelectionBackColor(bool use, Color color)
        {
            var colour = ColorTranslator.ToWin32(color);
            var useSelectionForeColour = (use ? new IntPtr(1) : IntPtr.Zero);

            DirectMessage(NativeMethods.SCI_SETSELBACK, useSelectionForeColour, new IntPtr(colour));
        }

        /// <summary>
        /// Sets a global override to the selection foreground color.
        /// </summary>
        /// <param name="use">true to override the selection foreground color; otherwise, false.</param>
        /// <param name="color">The global selection foreground color.</param>
        /// <seealso cref="SetSelectionBackColor" />
        public void SetSelectionForeColor(bool use, Color color)
        {
            var colour = ColorTranslator.ToWin32(color);
            var useSelectionForeColour = (use ? new IntPtr(1) : IntPtr.Zero);

            DirectMessage(NativeMethods.SCI_SETSELFORE, useSelectionForeColour, new IntPtr(colour));
        }

        /// <summary>
        /// Styles the specified length of characters.
        /// </summary>
        /// <param name="length">The number of characters to style.</param>
        /// <param name="style">The <see cref="Style" /> definition index to assign each character.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="length" /> or <paramref name="style" /> is less than zero. -or-
        /// The sum of a preceeding call to <see cref="StartStyling" /> or <see name="SetStyling" /> and <paramref name="length" /> is greater than the document length. -or-
        /// <paramref name="style" /> is greater than or equal to the number of style definitions.
        /// </exception>
        /// <remarks>
        /// The styling position is advanced by <paramref name="length" /> after each call allowing multiple
        /// calls to <see cref="SetStyling" /> for a single call to <see cref="StartStyling" />.
        /// </remarks>
        /// <seealso cref="StartStyling" />
        public void SetStyling(int length, int style)
        {
            var textLength = TextLength;

            if (length < 0)
                throw new ArgumentOutOfRangeException("length", "Length cannot be less than zero.");
            if (stylingPosition + length > textLength)
                throw new ArgumentOutOfRangeException("length", "Position and length must refer to a range within the document.");
            if (style < 0 || style >= Styles.Count)
                throw new ArgumentOutOfRangeException("style", "Style must be non-negative and less than the size of the collection.");

            var endPos = stylingPosition + length;
            var endBytePos = Lines.CharToBytePosition(endPos);
            DirectMessage(NativeMethods.SCI_SETSTYLING, new IntPtr(endBytePos - stylingPosition), new IntPtr(style));

            // Track this for the next call
            stylingPosition = endPos;
            stylingBytePosition = endBytePos;
        }

        /// <summary>
        /// Sets a global override to the whitespace background color.
        /// </summary>
        /// <param name="use">true to override the whitespace background color; otherwise, false.</param>
        /// <param name="color">The global whitespace background color.</param>
        /// <remarks>When not overridden globally, the whitespace background color is determined by the current lexer.</remarks>
        /// <seealso cref="ViewWhitespace" />
        /// <seealso cref="SetWhitespaceForeColor" />
        public void SetWhitespaceBackColor(bool use, Color color)
        {
            var colour = ColorTranslator.ToWin32(color);
            var useWhitespaceBackColour = (use ? new IntPtr(1) : IntPtr.Zero);

            DirectMessage(NativeMethods.SCI_SETWHITESPACEBACK, useWhitespaceBackColour, new IntPtr(colour));
        }

        /// <summary>
        /// Sets a global override to the whitespace foreground color.
        /// </summary>
        /// <param name="use">true to override the whitespace foreground color; otherwise, false.</param>
        /// <param name="color">The global whitespace foreground color.</param>
        /// <remarks>When not overridden globally, the whitespace foreground color is determined by the current lexer.</remarks>
        /// <seealso cref="ViewWhitespace" />
        /// <seealso cref="SetWhitespaceBackColor" />
        public void SetWhitespaceForeColor(bool use, Color color)
        {
            var colour = ColorTranslator.ToWin32(color);
            var useWhitespaceForeColour = (use ? new IntPtr(1) : IntPtr.Zero);

            DirectMessage(NativeMethods.SCI_SETWHITESPACEFORE, useWhitespaceForeColour, new IntPtr(colour));
        }

        /// <summary>
        /// Prepares for styling by setting the styling <paramref name="position" /> to start at.
        /// </summary>
        /// <param name="position">The zero-based character position in the document to start styling.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="position" /> is less than zero. -or-
        /// <paramref name="position" /> is greater than the document length.
        /// </exception>
        /// <remarks>
        /// After preparing the document for styling, use successive calls to <see cref="SetStyling" />
        /// to style the document.
        /// </remarks>
        /// <seealso cref="SetStyling" />
        public void StartStyling(int position)
        {
            var textLength = TextLength;

            if (position < 0)
                throw new ArgumentOutOfRangeException("position", "Position cannot be less than zero.");
            if (position > textLength)
                throw new ArgumentOutOfRangeException("position", "Position cannot exceed document length.");

            var pos = Lines.CharToBytePosition(position);
            DirectMessage(NativeMethods.SCI_STARTSTYLING, new IntPtr(pos));

            // Track this so we can validate calls to SetStyling
            stylingPosition = position;
            stylingBytePosition = pos;
        }

        /// <summary>
        /// Resets all style properties to those currently configured for the <see cref="Style.Default" /> style.
        /// </summary>
        /// <seealso cref="StyleResetDefault" />
        public void StyleClearAll()
        {
            DirectMessage(NativeMethods.SCI_STYLECLEARALL);
        }

        /// <summary>
        /// Resets the <see cref="Style.Default" /> style to its initial state.
        /// </summary>
        /// <seealso cref="StyleClearAll" />
        public void StyleResetDefault()
        {
            DirectMessage(NativeMethods.SCI_STYLERESETDEFAULT);
        }

        /// <summary>
        /// Sets the <see cref="TargetStart" /> and <see cref="TargetEnd" /> to the start and end positions of the selection.
        /// </summary>
        public void TargetFromSelection()
        {
            DirectMessage(NativeMethods.SCI_TARGETFROMSELECTION);
        }

        /// <summary>
        /// Measures the width in pixels of the specified string when rendered in the specified style.
        /// </summary>
        /// <param name="style">The index of the <see cref="Style" /> to use when rendering the text to measure.</param>
        /// <param name="text">The text to measure.</param>
        /// <returns>The width in pixels.</returns>
        public unsafe int TextWidth(int style, string text)
        {
            style = Helpers.Clamp(style, 0, Styles.Count - 1);
            var bytes = Helpers.GetBytes(text ?? string.Empty, Encoding, zeroTerminated: true);

            fixed (byte* bp = bytes)
            {
                return DirectMessage(NativeMethods.SCI_TEXTWIDTH, new IntPtr(style), new IntPtr(bp)).ToInt32();
            }
        }

        /// <summary>
        /// Undoes the previous action.
        /// </summary>
        public void Undo()
        {
            DirectMessage(NativeMethods.SCI_UNDO);
        }

        private void WmReflectNotify(ref Message m)
        {
            // A standard Windows notification and a Scintilla notification header are compatible
            NativeMethods.SCNotification scn = (NativeMethods.SCNotification)Marshal.PtrToStructure(m.LParam, typeof(NativeMethods.SCNotification));
            if (scn.nmhdr.code >= NativeMethods.SCN_STYLENEEDED && scn.nmhdr.code <= NativeMethods.SCN_FOCUSOUT)
            {
                var handler = Events[scNotificationEventKey] as EventHandler<SCNotificationEventArgs>;
                if (handler != null)
                    handler(this, new SCNotificationEventArgs(scn));

                switch (scn.nmhdr.code)
                {
                    case NativeMethods.SCN_MODIFIED:
                        ScnModified(ref scn);
                        break;

                    case NativeMethods.SCN_MODIFYATTEMPTRO:
                        OnModifyAttempt(EventArgs.Empty);
                        break;

                    case NativeMethods.SCN_STYLENEEDED:
                        OnStyleNeeded(EventArgs.Empty);
                        break;

                    case NativeMethods.SCN_SAVEPOINTLEFT:
                        OnSavePointLeft(EventArgs.Empty);
                        break;

                    case NativeMethods.SCN_SAVEPOINTREACHED:
                        OnSavePointReached(EventArgs.Empty);
                        break;

                    case NativeMethods.SCN_UPDATEUI:
                        OnUpdateUI(new UpdateUIEventArgs((UpdateChange)scn.updated));
                        break;

                    default:
                        // Not our notification
                        base.WndProc(ref m);
                        break;
                }
            }
        }

        /// <summary>
        /// Processes Windows messages.
        /// </summary>
        /// <param name="m">The Windows Message to process.</param>
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case (NativeMethods.WM_REFLECT + NativeMethods.WM_NOTIFY):
                    WmReflectNotify(ref m);
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        /// <summary>
        /// Increases the zoom factor by 1 until it reaches 20 points.
        /// </summary>
        /// <seealso cref="Zoom" />
        public void ZoomIn()
        {
            DirectMessage(NativeMethods.SCI_ZOOMIN);
        }

        /// <summary>
        /// Decreases the zoom factor by 1 until it reaches -10 points.
        /// </summary>
        /// <seealso cref="Zoom" />
        public void ZoomOut()
        {
            DirectMessage(NativeMethods.SCI_ZOOMOUT);
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Gets or sets the current anchor position.
        /// </summary>
        /// <returns>The zero-based character position of the anchor.</returns>
        /// <remarks>
        /// Setting the current anchor position will create a selection between it and the <see cref="CurrentPosition" />.
        /// The caret is not scrolled into view.
        /// </remarks>
        /// <seealso cref="ScrollCaret" />
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int AnchorPosition
        {
            get
            {
                var bytePos = DirectMessage(NativeMethods.SCI_GETANCHOR).ToInt32();
                return Lines.ByteToCharPosition(bytePos);
            }
            set
            {
                var textLength = TextLength;
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "Value cannot be less than zero.");
                if (value > textLength)
                    throw new ArgumentOutOfRangeException("value", "Value cannot exceed document length.");

                var bytePos = Lines.CharToBytePosition(value);
                DirectMessage(NativeMethods.SCI_SETANCHOR, new IntPtr(bytePos));
            }
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                base.BackColor = value;
            }
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Image BackgroundImage
        {
            get
            {
                return base.BackgroundImage;
            }
            set
            {
                base.BackgroundImage = value;
            }
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override ImageLayout BackgroundImageLayout
        {
            get
            {
                return base.BackgroundImageLayout;
            }
            set
            {
                base.BackgroundImageLayout = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether there is text on the clipboard that can be pasted into the document.
        /// </summary>
        /// <returns>true when there is text on the clipboard to paste; otherwise, false.</returns>
        /// <remarks>The document cannot be <see cref="ReadOnly" />  and the selection cannot contain protected text.</remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CanPaste
        {
            get
            {
                return (DirectMessage(NativeMethods.SCI_CANPASTE) != IntPtr.Zero);
            }
        }

        /// <summary>
        /// Gets a value indicating whether there is an undo action to redo.
        /// </summary>
        /// <returns>true when there is something to redo; otherwise, false.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CanRedo
        {
            get
            {
                return (DirectMessage(NativeMethods.SCI_CANREDO) != IntPtr.Zero);
            }
        }

        /// <summary>
        /// Gets a value indicating whether there is an action to undo.
        /// </summary>
        /// <returns>true when there is something to undo; otherwise, false.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CanUndo
        {
            get
            {
                return (DirectMessage(NativeMethods.SCI_CANUNDO) != IntPtr.Zero);
            }
        }

        /// <summary>
        /// Gets or sets the caret foreground color.
        /// </summary>
        /// <returns>The caret foreground color. The default is black.</returns>
        [DefaultValue(typeof(Color), "Black")]
        [Category("Caret")]
        [Description("The caret foreground color.")]
        public Color CaretForeColor
        {
            get
            {
                var color = DirectMessage(NativeMethods.SCI_GETCARETFORE).ToInt32();
                return ColorTranslator.FromWin32(color);
            }
            set
            {
                var color = ColorTranslator.ToWin32(value);
                DirectMessage(NativeMethods.SCI_SETCARETFORE, new IntPtr(color));
            }
        }

        /// <summary>
        /// Gets or sets the caret line background color.
        /// </summary>
        /// <returns>The caret line background color. The default is yellow.</returns>
        [DefaultValue(typeof(Color), "Yellow")]
        [Category("Caret")]
        [Description("The background color of the current line.")]
        public Color CaretLineBackColor
        {
            get
            {
                var color = DirectMessage(NativeMethods.SCI_GETCARETLINEBACK).ToInt32();
                return ColorTranslator.FromWin32(color);
            }
            set
            {
                var color = ColorTranslator.ToWin32(value);
                DirectMessage(NativeMethods.SCI_SETCARETLINEBACK, new IntPtr(color));
            }
        }

        /// <summary>
        /// Gets or sets the alpha transparency of the <see cref="CaretLineBackColor" />.
        /// </summary>
        /// <returns>
        /// The alpha transparency ranging from 0 (completely transparent) to 255 (completely opaque).
        /// The value 256 will disable alpha transparency. The default is 256.
        /// </returns>
        [DefaultValue(256)]
        [Category("Caret")]
        [Description("The transparency of the current line background color.")]
        public int CaretLineBackColorAlpha
        {
            get
            {
                return DirectMessage(NativeMethods.SCI_GETCARETLINEBACKALPHA).ToInt32();
            }
            set
            {
                value = Helpers.Clamp(value, 0, NativeMethods.SC_ALPHA_NOALPHA);
                DirectMessage(NativeMethods.SCI_SETCARETLINEBACKALPHA, new IntPtr(value));
            }
        }

        /// <summary>
        /// Gets or sets whether the caret line is visible (highlighted).
        /// </summary>
        /// <returns>true if the caret line is visible; otherwise, false. The default is false.</returns>
        [DefaultValue(false)]
        [Category("Caret")]
        [Description("Determines whether to highlight the current caret line.")]
        public bool CaretLineVisible
        {
            get
            {
                return (DirectMessage(NativeMethods.SCI_GETCARETLINEVISIBLE) != IntPtr.Zero);
            }
            set
            {
                var visible = (value ? new IntPtr(1) : IntPtr.Zero);
                DirectMessage(NativeMethods.SCI_SETCARETLINEVISIBLE, visible);
            }
        }

        /// <summary>
        /// Gets or sets the caret blink rate in milliseconds.
        /// </summary>
        /// <returns>The caret blink rate measured in milliseconds. The default is 530.</returns>
        /// <remarks>A value of 0 will stop the caret blinking.</remarks>
        [DefaultValue(530)]
        [Category("Caret")]
        [Description("The caret blink rate in milliseconds.")]
        public int CaretPeriod
        {
            get
            {
                return DirectMessage(NativeMethods.SCI_GETCARETPERIOD).ToInt32();
            }
            set
            {
                value = Helpers.ClampMin(value, 0);
                DirectMessage(NativeMethods.SCI_SETCARETPERIOD, new IntPtr(value));
            }
        }

        /// <summary>
        /// Gets or sets the caret display style.
        /// </summary>
        /// <returns>
        /// One of the <see cref="ScintillaNET.CaretStyle" /> enumeration values.
        /// The default is <see cref="ScintillaNET.CaretStyle.Line" />.
        /// </returns>
        [DefaultValue(CaretStyle.Line)]
        [Category("Caret")]
        [Description("The caret display style.")]
        public CaretStyle CaretStyle
        {
            get
            {
                return (CaretStyle)DirectMessage(NativeMethods.SCI_GETCARETSTYLE).ToInt32();
            }
            set
            {
                var style = (int)value;
                DirectMessage(NativeMethods.SCI_SETCARETSTYLE, new IntPtr(style));
            }
        }

        /// <summary>
        /// Gets or sets the width in pixels of the caret.
        /// </summary>
        /// <returns>The width of the caret in pixels. The default is 1 pixel.</returns>
        /// <remarks>
        /// The caret width can only be set to a value of 0, 1, 2 or 3 pixels and is only effective
        /// when the <see cref="CaretStyle" /> property is set to <see cref="ScintillaNET.CaretStyle.Line" />.
        /// </remarks>
        [DefaultValue(1)]
        [Category("Caret")]
        [Description("The width of the caret line measured in pixels (between 0 and 3).")]
        public int CaretWidth
        {
            get
            {
                return DirectMessage(NativeMethods.SCI_GETCARETWIDTH).ToInt32();
            }
            set
            {
                value = Helpers.Clamp(value, 0, 3);
                DirectMessage(NativeMethods.SCI_SETCARETWIDTH, new IntPtr(value));
            }
        }

        /// <summary>
        /// Gets the required creation parameters when the control handle is created.
        /// </summary>
        /// <returns>A CreateParams that contains the required creation parameters when the handle to the control is created.</returns>
        protected override CreateParams CreateParams
        {
            get
            {
                if (moduleHandle == IntPtr.Zero)
                {
                    var path = GetModulePath();

                    // Load the native Scintilla library
                    moduleHandle = NativeMethods.LoadLibrary(path);
                    if (moduleHandle == IntPtr.Zero)
                    {
                        var message = string.Format(CultureInfo.InvariantCulture, "Could not load the Scintilla module at the path '{0}'.", path);
                        throw new Win32Exception(message, new Win32Exception()); // Will GetLastError
                    }

                    // Get the native Scintilla direct function -- the only function the library exports
                    var directFunctionPointer = NativeMethods.GetProcAddress(new HandleRef(this, moduleHandle), "Scintilla_DirectFunction");
                    if (directFunctionPointer == IntPtr.Zero)
                    {
                        var message = "The Scintilla module has no export for the 'Scintilla_DirectFunction' procedure.";
                        throw new Win32Exception(message, new Win32Exception()); // Will GetLastError
                    }

                    // Create a managed callback
                    directFunction = (NativeMethods.Scintilla_DirectFunction)Marshal.GetDelegateForFunctionPointer(
                        directFunctionPointer,
                        typeof(NativeMethods.Scintilla_DirectFunction));
                }

                CreateParams cp = base.CreateParams;
                cp.ClassName = "Scintilla";

                // The border effect is achieved through a native Windows style
                cp.ExStyle &= (~NativeMethods.WS_EX_CLIENTEDGE);
                cp.Style &= (~NativeMethods.WS_BORDER);
                switch (borderStyle)
                {
                    case BorderStyle.Fixed3D:
                        cp.ExStyle |= NativeMethods.WS_EX_CLIENTEDGE;
                        break;
                    case BorderStyle.FixedSingle:
                        cp.Style |= NativeMethods.WS_BORDER;
                        break;
                }

                return cp;
            }
        }

        /// <summary>
        /// Gets or sets the current caret position.
        /// </summary>
        /// <returns>The zero-based character position of the caret.</returns>
        /// <remarks>
        /// Setting the current caret position will create a selection between it and the current <see cref="AnchorPosition" />.
        /// The caret is not scrolled into view.
        /// </remarks>
        /// <seealso cref="ScrollCaret" />
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CurrentPosition
        {
            get
            {
                var bytePos = DirectMessage(NativeMethods.SCI_GETCURRENTPOS).ToInt32();
                return Lines.ByteToCharPosition(bytePos);
            }
            set
            {
                var textLength = TextLength;
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "Value cannot be less than zero.");
                if (value > textLength)
                    throw new ArgumentOutOfRangeException("value", "Value cannot exceed document length.");

                var bytePos = Lines.CharToBytePosition(value);
                DirectMessage(NativeMethods.SCI_SETCURRENTPOS, new IntPtr(bytePos));
            }
        }

        /// <summary>
        /// Gets or sets the default cursor for the control.
        /// </summary>
        /// <returns>An object of type Cursor representing the current default cursor.</returns>
        protected override Cursor DefaultCursor
        {
            get
            {
                return Cursors.IBeam;
            }
        }

        /// <summary>
        /// Gets the default size of the control.
        /// </summary>
        /// <returns>The default Size of the control.</returns>
        protected override Size DefaultSize
        {
            get
            {
                // I've discovered that using a DefaultSize property other than 'empty' triggers a flaw (IMO)
                // in Windows Forms that will cause CreateParams to be called in the base constructor.
                // That's too early. It makes it impossible to use the Site or DesignMode properties during
                // handle creation because they haven't been set yet. Since we don't currently depend on those
                // properties it's okay, but if we need them this is the place to start fixing things.

                return new Size(200, 100);
            }
        }

        internal Encoding Encoding
        {
            get
            {
                // Should always be UTF-8 unless someone has done an end run around us
                int codePage = (int)DirectMessage(NativeMethods.SCI_GETCODEPAGE);
                return (codePage == 0 ? Encoding.Default : Encoding.GetEncoding(codePage));
            }
        }

        /// <summary>
        /// Gets or sets whether vertical scrolling ends at the last line or can scroll past.
        /// </summary>
        /// <returns>true if the maximum vertical scroll position ends at the last line; otherwise, false. The default is true.</returns>
        [DefaultValue(true)]
        [Category("Scrolling")]
        [Description("Determines whether the maximum vertical scroll position ends at the last line or can scroll past.")]
        public bool EndAtLastLine
        {
            get
            {
                return (DirectMessage(NativeMethods.SCI_GETENDATLASTLINE) != IntPtr.Zero);
            }
            set
            {
                var endAtLastLine = (value ? new IntPtr(1) : IntPtr.Zero);
                DirectMessage(NativeMethods.SCI_SETENDATLASTLINE, endAtLastLine);
            }
        }

        /// <summary>
        /// Gets or sets the background color to use when indicating long lines with
        /// <see cref="ScintillaNET.EdgeMode.Background" />.
        /// </summary>
        /// <returns>The background Color. The default is Silver.</returns>
        [DefaultValue(typeof(Color), "Silver")]
        [Category("Long Lines")]
        [Description("The background color to use when indicating long lines.")]
        public Color EdgeColor
        {
            get
            {
                var color = DirectMessage(NativeMethods.SCI_GETEDGECOLOUR).ToInt32();
                return ColorTranslator.FromWin32(color);
            }
            set
            {
                var color = ColorTranslator.ToWin32(value);
                DirectMessage(NativeMethods.SCI_SETEDGECOLOUR, new IntPtr(color));
            }
        }

        /// <summary>
        /// Gets or sets the column number at which to begin indicating long lines.
        /// </summary>
        /// <returns>The number of columns in a long line. The default is 0.</returns>
        /// <remarks>
        /// When using <see cref="ScintillaNET.EdgeMode.Line"/>, a column is defined as the width of a space character in the <see cref="Style.Default" /> style.
        /// When using <see cref="ScintillaNET.EdgeMode.Background" /> a column is equal to a character (including tabs).
        /// </remarks>
        [DefaultValue(0)]
        [Category("Long Lines")]
        [Description("The number of columns at which to display long line indicators.")]
        public int EdgeColumn
        {
            get
            {
                return DirectMessage(NativeMethods.SCI_GETEDGECOLUMN).ToInt32();
            }
            set
            {
                value = Helpers.ClampMin(value, 0);
                DirectMessage(NativeMethods.SCI_SETEDGECOLUMN, new IntPtr(value));
            }
        }

        /// <summary>
        /// Gets or sets the mode for indicating long lines.
        /// </summary>
        /// <returns>
        /// One of the <see cref="ScintillaNET.EdgeMode" /> enumeration values.
        /// The default is <see cref="ScintillaNET.EdgeMode.None" />.
        /// </returns>
        [DefaultValue(EdgeMode.None)]
        [Category("Long Lines")]
        [Description("Determines how long lines are indicated.")]
        public EdgeMode EdgeMode
        {
            get
            {
                return (EdgeMode)DirectMessage(NativeMethods.SCI_GETEDGEMODE);
            }
            set
            {
                var edgeMode = (int)value;
                DirectMessage(NativeMethods.SCI_SETEDGEMODE, new IntPtr(edgeMode));
            }
        }

        /// <summary>
        /// Gets or sets the amount of whitespace added to the ascent (top) of each line.
        /// </summary>
        /// <returns>The extra line ascent. The default is zero.</returns>
        [DefaultValue(0)]
        [Category("Whitespace")]
        [Description("Extra whitespace added to the ascent (top) of each line.")]
        public int ExtraAscent
        {
            get
            {
                return DirectMessage(NativeMethods.SCI_GETEXTRAASCENT).ToInt32();
            }
            set
            {
                DirectMessage(NativeMethods.SCI_SETEXTRAASCENT, new IntPtr(value));
            }
        }

        /// <summary>
        /// Gets or sets the amount of whitespace added to the descent (bottom) of each line.
        /// </summary>
        /// <returns>The extra line descent. The default is zero.</returns>
        [DefaultValue(0)]
        [Category("Whitespace")]
        [Description("Extra whitespace added to the descent (bottom) of each line.")]
        public int ExtraDescent
        {
            get
            {
                return DirectMessage(NativeMethods.SCI_GETEXTRADESCENT).ToInt32();
            }
            set
            {
                DirectMessage(NativeMethods.SCI_SETEXTRADESCENT, new IntPtr(value));
            }
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;
            }
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }
            set
            {
                base.ForeColor = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to display the horizontal scroll bar.
        /// </summary>
        /// <returns>true to display the horizontal scroll bar when needed; otherwise, false. The default is true.</returns>
        [DefaultValue(true)]
        [Category("Scrolling")]
        [Description("Determines whether to show the horizontal scroll bar if needed.")]
        public bool HScrollBar
        {
            get
            {
                return (DirectMessage(NativeMethods.SCI_GETHSCROLLBAR) != IntPtr.Zero);
            }
            set
            {
                var visible = (value ? new IntPtr(1) : IntPtr.Zero);
                DirectMessage(NativeMethods.SCI_SETHSCROLLBAR, visible);
            }
        }

        /// <summary>
        /// Gets a collection of objects for working with indicators.
        /// </summary>
        /// <returns>A collection of <see cref="Indicator" /> objects.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IndicatorCollection Indicators { get; private set; }

        /// <summary>
        /// Gets or sets the current lexer.
        /// </summary>
        /// <returns>One of the <see cref="Lexer" /> enumeration values. The default is <see cref="ScintillaNET.Lexer.Container" />.</returns>
        [DefaultValue(Lexer.Container)]
        [Category("Lexing")]
        [Description("The current lexer.")]
        public Lexer Lexer
        {
            get
            {
                return (Lexer)DirectMessage(NativeMethods.SCI_GETLEXER);
            }
            set
            {
                var lexer = (int)value;
                DirectMessage(NativeMethods.SCI_SETLEXER, new IntPtr(lexer));
            }
        }

        /// <summary>
        /// Gets a collection representing lines of text in the <see cref="Scintilla" /> control.
        /// </summary>
        /// <returns>A collection of text lines.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public LineCollection Lines { get; private set; }

        /// <summary>
        /// Gets a collection representing margins in a <see cref="Scintilla" /> control.
        /// </summary>
        /// <returns>A collection of margins.</returns>
        [Category("Collections")]
        [Description("The margins collection.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public MarginCollection Margins { get; private set; }

        /// <summary>
        /// Gets a collection representing markers in a <see cref="Scintilla" /> control.
        /// </summary>
        /// <returns>A collection of markers.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MarkerCollection Markers { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the document has been modified (is dirty)
        /// since the last call to <see cref="SetSavePoint" />.
        /// </summary>
        /// <returns>true if the document has been modified; otherwise, false.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Modified
        {
            get
            {
                return (DirectMessage(NativeMethods.SCI_GETMODIFY) != IntPtr.Zero);
            }
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Padding Padding
        {
            get
            {
                return base.Padding;
            }
            set
            {
                base.Padding = value;
            }
        }

        /// <summary>
        /// Gets or sets whether line breaks in pasted text are convereted to the documents <see cref="EolMode" />.
        /// </summary>
        /// <returns>true to convert line breaks in pasted text; otherwise, false. The default is true.</returns>
        [DefaultValue(true)]
        [Category("Behavior")]
        [Description("Whether line breaks in pasted text are converted to match the document EOL mode.")]
        public bool PasteConvertEndings
        {
            get
            {
                return (DirectMessage(NativeMethods.SCI_GETPASTECONVERTENDINGS) != IntPtr.Zero);
            }
            set
            {
                var convert = (value ? new IntPtr(1) : IntPtr.Zero);
                DirectMessage(NativeMethods.SCI_SETPASTECONVERTENDINGS, convert);
            }
        }

        /// <summary>
        /// Gets or sets whether the document is read-only.
        /// </summary>
        /// <returns>true if the document is read-only; otherwise, false. The default is false.</returns>
        /// <seealso cref="ModifyAttempt" />
        [DefaultValue(false)]
        [Category("Behavior")]
        [Description("Controls whether the document text can be modified.")]
        public bool ReadOnly
        {
            get
            {
                return (DirectMessage(NativeMethods.SCI_GETREADONLY) != IntPtr.Zero);
            }
            set
            {
                var readOnly = (value ? new IntPtr(1) : IntPtr.Zero);
                DirectMessage(NativeMethods.SCI_SETREADONLY, readOnly);
            }
        }

        private IntPtr SciPointer
        {
            get
            {
                // Enforce illegal cross-thread calls the way the Handle property does
                if (Control.CheckForIllegalCrossThreadCalls && InvokeRequired)
                {
                    string message = string.Format(CultureInfo.InvariantCulture, "Control '{0}' accessed from a thread other than the thread it was created on.", Name);
                    throw new InvalidOperationException(message);
                }

                if (sciPtr == IntPtr.Zero)
                {
                    // Get a pointer to the native Scintilla object (i.e. C++ 'this') to use with the
                    // direct function. This will happen for each Scintilla control instance.
                    sciPtr = NativeMethods.SendMessage(new HandleRef(this, Handle), NativeMethods.SCI_GETDIRECTPOINTER, IntPtr.Zero, IntPtr.Zero);
                }

                return sciPtr;
            }
        }

        /// <summary>
        /// Gets or sets the range of the horizontal scroll bar.
        /// </summary>
        /// <returns>The range in pixels of the horizontal scroll bar. The default is 2000.</returns>
        /// <remarks>The width will automatically increase as needed when <see cref="ScrollWidthTracking" /> is enabled.</remarks>
        [DefaultValue(2000)]
        [Category("Scrolling")]
        [Description("The range in pixels of the horizontal scroll bar.")]
        public int ScrollWidth
        {
            get
            {
                return DirectMessage(NativeMethods.SCI_GETSCROLLWIDTH).ToInt32();
            }
            set
            {
                DirectMessage(NativeMethods.SCI_SETSCROLLWIDTH, new IntPtr(value));
            }
        }

        /// <summary>
        /// Gets or sets whether the <see cref="ScrollWidth" /> is automatically increased as needed.
        /// </summary>
        /// <returns>
        /// true to automatically increase the horizontal scroll width as needed; otherwise, false.
        /// The default is true.
        /// </returns>
        [DefaultValue(true)]
        [Category("Scrolling")]
        [Description("Determines whether to increase the horizontal scroll width as needed.")]
        public bool ScrollWidthTracking
        {
            get
            {
                return (DirectMessage(NativeMethods.SCI_GETSCROLLWIDTHTRACKING) != IntPtr.Zero);
            }
            set
            {
                var tracking = (value ? new IntPtr(1) : IntPtr.Zero);
                DirectMessage(NativeMethods.SCI_SETSCROLLWIDTHTRACKING, tracking);
            }
        }

        /// <summary>
        /// Gets or sets the search flags used when searching text.
        /// </summary>
        /// <returns>A bitwise combination of <see cref="ScintillaNET.SearchFlags" /> values. The default is <see cref="ScintillaNET.SearchFlags.None" />.</returns>
        /// <seealso cref="SearchInTarget" />
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SearchFlags SearchFlags
        {
            get
            {
                return (SearchFlags)DirectMessage(NativeMethods.SCI_GETSEARCHFLAGS).ToInt32();
            }
            set
            {
                var searchFlags = (int)value;
                DirectMessage(NativeMethods.SCI_SETSEARCHFLAGS, new IntPtr(searchFlags));
            }
        }

        /// <summary>
        /// Gets a collection representing style definitions in a <see cref="Scintilla" /> control.
        /// </summary>
        /// <returns>A collection of style definitions.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public StyleCollection Styles { get; private set; }

        /// <summary>
        /// Gets or sets the width of a tab as a multiple of a space character.
        /// </summary>
        /// <returns>The width of a tab measured in characters. The default is 4.</returns>
        [DefaultValue(4)]
        [Category("Indentation")]
        [Description("The tab size in characters.")]
        public int TabWidth
        {
            get
            {
                return DirectMessage(NativeMethods.SCI_GETTABWIDTH).ToInt32();
            }
            set
            {
                DirectMessage(NativeMethods.SCI_SETTABWIDTH, new IntPtr(value));
            }
        }

        /// <summary>
        /// Gets or sets the end position used when performing a search or replace.
        /// </summary>
        /// <returns>The zero-based character position within the document to end a search or replace operation.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="value" /> is less than zero. -or-
        /// <paramref name="value" /> is greater than the document length.
        /// </exception>
        /// <seealso cref="TargetStart"/>
        /// <seealso cref="SearchInTarget" />
        /// <seealso cref="ReplaceInTarget" />
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TargetEnd
        {
            get
            {
                // The position can become stale. If it's beyond the end of the document
                // report the end of the document; otherwise, we can't convert it.
                var bytePos = DirectMessage(NativeMethods.SCI_GETTARGETEND).ToInt32();
                var byteLength = DirectMessage(NativeMethods.SCI_GETTEXTLENGTH).ToInt32();
                if (bytePos >= byteLength)
                    return TextLength;

                return Lines.ByteToCharPosition(bytePos);
            }
            set
            {
                Helpers.ValidateDocumentPosition(value, TextLength, "value");

                var bytePos = Lines.CharToBytePosition(value);
                DirectMessage(NativeMethods.SCI_SETTARGETEND, new IntPtr(bytePos));
            }
        }

        /// <summary>
        /// Gets or sets the start position used when performing a search or replace.
        /// </summary>
        /// <returns>The zero-based character position within the document to start a search or replace operation.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="value" /> is less than zero. -or-
        /// <paramref name="value" /> is greater than the document length.
        /// </exception>
        /// <seealso cref="TargetEnd"/>
        /// <seealso cref="SearchInTarget" />
        /// <seealso cref="ReplaceInTarget" />
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TargetStart
        {
            get
            {
                // See above
                var bytePos = DirectMessage(NativeMethods.SCI_GETTARGETSTART).ToInt32();
                var byteLength = DirectMessage(NativeMethods.SCI_GETTEXTLENGTH).ToInt32();
                if (bytePos >= byteLength)
                    return TextLength;

                return Lines.ByteToCharPosition(bytePos);
            }
            set
            {
                Helpers.ValidateDocumentPosition(value, TextLength, "value");

                var bytePos = Lines.CharToBytePosition(value);
                DirectMessage(NativeMethods.SCI_SETTARGETSTART, new IntPtr(bytePos));
            }
        }

        /// <summary>
        /// Gets or sets the current document text in the <see cref="Scintilla" /> control.
        /// </summary>
        /// <returns>The text displayed in the control.</returns>
        /// <remarks>Depending on the length of text get or set, this operation can be expensive.</remarks>
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design", typeof(UITypeEditor))]
        public unsafe override string Text
        {
            get
            {
                var length = DirectMessage(NativeMethods.SCI_GETTEXTLENGTH).ToInt32();
                var ptr = DirectMessage(NativeMethods.SCI_GETRANGEPOINTER, new IntPtr(0), new IntPtr(length));
                if (ptr == IntPtr.Zero)
                    return string.Empty;

                // Assumption is that moving the gap will always be equal to or less expensive
                // than using one of the APIs which requires an intermediate buffer.
                var text = new string((sbyte*)ptr, 0, length, Encoding);
                return text;
            }
            set
            {
                fixed (byte* bp = Helpers.GetBytes(value ?? string.Empty, Encoding, zeroTerminated: true))
                    DirectMessage(NativeMethods.SCI_SETTEXT, IntPtr.Zero, new IntPtr(bp));
            }
        }

        /// <summary>
        /// Gets the length of the text in the control.
        /// </summary>
        /// <returns>The number of characters in the document.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TextLength
        {
            get
            {
                return Lines.TextLength;
            }
        }

        /// <summary>
        /// Gets or sets whether to collect undo and redo information.
        /// </summary>
        /// <returns>true to collect undo and redo information; otherwise, false. The default is true.</returns>
        /// <remarks>Disabling undo collection will also empty the undo buffer. See <see cref="EmptyUndoBuffer" />.</remarks>
        [DefaultValue(true)]
        [Category("Behavior")]
        [Description("Determines whether to collect undo and redo information.")]
        public bool UndoCollection
        {
            get
            {
                return (DirectMessage(NativeMethods.SCI_GETUNDOCOLLECTION) != IntPtr.Zero);
            }
            set
            {
                var collectUndo = (value ? new IntPtr(1) : IntPtr.Zero);
                DirectMessage(NativeMethods.SCI_SETUNDOCOLLECTION, collectUndo);
                if (!value)
                {
                    // Scintilla documentation makes it clear that if you fail to empty the undo buffer
                    // when you disable collection the buffer could become unsynchronized with the document.
                    // Seems like something we should do automatically.
                    DirectMessage(NativeMethods.SCI_EMPTYUNDOBUFFER);
                }
            }
        }

        /// <summary>
        /// Gets or sets how to display whitespace characters.
        /// </summary>
        /// <returns>One of the <see cref="WhitespaceMode" /> enumeration values. The default is <see cref="WhitespaceMode.Invisible" />.</returns>
        /// <seealso cref="SetWhitespaceForeColor" />
        /// <seealso cref="SetWhitespaceBackColor" />
        [DefaultValue(WhitespaceMode.Invisible)]
        [Category("Whitespace")]
        [Description("Options for displaying whitespace characters.")]
        public WhitespaceMode ViewWhitespace
        {
            get
            {
                return (WhitespaceMode)DirectMessage(NativeMethods.SCI_GETVIEWWS);
            }
            set
            {
                var wsMode = (int)value;
                DirectMessage(NativeMethods.SCI_SETVIEWWS, new IntPtr(wsMode));
            }
        }

        /// <summary>
        /// Gets or sets whether to display the vertical scroll bar.
        /// </summary>
        /// <returns>true to display the vertical scroll bar when needed; otherwise, false. The default is true.</returns>
        [DefaultValue(true)]
        [Category("Scrolling")]
        [Description("Determines whether to show the vertical scroll bar when needed.")]
        public bool VScrollBar
        {
            get
            {
                return (DirectMessage(NativeMethods.SCI_GETVSCROLLBAR) != IntPtr.Zero);
            }
            set
            {
                var visible = (value ? new IntPtr(1) : IntPtr.Zero);
                DirectMessage(NativeMethods.SCI_SETVSCROLLBAR, visible);
            }
        }

        /// <summary>
        /// Gets or sets the size of the dots used to mark whitespace.
        /// </summary>
        /// <returns>The size of the dots used to mark whitespace. The default is 1.</returns>
        /// <seealso cref="ViewWhitespace" />
        [DefaultValue(1)]
        [Category("Whitespace")]
        [Description("The size of whitespace dots.")]
        public int WhitespaceSize
        {
            get
            {
                return DirectMessage(NativeMethods.SCI_GETWHITESPACESIZE).ToInt32();
            }
            set
            {
                DirectMessage(NativeMethods.SCI_SETWHITESPACESIZE, new IntPtr(value));
            }
        }

        /// <summary>
        /// Gets or sets the line wrapping indent mode.
        /// </summary>
        /// <returns>
        /// One of the <see cref="ScintillaNET.WrapIndentMode" /> enumeration values. 
        /// The default is <see cref="ScintillaNET.WrapIndentMode.Fixed" />.
        /// </returns>
        [DefaultValue(WrapIndentMode.Fixed)]
        [Category("Line Wrapping")]
        [Description("Determines how wrapped sublines are indented.")]
        public WrapIndentMode WrapIndentMode
        {
            get
            {
                return (WrapIndentMode)DirectMessage(NativeMethods.SCI_GETWRAPINDENTMODE);
            }
            set
            {
                var wrapIndentMode = (int)value;
                DirectMessage(NativeMethods.SCI_SETWRAPINDENTMODE, new IntPtr(wrapIndentMode));
            }
        }

        /// <summary>
        /// Gets or sets the line wrapping mode.
        /// </summary>
        /// <returns>
        /// One of the <see cref="ScintillaNET.WrapMode" /> enumeration values. 
        /// The default is <see cref="ScintillaNET.WrapMode.None" />.
        /// </returns>
        [DefaultValue(WrapMode.None)]
        [Category("Line Wrapping")]
        [Description("The line wrapping strategy.")]
        public WrapMode WrapMode
        {
            get
            {
                return (WrapMode)DirectMessage(NativeMethods.SCI_GETWRAPMODE);
            }
            set
            {
                var wrapMode = (int)value;
                DirectMessage(NativeMethods.SCI_SETWRAPMODE, new IntPtr(wrapMode));
            }
        }

        /// <summary>
        /// Gets or sets the indented size in pixels of wrapped sublines.
        /// </summary>
        /// <returns>The indented size of wrapped sublines measured in pixels. The default is 0.</returns>
        /// <remarks>
        /// Setting <see cref="WrapVisualFlags" /> to <see cref="ScintillaNET.WrapVisualFlags.Start" /> will add an
        /// additional 1 pixel to the value specified.
        /// </remarks>
        [DefaultValue(0)]
        [Category("Line Wrapping")]
        [Description("The amount of pixels to indent wrapped sublines.")]
        public int WrapStartIndent
        {
            get
            {
                return DirectMessage(NativeMethods.SCI_GETWRAPSTARTINDENT).ToInt32();
            }
            set
            {
                value = Helpers.ClampMin(value, 0);
                DirectMessage(NativeMethods.SCI_SETWRAPSTARTINDENT, new IntPtr(value));
            }
        }

        /// <summary>
        /// Gets or sets the wrap visual flags.
        /// </summary>
        /// <returns>
        /// A bitwise combination of the <see cref="ScintillaNET.WrapVisualFlags" /> enumeration.
        /// The default is <see cref="ScintillaNET.WrapVisualFlags.None" />.
        /// </returns>
        [DefaultValue(WrapVisualFlags.None)]
        [Category("Line Wrapping")]
        [Description("The visual indicator displayed on a wrapped line.")]
        [TypeConverter(typeof(FlagsEnumTypeConverter.FlagsEnumConverter))]
        public WrapVisualFlags WrapVisualFlags
        {
            get
            {
                return (WrapVisualFlags)DirectMessage(NativeMethods.SCI_GETWRAPVISUALFLAGS);
            }
            set
            {
                int wrapVisualFlags = (int)value;
                DirectMessage(NativeMethods.SCI_SETWRAPVISUALFLAGS, new IntPtr(wrapVisualFlags));
            }
        }

        /// <summary>
        /// Gets or sets additional location options when displaying wrap visual flags.
        /// </summary>
        /// <returns>
        /// One of the <see cref="ScintillaNET.WrapVisualFlagLocation" /> enumeration values.
        /// The default is <see cref="ScintillaNET.WrapVisualFlagLocation.Default" />.
        /// </returns>
        [DefaultValue(WrapVisualFlagLocation.Default)]
        [Category("Line Wrapping")]
        [Description("The location of wrap visual flags in relation to the line text.")]
        public WrapVisualFlagLocation WrapVisualFlagLocation
        {
            get
            {
                return (WrapVisualFlagLocation)DirectMessage(NativeMethods.SCI_GETWRAPVISUALFLAGSLOCATION);
            }
            set
            {
                var location = (int)value;
                DirectMessage(NativeMethods.SCI_SETWRAPVISUALFLAGSLOCATION, new IntPtr(location));
            }
        }

        /// <summary>
        /// Gets or sets the horizontal scroll offset.
        /// </summary>
        /// <returns>The horizontal scroll offset in pixels.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int XOffset
        {
            get
            {
                return DirectMessage(NativeMethods.SCI_GETXOFFSET).ToInt32();
            }
            set
            {
                value = Helpers.ClampMin(value, 0);
                DirectMessage(NativeMethods.SCI_SETXOFFSET, new IntPtr(value));
            }
        }

        /// <summary>
        /// Gets or sets the zoom factor.
        /// </summary>
        /// <returns>The zoom factor measured in points.</returns>
        /// <remarks>For best results, values should range from -10 to 20 points.</remarks>
        /// <seealso cref="ZoomIn" />
        /// <seealso cref="ZoomOut" />
        [DefaultValue(0)]
        [Category("Appearance")]
        [Description("Zoom factor in points applied to the displayed text.")]
        public int Zoom
        {
            get
            {
                return DirectMessage(NativeMethods.SCI_GETZOOM).ToInt32();
            }
            set
            {
                DirectMessage(NativeMethods.SCI_SETZOOM, new IntPtr(value));
            }
        }

        #endregion Properties

        #region Events

        /// <summary>
        /// Occurs when text is about to be deleted.
        /// </summary>
        [Category("Notifications")]
        [Description("Occurs before text is deleted.")]
        public event EventHandler<BeforeModificationEventArgs> BeforeDelete
        {
            add
            {
                Events.AddHandler(beforeDeleteEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(beforeDeleteEventKey, value);
            }
        }

        /// <summary>
        /// Occurs when text is about to be inserted.
        /// </summary>
        [Category("Notifications")]
        [Description("Occurs before text is inserted.")]
        public event EventHandler<BeforeModificationEventArgs> BeforeInsert
        {
            add
            {
                Events.AddHandler(beforeInsertEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(beforeInsertEventKey, value);
            }
        }

        /// <summary>
        /// Occurs when text has been deleted from the document.
        /// </summary>
        [Category("Notifications")]
        [Description("Occurs when text is deleted.")]
        public event EventHandler<ModificationEventArgs> Delete
        {
            add
            {
                Events.AddHandler(deleteEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(deleteEventKey, value);
            }
        }

        /// <summary>
        /// Occurs when text has been inserted into the document.
        /// </summary>
        [Category("Notifications")]
        [Description("Occurs when text is inserted.")]
        public event EventHandler<ModificationEventArgs> Insert
        {
            add
            {
                Events.AddHandler(insertEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(insertEventKey, value);
            }
        }

        /// <summary>
        /// Occurs when text is about to be inserted. The inserted text can be changed.
        /// </summary>
        [Category("Notifications")]
        [Description("Occurs before text is inserted. Permits changing the inserted text.")]
        public event EventHandler<InsertCheckEventArgs> InsertCheck
        {
            add
            {
                Events.AddHandler(insertCheckEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(insertCheckEventKey, value);
            }
        }

        /// <summary>
        /// Occurs when a user attempts to change text while the document is in read-only mode.
        /// </summary>
        /// <seealso cref="ReadOnly" />
        [Category("Notifications")]
        [Description("Occurs when an attempt is made to change text in read-only mode.")]
        public event EventHandler<EventArgs> ModifyAttempt
        {
            add
            {
                Events.AddHandler(modifyAttemptEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(modifyAttemptEventKey, value);
            }
        }

        internal event EventHandler<SCNotificationEventArgs> SCNotification
        {
            add
            {
                Events.AddHandler(scNotificationEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(scNotificationEventKey, value);
            }
        }

        /// <summary>
        /// Occurs when the document becomes 'dirty'.
        /// </summary>
        /// <remarks>The document 'dirty' state can be checked with the <see cref="Modified" /> property and reset by calling <see cref="SetSavePoint" />.</remarks>
        /// <seealso cref="SetSavePoint" />
        /// <seealso cref="SavePointReached" />
        [Category("Notifications")]
        [Description("Occurs when a save point is left and the document becomes dirty.")]
        public event EventHandler<EventArgs> SavePointLeft
        {
            add
            {
                Events.AddHandler(savePointLeftEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(savePointLeftEventKey, value);
            }
        }

        /// <summary>
        /// Occurs when the document 'dirty' flag is reset.
        /// </summary>
        /// <remarks>The document 'dirty' state can be reset by calling <see cref="SetSavePoint" /> or undoing an action that modified the document.</remarks>
        /// <seealso cref="SetSavePoint" />
        /// <seealso cref="SavePointLeft" />
        [Category("Notifications")]
        [Description("Occurs when a save point is reached and the document is no longer dirty.")]
        public event EventHandler<EventArgs> SavePointReached
        {
            add
            {
                Events.AddHandler(savePointReachedEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(savePointReachedEventKey, value);
            }
        }

        /// <summary>
        /// Occurs when the control is about to display or print text and requires styling.
        /// </summary>
        /// <remarks>
        /// This event is only raised when <see cref="Lexer" /> is set to <see cref="ScintillaNET.Lexer.Container" />.
        /// The last position styled can be determined by calling <see cref="GetEndStyled" />.
        /// </remarks>
        /// <seealso cref="GetEndStyled" />
        [Category("Notifications")]
        [Description("Occurs when the text needs styling.")]
        public event EventHandler<EventArgs> StyleNeeded
        {
            add
            {
                Events.AddHandler(styleNeededEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(styleNeededEventKey, value);
            }
        }

        /// <summary>
        /// Occurs when the control UI is updated as a result of changes to text (including styling),
        /// selection, and/or scroll positions.
        /// </summary>
        [Category("Notifications")]
        [Description("Occurs when the control UI is updated.")]
        public event EventHandler<UpdateUIEventArgs> UpdateUI
        {
            add
            {
                Events.AddHandler(updateUIEventKey, value);
            }
            remove
            {
                Events.RemoveHandler(updateUIEventKey, value);
            }
        }

        #endregion Events

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Scintilla" /> class.
        /// </summary>
        public Scintilla()
        {
            // We don't want .NET to use GetWindowText because we manage ('cache') our own text
            base.SetStyle(ControlStyles.CacheText, true);

            // Necessary control styles (see TextBoxBase)
            base.SetStyle(ControlStyles.StandardClick |
                     ControlStyles.StandardDoubleClick |
                     ControlStyles.UseTextForAccessibility |
                     ControlStyles.UserPaint,
                     false);

            this.borderStyle = BorderStyle.Fixed3D;

            Lines = new LineCollection(this);
            Styles = new StyleCollection(this);
            Indicators = new IndicatorCollection(this);
            Margins = new MarginCollection(this);
            Markers = new MarkerCollection(this);
        }

        #endregion Constructors
    }
}
