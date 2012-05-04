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
    public partial class GdiPanel : UserControl, IRenderer
    {
        public MainForm MainWin;
        public ViewHost Host;
        public ViewModel Model;

        Bitmap DisplayBuffer;

        StringFormat LabelFormat = new StringFormat();

        public IColorProfile ColorProfile;

        Graphics CurrentBuffer;

        Dictionary<Color, SolidBrush> BrushCache = new Dictionary<Color, SolidBrush>();
        Dictionary<int, Pen> PenCache = new Dictionary<int, Pen>();



        public GdiPanel(ViewHost host, IColorProfile profile)
        {
            InitializeComponent();

            MouseWheel += new MouseEventHandler(TreePanelGdiPlus_MouseWheel);

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            Host = host;
            MainWin = host.Main;
            Model = host.Model;

            LabelFormat.Trimming = StringTrimming.EllipsisCharacter;
            LabelFormat.FormatFlags |= StringFormatFlags.NoWrap;

            ColorProfile = profile;
        }

        public float ViewWidth { get { return Width; } }

        public float ViewHeight { get { return Height; } }

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

            CurrentBuffer = buffer;

            Model.Render();

            // Copy buffer to display
            e.Graphics.DrawImage(DisplayBuffer, 0, 0);
            Model.FpsCount++;

            Model.DoRedraw = false;
            Model.RedrawCount++;
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

        void TreePanelGdiPlus_MouseWheel(object sender, MouseEventArgs e)
        {
            Model.View_MouseWheel(e);
        }

        private void TreePanelGdiPlus_MouseDown(object sender, MouseEventArgs e)
        {
            Model.View_MouseDown(e);
        }

        private void TreePanelGdiPlus_MouseUp(object sender, MouseEventArgs e)
        {
            Model.View_MouseUp(e);
        }

        private void TreePanel_MouseMove(object sender, MouseEventArgs e)
        {
            Model.View_MouseMove(e);
        }

        public void ViewInvalidate()
        {
            Invalidate();
        }

        public void ViewInvalidate()
        {
            Refresh();
        }

        private void TreePanelGdiPlus_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Model.View_MouseDoubleClick(e.Location);
        }

        void TreePanel_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            Model.View_KeyDown(e);
        }

        private void TreePanel_KeyUp(object sender, KeyEventArgs e)
        {
            Model.View_KeyUp(e);
        }

        private void TreePanel_MouseLeave(object sender, EventArgs e)
        {
            Model.View_MouseLeave();
        }

        public SizeF MeasureString(string text, Font font)
        {
            return CurrentBuffer.MeasureString(text, font);

        }
        public void FillRectangle(Color color, float x, float y, float width, float height)
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

        public void FillEllipse(Color color, RectangleF area)
        {
            CurrentBuffer.FillEllipse(GetBrush(color), area);
        }

        public void FillRectangle(Color color, RectangleF area)
        {
            CurrentBuffer.FillRectangle(GetBrush(color), area);
        }

        public void DrawEllipse(Color color, int lineWidth, float x, float y, float width, float height)
        {
            CurrentBuffer.DrawEllipse(GetPen(color, lineWidth), x, y, width, height);
        }

        public void DrawRectangle(Color color, int lineWidth, float x, float y, float width, float height)
        {
            CurrentBuffer.DrawRectangle(GetPen(color, lineWidth), x, y, width, height);
        }

        public void DrawLine(Color color, int lineWidth, PointF start, PointF end)
        {
            CurrentBuffer.DrawLine(GetPen(color, lineWidth), start, end);
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

        Pen GetPen(Color color, int width)
        {
            int hash = color.GetHashCode() ^ width.GetHashCode();

            Pen pen;
            if (!PenCache.TryGetValue(hash, out pen))
            {
                pen = new Pen(color, width);
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
