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

        bool TrackFlow = true; // if this is true, then track thread needs to be true
        bool TrackInstances = false;
        

        public XDecompile(XNodeOut root, string path)
        {
            RootNode = root;
            OriginalPath = path;
        }

        internal void Decompile()
        {
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
            string ildasm = Path.Combine(Application.StartupPath, "ildasm.exe");
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
          
            // save original
            File.Copy(ILPath, ILPathOriginal, true);

            File.Delete(DasmPath); // so it can be replaced with asm exe
        }

        internal void ScanLines(List<string> assemblies)
        {
            XIL.Length = 0;
            CurrentNode = RootNode;

            InjectLibrary("XLibrary", "1:0:0:0");

            bool stripSig = false;

            using (StreamReader reader = new StreamReader(ILPathOriginal))
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

                        if (assembly == "DragonFly.Storm.UI")
                            continue; // gets storm to compile with depends

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

                            // inject gui after .custom instance void [mscorlib]System.STAThreadAttribute::.ctor() = ( 01 00 00 00 )
                            // if not then thread state will not be set for app's gui
                            if (line.Contains(".entrypoint"))
                                entry = true;
                        }

                        if (line.Length > 1 && line[0] == ".maxstack")
                        {
                            XIL.RemoveLine();

                            int maxstack = int.Parse(line[1]);
                            if (maxstack == 0)
                                maxstack = 1; // for method enter function

                            // if we need to increase stack further for other added functions, 
                            // this is how to do it, checksIndexOfAny  above, and check above this
                            if (entry && maxstack < 2)
                                maxstack = 2;

                            // needs to push 1 more item on stack above whats needed for return 
                            // elements so that MethodExit can run
                            if (TrackFlow)
                                maxstack++;

                            XIL.AppendLine(".maxstack " + maxstack); // increase stack enough for hit function - need room for thread, hit, constructor

                            if (entry)
                            {
                                InjectGui();
                                entry = false;
                            }
                        }

                        if (entry)
                            InjectGui();

                        InjectMethodEnter(CurrentNode);
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
                        bool inCatch = false;

                        if (line[0] == "catch")
                        {
                            line = reader.SplitNextLine(XIL);
                            inCatch = true; // inject after indent set
                        }

                        else if (line.Length > 1 && line[1] == "ret")
                        {
                            Debug.Assert(line.Length == 2);
                            InjectMethodExit(line, CurrentNode);
                        }

                        else if (line.Length > 1 && line[1].EndsWith(".s"))
                        {
                            // when we insert code we add more instructions, br is the branch instruction
                            // and br.s is the short version allowing a max jump of 255 places which may
                            // not be valid after our injection.  Strip the .s, and run ilasm with /optimize
                            // to add them back in

                            Debug.Assert(line.Length == 3); 
                            XIL.RemoveLine();
                            line[1] = line[1].Replace(".s", "");
                            XIL.AppendLine(string.Join(" ", line));
                        }


                        if (line[0] == "{") // try, catch, finallys, inside one another
                        {
                            CurrentNode.Indent++;
                            CurrentNode.IndentString += "  ";

                            if (inCatch)
                                InjectMethodCatch(CurrentNode);
                        }

                        else if (line[0] == "}") // try, catch, finallys, inside one another
                        {
                            if (CurrentNode.Indent == 0)
                                CurrentNode = CurrentNode.Parent as XNodeOut;

                            else
                            {
                                CurrentNode.Indent--;
                                CurrentNode.IndentString = CurrentNode.IndentString.Substring(2);
                            }
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
            XIL.AppendLine();
            XIL.AppendLine("// XRay");
            XIL.AppendLine(".assembly extern " + name);
            XIL.AppendLine("{");
            XIL.AppendLine("  .ver " + version);
            XIL.AppendLine("}");
            XIL.AppendLine();
        }

        private void InjectGui()
        {
            // check mscorlib is in assembly
            // make sure max stack is big enough for init parameters

            XIL.AppendLine();
            AddLine("// XRay");
            AddLine("ldc.i4  " + (TrackFlow ? "1" : "0"));
            AddLine("ldc.i4  " + (TrackInstances ? "1" : "0"));
            AddLine("call    void [XLibrary]XLibrary.XRay::Init(bool,bool)");
            XIL.AppendLine();
        }

        private void AddLine(string line)
        {
            XIL.AppendLine(CurrentNode.IndentString + line);
        }

        private void InjectMethodEnter(XNode node)
        {
            XIL.AppendLine();
            AddLine("// XRay");
            AddLine("ldc.i4  " + node.ID.ToString());
            AddLine("call    void [XLibrary]XLibrary.XRay::MethodEnter(int32)");

            if (TrackInstances)
                if (node.Name == ".ctor")
                {
                    AddLine("ldc.i4  " + node.Parent.ID.ToString());
                    AddLine("call    void [XLibrary]XLibrary.XRay::Constructed(int32)");
                }
                else if (node.Name == "Finalize")
                {
                    AddLine("ldc.i4  " + node.Parent.ID.ToString());
                    AddLine("call    void [XLibrary]XLibrary.XRay::Deconstructed(int32)");
                }

            XIL.AppendLine();
        }

        public int AllowedAdds;
        public int AddsDone;

        private void InjectMethodExit(string[] line, XNodeOut node)
        {
            if (!TrackFlow || node.Exclude)
                return;

            if (AddsDone >= AllowedAdds)
                return;

            XIL.RemoveLine(); // remove ret call

            XIL.AppendLine();
            AddLine("// XRay");
            AddLine(line[0] + " nop"); // anything jumping to return, will jump here instead and allow us to log exit

            AddLine("ldc.i4  " + node.ID.ToString());
            AddLine("call    void [XLibrary]XLibrary.XRay::MethodExit(int32)");

            AddLine("ret"); // put ret call back in

            AddsDone++;
        }

        Dictionary<string, bool> errorMap = new Dictionary<string, bool>();

        private void InjectMethodCatch(XNodeOut node)
        {
            if (!TrackFlow || node.Exclude)
                return;

            AddLine("// XRay");
            AddLine("ldc.i4  " + node.ID.ToString());
            AddLine("call    void [XLibrary]XLibrary.XRay::MethodCatch(int32)");
            XIL.AppendLine();
        }

        internal string Compile()
        {
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
            info.Arguments += " /optimize"; // turn br instructions to br.s

            if (OriginalPath.EndsWith(".dll"))
                info.Arguments += " /DLL ";

            Process process = Process.Start(info);
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (!output.Contains("Operation completed successfully"))
            {
                // for some reason running ilasm manually will give error details, while the process above will not
                output += "\r\n\r\nTry running ilasm manually for error details.";

                throw new CompileError("Error Recompiling", DasmPath, output);
            }

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


            if (output.Contains("All Classes and Methods in ") &&
                output.Contains("Verified"))
                return;
            else
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
