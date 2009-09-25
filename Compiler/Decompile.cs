using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using XLibrary;


namespace XBuilder
{
    class XDecompile
    {
        string OriginalPath;
        string DasmPath;
        string ILPath;
        string ILPathOriginal;
        string AsmDir;

        XNodeOut RootNode;
        XNodeOut CurrentNode;

        public static Random RndGen = new Random();

        StringBuilder XIL = new StringBuilder(4096);

        bool TrackThread = false;
        bool TrackConstruction = false;


        public XDecompile(XNodeOut root, string path)
        {
            RootNode = root;
            OriginalPath = path;
        }

        internal void Decompile()
        {
            string ildasm = Path.Combine(Application.StartupPath, "ildasm.exe");

            string name = Path.GetFileName(OriginalPath);

            // create directories
            AsmDir = Path.Combine(Application.StartupPath, name);
            Directory.CreateDirectory(AsmDir);

            // clear previous files
            foreach (string file in Directory.GetFiles(AsmDir))
                File.Delete(file);

            // remove spaces from name and move to dasm dir
            DasmPath = Path.Combine(AsmDir, Path.GetFileName(OriginalPath).Replace(' ', '_'));
            File.Copy(OriginalPath, DasmPath);
  
            // disassemble file
            string ILName = Path.GetFileNameWithoutExtension(DasmPath) + ".il";
            ProcessStartInfo info = new ProcessStartInfo(ildasm, Path.GetFileName(DasmPath) + " /utf8 /output=" + ILName);
            info.WorkingDirectory = AsmDir;
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;

            Process process = Process.Start(info);
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            ILPath = Path.Combine(AsmDir, ILName);

            // check output il for errors - no way to get ildasm to just output errrors
            using(StreamReader read = new StreamReader(File.OpenRead(ILPath)))
                while (!read.EndOfStream)
                {
                    string line = read.ReadLine();
                    if (line.Contains("has no valid CLR header and cannot be disassembled"))
                        throw new CompileError("Decompiling", ILPath, "Open IL file for more details");
                }

            ILPathOriginal = Path.Combine(AsmDir,  Path.GetFileNameWithoutExtension(DasmPath) + "_original.il");

            File.Delete(DasmPath); // so it can be replaced with asm exe
        }

