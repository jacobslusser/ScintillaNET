namespace Test {
    partial class Test {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.scintilla1 = new ScintillaNET.Scintilla();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.TestUTF8 = new System.Windows.Forms.ToolStripMenuItem();
            this.TestWindows1250 = new System.Windows.Forms.ToolStripMenuItem();
            this.TestISO8859_2 = new System.Windows.Forms.ToolStripMenuItem();
            this.TestUTF8NoBOM = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // scintilla1
            // 
            this.scintilla1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scintilla1.Location = new System.Drawing.Point(0, 24);
            this.scintilla1.Name = "scintilla1";
            this.scintilla1.Size = new System.Drawing.Size(284, 237);
            this.scintilla1.TabIndex = 0;
            this.scintilla1.Text = "scintilla1";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(284, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TestUTF8,
            this.TestUTF8NoBOM,
            this.TestWindows1250,
            this.TestISO8859_2});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // TestUTF8
            // 
            this.TestUTF8.Name = "TestUTF8";
            this.TestUTF8.Size = new System.Drawing.Size(176, 22);
            this.TestUTF8.Text = "Test UTF-8 (+BOM)";
            this.TestUTF8.Click += new System.EventHandler(this.TestUTF8_Click);
            // 
            // TestWindows1250
            // 
            this.TestWindows1250.Name = "TestWindows1250";
            this.TestWindows1250.Size = new System.Drawing.Size(176, 22);
            this.TestWindows1250.Text = "Test Windows-1250";
            this.TestWindows1250.Click += new System.EventHandler(this.TestWindows1250_Click);
            // 
            // TestISO8859_2
            // 
            this.TestISO8859_2.Name = "TestISO8859_2";
            this.TestISO8859_2.Size = new System.Drawing.Size(176, 22);
            this.TestISO8859_2.Text = "Test ISO-8859-2";
            this.TestISO8859_2.Click += new System.EventHandler(this.TestISO8859_2_Click);
            // 
            // TestUTF8NoBOM
            // 
            this.TestUTF8NoBOM.Name = "TestUTF8NoBOM";
            this.TestUTF8NoBOM.Size = new System.Drawing.Size(176, 22);
            this.TestUTF8NoBOM.Text = "Test UTF-8 (-BOM)";
            this.TestUTF8NoBOM.Click += new System.EventHandler(this.TestUTF8NoBOM_Click);
            // 
            // Test
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.scintilla1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Test";
            this.Text = "Form1";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ScintillaNET.Scintilla scintilla1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem TestUTF8;
        private System.Windows.Forms.ToolStripMenuItem TestWindows1250;
        private System.Windows.Forms.ToolStripMenuItem TestISO8859_2;
        private System.Windows.Forms.ToolStripMenuItem TestUTF8NoBOM;
    }
}

