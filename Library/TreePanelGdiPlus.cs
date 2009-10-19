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
    public partial class TreePanelGdiPlus : UserControl, ITreePanel
    {
        public MainForm MainForm;

        bool DoRedraw = true;
        bool DoResize = true;
        Bitmap DisplayBuffer;

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

        SolidBrush NothingBrush = new SolidBrush(Color.White);

        SolidBrush EntryBrush = new SolidBrush(Color.LightGreen);
        SolidBrush MultiEntryBrush = new SolidBrush(Color.LimeGreen);

        SolidBrush HoldingBrush = new SolidBrush(Color.FromArgb(255, 255, 192));
        SolidBrush MultiHoldingBrush = new SolidBrush(Color.Yellow);
        
        Color CallColor = Color.Blue;
        Pen ShowCallPen = new Pen(Color.FromArgb(32, Color.Black)) { EndCap = LineCap.ArrowAnchor };
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

        internal XNodeIn Root;

        SolidBrush[] OverBrushes = new SolidBrush[7];

        SolidBrush[] HitBrush;
        SolidBrush[] MultiHitBrush;

        SolidBrush[] ExceptionBrush;
        // no multi exception brush cause we dont know if multiple function calls resulted in an exception or just the 1

        Pen[] CallPen;

        int HoverHash;
        List<XNodeIn> GuiHovered = new List<XNodeIn>();
        XNode[] NodesHovered = new XNodeIn[]{};

        const int DashSize  = 3;
        const int DashSpace = 6;

        XNodeIn FocusedNode;
 

        public TreePanelGdiPlus(MainForm main, XNodeIn root)
        {
            InitializeComponent();

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            MainForm = main;
            Root = root;

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
                CallPen[i].DashPattern = new float[] { DashSize, DashSpace };
                CallPen[i].EndCap = LineCap.ArrowAnchor;
            }

            for (int i = 0; i < OverBrushes.Length; i++)
            {
                int brightness = 255 / OverBrushes.Length * (OverBrushes.Length - i);
                OverBrushes[i] = new SolidBrush(Color.FromArgb(brightness, brightness, 255));
            }

            // set colors of differnt brush / pen arrays
            Dictionary<int, Color> objColors = new Dictionary<int, Color>();

            objColors[(int)XObjType.Root] = UnknownColor;
            objColors[(int)XObjType.File] = FileColor;
            objColors[(int)XObjType.Namespace] = NamespaceColor;
            objColors[(int)XObjType.Class] = ClassColor;
            objColors[(int)XObjType.Field] = FieldColor;
            objColors[(int)XObjType.Method] = MethodColor;

            var objTypes = Enum.GetValues(typeof(XObjType));
           
            ObjBrushes = new SolidBrush[objTypes.Length];
            ObjBrushesDithered = new SolidBrush[objTypes.Length];
            ObjPens = new Pen[objTypes.Length];
            ObjFocused = new Pen[objTypes.Length];

            for (int i = 0; i < objTypes.Length; i++ )
            {
                ObjBrushes[i] = new SolidBrush(objColors[i]);
                ObjBrushesDithered[i] = new SolidBrush(Color.FromArgb(128, objColors[i]));
                ObjPens[i] = new Pen(objColors[i]);
                ObjFocused[i] = new Pen(objColors[i], 3);
            }
        }

        private void TreePanel_Paint(object sender, PaintEventArgs e)
        {
            if (DisplayBuffer == null)
                DisplayBuffer = new Bitmap(Width, Height);

            if ((!DoRedraw && !DoResize) || Root == null)
            {
                e.Graphics.DrawImage(DisplayBuffer, 0, 0);
                return;
            }

            // background
            Graphics buffer = Graphics.FromImage(DisplayBuffer);
            buffer.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed; // todo option to turn this off

            buffer.Clear(Color.White);

            if (XRay.CoverChange)
                RecalcCover(Root);

            if (DoResize || XRay.CoverChange)
            {
                Root.SetArea(new RectangleD(0, 0, Width, Height));

                PositionMap.Clear();
                PositionMap[Root.ID] = Root;

                SizeNode(buffer, Root);
            }

            DrawNode(buffer, Root, 0);

            // draw flow
            if (XRay.FlowTracking)
            {
                for (int i = 0; i < XRay.CallMap.Length; i++)
                {
                    FunctionCall call = XRay.CallMap.Values[i];

                    if (call != null && 
                        (XRay.ShowAllCalls || call.Hit > 0 || call.StillInside > 0) &&
                        PositionMap.ContainsKey(call.Source) &&
                        PositionMap.ContainsKey(call.Destination))
                    {

                        if (call.StillInside > 0)
                            buffer.DrawLine(HoldingCallPen, PositionMap[call.Source].CenterF, PositionMap[call.Destination].CenterF );
                        else if (XRay.ShowAllCalls)
                        {
                            //buffer.DrawLine(ShowCallPen, PositionMap[call.Source].CenterF, PositionMap[call.Destination].CenterF);

                            PointF start = PositionMap[call.Source].CenterF;
                            PointF end  = PositionMap[call.Destination].CenterF;
                            PointF mid = new PointF(start.X + (end.X - start.X) / 2, start.Y + (end.Y - start.Y) / 2);

                            buffer.DrawLine(ShowCallOutPen, start, mid);
                            buffer.DrawLine(ShowCallInPen, mid, end);
                        }
                        if (call.Hit > 0)
                        {
                            Pen pen = CallPen[call.Hit];

                            call.DashOffset -= DashSize;
                            if (call.DashOffset < 0)
                                call.DashOffset = DashSpace;

                            pen.DashOffset = call.DashOffset;
                            buffer.DrawLine(pen, PositionMap[call.Source].CenterF, PositionMap[call.Destination].CenterF );
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

        private int RecalcCover(XNodeIn root)
        {
            // only leaves have real value
            root.Value = (root.Nodes.Count == 0) ? root.Lines : 0;

            foreach (XNodeIn node in root.Nodes)
            {
                node.Show = (!XRay.ShowOnlyHit || XRay.CoveredFunctions[node.ID]);

                if (node.Show)
                    root.Value += RecalcCover(node);
            }

            return root.Value;                
        }

        const int Border = 4;

        private void SizeNode(Graphics buffer, XNodeIn root)
        {
            if (!root.Show)
                return;

            var nodes = root.Nodes.Cast<XNodeIn>()
                            .Where(n => n.Show)
                            .Select(n => n as InputValue);

            List<Sector> sectors = new TreeMap(nodes, root.AreaD.Size).Results;

            foreach (Sector sector in sectors)
            {
                XNodeIn node = sector.OriginalValue as XNodeIn;

                sector.Rect.Contract(Border);

                if (sector.Rect.X < Border) sector.Rect.X = Border;
                if (sector.Rect.Y < Border) sector.Rect.Y = Border;
                if (sector.Rect.X > root.AreaF.Width - Border) sector.Rect.X = root.AreaF.Width - Border;
                if (sector.Rect.Y > root.AreaF.Height - Border) sector.Rect.Y = root.AreaF.Height - Border;

                sector.Rect.X += root.AreaF.X;
                sector.Rect.Y += root.AreaF.Y;

                node.SetArea(sector.Rect);
                PositionMap[node.ID] = node;

                if(sector.Rect.Width > 1 && sector.Rect.Height > 1)
                    SizeNode(buffer, node);
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
                 if (needBorder) // dont draw the point if already lit up
                    buffer.FillRectangle(ObjBrushes[(int)node.ObjType], node.AreaF);
            }
            else if(FocusedNode == node)
                buffer.DrawRectangle(ObjFocused[(int)node.ObjType], node.AreaF.X, node.AreaF.Y, node.AreaF.Width, node.AreaF.Height);
            else
                buffer.DrawRectangle(ObjPens[(int)node.ObjType], node.AreaF.X, node.AreaF.Y, node.AreaF.Width, node.AreaF.Height);


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

            TestHovered(Root, loc);

            int hash = 0;
            GuiHovered.ForEach(n => hash = n.ID ^ hash);

            if (hash != HoverHash)
            {
                HoverHash = hash;
                DoRedraw = true;
                Invalidate();

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

        private void TreePanel_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void TreePanel_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                FocusedNode = GuiHovered.LastOrDefault();

                Invalidate();
            }
            else if (e.Button == MouseButtons.Right)
            {
                /*if (Root.Parent == null)
                    return;

                Root = Root.Parent as XNodeIn;

                MainForm.UpdateText();

                DoResize = true;
                Invalidate();*/
            }
            else if (e.Button == MouseButtons.XButton1)
                Zoom(false);
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
                if (Root.Parent == null)
                    return;

                SetRoot(Root.Parent as XNodeIn);
            }

            // put cursor in the same block after the view changes
            XNodeIn last = NodesHovered.LastOrDefault() as XNodeIn;

            if(last != null)
                Cursor.Position = PointToScreen(new Point((int)last.CenterF.X, (int)last.CenterF.Y));
        }

        void SetRoot(XNodeIn node)
        {
            Root = node;

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
            return Root;
        }

        public void Dispose2()
        {
        }
    }
}
