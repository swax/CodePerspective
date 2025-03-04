using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.CSharp.Resolver;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.Semantics;
using ICSharpCode.Decompiler.TypeSystem;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace XBuilder
{
    /**
     * This implementation uses the newest version of ICSharp decompiler
     */
    public sealed class BinaryOutput2 : TokenWriter
    {
        MemoryStream Output;

        int indent;
        bool needsIndent;

        public static int MagicTypeIDPrefix = 1272301500;
        public static Dictionary<string, int> TypeIDMap = new Dictionary<string, int>();
        public static int NextTypeID = MagicTypeIDPrefix;

        int line = 1;
        int column = 1;

        StringBuilder PendingWrite = new StringBuilder();

        public BinaryOutput2()
        {
            Output = new MemoryStream();
        }

        public byte[] GetOutput()
        {
            // flush pending output
            WriteOutput("", 0, true);
            return Output.ToArray();
        }

        private void WriteOutput(string text, int id = 0, bool flush = false)
        {
            //Debug.Write(text);
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

        private void WriteFinalOutput(string text, int id)
        {
            if (text.Length == 0)
                return;

            Output.Write(BitConverter.GetBytes(id));

            byte[] binary = UTF8Encoding.UTF8.GetBytes(text);
            Output.Write(BitConverter.GetBytes(binary.Length));
            Output.Write(binary);
        }

        private void WriteIndent()
        {
            if (needsIndent)
            {
                needsIndent = false;
                for (int i = 0; i < indent; i++)
                {
                    WriteOutput("    ");
                }
                column += indent * 4;
            }
        }

        // Private helper to write text (since Write is not part of TokenWriter)
        private void WriteText(string text, int id = 0)
        {
            WriteIndent();
            WriteOutput(text, id);
            column += text.Length;
        }

        // TokenWriter implementation
        public override void StartNode(AstNode node)
        {
            // No special handling needed for starting nodes
        }

        public override void EndNode(AstNode node)
        {
            // No special handling needed for ending nodes
        }

        public override void WriteIdentifier(Identifier identifier)
        {
            int id = 0;

            if (identifier.Parent is SimpleType simpleType)
            {
                var typeResolveResult = simpleType.GetResolveResult() as TypeResolveResult;

                if (typeResolveResult != null)
                {
                    // Example ICSharpt ReflectionName: XTestLib.LibClass+T3Class`1[[System.Int32]]
                    // Example class def fullname: XTestLib.LibClass/T3Class`1<System.Int32>

                    // Convert the ICSharp format name to the Cecil name so we can get the xray ID later on
                    var typeName = ConvertICSharpTypeToCecil(typeResolveResult.Type.ReflectionName);

                    var bracketPos = typeName.IndexOf('[');
                    if (bracketPos > 0)
                    {
                        typeName = typeName.Substring(0, bracketPos);
                    }

                    if (TypeIDMap.TryGetValue(typeName, out int typeId))
                    {
                        id = typeId;
                    }
                    else
                    {
                        id = NextTypeID++;
                        TypeIDMap[typeName] = id;
                    }
                }
            }
            else if (identifier.Parent is IdentifierExpression identExp)
            {
                if (identifier.Parent.Parent is InvocationExpression invoExp)
                {
                    var identResolveResult = invoExp.GetResolveResult() as MemberResolveResult;
                    if (identResolveResult != null)
                    {
                        // Set metadata token id for now, and later once all methods processed, replace with the xray id
                        id = identResolveResult.Member.MetadataToken.GetHashCode();
                    }
                }
            }

            WriteText(identifier.Name, id);
        }

        public static string ConvertICSharpTypeToCecil(string icSharpType)
        {
            // Handle nested types (+ becomes /)
            string result = icSharpType.Replace("+", "/");

            // Replace brackets and separators
            result = result
                .Replace("[[", "<")
                .Replace("]]", ">")
                .Replace("],[", ",");

            // Handle generic parameter placeholders inside brackets
            result = result
                .Replace("``0", "!0")
                .Replace("``1", "!1")
                .Replace("``2", "!2")
                .Replace("``3", "!3")
                .Replace("<`0", "<!0");

            return result;
        }

        public override void WriteKeyword(Role role, string keyword)
        {
            WriteText(keyword);
        }

        public override void WriteToken(Role role, string token)
        {
            WriteText(token);
        }

        public override void WritePrimitiveValue(object value, LiteralFormat format = LiteralFormat.None)
        {
            // Convert primitive value to string and write it
            string text;
            if (value == null)
                text = "null";
            else if (value is string str)
                text = "\"" + str.Replace("\"", "\\\"") + "\""; // Basic escaping
            else
                text = value.ToString();
            
            WriteText(text);
        }

        public override void WritePrimitiveType(string type)
        {
            WriteText(type);
        }

        public override void WriteInterpolatedText(string text)
        {
            WriteText(text);
        }

        public override void Space()
        {
            WriteText(" ");
        }

        public override void Indent()
        {
            indent++;
        }

        public override void Unindent()
        {
            indent--;
        }

        public override void NewLine()
        {
            WriteOutput("\r\n");
            needsIndent = true;
            line++;
            column = 1;
        }

        public override void WriteComment(CommentType commentType, string content)
        {
            switch (commentType)
            {
                case CommentType.SingleLine:
                    WriteText("// " + content);
                    break;
                case CommentType.MultiLine:
                    WriteText("/* " + content + " */");
                    break;
                case CommentType.Documentation:
                    WriteText("/// " + content);
                    break;
            }
            NewLine();
        }

        public override void WritePreProcessorDirective(PreProcessorDirectiveType type, string argument)
        {
            WriteText("#" + type.ToString().ToLowerInvariant());
            if (!string.IsNullOrEmpty(argument))
            {
                WriteText(" " + argument);
            }
        }
    }
}