        internal void ScanLines(List<string> assemblies)
        {
            CurrentNode = RootNode;

            InjectLibrary("XLibrary", "1:0:0:0");

            bool stripSig = false;

            using (StreamReader reader = new StreamReader(ILPath))
            {
                while (!reader.EndOfStream)
                {
                    string[] line = reader.SplitNextLine(XIL);

                    if (line.Length == 0)
                        continue;

                    else if (line[0] == ".assembly")
                    { 
                        // get last element, if in assemblies, replace with xray version
                        stripSig = false;
                        string assembly = line[line.Length - 1];

                        // assemblies are referenced externally by xray. prefix, internally namespace names are the same
                        if (assemblies.Contains(assembly))
                        {
                            line[line.Length - 1] = "XRay." + line[line.Length - 1];
                            XIL.RemoveLine();
                            XIL.AppendLine(String.Join(" ", line));
                            stripSig = true;
                        }
                    }
                        
                    // the result dll is changed so a strong sig links need to be removed
                    else if (line[0] == ".publickeytoken")
                    {
                        if(stripSig)
                            XIL.RemoveLine();
                    }

                    else if (line[0] == ".hash" && stripSig)
                    {
                        XIL.RemoveLine();

                        if (line[1] != "algorithm")
                        {
                            string nextLine = string.Join(" ", line).FilterComment();

                            while (!nextLine.Contains(")"))
                                nextLine = reader.ReadLine().FilterComment();
                        }
                    }

                    // remove assembly's public key
                    else if (line[0] == ".publickey")
                    {
                        XIL.RemoveLine();

                        string nextLine = string.Join(" ", line).FilterComment(); ;

                        while(!nextLine.Contains(")"))
                            nextLine = reader.ReadLine().FilterComment(); ;
                    }

                    else if (line[0] == ".class")
                    {
                        // read the whole class before the {
                        while (!line.Contains("{"))
                            line = line.Concat(reader.SplitNextLine(XIL)).ToArray();

                        // right before the class is extended that is the7 name
                        string name = line.TakeWhile(s => s != "extends" && s != "implements" && s != "{").LastOrDefault();

                        // add namespaces, and class, set current app obj to 
                        string[] namespaces = name.Split('.');

                        if (!line.Contains("nested"))
                        {
                            CurrentNode = RootNode;

                            for (int i = 0; i < namespaces.Length - 1; i++)
                                CurrentNode = CurrentNode.AddNode(namespaces[i], XObjType.Namespace);
                        }

                        CurrentNode = CurrentNode.AddNode(namespaces[namespaces.Length - 1], XObjType.Class);
                    }

                    else if (line[0] == ".method")
                    {
                        // read the whole method before the {
                        while (!line.Contains("{"))
                            line = line.Concat(reader.SplitNextLine(XIL)).ToArray();

                        // method is the name with the ( in it
                        string name = line.Where(s => s.Contains('(')).LastOrDefault(); //pinvokes can have afdaf( before method name

                        name = name.Substring(0, name.IndexOf('('));

                        CurrentNode = CurrentNode.AddNode(name, XObjType.Method);

                        // dont inject tracking code under these conditions
                        if (name.StartsWith("'") || // don't track generated methods
                            line.Contains("abstract") ||
                            line.Where(s => s.StartsWith("pinvokeimpl")).FirstOrDefault() != null ||
                            (line.Contains("runtime") && line.Contains("managed")))// runtime managed at end of function indicates the body should be empty
                        {
                            CurrentNode.Exclude = true;
                            continue;
                        }

                        // scan for entry, break on .maxstack or }
                        // read the whole method before the {
                        // order method, entry, custom, maxstack, IL_
                        bool entry = false;

                        while (!line.Contains(".maxstack"))
                        {
                            line = reader.SplitNextLine(XIL);
                            Debug.Assert(!line.Contains("}"));

                            if (line.Contains(".entrypoint"))
                                entry = true;

                            // read until custom and insert entry
                            // set entry to false
                            // after found maxstack, if entry still true then insert gui
                            if (entry && line.Contains(".custom"))
                            {
                                InjectGui();
                                entry = false;
                            }
                        }

                        if (line.Length > 1 && line[0] == ".maxstack" && (line[1].IndexOfAny(new char[]{'0', '1', '2'}) != -1))
                        {
                            XIL.RemoveLine();

                            int maxstack = int.Parse(line[1]);
                            if (maxstack == 0)
                                maxstack = 1; // for hit function
                            if (TrackThread && maxstack == 1)
                                maxstack = 2;
                            if (TrackConstruction && maxstack == 2 && (CurrentNode.Name == ".ctor" || CurrentNode.Name == "Finalize"))
                                maxstack = 3;

                            XIL.AppendLine(".maxstack " + maxstack); // increase stack enough for hit function - need room for thread, hit, constructor
                        }

                        if (entry)
                            InjectGui();

                        InjectMethodHit(CurrentNode);
                    }

                    else if (line[0] == ".property") // ignore for now
                    {
                        while (!line.Contains("}"))
                            line = reader.SplitNextLine(XIL);
                    }

                    else if (line[0] == ".field")
                    {
                        //todo - add option to track fields
                        //string name = line.LastOrDefault();

                        //XNodeOut fieldNode = CurrentNode.AddNode(name, XObjType.Field);
                        //fieldNode.Lines = 1;
                    }

                    else if (CurrentNode.ObjType == XObjType.Method)
                    {
                        /* (line[0].StartsWith(".entrypoint"))
                        {
                            // inject gui after .custom instance void [mscorlib]System.STAThreadAttribute::.ctor() = ( 01 00 00 00 )
                            // if not then thread state will not be set for app's gui

                            // save spot

                            bool found = false;
                            List<string> lines = new List<string>();

                            string scan = "";

                            while (!scan.Contains("} // end of method"))
                            {
                                scan = reader.ReadLine();
                                lines.Add(scan);

                                if (scan.Contains(".custom"))
                                {
                                    found = true;
                                    break;
                                }
                            }

                            if (!found)
                                InjectGui();

                            lines.ForEach(s => XIL.AppendLine(s));

                            if(found)
                                InjectGui();
                        }

                        else if (line[0].StartsWith(".maxstack") && line[1] == "1" && !CurrentNode.Exclude)
                        {
                            XIL.RemoveLine();
                            XIL.AppendLine(".maxstack 2"); // increase stack enough for hit function
                        }*/

                        if (line[0] == "{") // try, catch, finallys, inside one another
                        {
                            CurrentNode.Indent++;
                        }

                        else if (line[0] == "}") // try, catch, finallys, inside one another
                        {
                            if (CurrentNode.Indent == 0)
                                CurrentNode = CurrentNode.Parent as XNodeOut;

                            else
                                CurrentNode.Indent--;
                        }

                        CurrentNode.Lines++;
                    }

                    else if (CurrentNode.ObjType == XObjType.Class)
                    {
                        if (line[0] == "}") // try, catch, finallys, inside one another
                            CurrentNode = CurrentNode.Parent as XNodeOut;
                    }
                }
            }

            // change method call refs from old assembly to new
            foreach (string assembly in assemblies)
                XIL.Replace("[" + assembly + "]", "[XRay." + assembly + "]");
        }

