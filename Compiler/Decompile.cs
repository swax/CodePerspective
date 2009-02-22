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
        
        XNodeOut RootNode;
        XNodeOut CurrentNode;

        public static Random RndGen = new Random();

        StringBuilder XIL = new StringBuilder(4096);

        public XDecompile(string path)
        {
            OriginalPath = path;
        }

        internal void Decompile()
        {
            string ildasm = Path.Combine(Application.StartupPath, "ildasm.exe");

            string name = Path.GetFileNameWithoutExtension(OriginalPath);

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
            RootNode = new XNodeOut(null, "root", XObjType.Root);
            CurrentNode = RootNode;

            InjectLibrary("XLibrary", "3:5:0:0");

            using (StreamReader reader = new StreamReader(ILPath))
            {
                while (!reader.EndOfStream)
                {
                    string[] line = reader.SplitNextLine(XIL);

                    if (line.Length == 0)
                        continue;

                    else if (line[0] == ".class")
                    {
                        // read the whole class before the {
                        while (!line.Contains("{"))
                            line = line.Concat(reader.SplitNextLine(XIL)).ToArray();

                        // right before the class is extended that is the name
                        string name = line.TakeWhile(s => s != "extends").LastOrDefault();

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
                        string name = line.Where(s => s.Contains('(')).FirstOrDefault();

                        name = name.Substring(0, name.IndexOf('('));

                        CurrentNode = CurrentNode.AddNode(name, XObjType.Method);

                        if(!name.StartsWith("'")) // don't track generated methods
                            InjectMethodHit(CurrentNode);
                    }

                    else if (line[0] == ".property") // ignore for now
                    {
                        while (!line.Contains("}"))
                            line = reader.SplitNextLine(XIL);
                    }

                    else if (line[0] == ".field")
                    {
                        string name = line.LastOrDefault();

                        XNodeOut fieldNode = CurrentNode.AddNode(name, XObjType.Field);
                        fieldNode.Lines = 1;
                    }

                    else if (CurrentNode.ObjType == XObjType.Method)
                    {
                        if (line[0].StartsWith(".entrypoint"))
                        {
                            InjectGui();
                            //string[] line = reader.SplitNextLine(XIL);
                            //while (!line.Contains("{"))
                             //   line = line.Concat(reader.SplitNextLine(XIL)).ToArray();
                        }

                        else if (line[0] == "{") // try, catch, finallys, inside one another
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

            // sum up areas
            RootNode.ComputeSums();
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

        internal void SaveTree()
        {
            string path = Path.GetDirectoryName(OriginalPath);
            path = Path.Combine(path, "XRay.dat");

            byte[] temp = new byte[1024];

            using(FileStream stream = new FileStream(path, FileMode.Create))
                RootNode.Write(stream, temp);
        }

        internal string Compile()
        {
            byte[] buffer = UTF8Encoding.UTF8.GetBytes(XIL.ToString());

            using (FileStream outFile = new FileStream(ILPath, FileMode.Truncate))
                outFile.Write(buffer);

            string ilasm = Path.Combine(Environment.GetEnvironmentVariable("windir"), "//Microsoft.NET//Framework//v2.0.50727//ilasm.exe");

            // C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\ilasm.exe RiseOp

            // assemble file
            ProcessStartInfo info = new ProcessStartInfo(ilasm, Path.GetFileNameWithoutExtension(ILPath));
            info.WorkingDirectory = AsmDir;
            Process.Start(info).WaitForExit();

            // copy XLibrary to final destination
            File.Copy(Path.Combine(Application.StartupPath, "XLibrary.dll"), Path.Combine(Path.GetDirectoryName(OriginalPath), "XLibrary.dll"), true);


            // copy compiled file
            string recompliedPath = OriginalPath.Insert(OriginalPath.LastIndexOf('.'), ".XRay");

            File.Delete(recompliedPath); // delete prev compiled file

            File.Copy(DasmPath, recompliedPath);

            return recompliedPath;
        }
    }
}
