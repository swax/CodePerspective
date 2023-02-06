using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;

using ICSharpCode.Decompiler;
//using ICSharpCode.Decompiler.Ast;
//using ICSharpCode.Decompiler.Ast.Transforms;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;
using ICSharpCode.Decompiler.Disassembler;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
//using ICSharpCode.NRefactory;
//using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;

using XLibrary;


namespace XBuilder
{
    public partial class XDecompile
    {
        public byte[] DecompileMethod2(MethodDefinition method)
        {
            string result = string.Empty;

            try
            {
                var settings = new DecompilerSettings() { ThrowOnAssemblyResolveErrors = false };
                var decompiler = new CSharpDecompiler(method.DeclaringType.Module.FileName, settings);

                var systemReflectionMetadataToken = MetadataTokens.MethodDefinitionHandle((int)method.MetadataToken.RID);
                var syntaxTree = decompiler.Decompile(systemReflectionMetadataToken);

                var writer = new StringWriter();
                var visitor = new CSharpOutputVisitor(writer, FormattingOptionsFactory.CreateAllman());
                syntaxTree.AcceptVisitor(visitor);
                writer.Flush();

                result = writer.ToString();
            }
            catch (Exception ex)
            {
                result = "Error Decompiling: " + ex.Message;
            }

            return Encoding.UTF8.GetBytes(result);
        }

        /*public byte[] DecompileMethod(MethodDefinition method)
        {
            var binout = new BinaryOutput(this);
            
            try
            {
                DecompileMethod(method, binout);
            }
            catch (Exception ex)
            {
                binout.Write("Error Decompiling: " + ex.Message);
            }

            return binout.GetOutput();
        }
      
        public void DecompileMethod(MethodDefinition method, ITextOutput output)
        {

            WriteCommentLine(output?/, TypeToString(method.DeclaringType, includeNamespace: true));
            AstBuilder codeDomBuilder = CreateAstBuilder(settings, currentType: method.DeclaringType, isSingleMember: true);
            if (method.IsConstructor && !method.IsStatic && !method.DeclaringType.IsValueType)
            {
                // also fields and other ctors so that the field initializers can be shown as such
                AddFieldsAndCtors(codeDomBuilder, method.DeclaringType, method.IsStatic);
                RunTransformsAndGenerateCode(codeDomBuilder, output, settings, new SelectCtorTransform(method));
            }
            else
            {
                codeDomBuilder.AddMethod(method);
                RunTransformsAndGenerateCode(codeDomBuilder, output, settings);
            }*/
    }

