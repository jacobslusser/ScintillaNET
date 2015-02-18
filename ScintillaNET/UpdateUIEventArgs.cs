using System;

namespace ScintillaNET
{
    /// <summary>
    /// Provides data for the <see cref="Scintilla.UpdateUI" /> event.
    /// </summary>
    public class UpdateUIEventArgs : EventArgs
    {
        #region Properties

        /// <summary>
        /// The UI update that occurred.
        /// </summary>
        /// <returns>A bitwise combination of <see cref="Update" /> values specifying the UI update that occurred.</returns>
        public Update Update { get; private set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateUIEventArgs" /> class.
        /// </summary>
        /// <param name="update">A bitwise combination of <see cref="Update" /> values specifying the reason to update the UI.</param>
        public UpdateUIEventArgs(Update update)
        {
            Update = update;
        }

        #endregion Constructors
    }
}
