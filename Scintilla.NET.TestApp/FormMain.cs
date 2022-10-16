using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ScintillaNET;

namespace Scintilla.NET.TestApp;

public partial class FormMain : Form
{
    public FormMain()
    {
        InitializeComponent();
        // ReSharper disable once VirtualMemberCallInConstructor
        Text += @"  © VPKSoft " + DateTime.Now.Year;
//            scintilla.Technology = Technology.DirectWrite;
    }

    private void mnuOpen_Click(object sender, EventArgs e)
    {
        if (odFile.ShowDialog() == DialogResult.OK)
        {
            scintilla.Text = File.ReadAllText(odFile.FileName);
            scintilla.EmptyUndoBuffer();
            //scintilla.Lexer = Lexer.Xml;
            SetLexerCs();
        }
    }

    private void SetLexerCs()
    {
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
//            scintilla.Lexer = Lexer.Cpp;
        scintilla.LexerName = "cpp";

        // Set the keywords
        scintilla.SetKeywords(0,
            "abstract as base break case catch checked continue default delegate do else event explicit extern false finally fixed for foreach goto if implicit in interface internal is lock namespace new null object operator out override params private protected public readonly ref return sealed sizeof stackalloc switch this throw true try typeof unchecked unsafe using virtual while");
        scintilla.SetKeywords(1,
            "bool byte char class const decimal double enum float int long sbyte short static string struct uint ulong ushort void");
    }

    private void mnuExit_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void mnuTestMethod_Click(object sender, EventArgs e)
    {
        scintilla.ConvertEols(Eol.Cr); 
        scintilla.Refresh();
/*            string ohm = "\u2126";
            string omega = "\u03C9".ToUpper();
            scintilla.Text = $"Ohm: {ohm}\r\nOmega: {omega}";

            scintilla.SetRepresentation(ohm, "OHM");
            scintilla.SetRepresentation(omega, "OMEGA");
*/
    }

    private void listLexersToolStripMenuItem_Click(object sender, EventArgs e)
    {
        for (int i = 0; i < Lexilla.GetLexerCount(); i++)
        {
            scintilla.AppendText(Lexilla.GetLexerName(i) + Environment.NewLine);
        }
    }

    private void scintilla_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        MessageBox.Show($@"Selected text: {scintilla.SelectedText}.", @"Double-click", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
    }

    private void FormMain_Shown(object sender, EventArgs e)
    {
        scintilla1.Focus();
    }
}