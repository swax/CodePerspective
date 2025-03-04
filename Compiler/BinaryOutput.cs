using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.Disassembler;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBuilder
{

    /**
     * This implementation uses the old version of ICSharp decompiler
     */
    /*public sealed class BinaryOutput : ITextOutput
    {
        MemoryStream Output;

        int indent;
        bool needsIndent;

        int line = 1;
        int column = 1;

        StringBuilder PendingWrite = new StringBuilder();


        public BinaryOutput()
        {
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

        void WriteOutput(string text, int id = 0, bool flush = false)
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

            if (id != 0)
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

        void ITextOutput.MarkFoldStart(string collapsedText, bool defaultCollapsed, bool isDefinition)
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

        public void WriteReference(MetadataFile metadata, System.Reflection.Metadata.Handle handle, string text, string protocol, bool isDefinition)
        {
            throw new NotImplementedException();
        }

        public void WriteLocalReference(string text, object reference, bool isDefinition = false)
        {
            throw new NotImplementedException();
        }
    }*/
}
