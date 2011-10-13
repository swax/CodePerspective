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
    public enum LayoutType { TreeMap, CallGraph }
    public enum SizeLayouts { Constant, MethodSize, TimeInMethod, Hits, TimePerHit }
    public enum ShowNodes { All, Hit, Unhit, Instances }

    public partial class TreePanelGdiPlus : UserControl
    {
        public MainForm MainForm;

        bool DoRedraw = true;
        bool DoResize = true;
        bool DoRevalue = true;
        Bitmap DisplayBuffer;

        internal bool ShowOutside;
        internal bool ShowExternal;
        bool ShowingOutside;
        bool ShowingExternal;

        internal LayoutType ViewLayout = LayoutType.TreeMap;
        internal SizeLayouts SizeLayout = SizeLayouts.MethodSize;
        internal ShowNodes ShowLayout = ShowNodes.All;
        internal bool ShowFields = true;
        
        internal bool ShowLabels = true;
        float LabelPadding = 2;
           
        internal bool ShowCalls = true;

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

        internal static Dictionary<int, Color> ObjColors = new Dictionary<int, Color>();


        SolidBrush NothingBrush = new SolidBrush(Color.White);

        SolidBrush EntryBrush = new SolidBrush(Color.LightGreen);
        SolidBrush MultiEntryBrush = new SolidBrush(Color.LimeGreen);

        SolidBrush HoldingBrush = new SolidBrush(Color.FromArgb(255, 255, 192));
        SolidBrush MultiHoldingBrush = new SolidBrush(Color.Yellow);
        
        // border between outside/external panels
        int BorderWidth = 4;
        SolidBrush BorderBrush = new SolidBrush(Color.Silver);

        Color CallColor = Color.Blue;
        Pen ShowCallPen = new Pen(Color.FromArgb(32, Color.Black));// { EndCap = LineCap.ArrowAnchor };
        Pen CallOutPen = new Pen(Color.FromArgb(48, Color.Red));
        Pen CallInPen = new Pen(Color.FromArgb(48, Color.Blue));
        Pen CallOutPenFocused = new Pen(Color.FromArgb(70, Color.Red), 2);
        Pen CallInPenFocused = new Pen(Color.FromArgb(70, Color.Blue), 2);
        Pen HoldingCallPen = new Pen(Color.FromArgb(48, Color.Blue)) { EndCap = LineCap.ArrowAnchor };
        

        Color HitColor = Color.FromArgb(255, 192, 128);
        Color MultiHitColor = Color.Orange;

        Color ExceptionColor = Color.Red;
        Color MultiExceptionColor = Color.DarkRed;

        Color FieldSetColor = Color.Blue;
        Color FieldGetColor = Color.LimeGreen;

        SolidBrush TextBrush = new SolidBrush(Color.Black);
        SolidBrush TextBgBrush = new SolidBrush(Color.FromArgb(192, Color.White));
        SolidBrush LabelBgBrush = new SolidBrush(Color.FromArgb(128, Color.White));
        Font TextFont = new Font("tahoma", 8, FontStyle.Bold );

        Font InstanceFont = new Font("tahoma", 11, FontStyle.Bold);
        SolidBrush InstanceBrush = new SolidBrush(Color.Black);

        Dictionary<int, XNodeIn> PositionMap = new Dictionary<int, XNodeIn>();
        Dictionary<int, XNodeIn> CenterMap = new Dictionary<int, XNodeIn>(); // used to filter calls into and out of center

        internal XNodeIn CurrentRoot;
        XNodeIn InternalRoot;
        XNodeIn ExternalRoot;
        XNodeIn TopRoot;        

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

        XNodeIn FocusedNode;

        Pen FilteredPen = new Pen(Color.LimeGreen, 3);
        SolidBrush FilteredBrush = new SolidBrush(Color.LimeGreen);
        Dictionary<int, XNodeIn> FilteredNodes = new Dictionary<int, XNodeIn>();

        Pen IgnoredPen = new Pen(Color.Red, 3);
        SolidBrush IgnoredBrush = new SolidBrush(Color.Red);
        Dictionary<int, XNodeIn> IgnoredNodes = new Dictionary<int, XNodeIn>();


        public TreePanelGdiPlus(MainForm main)
        {
            InitializeComponent();

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            MainForm = main;

            TopRoot = XRay.RootNode;
            InternalRoot = TopRoot.Nodes.First(n => n.ObjType == XObjType.Internal) as XNodeIn;
            ExternalRoot = TopRoot.Nodes.First(n => n.ObjType == XObjType.External) as XNodeIn;
            CurrentRoot = InternalRoot;

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

            if ((!DoRedraw && !DoRevalue && !DoResize) || CurrentRoot == null)
            {
                e.Graphics.DrawImage(DisplayBuffer, 0, 0);
                return;
            }

            // background
            Graphics buffer = Graphics.FromImage(DisplayBuffer);
            buffer.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed; // todo option to turn this off

            buffer.Clear(Color.White);

            Debug.Assert(CurrentRoot != TopRoot); // current root should be intenalRoot in this case

            ShowingOutside = ShowOutside && CurrentRoot != InternalRoot;
            ShowingExternal = ShowExternal && !CurrentRoot.External;


            if (ViewLayout == LayoutType.TreeMap)
                DrawTreeMap(buffer);
            else if (ViewLayout == LayoutType.CallGraph)
                DrawCallGraph(buffer);


            // draw ignored over nodes ignored may contain
            foreach (XNodeIn ignored in IgnoredNodes.Values)
                if (PositionMap.ContainsKey(ignored.ID))
                {
                    buffer.DrawLine(IgnoredPen, ignored.AreaF.UpperLeftCorner(), ignored.AreaF.LowerRightCorner() );
                    buffer.DrawLine(IgnoredPen, ignored.AreaF.UpperRightCorner(), ignored.AreaF.LowerLeftCorner());
                }

            // draw dependency graph
            if (ViewLayout == LayoutType.CallGraph && GraphMode == CallGraphMode.Dependencies)
            {
                foreach (var source in PositionMap.Values)
                {
                    if (source.DependenciesTo == null)
                        continue;

                    foreach (var to in source.DependenciesTo)
                    {
                        if (!CenterMap.ContainsKey(source.ID) && !CenterMap.ContainsKey(to))
                            continue;

                        if (!PositionMap.ContainsKey(to))
                            continue;

                        XNodeIn destination = PositionMap[to];

                        bool focused = (source.Focused || destination.Focused);

                        if (source.AreaD.X < destination.AreaD.X)
                            DrawGraphEdge(buffer, focused ? CallOutPenFocused : CallOutPen, source, destination);
                        else
                            DrawGraphEdge(buffer, focused ? CallInPenFocused : CallInPen, source, destination);
                    }
                }
            }

            // draw call graph
            if (XRay.FlowTracking && GraphMode != CallGraphMode.Dependencies)
            {
                foreach (var source in PositionMap.Values)
                {
                    if (source.CallsOut == null)
                        continue;

                    // dont show class call graph in tree map mode
                    if (ViewLayout == LayoutType.TreeMap && source.ObjType == XObjType.Class)
                        continue;

                    foreach (var call in source.CallsOut)
                    {
                        if (!CenterMap.ContainsKey(call.Source) && !CenterMap.ContainsKey(call.Destination))
                            continue;

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

                        if (call.StillInside > 0 && ShowCalls)
                        {
                            if (ViewLayout == LayoutType.TreeMap)
                                buffer.DrawLine(HoldingCallPen, source.CenterF, destination.CenterF);
                            else if (ViewLayout == LayoutType.CallGraph)
                                DrawGraphEdge(buffer, HoldingCallPen, source, destination);
                        }

                        else if (XRay.ShowAllCalls)
                        {
                            if (ViewLayout == LayoutType.TreeMap)
                            {
                                PointF start = PositionMap[call.Source].CenterF;
                                PointF end = PositionMap[call.Destination].CenterF;
                                PointF mid = new PointF(start.X + (end.X - start.X) / 2, start.Y + (end.Y - start.Y) / 2);

                                buffer.DrawLine(callOutPen, start, mid);
                                buffer.DrawLine(callInPen, mid, end);
                            }
                            else if (ViewLayout == LayoutType.CallGraph)
                            {
                                if (source.AreaD.X < destination.AreaD.X)
                                    DrawGraphEdge(buffer, callOutPen, source, destination);
                                else
                                    DrawGraphEdge(buffer, callInPen, source, destination);
                            }
                        }

                        if (call.Hit > 0 && ShowCalls)
                        {
                            Pen pen = focused ? CallPenFocused[call.Hit] : CallPen[call.Hit];
                            pen.DashOffset = call.DashOffset;

                            if (ViewLayout == LayoutType.TreeMap)
                                buffer.DrawLine(pen, source.CenterF, destination.CenterF);
                            else if (ViewLayout == LayoutType.CallGraph)
                                DrawGraphEdge(buffer, pen, source, destination);
                        }
                    }
                }
            }

            // draw mouse over label
            PointF pos = PointToClient(Cursor.Position);
            if (NodesHovered.Length > 0 && 
                ClientRectangle.Contains((int)pos.X, (int)pos.Y))
            {
                // for each node selected, get size, figure out bg size and indents, then pass again and draw

                float bgWidth = 0;
                float bgHeight = 0;
                float lineHeight = 0;

                const float indent = 5;
                float indentAmount = 0;

                // show all labels if showing just graph nodes, or show labels isnt on, or the label isnt displayed cause there's not enough room
                var labels = NodesHovered.Where(n => !ShowLabels || !n.RoomForLabel || ViewLayout == LayoutType.CallGraph);

                // find the size of the background box
                foreach (XNode node in labels)
                {
                    SizeF size = buffer.MeasureString(node.Name, TextFont);

                    if (size.Width + indentAmount > bgWidth)
                        bgWidth = size.Width + indentAmount;

                    bgHeight += size.Height;
                    lineHeight = size.Height;
                    indentAmount += indent;
                }

                // put box lower right corner at cursor
                pos.X -= bgWidth;
                pos.Y -= bgHeight;
               
                // ensure it doesnt go off screen
                if (pos.X < 0) pos.X = 0;
                if (pos.Y < 0) pos.Y = 0;

                // draw background
                buffer.FillRectangle(TextBgBrush, pos.X, pos.Y, bgWidth, bgHeight);


                #region call graph debugging
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
                #endregion


                foreach (XNodeIn node in labels)
                {
                    // dither label if it is not on screen
                    if(GuiHovered.Contains(node))
                        buffer.DrawString(node.Name, TextFont, ObjBrushes[(int)node.ObjType], pos.X, pos.Y);
                    else
                        buffer.DrawString(node.Name, TextFont, ObjDitheredBrushes[(int)node.ObjType], pos.X, pos.Y);

                    pos.Y += lineHeight;
                    pos.X += indent;
                }
            }

            // Copy buffer to display
            e.Graphics.DrawImage(DisplayBuffer, 0, 0);

            DoRedraw = false;
        }

        private void DrawTreeMap(Graphics buffer)
        {
            if (DoRevalue ||
                (ShowLayout != ShowNodes.All && XRay.CoverChange) ||
                (ShowLayout == ShowNodes.Instances && XRay.InstanceChange))
            {
                RecalcCover(InternalRoot);
                RecalcCover(ExternalRoot);

                XRay.CoverChange = false;
                XRay.InstanceChange = false;

                DoRevalue = false;
                DoResize = true;
            }

            if (DoResize)
            {
                int offset = 0;
                int centerWidth = Width;

                PositionMap.Clear();
                CenterMap.Clear();

                if (ShowingOutside)
                {
                    offset = Width * 1 / 4;
                    centerWidth -= offset;

                    InternalRoot.SetArea(new RectangleD(0, 0, offset - BorderWidth, Height));
                    PositionMap[InternalRoot.ID] = InternalRoot;
                    SizeNode(buffer, InternalRoot, CurrentRoot, false);
                }
                if (ShowingExternal)
                {
                    int extWidth = Width * 1 / 4;
                    centerWidth -= extWidth;

                    ExternalRoot.SetArea(new RectangleD(offset + centerWidth + BorderWidth, 0, extWidth - BorderWidth, Height));
                    PositionMap[ExternalRoot.ID] = ExternalRoot;
                    SizeNode(buffer, ExternalRoot, null, false);
                }

                CurrentRoot.SetArea(new RectangleD(offset, 0, centerWidth, Height));
                PositionMap[CurrentRoot.ID] = CurrentRoot;
                SizeNode(buffer, CurrentRoot, null, true);

                DoResize = false;
            }

            if (ShowingOutside)
            {
                buffer.FillRectangle(BorderBrush, InternalRoot.AreaF.Width, 0, BorderWidth, InternalRoot.AreaF.Height);
                DrawNode(buffer, InternalRoot, 0, true);
            }

            if (ShowingExternal)
            {
                buffer.FillRectangle(BorderBrush, ExternalRoot.AreaF.X - BorderWidth, 0, BorderWidth, ExternalRoot.AreaF.Height);
                DrawNode(buffer, ExternalRoot, 0, true);
            }

            DrawNode(buffer, CurrentRoot, 0, true);
        }

        private bool IsNodeFiltered(bool select, XNodeIn node)
        {
            Dictionary<int, XNodeIn> map = select ? FilteredNodes : IgnoredNodes;

            foreach (XNodeIn parent in node.GetParents())
                if (map.ContainsKey(parent.ID))
                    return true;

            return false;
        }

        private long RecalcCover(XNodeIn root)
        {
            root.Value = 0;

            // only leaves have usable value
            if (root.ObjType == XObjType.Method || root.ObjType == XObjType.Field)
            {
                switch (SizeLayout)
                {
                    case SizeLayouts.Constant:
                        root.Value = 1;
                        break;
                    case SizeLayouts.MethodSize:
                        root.Value = root.Lines;
                        break;
                    case SizeLayouts.TimeInMethod:
                        // why is this negetive?? HAVENT RETURNED YET, property should return 0 i think if  neg, or detect still inside and return that
                        if (root.CalledIn != null)
                            foreach (FunctionCall call in root.CalledIn)
                                root.Value += call.TotalTimeInsideDest;
                        break;
                    case SizeLayouts.Hits:
                        if (root.CalledIn != null)
                            foreach (FunctionCall call in root.CalledIn)
                                root.Value += call.TotalHits;
                        break;
                    case SizeLayouts.TimePerHit:
                        if (root.CalledIn != null)
                        {
                            int count = 0;

                            foreach (FunctionCall call in root.CalledIn)
                                if (call.TotalHits > 0)
                                {
                                    count++;
                                    root.Value += call.TotalTimeInsideDest / call.TotalHits;
                                }

                            if (count > 0)
                                root.Value /= count;
                        }

                        break;
                }
            }

            foreach (XNodeIn node in root.Nodes)
            {
                if (node.ObjType == XObjType.Field && !ShowFields)
                {
                    node.Show = false;
                    continue;
                }

                node.Show = //node.ObjType != XObjType.Method ||
                    ShowLayout == ShowNodes.All ||
                    (ShowLayout == ShowNodes.Hit && XRay.CoveredFunctions[node.ID]) ||
                    (ShowLayout == ShowNodes.Unhit && !XRay.CoveredFunctions[node.ID]) ||
                    (ShowLayout == ShowNodes.Instances && (node.ObjType != XObjType.Class || (node.Record != null && node.Record.Active.Count > 0)));

                if (node.Show)
                    root.Value += RecalcCover(node);
            }


            //XRay.LogError("Calc'd Node: {0}, Value: {1}", root.Name, root.Value);

            Debug.Assert(root.Value >= 0);

            return root.Value;
        }

        const int Border = 4;

        private void SizeNode(Graphics buffer, XNodeIn root, XNodeIn exclude, bool center)
        {
            if (!root.Show)
                return;

            RectangleD insideArea = root.AreaD;

            if (ShowLabels)
            {
                // check if enough room in root box for label
                RectangleF label = new RectangleF(root.AreaF.Location, buffer.MeasureString(root.Name, TextFont));
          

                float minHeight = (root.Nodes.Count > 0) ? label.Height * 2 : label.Height;

                if (root.AreaF.Height > minHeight && root.AreaF.Width > label.Width + LabelPadding * 2)
                {
                    label.X += LabelPadding;
                    label.Y += LabelPadding;

                    insideArea.Y += label.Height;
                    insideArea.Height -= label.Height;

                    root.RoomForLabel = true;
                    root.LabelRect = label;
                }
            }

            List<Sector> sectors = new TreeMap(root, exclude, insideArea.Size).Results;

            foreach (Sector sector in sectors)
            {
                XNodeIn node = sector.OriginalValue as XNodeIn;

                sector.Rect.Contract(Border);

                if (sector.Rect.X < Border) sector.Rect.X = Border;
                if (sector.Rect.Y < Border) sector.Rect.Y = Border;
                if (sector.Rect.X > insideArea.Width - Border) sector.Rect.X = insideArea.Width - Border;
                if (sector.Rect.Y > insideArea.Height - Border) sector.Rect.Y = insideArea.Height - Border;

                sector.Rect.X += insideArea.X;
                sector.Rect.Y += insideArea.Y;

                node.SetArea(sector.Rect);
                PositionMap[node.ID] = node;

                node.RoomForLabel = false; // cant do above without graphic artifacts

                if (center)
                    CenterMap[node.ID] = node;
                
                if(sector.Rect.Width > 1 && sector.Rect.Height > 1)
                    SizeNode(buffer, node, exclude, center);
            }
        }

        private void DrawNode(Graphics buffer, XNodeIn node, int depth, bool drawChildren)
        {
            if (!node.Show)
                return;

            // blue selection area
            SolidBrush rectBrush = NothingBrush;
            if (node.Hovered)
            {
                if (depth > OverBrushes.Length - 1)
                    depth = OverBrushes.Length - 1;

                rectBrush = OverBrushes[depth];
            }

            bool pointBorder = node.AreaF.Width < 3 || node.AreaF.Height < 3;


            buffer.FillRectangle(rectBrush, node.AreaF);

            bool needBorder = true;

            // red hit check if function is hit
            if (XRay.FlowTracking && node.StillInside > 0)
            {
                needBorder = false;

                if (node.EntryPoint > 0)
                {
                    if (XRay.ThreadTracking && node.ConflictHit > 0)
                        buffer.FillRectangle(MultiEntryBrush, node.AreaF);
                    else
                        buffer.FillRectangle(EntryBrush, node.AreaF);
                }
                else
                {
                    if (XRay.ThreadTracking && node.ConflictHit > 0)
                        buffer.FillRectangle(MultiHoldingBrush, node.AreaF);
                    else
                        buffer.FillRectangle(HoldingBrush, node.AreaF);
                }
            }

            // not an else if, draw over holding or entry
            if (node.ExceptionHit > 0)
            {
                needBorder = false;
                buffer.FillRectangle(ExceptionBrush[node.FunctionHit], node.AreaF);
            }

            else if (node.FunctionHit > 0)
            {
                needBorder = false;

                if (XRay.ThreadTracking && node.ConflictHit > 0)
                    buffer.FillRectangle(MultiHitBrush[node.FunctionHit], node.AreaF);
                else if (node.ObjType == XObjType.Field)
                {
                    if (node.LastFieldOp == FieldOp.Set)
                        buffer.FillRectangle(FieldSetBrush[node.FunctionHit], node.AreaF);
                    else
                        buffer.FillRectangle(FieldGetBrush[node.FunctionHit], node.AreaF);
                }
                else
                    buffer.FillRectangle(HitBrush[node.FunctionHit], node.AreaF);
            }

            // if just a point, drawing a border messes up pixels
            if (pointBorder)
            {
                if (FilteredNodes.ContainsKey(node.ID))
                    buffer.FillRectangle(FilteredBrush, node.AreaF);
                else if (IgnoredNodes.ContainsKey(node.ID))
                    buffer.FillRectangle(IgnoredBrush, node.AreaF);

                else if (needBorder) // dont draw the point if already lit up
                    buffer.FillRectangle(ObjBrushes[(int)node.ObjType], node.AreaF);
            }
            else
            {
                Pen pen = null;

                if (FilteredNodes.ContainsKey(node.ID))
                    pen = FilteredPen;
                else if (IgnoredNodes.ContainsKey(node.ID))
                    pen = IgnoredPen;

                else if (FocusedNode == node)
                    pen = ObjFocusedPens[(int)node.ObjType];
                else
                    pen = ObjPens[(int)node.ObjType];

                try
                {
                    buffer.DrawRectangle(pen, node.AreaF.X, node.AreaF.Y, node.AreaF.Width, node.AreaF.Height);
                }
                catch (Exception ex)
                {
                    File.WriteAllText("debug.txt", string.Format("{0}\r\n{1}\r\n{2}\r\n{3}\r\n{4}\r\n", ex, node.AreaF.X, node.AreaF.Y, node.AreaF.Width, node.AreaF.Height));
                }
            }

            // draw label
            if (ShowLabels && node.RoomForLabel)
            {
                buffer.FillRectangle(LabelBgBrush, node.LabelRect);
                buffer.DrawString(node.Name, TextFont, ObjBrushes[(int)node.ObjType], node.LabelRect);
            }

            if (drawChildren && node.AreaF.Width > 1 && node.AreaF.Height > 1)
                foreach (XNodeIn sub in node.Nodes)
                    DrawNode(buffer, sub, depth + 1, drawChildren);


            // after drawing children, draw instance tracking on top of it all
            if (XRay.InstanceTracking && node.ObjType == XObjType.Class)
            {
                /*if (XRay.InstanceCount[node.ID] > 0)
                {
                    string count = XRay.InstanceCount[node.ID].ToString();
                    Rectangle x = new Rectangle(node.Area.Location, buffer.MeasureString(count, InstanceFont).ToSize());

                    if (node.Area.Contains(x))
                    {
                        buffer.FillRectangle(NothingBrush, x);
                        buffer.DrawString(count, InstanceFont, InstanceBrush, node.Area.Location.X + 2, node.Area.Location.Y + 2);
                    }
                }*/
            }
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

        private void TreePanel_MouseMove(object sender, MouseEventArgs e)
        {
            RefreshHovered(e.Location);
        }

        private void RefreshHovered(Point loc)
        {
            ClearHovered();

            if (ViewLayout == LayoutType.TreeMap)
            {
                if (ShowingOutside)
                    TestHovered(InternalRoot, loc);
                if (ShowingExternal)
                    TestHovered(ExternalRoot, loc);

                TestHovered(CurrentRoot, loc);
            }
            else if (ViewLayout == LayoutType.CallGraph)
            {
                foreach(var node in PositionMap.Values)
                    if (node.AreaD.Contains(loc.X, loc.Y))
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
            if (!node.Show || !node.AreaD.Contains(loc.X, loc.Y))
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

        private void TreePanel_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (FocusedNode != null)
                    FocusedNode.Focused = false;

                FocusedNode = GuiHovered.LastOrDefault();

                if (FocusedNode == null)
                    return;
                else
                    FocusedNode.Focused = true;

                Redraw();

                if (FocusedNode.ObjType == XObjType.Class)
                    MainForm.InstanceTab.NavigateTo(FocusedNode);

                MainForm.TimingPanel.NavigateTo(FocusedNode);
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

                    menu.MenuItems.Add( new MenuItem("Zoom", (s, a) => SetRoot(node)) );
                            
                    menu.MenuItems.Add( new MenuItem("Filter", (s, a) => ToggleNode(FilteredNodes, node)) { Checked = selected } );

                    menu.MenuItems.Add( new MenuItem("Ignore", (s, a) => ToggleNode(IgnoredNodes, node)) { Checked = ignored } );
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
                Zoom(false);

            // forward button zoom in
            else if (e.Button == MouseButtons.XButton2)
                Zoom(true);
        }

        void TreePanel_MouseWheel(object sender, MouseEventArgs e)
        {
            Zoom(e.Delta > 0);
        }

        void TreePanel_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right)
                Zoom(true);
            else if (e.KeyCode == Keys.Right)
                Zoom(false);
            else if (e.KeyCode == Keys.Space)
                SetRoot(XRay.RootNode);
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
                if (CurrentRoot.Parent == null)
                    return;

                SetRoot(CurrentRoot.Parent as XNodeIn);
            }

            // put cursor in the same block after the view changes
            XNodeIn last = NodesHovered.LastOrDefault() as XNodeIn;

            if(last != null)
                Cursor.Position = PointToScreen(new Point((int)last.CenterF.X, (int)last.CenterF.Y));
        }

        public void SetRoot(XNodeIn node)
        {
            // setting internal root will auto show properly sized external root area if showing it is enabled
            CurrentRoot = (node == TopRoot) ? InternalRoot : node;

            DoResize = true;
            Refresh();
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
    }
}
