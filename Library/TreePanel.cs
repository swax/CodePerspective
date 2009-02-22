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
        bool Redraw = true;
        Bitmap DisplayBuffer;

        Pen UnknownPen = new Pen(Color.Black);
        Pen NamespacePen = new Pen(Color.Red);
        Pen ClassPen = new Pen(Color.Green);
        Pen MethodPen = new Pen(Color.Blue);
        Pen FieldPen = new Pen(Color.Purple);

        SolidBrush UnknownBrush = new SolidBrush(Color.Black);
        SolidBrush NamespaceBrush = new SolidBrush(Color.Coral);
        SolidBrush ClassBrush = new SolidBrush(Color.PaleGreen);
        SolidBrush MethodBrush = new SolidBrush(Color.LightBlue);
        SolidBrush FieldBrush = new SolidBrush(Color.Purple);

        XNode Root;


        public TreePanel(XNode root)
        {
            InitializeComponent();

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            UpdateMap(root);
        }

        private void TreePanel_Paint(object sender, PaintEventArgs e)
        {
            if (DisplayBuffer == null)
                DisplayBuffer = new Bitmap(Width, Height);

            if (!Redraw)
            {
                e.Graphics.DrawImage(DisplayBuffer, 0, 0);
                return;
            }
            Redraw = false;

            // background
            Graphics buffer = Graphics.FromImage(DisplayBuffer);

            buffer.Clear(Color.White);

            if (Root != null)
            {
                // namespace red, class green, method blue

                DrawNode(buffer, Root, new Rectangle(0, 0, Width, Height));
            }

            // Copy buffer to display
            e.Graphics.DrawImage(DisplayBuffer, 0, 0);
        }

        private void DrawNode(Graphics buffer, XNode root, Rectangle area)
        {
            List<Sector> sectors = new TreeMap().Plot(root.Nodes.Cast<InputValue>().ToList(), area.Size);

            foreach (Sector sector in sectors)
            {
                XNode node = sector.OriginalValue as XNode;

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

                SolidBrush rectBrush = UnknownBrush;

                switch (node.ObjType)
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
                }

                sector.Rect.X += area.X;
                sector.Rect.Y += area.Y;

                sector.Rect = sector.Rect.Contract(4);

                buffer.FillRectangle(rectBrush, sector.Rect);
                buffer.DrawRectangle(rectPen, sector.Rect);

                if(node.Nodes.Count > 0)
                    DrawNode(buffer, node, sector.Rect);
            }
        }

        private void TreePanel_Resize(object sender, EventArgs e)
        {
            if (Width > 0 && Height > 0)
            {
                DisplayBuffer = new Bitmap(Width, Height);

                Redraw = true;
                Invalidate();
            }
        }

        internal void UpdateMap(XNode root)
        {
            Root = root;

            Redraw = true;
            Refresh();
        }
    }
}
