using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace XLibrary
{
    public enum SizeLayouts { Constant, MethodSize, TimeInMethod, Hits, TimePerHit }
    public enum ShowNodes { All, Hit, Unhit, Instances }

    public enum LayoutType { TreeMap, CallGraph, ThreeD, Timeline }
    public enum TreeMapMode { Normal, Dependencies }
    public enum CallGraphMode { Method, Class, Dependencies, Intermediates, Init }


    public partial class ViewModel
    {
        public NodeUI[] NodeUIs; // used so each XRay UI can keep separate views for each node

        public bool DoRedraw = true;
        public bool DoResize = true;
        public bool DoRevalue = true;

        public Dictionary<int, XNodeIn> PositionMap = new Dictionary<int, XNodeIn>();
        public Dictionary<int, XNodeIn> CenterMap = new Dictionary<int, XNodeIn>(); // used to filter calls into and out of center
        public Dictionary<int, XNodeIn> OutsideMap = new Dictionary<int, XNodeIn>(); // used to filter calls into and out of center     

        public XNodeIn CurrentRoot;
        public XNodeIn InternalRoot;
        public XNodeIn ExternalRoot;
        public XNodeIn TopRoot;

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

        public CallGraphMode GraphMode = CallGraphMode.Method;
        public bool ShowCalls = true;
        public bool SequenceOrder = false;
        public bool ShowLabels = true;

        public bool ShowAllDependencies = false;
        public Dictionary<int, XNodeIn> InterDependencies = new Dictionary<int, XNodeIn>();

        public List<XNodeIn> FocusedNodes = new List<XNodeIn>();

        public bool SearchStrobe;
        public long MaxSecondaryValue;

        public SizeF ScreenSize; // in screen dimensions, different from the view size which is constant
        public PointF ScreenOffset; // in screen dimensions
        public PointF PanOffset; // in model dimensions

        public HashSet<int> DependentClasses = new HashSet<int>();
        public HashSet<int> IndependentClasses = new HashSet<int>();

        public Font TextFont = new Font("tahoma", 8, FontStyle.Bold);

        // performance
        public int FpsCount;
        public int RevalueCount;
        public int ResizeCount;
        public int RedrawCount;


        public long RecalcCover(XNodeIn root)
        {
            NodeUIs = new NodeUI[XRay.Nodes.Length];
            for(int i = 0; i < XRay.Nodes.Length; i++)
                NodeUIs[i] = new NodeUI(XRay.Nodes[i]);


            root.Value = 0;

            // only leaves have usable value
            if (root.ObjType == XObjType.Method || root.ObjType == XObjType.Field)
            {
                if (ViewLayout == LayoutType.ThreeD)
                {
                    root.Value = GetValueForLayout(root, SizeLayouts.Constant);
                    root.SecondaryValue = GetValueForLayout(root, SizeLayout);

                    if (root.SecondaryValue > MaxSecondaryValue)
                        MaxSecondaryValue = root.SecondaryValue;
                }
                else
                    root.Value = GetValueForLayout(root, SizeLayout);
            }

            foreach (XNodeIn node in root.Nodes)
            {
                if ((node.ObjType == XObjType.Field && !ShowFields) ||
                     (node.ObjType == XObjType.Method && !ShowMethods) ||
                     (node.IsAnon && !ShowAnon)
                   )
                {
                    node.Show = false;
                    continue;
                }

                node.Show =
                    ShowLayout == ShowNodes.All ||
                    (ShowLayout == ShowNodes.Hit && XRay.CoveredNodes[node.ID]) ||
                    (ShowLayout == ShowNodes.Unhit && !XRay.CoveredNodes[node.ID]) ||
                    (ShowLayout == ShowNodes.Instances && (node.ObjType != XObjType.Class || (node.Record != null && node.Record.Active.Count > 0)));

                if (node.Show)
                    root.Value += RecalcCover(node);
            }


            //XRay.LogError("Calc'd Node: {0}, Value: {1}", root.Name, root.Value);

            //Debug.Assert(root.Value >= 0);

            return root.Value;
        }

        private long GetValueForLayout(XNodeIn root, SizeLayouts layout)
        {
            long value = 0;

            switch (layout)
            {
                case SizeLayouts.Constant:
                    value = 1;
                    break;
                case SizeLayouts.MethodSize:
                    value = root.Lines;
                    break;
                case SizeLayouts.TimeInMethod:
                    // why is this negetive?? HAVENT RETURNED YET, property should return 0 i think if  neg, or detect still inside and return that
                    if (root.CalledIn != null)
                        foreach (FunctionCall call in root.CalledIn)
                            value += call.TotalTimeInsideDest;
                    break;
                case SizeLayouts.Hits:
                    if (root.CalledIn != null)
                        foreach (FunctionCall call in root.CalledIn)
                            value += call.TotalHits;
                    break;
                case SizeLayouts.TimePerHit:
                    if (root.CalledIn != null)
                    {
                        int count = 0;

                        foreach (FunctionCall call in root.CalledIn)
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

        public Dictionary<int, XNodeIn> GetClassesFromFocusedNodes()
        {
            Dictionary<int, XNodeIn> classes = new Dictionary<int, XNodeIn>();

            foreach (var node in FocusedNodes)
            {
                if (node.ObjType == XObjType.Class ||
                    node.ObjType == XObjType.Method ||
                    node.ObjType == XObjType.Field)
                {
                    classes[node.ID] = node.GetParentClass(true) as XNodeIn;
                }
                else
                    Utilities.RecurseTree<XNodeIn>(
                        node.Nodes.Cast<XNodeIn>(),
                        evaluate: n =>
                        {
                            if (n.ObjType == XObjType.Class)
                                classes[n.ID] = n;
                        },
                        recurse: n =>
                        {
                            return (n.ObjType != XObjType.Class) ? n.Nodes.Cast<XNodeIn>() : null;
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
                Dictionary<int, XNodeIn> classes = GetClassesFromFocusedNodes();

                bool doRecurse = ShowAllDependencies;
                bool idFound = false;

                foreach (var dependenciesTo in classes.Values.Where(c => c.Dependencies != null)
                                                             .Select(c => c.Dependencies))
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
                foreach (var dependenciesFrom in classes.Values.Where(c => c.Independencies != null)
                                            .Select(c => c.Independencies))
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
