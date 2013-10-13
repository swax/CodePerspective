using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using XLibrary;
using System.Threading;
using System.Diagnostics;
using System.Security.Cryptography;

namespace XBuilder
{
    public class BuildModel
    {
        public string SourceDir;
        public string OutputDir;
        public string DatDir;

        public List<XRayedFile> Files = new List<XRayedFile>();

        // options
        public bool TrackFunctions = true;
        public bool TrackFlow = true;
        public bool TrackExternal = true;
        public bool TrackAnon = true;
        public bool TrackFields = true;
        public bool TrackInstances = true;
        public bool TrackReturnValue = false;
        public bool TrackParameters = false;
        public bool StaticAnalysis = true;

        public bool ReplaceOriginal;
        public bool CompileWithMS;
        public bool DecompileAgain;
        public bool SaveMsil = true;
        public bool DecompileCSharp;

        public bool EnableLocalViewer = true;
        public bool ShowViewerOnStart = true;
        public bool EnableIpcServer = true;
        
        public bool EnableTcpServer;
        public int TcpListenPort;
        public string EncryptionKey;

        public string BuildStatus = "";
        public string BuildError = "";
        public bool BuildSuccess;
        public Thread BuildThread;

        public BuildModel()
        {
            TcpListenPort = XRay.RndGen.Next(3000, 10000);
            EncryptionKey = Utilities.BytestoHex(new RijndaelManaged().Key);
        }

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
            DatDir = OutputDir;
        }

        public void Recompile(bool test)
        {
            if (Files.Count == 0)
                return;

            BuildStatus = "";
            BuildError = "";
            BuildSuccess = false;


            BuildThread = new Thread(() =>
            {
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
                   
                    // copy XLibrary to final destination
                    CopyLocalToOutputDir("XLibrary.dll", OutputDir);

                    if (EnableLocalViewer)
                    {
                        CopyLocalToOutputDir("OpenTK.dll", OutputDir);
                        CopyLocalToOutputDir("OpenTK.GLControl.dll", OutputDir);
                        CopyLocalToOutputDir("QuickFont.dll", OutputDir);
                    }

                    string errorLog = "";

                    XNodeOut.NextID = 0;
                    XNodeOut root = new XNodeOut(null, "root", XObjType.Root);
                    XNodeOut extRoot = root.AddNode("Not XRayed", XObjType.External);
                    XNodeOut intRoot = root.AddNode("XRayed", XObjType.Internal);

                    // init root file nodes so links can be made for processed fields before they are directly xrayed
                    foreach (var item in Files)
                        item.FileNode = intRoot.AddNode(Path.GetFileName(item.FilePath), XObjType.File);

                    var callMap = new Dictionary<int, FunctionCall>();
                    var initMap = new Dictionary<int, FunctionCall>();


                    foreach (var item in Files)
                    {
                        var decompile = new XDecompile(this, intRoot, extRoot, item, callMap, initMap);

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

                    if (EnableLocalViewer)
                    {
                        settings["EnableLocalViewer"] = EnableLocalViewer.ToString();
                        settings["EnableIpcServer"] = EnableIpcServer.ToString();
                        settings["ShowViewerOnStart"] = ShowViewerOnStart.ToString();
                    }

                    if (EnableTcpServer)
                    {
                        settings["EnableTcpServer"] = EnableTcpServer.ToString();
                        settings["TcpListenPort"] = TcpListenPort.ToString();
                        settings["EncryptionKey"] = EncryptionKey;
                    }

                    settings["FunctionCount"] = XNodeOut.NextID.ToString();

                    var writePath = Path.Combine(OutputDir, "XRay.dat");
                    trackedObjects = SaveDat(writePath, settings, root, callMap, initMap);

                    if (errorLog.Length > 0)
                    {
                        string logPath = Path.Combine(Application.StartupPath, "errorlog.txt");
                        File.WriteAllText(logPath, errorLog);
                        Process.Start(logPath);
                    }
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

        public long SaveDat(string path, Dictionary<string, string> settings, XNodeOut root, 
                            Dictionary<int, FunctionCall> callMap, Dictionary<int, FunctionCall> initMap)
        {
            long trackedObjects = 0;

            root.ComputeSums();

            byte[] temp = new byte[4096];

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                // save settings
                foreach (var setting in settings)
                    WriteSetting(stream, setting.Key, setting.Value);

                // save nodes
                trackedObjects += root.WriteNode(stream);

                // save call map
                SaveCallMap(stream, XPacketType.CallMap, callMap);
                SaveCallMap(stream, XPacketType.InitMap, initMap);
            }

            return trackedObjects;
        }

        public void WriteSetting(FileStream stream, string name, string value)
        {
            long startPos = stream.Length;

            // write empty size of packet to be set at the end of the function
            stream.Write(BitConverter.GetBytes(0));

            stream.WriteByte((byte)XPacketType.Setting);

            // name
            WriteString(stream, name);

            // value
            WriteString(stream, value);

            // write size of packet
            stream.Position = startPos;
            stream.Write(BitConverter.GetBytes((int)(stream.Length - startPos)));
            stream.Position = stream.Length;
        }

        static public void WriteString(FileStream stream, string str)
        {
            if (str.Length == 0)
            {
                stream.Write(BitConverter.GetBytes(0));
                return;
            }

            byte[] buff = UTF8Encoding.UTF8.GetBytes(str);
            stream.Write(BitConverter.GetBytes(buff.Length));
            stream.Write(buff);
        }

        private void SaveCallMap(FileStream stream, XPacketType type, Dictionary<int, FunctionCall> callMap)
        {
            long startPos = stream.Length;

            // write empty size of packet to be set at the end of the function
            stream.Write(BitConverter.GetBytes(0));

            stream.WriteByte((byte)type);

            // write the length of map
            stream.Write(BitConverter.GetBytes(callMap.Count));

            // write the call pairs
            foreach (var call in callMap.Values)
            {
                stream.Write(BitConverter.GetBytes(call.Source));
                stream.Write(BitConverter.GetBytes(call.Destination));
            }

            // write size of packet
            stream.Position = startPos;
            stream.Write(BitConverter.GetBytes((int)(stream.Length - startPos)));
            stream.Position = stream.Length;
        }

        private static void CopyLocalToOutputDir(string filename, string destPath)
        {
            File.Copy(Path.Combine(Application.StartupPath, filename), Path.Combine(destPath, filename), true);
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
