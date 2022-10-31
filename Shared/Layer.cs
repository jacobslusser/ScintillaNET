namespace ScintillaNET;

/// <summary>
/// The layer on which a <see cref="Scintilla"/> control will draw elements like for example the text selection.
/// </summary>
public enum Layer
{
	/// <summary>
	/// Draw the selection background opaquely on the base layer.
	/// </summary>
	Base = NativeMethods.SC_LAYER_BASE,

	/// <summary>
	/// Draw the selection background translucently under the text. This will not work in single phase drawing mode.
	/// (<see cref="Phases.One"/>) as there is no under-text phase.
	/// </summary>
	UnderText = NativeMethods.SC_LAYER_UNDER_TEXT,

	/// <summary>
	/// Draw the selection background translucently over the text.
	/// </summary>
	OverText = NativeMethods.SC_LAYER_OVER_TEXT
}
