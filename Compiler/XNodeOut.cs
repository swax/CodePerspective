using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Mono.Cecil;
using XLibrary;
using System.Runtime.Serialization;


namespace XBuilder
{
    public class XNodeOut : XNode
    {
        public static int NextID = 1;

        public bool Exclude;
        public int Lines;
        public int Indent;
        public string IndentString = "    "; // 2 for class, 2 for method
        public int InitCount = 1;
        public bool IsAnon;
        public int AnonFuncs = 1;
        public int AnonClasses = 1;

        public HashSet<int> ClassDependencies;


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

            byte[] temp = new byte[4096];

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
            // parent exist? 1
            //      parent id 4
            // dependencies? 1
            //      dependency count 4
            //      dependent ids 4x

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

            // parent 
            BitConverter.GetBytes(Parent != null).CopyTo(temp, pos);
            pos += 1;

            if (Parent != null)
            {
                BitConverter.GetBytes(Parent.ID).CopyTo(temp, pos);
                pos += 4;
            }

            // dependencies
            bool hasDependencies = (ClassDependencies != null && ClassDependencies.Count > 0);

            BitConverter.GetBytes(hasDependencies).CopyTo(temp, pos);
            pos += 1;

            if (hasDependencies)
            {
                BitConverter.GetBytes(ClassDependencies.Count).CopyTo(temp, pos);
                pos += 4;

                foreach (int dependentId in ClassDependencies)
                {
                    BitConverter.GetBytes(dependentId).CopyTo(temp, pos);
                    pos += 4;
                }
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

        internal XNodeOut AddField(FieldReference fieldDef)
        {
            // include namespace later for dynamic lookups - string name = fieldDef.FieldType.FullName + "::" + fieldDef.Name;
            string name = fieldDef.FieldType.Name + "  " + fieldDef.Name;

            var node = AddNode(name, XObjType.Field);

            node.Lines = 1;

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

        internal XNodeOut GetParentClass()
        {
            var parentClass = this;

            var up = Parent as XNodeOut;

            while (up != null)
            {
                if (up.ObjType == XObjType.Class)
                    parentClass = up;

                up = up.Parent as XNodeOut;
            }

            return parentClass;
        }
    }
}
