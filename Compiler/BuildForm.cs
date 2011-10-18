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
using System.CodeDom.Compiler;
using System.Reflection;

namespace XBuilder
{
    public partial class BuildForm : Form
    {
        string SourceDir;
        string OutputDir;

        public string xxx = "john";

        public BuildForm()
        {
            InitializeComponent();

            new ToolTip() { AutoPopDelay = 20000 }.SetToolTip(TrackFlowCheckBox,
@"Checked: Calls between functions are traced

Unchecked: Slightly less overhead.");

            new ToolTip() { AutoPopDelay = 20000 }.SetToolTip(SidebySideCheckBox,
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
                FileList.Items.Add(new XRayedFile(path));

            UpdateOutputPath();

            StatusLabel.Text = "Ready to Recompile";
        }

        private void RemoveLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (XRayedFile item in FileList.SelectedItems.Cast<XRayedFile>().ToArray())
                FileList.Items.Remove(item);

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

            bool trackFlow = TrackFlowCheckBox.Checked;
            bool trackExternal = TrackExternalCheckBox.Checked;
            bool trackAnon = TrackAnonCheckBox.Checked;
            bool trackFields = TrackFieldsCheckBox.Checked;
            bool trackInstances = TrackInstancesCheckBox.Checked;
            bool sidebySide = SidebySideCheckBox.Checked;
            bool doVerify = RunVerifyCheckbox.Checked;
            bool compileWithMS = MsToolsCheckbox.Checked;
            bool decompileAgain = DecompileAgainCheckbox.Checked;

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
                        XDecompile decompile = new XDecompile(intRoot, extRoot, item, OutputDir, files, trackFlow, trackExternal, trackAnon, trackFields, trackInstances);

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

                            if (decompileAgain)
                            {
                                string filename = Path.GetFileName(item.FilePath);
                                string compiler = compileWithMS ? "MS" : "Mono";

                                // create directories
                                var dir = Path.Combine(Application.StartupPath, "recompile", filename, compiler + "_original");
                                decompile.Decompile(item.FilePath, dir);

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
                    trackedObjects = root.SaveTree(OutputDir);


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

                    if (sidebySide)
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

                status(String.Format("Ready to Launch - {0:#,#} instructions added, {1:#,#} objects tracked", linesAdded, trackedObjects), "");

                RunInGui(() =>
                {
                    OptionsPanel.Enabled = true;
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
            XRayedFile item = GetSelectedItem();

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
            //string path = @"C:\RAID\Main\Dev\WellsResearch\PSDev - Copy\PixelScope\Application\bin\Debug PixelScopePro\XRay.dat";
            //XRay.TestInit(path);
            //return;

            try
            {
                XRayedFile item = GetSelectedItem();

                string path = Path.Combine(OutputDir, "XRay.dat");

                XRay.TestInit(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        XRayedFile GetSelectedItem()
        {
            XRayedFile item = FileList.SelectedItem as XRayedFile;

            return FileList.Items.Cast<XRayedFile>().FirstOrDefault(i => i.FileName.EndsWith(".exe"));
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

            if (SidebySideCheckBox.Checked)
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

        private void TestCompile_Click(object sender, EventArgs e)
        {
           // StringEval("y.xxx");
            ReCompile(true);
        }


       /* string EvalBody = @"
            using System;

            public delegate void Proc();

            public class Wrapper 
            { 
                public static object Set(string name, object value) 
                { 
                    AppDomain.CurrentDomain.SetData(name, value);
                    return value; 
                }

                public static object Get(string name) 
                { 
                    return AppDomain.CurrentDomain.GetData(name);
                }

                public static object Invoke(Proc proc) 
                { 
                    proc();
                    return null; 
                }

                public static object Eval(dynamic y) 
                { 
                    return {0}; 
                }
            }";

        string StringEval(string expr)
        {
            string program = EvalBody.Replace("{0}", expr);

            var x = CodeDomProvider.GetAllCompilerInfo();

            var provider = CodeDomProvider.CreateProvider("C#");
            CompilerParameters cp = new CompilerParameters();
            cp.ReferencedAssemblies.Add("System.Core.dll");
            cp.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
            cp.GenerateExecutable = false;
            cp.GenerateInMemory = true;

            CompilerResults results = provider.CompileAssemblyFromSource(cp, program);

            if (results.Errors.HasErrors)
            {
                if (results.Errors[0].ErrorNumber == "CS0029")
                    return StringEval("Invoke(delegate { " + expr + "; })");

                return results.Errors[0].ErrorText;
            }
            else
            {
                Assembly assm = results.CompiledAssembly;
                Type target = assm.GetType("Wrapper");
                MethodInfo method = target.GetMethod("Eval");

                object result = method.Invoke(null, new object[] { XRay });

                return result == null ? "null" : result.ToString();
            }
        }*/
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