        private void InjectLibrary(string name, string version)
        {
            XIL.AppendLine(".assembly extern " + name);
            XIL.AppendLine("{");
            XIL.AppendLine("  .ver " + version);
            XIL.AppendLine("}");
        }

        private void InjectGui()
        {
            // check mscorlib is in assembly

            XIL.AppendLine("call       void [XLibrary]XLibrary.XRay::Init()");
        }

        private void InjectMethodHit(XNode node)
        {
            if (TrackThread)
            {
                XIL.AppendLine("call       class [mscorlib]System.Threading.Thread [mscorlib]System.Threading.Thread::get_CurrentThread()");
                XIL.AppendLine("callvirt   instance int32 [mscorlib]System.Threading.Thread::get_ManagedThreadId()");
                XIL.AppendLine("ldc.i4     " + node.ID.ToString());
                XIL.AppendLine("call       void [XLibrary]XLibrary.XRay::Hit(int32, int32)");
            }
            else
            {
                XIL.AppendLine("ldc.i4     " + node.ID.ToString());
                XIL.AppendLine("call       void [XLibrary]XLibrary.XRay::Hit(int32)");
            }

            if (!TrackConstruction)
                return;

            if (node.Name == ".ctor")
            {
                XIL.AppendLine("ldc.i4     " + node.Parent.ID.ToString());
                XIL.AppendLine("call       void [XLibrary]XLibrary.XRay::Constructed(int32)");
            }
            else if (node.Name == "Finalize")
            {
                XIL.AppendLine("ldc.i4     " + node.Parent.ID.ToString());
                XIL.AppendLine("call       void [XLibrary]XLibrary.XRay::Deconstructed(int32)");
            }
        }

        internal string Compile()
        {
            // save original
            File.Copy(ILPath, ILPathOriginal, true);

            // write new IL
            byte[] buffer = UTF8Encoding.UTF8.GetBytes(XIL.ToString());

            using (FileStream outFile = new FileStream(ILPath, FileMode.Truncate))
                outFile.Write(buffer);

            string windir = Environment.GetEnvironmentVariable("windir");
            string ilasm = Path.Combine(windir, "Microsoft.NET\\Framework\\v2.0.50727\\ilasm.exe");

            // C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\ilasm.exe RiseOp

            // assemble file
            ProcessStartInfo info = new ProcessStartInfo(ilasm, Path.GetFileName(ILPath));
            info.WorkingDirectory = AsmDir;
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;

            if (OriginalPath.EndsWith(".dll"))
                info.Arguments += " /DLL";

            Process process = Process.Start(info);
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (output.Contains("***** FAILURE *****"))
                throw new CompileError("Error Recompiling", DasmPath, output);

            // copy XLibrary to final destination
            File.Copy(Path.Combine(Application.StartupPath, "XLibrary.dll"), Path.Combine(Path.GetDirectoryName(OriginalPath), "XLibrary.dll"), true);

            // copy compiled file
            string recompiledPath = Path.Combine(Path.GetDirectoryName(OriginalPath), "XRay." + Path.GetFileName(OriginalPath));

            File.Delete(recompiledPath); // delete prev compiled file

            File.Copy(DasmPath, recompiledPath);

            return recompiledPath;
        }

        internal static void Verify(string path)
        {
            // need to verify in final location because dll dependencies are checked as well

            // pe verify assembled file
            ProcessStartInfo info = new ProcessStartInfo("PEVerify.exe", Path.GetFileName(path));
            info.WorkingDirectory = Path.GetDirectoryName(path);
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;

            Process process = Process.Start(info);
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (output.Contains("Error Verifying") || output.Contains("Invalid option:"))
                throw new CompileError("Error Verifying", path, output);
        }
    }

    internal class CompileError : Exception 
    {
        internal string Summary { get; private set; }

        internal CompileError(string process, string path, string output)
        {
            Summary = process + " " + Path.GetFileName(path) + "\r\n\r\n" + output;
        }
    }
}
