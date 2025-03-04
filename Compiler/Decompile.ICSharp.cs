using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
using ICSharpCode.Decompiler.Semantics;
using ICSharpCode.Decompiler.TypeSystem;
//using ICSharpCode.NRefactory;
//using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;

using XLibrary;


namespace XBuilder
{
    public class DecompileICSharp
    {
        static public byte[] DecompileMethod(MethodDefinition method)
        {
            var binout = new BinaryOutput2();

            try
            {
                DecompileMethod(method, binout);
            }
            catch (Exception ex)
            {
                binout.WriteComment(CommentType.SingleLine, "Error Decompiling: " + ex.Message);
            }

            return binout.GetOutput();
        }

        static public void DecompileMethod(MethodDefinition method, BinaryOutput2 output)
        {
            try
            {
                // Create decompiler settings
                var settings = new DecompilerSettings() { ThrowOnAssemblyResolveErrors = false };

                // Special handling for constructors to include field initializers
                bool isConstructor = method.IsConstructor && !method.IsStatic && !method.DeclaringType.IsValueType;

                // Create the decompiler
                var decompiler = new CSharpDecompiler(method.DeclaringType.Module.FileName, settings);

                // Get the type definition from the type system
                var typeFullName = new FullTypeName(method.DeclaringType.FullName);
                var typeDefinition = decompiler.TypeSystem.FindType(typeFullName).GetDefinition();

                // Write a comment with the declaring type
                output.WriteComment(CommentType.Documentation, method.DeclaringType.FullName);

                // Convert the Mono.Cecil metadata token to System.Reflection.Metadata token
                var metadataToken = System.Reflection.Metadata.Ecma335.MetadataTokens.MethodDefinitionHandle((int)method.MetadataToken.RID);

                SyntaxTree syntaxTree;

                if (isConstructor)
                {
                    // For constructors, we need to include fields to show field initializers
                    // First, decompile the whole type
                    syntaxTree = decompiler.DecompileType(typeFullName);

                    // Filter the syntax tree to only include the constructor and relevant fields
                    FilterSyntaxTreeForConstructor(syntaxTree, method);
                }
                else
                {
                    // For regular methods, just decompile the method
                    syntaxTree = decompiler.Decompile(metadataToken);
                }

                // Write the syntax tree to the output
                var visitor = new CSharpOutputVisitor(output, FormattingOptionsFactory.CreateMono());
                syntaxTree.AcceptVisitor(visitor);
            }
            catch (Exception ex)
            {
                output.WriteComment(CommentType.SingleLine, "Error Decompiling: " + ex.Message);
            }
        }

        /// <summary>
        /// Filters a syntax tree to only include the specified constructor and relevant field initializers
        /// </summary>
        static private void FilterSyntaxTreeForConstructor(SyntaxTree syntaxTree, MethodDefinition ctorMethod)
        {
            // Get the method token for identification
            var ctorToken = ctorMethod.MetadataToken.ToInt32();

            // Get the TypeDeclaration node which contains all members
            var typeDeclaration = syntaxTree.Children
                .OfType<ICSharpCode.Decompiler.CSharp.Syntax.NamespaceDeclaration>()
                .SelectMany(n => n.Members)
                .OfType<ICSharpCode.Decompiler.CSharp.Syntax.TypeDeclaration>()
                .FirstOrDefault(t => t.Name == ctorMethod.DeclaringType.Name);

            if (typeDeclaration == null)
                return;

            // Find our specific constructor
            var targetCtor = typeDeclaration.Members
                .OfType<ICSharpCode.Decompiler.CSharp.Syntax.ConstructorDeclaration>()
                .FirstOrDefault(c => {
                    var metadata = c.GetResolveResult() as MemberResolveResult;
                    return metadata != null && metadata.Member.MetadataToken.GetHashCode() == ctorToken;
                });

            if (targetCtor == null)
                return;

            // If this constructor calls another constructor via "this(...)", 
            // we don't need field initializers
            bool callsThisConstructor = targetCtor.Initializer != null &&
                targetCtor.Initializer.ConstructorInitializerType ==
                ICSharpCode.Decompiler.CSharp.Syntax.ConstructorInitializerType.This;

            // Keep only necessary members
            var membersToKeep = new List<ICSharpCode.Decompiler.CSharp.Syntax.AstNode>();

            // Keep fields if needed (only if not calling another constructor)
            if (!callsThisConstructor)
            {
                // Keep field declarations with initializers
                var fieldsWithInitializers = typeDeclaration.Members
                    .OfType<ICSharpCode.Decompiler.CSharp.Syntax.FieldDeclaration>()
                    .Where(fd =>
                        // Match static fields with the constructor's static status
                        fd.Modifiers.HasFlag(ICSharpCode.Decompiler.CSharp.Syntax.Modifiers.Static) == ctorMethod.IsStatic &&
                        // Only keep fields with initializers
                        fd.Variables.Any(v => v.Initializer != null));

                membersToKeep.AddRange(fieldsWithInitializers);
            }

            // Add the target constructor
            membersToKeep.Add(targetCtor);

            // Remove all members that we don't want to keep
            foreach (var member in typeDeclaration.Members.ToList())
            {
                if (!membersToKeep.Contains(member))
                    member.Remove();
            }
        }

        /*public void DecompileMethodOld(MethodDefinition method, ITextOutput output)
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
            }
    }*/

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
        }*/
    }
}
