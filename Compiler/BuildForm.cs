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
            FileItem[] files = FileList.Items.Cast<FileItem>().ToArray();

            if (files.Length == 0)
                return;

            ReCompileButton.Enabled = false;
            TestButton.Enabled = false;

            new Thread(() =>
            {
                var status = new Action<string, string>((step, name) => 
                    RunInGui(() => 
                        Text = "XRay - " + step + " " + name + "..."));

                try
                {
                    List<string> assemblies = new List<string>();

                    foreach (FileItem item in files)
                        assemblies.Add(Path.GetFileNameWithoutExtension(item.FilePath));

                    XNodeOut root = new XNodeOut(null, "root", XObjType.Root);

                    // foreach flie
                    foreach (FileItem item in files)
                    {
                        XDecompile file = new XDecompile(root, item.FilePath);

                        status("Decompiling", item.Name);
                        file.Decompile();

                       //for (int i = 0; i < int.MaxValue; i++)
                       // {
                        file.AllowedAdds = int.MaxValue; // i;
                            file.AddsDone = 0;

                            status("Scanning", item.Name);
                            file.ScanLines(assemblies);

                            status("Recompiling", item.Name);
                            item.RecompiledPath = file.Compile();
                       // }
                    }

                    foreach (FileItem item in files)
                    {
                        status("Verifying", item.Name);
                        XDecompile.Verify(item.RecompiledPath);
                    }

                    status("Saving Map", "");
                    root.SaveTree(FilesDir);
                }
                catch (CompileError ex)
                {
                    string log = Path.Combine(Application.StartupPath, "errorlog.txt");
                    File.WriteAllText(log, ex.Summary);
                    Process.Start(log);
                }
                catch (Exception ex)
                {
                    RunInGui(() => MessageBox.Show("Error: " + ex.Message));
                }
                finally
                {
                    RunInGui(() =>
                    {
                        Text = "XRay";
                        ReCompileButton.Enabled = true;
                        TestButton.Enabled = true;
                    });
                }

            }).Start();
        }

        void RunInGui(Action method)
        {
            BeginInvoke(method);
        }

        private void TestButton_Click(object sender, EventArgs e)
        {
            //string path = @"C:\RAID\Main\Dev\WellsResearch\PSDev - Copy\PixelScope\Application\bin\Debug PixelScopePro\XRay.dat";
            //XRay.TestInit(path);
            //return;

            // execute selected assembly
            FileItem item = FileList.SelectedItem as FileItem;

            if (item == null)
                return;

            if (item.RecompiledPath == null)
            {
                MessageBox.Show(item.ToString() + " has not been re-compiled yet");
                return;
            }

            //string path = Path.Combine(Path.GetDirectoryName(item.RecompiledPath), "XRay.dat");
            //XRay.TestInit(path);

            Process.Start(item.RecompiledPath);
        }
    }

    public class FileItem
    {
        public string Name;
        public string FilePath;
        public string RecompiledPath;

        public FileItem(string path)
        {
            FilePath = path;
            Name = Path.GetFileName(path);
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
