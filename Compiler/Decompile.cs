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
        string OutputDir;

        bool SideBySide; // if true change the file name and refs to xray.etc..

        XNodeOut RootNode;
        XNodeOut FileNode;
        XNodeOut CurrentNode;

        public static Random RndGen = new Random();

        StringBuilder XIL = new StringBuilder(4096);

        public bool TrackFlow = true; // if this is true, then track thread needs to be true
        public bool TrackInstances = false;
        public bool TrackExternal = true;
        public bool TrackAnon = false;

        public XDecompile(XNodeOut root, string sourcePath, string outDir)
        {
            RootNode = root;
            FileNode = root.AddNode(Path.GetFileName(sourcePath), XObjType.File);
            OriginalPath = sourcePath;
            OutputDir = outDir;
            SideBySide = Path.GetDirectoryName(sourcePath) == outDir;
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
                        throw new CompileError("Decompiling", ILPath, "Has no valid CLR header and cannot be disassembled");
                }

            ILPathOriginal = Path.Combine(AsmDir,  Path.GetFileNameWithoutExtension(DasmPath) + "_original.il");
          
            // save original
            File.Copy(ILPath, ILPathOriginal, true);

            File.Delete(DasmPath); // so it can be replaced with asm exe
        }

        internal void ScanLines(List<string> assemblies, bool test)
        {
            XIL.Length = 0;
            CurrentNode = FileNode;
  
            if(!test)
                InjectLibrary("XLibrary", "1:0:0:0");

            bool stripSig = false;

            using (StreamReader reader = new StreamReader(ILPathOriginal))
            {
                while (!reader.EndOfStream)
                {
                    string[] line = reader.SplitNextLine(XIL);

                    if (test || line.Length == 0)
                        continue;

                    else if (line[0] == ".assembly")
                    { 
                        // get last element, if in assemblies, replace with xray version
                        stripSig = false;
                        string assembly = line.Last();


                        // assemblies are referenced externally by xray. prefix, internally namespace names are the same
                        if (assemblies.Contains(assembly))
                        {
                            if (SideBySide)
                            {
                                line[line.Length - 1] = "XRay." + line.Last();
                                XIL.RemoveLine();
                                XIL.AppendLine(String.Join(" ", line));
                            }

                            stripSig = true;
                        }
                    }
 
                    // the result dll is changed so a strong sig links need to be removed
                    else if (line[0] == ".publickeytoken")
                    {
                        if(stripSig)
                            XIL.RemoveLine();
                    }

                    else if (line[0] == ".file")
                    {
                        // embedded files have a .hash that we won't be messing with, they require a hash
                        stripSig = false;
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

                        while (!nextLine.Contains(")"))
                            nextLine = reader.ReadLine().FilterComment();
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
                            CurrentNode = FileNode;

                            for (int i = 0; i < namespaces.Length - 1; i++)
                                CurrentNode = CurrentNode.AddNode(namespaces[i], XObjType.Namespace);
                        }

                        string className = namespaces.Last();
                        int pos = className.LastIndexOf('`');
                        if (pos != -1)
                            className = className.Substring(0, pos);

                        CurrentNode = CurrentNode.AddNode(className, XObjType.Class);

                        // exclude if we dont track anon classes
                        if (!TrackAnon && className.StartsWith("'"))
                            CurrentNode.Exclude = true;
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
                        if (line.Contains("abstract") ||
                            line.Where(s => s.StartsWith("pinvokeimpl")).FirstOrDefault() != null ||
                            (line.Contains("runtime") && line.Contains("managed")) || // 'runtime managed' / 'managed internalcall' at end of function indicates the body should be empty
                            (line.Contains("managed") && line.Contains("internalcall")))
                        {
                            CurrentNode.Exclude = true;
                            continue;
                        }

                        // exclude if we dont track anony methods, but dont continue cause entry point could still be inside
                        if (!TrackAnon && name.StartsWith("'"))
                            CurrentNode.Exclude = true;

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

                        if (!CurrentNode.Exclude)
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

                        else if (TrackFlow && !CurrentNode.Exclude && line.Length > 1 && line[1] == "ret")
                        {
                            Debug.Assert(line.Length == 2);

                            XIL.RemoveLine(); // remove ret call

                            XIL.AppendLine();
                            AddLine(line[0] + " nop // XRay - Redirected return address"); // anything jumping to return, will jump here instead and allow us to log exit

                            InjectMethodExit(CurrentNode);

                            AddLine("ret"); // put ret call back in
                        }

                        else if (line.Length > 1 && (TrackFlow || TrackExternal) && line[1].EndsWith(".s"))
                        {
                            // when we insert code we add more instructions, br is the branch instruction
                            // and br.s is the short version allowing a max jump of 255 places which may
                            // not be valid after our injection.  Strip the .s, and run ilasm with /optimize
                            // to add them back in

                            Debug.Assert(line.Length == 3);
                            XIL.RemoveLine();
                            line[1] = line[1].Replace(".s", "");
                            AddLine(string.Join(" ", line) + " // XRay - removed .s");
                        }

                        // external method call tracking
                        if (TrackExternal && TrackFlow && !CurrentNode.Exclude && line.Length > 1 &&
                            (line[1].StartsWith("constrained.") || line[1].StartsWith("call") ||
                             line[1].StartsWith("callvirt") || line[1].StartsWith("calli")))
                        {

                            Debug.Assert(!line[1].StartsWith("calli")); // whats the format of calli?

                            // any line starting with a constrained prefix is immediately followed by a call virt
                            string[] constrainedLine = null;
                            if (line[1].StartsWith("constrained."))
                            {
                                XIL.RemoveLine(); // save constrained line a inject after external method tracking
                                constrainedLine = line;
                                line = reader.SplitNextLine(XIL); // read in callvirt
                            }

                            string parse = string.Join(" ", line);

                            // get function name
                            if (!parse.Contains("::"))
                                continue; // if no :: then caller is accessing a global internal function that is already tracked

                            int pos = parse.LastIndexOf('(');
                            Debug.Assert(pos != -1);
                            int pos2 = parse.LastIndexOf("::") + 2;
                            Debug.Assert(pos2 != 1 && pos2 < pos);
                            string functionName = parse.Substring(pos2, pos - pos2);

                            parse = parse.Substring(0, pos2 - 2); // cut out what we just parsed

                            // strip template info, read forward mark if in template
                            string withoutTemplate = "";
                            int inTemplate = 0;
                            for (int i = 0; i < parse.Length; i++)
                            {
                                if (parse[i] == '<')
                                    inTemplate++;
                                else if (parse[i] == '>')
                                    inTemplate--;
                                else if (inTemplate == 0)
                                    withoutTemplate += parse[i];
                            }

                            parse = withoutTemplate;

                            // now should just be namespace and extern dec to space
                            pos = parse.LastIndexOf(' ');
                            parse = parse.Substring(pos + 1);

                            // we only care if this function is external
                            if (!parse.StartsWith("["))
                                continue;

                            pos = parse.IndexOf("]");
                            string external = parse.Substring(1, pos - 1);
                            string namespaces = parse.Substring(pos + 1);

                            pos = namespaces.LastIndexOf('`');
                            if (pos != -1)
                                namespaces = namespaces.Substring(0, pos);

                            // if already tracked, skip
                            if (assemblies.Contains(external))
                                continue;

                            // add external file to root
                            XNodeOut extRoot = RootNode.Nodes.First(n => n.External) as XNodeOut;
                            XNodeOut node = extRoot.Nodes.FirstOrDefault(n => n.Name == external) as XNodeOut;

                            if (node == null)
                                node = extRoot.AddNode(external, XObjType.File);

                            // traverse or add namespace to root
                            string[] names = namespaces.Split('.');

                            for (int i = 0; i < names.Length; i++)
                            {
                                string name = names[i];

                                XNodeOut next = node.Nodes.FirstOrDefault(n => n.Name == name) as XNodeOut;

                                XObjType objType = (i == names.Length - 1) ? XObjType.Class : XObjType.Namespace;
                                node = next ?? node.AddNode(name, objType);
                            }

                            node = node.AddNode(functionName, XObjType.Method);
                            node.Lines = 1;


                            // add wrapping for external tracking

                            // remove function
                            XIL.RemoveLine();

                            // inject method enter - re-route IL address to function to ensure it is logged
                            XIL.AppendLine();

                            string address = (constrainedLine != null) ? constrainedLine[0] : line[0];
                            AddLine(address + " nop // XRay - Redirect external method begin");
                            XIL.AppendLine();
                            InjectMethodEnter(node);

                            // add function line - strip IL address
                            if (constrainedLine != null)
                                XIL.AppendLine(string.Join(" ", constrainedLine.Skip(1).ToArray()));

                            string nextLine = string.Join(" ", line.Skip(1).ToArray());
                            XIL.AppendLine(nextLine);

                            // read over rest of function until ')'
                            while (!nextLine.Contains(")"))
                            {
                                nextLine = reader.ReadLine();
                                XIL.AppendLine(nextLine);
                            }

                            // inject exit
                            InjectMethodExit(node);

                        }


                        if (line[0] == "{") // try, catch, finallys, inside one another
                        {
                            CurrentNode.Indent++;
                            CurrentNode.IndentString += "  ";

                            if (inCatch && TrackFlow && !CurrentNode.Exclude)
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
            if(SideBySide)
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

        private void InjectMethodEnter(XNodeOut node)
        {
            Debug.Assert(!node.Exclude);

            XIL.AppendLine();
            AddLine("ldc.i4  " + node.ID.ToString() + " // XRay - Method enter");
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

        private void InjectMethodExit( XNodeOut node)
        {
            Debug.Assert(!node.Exclude);
            Debug.Assert(TrackFlow);

            if (AddsDone >= AllowedAdds)
                return;

            XIL.AppendLine();
            AddLine("ldc.i4  " + node.ID.ToString() + " // XRay - Method Exit" );
            AddLine("call    void [XLibrary]XLibrary.XRay::MethodExit(int32)");
            XIL.AppendLine();

            AddsDone++;
        }

        Dictionary<string, bool> errorMap = new Dictionary<string, bool>();

        private void InjectMethodCatch(XNodeOut node)
        {
            Debug.Assert(!node.Exclude);
            Debug.Assert(TrackFlow);

            XIL.AppendLine();
            AddLine("ldc.i4  " + node.ID.ToString() + " // XRay - Method Catch");
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

            // copy compiled file
            string newName = Path.GetFileName(OriginalPath);
            if (SideBySide)
                newName = "XRay." + newName;

            string recompiledPath = Path.Combine(OutputDir, newName);

            File.Delete(recompiledPath); // delete prev compiled file

            File.Copy(DasmPath, recompiledPath, true);

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

        internal static void PrepareOutputDir(string sourcePath, string destPath)
        {
            bool sideBySide = sourcePath == destPath;

            // if seperate output dir
            if (!sideBySide)
            {
                Directory.CreateDirectory(destPath);

                // re-copy exe/dlls from original path
                foreach (string file in Directory.GetFiles(sourcePath))
                    if (file.EndsWith(".exe") || file.EndsWith(".dll"))
                        File.Copy(file, Path.Combine(destPath, Path.GetFileName(file)), true);
            }

            // copy XLibrary to final destination
            File.Copy(Path.Combine(Application.StartupPath, "XLibrary.dll"), Path.Combine(destPath, "XLibrary.dll"), true);
        }
    }

    internal class CompileError : Exception 
    {
        internal string Summary { get; private set; }

        // for throwing
        internal CompileError(string process, string path, string output)
        {
            Summary = process + " " + Path.GetFileName(path) + "\r\n\r\n" + output;
        }


        // for re-throwing
        internal CompileError(string summary)
        {
            Summary = summary;
        }
    }
}
