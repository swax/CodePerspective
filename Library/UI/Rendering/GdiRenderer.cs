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
    public partial class GdiRenderer : UserControl, IRenderer
    {
        public ViewModel Model;

        Bitmap DisplayBuffer;

        StringFormat LabelFormat = new StringFormat();

        Graphics CurrentBuffer;

        Dictionary<Color, SolidBrush> BrushCache = new Dictionary<Color, SolidBrush>();
        Dictionary<int, Pen> PenCache = new Dictionary<int, Pen>();



        public GdiRenderer(ViewModel model)
        {
            InitializeComponent();

            MouseWheel += GdiRenderer_MouseWheel;

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            Model = model;

            LabelFormat.Trimming = StringTrimming.EllipsisCharacter;
            LabelFormat.FormatFlags |= StringFormatFlags.NoWrap;
        }

        public void Start()
        {
            Model.TwoDimensionalValues = false;
        }

        public void Stop()
        {

        }

        public float ViewWidth { get { return Width; } }

        public float ViewHeight { get { return Height; } }

        private void GdiRenderer_Paint(object sender, PaintEventArgs e)
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

            buffer.Clear(Model.XColors.BackgroundColor);

            Debug.Assert(Model.CurrentRoot != Model.TopRoot); // current root should be intenalRoot in this case

            CurrentBuffer = buffer;

            Model.Render();

            // Copy buffer to display
            e.Graphics.DrawImage(DisplayBuffer, 0, 0);
            Model.FpsCount++;

            Model.DoRedraw = false;
            Model.RedrawCount++;
        }

        private void GdiRenderer_Resize(object sender, EventArgs e)
        {
            if (Width > 0 && Height > 0)
            {
                DisplayBuffer = new Bitmap(Width, Height);

                Model.DoResize = true;
                Invalidate();
            }
        }

        void GdiRenderer_MouseWheel(object sender, MouseEventArgs e)
        {
            Model.View_MouseWheel(e);
        }

        private void GdiRenderer_MouseDown(object sender, MouseEventArgs e)
        {
            Model.View_MouseDown(e);
        }

        private void GdiRenderer_MouseUp(object sender, MouseEventArgs e)
        {
            Model.View_MouseUp(e);
        }

        private void GdiRenderer_MouseMove(object sender, MouseEventArgs e)
        {
            Model.View_MouseMove(e);
        }

        public void ViewInvalidate()
        {
            Invalidate();
        }

        public void ViewRefresh()
        {
            Refresh();
        }

        private void GdiRenderer_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Model.View_MouseDoubleClick(e.Location);
        }

        void GdiRenderer_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            Model.View_KeyDown(e);
        }

        private void GdiRenderer_KeyUp(object sender, KeyEventArgs e)
        {
            Model.View_KeyUp(e);
        }

        private void GdiRenderer_MouseLeave(object sender, EventArgs e)
        {
            Model.View_MouseLeave();
        }

        public SizeF MeasureString(string text, Font font)
        {
            return CurrentBuffer.MeasureString(text, font);

        }
        public void DrawTextBackground(Color color, float x, float y, float width, float height)
        {
            CurrentBuffer.FillRectangle(GetBrush(color), x, y, width, height);
        }

        public void DrawString(string text, Font font, Color color, PointF point)
        {
            CurrentBuffer.DrawString(text, font, GetBrush(color), point, LabelFormat);
        }

        public void DrawString(string text, Font font, Color color, RectangleF rect)
        {
            CurrentBuffer.DrawString(text, font, GetBrush(color), rect, LabelFormat);
        }

        public void DrawString(string text, Font font, Color color, float x, float y)
        {
            CurrentBuffer.DrawString(text, font, GetBrush(color), x, y, LabelFormat);
        }

        public void DrawNode(Color color, RectangleF area, bool outside, NodeModel node, int depth)
        {
            if(outside)
                CurrentBuffer.FillEllipse(GetBrush(color), area);
            else
                CurrentBuffer.FillRectangle(GetBrush(color), area);
        }

        public void DrawNodeOutline(Color color, int lineWidth, RectangleF area, bool outside, NodeModel node, int depth)
        {
            if (outside)
                CurrentBuffer.DrawEllipse(GetPen(color, lineWidth, false), area.X, area.Y, area.Width, area.Height);
            else
                CurrentBuffer.DrawRectangle(GetPen(color, lineWidth, false), area.X, area.Y, area.Width, area.Height);
        }

        public void DrawEllipse(Color color, int lineWidth, float x, float y, float width, float height)
        {
            
        }

        public void DrawLine(Color color, int lineWidth, PointF start, PointF end, bool dashed)
        {             
            var pen = GetPen(color, lineWidth, dashed);

            if(dashed)
                pen.DashOffset = XRay.DashOffset * 3;

            CurrentBuffer.DrawLine(pen, start, end);
        }

        SolidBrush GetBrush(Color color)
        {
            SolidBrush brush;
            if (!BrushCache.TryGetValue(color, out brush))
            {
                brush = new SolidBrush(color);
                BrushCache[color] = brush;
            }

            return brush;
        }

        Pen GetPen(Color color, int width, bool dashed)
        {
            int hash = color.GetHashCode() ^ width.GetHashCode() ^ dashed.GetHashCode();

            int debug1 = color.GetHashCode();
            int debug2 = width.GetHashCode();
            int debug3 = dashed.GetHashCode();


            Pen pen;
            if (!PenCache.TryGetValue(hash, out pen))
            {
                pen = new Pen(color, width);

                if (dashed)
                    pen.DashPattern = new float[] { 3, 6 };

                PenCache[hash] = pen;
            }

            return pen;
        }

        public Point GetCursorPosition()
        {
            return PointToClient(Cursor.Position);
        }
    }
}
