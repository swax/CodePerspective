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
        VertexBuffer Overlays = new VertexBuffer();

        EnableCap[] LineCaps = new EnableCap[] { EnableCap.Blend, EnableCap.LineSmooth };

        Vector3 Normal = new Vector3(0, 1, 0);

        RectangleF ClientRect;


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

            ClientRect = ClientRectangle;
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
            Overlays.Init();
        }

        void GLRenderer_Resize(object sender, EventArgs e)
        {
            if (!GLLoaded)
                return;

            SetupViewport();
        }

        private void SetupViewport()
        {
            ClientRect = new RectangleF(0, 0, Width, Height);

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
                Overlays.Reset();

                // render
                Model.Render();

                // send vertex buffers to gpu
                Nodes.Load();
                FontMap.Values.ForEach(f => f.LoadVBOs());
                Outlines.Values.Where(v => v.VertexCount > 0).ForEach(v => v.Load());
                CallLines.Values.Where(v => v.VertexCount > 0).ForEach(v => v.Load());
                DashedCallLines.Values.Where(v => v.VertexCount > 0).ForEach(v => v.Load());
                Overlays.Load();
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

            GLUtils.SafeEnable(EnableCap.Blend, () =>
            {
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                Overlays.Draw(BeginMode.Triangles);
                FontMap.Values.ForEach(f => f.DrawVBOs());
            });

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

        public void DrawString(string text, Font font, Color color, float x, float y, float width, float height)
        {
            var rect = new RectangleF(x, y, width, height);
            if (!ClientRect.IntersectsWith(rect))
                return;

            QFont qfont = GetQFont(font);

            qfont.PrintToVBO(text, QFontAlignment.Left, new Vector3(x, y, 0), color, new SizeF(width, height));
        }

        public void DrawNodeLabel(string text, Font font, Color color, RectangleF rect, NodeModel node, int depth)
        {
            if (!ClientRect.IntersectsWith(rect))
                return;

            QFont qfont = GetQFont(font);

            qfont.PrintToVBO(text, QFontAlignment.Left, new Vector3(rect.X, rect.Y, 0), color, rect.Size);
        }

        public void DrawNode(Color color, RectangleF area, bool outside, NodeModel node, int depth)
        {
            if (!ClientRect.IntersectsWith(area))
                return;

            var verticies = outside ? GetTriangleVerticies(area) : GetRectVerticies(area);

            Nodes.AddVerticies(color, Normal, verticies);
        }

        public void DrawTextBackground(Color color, float x, float y, float width, float height)
        {
            var rect = new RectangleF(x, y, width, height);
            if (!ClientRect.IntersectsWith(rect))
                return;

            Overlays.AddVerticies(color, Normal, GetRectVerticies(rect));
        }

        private Vector3[] GetRectVerticies(RectangleF rect)
        {
            return GetRectVerticies(rect.X, rect.Y, rect.Width, rect.Height);
        }

        private Vector3[] GetRectVerticies(float x, float y, float width, float height)
        {
            var v1 = new Vector3(x, y, 0);
            var v2 = new Vector3(x + width, y, 0);
            var v3 = new Vector3(x + width, y + height, 0);
            var v4 = new Vector3(x, y + height, 0);

            return new Vector3[] { v1, v2, v3, v1, v3, v4 };
        }

        public Vector3[] GetTriangleVerticies(RectangleF area)
        {
            return new Vector3[] {
                new Vector3(area.X, area.Y + area.Height, 0),
                new Vector3(area.X + area.Width / 2f, area.Y, 0),
                new Vector3(area.X + area.Width, area.Y + area.Height, 0)
            };
        }

        public void DrawNodeOutline(Color color, int lineWidth, RectangleF area, bool outside, NodeModel node, int depth)
        {
            if (!ClientRect.IntersectsWith(area))
                return;

            float x = area.X, y = area.Y, width = area.Width, height = area.Height;

            var vbo = GetLineVbo(Outlines, lineWidth);

            if (outside)
            {
                var v1 = new Vector3(area.X, area.Y + area.Height, 0);
                var v2 = new Vector3(area.X + area.Width / 2f, area.Y, 0);
                var v3 = new Vector3(area.X + area.Width, area.Y + area.Height, 0);

                vbo.AddVerticies(color, Normal, v1, v2, v2, v3, v3, v1);
            }
            else
            {
                var v1 = new Vector3(x, y, 0);
                var v2 = new Vector3(x + width, y, 0);
                var v3 = new Vector3(x + width, y + height, 0);
                var v4 = new Vector3(x, y + height, 0);

                vbo.AddVerticies(color, Normal, v1, v2, v2, v3, v3, v4, v4, v1);
            }
        }

        private VertexBuffer GetLineVbo(Dictionary<int, VertexBuffer> widthMap, int width)
        {
            VertexBuffer vbo;
            if (!widthMap.TryGetValue(width, out vbo))
            {
                vbo = new VertexBuffer();
                vbo.Init();
                widthMap[width] = vbo;
            }

            return vbo;
        }

        public void DrawCallLine(Color color, int lineWidth, PointF start, PointF end, bool dashed, NodeModel source, NodeModel destination)
        {
            var lineRect = new RectangleF(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y), Math.Abs(end.X - start.X), Math.Abs(end.Y - start.Y));
            if (!ClientRect.IntersectsWith(lineRect))
                return;

            var a = new Vector3(start.X, start.Y, 0);
            var b = new Vector3(end.X, end.Y, 0);

            VertexBuffer vbo = null;
            
            if (!dashed)
                vbo = GetLineVbo(CallLines, lineWidth);
            else
                vbo = GetLineVbo(DashedCallLines, lineWidth);

            vbo.AddVerticies(color, Normal, a, b);
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
