using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

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
        XRayedFile XFile;

        bool SideBySide; // if true change the file name and refs to xray.etc..

        XNodeOut ExtRoot;
        XNodeOut CurrentNode;

        public static Random RndGen = new Random();

        StringBuilder XIL = new StringBuilder(4096);

        public bool TrackFlow;
        public bool TrackInstances;
        public bool TrackExternal;
        public bool TrackAnon;
        public bool TrackFields;

        MethodReference XRayInitRef;
        MethodReference EnterMethodRef;
        MethodReference ExitMethodRef;
        MethodReference CatchMethodRef;
        MethodReference ClassConstructedRef;
        MethodReference ClassDeconstructedRef;
        MethodReference LoadFieldRef;
        MethodReference SetFieldRef;
        MethodReference ObjectFinalizeRef;
        MethodReference GetTypeFromHandleRef;
        TypeReference VoidRef;

        public long LinesAdded = 0;

        public XRayedFile[] XRayedFiles;


        public XDecompile(XNodeOut intRoot, XNodeOut extRoot, XRayedFile item, string outDir, XRayedFile[] files, bool trackFlow, bool trackExternal, bool trackAnon, bool trackFields, bool trackInstances)
        {
            ExtRoot = extRoot;
            OriginalPath = item.FilePath;
            OutputDir = outDir;
            SideBySide = Path.GetDirectoryName(item.FilePath) == outDir;
            item.RecompiledPath = null; // reset
            XFile = item;

            XRayedFiles = files;
            TrackFlow = trackFlow;
            TrackExternal = trackExternal;
            TrackAnon = trackAnon;
            TrackFields = trackFields;
            TrackInstances = trackInstances;
        }

        internal void MonoRecompile()
        {
            // copy target file to build dir so we can re-compile a running app
            string filename = Path.GetFileName(OriginalPath);
            string buildDir = Path.Combine(Application.StartupPath, "recompile", filename);
            Directory.CreateDirectory(buildDir);
            
            string assemblyPath = Path.Combine(buildDir, filename);
            File.Copy(OriginalPath, assemblyPath, true);

            AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(assemblyPath);

            // similar to using ilasm 4.0, adds // Metadata version: v4.0.30319 to disassembled version (though mscorlib ref stays original version)
            // lets us link xlibrary without error because it uses 4.0 runtime as well
            asm.MainModule.Runtime = TargetRuntime.Net_4_0; 

            var asmDef = asm.MainModule.Assembly.Name;

            var originalAsmName = asmDef.Name;

            asmDef.HashAlgorithm = AssemblyHashAlgorithm.None;
            
            XRayInitRef = asm.MainModule.Import(typeof(XLibrary.XRay).GetMethod("Init", new Type[] { typeof(bool), typeof(bool) }));
			EnterMethodRef = asm.MainModule.Import(typeof(XLibrary.XRay).GetMethod("MethodEnter", new Type[]{typeof(int)}));
            ExitMethodRef = asm.MainModule.Import(typeof(XLibrary.XRay).GetMethod("MethodExit", new Type[] { typeof(int) }));
            CatchMethodRef = asm.MainModule.Import(typeof(XLibrary.XRay).GetMethod("MethodCatch", new Type[] { typeof(int) }));
            ClassConstructedRef = asm.MainModule.Import(typeof(XLibrary.XRay).GetMethod("Constructed", new Type[] { typeof(int), typeof(Object) }));
            ClassDeconstructedRef = asm.MainModule.Import(typeof(XLibrary.XRay).GetMethod("Deconstructed", new Type[]{typeof(int), typeof(Object) }));
            LoadFieldRef = asm.MainModule.Import(typeof(XLibrary.XRay).GetMethod("LoadField", new Type[] { typeof(int) }));
            SetFieldRef = asm.MainModule.Import(typeof(XLibrary.XRay).GetMethod("SetField", new Type[] { typeof(int) }));
            VoidRef = asm.MainModule.Import(typeof(void));
            ObjectFinalizeRef = new MethodReference("Finalize", VoidRef, asm.MainModule.Import(typeof(Object)));
            ObjectFinalizeRef.HasThis = true;  // call on the current instance
            GetTypeFromHandleRef = asm.MainModule.Import(typeof(Type).GetMethod("GetTypeFromHandle"));

            // iterate class nodes
            foreach(var classDef in asm.MainModule.Types.Where(t => t.MetadataType == MetadataType.Class))
            {
                CurrentNode = XFile.FileNode;

                string[] namespaces = classDef.Namespace.Split('.');

                for (int i = 0; i < namespaces.Length; i++)
                    CurrentNode = CurrentNode.AddNode(namespaces[i], XObjType.Namespace);

                RecompileClass(classDef, CurrentNode);
            }

            // init xray at assembly entry point, do last so it comes before method enter
            if (asm.EntryPoint != null)
            {
                var processor = asm.EntryPoint.Body.GetILProcessor();

                AddInstruction(asm.EntryPoint, 0, processor.Create(OpCodes.Ldc_I4, TrackFlow ? 1 : 0));
                AddInstruction(asm.EntryPoint, 1, processor.Create(OpCodes.Ldc_I4, TrackInstances ? 1 : 0));
                AddInstruction(asm.EntryPoint, 2, processor.Create(OpCodes.Call, XRayInitRef));
            }

            ComputeFieldValues(XFile.FileNode);

            ProcessClassNames(XFile.FileNode);

            // change module names after class/method processing so we dont interfere with dependency lookup
            if (XRayedFiles.Any(f => f.AssemblyName == asmDef.Name))
            {
                asmDef.HasPublicKey = false;

                if (SideBySide)
                    asmDef.Name = "XRay." + asmDef.Name;
            }

            // string public key and re-name assemblies if needed
            foreach (var asmRef in asm.MainModule.AssemblyReferences)
                if (XRayedFiles.Any(f => f.AssemblyName == asmRef.Name))
                {
                    asmRef.HasPublicKey = false;

                    if (SideBySide)
                        asmRef.Name = "XRay." + asmRef.Name;
                }

            // compile
            string newName = Path.GetFileName(OriginalPath);
            if (SideBySide)
                newName = "XRay." + newName;

            XFile.RecompiledPath = Path.Combine(OutputDir, newName);

            File.Delete(XFile.RecompiledPath); // delete prev compiled file

            asm.Write(XFile.RecompiledPath);
        }

        private XNodeOut RecompileClass(TypeDefinition classDef, XNodeOut parentNode)
        {
            if ( !TrackAnon && classDef.Name.StartsWith("<>") )
                return null;

            var classNode = parentNode.AddNode(classDef.Name, XObjType.Class);

            RecompileMethods(classDef, classNode);

            foreach (var nestedDef in classDef.NestedTypes.Where(nt => nt.MetadataType == MetadataType.Class))
                RecompileClass(nestedDef, classNode);

            return classNode;
        }

        private void RecompileMethods(TypeDefinition classDef, XNodeOut classNode)
        {
            ILProcessor processor = null;

            // add fields
            if (TrackFields && classDef.HasFields)
                foreach (var fieldDef in classDef.Fields)
                    classNode.AddField(fieldDef);

            if (TrackInstances)
            {
                bool hasCtor = false;

                // add tracking to constructor
                foreach(var ctorMethod in classDef.Methods.Where(m =>  (m.Name == ".ctor" || m.Name == ".cctor") && m.Body != null))
                {
                    ctorMethod.Body.SimplifyMacros();

                    processor = ctorMethod.Body.GetILProcessor();

                    // to prevent warnings in verify, our code should be put after the base constructor call

                    AddInstruction(ctorMethod, 0, processor.Create(OpCodes.Ldc_I4, classNode.ID));

                    if (ctorMethod.Name == ".ctor")
                    {
                        hasCtor = true;
                        AddInstruction(ctorMethod, 1, processor.Create(OpCodes.Ldarg, 0));
                        AddInstruction(ctorMethod, 2, processor.Create(OpCodes.Call, ClassConstructedRef));
                    }
                    // else static constructor
                    else
                    {
                        // ldtoken    XTestLib.SmallStatic
                        // call       class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)

                        AddInstruction(ctorMethod, 1, processor.Create(OpCodes.Ldtoken, classDef));
                        AddInstruction(ctorMethod, 2, processor.Create(OpCodes.Call, GetTypeFromHandleRef));
                        AddInstruction(ctorMethod, 3, processor.Create(OpCodes.Call, ClassConstructedRef));
                    }
                    ctorMethod.Body.OptimizeMacros();
                }

                // add tracking to desconstructor (only add if ctor tracking added)
                if (hasCtor)
                {
                     var finMethod = classDef.Methods.FirstOrDefault(m => m.Name == "Finalize");
                     bool callObjectFinalize = false;

                     if (finMethod == null)
                     {
                         finMethod = new MethodDefinition("Finalize", Mono.Cecil.MethodAttributes.Family | Mono.Cecil.MethodAttributes.HideBySig | Mono.Cecil.MethodAttributes.Virtual, VoidRef);
                         callObjectFinalize = true;
                         classDef.Methods.Add(finMethod);
                     }

                     finMethod.Body.SimplifyMacros();

                     processor = finMethod.Body.GetILProcessor();

                     AddInstruction(finMethod, 0, processor.Create(OpCodes.Ldc_I4, classNode.ID));
                     AddInstruction(finMethod, 1, processor.Create(OpCodes.Ldarg, 0));
                     AddInstruction(finMethod, 2, processor.Create(OpCodes.Call, ClassDeconstructedRef));

                     if (callObjectFinalize)
                     {
                         AddInstruction(finMethod, 3, processor.Create(OpCodes.Ldarg, 0));
                         AddInstruction(finMethod, 4, processor.Create(OpCodes.Call, ObjectFinalizeRef));

                         AddInstruction(finMethod, 5, processor.Create(OpCodes.Ret));
                     }

                     finMethod.Body.OptimizeMacros();
                }
            }

            // iterate method nodes
            foreach (var method in classDef.Methods)
            {
                XNodeOut methodNode = classNode.AddNode(method.Name, XObjType.Method);

                if (method.Body == null)
                    continue;

                // expands branches/jumps to support adddresses > 255
                // possibly needed if injecting code into large functions
                // OptimizeMacros at end of the function re-optimizes
                method.Body.SimplifyMacros(); 

                methodNode.Lines = method.Body.Instructions.Count;

                processor = method.Body.GetILProcessor();

                for (int i = 0; i < method.Body.Instructions.Count; i++)
                {
                    var instruction = method.Body.Instructions[i];

                    // record method exited
                    if (TrackFlow && instruction.OpCode == OpCodes.Ret)
                    {
                        instruction.OpCode = OpCodes.Nop; // any 'goto return' will go to here so method exit gets logged first
                        AddInstruction(method, i + 1, processor.Create(OpCodes.Ldc_I4, methodNode.ID));
                        AddInstruction(method, i + 2, processor.Create(OpCodes.Call, ExitMethodRef));
                        AddInstruction(method, i + 3, processor.Create(OpCodes.Ret));

                        i += 3;
                    }

                    // if we're tracking calls to non-xrayed assemblies
                    else if (TrackExternal && 
                             (instruction.OpCode == OpCodes.Call ||
                              instruction.OpCode == OpCodes.Calli ||
                              instruction.OpCode == OpCodes.Callvirt) &&
                             !(method.Name == "Finalize" && method.DeclaringType.Namespace == "System") &&
                             (instruction.Operand as MethodReference).DeclaringType.Namespace != EnterMethodRef.DeclaringType.Namespace)
                    {
                        var call = instruction.Operand as MethodReference;

                        if (method.Name == ".cctor" && call.Name == "GetTypeFromHandle")
                            continue; // call added to cctor by xray

                        var scope = call.DeclaringType.Scope;

                        if (scope.MetadataScopeType == MetadataScopeType.ModuleReference)
                        {
                            // is this scope type internal or external, should it be tracked externally?
                            Debug.Assert(false);
                            continue;
                        }

                        // if internal method call or call to xrayed module
                        if (scope.MetadataScopeType != MetadataScopeType.AssemblyNameReference ||
                            XRayedFiles.Any(f => f.AssemblyName == scope.Name)) // when not in sxs mode, 
                            continue;

                        string moduleName = scope.Name;
                        string namespaces = (call.DeclaringType.DeclaringType != null) ? call.DeclaringType.DeclaringType.Namespace : call.DeclaringType.Namespace;
                        string className  = call.DeclaringType.Name;
                        string methodName = call.Name;

                        // add external file to root
                        XNodeOut node = ExtRoot.AddNode(moduleName, XObjType.File);

                        // traverse or add namespace to root
                        foreach(var name in namespaces.Split('.'))
                            node = node.AddNode(name, XObjType.Namespace);

                        node = node.AddNode(className, XObjType.Class);
                        node = node.AddNode(methodName, XObjType.Method);

                        node.Lines = 1;

                        // in function is prefixed by .constrained, wrap enter/exit around those 2 lines
                        int offset = 0;
                        if (i > 0 && method.Body.Instructions[i - 1].OpCode == OpCodes.Constrained)
                        {
                            i -= 1;
                            offset = 1;
                        }

                        var oldPos = method.Body.Instructions[i];

                        // wrap the call with enter and exit, because enter to an external method may cause
                        // an xrayed method to be called, we want to track the flow of that process
                        int pos = i;
                        AddInstruction(method, pos++, processor.Create(OpCodes.Ldc_I4, node.ID));
                        AddInstruction(method, pos++, processor.Create(OpCodes.Call, EnterMethodRef));
                        
                        // method
                        pos += 1 + offset;

                        AddInstruction(method, pos++, processor.Create(OpCodes.Ldc_I4, node.ID));
                        AddInstruction(method, pos, processor.Create(OpCodes.Call, ExitMethodRef));

                        var newPos = method.Body.Instructions[i];
                        UpdateExceptionHandlerPositions(method, oldPos, newPos);

                        i = pos; // loop end will add 1 putting us right after last added function
                    }

                    
                    else if (TrackFields && 
                             (instruction.OpCode == OpCodes.Stfld || instruction.OpCode == OpCodes.Ldfld || instruction.OpCode == OpCodes.Ldflda))
                    {
                        var fieldDef = instruction.Operand as FieldReference;
                        
                        var scope = fieldDef.DeclaringType.Scope;

                        if (scope.MetadataScopeType == MetadataScopeType.ModuleReference)
                        {
                            // is this scope type internal or external, should it be tracked externally?
                            Debug.Assert(false);
                            continue;
                        }

                        string namespaces = fieldDef.DeclaringType.Namespace;
                        string className = fieldDef.DeclaringType.Name;
                        string fieldName = fieldDef.Name;
                        string fieldType = fieldDef.FieldType.FullName;

                        XNodeOut node = null;

                        // if xrayed internal
                        if (scope.MetadataScopeType == MetadataScopeType.ModuleDefinition)
                            node = XFile.FileNode;

                        // xrayed, but in diff module
                        else if (XRayedFiles.Any(f => f.AssemblyName == scope.Name))
                            node = XRayedFiles.First(f => f.AssemblyName == scope.Name).FileNode;

                        // if not xrayed - map to external root
                        else
                            node = ExtRoot.AddNode(scope.Name, XObjType.File);

                        // traverse or add namespace to root
                        foreach (var name in namespaces.Split('.'))
                            node = node.AddNode(name, XObjType.Namespace);

                        node = node.AddNode(className, XObjType.Class);

                        node = node.AddField(fieldDef);

                        // some times volitile prefixes set/get field
                        int offset = 0;
                        if (i > 0 && method.Body.Instructions[i - 1].OpCode == OpCodes.Volatile)
                        {
                            i--;
                            offset = 1;
                        }

                        AddInstruction(method, i, processor.Create(OpCodes.Ldc_I4, node.ID));
                        
                        if(instruction.OpCode == OpCodes.Stfld)
                            AddInstruction(method, i + 1, processor.Create(OpCodes.Call, SetFieldRef));
                        else
                            AddInstruction(method, i + 1, processor.Create(OpCodes.Call, LoadFieldRef));
                        
                        i = i + 2 + offset; // skip instuction added, and field itself, end of loop will iterate this again

                        // Debug.WriteLine("{0} in Module: {1}, Namespace: {2}, Class: {3}, Name: {4}, Type: {5}", instruction.OpCode, fieldDef.DeclaringType.Scope.Name, namespaces, className, fieldName, fieldType);
                    }

                } // end iterating through instructions


                // record function was entered
                AddInstruction(method, 0, processor.Create(OpCodes.Ldc_I4, methodNode.ID));
                AddInstruction(method, 1, processor.Create(OpCodes.Call, EnterMethodRef));

                // record catches
                if (TrackFlow)
                    foreach (var handler in method.Body.ExceptionHandlers)
                    {
                        if (handler.HandlerType != ExceptionHandlerType.Catch)
                            continue;

                        int i = method.Body.Instructions.IndexOf(handler.HandlerStart);

                        AddInstruction(method, i, processor.Create(OpCodes.Ldc_I4, methodNode.ID));
                        AddInstruction(method, i + 1, processor.Create(OpCodes.Call, CatchMethodRef));

                        var oldPos = handler.HandlerStart;
                        var newPos = method.Body.Instructions[i];

                        UpdateExceptionHandlerPositions(method, oldPos, newPos);
                    }

                method.Body.OptimizeMacros();
            }
        }

        internal void AddInstruction(MethodDefinition method, int pos, Instruction instruction)
        {
            method.Body.Instructions.Insert(pos, instruction);

            LinesAdded++;
        }

        internal void UpdateExceptionHandlerPositions(MethodDefinition method, Instruction oldPos, Instruction newPos)
        {
            // replace prev instructions pointing to old address to new address
            foreach (var check in method.Body.ExceptionHandlers)
            {
                if (check.TryStart == oldPos) check.TryStart = newPos; // empty try
                if (check.TryEnd == oldPos) check.TryEnd = newPos;
                if (check.HandlerStart == oldPos) check.HandlerStart = newPos;
                if (check.HandlerEnd == oldPos) check.HandlerEnd = newPos;
            }
        }

        internal double ComputeFieldValues(XNodeOut node)
        {
            // give fields a value that makes them take up a total of %15 of the value of the class

            double total = node.Lines;
            double fieldCount = 0;

            // compute sum of all dependents
            foreach (XNodeOut subnode in node.Nodes)
                if(subnode.ObjType == XObjType.Field)
                    fieldCount++;
                else
                    total += ComputeFieldValues(subnode);

            if(fieldCount > 0)
            {
                // inflate total 15% and fit fields in there
                double subTotal = total;
                total = total * 100.0 / 85.0;

                double fieldTotal = total - subTotal;

                int fieldValue = (int) (fieldTotal / fieldCount);
                if (fieldValue < 1)
                    fieldValue = 1;

                foreach (XNodeOut field in node.Nodes.Where(n => n.ObjType == XObjType.Field))
                    field.Lines = fieldValue;
            }

            return total;
        }

        internal void ProcessClassNames(XNodeOut node)
        {
            // if class move anonymous objects into their non-anon counterparts
            if(node.ObjType == XObjType.Class)
            {
                // put anonymous classes in the functions that they were generated from
                // ex -. Nested Class: <>c__DisplayClass15, has method: <FilterTextBox_TextChanged>b__11
                var anonClasses = node.Nodes.Where(n => n.ObjType == XObjType.Class && n.Name.StartsWith("<>c")).ToArray();

                foreach(XNodeOut anonClass in anonClasses)
                {
                    // mark as anon
                    MarkNodeAsAnon(anonClass);

                    // if parent is not anon
                    if( !(anonClass.Parent as XNodeOut).IsAnon )
                    {
                        // iterate anon methods to find association
                        var anonMethod = anonClass.Nodes.FirstOrDefault(n => !n.Name.StartsWith("<>") && n.Name.StartsWith("<") && n.Name.Contains(">"));
                        if (anonMethod == null)
                            continue;

                        string anonMethodName = anonMethod.Name.Substring(1, anonMethod.Name.IndexOf(">") - 1);

                        // iterate parent class methods to find match
                        var parentMethod = node.Nodes.FirstOrDefault(n => n.Name == anonMethodName) as XNodeOut;
                        if (parentMethod == null)
                            continue;

                        // move node under that method
                        node.Nodes.Remove(anonClass);
                        parentMethod.Nodes.Add(anonClass);
                        anonClass.Parent = parentMethod;

                        // will be renamed when class is iterated below
                    }
                }

                // iterate anon methods, move into parent methods
                var anonMethods = node.Nodes.Where(n => n.ObjType == XObjType.Method && n.Name.StartsWith("<")).ToArray();

                foreach(XNodeOut anonMethod in anonMethods)
                {
                    MarkNodeAsAnon(anonMethod);

                    string parentName = anonMethod.Name.Substring(1, anonMethod.Name.IndexOf(">") - 1);

                    var parentMethod = node.Nodes.FirstOrDefault(n => n.Name == parentName) as XNodeOut;
                    if (parentMethod == null)
                        continue;

                    // move node under that method
                    node.Nodes.Remove(anonMethod);
                    parentMethod.Nodes.Add(anonMethod);
                    anonMethod.Parent = parentMethod;                    
                }

                // if current class is anon, rename class parentNodeName.classX
                if (node.IsAnon)
                {
                    XNodeOut parent = node.GetNotAnonParent();
                    node.Name = parent.Name + ".class" + parent.AnonClasses.ToString();
                    parent.AnonClasses++;
                }
            }

            // if method
            if(node.ObjType == XObjType.Method)
            {
                XNodeOut parent = node.GetNotAnonParent();

                // if method is ctor/cctor rename
                if (node.Name == ".ctor" || node.Name == ".cctor")
                {
                    node.Name = parent.Name + ((node.Name == ".ctor") ? ".init" : ".static_init");
                    if (parent.InitCount > 1)
                        node.Name += parent.InitCount.ToString();

                    parent.InitCount++;
                }

                else if (node.IsAnon || // simple anon method
                         (node.Parent as XNodeOut).IsAnon) // method of an anon class
                {
                    if (node.Name.StartsWith("<"))
                    {
                        node.Name = parent.Name + ".func" + parent.AnonFuncs.ToString();
                        parent.AnonFuncs++;
                    }
                }
            }

            // iterate sub elements
            foreach (XNodeOut subnode in node.Nodes)
                ProcessClassNames(subnode);
        }

        void MarkNodeAsAnon(XNodeOut node)
        {
            node.IsAnon = true;

            foreach (XNodeOut subnode in node.Nodes)
                MarkNodeAsAnon(subnode);
        }

        // ----- MS RECOMPILER ---------------------------------------------------------------------------------------------------

        internal void MsDecompile()
        {
            string name = Path.GetFileName(OriginalPath);
           
            // create directories
            AsmDir = Path.Combine(Application.StartupPath, name);

            Decompile(OriginalPath, AsmDir);

            ILPathOriginal = Path.Combine(AsmDir,  Path.GetFileNameWithoutExtension(DasmPath) + "_original.il");
          
            // save original
            File.Copy(ILPath, ILPathOriginal, true);

            File.Delete(DasmPath); // so it can be replaced with asm exe
        }

        /// <summary>
        /// decompiles file at OriginalPath to AsmDir
        /// </summary>
        internal void Decompile(string exePath, string destinationDir)
        {
            string name = Path.GetFileName(exePath);

            Directory.CreateDirectory(destinationDir);

            // clear previous files
            foreach (string file in Directory.GetFiles(destinationDir))
                File.Delete(file);

            // remove spaces from name and move to dasm dir
            DasmPath = Path.Combine(destinationDir, Path.GetFileName(exePath).Replace(' ', '_'));
            File.Copy(exePath, DasmPath);

            // disassemble file
            string ildasm = Path.Combine(Application.StartupPath, "ildasm.exe");
            string ILName = Path.GetFileNameWithoutExtension(DasmPath) + ".il";
            ProcessStartInfo info = new ProcessStartInfo(ildasm, Path.GetFileName(DasmPath) + " /utf8 /output=" + ILName);
            info.WorkingDirectory = destinationDir;
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;

            Process process = Process.Start(info);
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            ILPath = Path.Combine(destinationDir, ILName);

            // check output il for errors - no way to get ildasm to just output errrors
            using (StreamReader read = new StreamReader(File.OpenRead(ILPath)))
                while (!read.EndOfStream)
                {
                    string line = read.ReadLine();
                    if (line.Contains("has no valid CLR header and cannot be disassembled"))
                        throw new CompileError("Decompiling", ILPath, "Has no valid CLR header and cannot be disassembled");
                }
        }

        internal void ScanLines(bool test)
        {
            XIL.Length = 0;
            CurrentNode = XFile.FileNode;
  
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
                        if (XRayedFiles.Any(f => f.AssemblyName == assembly))
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
                            CurrentNode = XFile.FileNode;

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
                            if (XRayedFiles.Any(f => f.AssemblyName == external))
                                continue;

                            // add external file to root
                            XNodeOut node = ExtRoot.Nodes.FirstOrDefault(n => n.Name == external) as XNodeOut;

                            if (node == null)
                                node = ExtRoot.AddNode(external, XObjType.File);

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
                foreach (string assembly in XRayedFiles.Select(f => f.AssemblyName))
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

            LinesAdded += 5;
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

            LinesAdded += 4;
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
            LinesAdded += 2;

            if (TrackInstances)
                if (node.Name == ".ctor")
                {
                    AddLine("ldc.i4  " + node.Parent.ID.ToString());
                    AddLine("call    void [XLibrary]XLibrary.XRay::Constructed(int32)");
                    LinesAdded += 2;
                }
                else if (node.Name == "Finalize")
                {
                    AddLine("ldc.i4  " + node.Parent.ID.ToString());
                    AddLine("call    void [XLibrary]XLibrary.XRay::Deconstructed(int32)");
                    LinesAdded += 2;
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
            LinesAdded += 2;
        }

        private void InjectMethodCatch(XNodeOut node)
        {
            Debug.Assert(!node.Exclude);
            Debug.Assert(TrackFlow);

            XIL.AppendLine();
            AddLine("ldc.i4  " + node.ID.ToString() + " // XRay - Method Catch");
            AddLine("call    void [XLibrary]XLibrary.XRay::MethodCatch(int32)");
            XIL.AppendLine();

            LinesAdded += 2;
        }

        internal string Compile()
        {
            // write new IL
            byte[] buffer = UTF8Encoding.UTF8.GetBytes(XIL.ToString());

            using (FileStream outFile = new FileStream(ILPath, FileMode.Truncate))
                outFile.Write(buffer);

            string windir = Environment.GetEnvironmentVariable("windir");
            string ilasm = Path.Combine(windir, "Microsoft.NET\\Framework\\v4.0.30319\\ilasm.exe");
            //string ilasm = Path.Combine(windir, "Microsoft.NET\\Framework\\v2.0.50727\\ilasm.exe");
            
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
