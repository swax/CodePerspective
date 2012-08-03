using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.Remoting.Channels.Ipc;
using System.IO;
using System.Runtime.Remoting;
using XLibrary;
using System.Diagnostics;
using System.Runtime.Remoting.Channels;
using XLibrary.Meta;

namespace XBuilder
{
    public partial class BuildPanel : UserControl
    {
        string SourceDir;
        string OutputDir;
        string DatPath;

        public BuildPanel()
        {
            InitializeComponent();

            TrackFlowCheckBox.AttachToolTip("Calls between functions are tracked so call graphs can work");

            TrackFieldsCheckBox.AttachToolTip("Set and get operations to class members are tracked");

            TrackInstancesCheckBox.AttachToolTip("Creation and deletion of classes are tracked, and class introspection is enabled");

            ReplaceOriginalCheckBox.AttachToolTip(
@"Unchecked: XRay copies and re-compiles the selected files then runs them side by side the originals.
           This case maintains the relative paths the original files had for configuration, etc...

Checked: XRay overwrites the original files with XRayed versions, originals are put in a backup directory.
           Namespaces are kept the same so referencing assemblies should still work.");

            DecompileCSharpCheckBox.AttachToolTip("Recompile time takes signifigantly longer with this option");
        }

        private void AddLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = ".Net Assemblies|*.exe;*.dll";
            open.Multiselect = true;

            if (open.ShowDialog() != DialogResult.OK)
                return;

            AddFilesToList(open.FileNames);
        }

        public void AddFilesToList(string[] paths)
        {
            foreach (string path in paths)
            {
                if (SourceDir == null)
                    SourceDir = Path.GetDirectoryName(path);

                else if (SourceDir != Path.GetDirectoryName(path))
                {
                    MessageBox.Show("Files must be from the same directory");
                    return;
                }

                FileList.Items.Add(new XRayedFile(path));
            }

            UpdateOutputPath();

            StatusLabel.Text = "Ready to Recompile";
        }

        private void RemoveLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (XRayedFile item in FileList.SelectedItems.Cast<XRayedFile>().ToArray())
                FileList.Items.Remove(item);

            UpdateOutputPath();
        }

