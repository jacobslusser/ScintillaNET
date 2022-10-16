namespace Scintilla.NET.TestApp
{
    partial class FormMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.scintilla = new ScintillaNET.Scintilla();
            this.msMain = new System.Windows.Forms.MenuStrip();
            this.mnuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuExit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuTestMethod = new System.Windows.Forms.ToolStripMenuItem();
            this.lexillaTestsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.listLexersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.odFile = new System.Windows.Forms.OpenFileDialog();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.scintilla1 = new ScintillaNET.Scintilla();
            this.msMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // scintilla
            // 
            this.scintilla.AutoCMaxHeight = 9;
            this.scintilla.BiDirectionality = ScintillaNET.BiDirectionalDisplayType.Disabled;
            this.scintilla.CaretLineBackColor = System.Drawing.Color.White;
            this.scintilla.CaretLineVisible = true;
            this.scintilla.LexerName = null;
            this.scintilla.Location = new System.Drawing.Point(0, 24);
            this.scintilla.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.scintilla.Name = "scintilla";
            this.scintilla.ScrollWidth = 6;
            this.scintilla.Size = new System.Drawing.Size(331, 426);
            this.scintilla.TabIndents = true;
            this.scintilla.TabIndex = 0;
            this.scintilla.UseRightToLeftReadingLayout = false;
            this.scintilla.WrapMode = ScintillaNET.WrapMode.None;
            this.scintilla.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.scintilla_MouseDoubleClick);
            // 
            // msMain
            // 
            this.msMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFile,
            this.mnuTestMethod,
            this.lexillaTestsToolStripMenuItem});
            this.msMain.Location = new System.Drawing.Point(0, 0);
            this.msMain.Name = "msMain";
            this.msMain.Size = new System.Drawing.Size(800, 24);
            this.msMain.TabIndex = 1;
            this.msMain.Text = "menuStrip1";
            // 
            // mnuFile
            // 
            this.mnuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuOpen,
            this.mnuExit});
            this.mnuFile.Name = "mnuFile";
            this.mnuFile.Size = new System.Drawing.Size(37, 20);
            this.mnuFile.Text = "File";
            // 
            // mnuOpen
            // 
            this.mnuOpen.Name = "mnuOpen";
            this.mnuOpen.Size = new System.Drawing.Size(124, 22);
            this.mnuOpen.Text = "Open File";
            this.mnuOpen.Click += new System.EventHandler(this.mnuOpen_Click);
            // 
            // mnuExit
            // 
            this.mnuExit.Name = "mnuExit";
            this.mnuExit.Size = new System.Drawing.Size(124, 22);
            this.mnuExit.Text = "Exit";
            this.mnuExit.Click += new System.EventHandler(this.mnuExit_Click);
            // 
            // mnuTestMethod
            // 
            this.mnuTestMethod.Name = "mnuTestMethod";
            this.mnuTestMethod.Size = new System.Drawing.Size(160, 20);
            this.mnuTestMethod.Text = "Test some Scintilla method";
            this.mnuTestMethod.Click += new System.EventHandler(this.mnuTestMethod_Click);
            // 
            // lexillaTestsToolStripMenuItem
            // 
            this.lexillaTestsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.listLexersToolStripMenuItem});
            this.lexillaTestsToolStripMenuItem.Name = "lexillaTestsToolStripMenuItem";
            this.lexillaTestsToolStripMenuItem.Size = new System.Drawing.Size(80, 20);
            this.lexillaTestsToolStripMenuItem.Text = "Lexilla Tests";
            // 
            // listLexersToolStripMenuItem
            // 
            this.listLexersToolStripMenuItem.Name = "listLexersToolStripMenuItem";
            this.listLexersToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.listLexersToolStripMenuItem.Text = "List Lexers";
            this.listLexersToolStripMenuItem.Click += new System.EventHandler(this.listLexersToolStripMenuItem_Click);
            // 
            // odFile
            // 
            this.odFile.DefaultExt = "*.*";
            this.odFile.Filter = "All Files|*.*";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(364, 56);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(169, 23);
            this.textBox1.TabIndex = 2;
            // 
            // scintilla1
            // 
            this.scintilla1.AutoCMaxHeight = 9;
            this.scintilla1.BiDirectionality = ScintillaNET.BiDirectionalDisplayType.Disabled;
            this.scintilla1.CaretLineBackColor = System.Drawing.Color.Black;
            this.scintilla1.CaretLineVisible = true;
            this.scintilla1.LexerName = null;
            this.scintilla1.Location = new System.Drawing.Point(364, 96);
            this.scintilla1.Name = "scintilla1";
            this.scintilla1.ScrollWidth = 49;
            this.scintilla1.Size = new System.Drawing.Size(374, 323);
            this.scintilla1.TabIndents = true;
            this.scintilla1.TabIndex = 3;
            this.scintilla1.Text = "scintilla1";
            this.scintilla1.UseRightToLeftReadingLayout = false;
            this.scintilla1.WrapMode = ScintillaNET.WrapMode.None;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.scintilla1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.scintilla);
            this.Controls.Add(this.msMain);
            this.MainMenuStrip = this.msMain;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "FormMain";
            this.Text = "ScintillaNET.TestApp";
            this.Shown += new System.EventHandler(this.FormMain_Shown);
            this.msMain.ResumeLayout(false);
            this.msMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ScintillaNET.Scintilla scintilla;
        private System.Windows.Forms.MenuStrip msMain;
        private System.Windows.Forms.ToolStripMenuItem mnuFile;
        private System.Windows.Forms.ToolStripMenuItem mnuOpen;
        private System.Windows.Forms.ToolStripMenuItem mnuExit;
        private System.Windows.Forms.OpenFileDialog odFile;
        private System.Windows.Forms.ToolStripMenuItem mnuTestMethod;
        private System.Windows.Forms.ToolStripMenuItem lexillaTestsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem listLexersToolStripMenuItem;
        private System.Windows.Forms.TextBox textBox1;
        private ScintillaNET.Scintilla scintilla1;
    }
}

