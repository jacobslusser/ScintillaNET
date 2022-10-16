using System.Linq;

namespace ScintillaNET;

/// <summary>
/// Helper methods for the <see cref="Scintilla"/> control.
/// </summary>
public static class HelperMethods
{
    /// <summary>
    /// Gets the folding state of the control as a delimited string containing line indexes.
    /// </summary>
    /// <param name="scintilla">The <see cref="Scintilla"/> control instance.</param>
    /// <param name="separator">The string to use as a separator.</param>
    /// <returns>The folding state of the control.</returns>
    public static string GetFoldingState(this Scintilla scintilla, string separator = ";")
    {
        return string.Join(separator,
            scintilla.Lines.Where(f => !f.Expanded).Select(f => f.Index).OrderBy(f => f).ToArray());
    }

    /// <summary>
    /// Sets the folding state of the state of the control with specified index string.
    /// </summary>
    /// <param name="scintilla">The <see cref="Scintilla"/> control instance.</param>
    /// <param name="foldingState">A string containing the folded line indexes separated with the <paramref name="separator"/> to restore the folding.</param>
    /// <param name="separator">The string to use as a separator.</param>
    public static void SetFoldingState(this Scintilla scintilla, string foldingState, string separator = ";")
    {
        scintilla.FoldAll(FoldAction.Expand);
#if NET45
            foreach (var index in foldingState.Split(separator[0]).Select(int.Parse))
#else
        foreach (var index in foldingState.Split(separator).Select(int.Parse))
#endif
        {
            if (index < 0 || index >= scintilla.Lines.Count)
            {
                continue;
            }
            scintilla.Lines[index].ToggleFold();
        }
    }
}