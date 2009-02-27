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

        Pen HighlightPen = new Pen(Color.Black);

        SolidBrush NothingBrush = new SolidBrush(Color.White);
        SolidBrush[] OverBrushes = new SolidBrush[7];

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

            if (!DoRedraw && !DoResize)
            {
                e.Graphics.DrawImage(DisplayBuffer, 0, 0);
                return;
            }

            // background
            Graphics buffer = Graphics.FromImage(DisplayBuffer);

            buffer.Clear(Color.White);

            if (Root != null && DoResize)
                SizeNode(buffer, Root, new Rectangle(0, 0, Width, Height));
            
            if (Root != null)
                DrawNode(buffer, Root, 0);
            
            // Copy buffer to display
            e.Graphics.DrawImage(DisplayBuffer, 0, 0);

            DoRedraw = false;
            DoResize = false;
        }

        private void SizeNode(Graphics buffer, XNodeIn root, Rectangle area)
        {
            root.Area = area;

            if (root.Nodes.Count == 0)
                return;

            List<Sector> sectors = new TreeMap().Plot(root.Nodes.Cast<InputValue>().ToList(), area.Size);

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

            if (node.Selected)
                rectPen = HighlightPen;


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
            Selected.ForEach(n => n.Selected = false);
            Selected.Clear();

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
            if (!node.Area.Contains(loc))
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
            Selected = new List<XNodeIn>();

            DoRedraw= true;
            Invalidate();
        }
    }
}
