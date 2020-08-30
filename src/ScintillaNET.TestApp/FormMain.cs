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
    }
}
