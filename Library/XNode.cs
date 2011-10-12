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
        Method, 
        Field
    }

    public class InputValue
    {
        public string Name;
        public long Value;

        public InputValue() { }

        public InputValue(string name, long value)
        {
            Name = name;
            Value = value;
        }
    }

    public class XNode : InputValue
    {
        public XObjType ObjType;

        public int ID;

        public XNode Parent;
        public List<XNode> Nodes = new List<XNode>();

        public bool External;


        public string FullName()
        {
            if (Parent == null) // dont show name for root node
                return "";

            string parent = Parent.FullName();

            string myName = Name;
            if (ObjType == XObjType.File)
                myName = "(" + Name + ")";

            return (parent == "") ? myName : parent + "." + myName;
        }

        internal string AppendClassName()
        {
            if (Parent != null && ObjType == XObjType.Method)
                return Parent.Name + "." + Name;
            else
                return Name;
        }

        internal XNode[] GetParents()
        {
            return GetParents(0, false);
        }

        private XNode[] GetParents(int count, bool includeRoot)
        {
            count++; // count is also the index position of this node from the back

            XNode[] result = null;

            if (Parent == null || (!includeRoot && Parent.ObjType == XObjType.Root))
                result = new XNode[count];
            else
                result = Parent.GetParents(count, includeRoot);

            result[result.Length - count] = this;

            return result;
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

    public class XNodeIn : XNode
    {
        internal int ParentID;
        internal int Lines; // save here so final value can be manipulated

        internal bool Hovered;
        internal bool Show = true;

        internal RectangleD AreaD { get; private set; }
        internal RectangleF AreaF { get; private set; }
        internal PointF CenterF { get; private set; }

        internal int FunctionHit;
        internal int LastCallingThread;
        internal int ConflictHit;
        internal int ExceptionHit;
        internal uint HitSequence;

        internal int EntryPoint;
        internal int StillInside;

        internal bool RoomForLabel;
        internal RectangleF LabelRect;

        internal SharedDictionary<FunctionCall> CalledIn;
        internal SharedDictionary<FunctionCall> CallsOut;
        internal FieldOp LastFieldOp;

        // call graph view
        internal int? Rank;
        internal List<XNodeIn> Adjacents;
        internal PointF ScaledLocation;
        internal float ScaledSize; // width and height

        internal InstanceRecord Record;

        internal int[] Dependencies = null;


        internal static XNodeIn Read(FileStream stream)
        {
            // total size 4
            // name size 4
            // name x
            // type 4
            // lines 4
            // id 8
            // parent exist? 1
            //      parent id 4
            // dependencies? 1
            //      dependency count 4
            //      dependent ids 4x

            XNodeIn node = new XNodeIn();

            long startPos = stream.Position;

            int totalSize = BitConverter.ToInt32(stream.Read(4), 0);
            int nameSize = BitConverter.ToInt32(stream.Read(4), 0);
            node.Name = UTF8Encoding.UTF8.GetString(stream.Read(nameSize));
            node.ObjType =(XObjType) BitConverter.ToInt32(stream.Read(4), 0);
            node.Lines = BitConverter.ToInt32(stream.Read(4), 0);
            node.Value = node.Lines; // default
            node.External = BitConverter.ToBoolean(stream.Read(1), 0);
            node.ID = BitConverter.ToInt32(stream.Read(4), 0);

            bool hasParent = BitConverter.ToBoolean(stream.Read(1), 0);
            if(hasParent)
                node.ParentID = BitConverter.ToInt32(stream.Read(4), 0);

            bool hasDependencies = BitConverter.ToBoolean(stream.Read(1), 0);
            if (hasDependencies)
            {
                int dependencyCount = BitConverter.ToInt32(stream.Read(4), 0);
                node.Dependencies = new int[dependencyCount];

                for (int i = 0; i < dependencyCount; i++)
                    node.Dependencies[i] = BitConverter.ToInt32(stream.Read(4), 0);
            }

            stream.Position = startPos + totalSize;

            return node;
        }

        internal void SetArea(RectangleD area)
        {
            AreaD = area;
            AreaF = area.ToRectangleF();
            CenterF = new PointF((float)(area.X + area.Width / 2),
                                     (float)(area.Y + area.Height / 2));
        }

        internal void AddCallIn(int sourceID, FunctionCall call)
        {
            if (CalledIn == null)
                CalledIn = new SharedDictionary<FunctionCall>(1);

            if (!CalledIn.Contains(sourceID))
                CalledIn.Add(sourceID, call);

        }

        internal void AddCallOut(int destId, FunctionCall call)
        {
            if (CallsOut == null)
                CallsOut = new SharedDictionary<FunctionCall>(1);

            if (!CallsOut.Contains(destId))
                CallsOut.Add(destId, call);
        }
    }
}