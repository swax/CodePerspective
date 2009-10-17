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
    public enum XObjType { Root, Namespace, Class, Method, Field }

    public class InputValue
    {
        public string Name;
        public int Value;

        public InputValue() { }

        public InputValue(string name, int value)
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


        public string FullName()
        {
            if (Parent == null) // dont show name for root node
                return "";

            string parent = Parent.FullName();

            return (parent == "") ? Name : parent + "." + Name;
        }
    }

    public class XNodeOut : XNode
    {
        public static int NextID = 1;

        public bool Exclude;
        public int Lines;
        public int Indent;
        public string IndentString = "    "; // 2 for class, 2 for method

        public XNodeOut(XNodeOut parent, string name, XObjType objType)
        {
            Parent = parent;
            Name = name;
            ObjType = objType;

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
            // dont count anonymous methods/classes
            foreach (XNode node in Nodes.Where(n => n.Name.StartsWith("'")).ToArray())
                Nodes.Remove(node);

            int sum = Lines;

            foreach (XNodeOut node in Nodes)
                sum += node.ComputeSums();

            Value = sum;

            return sum;
        }

        public void SaveTree(string dir)
        {
            ComputeSums();

            string path = Path.Combine(dir, "XRay.dat");

            byte[] temp = new byte[1024];

            using (FileStream stream = new FileStream(path, FileMode.Create))
                Write(stream, temp);
        }

        public void Write(FileStream stream, byte[] temp)
        {
            if (Exclude)
                return;

            // total size 4
            // name size 4
            // name x
            // type 4
            // value 4
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

            BitConverter.GetBytes(Value).CopyTo(temp, pos);
            pos += 4;

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


            foreach (XNodeOut child in Nodes.Cast<XNodeOut>())
                child.Write(stream, temp);
        }
    }

    public class XNodeIn : XNode
    {
        internal int ParentID;
        internal int Lines; // save here so final value can be manipulated

        internal bool Selected;
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


        internal static XNodeIn Read(FileStream stream)
        {
            // total size 4
            // name size 4
            // name x
            // type 4
            // value 4
            // id 8
            // optional parent id 8

            XNodeIn node = new XNodeIn();

            long startPos = stream.Position;

            int totalSize = BitConverter.ToInt32(stream.Read(4), 0);
            int nameSize = BitConverter.ToInt32(stream.Read(4), 0);
            node.Name = UTF8Encoding.UTF8.GetString(stream.Read(nameSize));
            node.ObjType =(XObjType) BitConverter.ToInt32(stream.Read(4), 0);
            node.Value = BitConverter.ToInt32(stream.Read(4), 0);
            node.Lines = node.Value; 
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