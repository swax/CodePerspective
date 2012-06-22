using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Reflection;

namespace XLibrary
{
    public class NodeModel
    {
        public ViewModel Model;
        public XNodeIn XNode;
        public NodeModel Parent;

        // copied
        public string Name;
        public int ID;
        public XObjType ObjType;
        public long Value;

        // ui
        internal bool Hovered;
        internal bool Show = true;

        internal RectangleF AreaF { get; private set; }
        internal PointF CenterF { get; private set; }

        internal bool RoomForLabel;
        internal bool LabelClipped;
        internal RectangleF LabelRect;

        internal List<NodeModel> Nodes = new List<NodeModel>();

        internal int[] EdgesIn;
        internal int[] EdgesOut;
        internal Dictionary<int, List<NodeModel>> Intermediates;

        public Dictionary<int, NodeModel> DependencyChainOut;
        public Dictionary<int, NodeModel> DependencyChainIn;

        public bool Focused;
        public bool SearchMatch;

        public long SecondaryValue; // used for showing 3d height

        // call graph view
        internal int? Rank;
        internal List<NodeModel> Adjacents;
        internal PointF ScaledLocation;
        internal float ScaledSize; // width and height
        internal float VelocityY;


        public NodeModel(ViewModel model, XNodeIn xNode)
        {
            Model = model;
            XNode = xNode;
            Name = XNode.Name;
            ID = XNode.ID;
            ObjType = XNode.ObjType;
        }

        public NodeModel(ViewModel model)
            : this(model, new XNodeIn())
        {
            // used to create intermediate nodes  
        }


        internal void SetArea(RectangleF area)
        {
            AreaF = area;
            CenterF = new PointF(area.X + area.Width / 2.0f,
                                 area.Y + area.Height / 2.0f);
        }

        internal NodeModel GetParentClass(bool rootClass)
        {
            var node = XNode.GetParentClass(rootClass);
            if (node == null)
                return null;
                    
            return Model.NodeModels[node.ID];
        }

        internal void AddIntermediateDependency(NodeModel sub)
        {
            if (DependencyChainOut == null)
                DependencyChainOut = new Dictionary<int, NodeModel>();
            DependencyChainOut[sub.ID] = sub;

            if (sub.DependencyChainIn == null)
                sub.DependencyChainIn = new Dictionary<int, NodeModel>();
            sub.DependencyChainIn[ID] = this;
        }

        internal NodeModel[] GetParents()
        {
            return GetParents(0, false);
        }

        private NodeModel[] GetParents(int count, bool includeRoot)
        {
            count++; // count is also the index position of this node from the back

            NodeModel[] result = null;

            if (Parent == null || (!includeRoot && Parent.ObjType == XObjType.Root))
                result = new NodeModel[count];
            else
                result = Parent.GetParents(count, includeRoot);

            result[result.Length - count] = this;

            return result;
        }

        internal string AppendClassName()
        {
            if (Parent != null && ObjType == XObjType.Method)
                return Parent.Name + "." + Name;
            else
                return Name;
        }

        internal IEnumerable<string> GetFieldValues()
        {
            var classNode = GetParentClass(false);

            if (classNode != null && classNode.XNode.Record != null && classNode.XNode.Record.Active.Count > 0)
            {
                var record = classNode.XNode.Record;

                lock (record.Active)
                {
                    FieldInfo field = null;

                    for (int i = 0; i < record.Active.Count; i++)
                    {
                        var instance = record.Active[i];

                        field = instance.GetField(XNode.UnformattedName);

                        object target = null;
                        if (instance != null && instance.Ref != null)
                            target = instance.Ref.Target;

                        // dont query the static class instance of the class for non-static fields
                        if (field == null || !field.IsStatic && target == null)
                            continue;

                        string text = "";
                        try
                        {
                            if (target == null)
                                text += "(static) ";
                            else
                                text += "#" + instance.Number + ": ";

                            object val = field.GetValue(target);

                            text += (val != null) ? val.ToString() : "<null>";
                        }
                        catch (Exception ex)
                        {
                            text = ex.Message;
                            //continue; 
                        }

                        yield return text;

                        if (field.IsStatic)
                            break;
                    }
                }
            }
        }



    }
}
