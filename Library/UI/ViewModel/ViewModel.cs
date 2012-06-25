using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace XLibrary
{
    public enum SizeLayouts { Constant, MethodSize, TimeInMethod, Hits, TimePerHit }
    public enum ShowNodes { All, Hit, Unhit, Instances }

    public enum LayoutType { TreeMap, CallGraph, Timeline }
    public enum TreeMapMode { Normal, Dependencies }
    public enum CallGraphMode { Method, Class, Dependencies, Intermediates, Init, Layers }


    public partial class ViewModel
    {
        public NodeModel[] NodeModels; // used so each XRay UI can keep separate views for each node

        public bool DoRedraw = true;
        public bool DoResize = true;
        public bool DoRevalue = true;

        public Dictionary<int, NodeModel> PositionMap = new Dictionary<int, NodeModel>();
        public HashSet<int> CenterMap = new HashSet<int>(); // used to filter calls into and out of center

        public NodeModel CurrentRoot;
        public NodeModel InternalRoot;
        public NodeModel ExternalRoot;
        public NodeModel TopRoot;

        public bool ShowOutside;
        public bool ShowExternal;

        public bool ShowFields = false;
        public bool ShowMethods = true;
        public bool ShowAnon = false;

        public LayoutType ViewLayout = LayoutType.TreeMap;
        public TreeMapMode MapMode = TreeMapMode.Normal;
        public SizeLayouts SizeLayout = SizeLayouts.MethodSize;
        public ShowNodes ShowLayout = ShowNodes.All;
        public bool DrawCallGraphVertically = false;
        public bool TwoDimensionalValues;

        public CallGraphMode GraphMode = CallGraphMode.Method;
        public bool ShowAllCalls = false;
        public bool ShowCalls = true;
        public bool SequenceOrder = false;
        public bool ShowLabels = true;
        public bool ShowCode = false;
        public HashSet<int> ShowThreads;

        public bool ShowAllDependencies = false;
        public Dictionary<int, NodeModel> InterDependencies = new Dictionary<int, NodeModel>();

        public List<NodeModel> FocusedNodes = new List<NodeModel>();

        public bool SearchStrobe;
        public bool SearchHighlightSubs;
        public long MaxSecondaryValue;

        public SizeF ScreenSize; // in screen dimensions, different from the view size which is constant
        public PointF ScreenOffset; // in screen dimensions
        public PointF PanOffset; // in model dimensions

        public HashSet<int> DependentClasses = new HashSet<int>();
        public HashSet<int> IndependentClasses = new HashSet<int>();

        public Font TextFont = new Font("tahoma", 8, FontStyle.Bold);

        public string SearchString;
        public string LastSearch;

        public HashSet<int> FilteredNodes = new HashSet<int>();
        public HashSet<int> IgnoredNodes = new HashSet<int>(); 

        // performance
        public int FpsCount;
        public int RevalueCount;
        public int ResizeCount;
        public int RedrawCount;

        public bool ShowingOutside { get { return ShowOutside && CurrentRoot != InternalRoot; } }
        public bool ShowingExternal { get { return ShowExternal && !CurrentRoot.XNode.External; } }


        public ViewModel(IMainUI mainUI, IColorProfile xColors)
        {
            MainUI = mainUI;
            XColors = xColors;

            NodeModels = new NodeModel[XRay.Nodes.Length];
            foreach (var node in XRay.Nodes)
                NodeModels[node.ID] = new NodeModel(this, XRay.Nodes[node.ID]);

            foreach (var uiNode in NodeModels)
            {
                if(uiNode.XNode.Parent != null)
                    uiNode.Parent = NodeModels[uiNode.XNode.Parent.ID];

                foreach (var subnode in uiNode.XNode.Nodes)
                    uiNode.Nodes.Add(NodeModels[subnode.ID]);
            }

            TopRoot =      NodeModels[XRay.RootNode.ID];
            InternalRoot = TopRoot.Nodes.First(n => n.ObjType == XObjType.Internal);
            ExternalRoot = TopRoot.Nodes.First(n => n.ObjType == XObjType.External);
            CurrentRoot =  InternalRoot;
        }

        public void RecalcCover(NodeModel root, bool rootShow=true)
        {
            root.Value = 0;
            root.SecondaryValue = 0;

            // if a leaf node - method or field
            if (root.ObjType == XObjType.Method || root.ObjType == XObjType.Field)
            {
                if (TwoDimensionalValues)
                {
                    root.Value = GetValueForLayout(root, SizeLayouts.Constant);
                    root.SecondaryValue = GetValueForLayout(root, SizeLayout);

                    if (root.SecondaryValue > MaxSecondaryValue)
                        MaxSecondaryValue = root.SecondaryValue;
                }
                else
                    root.Value = GetValueForLayout(root, SizeLayout);


                if ((ShowLayout == ShowNodes.Hit && !XRay.CoveredNodes[root.ID]) ||
                    (ShowLayout == ShowNodes.Unhit && XRay.CoveredNodes[root.ID]))
                    root.Value = 0;

                // processs subnodes because methods/fields can have anon sub classes
            }


            foreach (var node in root.Nodes)
            {
                bool nodeShow = rootShow;

                // show can only be set to false
                if (nodeShow)
                {
                    nodeShow =
                        ShowLayout == ShowNodes.All ||
                        ShowLayout == ShowNodes.Hit ||
                        ShowLayout == ShowNodes.Unhit ||
                        (ShowLayout == ShowNodes.Instances && (node.ObjType != XObjType.Class || (node.XNode.Record != null && node.XNode.Record.Created > 0)));

                    if ((node.ObjType == XObjType.Field && !ShowFields) ||
                        (node.ObjType == XObjType.Method && !ShowMethods) ||
                        (node.XNode.IsAnon && !ShowAnon))
                        nodeShow = false;

                    if (ShowThreads != null &&
                        (node.ObjType == XObjType.Method || node.ObjType == XObjType.Field) &&
                        (node.XNode.ThreadIDs == null || !node.XNode.ThreadIDs.Any(id => ShowThreads.Contains(id))))
                        nodeShow = false;
                }
 
                RecalcCover(node, nodeShow);

                if (nodeShow)
                {
                    root.Value += node.Value;
                    root.SecondaryValue += node.SecondaryValue;

                    if (node.Value == 0)
                    {
                        nodeShow = false;
                        Utilities.RecurseTree(node, n => n.Show = false, n => n.Nodes);
                    }
                }

                node.Show = nodeShow;
            }
        
            // when hide field/methods selected and user is viewing treemap - just show class map
            // make sure class graph still shows up with field/methods unselected
            // when show fields/methods is on we still want to hide certain classes that are filtered by thread for instance
            if (root.Value == 0 && !ShowFields && !ShowMethods &&
                (root.ObjType != XObjType.Method && root.ObjType != XObjType.Field) &&
                (ViewLayout == LayoutType.TreeMap ||
                 (ViewLayout == LayoutType.CallGraph && GraphMode != CallGraphMode.Method)))
            {
                root.Value = 1;
                // on return to calling function causes root.show = true
            }

            if (ViewLayout == LayoutType.CallGraph && SizeLayout == SizeLayouts.Constant)
                root.Value = 1;

            //XRay.LogError("Calc'd Node: {0}, Value: {1}", root.Name, root.Value);

            //Debug.Assert(root.Value >= 0);
        }

        private long GetValueForLayout(NodeModel root, SizeLayouts layout)
        {
            long value = 0;

            switch (layout)
            {
                case SizeLayouts.Constant:
                    value = 1;
                    break;
                case SizeLayouts.MethodSize:
                    value = root.XNode.Lines;
                    break;
                case SizeLayouts.TimeInMethod:
                    // why is this negetive?? HAVENT RETURNED YET, property should return 0 i think if  neg, or detect still inside and return that
                    if (root.XNode.CalledIn != null)
                        foreach (FunctionCall call in root.XNode.CalledIn)
                            value += call.TotalTimeInsideDest;
                    break;
                case SizeLayouts.Hits:
                    if (root.XNode.CalledIn != null)
                        foreach (FunctionCall call in root.XNode.CalledIn)
                            value += call.TotalHits;
                    break;
                case SizeLayouts.TimePerHit:
                    if (root.XNode.CalledIn != null)
                    {
                        int count = 0;

                        foreach (FunctionCall call in root.XNode.CalledIn)
                            if (call.TotalHits > 0)
                            {
                                count++;
                                value += call.TotalTimeInsideDest / call.TotalHits;
                            }

                        if (count > 0)
                            value /= count;
                    }

                    break;
            }

            return value;
        }

        public Dictionary<int, NodeModel> GetClassesFromFocusedNodes()
        {
            Dictionary<int, NodeModel> classes = new Dictionary<int, NodeModel>();

            foreach (var node in FocusedNodes)
            {
                if (node.ObjType == XObjType.Class ||
                    node.ObjType == XObjType.Method ||
                    node.ObjType == XObjType.Field)
                {
                    classes[node.ID] = node.GetParentClass(true);
                }
                else
                    Utilities.RecurseTree<NodeModel>(
                        node.Nodes,
                        evaluate: n =>
                        {
                            if (n.ObjType == XObjType.Class)
                                classes[n.ID] = n;
                        },
                        recurse: n =>
                        {
                            return (n.ObjType != XObjType.Class) ? n.Nodes : null;
                        }
                    );
            }

            return classes;
        }

        public void RecalcDependencies()
        {
            DependentClasses.Clear();
            IndependentClasses.Clear();

            if ((ViewLayout == LayoutType.TreeMap && MapMode == TreeMapMode.Dependencies) ||
                (ViewLayout == LayoutType.CallGraph && GraphMode == CallGraphMode.Dependencies))
            {

                if (FocusedNodes.Count == 0)
                    return;

                // find dependencies for selected classes
                var classes = GetClassesFromFocusedNodes();

                bool doRecurse = ShowAllDependencies;
                bool idFound = false;

                foreach (var dependenciesTo in classes.Values.Where(c => c.XNode.Dependencies != null)
                                                             .Select(c => c.XNode.Dependencies))
                {
                    Utilities.RecurseTree<int>(
                        dependenciesTo,
                        evaluate: id =>
                        {
                            idFound = DependentClasses.Contains(id);
                            DependentClasses.Add(id);
                        },
                        recurse: id =>
                        {
                            return (doRecurse && !idFound) ? XRay.Nodes[id].Dependencies : null;
                        }
                    );
                }

                // find all dependencies from
                foreach (var dependenciesFrom in classes.Values.Where(c => c.XNode.Independencies != null)
                                                               .Select(c => c.XNode.Independencies))
                {
                    Utilities.RecurseTree<int>(
                        dependenciesFrom,
                        evaluate: id =>
                        {
                            idFound = IndependentClasses.Contains(id);
                            IndependentClasses.Add(id);
                        },
                        recurse: id =>
                        {
                            return (doRecurse && !idFound) ? XRay.Nodes[id].Independencies : null;
                        }
                    );
                }
            }
        }
    }
}
