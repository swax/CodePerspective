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
        string AsmDir;

        List<string> Assemblies = new List<string>();

        XNodeOut RootNode;
        XNodeOut CurrentNode;

        public static Random RndGen = new Random();

        StringBuilder XIL = new StringBuilder(4096);


        public XDecompile(XNodeOut root, string path, List<string> assemblies)
        {
            RootNode = root;
            OriginalPath = path;
            Assemblies = assemblies;
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
            ProcessStartInfo info = new ProcessStartInfo(ildasm, Path.GetFileName(DasmPath) + " /output=" + ILName);
            info.WorkingDirectory = AsmDir;
            Process.Start(info).WaitForExit();

            ILPath = Path.Combine(AsmDir, ILName);

            File.Delete(DasmPath); // so it can be replaced with asm exe
        }

        internal void ScanLines()
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
                        if(Assemblies.Contains(assembly))
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

                    else if (line[0] == ".hash")
                    {
                        XIL.RemoveLine();
                    }

                    else if (line[0] == ".publickey")
                    {
                        XIL.RemoveLine();

                        string nextLine = "";
                        while(!nextLine.Contains(") //"))
                            nextLine = reader.ReadLine();
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

                        if (line.Length > 1 && line[0] == ".maxstack" && line[1] == "1")
                        {
                            XIL.RemoveLine();
                            XIL.AppendLine(".maxstack 2"); // increase stack enough for hit function
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
            foreach (string assembly in Assemblies)
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
            XIL.AppendLine("call       class [mscorlib]System.Threading.Thread [mscorlib]System.Threading.Thread::get_CurrentThread()");
            XIL.AppendLine("callvirt   instance int32 [mscorlib]System.Threading.Thread::get_ManagedThreadId()");
            XIL.AppendLine("ldc.i4     " + node.ID.ToString());
            XIL.AppendLine("call       void [XLibrary]XLibrary.XRay::Hit(int32, int32)");
        }

        internal string Compile()
        {
            byte[] buffer = UTF8Encoding.UTF8.GetBytes(XIL.ToString());

            using (FileStream outFile = new FileStream(ILPath, FileMode.Truncate))
                outFile.Write(buffer);

            string ilasm = Path.Combine(Environment.GetEnvironmentVariable("windir"), "//Microsoft.NET//Framework//v2.0.50727//ilasm.exe");

            // C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\ilasm.exe RiseOp

            // assemble file
            ProcessStartInfo info = new ProcessStartInfo(ilasm, Path.GetFileName(ILPath));
            info.WorkingDirectory = AsmDir;

            if (OriginalPath.EndsWith(".dll"))
                info.Arguments += " /DLL";

            Process.Start(info).WaitForExit();

            // copy XLibrary to final destination
            File.Copy(Path.Combine(Application.StartupPath, "XLibrary.dll"), Path.Combine(Path.GetDirectoryName(OriginalPath), "XLibrary.dll"), true);

            // copy compiled file
            string recompliedPath = Path.Combine(Path.GetDirectoryName(OriginalPath), "XRay." + Path.GetFileName(OriginalPath));

            File.Delete(recompliedPath); // delete prev compiled file

            File.Copy(DasmPath, recompliedPath);

            return recompliedPath;
        }
    }
}
