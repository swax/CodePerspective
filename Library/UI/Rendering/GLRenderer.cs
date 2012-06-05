using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using QuickFont;
using OpenTK.Graphics;


namespace XLibrary
{
    /* Originally implemented with vbo's but because the rendering interface
     * calls in order of drawing bottom to top this makes it difficult to use vbo's for
     * 2d while keeping the right order of layers or draw calls of all the rectangles and lines.
     */

    public class GLRenderer : GLControl, IRenderer
    {
        ViewModel Model;
        bool GLLoaded;

        VertexBuffer Nodes = new VertexBuffer();
        Dictionary<int, VertexBuffer> Outlines = new Dictionary<int, VertexBuffer>();
        Dictionary<int, VertexBuffer> CallLines = new Dictionary<int, VertexBuffer>();
        Dictionary<int, VertexBuffer> DashedCallLines = new Dictionary<int, VertexBuffer>();

        EnableCap[] LineCaps = new EnableCap[] { EnableCap.Blend, EnableCap.LineSmooth };

        Vector3 Normal = new Vector3(0, 1, 0);


        public GLRenderer(ViewModel model)
        {
            Model = model;

            Load += GLRenderer_Load;
            Paint += GLRenderer_Paint;
            Resize += GLRenderer_Resize;

            MouseWheel += new MouseEventHandler(GLRenderer_MouseWheel);
            MouseDown += new MouseEventHandler(GLRenderer_MouseDown);
            MouseUp += new MouseEventHandler(GLRenderer_MouseUp);
            MouseMove += new MouseEventHandler(GLRenderer_MouseMove);
            MouseDoubleClick += new MouseEventHandler(GLRenderer_MouseDoubleClick);
            KeyDown += new KeyEventHandler(GLRenderer_KeyDown);
            KeyUp += new KeyEventHandler(GLRenderer_KeyUp);
            MouseLeave += new EventHandler(GLRenderer_MouseLeave);
        }

        public void Start()
        {
            Model.TwoDimensionalValues = false;
            Model.DrawSubpixel = false;
            MakeCurrent();
        }

        public void Stop()
        {

        }

        void GLRenderer_Load(object sender, EventArgs e)
        {
            SetupViewport();

            GL.ClearColor(Model.XColors.BackgroundColor);

            GLLoaded = true;

            Nodes.Init();
        }

        void GLRenderer_Resize(object sender, EventArgs e)
        {
            if (!GLLoaded)
                return;

            SetupViewport();
        }

        private void SetupViewport()
        {
            GL.Viewport(0, 0, Width, Height);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Width, Height, 0, 0, 1); // sets 0,0 to upper left

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            Model.DoResize = true;
            Invalidate();
        }

        public float ViewWidth
        {
            get { return Width; }
        }

        public float ViewHeight
        {
            get { return Height; }
        }

        void GLRenderer_Paint(object sender, PaintEventArgs e)
        {
            if (!GLLoaded)
                return;

            GL.Clear(ClearBufferMask.ColorBufferBit);

            if (!Model.Paused)
            {
                // reset vertex buffers
                Nodes.Reset();
                FontMap.Values.ForEach(f => f.ResetVBOs());
                Outlines.Values.ForEach(v => v.Reset());
                CallLines.Values.ForEach(v => v.Reset());
                DashedCallLines.Values.ForEach(v => v.Reset());

                // render
                Model.Render();

                // send vertex buffers to gpu
                Nodes.Load();
                FontMap.Values.ForEach(f => f.LoadVBOs());
                Outlines.Values.Where(v => v.VertexCount > 0).ForEach(v => v.Load());
                CallLines.Values.Where(v => v.VertexCount > 0).ForEach(v => v.Load());
                DashedCallLines.Values.Where(v => v.VertexCount > 0).ForEach(v => v.Load());
            }

            // draw vertex buffers
            Nodes.Draw(BeginMode.Triangles);
            DrawLineVbo(Outlines);

            GLUtils.SafeEnable(LineCaps, () =>
            {
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
             
                DrawLineVbo(CallLines);

                GLUtils.SafeEnable(EnableCap.LineStipple, () =>
                {
                    //1111 1000 0000 0000
                    ushort pattern = (ushort)(0xF800 >> (XRay.DashOffset * 5));
                    GL.LineStipple(1, pattern);

                    DrawLineVbo(DashedCallLines);
                });
            });

            FontMap.Values.ForEach(f => f.DrawVBOs());
         

            SwapBuffers();

            Model.FpsCount++;
        }

        private void DrawLineVbo(Dictionary<int, VertexBuffer> widthMap)
        {
            foreach (int width in widthMap.Keys)
            {
                var vbo = widthMap[width];
                if (vbo.VertexCount == 0)
                    continue;

                GL.LineWidth(width);
                vbo.Draw(BeginMode.Lines);
            }
        }

        public SizeF MeasureString(string text, Font font)
        {
            QFont qfont = GetQFont(font);

            return qfont.Measure(text);
        }

        Dictionary<Font, QFont> FontMap = new Dictionary<Font, QFont>();

