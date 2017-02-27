using ScintillaNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test {
    public partial class Test : Form {


        public Test() {
            InitializeComponent();
            //scintilla1.Document = Document.Empty;
        }

        public async Task<Document> LoadFileAsync(ILoader loader, string path, CancellationToken cancellationToken, Encoding encoding = null, bool detectBOM = true) {
            const int bufferSize = 1024 * 1024;
            try {
                using (var file = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, bufferSize: bufferSize, useAsync: true))
                using (var reader = new StreamReader(file, encoding ?? Encoding.UTF8, detectBOM)) {
                    var count = 0;
                    var buffer = new char[bufferSize];
                    while ((count = await reader.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0) {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (!loader.AddData(buffer, count)) throw new IOException("The data could not be added to the loader.");
                    }
                    return loader.ConvertToDocument();
                }
            } catch {
                loader.Release();
                throw;
            }
        }

        public async void LoadFile(string path, Encoding encoding = null) {
            var _enabled = scintilla1.Enabled;
            var _readonly = scintilla1.ReadOnly;
            scintilla1.Enabled = false;
            scintilla1.ReadOnly = true;
            try {
                var loader = scintilla1.CreateLoader(1024 * 1024);
                if (loader == null) throw new ApplicationException("Unable to create loader.");
                var cts = new CancellationTokenSource();
                var document = await LoadFileAsync(loader, path, cts.Token, encoding, false);
                scintilla1.Document = document;
                scintilla1.ReleaseDocument(document);
            } catch (OperationCanceledException) { } catch (Exception x) {
                MessageBox.Show(this, x.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } finally {
                scintilla1.Enabled = _enabled;
                scintilla1.ReadOnly = _readonly;
            }
        }

        private void TestUTF8_Click(object sender, EventArgs e) {
            LoadFile(@"..\..\UTF-8+BOM.txt", Encoding.UTF8);
        }

        private void TestUTF8NoBOM_Click(object sender, EventArgs e) {
            LoadFile(@"..\..\UTF-8.txt", Encoding.UTF8);
        }


        private void TestWindows1250_Click(object sender, EventArgs e) {
            LoadFile(@"..\..\CP1250.txt", Encoding.GetEncoding(1250));
        }

        private void TestISO8859_2_Click(object sender, EventArgs e) {
            LoadFile(@"..\..\ISO8859-2.txt", Encoding.GetEncoding("ISO8859-2"));
        }

    }
}
