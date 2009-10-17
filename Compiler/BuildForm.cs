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
        string SourceDir;
        string OutputDir;


        public BuildForm()
        {
            InitializeComponent();

            new ToolTip() { AutoPopDelay = 20000 }.SetToolTip(FlowBox,
@"Checked: Calls between functions are traced

Unchecked: Slightly less overhead.");

            new ToolTip() { AutoPopDelay = 20000 }.SetToolTip(SidebySideBox,
@"Checked: XRay copies and re-compiles the selected files then runs them side by side the originals.
          This case maintains the relative paths the original files had for configuration, etc...

Unchecked: XRay creates a new directory to put re-compiled files into so that renaming is not necessary.
          This case is useful when getting 'error loading token' during verification usually cause by 
          re-compiling a sub-set of a project with many interconnected dlls.");
        }

        private void AddLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = ".Net Assemblies|*.exe;*.dll";
            open.Multiselect = true;

            if (open.ShowDialog() != DialogResult.OK)
                return;

            if (SourceDir == null)
                SourceDir = Path.GetDirectoryName(open.FileName);

            else if (SourceDir != Path.GetDirectoryName(open.FileName))
            {
                MessageBox.Show("Files must be from the same directory");
                return;
            }

            foreach(string path in open.FileNames)
                FileList.Items.Add(new FileItem(path));

            UpdateOutputPath();
        }

        private void RemoveLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (FileItem item in FileList.SelectedItems.Cast<FileItem>().ToArray())
                FileList.Items.Remove(item);

            UpdateOutputPath();
        }

        private void ReCompileButton_Click(object sender, EventArgs e)
        {
            FileItem[] files = FileList.Items.Cast<FileItem>().ToArray();

            if (files.Length == 0)
                return;

            ReCompileButton.Enabled = false;
            LaunchButton.Enabled = false;

            bool trackFlow = FlowBox.Checked;
            bool sidebySide = SidebySideBox.Checked;

            new Thread(() =>
            {
                string stepname = "";

                var status = new Action<string, string>((step, name) =>
                {
                    stepname = step;

                    RunInGui(() =>
                        Text = "XRay - " + step + " " + name + "...");
                });

                try
                {
                    status("Preparing", "");
                    XDecompile.PrepareOutputDir(SourceDir, OutputDir);

                    List<string> assemblies = new List<string>();

                    foreach (FileItem item in files)
                        assemblies.Add(Path.GetFileNameWithoutExtension(item.FilePath));

                    XNodeOut.NextID = 0;
                    XNodeOut root = new XNodeOut(null, "root", XObjType.Root);

                    string errorLog = "";

                    // foreach file
                    foreach (FileItem item in files)
                    {
                        XDecompile file = new XDecompile(root, item.FilePath, OutputDir);

                        file.TrackFlow = trackFlow;

                        try
                        {
                            status("Decompiling", item.Name);
                            file.Decompile();
                        }
                        catch (CompileError ex)
                        {
                            errorLog += item.Name + "\r\n" + ex.Summary + "\r\n--------------------------------------------------------\r\n";
                            continue;
                        }

                        // used for debugging tricky ilasm errors
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

                    // save node map before verifying because we dont want bogus verify 
                    // errors from preventing the dat file form being made
                    status("Saving Map", "");
                    root.SaveTree(OutputDir);

                    // verify last and aggregate errors
                    foreach (FileItem item in files)
                    {
                        try
                        {
                            status("Verifying", item.Name);
                            XDecompile.Verify(item.RecompiledPath);
                        }
                        catch (CompileError ex)
                        {
                            errorLog += item.Name + "\r\n" + ex.Summary + "\r\n--------------------------------------------------------\r\n";
                        }
                    }

                    if (errorLog.Length > 0)
                        throw new CompileError(errorLog);
                }
                catch (CompileError ex)
                {
                    string summary = ex.Summary;

                    summary = summary.Replace("Unexpected type on the stack.",
                        "Unexpected type on the stack. (Ignore)");

                    summary = summary.Replace("Unmanaged pointers are not a verifiable type.",
                        "Unmanaged pointers are not a verifiable type. (Ignore)");

                    if(sidebySide)
                        summary = summary.Replace("Unable to resolve token.",
                            "UUnable to resolve token. (Try turning off side by side)");
                    

                    //todo token error - turn off side by side

                    string logPath = Path.Combine(Application.StartupPath, "errorlog.txt");
                    File.WriteAllText(logPath, summary);
                    Process.Start(logPath);
                }
                catch (Exception ex)
                {
                    RunInGui(() => MessageBox.Show("Error during " + stepname + ": " + ex.Message));
                }


                // success
                RunInGui(() =>
                {
                    Text = "XRay";
                    ReCompileButton.Enabled = true;
                    LaunchButton.Enabled = true;
                });


            }).Start();
        }

        void RunInGui(Action method)
        {
            BeginInvoke(method);
        }

        private void LaunchButton_Click(object sender, EventArgs e)
        {
            // execute selected assembly
            FileItem item = GetSelectedItem();

            if (item.RecompiledPath == null)
            {
                MessageBox.Show(item.ToString() + " has not been re-compiled yet");
                return;
            }

            Process.Start(item.RecompiledPath);
        }

        private void ShowMapButton_Click(object sender, EventArgs e)
        {
            //string path = @"C:\RAID\Main\Dev\WellsResearch\PSDev - Copy\PixelScope\Application\bin\Debug PixelScopePro\XRay.dat";
            //XRay.TestInit(path);
            //return;

            try
            {
                FileItem item = GetSelectedItem();

                string path = Path.Combine(OutputDir, "XRay.dat");

                XRay.TestInit(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        FileItem GetSelectedItem()
        {
            FileItem item = FileList.SelectedItem as FileItem;

            return FileList.Items.Cast<FileItem>().FirstOrDefault(i => i.Name.EndsWith(".exe"));
        }

        private void ResetLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FileList.Items.Clear();
            UpdateOutputPath();
        }

        private void UpdateOutputPath()
        {
            if (FileList.Items.Count == 0)
                SourceDir = null;

            if (SourceDir == null)
            {
                OutputDir = null;
                OutputLink.Visible = false;
                return;
            }

            if (SidebySideBox.Checked)
                OutputDir = SourceDir;
            else
                OutputDir = Path.Combine(SourceDir, "XRay");

            OutputLink.Text = "Output: " + OutputDir;
            OutputLink.Visible = true;
        }

        private void OutputLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(OutputDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SidebySideBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateOutputPath();
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
