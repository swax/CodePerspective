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

namespace XLibrary
{
    

    public partial class TreePanelGdiPlus : UserControl
    {
        public MainForm MainForm;
        public ViewModel Model;

        public bool DoRedraw = true;
        public bool DoResize = true;
        bool DoRevalue = true;
        Bitmap DisplayBuffer;

        bool ShowingOutside;
        bool ShowingExternal;

        internal bool ShowLabels = true;
        StringFormat LabelFormat = new StringFormat();

        float LabelPadding = 2;
           
        Color UnknownColor = Color.Black;
        Color FileColor = Color.Black;
        Color NamespaceColor = Color.DarkBlue;
        Color ClassColor = Color.DarkGreen;
        Color MethodColor = Color.DarkRed;
        Color FieldColor = Color.Goldenrod;

        SolidBrush[] ObjBrushes;
        SolidBrush[] ObjDitheredBrushes;
        Pen[] ObjPens;
        Pen[] ObjFocusedPens;

        public static Dictionary<int, Color> ObjColors = new Dictionary<int, Color>();


        SolidBrush NothingBrush = new SolidBrush(Color.White);
        SolidBrush OutsideBrush = new SolidBrush(Color.LightGray);

        SolidBrush EntryBrush = new SolidBrush(Color.LightGreen);
        SolidBrush MultiEntryBrush = new SolidBrush(Color.LimeGreen);

        SolidBrush HoldingBrush = new SolidBrush(Color.FromArgb(255, 255, 192));
        SolidBrush MultiHoldingBrush = new SolidBrush(Color.Yellow);

        SolidBrush SearchMatchBrush = new SolidBrush(Color.Red);

        // border between outside/external panels
        float PanelBorderWidth = 4;
        float NodeBorderWidth = 4;
        SolidBrush BorderBrush = new SolidBrush(Color.Silver);

        Color CallColor = Color.DarkGreen;
        Pen ShowCallPen = new Pen(Color.FromArgb(32, Color.Black));// { EndCap = LineCap.ArrowAnchor };
        Pen CallOutPen = new Pen(Color.FromArgb(48, Color.Red));
        Pen CallInPen = new Pen(Color.FromArgb(48, Color.Blue));
        Pen CallOutPenFocused = new Pen(Color.FromArgb(70, Color.Red), 2);
        Pen CallInPenFocused = new Pen(Color.FromArgb(70, Color.Blue), 2);
        Pen HoldingCallPen = new Pen(Color.FromArgb(48, Color.Green)) { EndCap = LineCap.ArrowAnchor };

        Pen CallDividerPen = new Pen(Color.FromArgb(0xcc, 0xcc, 0xcc));

        Color HitColor = Color.FromArgb(255, 192, 128);
        Color MultiHitColor = Color.Orange;

        Color ExceptionColor = Color.Red;
        Color MultiExceptionColor = Color.DarkRed;

        Color FieldSetColor = Color.Blue;
        Color FieldGetColor = Color.LimeGreen;

        SolidBrush TextBrush = new SolidBrush(Color.Black);
        SolidBrush TextBgBrush = new SolidBrush(Color.FromArgb(192, Color.White));
        SolidBrush LabelBgBrush = new SolidBrush(Color.FromArgb(128, Color.White));
        SolidBrush FooterBgBrush = new SolidBrush(Color.White);
        Font TextFont = new Font("tahoma", 8, FontStyle.Bold );

        Font InstanceFont = new Font("tahoma", 11, FontStyle.Bold);
        SolidBrush InstanceBrush = new SolidBrush(Color.Black);

        Dictionary<int, XNodeIn> PositionMap = new Dictionary<int, XNodeIn>();
        Dictionary<int, XNodeIn> CenterMap = new Dictionary<int, XNodeIn>(); // used to filter calls into and out of center
        Dictionary<int, XNodeIn> OutsideMap = new Dictionary<int, XNodeIn>(); // used to filter calls into and out of center     

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

        Pen FilteredPen = new Pen(Color.LimeGreen, 3);
        SolidBrush FilteredBrush = new SolidBrush(Color.LimeGreen);
        Dictionary<int, XNodeIn> FilteredNodes = new Dictionary<int, XNodeIn>();

