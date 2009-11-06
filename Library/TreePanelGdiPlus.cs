using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace XLibrary
{
    public partial class TreePanelGdiPlus : UserControl
    {
        public MainForm MainForm;

        bool DoRedraw = true;
        bool DoResize = true;
        Bitmap DisplayBuffer;

        internal bool ShowOutside;
        internal bool ShowExternal;
        bool ShowingOutside;
        bool ShowingExternal;

        internal bool ShowLabels = true;
        float LabelPadding = 2;

        internal bool ShowCalls = true;

        Color UnknownColor = Color.Black;
        Color FileColor = Color.Black;
        Color NamespaceColor = Color.DarkBlue;
        Color ClassColor = Color.DarkGreen;
        Color MethodColor = Color.DarkRed;
        Color FieldColor = Color.Brown;

        SolidBrush[] ObjBrushes;
        SolidBrush[] ObjBrushesDithered;
        Pen[] ObjPens;
        Pen[] ObjFocused;

        internal static Dictionary<int, Color> ObjColors = new Dictionary<int, Color>();


        SolidBrush NothingBrush = new SolidBrush(Color.White);

        SolidBrush EntryBrush = new SolidBrush(Color.LightGreen);
        SolidBrush MultiEntryBrush = new SolidBrush(Color.LimeGreen);

        SolidBrush HoldingBrush = new SolidBrush(Color.FromArgb(255, 255, 192));
        SolidBrush MultiHoldingBrush = new SolidBrush(Color.Yellow);
        
        Color CallColor = Color.Blue;
        Pen ShowCallPen = new Pen(Color.FromArgb(32, Color.Black));// { EndCap = LineCap.ArrowAnchor };
        Pen ShowCallOutPen = new Pen(Color.FromArgb(32, Color.Red));
        Pen ShowCallInPen = new Pen(Color.FromArgb(32, Color.Blue));
        Pen HoldingCallPen = new Pen(Color.FromArgb(32, Color.Blue)) { EndCap = LineCap.ArrowAnchor };
        

        Color HitColor = Color.FromArgb(255, 192, 128);
        Color MultiHitColor = Color.Orange;

        Color ExceptionColor = Color.Red;
        Color MultiExceptionColor = Color.DarkRed;

        SolidBrush TextBrush = new SolidBrush(Color.Black);
        SolidBrush TextBgBrush = new SolidBrush(Color.FromArgb(192, Color.White));
        Font TextFont = new Font("tahoma", 9, FontStyle.Bold );

        Font InstanceFont = new Font("tahoma", 11, FontStyle.Bold);
        SolidBrush InstanceBrush = new SolidBrush(Color.Black);

        Dictionary<int, XNodeIn> PositionMap = new Dictionary<int, XNodeIn>();
        Dictionary<int, XNodeIn> CenterMap = new Dictionary<int, XNodeIn>();

        internal XNodeIn CurrentRoot;
        XNodeIn InternalRoot;
        XNodeIn ExternalRoot;
        XNodeIn TopRoot;        

        SolidBrush[] OverBrushes = new SolidBrush[7];

        SolidBrush[] HitBrush;
        SolidBrush[] MultiHitBrush;

        SolidBrush[] ExceptionBrush;
        // no multi exception brush cause we dont know if multiple function calls resulted in an exception or just the 1

        Pen[] CallPen;

        int HoverHash;
        List<XNodeIn> GuiHovered = new List<XNodeIn>();
        XNode[] NodesHovered = new XNodeIn[]{};

        XNodeIn FocusedNode;

        Pen SelectedPen = new Pen(Color.LimeGreen, 3);
        SolidBrush SelectedBrush = new SolidBrush(Color.LimeGreen);
        Dictionary<int, XNodeIn> SelectedNodes = new Dictionary<int, XNodeIn>();

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

            CallPen = new Pen[XRay.HitFrames];

            for (int i = 0; i < XRay.HitFrames; i++)
            {
                int brightness = 255 - (255 / XRay.HitFrames * i);

                HitBrush[i] = new SolidBrush(Color.FromArgb(255 - brightness, HitColor));
                MultiHitBrush[i] = new SolidBrush(Color.FromArgb(255 - brightness, MultiHitColor));
                ExceptionBrush[i] = new SolidBrush(Color.FromArgb(255 - brightness, ExceptionColor));

                CallPen[i] = new Pen(Color.FromArgb(255 - brightness, CallColor));
                CallPen[i].DashPattern = new float[] { FunctionCall.DashSize, FunctionCall.DashSpace };
                CallPen[i].EndCap = LineCap.ArrowAnchor;
            }

            for (int i = 0; i < OverBrushes.Length; i++)
            {
                int brightness = 255 / (OverBrushes.Length  + 1) * (OverBrushes.Length - i);
                OverBrushes[i] = new SolidBrush(Color.FromArgb(brightness, brightness, 255));
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
            ObjBrushesDithered = new SolidBrush[objTypes.Length];
            ObjPens = new Pen[objTypes.Length];
            ObjFocused = new Pen[objTypes.Length];

            for (int i = 0; i < objTypes.Length; i++ )
            {
                ObjBrushes[i] = new SolidBrush(ObjColors[i]);
                ObjBrushesDithered[i] = new SolidBrush(Color.FromArgb(128, ObjColors[i]));
                ObjPens[i] = new Pen(ObjColors[i]);
                ObjFocused[i] = new Pen(ObjColors[i], 3);
            }
        }

        private void TreePanel_Paint(object sender, PaintEventArgs e)
        {
            if (DisplayBuffer == null)
                DisplayBuffer = new Bitmap(Width, Height);

            if ((!DoRedraw && !DoResize) || CurrentRoot == null)
            {
                e.Graphics.DrawImage(DisplayBuffer, 0, 0);
                return;
            }

            // background
            Graphics buffer = Graphics.FromImage(DisplayBuffer);
            buffer.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed; // todo option to turn this off

            buffer.Clear(Color.White);

            if (XRay.CoverChange)
            {
                RecalcCover(InternalRoot);
                RecalcCover(ExternalRoot);
            }

            Debug.Assert(CurrentRoot != TopRoot); // current root should be intenalRoot in this case

            ShowingOutside = ShowOutside && CurrentRoot != InternalRoot;
            ShowingExternal = ShowExternal && !CurrentRoot.External;

            if (DoResize || XRay.CoverChange)
            {
                int offset = 0;
                int centerWidth = Width;

                PositionMap.Clear();
                CenterMap.Clear();

                if (ShowingOutside)
                {
                    offset = Width * 1 / 4;
                    centerWidth -= offset;

                    InternalRoot.SetArea(new RectangleD(0, 0, offset, Height));
                    PositionMap[InternalRoot.ID] = InternalRoot;
                    SizeNode(buffer, InternalRoot, CurrentRoot, false);
                }
                if (ShowingExternal)
                {
                    int extWidth = Width * 1 / 4;
                    centerWidth -= extWidth;

                    ExternalRoot.SetArea(new RectangleD(offset + centerWidth, 0, extWidth, Height));
                    PositionMap[ExternalRoot.ID] = ExternalRoot;
                    SizeNode(buffer, ExternalRoot, null, false);
                }

                CurrentRoot.SetArea(new RectangleD(offset, 0, centerWidth, Height));
                PositionMap[CurrentRoot.ID] = CurrentRoot;
                SizeNode(buffer, CurrentRoot, null, true);
            }

            if (ShowingOutside)
                DrawNode(buffer, InternalRoot, 0);

            if (ShowingExternal)
                DrawNode(buffer, ExternalRoot, 0);    

            DrawNode(buffer, CurrentRoot, 0);

            // draw ignored over nodes ignored may contain
            foreach (XNodeIn ignored in IgnoredNodes.Values)
                if (PositionMap.ContainsKey(ignored.ID))
                {
                    buffer.DrawLine(IgnoredPen, ignored.AreaF.UpperLeftCorner(), ignored.AreaF.LowerRightCorner() );
                    buffer.DrawLine(IgnoredPen, ignored.AreaF.UpperRightCorner(), ignored.AreaF.LowerLeftCorner());
                }

            // draw flow
            if (XRay.FlowTracking)
            {
                for (int i = 0; i < XRay.CallMap.Length; i++)
                {
                    FunctionCall call = XRay.CallMap.Values[i];

                    if (call != null && 
                        (XRay.ShowAllCalls || call.Hit > 0 || call.StillInside > 0) &&
                        (CenterMap.ContainsKey(call.Source) || CenterMap.ContainsKey(call.Destination)) &&
                        PositionMap.ContainsKey(call.Source) &&
                        PositionMap.ContainsKey(call.Destination))
                    {
                        XNodeIn source = PositionMap[call.Source];
                        XNodeIn destination = PositionMap[call.Destination];

                        // if there are items we're filtering on then only show calls to those nodes
                        if (SelectedNodes.Count > 0)
                            if (!IsNodeFiltered(true, source) && !IsNodeFiltered(true, destination))
                                continue;

                        // do after select filter so we can have ignored nodes inside selected, but not the otherway around
                        if (IgnoredNodes.Count > 0)
                            if (IsNodeFiltered(false, source) || IsNodeFiltered(false, destination))
                                continue;

                        if (call.StillInside > 0 && ShowCalls)
                            buffer.DrawLine(HoldingCallPen, source.CenterF, destination.CenterF );

                        else if (XRay.ShowAllCalls)
                        {
                            //buffer.DrawLine(ShowCallPen, PositionMap[call.Source].CenterF, PositionMap[call.Destination].CenterF);

                            PointF start = PositionMap[call.Source].CenterF;
                            PointF end  = PositionMap[call.Destination].CenterF;
                            PointF mid = new PointF(start.X + (end.X - start.X) / 2, start.Y + (end.Y - start.Y) / 2);

                            buffer.DrawLine(ShowCallOutPen, start, mid);
                            buffer.DrawLine(ShowCallInPen, mid, end);
                        }

                        if (call.Hit > 0 && ShowCalls)
                        {
                            Pen pen = CallPen[call.Hit];
                            pen.DashOffset = call.DashOffset;
                            buffer.DrawLine(pen, source.CenterF, destination.CenterF);
                        }    
                    }
                }
            }

            // draw mouse over label
            PointF pos = PointToClient(Cursor.Position);
            if (NodesHovered.Length > 0 && ClientRectangle.Contains((int)pos.X, (int)pos.Y))
            {
                // for each node selected, get size, figure out bg size and indents, then pass again and draw

                float bgWidth = 0;
                float bgHeight = 0;
                float lineHeight = 0;

                const float indent = 5;
                float indentAmount = 0;

                // find the size of the background box
                foreach (XNode node in NodesHovered)
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

                foreach (XNodeIn node in NodesHovered)
                {
                    // dither label if it is not on screen
                    if(GuiHovered.Contains(node))
                        buffer.DrawString(node.Name, TextFont, ObjBrushes[(int)node.ObjType], pos.X, pos.Y);
                    else
                        buffer.DrawString(node.Name, TextFont, ObjBrushesDithered[(int)node.ObjType], pos.X, pos.Y);

                    pos.Y += lineHeight;
                    pos.X += indent;
                }
            }

            // Copy buffer to display
            e.Graphics.DrawImage(DisplayBuffer, 0, 0);

            DoRedraw = false;
            DoResize = false;
            XRay.CoverChange = false;
        }

        private bool IsNodeFiltered(bool select, XNodeIn node)
        {
            Dictionary<int, XNodeIn> map = select ? SelectedNodes : IgnoredNodes;

            foreach (XNodeIn parent in node.GetParents())
                if (map.ContainsKey(parent.ID))
                    return true;

            return false;
        }

        private int RecalcCover(XNodeIn root)
        {
            // only leaves have real value
            root.Value = (root.Nodes.Count == 0) ? root.Lines : 0;

            foreach (XNodeIn node in root.Nodes)
            {
                node.Show = node.ObjType != XObjType.Method ||
                    XRay.ShowHit == ShowHitMode.All ||
                    (XRay.ShowHit == ShowHitMode.Hit && XRay.CoveredFunctions[node.ID]) ||
                    (XRay.ShowHit == ShowHitMode.Unhit && !XRay.CoveredFunctions[node.ID]);
                
                if (node.Show)
                    root.Value += RecalcCover(node);
            }

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
                SizeF labelSize = buffer.MeasureString(root.Name, TextFont);
      
                float minHeight = (root.Nodes.Count > 0) ? labelSize.Height * 2 : labelSize.Height;

                if (root.AreaF.Height > minHeight && root.AreaF.Width > labelSize.Width )
                {
                    insideArea.Y += labelSize.Height;
                    insideArea.Height -= labelSize.Height;
                    root.RoomForLabel = true;
                }
                else
                    root.RoomForLabel = false;
            }

            var nodes = from n in root.Nodes.Cast<XNodeIn>()
                        where n.Show && n != exclude
                        select n as InputValue;

            List<Sector> sectors = new TreeMap(nodes, insideArea.Size).Results;

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

                if (center)
                    CenterMap[node.ID] = node;

                if(sector.Rect.Width > 1 && sector.Rect.Height > 1)
                    SizeNode(buffer, node, exclude, center);
            }
        }

        private void DrawNode(Graphics buffer, XNodeIn node, int depth)
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
                else
                    buffer.FillRectangle(HitBrush[node.FunctionHit], node.AreaF);
            }

            // if just a point, drawing a border messes up pixels
            if (pointBorder)
            {
                if (SelectedNodes.ContainsKey(node.ID))
                    buffer.FillRectangle(SelectedBrush, node.AreaF);
                else if (IgnoredNodes.ContainsKey(node.ID))
                    buffer.FillRectangle(IgnoredBrush, node.AreaF);

                else  if (needBorder) // dont draw the point if already lit up
                    buffer.FillRectangle(ObjBrushes[(int)node.ObjType], node.AreaF);
            }
            else
            {
                Pen pen = null;

                if (SelectedNodes.ContainsKey(node.ID))
                    pen = SelectedPen;
                else if (IgnoredNodes.ContainsKey(node.ID))
                    pen = IgnoredPen;

                else if (FocusedNode == node)
                    pen = ObjFocused[(int)node.ObjType];
                else
                    pen = ObjPens[(int)node.ObjType];

                buffer.DrawRectangle(pen, node.AreaF.X, node.AreaF.Y, node.AreaF.Width, node.AreaF.Height);
            }

            // draw label
            if (ShowLabels && node.RoomForLabel)
                buffer.DrawString(node.Name, TextFont, ObjBrushes[(int)node.ObjType], node.AreaF.X + LabelPadding, node.AreaF.Y + LabelPadding);

            if (node.AreaF.Width > 1 && node.AreaF.Height > 1)
                foreach (XNodeIn sub in node.Nodes)
                    DrawNode(buffer, sub, depth + 1);


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

            if (ShowingOutside)
                TestHovered(InternalRoot, loc);
            if (ShowingExternal)
                TestHovered(ExternalRoot, loc);

            TestHovered(CurrentRoot, loc);

            int hash = 0;
            GuiHovered.ForEach(n => hash = n.ID ^ hash);

            if (hash != HoverHash)
            {
                HoverHash = hash;
                Redraw();

                if (GuiHovered.Count > 0)
                {
                    NodesHovered = GuiHovered.Last().GetParents();
                    MainForm.SelectedLabel.Text = GuiHovered.Last().FullName();
                }
                else
                {
                    NodesHovered = new XNodeIn[] { };
                    MainForm.SelectedLabel.Text = "";
                }
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

        private void TreePanel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            XNodeIn node = GuiHovered.LastOrDefault();
            if (node == null)
                return;

            new DetailsForm(node).Show();
        }

        void ToggleNode(Dictionary<int, XNodeIn> map, XNodeIn node)
        {
            // make sure a node cant be selected and ignored simultaneously
            if (map != IgnoredNodes && IgnoredNodes.ContainsKey(node.ID))
                IgnoredNodes.Remove(node.ID);

            if (map != SelectedNodes && SelectedNodes.ContainsKey(node.ID))
                SelectedNodes.Remove(node.ID);

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
                FocusedNode = GuiHovered.LastOrDefault();

                Redraw();
            }
            else if (e.Button == MouseButtons.Right)
            {
                ContextMenu menu = new ContextMenu();

                string indent = "";
                foreach (XNodeIn node in GuiHovered)
                {
                    bool selected = SelectedNodes.ContainsKey(node.ID);
                    bool ignored = IgnoredNodes.ContainsKey(node.ID);

                    menu.MenuItems.Add(new MenuItem(indent + node.Name, new MenuItem[] 
                    {
                        new MenuItem("Details", (s, a) =>
                            new DetailsForm(node).Show()),

                        new MenuItem("Zoom", (s, a) =>
                            SetRoot(node)),
                            
                        new MenuItem("-"),

                        new MenuItem("Filter", (s, a) =>
                            ToggleNode(SelectedNodes, node)) { Checked = selected },

                        new MenuItem("Ignore", (s, a) =>
                            ToggleNode(IgnoredNodes, node)) { Checked = ignored }
                    }));

                    indent += "  ";
                }

                if (SelectedNodes.Count > 0 || IgnoredNodes.Count > 0)
                {
                    menu.MenuItems.Add("-");

                    menu.MenuItems.Add(new MenuItem("Clear Filtering", (s, a) =>
                    {
                        SelectedNodes.Clear();
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

        void SetRoot(XNodeIn node)
        {
            // setting internal root will auto show properly sized external root area if showing it is enabled
            CurrentRoot = (node == TopRoot) ? InternalRoot : node;

            MainForm.UpdateText();

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

        public XNodeIn GetRoot()
        {
            return CurrentRoot;
        }

        public void Dispose2()
        {
        }
    }
}
