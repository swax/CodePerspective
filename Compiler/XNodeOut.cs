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
            {
                trackedObjects += WriteDat(stream);
            }

            return trackedObjects;
        }

        public long WriteDat(FileStream stream)
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

            byte[] str = UTF8Encoding.UTF8.GetBytes(Name);
            stream.Write(BitConverter.GetBytes(str.Length));
            stream.Write(str);

            stream.Write(BitConverter.GetBytes((int)ObjType));

            stream.Write(BitConverter.GetBytes(Lines));

            stream.Write(BitConverter.GetBytes(External));

            stream.Write(BitConverter.GetBytes(IsAnon));

            stream.Write(BitConverter.GetBytes(ID));

            // parent 
            stream.Write(BitConverter.GetBytes(Parent != null));

            if (Parent != null)
                stream.Write(BitConverter.GetBytes(Parent.ID));

            // dependencies
            int dependencyCount = 0;
            if (ClassDependencies != null)
                dependencyCount = ClassDependencies.Count;

            stream.Write(BitConverter.GetBytes(dependencyCount));

            if (dependencyCount > 0)
                foreach (int dependentId in ClassDependencies)
                    stream.Write(BitConverter.GetBytes(dependentId));

            int codeLines = 0;
            if (Code != null)
                codeLines = Code.Count;

            stream.Write(BitConverter.GetBytes(codeLines));

            if (codeLines > 0)
                foreach (var line in Code)
                {
                    str = UTF8Encoding.UTF8.GetBytes(line);
                    stream.Write(BitConverter.GetBytes(str.Length));
                    stream.Write(str);
                }

            // write size of packet
            stream.Position = startPos;
            stream.Write(BitConverter.GetBytes((int)(stream.Length - startPos)));
            stream.Position = stream.Length;

            long trackedObjects = 1;

            foreach (XNodeOut child in Nodes.Cast<XNodeOut>())
                trackedObjects += child.WriteDat(stream);

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
    }
}
