using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

        // The goods
        private IntPtr sciPtr;

        private BorderStyle borderStyle;

        #endregion Fields

        #region Methods

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

        internal unsafe string GetText(int startInBytes, int lengthInBytes)
        {
            var ptr = DirectMessage(NativeMethods.SCI_GETRANGEPOINTER, new IntPtr(startInBytes), new IntPtr(lengthInBytes));
            if (ptr == IntPtr.Zero)
                return string.Empty;

            var text = new string((sbyte*)ptr, 0, lengthInBytes, Encoding);
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

        #endregion Methods

        #region Properties

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

        internal Encoding Encoding
        {
            get
            {
                // Should always be UTF-8
                int codePage = (int)DirectMessage(NativeMethods.SCI_GETCODEPAGE);
                return (codePage == 0 ? Encoding.Default : Encoding.GetEncoding(codePage));
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
        /// Gets or sets the current text in the <see cref="Scintilla" /> control.
        /// </summary>
        /// <returns>The text displayed in the control.</returns>
        /// <remarks>Depending on the length of text get or set, this operation can be expensive.</remarks>
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design", typeof(UITypeEditor))]
        public override string Text
        {
            get
            {
                var length = DirectMessage(NativeMethods.SCI_GETLENGTH).ToInt32();
                return GetText(0, length);
            }
            set
            {
                var bytes = Helpers.GetBytes(value ?? string.Empty, Encoding, zeroTerminated: true);
                unsafe
                {
                    fixed (byte* bp = bytes)
                        DirectMessage(NativeMethods.SCI_SETTEXT, IntPtr.Zero, new IntPtr(bp));
                }
            }
        }

        /// <summary>
        /// Gets the length of the document text measured in bytes.
        /// </summary>
        /// <returns>The number of bytes in the document.</returns>
        /// <remarks>The value is bytes, not characters.</remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TextLength
        {
            get
            {
                return DirectMessage(NativeMethods.SCI_GETTEXTLENGTH).ToInt32();
            }
        }

        #endregion Properties

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
        }

        #endregion Constructors
    }
}
