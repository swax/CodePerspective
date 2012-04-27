using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

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


    }
}
