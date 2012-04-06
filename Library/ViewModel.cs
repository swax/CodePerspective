using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XLibrary
{
    public enum SizeLayouts { Constant, MethodSize, TimeInMethod, Hits, TimePerHit }
    public enum ShowNodes { All, Hit, Unhit, Instances }

    public enum LayoutType { TreeMap, CallGraph, ThreeD }
    public enum TreeMapMode { Normal, Dependencies }
    public enum CallGraphMode { Method, Class, Dependencies }
    public enum ShowDependenciesMode { None, All, Direct, Intermediates }


    public class ViewModel
    {
        internal XNodeIn CurrentRoot;
        internal XNodeIn InternalRoot;
        internal XNodeIn ExternalRoot;
        internal XNodeIn TopRoot;   

        public bool ShowOutside;
        public bool ShowExternal;

        public bool ShowFields = true;
        public bool ShowMethods = true;

        public LayoutType ViewLayout = LayoutType.TreeMap;
        public TreeMapMode MapMode = TreeMapMode.Normal;
        public SizeLayouts SizeLayout = SizeLayouts.MethodSize;
        public ShowNodes ShowLayout = ShowNodes.All;

        public CallGraphMode GraphMode = CallGraphMode.Method;
        public bool ShowCalls = true;
        public bool SequenceOrder = false;

        public ShowDependenciesMode DependenciesMode = ShowDependenciesMode.Direct;
        public Dictionary<int, XNodeIn> InterDependencies = new Dictionary<int, XNodeIn>();

        public List<XNodeIn> FocusedNodes = new List<XNodeIn>();

        public bool SearchStrobe;
        public long MaxSecondaryValue;
        public int FpsCounter;


        public long RecalcCover(XNodeIn root)
        {
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
                     (node.ObjType == XObjType.Method && !ShowMethods))
                {
                    node.Show = false;
                    continue;
                }

                node.Show = //node.ObjType != XObjType.Method ||
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
                    classes[node.ID] = node.GetParentClass() as XNodeIn;
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
    }
}
