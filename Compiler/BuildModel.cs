using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using XLibrary;
using System.Threading;
using System.Diagnostics;

namespace XBuilder
{


    public class BuildModel
    {
        public string SourceDir;
        public string OutputDir;
        public string DatPath;

        public List<XRayedFile> Files = new List<XRayedFile>();

        // options
        public bool TrackFlow;
        public bool TrackExternal;
        public bool TrackAnon;
        public bool TrackFields;
        public bool TrackInstances;
        public bool ReplaceOriginal;
        public bool DoVerify;
        public bool CompileWithMS;
        public bool DecompileAgain;
        public bool ShowUiOnStart;
        public bool SaveMsil;
        public bool DecompileCSharp;

        public string BuildStatus = "";
        public string BuildError = "";
        public bool BuildSuccess;
        public Thread BuildThread;


        public void AddFiles(string[] paths)
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

                Files.Add(new XRayedFile(path));
            }
        }

        public void UpdateOutputPath()
        {
            if (Files.Count == 0)
                SourceDir = null;

            if (SourceDir == null)
            {
                OutputDir = null;
                return;
            }

            OutputDir = SourceDir;
            DatPath = Path.Combine(OutputDir, "XRay.dat");
        }

        public void Recompile(bool test)
        {
            if (Files.Count == 0)
                return;

            BuildStatus = "";

            BuildThread = new Thread(() =>
            {
                BuildStatus = "";
                BuildError = "";
                BuildSuccess = false;

                string stepname = "";

                long linesAdded = 0;
                long trackedObjects = 0;

                try
                {
                    BuildStatus = "Checking";
                    if (XDecompile.CheckIfAlreadyXRayed(Files) &&
                        !TryRestoreBackups())
                    {
                        return;
                    }

                    BuildStatus = "Preparing";
                    XDecompile.PrepareOutputDir(SourceDir, OutputDir);

                    string errorLog = "";

                    XNodeOut.NextID = 0;
                    XNodeOut root = new XNodeOut(null, "root", XObjType.Root);
                    XNodeOut extRoot = root.AddNode("Not XRayed", XObjType.External);
                    XNodeOut intRoot = root.AddNode("XRayed", XObjType.Internal);

                    // init root file nodes so links can be made for processed fields before they are directly xrayed
                    foreach (var item in Files)
                        item.FileNode = intRoot.AddNode(Path.GetFileName(item.FilePath), XObjType.File);


                    foreach (var item in Files)
                    {
                        var decompile = new XDecompile(intRoot, extRoot, item, this);

                        try
                        {
                            if (CompileWithMS)
                            {
                                BuildStatus = "Decompiling " + item.FileName;
                                decompile.MsDecompile();

                                // used for debugging tricky ilasm errors
                                //for (int i = 0; i < int.MaxValue; i++)
                                // {
                                decompile.AllowedAdds = int.MaxValue; // i;
                                decompile.AddsDone = 0;

                                BuildStatus = "Scanning " + item.FileName;
                                decompile.ScanLines(test);

                                BuildStatus = "Recompiling " + item.FileName;
                                item.RecompiledPath = decompile.Compile();
                                // }
                            }
                            else
                            {
                                BuildStatus = "Recompiling " + item.FileName;
                                decompile.MonoRecompile();
                            }

                            // remove signature
                            // files that arent signed are coming up as signed meaning this would probably corrupt a file
                            // also not sure if checked anymore - http://msdn.microsoft.com/en-us/library/cc713694.aspx
                            //var meta = new MetaInfo(item.RecompiledPath);
                            //if (meta.Load())
                            //    meta.RemoveSignature();

                            // decompile to check
                            if (DecompileAgain)
                            {
                                string filename = Path.GetFileName(item.FilePath);
                                string compiler = CompileWithMS ? "MS" : "Mono";

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
                            BuildError = "Error recompiling: " + ex.Message + "\r\n" + ex.StackTrace;
                            BuildStatus = "Error on " + item.FileName;
                            return;
                        }

                        linesAdded += decompile.LinesAdded;
                    }

                    // save node map before verifying because we dont want bogus verify 
                    // errors from preventing the dat file form being made
                    BuildStatus = "Saving Map";

                    var settings = new Dictionary<string, string>();

                    settings["Version"] = XRay.BuilderVersion;

                    if (Pro.Verified)
                        settings["Pro"] = Pro.SignedFile;

                    trackedObjects = root.SaveTree(DatPath, settings);


                    // verify last and aggregate errors'
                    if (DoVerify)
                        foreach (var item in Files)
                        {
                            try
                            {
                                BuildStatus = "Verifying " + item.FileName;
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

                    if (!ReplaceOriginal)
                        summary = summary.Replace("Unable to resolve token.",
                            "Unable to resolve token. (Try turning off side by side)");

                    //todo token error - turn off side by side

                    string logPath = Path.Combine(Application.StartupPath, "errorlog.txt");
                    File.WriteAllText(logPath, summary);
                    Process.Start(logPath);
                }
                catch (Exception ex)
                {
                    BuildError = "Error during " + stepname + ": " + ex.Message;
                }

                BuildStatus = String.Format("Success! {0:#,#} instructions added, {1:#,#} objects tracked", linesAdded, trackedObjects);
                BuildSuccess = true;
            });
            
            BuildThread.Start();
        }


        private bool TryRestoreBackups()
        {
            var result = MessageBox.Show("Some of these files have already been XRayed. Try to restore originals?", "Problem", MessageBoxButtons.YesNo);

            // dont just ignore error because we dont want to backup an xrayed version over an original version and have the user lose their original file

            if (result == DialogResult.No)
            {
                BuildStatus = "Recompile canceled";
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
            if (XDecompile.CheckIfAlreadyXRayed(Files))
            {
                BuildStatus = "Failed to restore backup";
                return false;
            }

            BuildStatus = "Restored backups";
            return true;
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
