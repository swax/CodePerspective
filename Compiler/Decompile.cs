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
    partial class XDecompile
    {
        string OriginalPath;
        public string BackupPath;
        string DasmPath;
        string ILPath;
        string ILPathOriginal;
        string AsmDir;
        string OutputDir;
        string DatPath;
        XRayedFile XFile;

        bool SideBySide; // if true change the file name and refs to xray.etc..

        XNodeOut ExtRoot;
        XNodeOut CurrentNode;

        public static Random RndGen = new Random();

        StringBuilder XIL = new StringBuilder(4096);

        public bool TrackFlow = true;
        public bool TrackInstances = true;
        public bool TrackExternal = true;
        public bool TrackAnon = true;
        public bool TrackFields = true;
        public bool TrackCode = true;
        public bool ShowUIonStart = true;
        public bool SaveMsil = true;
        public bool DecompileCSharp = false;

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


        public XDecompile(XNodeOut intRoot, XNodeOut extRoot, XRayedFile item, string outDir, string datPath, XRayedFile[] files, bool sxs)
        {
            ExtRoot = extRoot;
            OriginalPath = item.FilePath;
            OutputDir = outDir;
            DatPath = datPath;
            SideBySide = sxs;
            item.RecompiledPath = null; // reset
            XFile = item;

            XRayedFiles = files;
        }

        internal void MonoRecompile()
        {
            // copy target file to build dir so we can re-compile a running app
            string filename = Path.GetFileName(OriginalPath);
            string buildDir = Path.Combine(Application.StartupPath, "recompile", filename);
            Directory.CreateDirectory(buildDir);
            
            string assemblyPath = Path.Combine(buildDir, filename);
            File.Copy(OriginalPath, assemblyPath, true);

            var asm = AssemblyDefinition.ReadAssembly(assemblyPath);

            // don't remove strong name because in vs plugin and xaml files, assemblies are referenced by their public key (nuGet, ILSpy)
            // no point either, what really needs to be done is to remove any signatures - strong names aren't signatures
            //asm.Name.PublicKey = new byte[0];
            //asm.Name.PublicKeyToken = new byte[0];
            //asm.Name.HasPublicKey = false; 

            // if we're decompiling a moved dll, then we need to point the resolver to its original path to resolve dependencies
            var resolver = asm.MainModule.AssemblyResolver as DefaultAssemblyResolver;
            if(resolver != null)
                resolver.AddSearchDirectory(Path.GetDirectoryName(OriginalPath));

            // similar to using ilasm 4.0, adds // Metadata version: v4.0.30319 to disassembled version (though mscorlib ref stays original version)
            // lets us link xlibrary without error because it uses 4.0 runtime as well
            // asm.MainModule.Runtime = TargetRuntime.Net_4_0; 


            var asmDef = asm.MainModule.Assembly.Name;

            var originalAsmName = asmDef.Name;

            asmDef.HashAlgorithm = AssemblyHashAlgorithm.None;

            /*var corlib = asm.MainModule.AssemblyReferences.FirstOrDefault(r => r.Name == "mscorlib");
            corlib.Version = new Version("4.0.0.0");

            var syslib = asm.MainModule.AssemblyReferences.FirstOrDefault(r => r.Name == "System");
            syslib.Version = new Version("4.0.0.0");*/

            XRayInitRef = asm.MainModule.Import(typeof(XLibrary.XRay).GetMethod("Init", new Type[] { typeof(string), typeof(bool), typeof(bool), typeof(bool) }));
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
            //(ObjectFinalizeRef.DeclaringType.Scope as AssemblyNameReference).Version = new Version("2.0.0.0");
            GetTypeFromHandleRef = asm.MainModule.Import(typeof(Type).GetMethod("GetTypeFromHandle"));

            // iterate class nodes
            foreach(var classDef in asm.MainModule.Types.Where(t => t.MetadataType == MetadataType.Class))
                RecompileClass(classDef, XFile.FileNode);

            // init xray at assembly entry point, do last so it comes before method enter
            if (asm.EntryPoint != null)
                AddInit(asm.EntryPoint, false);
 
            ComputeFieldValues(XFile.FileNode);

            ProcessClassNames(XFile.FileNode);

            // change module names after class/method processing so we dont interfere with dependency lookup
            if (XRayedFiles.Any(f => f.AssemblyName == asmDef.Name))
            {
                asmDef.HasPublicKey = false; //necessary? maybe if referencing strong signed.. are hashes put into references?

                if (SideBySide)
                    asmDef.Name = "XRay." + asmDef.Name;
            }

            // string public key and re-name assemblies if needed
            foreach (var asmRef in asm.MainModule.AssemblyReferences)
                if (XRayedFiles.Any(f => f.AssemblyName == asmRef.Name))
                {
                    asmRef.HasPublicKey = false; //necessary? maybe if referencing strong signed.. are hashes put into references?

                    if (SideBySide)
                        asmRef.Name = "XRay." + asmRef.Name;
                }


            // inject dll init - after processed so xray doesnt get injected into this new method
            TypeDefinition moduleClass = asm.MainModule.Types.FirstOrDefault(t => t.Name == "<Module>");
            if (moduleClass != null)
            {
                var cctor = moduleClass.Methods.FirstOrDefault(m => m.Name == ".cctor");
                var created = false;

                if (cctor == null)
                {
                    var attributes = Mono.Cecil.MethodAttributes.Static
                                    | Mono.Cecil.MethodAttributes.SpecialName
                                    | Mono.Cecil.MethodAttributes.RTSpecialName;
                    cctor = new MethodDefinition(".cctor", attributes, VoidRef);

                    moduleClass.Methods.Add(cctor);
                    created = true;
                }

                AddInit(cctor, created);
            }

            // compile
            string newName = Path.GetFileName(OriginalPath);
            if (SideBySide)
                newName = "XRay." + newName;
            else
            {
                // copy original file into backup folder
                // dont clear back up dir, just overwrite, in case multiple xrays of diff files
                var backupDir = Path.Combine(OutputDir, "xBackup");
                Directory.CreateDirectory(backupDir);

                BackupPath = Path.Combine(backupDir, newName);
                File.Copy(OriginalPath, BackupPath, true);      
            }
            
            XFile.RecompiledPath = Path.Combine(OutputDir, newName);

            File.Delete(XFile.RecompiledPath); // delete prev compiled file

            asm.Write(XFile.RecompiledPath);
        }

        private void AddInit(MethodDefinition cctor, bool created)
        {
            var processor = cctor.Body.GetILProcessor();

            int i = 0;

            AddInstruction(cctor, i++, processor.Create(OpCodes.Ldstr, DatPath));
            AddInstruction(cctor, i++, processor.Create(OpCodes.Ldc_I4, ShowUIonStart ? 1 : 0));
            AddInstruction(cctor, i++, processor.Create(OpCodes.Ldc_I4, TrackFlow ? 1 : 0));
            AddInstruction(cctor, i++, processor.Create(OpCodes.Ldc_I4, TrackInstances ? 1 : 0));
            AddInstruction(cctor, i++, processor.Create(OpCodes.Call, XRayInitRef));

            if(created)
                AddInstruction(cctor, i++, processor.Create(OpCodes.Ret));

            cctor.Body.OptimizeMacros();
        }

        public static XNodeOut SignatureToClass(string signature, XNodeOut fileNode)
        {
            // create syntax tree for signature
            XDef def = XDef.ParseAndCheck(signature);

            // iterate syntax tree and add to our node map
            XNodeOut currentNode = fileNode;

            while (def != null)
            {
                if (def.DefType == XDef.XDefType.Namespace)
                    currentNode = currentNode.AddNode(def.Name, XObjType.Namespace);

                else if (def.DefType == XDef.XDefType.Class)
                {
                    currentNode = currentNode.AddNode(def.GetShortName(), XObjType.Class);

                    /* Cant map generic params because the fileNode is not right
                    if (def.Generics != null)
                        foreach (var genericSig in def.Generics)
                            SignatureToClass(genericSig.GetFullName(), fileNode);*/
                }

                def = def.SubDef;
            }

            Debug.Assert(currentNode.ObjType == XObjType.Class);

            return currentNode;
        }

        private XNodeOut RecompileClass(TypeDefinition classDef, XNodeOut fileNode)
        {
            if ( !TrackAnon && classDef.Name.StartsWith("<>") )
                return null;

            var classNode = SignatureToClass(classDef.ToString(), fileNode);

            if(classDef.BaseType != null)
                SetClassDependency(classNode, classDef.BaseType);

            RecompileMethods(classDef, classNode);

            foreach (var nestedDef in classDef.NestedTypes)
                if (nestedDef.MetadataType == MetadataType.Class || nestedDef.MetadataType == MetadataType.ValueType)
                    RecompileClass(nestedDef, fileNode);
                else
                    Debug.WriteLine("Ignored nested type - " + nestedDef.ToString());

            return classNode;
        }

        private void RecompileMethods(TypeDefinition classDef, XNodeOut classNode)
        {
            ILProcessor processor = null;

            // add fields
            if (TrackFields && classDef.HasFields)
                foreach (var fieldDef in classDef.Fields)
                {
                    var fieldNode = classNode.AddField(fieldDef);

                    SetClassDependency(classNode, fieldDef.DeclaringType);

                    if (fieldDef.FieldType.IsGenericParameter)
                        Debug.WriteLine("Generic parameter ignored - " + fieldDef.FieldType.ToString());
                    else
                        fieldNode.ReturnID = GetClassRef(fieldDef.FieldType).ID;
                }

            if(TrackCode)
                foreach (var method in classDef.Methods)
                {
                    XNodeOut methodNode = classNode.AddMethod(method);
                    
                    if(DecompileCSharp)
                        methodNode.CSharp = DecompileMethod(method);

                    if (method.Body == null)
                        continue;

                    // record MSIL
                    if (SaveMsil)
                    {
                        methodNode.Msil = new List<XInstruction>();
                        foreach (var inst in method.Body.Instructions)
                        {
                            var xInst = new XInstruction();
                            xInst.Offset = inst.Offset;
                            xInst.OpCode = inst.OpCode.Name;
                            xInst.Line = (inst.Operand != null) ? inst.Operand.ToString() : "";

                            if (inst.OpCode == OpCodes.Call ||
                                inst.OpCode == OpCodes.Calli ||
                                inst.OpCode == OpCodes.Callvirt ||
                                inst.OpCode == OpCodes.Newobj ||
                                inst.OpCode == OpCodes.Ldftn) // pushes a function pointer to the stack
                            {
                                var call = inst.Operand as MethodReference;
                                if (call != null)
                                {
                                    var classRef = GetClassRef(call.DeclaringType);
                                    var methodRef = classRef.AddMethod(call);

                                    xInst.RefId = methodRef.ID;
                                }
                                else
                                    Debug.WriteLine("Unable to track: " + inst.Operand.ToString());
                            }
                            else if (inst.OpCode == OpCodes.Stfld ||
                                      inst.OpCode == OpCodes.Stsfld ||
                                      inst.OpCode == OpCodes.Ldfld ||
                                      inst.OpCode == OpCodes.Ldflda ||
                                      inst.OpCode == OpCodes.Ldsfld ||
                                      inst.OpCode == OpCodes.Ldsflda)
                            {
                                var fieldDef = inst.Operand as FieldReference;
                                var classRef = GetClassRef(fieldDef.DeclaringType);
                                var fieldRef = classRef.AddField(fieldDef);
                                xInst.RefId = fieldRef.ID;
                            }
                            else if (inst.OpCode.FlowControl == FlowControl.Branch ||
                                     inst.OpCode.FlowControl == FlowControl.Cond_Branch)
                            {
                                var op = inst.Operand as Instruction;
                                if (op != null)
                                {
                                    int offset = op.Offset;
                                    xInst.Line = "goto " + offset.ToString("X");
                                    xInst.RefId = offset;
                                }
                            }

                            methodNode.Msil.Add(xInst);
                        }
                    }
                }

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
                        // ldtoken    XTestLib.StaticTemplateClass`1<!T> (for generic static classes)
                        // call       class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)

                        if (classDef.HasGenericParameters)
                        {
                            // for some reason the GenericInstanceType does not carry over the parameters
                            var genericDef = new GenericInstanceType(classDef);
                            foreach (var p in classDef.GenericParameters)
                                genericDef.GenericArguments.Add(p);
                            
                            AddInstruction(ctorMethod, 1, processor.Create(OpCodes.Ldtoken, genericDef));
                        }
                        else
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
                XNodeOut methodNode = classNode.AddMethod(method);

                // return
                if(method.ReturnType != null)
                {
                    var returnNode = SetClassDependency(classNode, method.ReturnType);
                    if(returnNode != null)
                        methodNode.ReturnID = returnNode.ID;
                }
                // params
                for(int i = 0; i < method.Parameters.Count; i++)
                {
                    var p = method.Parameters[i];

                    if (methodNode.ParamIDs == null)
                    {
                        methodNode.ParamIDs = new int[method.Parameters.Count];
                        methodNode.ParamNames = new string[method.Parameters.Count];
                    }

                    var paramNode = SetClassDependency(classNode, p.ParameterType);
                    methodNode.ParamIDs[i] = paramNode.ID;
                    methodNode.ParamNames[i] = p.Name;
                }

                if (method.Body == null)
                    continue;

                // local vars
                foreach (var local in method.Body.Variables)
                    SetClassDependency(classNode, local.VariableType);

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
                    else if ( instruction.OpCode == OpCodes.Call ||
                              instruction.OpCode == OpCodes.Calli ||
                              instruction.OpCode == OpCodes.Callvirt)
                    {
                        var call = instruction.Operand as MethodReference;

                        if (call == null)
                        {
                            Debug.WriteLine("Unable to track not xrayed: " + instruction.Operand.ToString());
                            continue;
                        }

                        SetClassDependency(classNode, call.ReturnType);
                        SetClassDependency(classNode, call.DeclaringType);
                        foreach(var p in call.Parameters)
                            SetClassDependency(classNode, p.ParameterType);

                        var calledRef = GetClassRef(call.DeclaringType);

                        var calledNode = calledRef.AddMethod(call);


                        /*if( TrackExternal && 
                            !(method.Name == "Finalize" && method.DeclaringType.Namespace == "System") &&
                            (instruction.Operand as MethodReference).DeclaringType.Namespace != EnterMethodRef.DeclaringType.Namespace )*/
                        if (TrackExternal && 
                            calledNode.External &&
                            calledRef.Name != "XRay")
                           // !(method.Name == "Finalize" && method.DeclaringType.Namespace == "System"))
                        {
                            if (method.Name == ".cctor" && call.Name == "GetTypeFromHandle")
                                continue; // call added to cctor by xray

                            calledNode.Lines = 1;

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
                            AddInstruction(method, pos++, processor.Create(OpCodes.Ldc_I4, calledNode.ID));
                            AddInstruction(method, pos++, processor.Create(OpCodes.Call, EnterMethodRef));
                        
                            // method
                            pos += 1 + offset;

                            AddInstruction(method, pos++, processor.Create(OpCodes.Ldc_I4, calledNode.ID));
                            AddInstruction(method, pos, processor.Create(OpCodes.Call, ExitMethodRef));

                            var newPos = method.Body.Instructions[i];
                            UpdateExceptionHandlerPositions(method, oldPos, newPos);

                            i = pos; // loop end will add 1 putting us right after last added function
                        }
                    }

                    else if (TrackFields && 
                             (instruction.OpCode == OpCodes.Stfld || 
                              instruction.OpCode == OpCodes.Stsfld ||
                              instruction.OpCode == OpCodes.Ldfld ||
                              instruction.OpCode == OpCodes.Ldflda ||
                              instruction.OpCode == OpCodes.Ldsfld ||
                              instruction.OpCode == OpCodes.Ldsflda))
                    {
                        var fieldDef = instruction.Operand as FieldReference;

                        var classRef = GetClassRef(fieldDef.DeclaringType);
                        var fieldRef = classRef.AddField(fieldDef);

                        // some times volitile prefixes set/get field
                        int offset = 0;
                        if (i > 0 && method.Body.Instructions[i - 1].OpCode == OpCodes.Volatile)
                        {
                            i--;
                            offset = 1;
                        }

                        AddInstruction(method, i, processor.Create(OpCodes.Ldc_I4, fieldRef.ID));

                        if (instruction.OpCode == OpCodes.Stfld || instruction.OpCode == OpCodes.Stsfld)
                            AddInstruction(method, i + 1, processor.Create(OpCodes.Call, SetFieldRef));
                        else
                            AddInstruction(method, i + 1, processor.Create(OpCodes.Call, LoadFieldRef));
                        
                        i = i + 2 + offset; // skip instuction added, and field itself, end of loop will iterate this again

                        // Debug.WriteLine("{0} in Module: {1}, Namespace: {2}, Class: {3}, Name: {4}, Type: {5}", instruction.OpCode, fieldDef.DeclaringType.Scope.Name, namespaces, className, fieldName, fieldType);
                    }

                    else if (instruction.OpCode == OpCodes.Newobj)
                    {
                        var newObj = instruction.Operand as MethodReference;
                        SetClassDependency(classNode, newObj.DeclaringType);
                    }

                    /* Still not really working - goal - to get side by side wpf apps to work
                     * else if (instruction.OpCode == OpCodes.Ldstr && SideBySide)
                    {
                        // rename Pack URIs in WPF so resources can be found with an XRay.. namespace
                        // ex:  "/MyApp;component/views/aboutview.xaml" ->  "/XRay.MyApp;component/views/aboutview.xaml"
                        var packUri = instruction.Operand as String;

                        foreach (var file in XRayedFiles)
                            packUri = packUri.Replace("/" + file.AssemblyName + ";", "/XRay." + file.AssemblyName + ";");
                            //packUri = packUri.Replace(file.AssemblyName, "XRay." + file.AssemblyName);

                        instruction.Operand = packUri;
                    }*/

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

        XNodeOut SetClassDependency(XNodeOut dependentClass, TypeReference declaringType)
        {
            if (declaringType == null)
                return null;

            var target = GetClassRef(declaringType);
            target = target.GetParentClass(true) as XNodeOut;

            dependentClass = dependentClass.GetParentClass(true) as XNodeOut; 

            if (dependentClass.ClassDependencies == null)
                dependentClass.ClassDependencies = new HashSet<int>();

            dependentClass.ClassDependencies.Add(target.ID);

            return target;
        }

        XNodeOut GetClassRef(TypeReference declaringType)
        {
            //if(!declaringType.IsGenericParameter && !declaringType.IsFunctionPointer)
            //    XDef.ParseAndCheck(declaringType.ToString());

            if (declaringType.IsGenericParameter)
                Debug.WriteLine("GetClassRef for Generic Param - " + declaringType.ToString());

            if (declaringType.IsFunctionPointer)
                Debug.WriteLine("GetClassRef for Function Pointer - " + declaringType.ToString());

            var scope = declaringType.Scope;

            if (scope.MetadataScopeType == MetadataScopeType.ModuleReference)
            {
                // is this scope type internal or external, should it be tracked externally?
                Debug.WriteLine("Skipped GetClassRef for - " + declaringType.ToString());
                Debug.Assert(false);
                return null;
            }

           
            //string namespaces = (declaringType.DeclaringType != null) ? declaringType.DeclaringType.Namespace : declaringType.Namespace;
            //string className = declaringType.Name;


            XNodeOut fileNode = null;

            // if xrayed internal
            if (scope.MetadataScopeType == MetadataScopeType.ModuleDefinition)
                fileNode = XFile.FileNode;

            // xrayed, but in diff module
            else if (XRayedFiles.Any(f => f.AssemblyName == scope.Name))
                fileNode = XRayedFiles.First(f => f.AssemblyName == scope.Name).FileNode;

            // if not xrayed - map to external root
            else
            {
                string moduleName = scope.Name;
                fileNode = ExtRoot.AddNode(moduleName, XObjType.File);
            }

            return SignatureToClass(declaringType.ToString(), fileNode);
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
                // class under a namespace could be anon
                if(node.Name.StartsWith("<>"))
                    MarkNodeAsAnon(node);

                // put anonymous classes in the functions that they were generated from
                // ex -. Nested Class: <>c__DisplayClass15, has method: <FilterTextBox_TextChanged>b__11
                var anonClasses = node.Nodes.Where(n => n.ObjType == XObjType.Class && n.Name.StartsWith("<>")).ToArray();

                foreach(XNodeOut anonClass in anonClasses)
                {
                    // mark as anon
                    MarkNodeAsAnon(anonClass);

                    // if parent is not anon
                    if( !anonClass.Parent.IsAnon )
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
                    node.Name = parent.Name + ((node.Name == ".ctor") ? ".ctor" : ".static_ctor");
                    if (parent.InitCount > 1)
                        node.Name += parent.InitCount.ToString();

                    parent.InitCount++;
                }

                else if (node.IsAnon || // simple anon method
                         node.Parent.IsAnon) // method of an anon class
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

        internal static void PrepareOutputDir(string sourcePath, string destPath)
        {
            // copy XLibrary to final destination
            CopyLocalToOutputDir("XLibrary.dll", destPath);
            CopyLocalToOutputDir("OpenTK.dll", destPath);
            CopyLocalToOutputDir("OpenTK.GLControl.dll", destPath);
            CopyLocalToOutputDir("QuickFont.dll", destPath);
        }

        private static void CopyLocalToOutputDir(string filename, string destPath)
        {
            File.Copy(Path.Combine(Application.StartupPath, filename), Path.Combine(destPath, filename), true);
        }

        internal static bool CheckIfAlreadyXRayed(XRayedFile[] files)
        {
            foreach (var file in files)
            {
                var asm = AssemblyDefinition.ReadAssembly(file.FilePath);

                if(asm.MainModule.AssemblyReferences.Any(r => r.Name == "XLibrary"))
                    return true;
            }

            return false;
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
