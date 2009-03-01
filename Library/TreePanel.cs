using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace XLibrary
{
    public partial class TreePanel : UserControl
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

        SolidBrush UnknownBrush = new SolidBrush(Color.Black);
        SolidBrush NamespaceBrush = new SolidBrush(Color.Coral);
        SolidBrush ClassBrush = new SolidBrush(Color.PaleGreen);
        SolidBrush MethodBrush = new SolidBrush(Color.LightBlue);
        SolidBrush FieldBrush = new SolidBrush(Color.Purple);

        internal XNodeIn Root;

        SolidBrush[] HitBrush;

        int SelectHash;
        List<XNodeIn> Selected = new List<XNodeIn>();


        public TreePanel()
        {
            InitializeComponent();

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            HitBrush = new SolidBrush[XRay.HitFrames];
            for(int i = 0; i < XRay.HitFrames; i++)
            {
                int brightness = 255 / XRay.HitFrames * i;
                HitBrush[i] = new SolidBrush(Color.FromArgb(255, brightness, brightness));
            }

            for (int i = 0; i < OverBrushes.Length; i++)
            {
                int brightness = 255 / OverBrushes.Length * (OverBrushes.Length - i);
                OverBrushes[i] = new SolidBrush(Color.FromArgb(brightness, brightness, 255));
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

        private void SizeNode(Graphics buffer, XNodeIn root, Rectangle area)
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

            buffer.FillRectangle(rectBrush, node.Area);
            

            // red hit check if function is hit
            if(XRay.HitFunctions != null)
                for (int i = 0; i < XRay.HitFrames; i++)
                {
                    int circ = XRay.HitIndex - i;

                    if (circ < 0)
                        circ = XRay.HitFrames + circ;

                    if (XRay.HitFunctions[circ][node.ID])
                    {
                        buffer.FillRectangle(HitBrush[i], node.Area);
                        break;
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

            

            buffer.DrawRectangle(rectPen, node.Area);

            foreach (XNodeIn sub in node.Nodes)
                DrawNode(buffer, sub, depth + 1);
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

        internal void UpdateMap(XNodeIn root)
        {
            Root = root;

            DoRedraw = true;
            Invalidate();
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
                Invalidate();

                if (Selected.Count > 0)
                    MainForm.SelectedLabel.Text = Selected[Selected.Count - 1].GetName();
                else
                    MainForm.SelectedLabel.Text = "";
            }
        }

        private void TestSelected(XNodeIn node, Point loc)
        {
            if (!node.Show || !node.Area.Contains(loc))
                return;

            node.Selected = true;
            Selected.Add(node);

            foreach (XNodeIn sub in node.Nodes)
                TestSelected(sub, loc);
        }

        internal void Redraw()
        {
            DoRedraw = true;
            Invalidate();
        }

        private void TreePanel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (Selected.Count < 2)
                return;

            Root = Selected[1];

            MainForm.UpdateText();

            DoResize = true;
            Invalidate();
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
            Invalidate();
        }

        private void TreePanel_MouseHover(object sender, EventArgs e)
        {

        }

        private void TreePanel_MouseLeave(object sender, EventArgs e)
        {
            ClearSelected();

            DoRedraw= true;
            Invalidate();
        }

        private void ClearSelected()
        {
            Selected.ForEach(n => n.Selected = false);
            Selected.Clear();
        }
    }
}
