using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace XLibrary
{
    public partial class GdiPanel : UserControl
    {
        public MainForm MainWin;
        public ViewHost Host;
        public ViewModel Model;

        Bitmap DisplayBuffer;

        StringFormat LabelFormat = new StringFormat();

        public IColorProfile ColorProfile;

        SolidBrush[] ObjBrushes;
        SolidBrush[] ObjDitheredBrushes;
        Pen[] ObjPens;
        Pen[] ObjFocusedPens;

        public static Dictionary<int, Color> ObjColors = new Dictionary<int, Color>();

        SolidBrush NothingBrush;
        SolidBrush OutsideBrush;

        SolidBrush EntryBrush;
        SolidBrush MultiEntryBrush;

        SolidBrush HoldingBrush;
        SolidBrush MultiHoldingBrush;

        SolidBrush SearchMatchBrush;

        SolidBrush BorderBrush;

        Pen ShowCallPen;
        Pen CallOutPen;
        Pen CallInPen;
        Pen CallOutPenFocused;
        Pen CallInPenFocused;
        Pen HoldingCallPen;

        Pen CallDividerPen;

        SolidBrush TextBrush;
        SolidBrush TextBgBrush;
        SolidBrush LabelBgBrush;
        SolidBrush FooterBgBrush;

        Font InstanceFont = new Font("tahoma", 11, FontStyle.Bold);
        SolidBrush InstanceBrush;

        SolidBrush[] OverBrushes = new SolidBrush[7];

        SolidBrush[] HitBrush;
        SolidBrush[] MultiHitBrush;

        SolidBrush[] ConstructedBrush;
        SolidBrush[] DisposedBrush;

        SolidBrush[] FieldSetBrush;
        SolidBrush[] FieldGetBrush;

        SolidBrush[] ExceptionBrush;
        // no multi exception brush cause we dont know if multiple function calls resulted in an exception or just the 1

        Pen[] CallPen;
        Pen[] CallPenFocused;

        Pen FilteredPen;
        SolidBrush FilteredBrush;

        Pen IgnoredPen;
        SolidBrush IgnoredBrush;
       
        // dependencies
        SolidBrush DependentBrush;
        SolidBrush IndependentBrush;
        SolidBrush InterdependentBrush;


        public GdiPanel(ViewHost host, IColorProfile profile)
        {
            InitializeComponent();

            MouseWheel += new MouseEventHandler(TreePanelGdiPlus_MouseWheel);

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            Host = host;
            MainWin = host.Main;
            Model = host.Model;

            LabelFormat.Trimming = StringTrimming.EllipsisCharacter;
            LabelFormat.FormatFlags |= StringFormatFlags.NoWrap;

            HitBrush = new SolidBrush[XRay.HitFrames];
            MultiHitBrush = new SolidBrush[XRay.HitFrames];

            ExceptionBrush = new SolidBrush[XRay.HitFrames];
            FieldSetBrush = new SolidBrush[XRay.HitFrames];
            FieldGetBrush = new SolidBrush[XRay.HitFrames];
            ConstructedBrush = new SolidBrush[XRay.HitFrames];
            DisposedBrush = new SolidBrush[XRay.HitFrames];

            CallPen = new Pen[XRay.HitFrames];
            CallPenFocused = new Pen[XRay.HitFrames];

            SetColorProfile(profile);
        }

        public void SetColorProfile(IColorProfile profile)
        {
            ColorProfile = profile;

            NothingBrush = new SolidBrush(profile.EmptyColor);
            OutsideBrush = new SolidBrush(profile.OutsideColor);

            EntryBrush = new SolidBrush(profile.EntryColor);
            MultiEntryBrush = new SolidBrush(profile.MultiEntryColor);

            HoldingBrush = new SolidBrush(profile.HoldingColor);
            MultiHoldingBrush = new SolidBrush(profile.MultiHoldingColor);

            SearchMatchBrush = new SolidBrush(profile.SearchMatchColor);

            ShowCallPen = new Pen(profile.ShowCallColor);// { EndCap = LineCap.ArrowAnchor };
            CallOutPen = new Pen(profile.CallOutColor);
            CallInPen = new Pen(profile.CallInColor);
            CallOutPenFocused = new Pen(profile.CallOutColorFocused, 2);
            CallInPenFocused = new Pen(profile.CallInColorFocused, 2);
            HoldingCallPen = new Pen(profile.HoldingCallColor) { EndCap = LineCap.ArrowAnchor };

            BorderBrush = new SolidBrush(profile.BorderColor);
            CallDividerPen = new Pen(profile.CallDividerColor);

            TextBrush = new SolidBrush(profile.TextColor);
            TextBgBrush = new SolidBrush(profile.TextBgColor);
            LabelBgBrush = new SolidBrush(profile.LabelBgColor);
            FooterBgBrush = new SolidBrush(profile.FooterBgColor);

            InstanceBrush = new SolidBrush(profile.InstanceColor);

            FilteredPen = new Pen(profile.FilteredColor, 3);
            FilteredBrush = new SolidBrush(profile.FilteredColor);

            IgnoredPen = new Pen(profile.IgnoredColor, 3);
            IgnoredBrush = new SolidBrush(profile.IgnoredColor);

            DependentBrush = new SolidBrush(profile.DependentColor);
            IndependentBrush = new SolidBrush(profile.IndependentColor);
            InterdependentBrush = new SolidBrush(profile.InterdependentColor);


            // set colors of differnt brush / pen arrays
            ObjColors.Clear();
            ObjColors[(int)XObjType.Root] = profile.UnknownColor;
            ObjColors[(int)XObjType.External] = profile.UnknownColor;
            ObjColors[(int)XObjType.Internal] = profile.UnknownColor;
            ObjColors[(int)XObjType.File] = profile.FileColor;
            ObjColors[(int)XObjType.Namespace] = profile.NamespaceColor;
            ObjColors[(int)XObjType.Class] = profile.ClassColor;
            ObjColors[(int)XObjType.Field] = profile.FieldColor;
            ObjColors[(int)XObjType.Method] = profile.MethodColor;

            for (int i = 0; i < XRay.HitFrames; i++)
            {
                int brightness = 255 - (255 / XRay.HitFrames * i);

                HitBrush[i] = new SolidBrush(Color.FromArgb(255 - brightness, profile.HitColor));
                MultiHitBrush[i] = new SolidBrush(Color.FromArgb(255 - brightness, profile.MultiHitColor));
                ExceptionBrush[i] = new SolidBrush(Color.FromArgb(255 - brightness, profile.ExceptionColor));

                ConstructedBrush[i] = new SolidBrush(Color.FromArgb(255 - brightness, profile.ConstructedColor));
                DisposedBrush[i] = new SolidBrush(Color.FromArgb(255 - brightness, profile.DisposedColor));

                FieldSetBrush[i] = new SolidBrush(Color.FromArgb(255 - brightness, profile.FieldSetColor));
                FieldGetBrush[i] = new SolidBrush(Color.FromArgb(255 - brightness, profile.FieldGetColor));

                CallPen[i] = new Pen(Color.FromArgb(255 - brightness, profile.CallColor));
                CallPen[i].DashPattern = new float[] { FunctionCall.DashSize, FunctionCall.DashSpace };
                CallPen[i].EndCap = LineCap.ArrowAnchor;

                CallPenFocused[i] = new Pen(Color.FromArgb(255 - (brightness / 2), profile.CallColor), 2);
                CallPenFocused[i].DashPattern = new float[] { FunctionCall.DashSize, FunctionCall.DashSpace };
                CallPenFocused[i].EndCap = LineCap.ArrowAnchor;
            }

            for (int i = 0; i < OverBrushes.Length; i++)
            {
                int brightness = 128 / (OverBrushes.Length + 1) * (OverBrushes.Length - i);
                OverBrushes[i] = new SolidBrush(Color.FromArgb(128 + brightness, 128 + brightness, 255));
            }

            var objTypes = Enum.GetValues(typeof(XObjType));

            ObjBrushes = new SolidBrush[objTypes.Length];
            ObjDitheredBrushes = new SolidBrush[objTypes.Length];
            ObjPens = new Pen[objTypes.Length];
            ObjFocusedPens = new Pen[objTypes.Length];

            for (int i = 0; i < objTypes.Length; i++)
            {
                ObjBrushes[i] = new SolidBrush(ObjColors[i]);
                ObjDitheredBrushes[i] = new SolidBrush(Color.FromArgb(128, ObjColors[i]));
                ObjPens[i] = new Pen(ObjColors[i]);
                ObjFocusedPens[i] = new Pen(ObjColors[i], 3);
            }
        }

        private void TreePanel_Paint(object sender, PaintEventArgs e)
        {
            if (DisplayBuffer == null)
                DisplayBuffer = new Bitmap(Width, Height);

            if ((!Model.DoRedraw && !Model.DoRevalue && !Model.DoResize) || Model.CurrentRoot == null)
            {
                e.Graphics.DrawImage(DisplayBuffer, 0, 0);
                Model.FpsCount++;
                return;
            }

            // background
            Graphics buffer = Graphics.FromImage(DisplayBuffer);
            buffer.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed; // todo option to turn this off

            buffer.Clear(ColorProfile.BackgroundColor);

            Debug.Assert(Model.CurrentRoot != Model.TopRoot); // current root should be intenalRoot in this case

            // clear and pre-process marked depencies
            Model.RecalcDependencies();

            DoSearch();

            // draw layout
            Model.ScreenSize.Width = Width * ZoomFactor;
            Model.ScreenSize.Height = Height * ZoomFactor;
            Model.ScreenOffset.X = Model.PanOffset.X * Model.ScreenSize.Width;// +(Width * CenterFactor.X - ModelSize.Width * CenterFactor.X);
            Model.ScreenOffset.Y = Model.PanOffset.Y * Model.ScreenSize.Height;// +(Height * CenterFactor.Y - ModelSize.Height * CenterFactor.Y);

            if (Model.ViewLayout == LayoutType.TreeMap)
            {
                Model.DrawTreeMap(buffer);

                if (Model.ShowingOutside)
                {
                    buffer.FillRectangle(BorderBrush, Model.InternalRoot.AreaF.Width, 0, Model.PanelBorderWidth, Model.InternalRoot.AreaF.Height);
                    DrawNode(buffer, Model.InternalRoot, 0, true);
                }

                if (Model.ShowingExternal)
                {
                    buffer.FillRectangle(BorderBrush, Model.ExternalRoot.AreaF.X - Model.PanelBorderWidth, 0, Model.PanelBorderWidth, Model.ExternalRoot.AreaF.Height);
                    DrawNode(buffer, Model.ExternalRoot, 0, true);
                }

                DrawNode(buffer, Model.CurrentRoot, 0, true);
            }

            else if (Model.ViewLayout == LayoutType.CallGraph)
            {
                Model.DrawCallGraph(buffer);

                // id 0 nodes are intermediates
                foreach (var node in Model.Graphs.SelectMany(g => g.Nodes()).Where(n => n.ID != 0))
                    DrawNode(buffer, node, 0, false);
            }

            else if (Model.ViewLayout == LayoutType.Timeline)
            {
                Model.DrawTheadline(buffer);

                foreach (var node in Model.ThreadlineNodes)
                    DrawNode(buffer, node.Node, node.Area, node.LabelArea, 0, false, node.ShowHit);
            }

            // draw ignored over nodes ignored may contain
            foreach (var ignored in Model.IgnoredNodes.Values)
                if (Model.PositionMap.ContainsKey(ignored.ID))
                {
                    buffer.DrawLine(IgnoredPen, ignored.AreaF.UpperLeftCorner(), ignored.AreaF.LowerRightCorner() );
                    buffer.DrawLine(IgnoredPen, ignored.AreaF.UpperRightCorner(), ignored.AreaF.LowerLeftCorner());
                }

            // draw dividers for call graph
            if (Model.ViewLayout == LayoutType.CallGraph)
            {
                /*if (ShowRightOutsideArea)
                    buffer.DrawLine(CallDividerPen, RightDivider, 0, RightDivider, Height);

                if (ShowLeftOutsideArea)
                    buffer.DrawLine(CallDividerPen, LeftDivider, 0, LeftDivider, Height);*/
            }

            // draw dependency graph
            if (Model.ViewLayout == LayoutType.CallGraph && 
                (Model.GraphMode == CallGraphMode.Dependencies || 
                 Model.GraphMode == CallGraphMode.Init || 
                 Model.GraphMode == CallGraphMode.Intermediates))
            {
                foreach (var source in Model.PositionMap.Values)
                {
                    if (source.EdgesOut == null)
                        continue;

                    foreach (var to in source.EdgesOut)
                    {
                        if (!Model.PositionMap.ContainsKey(to))
                            continue;

                        var destination = Model.PositionMap[to];

                        bool focused = (source.Focused || destination.Focused);

                        if ((!Model.DrawCallGraphVertically && source.AreaF.X < destination.AreaF.X) ||
                            (Model.DrawCallGraphVertically && source.AreaF.Y < destination.AreaF.Y))
                            DrawGraphEdge(buffer, focused ? CallOutPenFocused : CallOutPen, source, destination);
                        else
                            DrawGraphEdge(buffer, focused ? CallInPenFocused : CallInPen, source, destination);
                    }
                }
            }

            // draw call graph
            if (XRay.FlowTracking && Model.ViewLayout != LayoutType.Timeline)
            {
                foreach (var source in Model.PositionMap.Values)
                {
                    if (source.XNode.CallsOut == null)
                        continue;

                    if (Model.ViewLayout == LayoutType.TreeMap && source.ObjType == XObjType.Class)
                        continue;

                    foreach (var call in source.XNode.CallsOut)
                    {
                        if (!Model.PositionMap.ContainsKey(call.Destination))
                            continue;

                        var destination = Model.PositionMap[call.Destination];

                        // if there are items we're filtering on then only show calls to those nodes
                        if (Model.FilteredNodes.Count > 0 && !IsNodeFiltered(true, source) && !IsNodeFiltered(true, destination))
                            continue;

                        // do after select filter so we can have ignored nodes inside selected, but not the otherway around
                        if (Model.IgnoredNodes.Count > 0 && IsNodeFiltered(false, source) || IsNodeFiltered(false, destination))
                            continue;

                        bool focused = (source.Focused || destination.Focused);
                        var callOutPen = focused ? CallOutPenFocused : CallOutPen;
                        var callInPen = focused ? CallInPenFocused : CallInPen;

                        if (call.StillInside > 0 && Model.ShowCalls)
                        {
                            if (Model.ViewLayout == LayoutType.TreeMap)
                                buffer.DrawLine(HoldingCallPen, source.CenterF, destination.CenterF);
                            else if (Model.ViewLayout == LayoutType.CallGraph)
                                DrawGraphEdge(buffer, HoldingCallPen, source, destination);
                        }

                        else if (XRay.ShowAllCalls && 
                                 Model.GraphMode != CallGraphMode.Intermediates && 
                                 Model.GraphMode != CallGraphMode.Init)
                        {
                            if (Model.ViewLayout == LayoutType.TreeMap)
                            {
                                PointF start = Model.PositionMap[call.Source].CenterF;
                                PointF end = Model.PositionMap[call.Destination].CenterF;
                                PointF mid = new PointF(start.X + (end.X - start.X) / 2, start.Y + (end.Y - start.Y) / 2);

                                buffer.DrawLine(callOutPen, start, mid);
                                buffer.DrawLine(callInPen, mid, end);
                            }
                            else if (Model.ViewLayout == LayoutType.CallGraph)
                            {
                                if ((!Model.DrawCallGraphVertically && source.AreaF.X < destination.AreaF.X) ||
                                    (Model.DrawCallGraphVertically && source.AreaF.Y < destination.AreaF.Y))
                                    DrawGraphEdge(buffer, callOutPen, source, destination);
                                else
                                    DrawGraphEdge(buffer, callInPen, source, destination);
                            }
                        }

                        if (call.Hit > 0 && Model.ShowCalls)
                        {
                            Pen pen = focused ? CallPenFocused[call.Hit] : CallPen[call.Hit];
                            pen.DashOffset = call.DashOffset;

                            if (Model.ViewLayout == LayoutType.TreeMap)
                                buffer.DrawLine(pen, source.CenterF, destination.CenterF);
                            else if (Model.ViewLayout == LayoutType.CallGraph)
                                DrawGraphEdge(buffer, pen, source, destination);
                        }
                    }
                }
            }

            // draw mouse over label
            DrawFooterLabel(buffer);
            DrawToolTip(buffer);

            // Copy buffer to display
            e.Graphics.DrawImage(DisplayBuffer, 0, 0);
            Model.FpsCount++;

            Model.DoRedraw = false;
            Model.RedrawCount++;
        }

        private void DoSearch()
        {
            // figure out if we need to do a search
            if (Model.SearchString == Model.LastSearch)
                return;

            Model.LastSearch = Model.SearchString;
            bool empty = string.IsNullOrEmpty(Model.SearchString);

            if (Model.SearchHighlightSubs)
            {
                // reset all
                Utilities.RecurseTree<NodeModel>(
                    tree: Model.TopRoot.Nodes,
                    evaluate: n => n.SearchMatch = false,
                    recurse: n => n.Nodes
                );

                // look for match
                Utilities.RecurseTree<NodeModel>(
                    tree: Model.TopRoot.Nodes,
                    evaluate: n =>
                    {
                        // set all nodes under match to also matched
                        n.SearchMatch = !empty && n.Name.ToLowerInvariant().IndexOf(Model.SearchString) != -1;
                        if (n.SearchMatch)
                            Utilities.RecurseTree<NodeModel>(
                                tree: n.Nodes,
                                evaluate: n2 => n2.SearchMatch = true,
                                recurse: n2 => n2.Nodes
                            );
                    },
                    recurse: n => n.Nodes.Where(n2 => !n2.SearchMatch)
                );
            }
            // only highlight specific node matches
            else
            {
                Utilities.RecurseTree<NodeModel>(
                    tree: Model.TopRoot.Nodes,
                    evaluate: n => n.SearchMatch = !empty && n.Name.ToLowerInvariant().IndexOf(Model.SearchString) != -1,
                    recurse: n => n.Nodes
                );
            }
            
        }

        private bool IsNodeFiltered(bool select, NodeModel node)
        {
            var map = select ? Model.FilteredNodes : Model.IgnoredNodes;

            foreach (var parent in node.GetParents())
                if (map.ContainsKey(parent.ID))
                    return true;

            return false;
        }

        private void DrawNode(Graphics buffer, NodeModel node, int depth, bool drawChildren)
        {
            DrawNode(buffer, node, node.AreaF, node.LabelRect, depth, drawChildren, true);
        }

        private void DrawNode(Graphics buffer, NodeModel node, RectangleF area, RectangleF labelArea, int depth, bool drawChildren, bool showHit)
        {
            if (!node.Show)
                return;

            var xNode = node.XNode;

            bool pointBorder = area.Width < 3.0f || area.Height < 3.0f;

            // use a circle for external/outside nodes in the call map
            bool ellipse = Model.ViewLayout == LayoutType.CallGraph && node.XNode.External;
            bool needBorder = true;

            Action<Brush> fillFunction = (b) =>
                {
                    if (ellipse)
                         buffer.FillEllipse(b, area);
                    else
                        buffer.FillRectangle(b, area);

                    needBorder = false;
                };

            // blue selection area
            if (node.Hovered)
            {
                if (depth > OverBrushes.Length - 1)
                    depth = OverBrushes.Length - 1;

                fillFunction(OverBrushes[depth]);
            }
            else if (Model.ViewLayout == LayoutType.TreeMap ||
                     Model.ViewLayout == LayoutType.Timeline || 
                     Model.CenterMap.ContainsKey(node.ID))
                fillFunction(NothingBrush);
            else
                fillFunction(OutsideBrush);

            if (showHit)
            {
                // check if function is an entry point or holding
                if (XRay.FlowTracking && xNode.StillInside > 0)
                {
                    if (xNode.EntryPoint > 0)
                    {
                        if (XRay.ThreadTracking && xNode.ConflictHit > 0)
                            fillFunction(MultiEntryBrush);
                        else
                            fillFunction(EntryBrush);
                    }
                    else
                    {
                        if (XRay.ThreadTracking && xNode.ConflictHit > 0)
                            fillFunction(MultiHoldingBrush);
                        else
                            fillFunction(HoldingBrush);
                    }
                }

                // not an else if, draw over holding or entry
                if (xNode.ExceptionHit > 0)
                    fillFunction(ExceptionBrush[xNode.ExceptionHit]);

                else if (xNode.FunctionHit > 0)
                {
                    if (XRay.ThreadTracking && xNode.ConflictHit > 0)
                        fillFunction(MultiHitBrush[xNode.FunctionHit]);

                    else if (node.ObjType == XObjType.Field)
                    {
                        if (xNode.LastFieldOp == FieldOp.Set)
                            fillFunction(FieldSetBrush[xNode.FunctionHit]);
                        else
                            fillFunction(FieldGetBrush[xNode.FunctionHit]);
                    }
                    else
                        fillFunction(HitBrush[xNode.FunctionHit]);
                }

                else if (xNode.ConstructedHit > 0)
                {
                    fillFunction(ConstructedBrush[xNode.ConstructedHit]);
                }

                else if (xNode.DisposeHit > 0)
                {
                    fillFunction(DisposedBrush[xNode.DisposeHit]);
                }
            }

            if (Model.FocusedNodes.Count > 0 && node.ObjType == XObjType.Class)
            {
                bool dependent = Model.DependentClasses.Contains(node.ID);
                bool independent = Model.IndependentClasses.Contains(node.ID);

                if (dependent && independent)
                    fillFunction(InterdependentBrush);

                else if (dependent)
                    fillFunction(DependentBrush);

                else if (independent)
                    fillFunction(IndependentBrush);
            }

            if (node.SearchMatch && !Model.SearchStrobe)
                fillFunction(SearchMatchBrush); 

            // if just a point, drawing a border messes up pixels
            if (pointBorder)
            {
                if (Model.FilteredNodes.ContainsKey(node.ID))
                    fillFunction(FilteredBrush);
                else if (Model.IgnoredNodes.ContainsKey(node.ID))
                    fillFunction(IgnoredBrush);

                else if (needBorder) // dont draw the point if already lit up
                    fillFunction(ObjBrushes[(int)node.ObjType]);
            }
            else
            {
                Pen pen = null;

                if (Model.FilteredNodes.ContainsKey(node.ID))
                    pen = FilteredPen;
                else if (Model.IgnoredNodes.ContainsKey(node.ID))
                    pen = IgnoredPen;

                else if (Model.FocusedNodes.Contains(node))
                    pen = ObjFocusedPens[(int)node.ObjType];
                else
                    pen = ObjPens[(int)node.ObjType];

                try
                {
                    if(ellipse)
                         buffer.DrawEllipse(pen, area.X, area.Y, area.Width, area.Height);
                    else
                        buffer.DrawRectangle(pen, area.X, area.Y, area.Width, area.Height);    
                }
                catch (Exception ex)
                {
                    File.WriteAllText("debug.txt", string.Format("{0}\r\n{1}\r\n{2}\r\n{3}\r\n{4}\r\n", ex, area.X, area.Y, area.Width, area.Height));
                }
            }

            // draw label
            //buffer.FillRectangle(SearchMatchBrush, node.DebugRect);
            if (Model.ShowLabels && node.RoomForLabel)
            {
                buffer.FillRectangle(LabelBgBrush, labelArea);
                buffer.DrawString(node.Name, Model.TextFont, ObjBrushes[(int)node.ObjType], labelArea, LabelFormat);
            }


            if (Model.MapMode == TreeMapMode.Dependencies && node.ObjType == XObjType.Class)
                drawChildren = false;

            if (drawChildren && area.Width > 1 && area.Height > 1)
                foreach (var sub in node.Nodes)
                    DrawNode(buffer, sub, depth + 1, drawChildren);


            // after drawing children, draw instance tracking on top of it all
            /*if (XRay.InstanceTracking && node.ObjType == XObjType.Class)
            {
               if (XRay.InstanceCount[node.ID] > 0)
                {
                    string count = XRay.InstanceCount[node.ID].ToString();
                    Rectangle x = new Rectangle(node.Area.Location, buffer.MeasureString(count, InstanceFont).ToSize());

                    if (node.Area.Contains(x))
                    {
                        buffer.FillRectangle(NothingBrush, x);
                        buffer.DrawString(count, InstanceFont, InstanceBrush, node.Area.Location.X + 2, node.Area.Location.Y + 2);
                    }
                }
            }*/
        }

        private void TreePanel_Resize(object sender, EventArgs e)
        {
            if (Width > 0 && Height > 0)
            {
                DisplayBuffer = new Bitmap(Width, Height);

                Model.DoResize = true;
                Invalidate();
            }
        }

        Point MouseDownPoint;
        PointF PanStart;
        float ZoomFactor = 1;

        void TreePanelGdiPlus_MouseWheel(object sender, MouseEventArgs e)
        {
            if (Model.ViewLayout == LayoutType.Timeline)
            {
                // move screen up/down 50%
                Model.PanOffset.Y += (float)e.Delta / 120.0f * 0.50f;

                if (Model.PanOffset.Y < 0)
                    Model.PanOffset.Y = 0;

                return;
            }

            // get fractional position in model
            var modelPos = new PointF();
            modelPos.X = (e.Location.X - Model.ScreenOffset.X) / Model.ScreenSize.Width;
            modelPos.Y = (e.Location.Y - Model.ScreenOffset.Y) / Model.ScreenSize.Height; 

            // get fractional position in window
            var winPos = new SizeF();
            winPos.Width = (float)e.Location.X / (float)Width;// -0.5f;
            winPos.Height = (float)e.Location.Y / (float)Height;// -0.5f;
            
            // change view point size and redraw
            float zoomAmount = (float)Math.Pow(1.3, e.Delta / 120.0);

            ZoomFactor *= zoomAmount;


            if (ZoomFactor < 1)
            {
                ZoomFactor = 1;
                Model.PanOffset = new PointF();
            }
            else
            {
                // we want to keep the zoom over the cursor, the modify the window offset by the zoom levl
                winPos.Width /= ZoomFactor;
                winPos.Height /= ZoomFactor;

                // subtract the window pos from our target pos in the model to find the amount that should be panned
                Model.PanOffset.X = winPos.Width - modelPos.X;
                Model.PanOffset.Y = winPos.Height - modelPos.Y;
            }

            Model.DoResize = true;
            Invalidate();
        }

        private void TreePanelGdiPlus_MouseDown(object sender, MouseEventArgs e)
        {
            MouseDownPoint = e.Location;
            PanStart = Model.PanOffset;
        }

        private void TreePanelGdiPlus_MouseUp(object sender, MouseEventArgs e)
        {
            if (MouseDownPoint == e.Location)
                Host.ManualMouseClick(e);

            MouseDownPoint = Point.Empty;
            PanStart = Point.Empty;
        }

        private void TreePanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseDownPoint != Point.Empty)
            {
                Model.PanOffset.X = PanStart.X + (e.Location.X - MouseDownPoint.X) / Model.ScreenSize.Width;
                Model.PanOffset.Y = PanStart.Y + (e.Location.Y - MouseDownPoint.Y) / Model.ScreenSize.Height;

                if (Model.ViewLayout == LayoutType.Timeline && Model.PanOffset.Y < 0)
                    Model.PanOffset.Y = 0;

                // to make faster resizing should keep things in scale value
                // so we just have to call redraw() which is faster and would apply perspective correction at draw time

                RecalcSizes(); 
            }
            else
                Host.RefreshHovered(e.Location);
        }

        public void Redraw()
        {
            Model.DoRedraw = true;
            Invalidate();
        }

        // re-calcs the sizes of all nodes
        public void RecalcSizes()
        {
            Model.DoResize = true;
            Invalidate();
        }

        // re-calcs the sizes of all nodes
        public void RecalcValues()
        {
            Model.DoRevalue = true;
            Invalidate();
        }

        private void TreePanelGdiPlus_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Host.View_MouseDoubleClick(e.Location);
        }

        void TreePanel_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            Host.View_KeyDown(e);
        }

        private void TreePanel_KeyUp(object sender, KeyEventArgs e)
        {
            Host.View_KeyUp(e);
        }

        public void ResetZoom()
        {
            Model.PanOffset = Point.Empty;
            ZoomFactor = 1;
        }

        private void TreePanel_MouseLeave(object sender, EventArgs e)
        {
            Host.View_MouseLeave();
        }

        public void DrawFooterLabel(Graphics buffer)
        {
            if (Host.NodesHovered.Length == 0)
                return;

            PointF pos = PointToClient(Cursor.Position);
            if (!ClientRectangle.Contains((int)pos.X, (int)pos.Y))
                return;

            float x = 0;
            var lastNode = Host.NodesHovered.Last();

            foreach (var node in Host.NodesHovered)
            {
                DrawFooterPart(buffer, node.Name, ObjBrushes[(int)node.ObjType], ref x);

                if(node != lastNode)
                    DrawFooterPart(buffer, ".", TextBrush, ref x);
            }

            DrawFooterPart(buffer, " (" + GetMetricForNode(lastNode) + ")", TextBrush, ref x);
        }

        public void DrawFooterPart(Graphics buffer, string label, SolidBrush brush, ref float x)
        {
            SizeF size = buffer.MeasureString(label, Model.TextFont);
            buffer.FillRectangle(FooterBgBrush, x, Height - size.Height, size.Width, size.Height);
            buffer.DrawString(label, Model.TextFont, brush, x, Height - size.Height);
            x += size.Width;
        }

        PointF LastHoverPoint = new PointF();
        DateTime LastHoverTime = DateTime.Now;

        public void DrawToolTip(Graphics buffer)
        {
            if (Host.NodesHovered.Length == 0)
                return;

            PointF pos = PointToClient(Cursor.Position);
            if (!ClientRectangle.Contains((int)pos.X, (int)pos.Y))
                return;

            if (!LastHoverPoint.Equals(pos))
            {
                LastHoverPoint = pos;
                LastHoverTime = DateTime.Now;
                return;
            }
            else if (DateTime.Now.Subtract(LastHoverTime).TotalSeconds < .5)
            {
                return;
            }

            // for each node selected, get size, figure out bg size and indents, then pass again and draw

            float bgWidth = 0;
            float bgHeight = 0;
            float lineHeight = 0;
            float indentAmount = 0;

            var labels = new List<Tuple<string, SolidBrush>>();
            string indentStr = "";

            // show all labels if showing just graph nodes, or show labels isnt on, or the label isnt displayed cause there's not enough room
            //foreach(var node in NodesHovered)//.Where(n => !ShowLabels || !n.RoomForLabel || n.LabelClipped))
            //{    
            var n = Host.NodesHovered.Last();
            SolidBrush color = Host.GuiHovered.Contains(n) ? ObjBrushes[(int)n.ObjType] : ObjDitheredBrushes[(int)n.ObjType];
            labels.Add(new Tuple<string, SolidBrush>(indentStr + n.Name, color));
            indentStr += " ";
            //}

            if (labels.Count == 0)
                return;

            var lastNode = Host.NodesHovered.Last();

            if (lastNode.ObjType == XObjType.Class)
            {
                int staticCount = 0;
                int instCount = 0;
                if (lastNode != null && lastNode.XNode.Record != null && lastNode.XNode.Record.Active.Count > 0)
                {
                    var record = lastNode.XNode.Record;

                    lock (record.Active)
                    {
                        for (int i = 0; i < record.Active.Count; i++)
                        {
                            var instance = record.Active[i];

                            if (instance.IsStatic)
                                staticCount++;
                            else
                                instCount++;
                        }
                    }
                }

                string classInfo = "0 instances";

                if (instCount > 0)
                    classInfo = instCount.ToString() + " instances";
                else if (staticCount > 0)
                    classInfo = "static instance";

                labels.Add(new Tuple<string, SolidBrush>(classInfo, TextBrush));
            }

            else if (lastNode.ObjType == XObjType.Method)
            {
                labels.Add(new Tuple<string, SolidBrush>(lastNode.XNode.GetMethodName(false), TextBrush));
            }

            else if (lastNode.ObjType == XObjType.Namespace)
            {
                labels.Clear();
                labels.Add(new Tuple<string, SolidBrush>(lastNode.XNode.FullName(true), ObjBrushes[(int)lastNode.ObjType]));
            }

            else if (lastNode.ObjType == XObjType.Field)
            {
                var classNode = lastNode.GetParentClass(false);

                if (classNode != null && classNode.XNode.Record != null && classNode.XNode.Record.Active.Count > 0)
                {
                    var record = classNode.XNode.Record;

                    lock (record.Active)
                    {
                        FieldInfo field = null;

                        for (int i = 0; i < record.Active.Count; i++)
                        {                        
                            var instance = record.Active[i];

                            field = instance.GetField(lastNode.XNode.UnformattedName);

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

                            labels.Add(new Tuple<string, SolidBrush>(text, TextBrush));

                            if (field.IsStatic)
                                break;
                        }
                    }
                }
            }

            // find the size of the background box
            foreach (var node in labels)
            {
                string label = node.Item1;
                //if (node == lastNode)
                //     label += " (" + GetMetricForNode(node) + ")";

                SizeF size = buffer.MeasureString(label, Model.TextFont);

                if (size.Width + indentAmount > bgWidth)
                    bgWidth = size.Width + indentAmount;

                bgHeight += size.Height;
                lineHeight = size.Height;
                //indentAmount += indent;
            }

            // put box lower right corner at cursor
            if (pos.X + bgWidth > Width)
                pos.X = Width - bgWidth;

            pos.Y -= bgHeight;

            // ensure it doesnt go off screen
            if (pos.X < 0) pos.X = 0;
            if (pos.Y < 0) pos.Y = 0;


            // draw background
            buffer.FillRectangle(TextBgBrush, pos.X, pos.Y, bgWidth, bgHeight);

            //GraphDebuggingLabel(buffer, pos);

            foreach (var node in labels)
            {
                buffer.DrawString(node.Item1, Model.TextFont, node.Item2, pos.X, pos.Y);

                pos.Y += lineHeight;
               // pos.X += indent;
            }
        }

        private void GraphDebuggingLabel(Graphics buffer, PointF pos)
        {
            /* useful for debugging call graph
            var debugNode = NodesHovered.Last();
            string debugString = "";

            float sum = 0;
            float count = 0;


            debugString += string.Format("My Loc {0}\r\n", debugNode.ScaledLocation.Y);

            if (debugNode.CallsOut != null)
                foreach (var call in debugNode.CallsOut)
                    if (PositionMap.ContainsKey(call.Destination))
                    {
                        if (call.Intermediates != null && call.Intermediates.Count > 0)
                        {
                            sum += call.Intermediates[0].ScaledLocation.Y;
                            debugString += string.Format("Out intermediate {0}\r\n", call.Intermediates[0].ScaledLocation.Y);
                        }
                        else
                        {
                            sum += PositionMap[call.Destination].ScaledLocation.Y;
                            debugString += string.Format("Out node {0}\r\n", PositionMap[call.Destination].ScaledLocation.Y);
                        }

                        count++;
                    }

            if (debugNode.CalledIn != null)
                foreach (var call in debugNode.CalledIn)
                    if (PositionMap.ContainsKey(call.Source))
                    {
                        if (call.Intermediates != null && call.Intermediates.Count > 0)
                        {
                            sum += call.Intermediates.Last().ScaledLocation.Y;
                            debugString += string.Format("In intermediate {0}\r\n", call.Intermediates.Last().ScaledLocation.Y);
                        }
                        else
                        {
                            sum += PositionMap[call.Source].ScaledLocation.Y;
                            debugString += string.Format("In node {0}\r\n", PositionMap[call.Source].ScaledLocation.Y);
                        }
                        count++;
                    }

            // should only be attached to intermediate nodes
            if (debugNode.Adjacents != null)
            {
                Debug.Assert(debugNode.ID == 0); // adjacents should only be on temp nodes

                foreach (var adj in debugNode.Adjacents)
                {
                    debugString += string.Format("Adj {0}\r\n", adj.ScaledLocation.Y);

                    sum += adj.ScaledLocation.Y;
                    count++;
                }
            }

            if (count != 0)
            {
                float result = sum / count;
                debugString += string.Format("My Avg {0}\r\n", result);
            }

            buffer.DrawString(debugString, TextFont, ObjBrushes[(int)debugNode.ObjType], pos.X, pos.Y);*/
        }

        public string GetMetricForNode(NodeModel node)
        {
            switch (Model.SizeLayout)
            {
                case SizeLayouts.Constant:
                    return node.Value.ToString() + " elements";

                case SizeLayouts.MethodSize:
                    return node.XNode.Lines.ToString() + " lines";

                case SizeLayouts.TimeInMethod:
                    return Utilities.TicksToString(node.Value);

                case SizeLayouts.Hits:
                    return node.Value.ToString() + " calls";

                case SizeLayouts.TimePerHit:
                    return Utilities.TicksToString(node.Value);
            }

            return "";
        }

        private void DrawGraphEdge(Graphics buffer, Pen pen, NodeModel source, NodeModel destination)
        {
            if (source.Intermediates == null || !source.Intermediates.ContainsKey(destination.ID))
                buffer.DrawLine(pen, source.CenterF, destination.CenterF);
            else
            {
                var intermediates = source.Intermediates[destination.ID];

                var originalCap = pen.EndCap;
                pen.EndCap = System.Drawing.Drawing2D.LineCap.NoAnchor;

                var prev = source;
                var last = intermediates.Last();

                foreach (var next in intermediates)
                {
                    if (next == last)
                        pen.EndCap = originalCap;

                    buffer.DrawLine(pen, prev.CenterF, next.CenterF);
                    prev = next;
                }
            }
        }
    }
}
