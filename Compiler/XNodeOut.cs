﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Mono.Cecil;
using XLibrary;
using System.Runtime.Serialization;
using Mono.Cecil.Cil;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;


namespace XBuilder
{
    public class XNodeOut : XNode
    {
        public static int NextID = 1;

        public static Dictionary<string, XNodeOut> TypeNodeMap = new Dictionary<string, XNodeOut>();
        public static Dictionary<int, List<XNodeOut>> MethodNodeMap = new Dictionary<int, List<XNodeOut>>();
        public static Dictionary<int, List<XNodeOut>> FieldNodeMap = new Dictionary<int, List<XNodeOut>>();


        public bool Exclude;
        public int Indent;
        public string IndentString = "    "; // 2 for class, 2 for method
        public int InitCount = 1;
        public int AnonFuncs = 1;
        public int AnonClasses = 1;

        public HashSet<int> ClassDependencies;

        public string MethodFullName;
        public string FieldFullName;

        public XNodeOut(XNodeOut parent, string name, XObjType objType)
        {
            Parent = parent;
            Name = name;
            ObjType = objType;

            if (objType == XObjType.External)
                External = true;

            else if (parent != null) // else important cause external objs parent is not tagged as external
                External = parent.External;

            ID = NextID++;

            //Debug.WriteLine(string.Format("Added {0}: {1}", objType, FullName()));
        }

        public XNodeOut AddNode(string name, XObjType objType)
        {
            // used for namespaces
            XNodeOut existing = Nodes.Where(n => n.Name == name && n.ObjType == objType).FirstOrDefault() as XNodeOut;
            if (existing != null)
                return existing;

            XNodeOut node = new XNodeOut(this, name, objType);
            Nodes.Add(node);
            return node;
        }

        public XNodeOut AddMethod(MethodReference method)
        {
            XNodeOut existing = Nodes.Cast<XNodeOut>().Where(n => n.MethodFullName == method.FullName).FirstOrDefault();
            if (existing != null)
                return existing;

            XNodeOut node = new XNodeOut(this, method.Name, XObjType.Method);
            Nodes.Add(node);

            node.MethodFullName = method.FullName;

            int tokenId = method.MetadataToken.ToInt32();

            if (MethodNodeMap.TryGetValue(tokenId, out List<XNodeOut> existingTokens))
            {
                if (!existingTokens.Any(n => n.MethodFullName == method.FullName))
                    existingTokens.Add(node);
            }
            else 
                MethodNodeMap[tokenId] = new List<XNodeOut> { node }; 

            return node;
        }
        
        public long ComputeSums()
        {
            long sum = Lines;

            foreach (XNodeOut node in Nodes.Cast<XNodeOut>().Where(n => !n.Exclude))
                sum += node.ComputeSums();

            Lines = sum;

            return sum;
        }

        public long WriteNode(FileStream stream)
        {
            if (Exclude)
                return 0;

            // total size 4
            // name size 4
            // name x
            // type 4
            // value 4
            // external 1
            // anon 1
            // id 8
            // parent exist? 1
            //      parent id 4
            // dependencies? 1
            //      dependency count 4
            //      dependent ids 4x

            long startPos = stream.Length;

            // write empty size of packet to be set at the end of the function
            stream.Write(BitConverter.GetBytes(0));

            stream.WriteByte((byte)XPacketType.Node);

            BuildModel.WriteString(stream, Name);

            stream.Write(BitConverter.GetBytes((int)ObjType));

            stream.Write(BitConverter.GetBytes(Lines));

            stream.Write(BitConverter.GetBytes(External));

            stream.Write(BitConverter.GetBytes(IsAnon));

            stream.Write(BitConverter.GetBytes(ID));

            // parent 
            stream.Write(BitConverter.GetBytes(Parent != null));

            if (Parent != null)
                stream.Write(BitConverter.GetBytes(Parent.ID));

            // return
            stream.Write(BitConverter.GetBytes(ReturnID));

            // params
            int paramCount = 0;
            if (ParamIDs != null)
                paramCount = ParamIDs.Length;
            stream.Write(BitConverter.GetBytes(paramCount));

            if (paramCount > 0)
                for (int i = 0; i < paramCount; i++)
                {
                    stream.Write(BitConverter.GetBytes(ParamIDs[i]));
                    BuildModel.WriteString(stream, ParamNames[i]);
                }

            // dependencies
            int dependencyCount = 0;
            if (ClassDependencies != null)
                dependencyCount = ClassDependencies.Count;
            stream.Write(BitConverter.GetBytes(dependencyCount));

            if (dependencyCount > 0)
                foreach (int dependentId in ClassDependencies)
                    stream.Write(BitConverter.GetBytes(dependentId));

            // write C#
            int csharpLength = (CSharp != null) ? CSharp.Length : 0;
            stream.Write(BitConverter.GetBytes(csharpLength));

            if (csharpLength > 0)
                stream.Write(CSharp);

            // write MSIL
            int codeLines = (Msil != null) ? Msil.Count : 0;
            stream.Write(BitConverter.GetBytes(codeLines));

            if (codeLines > 0)
                foreach (var inst in Msil)
                {
                    stream.Write(BitConverter.GetBytes(inst.Offset));
                    BuildModel.WriteString(stream, inst.OpCode);
                    BuildModel.WriteString(stream, inst.Line);
                    stream.Write(BitConverter.GetBytes(inst.RefId));
                }

            // write size of packet
            stream.Position = startPos;
            stream.Write(BitConverter.GetBytes((int)(stream.Length - startPos)));
            stream.Position = stream.Length;

            long trackedObjects = 1;

            foreach (XNodeOut child in Nodes.Cast<XNodeOut>())
                trackedObjects += child.WriteNode(stream);

            return trackedObjects;
        }

        internal XNodeOut AddField(FieldReference field)
        {
            // include namespace later for dynamic lookups - string name = fieldDef.FieldType.FullName + "::" + fieldDef.Name;
            XDef fieldTypeDef = XDef.ParseAndCheck(field.FieldType.ToString());

            string name = fieldTypeDef.GetShortName() + " " + field.Name;

            XNodeOut existing = Nodes.Cast<XNodeOut>().Where(n => n.FieldFullName == field.FullName).FirstOrDefault();
            if (existing != null)
                return existing;

            var node = AddNode(name, XObjType.Field);
            Nodes.Add(node);

            node.Lines = 1;
            node.FieldFullName = field.FullName;

            int tokenId = field.MetadataToken.ToInt32();

            if (FieldNodeMap.TryGetValue(tokenId, out List<XNodeOut> existingTokens))
            {
                if (!existingTokens.Any(n => n.FieldFullName == field.FullName))
                    existingTokens.Add(node);
            }
            else
                FieldNodeMap[tokenId] = new List<XNodeOut> { node };

            return node;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}, lines {2}", ObjType, Name, Lines);
        }

        internal XNodeOut GetNotAnonParent()
        {
            XNodeOut next = Parent as XNodeOut;

            while (next != null && next.IsAnon)
                next = next.Parent as XNodeOut;

            return next;
        }
    }

    class CodeLine
    {

    }
}
