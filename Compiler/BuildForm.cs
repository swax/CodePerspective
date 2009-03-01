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

using XLibrary;

namespace XBuilder
{
    public partial class BuildForm : Form
    {
        string FilesDir;


        public BuildForm()
        {
            InitializeComponent();
        }

        private void AddLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = ".Net Assemblies|*.exe;*.dll";
            open.Multiselect = true;

            if (open.ShowDialog() != DialogResult.OK)
                return;

            if (FilesDir == null)
                FilesDir = Path.GetDirectoryName(open.FileName);
            else if (FilesDir != Path.GetDirectoryName(open.FileName))
            {
                MessageBox.Show("Assemblies must all be in the same directory");
                return;
            }

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
                    List<string> assemblies = new List<string>();

                    foreach (FileItem item in files)
                        assemblies.Add(Path.GetFileNameWithoutExtension(item.FilePath));

                    XNodeOut root = new XNodeOut(null, "root", XObjType.Root);

                    // foreach flie
                    foreach (FileItem item in files)
                    {
                        XDecompile file = new XDecompile(root, item.FilePath, assemblies);

                        updateTitle("XRay - Decompiling...");
                        file.Decompile();

                        updateTitle("XRay - Scanning...");
                        file.ScanLines();

                        updateTitle("XRay - Compiling...");
                        item.RecompiledPath = file.Compile();
                    }

                    updateTitle("XRay - Saving Map...");
                    root.SaveTree(FilesDir);
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

            //XRay.TestInit(Path.Combine(Path.GetDirectoryName(item.RecompiledPath), "XRay.dat"));

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
