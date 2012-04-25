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
    public partial class TreePanelGdiPlus : UserControl
    {
        public MainForm MainForm;
        public ViewModel Model;

        Bitmap DisplayBuffer;

        bool ShowingOutside;
        bool ShowingExternal;

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

        Color CallColor;
        Pen ShowCallPen;
        Pen CallOutPen;
        Pen CallInPen;
        Pen CallOutPenFocused;
        Pen CallInPenFocused;
        Pen HoldingCallPen;

        Pen CallDividerPen;

        Color HitColor;
        Color MultiHitColor;

        Color ExceptionColor;
        Color MultiExceptionColor;

        Color FieldSetColor;
        Color FieldGetColor;

        SolidBrush TextBrush;
        SolidBrush TextBgBrush;
        SolidBrush LabelBgBrush;
        SolidBrush FooterBgBrush;

        Font InstanceFont = new Font("tahoma", 11, FontStyle.Bold);
        SolidBrush InstanceBrush;

        SolidBrush[] OverBrushes = new SolidBrush[7];

        SolidBrush[] HitBrush;
        SolidBrush[] MultiHitBrush;

        SolidBrush[] FieldSetBrush;
        SolidBrush[] FieldGetBrush;

        SolidBrush[] ExceptionBrush;
        // no multi exception brush cause we dont know if multiple function calls resulted in an exception or just the 1

        Pen[] CallPen;
        Pen[] CallPenFocused;

        int HoverHash;
        List<XNodeIn> GuiHovered = new List<XNodeIn>();
        XNodeIn[] NodesHovered = new XNodeIn[] { };

        Pen FilteredPen;
        SolidBrush FilteredBrush;
        Dictionary<int, XNodeIn> FilteredNodes = new Dictionary<int, XNodeIn>();

        Pen IgnoredPen;
        SolidBrush IgnoredBrush;
        Dictionary<int, XNodeIn> IgnoredNodes = new Dictionary<int, XNodeIn>();

        // dependencies
        SolidBrush DependentBrush;
        SolidBrush IndependentBrush;
        SolidBrush InterdependentBrush;

        public string SearchString;
        public string LastSearch;


        public TreePanelGdiPlus(MainForm main, IColorProfile profile)
        {
            InitializeComponent();

            MouseWheel += new MouseEventHandler(TreePanelGdiPlus_MouseWheel);

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            MainForm = main;
            Model = main.Model;

            Model.TopRoot = XRay.RootNode;
            Model.InternalRoot = Model.TopRoot.Nodes.First(n => n.ObjType == XObjType.Internal) as XNodeIn;
            Model.ExternalRoot = Model.TopRoot.Nodes.First(n => n.ObjType == XObjType.External) as XNodeIn;
            Model.CurrentRoot = Model.InternalRoot;

            LabelFormat.Trimming = StringTrimming.EllipsisCharacter;

            HitBrush = new SolidBrush[XRay.HitFrames];
            MultiHitBrush = new SolidBrush[XRay.HitFrames];
            ExceptionBrush = new SolidBrush[XRay.HitFrames];
            FieldSetBrush = new SolidBrush[XRay.HitFrames];
            FieldGetBrush = new SolidBrush[XRay.HitFrames];

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

            CallColor = profile.CallColor;
            ShowCallPen = new Pen(profile.ShowCallColor);// { EndCap = LineCap.ArrowAnchor };
            CallOutPen = new Pen(profile.CallOutColor);
            CallInPen = new Pen(profile.CallInColor);
            CallOutPenFocused = new Pen(profile.CallOutColorFocused, 2);
            CallInPenFocused = new Pen(profile.CallInColorFocused, 2);
            HoldingCallPen = new Pen(profile.HoldingCallColor) { EndCap = LineCap.ArrowAnchor };

            BorderBrush = new SolidBrush(profile.BorderColor);
            CallDividerPen = new Pen(profile.CallDividerColor);

            HitColor = profile.HitColor;
            MultiHitColor = profile.MultiHitColor;

            ExceptionColor = profile.ExceptionColor;
            MultiExceptionColor = profile.MultiExceptionColor;

            FieldSetColor = profile.FieldSetColor;
            FieldGetColor = profile.FieldGetColor;

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

                HitBrush[i] = new SolidBrush(Color.FromArgb(255 - brightness, HitColor));
                MultiHitBrush[i] = new SolidBrush(Color.FromArgb(255 - brightness, MultiHitColor));
                ExceptionBrush[i] = new SolidBrush(Color.FromArgb(255 - brightness, ExceptionColor));
                FieldSetBrush[i] = new SolidBrush(Color.FromArgb(255 - brightness, FieldSetColor));
                FieldGetBrush[i] = new SolidBrush(Color.FromArgb(255 - brightness, FieldGetColor));

                CallPen[i] = new Pen(Color.FromArgb(255 - brightness, CallColor));
                CallPen[i].DashPattern = new float[] { FunctionCall.DashSize, FunctionCall.DashSpace };
                CallPen[i].EndCap = LineCap.ArrowAnchor;

                CallPenFocused[i] = new Pen(Color.FromArgb(255 - (brightness / 2), CallColor), 2);
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

            ShowingOutside = Model.ShowOutside && Model.CurrentRoot != Model.InternalRoot;
            ShowingExternal = Model.ShowExternal && !Model.CurrentRoot.External;

            // clear and pre-process marked depencies
            Model.RecalcDependencies();

            // figure out if we need to do a search
            if (SearchString != LastSearch)
            {
                LastSearch = SearchString;
                bool empty = string.IsNullOrEmpty(SearchString);

                Utilities.RecurseTree<XNodeIn>(
                        tree: XRay.RootNode.Nodes.Cast<XNodeIn>(),
                        evaluate: n =>
                        {
                            n.SearchMatch = !empty && n.Name.ToLowerInvariant().IndexOf(SearchString) != -1;
                        },
                        recurse: n =>
                        {
                            return n.Nodes.Cast<XNodeIn>();
                        }
                    );
            }

            // draw layout
            Model.Size.Width = Width * ZoomFactor;
            Model.Size.Height = Height * ZoomFactor;
            Model.Offset.X = Model.PanOffset.X * Model.Size.Width;// +(Width * CenterFactor.X - ModelSize.Width * CenterFactor.X);
            Model.Offset.Y = Model.PanOffset.Y * Model.Size.Height;// +(Height * CenterFactor.Y - ModelSize.Height * CenterFactor.Y);

            if (Model.ViewLayout == LayoutType.TreeMap)
            {
                Model.DrawTreeMap(buffer, ShowingOutside, ShowingExternal);

                if (ShowingOutside)
                {
                    buffer.FillRectangle(BorderBrush, Model.InternalRoot.AreaF.Width, 0, Model.PanelBorderWidth, Model.InternalRoot.AreaF.Height);
                    DrawNode(buffer, Model.InternalRoot, 0, true);
                }

                if (ShowingExternal)
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

            // draw ignored over nodes ignored may contain
            foreach (XNodeIn ignored in IgnoredNodes.Values)
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

                        XNodeIn destination = Model.PositionMap[to];

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
            if (XRay.FlowTracking)
            {
                foreach (var source in Model.PositionMap.Values)
                {
                    if (source.CallsOut == null)
                        continue;

                    if (Model.ViewLayout == LayoutType.TreeMap && source.ObjType == XObjType.Class)
                        continue;

                    foreach (var call in source.CallsOut)
                    {
                        if (!Model.PositionMap.ContainsKey(call.Destination))
                            continue;

                        XNodeIn destination = Model.PositionMap[call.Destination];

                        // if there are items we're filtering on then only show calls to those nodes
                        if (FilteredNodes.Count > 0 && !IsNodeFiltered(true, source) && !IsNodeFiltered(true, destination))
                            continue;

                        // do after select filter so we can have ignored nodes inside selected, but not the otherway around
                        if (IgnoredNodes.Count > 0 && IsNodeFiltered(false, source) || IsNodeFiltered(false, destination))
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

        private bool IsNodeFiltered(bool select, XNodeIn node)
        {
            Dictionary<int, XNodeIn> map = select ? FilteredNodes : IgnoredNodes;

            foreach (XNodeIn parent in node.GetParents())
                if (map.ContainsKey(parent.ID))
                    return true;

            return false;
        }

        private void DrawNode(Graphics buffer, XNodeIn node, int depth, bool drawChildren)
        {
            if (!node.Show)
                return;

            bool pointBorder = node.AreaF.Width < 3.0f || node.AreaF.Height < 3.0f;

            // use a circle for external/outside nodes in the call map
            bool ellipse = Model.ViewLayout == LayoutType.CallGraph && node.External;
            bool needBorder = true;

            Action<Brush> fillFunction = (b) =>
                {
                    if (ellipse)
                         buffer.FillEllipse(b, node.AreaF);
                    else
                        buffer.FillRectangle(b, node.AreaF);

                    needBorder = false;
                };

            // blue selection area
            if (node.Hovered)
            {
                if (depth > OverBrushes.Length - 1)
                    depth = OverBrushes.Length - 1;

                fillFunction(OverBrushes[depth]);
            }
            else if (Model.ViewLayout == LayoutType.TreeMap || Model.CenterMap.ContainsKey(node.ID))
                fillFunction(NothingBrush);
            else
                fillFunction(OutsideBrush);

            // check if function is an entry point or holding
            if (XRay.FlowTracking && node.StillInside > 0)
            {
                if (node.EntryPoint > 0)
                {
                    if (XRay.ThreadTracking && node.ConflictHit > 0)
                        fillFunction(MultiEntryBrush);
                    else
                        fillFunction(EntryBrush);
                }
                else
                {
                    if (XRay.ThreadTracking && node.ConflictHit > 0)
                        fillFunction(MultiHoldingBrush);
                    else
                        fillFunction(HoldingBrush);
                }
            }

            // not an else if, draw over holding or entry
            if (node.ExceptionHit > 0)
                fillFunction(ExceptionBrush[node.FunctionHit]);

            else if (node.FunctionHit > 0)
            {
                if (XRay.ThreadTracking && node.ConflictHit > 0)
                    fillFunction(MultiHitBrush[node.FunctionHit]);

                else if (node.ObjType == XObjType.Field)
                {
                    if (node.LastFieldOp == FieldOp.Set)
                        fillFunction(FieldSetBrush[node.FunctionHit]);
                    else
                        fillFunction(FieldGetBrush[node.FunctionHit]);
                }
                else
                    fillFunction(HitBrush[node.FunctionHit]);
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
                if (FilteredNodes.ContainsKey(node.ID))
                    fillFunction(FilteredBrush);
                else if (IgnoredNodes.ContainsKey(node.ID))
                    fillFunction(IgnoredBrush);

                else if (needBorder) // dont draw the point if already lit up
                    fillFunction(ObjBrushes[(int)node.ObjType]);
            }
            else
            {
                Pen pen = null;

                if (FilteredNodes.ContainsKey(node.ID))
                    pen = FilteredPen;
                else if (IgnoredNodes.ContainsKey(node.ID))
                    pen = IgnoredPen;

                else if (Model.FocusedNodes.Contains(node))
                    pen = ObjFocusedPens[(int)node.ObjType];
                else
                    pen = ObjPens[(int)node.ObjType];

                try
                {
                    if(ellipse)
                         buffer.DrawEllipse(pen, node.AreaF.X, node.AreaF.Y, node.AreaF.Width, node.AreaF.Height);
                    else
                        buffer.DrawRectangle(pen, node.AreaF.X, node.AreaF.Y, node.AreaF.Width, node.AreaF.Height);    
                }
                catch (Exception ex)
                {
                    File.WriteAllText("debug.txt", string.Format("{0}\r\n{1}\r\n{2}\r\n{3}\r\n{4}\r\n", ex, node.AreaF.X, node.AreaF.Y, node.AreaF.Width, node.AreaF.Height));
                }
            }

            // draw label
            //buffer.FillRectangle(SearchMatchBrush, node.DebugRect);
            if (Model.ShowLabels && node.RoomForLabel)
            {
                buffer.FillRectangle(LabelBgBrush, node.LabelRect);
                buffer.DrawString(node.Name, Model.TextFont, ObjBrushes[(int)node.ObjType], node.LabelRect, LabelFormat);
            }


            if (Model.MapMode == TreeMapMode.Dependencies && node.ObjType == XObjType.Class)
                drawChildren = false;

            if (drawChildren && node.AreaF.Width > 1 && node.AreaF.Height > 1)
                foreach (XNodeIn sub in node.Nodes)
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
            // get fractional position in model
            var modelPos = new PointF();
            modelPos.X = (e.Location.X - Model.Offset.X) / Model.Size.Width;
            modelPos.Y = (e.Location.Y - Model.Offset.Y) / Model.Size.Height; 

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
                ManualMouseClick(e);

            MouseDownPoint = Point.Empty;
            PanStart = Point.Empty;
        }

        private void TreePanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseDownPoint != Point.Empty)
            {
                Model.PanOffset.X = PanStart.X + (e.Location.X - MouseDownPoint.X) / Model.Size.Width;
                Model.PanOffset.Y = PanStart.Y + (e.Location.Y - MouseDownPoint.Y) / Model.Size.Height;

                // to make faster resizing should keep things in scale value
                // so we just have to call redraw() which is faster and would apply perspective correction at draw time

                RecalcSizes(); 
            }
            else
                RefreshHovered(e.Location);
        }

        private void RefreshHovered(Point loc)
        {
            ClearHovered();

            if (Model.ViewLayout == LayoutType.TreeMap)
            {
                if (ShowingOutside)
                    TestHovered(Model.InternalRoot, loc);
                if (ShowingExternal)
                    TestHovered(Model.ExternalRoot, loc);

                TestHovered(Model.CurrentRoot, loc);
            }
            else if (Model.ViewLayout == LayoutType.CallGraph)
            {
                foreach (var node in Model.PositionMap.Values)
                    if (node.AreaF.Contains(loc.X, loc.Y))
                    {
                        GuiHovered.Add(node);
                        XNodeIn parent = node.Parent as XNodeIn;

                        while (parent != null)
                        {
                            GuiHovered.Add(parent);
                            parent = parent.Parent as XNodeIn;
                        }

                        GuiHovered.Reverse();
                        break;
                    }
            }

            int hash = 0;
            GuiHovered.ForEach(n => hash = n.ID ^ hash);

            // only continuously redraw label if hover is different than before
            if (hash != HoverHash)
            {
                HoverHash = hash;

                if (GuiHovered.Count > 0)
                {
                    NodesHovered = GuiHovered.Last().GetParents().Cast<XNodeIn>().ToArray();
                    //MainForm.SelectedLabel.Text = GuiHovered.Last().FullName();
                }
                else
                {
                    NodesHovered = new XNodeIn[] { };
                    //MainForm.SelectedLabel.Text = "";
                }

                Redraw();
            }
        }

        private void TestHovered(XNodeIn node, Point loc)
        {
            if (!node.Show || !node.AreaF.Contains(loc.X, loc.Y))
                return;

            node.Hovered = true;
            GuiHovered.Add(node);

            foreach (XNodeIn sub in node.Nodes)
                TestHovered(sub, loc);
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

        void ToggleNode(Dictionary<int, XNodeIn> map, XNodeIn node)
        {
            // make sure a node cant be selected and ignored simultaneously
            if (map != IgnoredNodes && IgnoredNodes.ContainsKey(node.ID))
                IgnoredNodes.Remove(node.ID);

            if (map != FilteredNodes && FilteredNodes.ContainsKey(node.ID))
                FilteredNodes.Remove(node.ID);

            // toggle the setting of the node in the map
            if (map.ContainsKey(node.ID))
                map.Remove(node.ID);
            else
                map[node.ID] = node;

            Redraw();
        }

        private void TreePanelGdiPlus_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            XNodeIn node = GuiHovered.LastOrDefault();
            if (node != null)
                SetRoot(node);
        }

        bool CtrlDown;

        void TreePanel_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
                CtrlDown = true;

            /*if (e.KeyCode == Keys.Right)
                Zoom(true);
            else if (e.KeyCode == Keys.Right)
                Zoom(false);
            else if (e.KeyCode == Keys.Space)
                SetRoot(XRay.RootNode);*/
        }

        private void TreePanel_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
                CtrlDown = false;
        }


        private void TreePanel_MouseClick(object sender, MouseEventArgs e)
        {
            //ManualMouseClick();
        }

        private void ManualMouseClick(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (!CtrlDown)
                {
                    Model.FocusedNodes.ForEach(n => n.Focused = false);
                    Model.FocusedNodes.Clear();
                }

                var node = GuiHovered.LastOrDefault();

                if (node == null)
                    return;

                else if (node.Focused && CtrlDown)
                {
                    node.Focused = false;

                    Model.FocusedNodes.Remove(node);
                }

                else
                {
                    node.Focused = true;

                    Model.FocusedNodes.Add(node);

                    MainForm.InstanceTab.NavigateTo(node);
                    MainForm.CodeTab.NavigateTo(node);
                    MainForm.TimingPanel.NavigateTo(node);
                }

                Redraw();
            }
            else if (e.Button == MouseButtons.Right)
            {
                ContextMenu menu = new ContextMenu();


                XNodeIn node = GuiHovered.LastOrDefault();
                if (node != null)
                {
                    menu.MenuItems.Add(node.ObjType.ToString() + " " + node.Name);
                    menu.MenuItems.Add("-");

                    bool selected = FilteredNodes.ContainsKey(node.ID);
                    bool ignored = IgnoredNodes.ContainsKey(node.ID);

                    menu.MenuItems.Add(new MenuItem("Zoom", (s, a) => SetRoot(node)));

                    menu.MenuItems.Add(new MenuItem("Filter", (s, a) => ToggleNode(FilteredNodes, node)) { Checked = selected });

                    menu.MenuItems.Add(new MenuItem("Ignore", (s, a) => ToggleNode(IgnoredNodes, node)) { Checked = ignored });
                }

                if (FilteredNodes.Count > 0 || IgnoredNodes.Count > 0)
                {
                    menu.MenuItems.Add("-");

                    menu.MenuItems.Add(new MenuItem("Clear Filtering", (s, a) =>
                    {
                        FilteredNodes.Clear();
                        IgnoredNodes.Clear();
                        Redraw();
                    }));
                }

                menu.Show(this, e.Location);
            }

            // back button zoom out
            else if (e.Button == MouseButtons.XButton1)
                NavBack();

            // forward button zoom in
            else if (e.Button == MouseButtons.XButton2)
                NavForward();
        }

        private void Zoom(bool inward)
        {
            if (inward)
            {
                if (GuiHovered.Count < 2)
                    return;

                if (GuiHovered[1].Nodes.Count > 0)
                    SetRoot(GuiHovered[1]);
            }
            else
            {
                if (Model.CurrentRoot.Parent == null)
                    return;

                SetRoot(Model.CurrentRoot.Parent as XNodeIn);
            }

            // put cursor in the same block after the view changes
            XNodeIn last = NodesHovered.LastOrDefault() as XNodeIn;

            if(last != null)
                Cursor.Position = PointToScreen(new Point((int)last.CenterF.X, (int)last.CenterF.Y));
        }

        public void ResetZoom()
        {
            Model.PanOffset = Point.Empty;
            ZoomFactor = 1;
        }

        public LinkedList<XNodeIn> HistoryList = new LinkedList<XNodeIn>();
        public LinkedListNode<XNodeIn> CurrentHistory;

        public void SetRoot(XNodeIn node, bool logHistory=true)
        {
            // setting internal root will auto show properly sized external root area if showing it is enabled
            ResetZoom();
            Model.CurrentRoot = (node == Model.TopRoot) ? Model.InternalRoot : node;
           
            if (logHistory)
            {
                // re-write forward log with new node
                while(CurrentHistory != HistoryList.Last)
                    HistoryList.RemoveLast();

                // dont set node if last node is already this
                var last = HistoryList.LastOrDefault();
                if (Model.CurrentRoot != last)
                {
                    HistoryList.AddLast(Model.CurrentRoot);
                    CurrentHistory = HistoryList.Last;
                }
            }

            MainForm.UpdateBreadCrumbs();

            Model.DoRevalue = true;
            Refresh();
        }

        public void NavBack()
        {
            if (CurrentHistory != null && CurrentHistory.Previous != null)
            {
                // update current history before calling so breadcrumbs are updated properly
                var prev = CurrentHistory.Previous;

                CurrentHistory = CurrentHistory.Previous;

                SetRoot(prev.Value, false);
            }
        }

        public void NavForward()
        {
            if (CurrentHistory != null && CurrentHistory.Next != null)
            {
                // update current history before calling so breadcrumbs are updated properly
                var next = CurrentHistory.Next;

                CurrentHistory = CurrentHistory.Next;

                SetRoot(next.Value, false);
            }
        }

        private void TreePanel_MouseLeave(object sender, EventArgs e)
        {
            ClearHovered();

            Redraw();
        }

        private void ClearHovered()
        {
            GuiHovered.ForEach(n => n.Hovered = false);
            GuiHovered.Clear(); 
        }

        public void DrawFooterLabel(Graphics buffer)
        {
            if (NodesHovered.Length == 0)
                return;

            PointF pos = PointToClient(Cursor.Position);
            if (!ClientRectangle.Contains((int)pos.X, (int)pos.Y))
                return;

            float x = 0;
            var lastNode = NodesHovered.Last();

            foreach (XNodeIn node in NodesHovered)
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
            if (NodesHovered.Length == 0)
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

            const float indent = 5;
            float indentAmount = 0;

            var labels = new List<Tuple<string, SolidBrush>>();
            string indentStr = "";

            // show all labels if showing just graph nodes, or show labels isnt on, or the label isnt displayed cause there's not enough room
            //foreach(var node in NodesHovered)//.Where(n => !ShowLabels || !n.RoomForLabel || n.LabelClipped))
            //{    
            var n = NodesHovered.Last();
            SolidBrush color = GuiHovered.Contains(n) ? ObjBrushes[(int)n.ObjType] : ObjDitheredBrushes[(int)n.ObjType];
            labels.Add(new Tuple<string, SolidBrush>(indentStr + n.Name, color));
            indentStr += " ";
            //}

            if (labels.Count == 0)
                return;

            var lastNode = NodesHovered.Last();

            if (lastNode.ObjType == XObjType.Class)
            {
                int staticCount = 0;
                int instCount = 0;
                if (lastNode != null && lastNode.Record != null && lastNode.Record.Active.Count > 0)
                {
                    var record = lastNode.Record;

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
                labels.Add(new Tuple<string, SolidBrush>(lastNode.GetMethodName(false), TextBrush));
            }

            else if (lastNode.ObjType == XObjType.Namespace)
            {
                labels.Clear();
                labels.Add(new Tuple<string, SolidBrush>(lastNode.FullName(true), ObjBrushes[(int)lastNode.ObjType]));
            }

            else if (lastNode.ObjType == XObjType.Field)
            {
                var classNode = lastNode.GetParentClass(false) as XNodeIn;

                if (classNode != null && classNode.Record != null && classNode.Record.Active.Count > 0)
                {
                    var record = classNode.Record;

                    lock (record.Active)
                    {
                        FieldInfo field = null;

                        for (int i = 0; i < record.Active.Count; i++)
                        {                        
                            var instance = record.Active[i];

                            field = instance.GetField(lastNode.UnformattedName);

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

        public string GetMetricForNode(XNodeIn node)
        {
            switch (Model.SizeLayout)
            {
                case SizeLayouts.Constant:
                    return node.Value.ToString() + " elements";

                case SizeLayouts.MethodSize:
                    return node.Value.ToString() + " lines";

                case SizeLayouts.TimeInMethod:
                    return Xtensions.TicksToString(node.Value);

                case SizeLayouts.Hits:
                    return node.Value.ToString() + " calls";

                case SizeLayouts.TimePerHit:
                    return Xtensions.TicksToString(node.Value);
            }

            return "";
        }

        private void DrawGraphEdge(Graphics buffer, Pen pen, XNodeIn source, XNodeIn destination)
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
