using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace XLibrary
{
    public enum XObjType 
    { 
        Root, 
        External,
        Internal,
        File, 
        Namespace, 
        Class, 
        Field,
        Method 
    }

    public class XNode
    {
        public int ID;
        public string Name;
        public long Lines;
        public XObjType ObjType;

        public XNode Parent;
        public List<XNode> Nodes = new List<XNode>();

        public bool External;
        public bool IsAnon;

        public int ReturnID;
        public int[] ParamIDs;
        public string[] ParamNames;

        public List<XInstruction> Msil;
        public byte[] CSharp;


        public string FullName(bool excludeFile=false)
        {
            if (Parent == null) // dont show name for root node
                return "";

            string parent = Parent.FullName(excludeFile);

            if (excludeFile && ObjType == XObjType.File)
                return "";

            string myName = Name;
            if (ObjType == XObjType.File)
                myName = "(" + Name + ")";

            return (parent == "") ? myName : parent + "." + myName;
        }

        public XNode GetParentClass(bool rootClass)
        {
            var parentClass = this;
            var up = Parent;

            while (up != null)
            {
                if (up.ObjType == XObjType.Class)
                {
                    parentClass = up;

                    if (!rootClass)
                        break;
                }
                up = up.Parent;
            }

            return parentClass;
        }

        /*public string MultiLineFullName()
        {
            int levels;
            return MultiLineFullName(out levels);
        }

        public string MultiLineFullName(out int levels)
        {
            if (Parent == null) // dont show name for root node
            {
                levels = 0;
                return "";
            }

            string result = Parent.MultiLineFullName(out levels);
            levels++;

            string spaces = new string(' ', levels * 1);

            return (result == "") ? Name : result + "\r\n" + spaces + Name;
        }*/
    }

    public enum FieldOp { Get, Set };

    [DebuggerDisplay("{Name}")]
    public class XNodeIn : XNode
    {
        public string UnformattedName; 
        
        internal int ParentID;

        internal int FunctionHit; 
        internal int ExceptionHit;
        internal int ConstructedHit;
        internal int DisposeHit;

        internal uint HitSequence;

        internal int EntryPoint;
        internal int StillInside;

        internal HashSet<int> ThreadIDs;

        internal SharedDictionary<FunctionCall> CalledIn;
        internal SharedDictionary<FunctionCall> CallsOut;

        internal SharedDictionary<FunctionCall> InitsOf;
        internal SharedDictionary<FunctionCall> InitsBy;

        internal FieldOp LastFieldOp;

        public InstanceRecord Record;

        internal int[] Dependencies;
        internal int[] Independencies;

        public long MsilPos;
        public int MsilLines;

        public long CSharpPos;
        public int CSharpLength;


        internal static XNodeIn Read(FileStream stream)
        {
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

            XNodeIn node = new XNodeIn();

            long startPos = stream.Position;

            int totalSize = BitConverter.ToInt32(stream.Read(4), 0);
            node.Name = ReadString(stream);

            node.ObjType =(XObjType) BitConverter.ToInt32(stream.Read(4), 0);
            node.Lines = BitConverter.ToInt64(stream.Read(8), 0);
            node.External = BitConverter.ToBoolean(stream.Read(1), 0);
            node.IsAnon = BitConverter.ToBoolean(stream.Read(1), 0);
            node.ID = BitConverter.ToInt32(stream.Read(4), 0);

            // mod name for readability
            node.UnformattedName = node.Name;
            node.Name = Utilities.FormatTemplateName(node.UnformattedName);
            
            if (node.ObjType == XObjType.Field)
            {
                int pos = node.Name.LastIndexOf(' ');
                if (pos != -1)
                    node.UnformattedName = node.Name.Substring(pos + 1);
            }

            bool hasParent = BitConverter.ToBoolean(stream.Read(1), 0);
            if(hasParent)
                node.ParentID = BitConverter.ToInt32(stream.Read(4), 0);

            node.ReturnID = BitConverter.ToInt32(stream.Read(4), 0);

            int paramCount = BitConverter.ToInt32(stream.Read(4), 0);
            if (paramCount > 0)
            {
                node.ParamIDs = new int[paramCount];
                node.ParamNames = new string[paramCount];

                for (int i = 0; i < paramCount; i++)
                {
                    node.ParamIDs[i] = BitConverter.ToInt32(stream.Read(4), 0);
                    node.ParamNames[i] = ReadString(stream);
                }
            }

            int dependencyCount = BitConverter.ToInt32(stream.Read(4), 0);
            if(dependencyCount > 0)
            {
                node.Dependencies = new int[dependencyCount];

                for (int i = 0; i < dependencyCount; i++)
                    node.Dependencies[i] = BitConverter.ToInt32(stream.Read(4), 0);
            }

            node.CSharpLength = BitConverter.ToInt32(stream.Read(4), 0);
            node.CSharpPos = stream.Position;
            stream.Position += node.CSharpLength;

            node.MsilLines = BitConverter.ToInt32(stream.Read(4), 0);
            if (node.MsilLines > 0)
                node.MsilPos = stream.Position;
            
            //stream.Position += node.MsilLines; // lines dont translate into bytes

            stream.Position = startPos + totalSize;

            return node;
        }

        public static string ReadString(FileStream stream)
        {
            int strlen = BitConverter.ToInt32(stream.Read(4), 0);

            if (strlen == 0)
                return "";

            return UTF8Encoding.UTF8.GetString(stream.Read(strlen));
        }

        internal void AddFunctionCall(ref SharedDictionary<FunctionCall> map, int nodeId, FunctionCall call)
        {
            if (map == null)
                map = new SharedDictionary<FunctionCall>(1);

            if (!map.Contains(nodeId))
                map.Add(nodeId, call);
        }

        public string GetMethodName(bool includeClass)
        {
            string name = Name;

            if (includeClass)
            {
                var parentClass = GetParentClass(false);
                name = parentClass.Name + "::" + Name;
            }

            if (ReturnID != 0)
            {
                var retNode = XRay.Nodes[ReturnID];
                name = retNode.Name + " " + name;
            }

            if (ObjType == XObjType.Method)
            {
                name += "(";

                if (ParamIDs != null)
                {
                    for (int i = 0; i < ParamIDs.Length; i++)
                    {
                        var pId = ParamIDs[i];
                        var pName = ParamNames[i];
                        var pNode = XRay.Nodes[pId];

                        name += pNode.Name + " " + pName + ", ";
                    }

                    name = name.TrimEnd(' ', ',');
                }

                name += ")";
            }

            return name;
        }

        internal void DecrementHits()
        {
            if (FunctionHit > 0)
                FunctionHit--;

            if (ExceptionHit > 0)
                ExceptionHit--;

            if (ConstructedHit > 0)
                ConstructedHit--;

            if (DisposeHit > 0)
                DisposeHit--;
        }
    }

    public class XInstruction
    {
        public int Offset;
        public string OpCode;
        public string Line;
        public int RefId;
    }


}