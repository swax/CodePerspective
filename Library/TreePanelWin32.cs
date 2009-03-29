using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace XLibrary
{
    public partial class TreePanelWin32 : UserControl, ITreePanel
    {
        public TreeForm MainForm;

        bool DoRedraw = true;
        bool DoResize = true;
        Bitmap DisplayBuffer;

        /*Pen UnknownPen = new Pen(Color.Black);
        Pen NamespacePen = new Pen(Color.Red);
        Pen ClassPen = new Pen(Color.Green);
        Pen MethodPen = new Pen(Color.Blue);
        Pen FieldPen = new Pen(Color.Purple);*/


        Pen UnknownPen = new Pen(Color.Black);
        Pen NamespacePen = new Pen(Color.DarkBlue);
        Pen ClassPen = new Pen(Color.DarkGreen);
        Pen MethodPen = new Pen(Color.DarkRed);
        Pen FieldPen = new Pen(Color.Black);

        SolidBrush NothingBrush = new SolidBrush(Color.White);
        SolidBrush[] OverBrushes = new SolidBrush[7];

        SolidBrush TextBrush = new SolidBrush(Color.Black);
        SolidBrush TextBgBrush = new SolidBrush(Color.FromArgb(192, Color.White));
        Font TextFont = new Font("tahoma", 9, FontStyle.Bold);

        Font InstanceFont = new Font("tahoma", 11, FontStyle.Bold);
        SolidBrush InstanceBrush = new SolidBrush(Color.Black);

        SolidBrush UnknownBrush = new SolidBrush(Color.Black);
        SolidBrush NamespaceBrush = new SolidBrush(Color.Coral);
        SolidBrush ClassBrush = new SolidBrush(Color.PaleGreen);
        SolidBrush MethodBrush = new SolidBrush(Color.LightBlue);
        SolidBrush FieldBrush = new SolidBrush(Color.Purple);

        internal XNodeIn Root;

        SolidBrush[] HitBrush;
        SolidBrush[] ConflictBrush;

        int SelectHash;
        List<XNodeIn> Selected = new List<XNodeIn>();


        public TreePanelWin32(TreeForm main, XNodeIn root)
        {
            InitializeComponent();

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            MainForm = main;
            Root = root;

            HitBrush = new SolidBrush[XRay.HitFrames];
            ConflictBrush = new SolidBrush[XRay.HitFrames];

            for(int i = 0; i < XRay.HitFrames; i++)
            {
                int brightness = 255 - (255 / XRay.HitFrames * i);
                HitBrush[i] = new SolidBrush(Color.FromArgb(255, brightness, 255, brightness));
                ConflictBrush[i] = new SolidBrush(Color.FromArgb(255, 255, brightness, brightness));
            }

            for (int i = 0; i < OverBrushes.Length; i++)
            {
                int brightness = 255 / OverBrushes.Length * (OverBrushes.Length - i);
                OverBrushes[i] = new SolidBrush(Color.FromArgb(brightness, brightness, 255));
            }

            DrawThread = new Thread(RunDrawing);
            DrawThread.Start();
        }

        internal AutoResetEvent DrawEvent = new AutoResetEvent(false);
        bool StopDrawing;
        Thread DrawThread;

        int ColorBug = System.Drawing.ColorTranslator.ToOle(Color.LimeGreen);
        int ColorOpt = System.Drawing.ColorTranslator.ToOle(Color.Orange);

        void RunDrawing()
        {
            Bitmap background = new Bitmap(10, 10);

            IntPtr mainDC;
            IntPtr memDC;
            IntPtr tempDC;
            IntPtr offscreenBmp;
            IntPtr oldBmp;
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr prevBmp;

            IntPtr bugPen = Win32.CreatePen(Win32.PenStyles.PS_SOLID, 1, ColorBug);
            IntPtr bugBrush = Win32.CreateSolidBrush(ColorBug); ;

            IntPtr optPen = Win32.CreatePen(Win32.PenStyles.PS_SOLID, 1, ColorOpt);
            IntPtr optBrush = Win32.CreateSolidBrush(ColorOpt);

            IntPtr oldPen;
            IntPtr oldBrush;


            while (true)
            {
                DrawEvent.WaitOne();

                if (StopDrawing)
                    break;

                /*if (background.Width != Width || background.Height != Height)
                {
                    background = new Bitmap(Width, Height);

                    if (hBitmap != IntPtr.Zero)
                    {
                        Win32.DeleteObject(hBitmap);
                        hBitmap = IntPtr.Zero;
                    }

                    hBitmap = background.GetHbitmap();
                }*/


                // start drawing
                Graphics mainGraphics = CreateGraphics();
                mainDC = mainGraphics.GetHdc();

                // create memeory DC and select an offscreen bmp into it
                memDC = Win32.CreateCompatibleDC(mainDC);
                offscreenBmp = Win32.CreateCompatibleBitmap(mainDC, Width, Height);
                oldBmp = Win32.SelectObject(memDC, offscreenBmp);

                /*tempDC = Win32.CreateCompatibleDC(mainDC);
                prevBmp = Win32.SelectObject(tempDC, hBitmap);
                Win32.StretchBlt(memDC, 0, 0, Width, Height, tempDC, 0, 0, background.Width, background.Height, (int)Win32.SRCCOPY);
                Win32.SelectObject(tempDC, prevBmp);
                Win32.DeleteDC(tempDC);*/


                // draw global optimum
                oldPen = Win32.SelectObject(memDC, optPen);
                oldBrush = Win32.SelectObject(memDC, optBrush);

                Win32.Rectangle(memDC, 5, 5, Width - 100, Height - 100);

                Win32.SelectObject(memDC, oldPen);
                Win32.SelectObject(memDC, oldBrush);


                // copy to main screen
                Win32.BitBlt(mainDC, 0, 0, Width, Height, memDC, 0, 0, Win32.TernaryRasterOperations.SRCCOPY);


                // release objects
                Win32.SelectObject(memDC, oldBmp);
                Win32.DeleteObject(offscreenBmp);

                Win32.DeleteDC(memDC);
                mainGraphics.ReleaseHdc(mainDC);
            }

            if (hBitmap != IntPtr.Zero)
            {
                Win32.DeleteObject(hBitmap);
                hBitmap = IntPtr.Zero;
            }
        }

        public void Dispose2()
        {
            StopDrawing = true;

            DrawEvent.Set();

            DrawThread.Join(5000);
        }

        private void TreePanel_Paint(object sender, PaintEventArgs e)
        {
            /*if (DisplayBuffer == null)
                DisplayBuffer = new Bitmap(Width, Height);

            if ((!DoRedraw && !DoResize) || Root == null)
            {
                e.Graphics.DrawImage(DisplayBuffer, 0, 0);
                return;
            }

            // background
            Graphics buffer = Graphics.FromImage(DisplayBuffer);

            buffer.Clear(Color.White);

            if (XRay.CoverChange)
                RecalcCover(Root);
                
            if (DoResize || XRay.CoverChange)
                SizeNode(buffer, Root, new Rectangle(0, 0, Width, Height));

            DrawNode(buffer, Root, 0);

            // draw mouse over label
            Point pos = PointToClient(Cursor.Position);
            if (ClientRectangle.Contains(pos))
            {
                SizeF size = buffer.MeasureString(MainForm.SelectedLabel.Text, TextFont);

                pos.Y -= (int)size.Height;

                if (pos.X + size.Width > Width) pos.X = (int)(Width - size.Width);
                if (pos.Y < 0) pos.Y = 0;

                buffer.FillRectangle(TextBgBrush, pos.X, pos.Y, size.Width, size.Height);
                buffer.DrawString(MainForm.SelectedLabel.Text, TextFont, TextBrush, pos);
            }

            // Copy buffer to display
            e.Graphics.DrawImage(DisplayBuffer, 0, 0);

            DoRedraw = false;
            DoResize = false;
            XRay.CoverChange = false;*/
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

        private void SizeNode(Graphics buffer, XNodeIn root, RectangleD area)
        {
            if (!root.Show)
                return;

            root.Area = area;

            var nodes = root.Nodes.Cast<XNodeIn>()
                            .Where(n => n.Show)
                            .Select(n => n as InputValue);

            List<Sector> sectors = new TreeMap().Plot(nodes, area.Size);

            foreach (Sector sector in sectors)
            {
                XNodeIn node = sector.OriginalValue as XNodeIn;

                sector.Rect.X += area.X;
                sector.Rect.Y += area.Y;

                SizeNode(buffer, node, sector.Rect.Contract(4));
            }
        }

        private void DrawNode(Graphics buffer, XNodeIn node, int depth)
        {
            if (!node.Show)
                return;

            Pen rectPen = UnknownPen;

            switch (node.ObjType)
            {
                case XObjType.Namespace:
                    rectPen = NamespacePen;
                    break;

                case XObjType.Class:
                    rectPen = ClassPen;
                    break;

                case XObjType.Method:
                    rectPen = MethodPen;
                    break;

                case XObjType.Field:
                    rectPen = FieldPen;
                    break;
            }

            // blue selection area
            SolidBrush rectBrush = NothingBrush;
            if (node.Selected)
            {
                if (depth > OverBrushes.Length - 1)
                    depth = OverBrushes.Length - 1;

                rectBrush = OverBrushes[depth];
            }

            RectangleF area = node.Area.ToRectangleF();

            buffer.FillRectangle(rectBrush, area);
            
            // red hit check if function is hit
            if(XRay.HitFunctions != null)
            {
                int value = XRay.Conflicts[node.ID];
                if (value > 0)
                    buffer.FillRectangle(ConflictBrush[value], area);
                else
                {
                    value = XRay.HitFunctions[node.ID];
                    if (value > 0)
                        buffer.FillRectangle(HitBrush[value], area);
                }
            }



            /*switch (node.ObjType)
            {
                case XObjType.Namespace:
                    rectBrush = NamespaceBrush;
                    break;

                case XObjType.Class:
                    rectBrush = ClassBrush;
                    break;

                case XObjType.Method:
                    rectBrush = MethodBrush;
                    break;

                case XObjType.Field:
                    rectBrush = FieldBrush;
                    break;
            }*/

            

            buffer.DrawRectangle(rectPen, area.X, area.Y, area.Width, area.Height);

            foreach (XNodeIn sub in node.Nodes)
                DrawNode(buffer, sub, depth + 1);

            // after drawing children, draw instance tracking on top of it all
            if (XRay.TrackInstances && node.ObjType == XObjType.Class)
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
                DrawEvent.Set();
            }
        }

        private void TreePanel_MouseMove(object sender, MouseEventArgs e)
        {
            ClearSelected();

            TestSelected(Root, e.Location);

            int hash = 0;
            Selected.ForEach(n => hash = n.ID ^ hash);

            if(hash != SelectHash)
            {
                SelectHash = hash;
                DoRedraw = true;
                DrawEvent.Set();

                if (Selected.Count > 0)
                    MainForm.SelectedLabel.Text = Selected[Selected.Count - 1].GetName();
                else
                    MainForm.SelectedLabel.Text = "";
            }
        }

        private void TestSelected(XNodeIn node, Point loc)
        {
            if (!node.Show || !node.Area.Contains(loc.X, loc.Y))
                return;

            node.Selected = true;
            Selected.Add(node);

            foreach (XNodeIn sub in node.Nodes)
                TestSelected(sub, loc);
        }

        private void TreePanel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (Selected.Count < 2)
                return;

            Root = Selected[1];

            MainForm.UpdateText();

            DoResize = true;
            DrawEvent.Set();
        }

        private void TreePanel_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            if (Root.Parent == null)
                return;

            Root = Root.Parent as XNodeIn;

            MainForm.UpdateText();

            DoResize = true;
            DrawEvent.Set();
        }

        private void TreePanel_MouseLeave(object sender, EventArgs e)
        {
            ClearSelected();
        }

        private void ClearSelected()
        {
            Selected.ForEach(n => n.Selected = false);
            Selected.Clear();
        }

        public void Redraw()
        {
            DoRedraw = true;
            DrawEvent.Set();
        }

        public XNodeIn GetRoot()
        {
            return Root;
        }
    }
}
