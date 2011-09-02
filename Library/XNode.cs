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

    public class XNodeOut : XNode
    {
        public static int NextID = 1;

        public bool Exclude;
        public int Lines;
        public int Indent;
        public string IndentString = "    "; // 2 for class, 2 for method
        public int AnonFuncs = 1;
        public int AnonClasses = 1;

        public XNodeOut(XNodeOut parent, string name, XObjType objType)
        {
            Parent = parent;
            Name = name;
            ObjType = objType;

            if (objType == XObjType.External)
                External = true;

            else if(parent != null) // else important cause external objs parent is not tagged as external
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

        public int ComputeSums()
        {
            int sum = Lines;

            foreach (XNodeOut node in Nodes.Cast<XNodeOut>().Where(n => !n.Exclude))
                sum += node.ComputeSums();

            Value = sum;

            return sum;
        }

        public long SaveTree(string dir)
        {
            long trackedObjects = 0;

            ComputeSums();

            string path = Path.Combine(dir, "XRay.dat");

            byte[] temp = new byte[1024];

            using (FileStream stream = new FileStream(path, FileMode.Create))
                trackedObjects += Write(stream, temp);

            return trackedObjects;
        }

        public long Write(FileStream stream, byte[] temp)
        {
            if (Exclude)
                return 0;

            // total size 4
            // name size 4
            // name x
            // type 4
            // value 4
            // external 1
            // id 8
            // optional parent id 8

            int pos = 0;

            byte[] name = UTF8Encoding.UTF8.GetBytes(Name);

            BitConverter.GetBytes(name.Length).CopyTo(temp, pos);
            pos += 4;

            name.CopyTo(temp, pos);
            pos += name.Length;

            BitConverter.GetBytes((int)ObjType).CopyTo(temp, pos);
            pos += 4;

            BitConverter.GetBytes(Lines).CopyTo(temp, pos);
            pos += 4;

            BitConverter.GetBytes(External).CopyTo(temp, pos);
            pos += 1;

            BitConverter.GetBytes(ID).CopyTo(temp, pos);
            pos += 4;

            if (Parent != null)
            {
                BitConverter.GetBytes(Parent.ID).CopyTo(temp, pos);
                pos += 4;
            }

            // write total size of packet
            stream.Write(BitConverter.GetBytes(4 + pos));

            //write packet
            stream.Write(temp, 0, pos);

            long trackedObjects = 1;

            foreach (XNodeOut child in Nodes.Cast<XNodeOut>())
                trackedObjects += child.Write(stream, temp);

            return trackedObjects;
        }
    }

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

        internal int EntryPoint;
        internal int StillInside;

        internal bool RoomForLabel;
        internal RectangleF LabelRect;

        internal SharedDictionary<FunctionCall> CalledIn;
        internal SharedDictionary<FunctionCall> CallsOut;

        // call graph view
        internal int? Rank;
        internal List<XNodeIn> Adjacents;
        internal PointF ScaledLocation;
        internal float ScaledSize; // width and height


        internal static XNodeIn Read(FileStream stream)
        {
            // total size 4
            // name size 4
            // name x
            // type 4
            // lines 4
            // id 8
            // optional parent id 8

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

            if(stream.Position < startPos + totalSize)
                node.ParentID = BitConverter.ToInt32(stream.Read(4), 0);

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
    }
}