        private void ResetLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FileList.Items.Clear();
            UpdateOutputPath();
        }

        private void ReCompileButton_Click(object sender, EventArgs e)
        {
            ReCompile(false);

        }

        private void ReCompile(bool test)
        {
            XRayedFile[] files = FileList.Items.Cast<XRayedFile>().ToArray();

            if (files.Length == 0)
                return;

            OptionsPanel.Enabled = false;

            // have to extract out because thread cant access gui elements
            bool trackFlow = TrackFlowCheckBox.Checked;
            bool trackExternal = TrackExternalCheckBox.Checked;
            bool trackAnon = TrackAnonCheckBox.Checked;
            bool trackFields = TrackFieldsCheckBox.Checked;
            bool trackInstances = TrackInstancesCheckBox.Checked;
            bool replaceOriginal = ReplaceOriginalCheckBox.Checked;
            bool doVerify = RunVerifyCheckbox.Checked;
            bool compileWithMS = MsToolsCheckbox.Checked;
            bool decompileAgain = DecompileAgainCheckbox.Checked;
            bool showUiOnStart = ShowOnStartCheckBox.Checked;
            bool saveMsil = SaveMsilCheckBox.Checked;
            bool decompileCSharp = DecompileCSharpCheckBox.Checked;


            new Thread(() =>
            {
                string stepname = "";

                var status = new Action<string, string>((step, name) =>
                {
                    stepname = step;

                    RunInGui(() =>
                        StatusLabel.Text = step + " " + name);
                });

                long linesAdded = 0;
                long trackedObjects = 0;

                try
                {
                    status("checking", "");
                    if (XDecompile.CheckIfAlreadyXRayed(files) &&
                        !TryRestoreBackups(files))
                    {
                        RunInGui(() => OptionsPanel.Enabled = true);
                        return;
                    }

                    status("Preparing", "");
                    XDecompile.PrepareOutputDir(SourceDir, OutputDir);

                    string errorLog = "";

                    XNodeOut.NextID = 0;
                    XNodeOut root = new XNodeOut(null, "root", XObjType.Root);
                    XNodeOut extRoot = root.AddNode("Not XRayed", XObjType.External);
                    XNodeOut intRoot = root.AddNode("XRayed", XObjType.Internal);

                    // init root file nodes so links can be made for processed fields before they are directly xrayed
                    foreach (XRayedFile item in files)
                        item.FileNode = intRoot.AddNode(Path.GetFileName(item.FilePath), XObjType.File);


                    foreach (XRayedFile item in files)
                    {
                        XDecompile decompile = new XDecompile(intRoot, extRoot, item, OutputDir, DatPath, files, !replaceOriginal)
                        {
                            TrackFlow = trackFlow,
                            TrackExternal = trackExternal,
                            TrackAnon = trackAnon,
                            TrackFields = trackFields,
                            TrackInstances = trackInstances,
                            ShowUIonStart = showUiOnStart,
                            SaveMsil = saveMsil,
                            DecompileCSharp = decompileCSharp
                        };

                        try
                        {
                            if (compileWithMS)
                            {
                                status("Decompiling", item.FileName);
                                decompile.MsDecompile();

                                // used for debugging tricky ilasm errors
                                //for (int i = 0; i < int.MaxValue; i++)
                                // {
                                decompile.AllowedAdds = int.MaxValue; // i;
                                decompile.AddsDone = 0;

                                status("Scanning", item.FileName);
                                decompile.ScanLines(test);

                                status("Recompiling", item.FileName);
                                item.RecompiledPath = decompile.Compile();
                                // }
                            }
                            else
                            {
                                status("Recompiling", item.FileName);
                                decompile.MonoRecompile();
                            }
                            
                            // remove signature
                            // files that arent signed are coming up as signed meaning this would probably corrupt a file
                            // also not sure if checked anymore - http://msdn.microsoft.com/en-us/library/cc713694.aspx
                            //var meta = new MetaInfo(item.RecompiledPath);
                            //if (meta.Load())
                            //    meta.RemoveSignature();

                            // decompile to check
                            if (decompileAgain)
                            {
                                string filename = Path.GetFileName(item.FilePath);
                                string compiler = compileWithMS ? "MS" : "Mono";

                                // create directories
                                var dir = Path.Combine(Application.StartupPath, "recompile", filename, compiler + "_original");
                                var originalPath = decompile.BackupPath != null ? decompile.BackupPath : item.FilePath;
                                decompile.Decompile(originalPath, dir);

                                dir = Path.Combine(Application.StartupPath, "recompile", filename, compiler + "_new");
                                decompile.Decompile(item.RecompiledPath, dir);
                            }
                        }
                        catch (CompileError ex)
                        {
                            errorLog += item.FileName + "\r\n" + ex.Summary + "\r\n--------------------------------------------------------\r\n";
                        }
                        catch (Exception ex)
                        {
                            RunInGui(() =>
                            {
                                MessageBox.Show("Error recompiling: " + ex.Message + "\r\n" + ex.StackTrace);
                                StatusLabel.Text = "Error on " + item.FileName;
                                OptionsPanel.Enabled = true;
                            });

                            return;
                        }

                        linesAdded += decompile.LinesAdded;
                    }

                    // save node map before verifying because we dont want bogus verify 
                    // errors from preventing the dat file form being made
                    status("Saving Map", "");

                    var settings = new Dictionary<string, string>();

                    settings["Version"] = XRay.BuilderVersion;

                    if (Pro.Verified)
                        settings["Pro"] = Pro.SignedFile;

                    trackedObjects = root.SaveTree(DatPath, settings);


                    // verify last and aggregate errors'
                    if (doVerify)
                        foreach (XRayedFile item in files)
                        {
                            try
                            {
                                status("Verifying", item.FileName);
                                XDecompile.Verify(item.RecompiledPath);
                            }
                            catch (CompileError ex)
                            {
                                errorLog += item.FileName + "\r\n" + ex.Summary + "\r\n--------------------------------------------------------\r\n";
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

                    if (!replaceOriginal)
                        summary = summary.Replace("Unable to resolve token.",
                            "Unable to resolve token. (Try turning off side by side)");

                    //todo token error - turn off side by side

                    string logPath = Path.Combine(Application.StartupPath, "errorlog.txt");
                    File.WriteAllText(logPath, summary);
                    Process.Start(logPath);
                }
                catch (Exception ex)
                {
                    RunInGui(() => MessageBox.Show("Error during " + stepname + ": " + ex.Message));
                }

                status(String.Format("Ready to Launch - {0:#,#} instructions added, {1:#,#} objects tracked", linesAdded, trackedObjects), "");

                RunInGui(() =>
                {
                    OptionsPanel.Enabled = true;
                });


            }).Start();
        }


        private bool TryRestoreBackups(XRayedFile[] files)
        {
            var result = MessageBox.Show("Some of these files have already been XRayed. Try to restore originals?", "Problem", MessageBoxButtons.YesNo);

            // dont just ignore error because we dont want to backup an xrayed version over an original version and have the user lose their original file

            if (result == DialogResult.No)
            {
                RunInGui(() => StatusLabel.Text = "Recompile canceled");
                return false;
            }

            // iterate through backup dir, copy overwrite xrayed files
            var backupDir = Path.Combine(OutputDir, "xBackup");
            foreach (var backupFile in Directory.GetFiles(backupDir))
            {
                var originalPath = backupFile.Replace("\\xBackup", "");
                File.Copy(backupFile, originalPath, true);
            }

            // re-check, if still xrayed, say failed to restore backups
            if (XDecompile.CheckIfAlreadyXRayed(files))
            {
                RunInGui(() => StatusLabel.Text = "Failed to restore backup");
                return false;
            }

            RunInGui(() => StatusLabel.Text = "Restored backups");
            return true;
        }

        void RunInGui(Action method)
        {
            BeginInvoke(method);
        }

        private void LaunchButton_Click(object sender, EventArgs e)
        {
            if(FileList.Items.Count == 0)
            {
                MessageBox.Show("Nothing to launch");
                return;
            }

            // execute selected assembly
            XRayedFile item =  FileList.Items.Cast<XRayedFile>().FirstOrDefault(i => i.FileName.EndsWith(".exe"));

            if (item == null)
            {
                MessageBox.Show("Can only launch exes, for dlls launch the exe that uses the dll.");
                return;
            }

            if (item.RecompiledPath == null)
            {
                MessageBox.Show(item.ToString() + " has not been re-compiled yet");
                return;
            }

            ProcessStartInfo info = new ProcessStartInfo(item.RecompiledPath);
            info.WorkingDirectory = Path.GetDirectoryName(item.RecompiledPath);

            Process.Start(info);
        }


        private void ShowMapButton_Click(object sender, EventArgs e)
        {
            try
            {
                string path = Path.Combine(OutputDir, "XRay.dat");

                XRay.Analyze(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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

            OutputDir = SourceDir;
            DatPath = Path.Combine(OutputDir, "XRay.dat");
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

        private void SidebySideCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateOutputPath();
        }

        private void TestCompile_Click(object sender, EventArgs e)
        {
            // StringEval("y.xxx");
            ReCompile(true);
        }


    }


    public class XRayedFile
    {
        public string AssemblyName;
        public string FileName;
        public string FilePath;
        public string RecompiledPath;
        public XNodeOut FileNode;

        public XRayedFile(string path)
        {
            FilePath = path;
            FileName = Path.GetFileName(path);
            AssemblyName = Path.GetFileNameWithoutExtension(path);
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