        public QFont GetQFont(Font font)
        {
            QFont qfont;
            if(FontMap.TryGetValue(font, out qfont))
                return qfont;

            var config = new QFontBuilderConfiguration() 
            {
                UseVertexBuffer = true,
                TextGenerationRenderHint = TextGenerationRenderHint.SystemDefault 
            };

            qfont = new QFont(font, config);
            qfont.Options.TransformToViewport = null;
            qfont.Options.WordWrap = false;

            FontMap[font] = qfont;
            return qfont;
        }

        public void DrawString(string text, Font font, Color color, float x, float y)
        {
            QFont qfont = GetQFont(font);

            qfont.PrintToVBO(text, new Vector3(x, y, 0), color);
        }

        public void DrawNodeLabel(string text, Font font, Color color, RectangleF rect, NodeModel node, int depth)
        {
            QFont qfont = GetQFont(font);

            qfont.PrintToVBO(text, rect.Width, QFontAlignment.Left, new Vector3(rect.X, rect.Y, 0), color);
        }

        public void DrawNode(Color color, RectangleF area, bool outside, NodeModel node, int depth)
        {
            if (outside)
                FillEllipse(color, area);
            else
                FillRectangle(color, area.X, area.Y, area.Width, area.Height);
        }

        public void DrawTextBackground(Color color, float x, float y, float width, float height)
        {
            FillRectangle(color, x, y, width, height);
        }

        private void FillRectangle(Color color, float x, float y, float width, float height)
        {
            var v1 = new Vector3(x, y, 0);
            var v2 = new Vector3(x + width, y, 0);
            var v3 = new Vector3(x + width, y + height, 0);
            var v4 = new Vector3(x, y + height, 0);

            Nodes.AddVerticies(color, Normal, v1, v2, v3, v1, v3, v4);
        }

        public void DrawNodeOutline(Color color, int lineWidth, RectangleF area, bool outside, NodeModel node, int depth)
        {
            float x = area.X, y = area.Y, width = area.Width, height = area.Height;

            if (outside)
            {
                var v1 = new Vector3(area.X, area.Y + area.Height, 0);
                var v2 = new Vector3(area.X + area.Width / 2f, area.Y, 0);
                var v3 = new Vector3(area.X + area.Width, area.Y + area.Height, 0);

                DrawLine(v1, v2, Outlines, color, lineWidth);
                DrawLine(v2, v3, Outlines, color, lineWidth);
                DrawLine(v3, v1, Outlines, color, lineWidth);
            }
            else
            {
                var v1 = new Vector3(x, y, 0);
                var v2 = new Vector3(x + width, y, 0);
                var v3 = new Vector3(x + width, y + height, 0);
                var v4 = new Vector3(x, y + height, 0);

                DrawLine(v1, v2, Outlines, color, lineWidth);
                DrawLine(v2, v3, Outlines, color, lineWidth);
                DrawLine(v3, v4, Outlines, color, lineWidth);
                DrawLine(v4, v1, Outlines, color, lineWidth);
            }
        }

        void DrawLine(Vector3 a, Vector3 b, Dictionary<int, VertexBuffer> widthMap, Color color, int width)
        {
            VertexBuffer vbo;
            if (!widthMap.TryGetValue(width, out vbo))
            {
                vbo = new VertexBuffer();
                vbo.Init();
                widthMap[width] = vbo;
            }

            var normal = new Vector3();
            vbo.AddVertex(a, color, normal);
            vbo.AddVertex(b, color, normal);
        }

        public void FillEllipse(Color color, RectangleF area)
        {
            var v1 = new Vector3(area.X, area.Y + area.Height, 0);
            var v2 = new Vector3(area.X + area.Width/2f, area.Y, 0);
            var v3 = new Vector3(area.X + area.Width, area.Y + area.Height, 0);

            Nodes.AddVerticies(color, Normal, v1, v2, v3);
        }

        public void DrawEdge(Color color, int lineWidth, PointF start, PointF end, bool dashed, NodeModel source, NodeModel destination)
        {
            var a = new Vector3(start.X, start.Y, 0);
            var b = new Vector3(end.X, end.Y, 0);

            if (!dashed)
                DrawLine(a, b, CallLines, color, lineWidth);
            else
                DrawLine(a, b, DashedCallLines, color, lineWidth);
        }

        public void ViewInvalidate()
        {
            Invalidate();
        }

        public Point GetCursorPosition()
        {
            return PointToClient(Cursor.Position);
        }

        void GLRenderer_MouseLeave(object sender, EventArgs e)
        {
            Model.View_MouseLeave();
        }

        void GLRenderer_KeyUp(object sender, KeyEventArgs e)
        {
            Model.View_KeyUp(e);
        }

        void GLRenderer_KeyDown(object sender, KeyEventArgs e)
        {
            Model.View_KeyDown(e);
        }

        void GLRenderer_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Model.View_MouseDoubleClick(e.Location);
        }

        void GLRenderer_MouseMove(object sender, MouseEventArgs e)
        {
            Model.View_MouseMove(e);
        }

        void GLRenderer_MouseUp(object sender, MouseEventArgs e)
        {
            Model.View_MouseUp(e);
        }

        void GLRenderer_MouseDown(object sender, MouseEventArgs e)
        {
            Model.View_MouseDown(e);
        }

        void GLRenderer_MouseWheel(object sender, MouseEventArgs e)
        {
            Model.View_MouseWheel(e);
        }
    }
}