    /*AstBuilder CreateAstBuilder(DecompilerSettings settings, ModuleDefinition currentModule = null, TypeDefinition currentType = null, bool isSingleMember = false)
    {
        if (currentModule == null)
            currentModule = currentType.Module;

        if (isSingleMember)
        {
            settings = settings.Clone();
            settings.UsingDeclarations = false;
        }

        return new AstBuilder(
            new DecompilerContext(currentModule)
            {
                //CancellationToken = options.CancellationToken,
                CurrentType = currentType,
                Settings = settings
            });
    }

    void AddFieldsAndCtors(AstBuilder codeDomBuilder, TypeDefinition declaringType, bool isStatic)
    {
        foreach (var field in declaringType.Fields)
        {
            if (field.IsStatic == isStatic)
                codeDomBuilder.AddField(field);
        }
        foreach (var ctor in declaringType.Methods)
        {
            if (ctor.IsConstructor && ctor.IsStatic == isStatic)
                codeDomBuilder.AddMethod(ctor);
        }
    }

    void RunTransformsAndGenerateCode(AstBuilder astBuilder, ITextOutput output, DecompilerSettings settings, IAstTransform additionalTransform = null)
    {
        astBuilder.RunTransformations();//transformAbortCondition);
        if (additionalTransform != null)
        {
            additionalTransform.Run(astBuilder.CompilationUnit);
        }
        if (settings.ShowXmlDocumentation)
        {
            //AddXmlDocTransform.Run(astBuilder.CompilationUnit);
        }
        astBuilder.GenerateCode(output);
    }

    class SelectCtorTransform : IAstTransform
    {
        readonly MethodDefinition ctorDef;

        public SelectCtorTransform(MethodDefinition ctorDef)
        {
            this.ctorDef = ctorDef;
        }

        public void Run(AstNode compilationUnit)
        {
            ConstructorDeclaration ctorDecl = null;
            foreach (var node in compilationUnit.Children)
            {
                ConstructorDeclaration ctor = node as ConstructorDeclaration;
                if (ctor != null)
                {
                    if (ctor.Annotation<MethodDefinition>() == ctorDef)
                    {
                        ctorDecl = ctor;
                    }
                    else
                    {
                        // remove other ctors
                        ctor.Remove();
                    }
                }
                // Remove any fields without initializers
                FieldDeclaration fd = node as FieldDeclaration;
                if (fd != null && fd.Variables.All(v => v.Initializer.IsNull))
                    fd.Remove();
            }
            if (ctorDecl.Initializer.ConstructorInitializerType == ConstructorInitializerType.This)
            {
                // remove all fields
                foreach (var node in compilationUnit.Children)
                    if (node is FieldDeclaration)
                        node.Remove();
            }
        }

        public void Run(AstNode rootNode, TransformContext context)
        {
            throw new NotImplementedException();
        }
    }

    // based on PlainTextOutput
    public sealed class BinaryOutput : ITextOutput
    {
        XDecompile Decompiler;
        MemoryStream Output;

        int indent;
        bool needsIndent;

        int line = 1;
        int column = 1;

        StringBuilder PendingWrite = new StringBuilder();


        public BinaryOutput(XDecompile decompiler)
        {
            Decompiler = decompiler;
            Output = new MemoryStream();
        }

        public byte[] GetOutput()
        {
            // flush pending output
            WriteOutput("", 0, true);

            return Output.ToArray();
        }

        public TextLocation Location
        {
            get
            {
                return new TextLocation(line, column + (needsIndent ? indent : 0));
            }
        }

        public string IndentationString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Indent()
        {
            indent++;
        }

        public void Unindent()
        {
            indent--;
        }

        void WriteOutput(string text, int id=0, bool flush=false)
        {
            // accumulate non-linked output to write all at once
            if (id == 0)
            {
                PendingWrite.Append(text);

                if (!flush)
                    return;
            }

            // only regular output isn't flushed immediately
            if (PendingWrite.Length > 0)
            {
                WriteFinalOutput(PendingWrite.ToString(), 0);
                PendingWrite.Clear();
            }

            if(id != 0)
                WriteFinalOutput(text, id);
        }

        void WriteFinalOutput(string text, int id)
        {
            if (text.Length == 0)
                return;

            Output.Write(BitConverter.GetBytes(id));

            byte[] binary = UTF8Encoding.UTF8.GetBytes(text);
            Output.Write(BitConverter.GetBytes(binary.Length));
            Output.Write(binary);
        }

        void WriteIndent()
        {
            if (needsIndent)
            {
                needsIndent = false;
                for (int i = 0; i < indent; i++)
                {
                    WriteOutput("    ");
                }
                column += indent;
            }
        }

        public void Write(char ch)
        {
            WriteIndent();
            WriteOutput(ch.ToString());
            column++;
        }

        public void Write(string text)
        {
            Write(text, 0);
        }

        public void Write(string text, int id)
        {
            WriteIndent();
            WriteOutput(text, id);
            column += text.Length;
        }

        public void WriteLine()
        {
            WriteOutput("\r\n");
            needsIndent = true;
            line++;
            column = 1;
        }

        public void WriteDefinition(string text, object definition, bool isLocal)
        {
            Write(text);
        }

        public void WriteReference(string text, object reference, bool isLocal)
        {
            // if field
            var fieldDef = reference as FieldDefinition;

            if (text == "(" || text == ")" || text == ",")
            {
                Write(text);
                return;
            }

            if (fieldDef != null)
            {
                var classRef = Decompiler.GetClassRef(fieldDef.DeclaringType);
                var fieldNode = classRef.AddField(fieldDef);
                Write(text, fieldNode.ID);
                return;
            }

            // if method
            var methodRef = reference as MethodReference;

            if (methodRef != null)
            {
                var classRef = Decompiler.GetClassRef(methodRef.DeclaringType);
                var methodNode = classRef.AddMethod(methodRef);
                Write(text, methodNode.ID);
                return;
            }

            // if class
            var typeRef = reference as TypeReference;

            if (typeRef != null)
            {
                var classNode = Decompiler.GetClassRef(typeRef);
                Write(text, classNode.ID);
                return;
            }

            // else unknown
            Write(text);
        }

        void ITextOutput.MarkFoldStart(string collapsedText, bool defaultCollapsed)
        {
        }

        void ITextOutput.MarkFoldEnd()
        {
        }

        public void WriteReference(OpCodeInfo opCode, bool omitSuffix = false)
        {
            throw new NotImplementedException();
        }

        public void WriteReference(PEFile module, System.Reflection.Metadata.Handle handle, string text, string protocol = "decompile", bool isDefinition = false)
        {
            throw new NotImplementedException();
        }

        public void WriteReference(IType type, string text, bool isDefinition = false)
        {
            throw new NotImplementedException();
        }

        public void WriteReference(IMember member, string text, bool isDefinition = false)
        {
            throw new NotImplementedException();
        }

        public void WriteLocalReference(string text, object reference, bool isDefinition = false)
        {
            throw new NotImplementedException();
        }
    }*/
}
