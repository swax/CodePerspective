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

    public enum XPacketType
    {
        Node = 1, Setting = 2, CallMap = 3, InitMap = 4
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
        public string PlainCSharp;
        public string PlainMsil;


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
        internal int FunctionNewHit;
        internal int ExceptionHit;
        internal int ConstructedHit;
        internal int DisposeHit;

        internal uint HitSequence;

        internal int EntryPoint;
        internal int StillInside;

        internal HashSet<int> ThreadIDs;

        // either method to method, or class to class calls
        internal SharedDictionary<FunctionCall> CalledIn;
        internal SharedDictionary<FunctionCall> CallsOut;

        // class initializing another class
        internal HashSet<int> InitsOf;
        internal HashSet<int> InitsBy;

        // layer graph, graph for each depth from root
        internal HashSet<int> LayerIn;
        internal HashSet<int> LayerOut;

        internal FieldOp LastFieldOp;

        public InstanceRecord Record;

        internal int[] Dependencies;
        internal int[] Independencies;

        public long MsilPos;
        public int MsilLines;

        public long CSharpPos;
        public int CSharpLength;


        public static XPacketType ReadNextPacket(FileStream stream, out int totalSize)
        {
            totalSize = BitConverter.ToInt32(stream.Read(4), 0);

            return (XPacketType) stream.ReadByte();
        }

        public static void ReadSetting(FileStream stream, out string name, out string value)
        {
            int totalSize = BitConverter.ToInt32(stream.Read(4), 0);

            int packetType = stream.ReadByte();
            Debug.Assert(packetType == (int)XPacketType.Setting);

            name = ReadString(stream);
            value = ReadString(stream);
        }

        public static XNodeIn ReadNode(FileStream stream)
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

            int totalSize = BitConverter.ToInt32(stream.Read(4), 0);

            int packetType = stream.ReadByte();
            Debug.Assert(packetType == (int)XPacketType.Node);

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

            if (FunctionNewHit > 0)
                FunctionNewHit--;

            if (ExceptionHit > 0)
                ExceptionHit--;

            if (ConstructedHit > 0)
                ConstructedHit--;

            if (DisposeHit > 0)
                DisposeHit--;
        }

        XNodeIn[] ParentChain;

        internal XNodeIn[] GetParentChain()
        {
            if (ParentChain != null)
                return ParentChain;

            var chain = new List<XNodeIn>();

            var next = this;
            chain.Add(next);

            while (next.Parent != null)
            {
                next = next.Parent as XNodeIn;
                chain.Add(next);
            }

            ParentChain = chain.ToArray();
            return ParentChain;
        }

        internal bool LoadMsil()
        {
            if (MsilPos == 0 || MsilLines == 0)
                return false;

            if (Msil != null)
                return true;

            using (FileStream DatStream = new FileStream(XRay.DatPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                DatStream.Position = MsilPos;

                Msil = new List<XInstruction>();
                var code = new StringBuilder();

                for (int i = 0; i < MsilLines; i++)
                {
                    var inst = new XInstruction();
                    inst.Offset = BitConverter.ToInt32(DatStream.Read(4), 0);
                    inst.OpCode = XNodeIn.ReadString(DatStream);
                    inst.Line = XNodeIn.ReadString(DatStream);
                    inst.RefId = BitConverter.ToInt32(DatStream.Read(4), 0);

                    if (inst.RefId != 0 && !inst.Line.StartsWith("goto "))
                        inst.Line = Utilities.GetMethodName(this, inst.RefId);

                    Msil.Add(inst);
                    code.Append(inst.Offset.ToString("X") + ": " + inst.OpCode + " " + inst.Line + "\r\n");
                }

                PlainMsil = code.ToString();
            }

            return true;
        }

        internal bool LoadCSharp()
        {
            if (CSharpPos == 0 || CSharpLength == 0)
                return false;

            if (CSharp != null)
                return true;

            using (FileStream DatStream = new FileStream(XRay.DatPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                DatStream.Position = CSharpPos;

                CSharp = DatStream.Read(CSharpLength);
            }

            // read byte stream and build html
            var code = new StringBuilder();

            // format - id, length, string
            var stream = new MemoryStream(CSharp);

            while (stream.Position < stream.Length)
            {
                var id = BitConverter.ToInt32(stream.Read(4), 0);
                var strlen = BitConverter.ToInt32(stream.Read(4), 0);
                string text = UTF8Encoding.UTF8.GetString(stream.Read(strlen));

               code.Append(text);
            }

            PlainCSharp = code.ToString();

            return true;
        }

        
        internal string GetMethodCode()
        {
            if (LoadCSharp())
                return PlainCSharp;

            if (LoadMsil())
                return PlainMsil;

            return "Code not available";
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