        Pen IgnoredPen = new Pen(Color.Red, 3);
        SolidBrush IgnoredBrush = new SolidBrush(Color.Red);
        Dictionary<int, XNodeIn> IgnoredNodes = new Dictionary<int, XNodeIn>();

        // dependencies
        SolidBrush DependentBrush = new SolidBrush(Color.Red);
        SolidBrush IndependentBrush = new SolidBrush(Color.Blue);
        SolidBrush InterdependentBrush = new SolidBrush(Color.Purple);

        HashSet<int> DependentClasses = new HashSet<int>();
        HashSet<int> IndependentClasses = new HashSet<int>();

        public string SearchString;
        public string LastSearch;

        SizeF ModelSize = Size.Empty; // in screen coords
        PointF ModelOffset = PointF.Empty; // in model coords 0-1
        PointF PanOffset; // in model coords
 

        public TreePanelGdiPlus(MainForm main)
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
                int brightness = 128 / (OverBrushes.Length  + 1) * (OverBrushes.Length - i);
                OverBrushes[i] = new SolidBrush(Color.FromArgb(128 + brightness, 128 + brightness, 255));
            }

            // set colors of differnt brush / pen arrays
            ObjColors.Clear();
            ObjColors[(int)XObjType.Root] = UnknownColor;
            ObjColors[(int)XObjType.External] = UnknownColor;
            ObjColors[(int)XObjType.Internal] = UnknownColor;
            ObjColors[(int)XObjType.File] = FileColor;
            ObjColors[(int)XObjType.Namespace] = NamespaceColor;
            ObjColors[(int)XObjType.Class] = ClassColor;
            ObjColors[(int)XObjType.Field] = FieldColor;
            ObjColors[(int)XObjType.Method] = MethodColor;

            var objTypes = Enum.GetValues(typeof(XObjType));
           
            ObjBrushes = new SolidBrush[objTypes.Length];
            ObjDitheredBrushes = new SolidBrush[objTypes.Length];
            ObjPens = new Pen[objTypes.Length];
            ObjFocusedPens = new Pen[objTypes.Length];

            for (int i = 0; i < objTypes.Length; i++ )
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

            if ((!DoRedraw && !DoRevalue && !DoResize) || Model.CurrentRoot == null)
            {
                e.Graphics.DrawImage(DisplayBuffer, 0, 0);
                Model.FpsCount++;
                return;
            }

            // background
            Graphics buffer = Graphics.FromImage(DisplayBuffer);
            buffer.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed; // todo option to turn this off

            buffer.Clear(Color.White);

            Debug.Assert(Model.CurrentRoot != Model.TopRoot); // current root should be intenalRoot in this case

            ShowingOutside = Model.ShowOutside && Model.CurrentRoot != Model.InternalRoot;
            ShowingExternal = Model.ShowExternal && !Model.CurrentRoot.External;

            // clear and pre-process marked depencies
            DependentClasses.Clear();
            IndependentClasses.Clear();

            if ((Model.ViewLayout == LayoutType.TreeMap && Model.MapMode == TreeMapMode.Dependencies) ||
                (Model.ViewLayout == LayoutType.CallGraph && Model.GraphMode == CallGraphMode.Dependencies))
            {
                RecalcDependencies();
            }

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
            ModelSize.Width = Width * ZoomFactor;
            ModelSize.Height = Height * ZoomFactor;
            ModelOffset.X = PanOffset.X * ModelSize.Width;// +(Width * CenterFactor.X - ModelSize.Width * CenterFactor.X);
            ModelOffset.Y = PanOffset.Y * ModelSize.Height;// +(Height * CenterFactor.Y - ModelSize.Height * CenterFactor.Y);

            if (Model.ViewLayout == LayoutType.TreeMap)
                DrawTreeMap(buffer);

            else if (Model.ViewLayout == LayoutType.CallGraph)
                DrawCallGraph(buffer);

