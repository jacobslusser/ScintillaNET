using System;
using System.IO;
using System.Windows.Forms;

namespace ScintillaNET.TestApp
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
            // ReSharper disable once VirtualMemberCallInConstructor
            Text += @"  © VPKSoft " + DateTime.Now.Year;
        }

        private void mnuOpen_Click(object sender, EventArgs e)
        {
            if (odFile.ShowDialog() == DialogResult.OK)
            {
                scintilla.Text = File.ReadAllText(odFile.FileName);
                scintilla.EmptyUndoBuffer();
            }
        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void mnuTestMethod_Click(object sender, EventArgs e)
        {
            string ohm = "\u2126";
            string omega = "\u03C9".ToUpper();
            scintilla.Text = $"Ohm: {ohm}\r\nOmega: {omega}";

            scintilla.SetRepresentation(ohm, "OHM");
            scintilla.SetRepresentation(omega, "OMEGA");
        }
    }
}
