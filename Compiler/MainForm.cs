using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace XBuilder
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void AddLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = ".Net Assemblies|*.exe;*.dll";

            if (open.ShowDialog() != DialogResult.OK)
                return;

            foreach(string path in open.FileNames)
                FileList.Items.Add(new FileItem(path));
        }

        private void RemoveLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (FileItem item in FileList.SelectedItems.Cast<FileItem>().ToArray())
                FileList.Items.Remove(item);
        }

        private void ReCompileButton_Click(object sender, EventArgs e)
        {
            ReCompileButton.Enabled = false;

            FileItem[] files = FileList.Items.Cast<FileItem>().ToArray();

            new Thread(() =>
            {
                var updateTitle = new Action<string>(s => RunInGui(() => Text = s));

                try
                {
                    // foreach flie
                    foreach (FileItem item in files)
                    {
                        XDecompile file = new XDecompile(item.FilePath);

                        updateTitle("XRay - Decompiling...");
                        file.Decompile();

                        updateTitle("XRay - Scanning...");
                        file.ScanLines();

                        updateTitle("XRay - Saving...");
                        file.SaveTree();

                        updateTitle("XRay - Compiling...");
                        item.RecompiledPath = file.Compile();
                    }
                }
                catch (Exception ex)
                {
                    RunInGui(() => MessageBox.Show("Error: " + ex.Message));
                }
                finally
                {
                    updateTitle("XRay");
                    RunInGui(() => ReCompileButton.Enabled = true);
                }

            }).Start();
        }

        void RunInGui(Action method)
        {
            BeginInvoke(method);
        }

        private void TestButton_Click(object sender, EventArgs e)
        {
            // execute selected assembly
            FileItem item = FileList.SelectedItem as FileItem;

            if (item == null)
                return;

            if (item.RecompiledPath == null)
            {
                MessageBox.Show(item.ToString() + " has not been re-compiled yet");
                return;
            }

            XLibrary.XRay.TestInit(Path.Combine(Path.GetDirectoryName(item.RecompiledPath),"XRay.dat"));
            return;

            Process.Start(item.RecompiledPath);
        }
    }

    public class FileItem
    {
        public string FilePath = "";
        public string RecompiledPath;

        public FileItem(string path)
        {
            FilePath = path;
        }

        public override string ToString()
        {
            string text = Path.GetFileName(FilePath);

            if (RecompiledPath != null)
                text += " (re-compiled)";

            return text;
        }
    }
}