            // draw ignored over nodes ignored may contain
            foreach (XNodeIn ignored in IgnoredNodes.Values)
                if (PositionMap.ContainsKey(ignored.ID))
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
                foreach (var source in PositionMap.Values)
                {
                    if (source.EdgesOut == null)
                        continue;

                    foreach (var to in source.EdgesOut)
                    {
                        if (!PositionMap.ContainsKey(to))
                            continue;

                        XNodeIn destination = PositionMap[to];

                        bool focused = (source.Focused || destination.Focused);

                        if ((!DrawCallGraphVertically && source.AreaF.X < destination.AreaF.X) ||
                            (DrawCallGraphVertically && source.AreaF.Y < destination.AreaF.Y))
                            DrawGraphEdge(buffer, focused ? CallOutPenFocused : CallOutPen, source, destination);
                        else
                            DrawGraphEdge(buffer, focused ? CallInPenFocused : CallInPen, source, destination);
                    }
                }
            }

            // draw call graph
            if (XRay.FlowTracking)
            {
                foreach (var source in PositionMap.Values)
                {
                    if (source.CallsOut == null)
                        continue;

                    foreach (var call in source.CallsOut)
                    {
                        if (!PositionMap.ContainsKey(call.Destination))
                            continue;

                        XNodeIn destination = PositionMap[call.Destination];

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
                                PointF start = PositionMap[call.Source].CenterF;
                                PointF end = PositionMap[call.Destination].CenterF;
                                PointF mid = new PointF(start.X + (end.X - start.X) / 2, start.Y + (end.Y - start.Y) / 2);

                                buffer.DrawLine(callOutPen, start, mid);
                                buffer.DrawLine(callInPen, mid, end);
                            }
                            else if (Model.ViewLayout == LayoutType.CallGraph)
                            {
                                if ((!DrawCallGraphVertically && source.AreaF.X < destination.AreaF.X) || 
                                    (DrawCallGraphVertically && source.AreaF.Y < destination.AreaF.Y))
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

            DoRedraw = false;
            Model.RedrawCount++;
        }

        private void DrawTreeMap(Graphics buffer)
        {
            if (DoRevalue ||
                (Model.ShowLayout != ShowNodes.All && XRay.CoverChange) ||
                (Model.ShowLayout == ShowNodes.Instances && XRay.InstanceChange))
            {
                Model.RecalcCover(Model.InternalRoot);
                Model.RecalcCover(Model.ExternalRoot);

                XRay.CoverChange = false;
                XRay.InstanceChange = false;

                DoRevalue = false;
                Model.RevalueCount++;

                DoResize = true;
            }

            if (DoResize)
            {
                var drawArea = new RectangleF(ModelOffset.X, ModelOffset.Y, ModelSize.Width, ModelSize.Height);

                float offset = 0;
                float centerWidth = drawArea.Width;

                PositionMap.Clear();
                CenterMap.Clear();

                if (ShowingOutside)
                {
                    offset = drawArea.Width * 1.0f / 4.0f;
                    centerWidth -= offset;

                    Model.InternalRoot.SetArea(new RectangleF(ModelOffset.X, ModelOffset.Y, offset - PanelBorderWidth, drawArea.Height));
                    PositionMap[Model.InternalRoot.ID] = Model.InternalRoot;
                    SizeNode(buffer, Model.InternalRoot, Model.CurrentRoot, false);
                }
                if (ShowingExternal)
                {
                    float extWidth = drawArea.Width * 1.0f / 4.0f;
                    centerWidth -= extWidth;

                    Model.ExternalRoot.SetArea(new RectangleF(ModelOffset.X + offset + centerWidth + PanelBorderWidth, ModelOffset.Y, extWidth - PanelBorderWidth, drawArea.Height));
                    PositionMap[Model.ExternalRoot.ID] = Model.ExternalRoot;
                    SizeNode(buffer, Model.ExternalRoot, null, false);
                }

                Model.CurrentRoot.SetArea(new RectangleF(ModelOffset.X + offset, ModelOffset.Y, centerWidth, drawArea.Height));
                PositionMap[Model.CurrentRoot.ID] = Model.CurrentRoot;
                SizeNode(buffer, Model.CurrentRoot, null, true);

                DoResize = false;
                Model.ResizeCount++;
            }

            if (ShowingOutside)
            {
                buffer.FillRectangle(BorderBrush, Model.InternalRoot.AreaF.Width, 0, PanelBorderWidth, Model.InternalRoot.AreaF.Height);
                DrawNode(buffer, Model.InternalRoot, 0, true);
            }

            if (ShowingExternal)
            {
                buffer.FillRectangle(BorderBrush, Model.ExternalRoot.AreaF.X - PanelBorderWidth, 0, PanelBorderWidth, Model.ExternalRoot.AreaF.Height);
                DrawNode(buffer, Model.ExternalRoot, 0, true);
            }

            DrawNode(buffer, Model.CurrentRoot, 0, true);
        }

        private bool IsNodeFiltered(bool select, XNodeIn node)
        {
            Dictionary<int, XNodeIn> map = select ? FilteredNodes : IgnoredNodes;

            foreach (XNodeIn parent in node.GetParents())
                if (map.ContainsKey(parent.ID))
                    return true;

            return false;
        }

        private void SizeNode(Graphics buffer, XNodeIn root, XNodeIn exclude, bool center)
        {
            if (!root.Show)
                return;

            RectangleF insideArea = root.AreaF;

            if (ShowLabels)
            {
                // check if enough room in root box for label
                var labelSpace = root.AreaF;
                labelSpace.Width -= LabelPadding * 2.0f;
                labelSpace.Height -= LabelPadding * 2.0f;

                var labelSize = new RectangleF(root.AreaF.Location, buffer.MeasureString(root.Name, TextFont));
                float minHeight = (root.Nodes.Count > 0) ? labelSize.Height * 2.0f : labelSize.Height;

                if (minHeight < labelSpace.Height && labelSize.Width / 3f < labelSpace.Width )
                {
                    labelSize.X += LabelPadding;
                    labelSize.Y += LabelPadding;

                    if (labelSpace.Width < labelSize.Width)
                    {
                        root.LabelClipped = true;
                        labelSize.Width = labelSpace.Width;
                    }

                    insideArea.Y += labelSize.Height;
                    insideArea.Height -= labelSize.Height;

                    root.RoomForLabel = true;
                    root.LabelRect = labelSize;
                }
            }

            List<Sector> sectors = new TreeMap(root, exclude, insideArea.Size).Results;

            foreach (Sector sector in sectors)
            {
                XNodeIn node = sector.OriginalValue as XNodeIn;

                sector.Rect = RectangleExtensions.Contract(sector.Rect, NodeBorderWidth);

                if (sector.Rect.X < NodeBorderWidth) sector.Rect.X = NodeBorderWidth;
                if (sector.Rect.Y < NodeBorderWidth) sector.Rect.Y = NodeBorderWidth;
                if (sector.Rect.X > insideArea.Width - NodeBorderWidth) sector.Rect.X = insideArea.Width - NodeBorderWidth;
                if (sector.Rect.Y > insideArea.Height - NodeBorderWidth) sector.Rect.Y = insideArea.Height - NodeBorderWidth;

                sector.Rect.X += insideArea.X;
                sector.Rect.Y += insideArea.Y;

                node.SetArea(sector.Rect);
                PositionMap[node.ID] = node;

                node.RoomForLabel = false; // cant do above without graphic artifacts
                node.LabelClipped = false;

                if (center)
                    CenterMap[node.ID] = node;

                if (sector.Rect.Width > 1.0f && sector.Rect.Height > 1.0f)
                    SizeNode(buffer, node, exclude, center);
            }
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
            else if (Model.ViewLayout == LayoutType.TreeMap || CenterMap.ContainsKey(node.ID))
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
                bool dependent = DependentClasses.Contains(node.ID);
                bool independent = IndependentClasses.Contains(node.ID);

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
            if (ShowLabels && node.RoomForLabel)
            {
                buffer.FillRectangle(LabelBgBrush, node.LabelRect);
                buffer.DrawString(node.Name, TextFont, ObjBrushes[(int)node.ObjType], node.LabelRect, LabelFormat);
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

                DoResize = true;
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
            modelPos.X = (e.Location.X - ModelOffset.X) / ModelSize.Width;
            modelPos.Y = (e.Location.Y - ModelOffset.Y) / ModelSize.Height; 

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
                PanOffset = new PointF();
            }
            else
            {
                // we want to keep the zoom over the cursor, the modify the window offset by the zoom levl
                winPos.Width /= ZoomFactor;
                winPos.Height /= ZoomFactor;

                // subtract the window pos from our target pos in the model to find the amount that should be panned
                PanOffset.X = winPos.Width - modelPos.X;
                PanOffset.Y = winPos.Height - modelPos.Y;
            }

            DoResize = true;
            Invalidate();
        }

        private void TreePanelGdiPlus_MouseDown(object sender, MouseEventArgs e)
        {
            MouseDownPoint = e.Location;
            PanStart = PanOffset;
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
                PanOffset.X = PanStart.X + (e.Location.X - MouseDownPoint.X) / ModelSize.Width;
                PanOffset.Y = PanStart.Y + (e.Location.Y - MouseDownPoint.Y) / ModelSize.Height;

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
                foreach(var node in PositionMap.Values)
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
            DoRedraw = true;
            Invalidate();
        }

        // re-calcs the sizes of all nodes
        public void RecalcSizes()
        {
            DoResize = true;
            Invalidate();
        }

        // re-calcs the sizes of all nodes
        public void RecalcValues()
        {
            DoRevalue = true;
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

                    if (node.ObjType == XObjType.Class)
                        MainForm.InstanceTab.NavigateTo(node);

                    if (node.ObjType == XObjType.Method)
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
            PanOffset = Point.Empty;
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

            DoRevalue = true;
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

        private void RecalcDependencies()
        {

            if (Model.FocusedNodes.Count == 0)
                return;

            // find dependencies for selected classes
            Dictionary<int, XNodeIn> classes = Model.GetClassesFromFocusedNodes();

            bool doRecurse = Model.ShowAllDependencies;
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
            SizeF size = buffer.MeasureString(label, TextFont);
            buffer.FillRectangle(FooterBgBrush, x, Height - size.Height, size.Width, size.Height);
            buffer.DrawString(label, TextFont, brush, x, Height - size.Height);
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

            // show all labels if showing just graph nodes, or show labels isnt on, or the label isnt displayed cause there's not enough room
            var labels = NodesHovered.Where(n => !ShowLabels || !n.RoomForLabel || n.LabelClipped).ToArray();

            if (labels.Length == 0)
                return;

            var lastNode = labels.Last();

            // find the size of the background box
            //foreach (XNodeIn node in labels)
            XNodeIn node = labels.Last();
            //{
                string label = node.Name;
                //if (node == lastNode)
                //     label += " (" + GetMetricForNode(node) + ")";

                SizeF size = buffer.MeasureString(label, TextFont);

                if (size.Width + indentAmount > bgWidth)
                    bgWidth = size.Width + indentAmount;

                bgHeight += size.Height;
                lineHeight = size.Height;
                indentAmount += indent;
            //}

            // put box lower right corner at cursor
            //pos.X -= bgWidth;
            pos.Y -= bgHeight;

            // ensure it doesnt go off screen
            if (pos.X < 0) pos.X = 0;
            if (pos.Y < 0) pos.Y = 0;

            // draw background
            buffer.FillRectangle(TextBgBrush, pos.X, pos.Y, bgWidth, bgHeight);

            //GraphDebuggingLabel(buffer, pos);

            //foreach (XNodeIn node in labels)
            node = labels.Last();
            //{
                // dither label if it is not on screen
                SolidBrush textColor = GuiHovered.Contains(node) ? ObjBrushes[(int)node.ObjType] : ObjDitheredBrushes[(int)node.ObjType];

                label = node.Name;
                //if (node == lastNode)
                //    label += " (" + GetMetricForNode(node) + ")";

                buffer.DrawString(label, TextFont, textColor, pos.X, pos.Y);

                pos.Y += lineHeight;
                pos.X += indent;
            //}
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
    }
}
