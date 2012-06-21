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

    public class GibsonRenderer : GLControl, IRenderer
    {
        ViewModel Model;
        bool GLLoaded;

        Timer LogicTimer;
        int LogicFPS = 20;

        bool MouseLook;
        Point MidWindow = new Point();
        FpsCamera FpsCam = new FpsCamera(53, -0.1f, 273, 420, 458);

        float LevelSize = 3;

        float OctogonEdge = (float) (1.0 - Math.Sqrt(2) / 2.0);

        VertexBuffer Nodes = new VertexBuffer();
        Dictionary<int, VertexBuffer> Outlines = new Dictionary<int,VertexBuffer>();
        Dictionary<int, VertexBuffer> CallLines = new Dictionary<int, VertexBuffer>();
        Dictionary<int, VertexBuffer> DashedCallLines = new Dictionary<int, VertexBuffer>();

        enum SelectionModes { None, Single, Double };
        SelectionModes SelectionMode;
        Point SelectionPoint;
        Dictionary<int, NodeModel> SelectionMap = new Dictionary<int,NodeModel>();

        EnableCap[] LineCaps = new EnableCap[] { EnableCap.Blend, EnableCap.LineSmooth };

        Point MouseDownPoint;
        Point PanStart;

        float LabelHeight = 0f;
        float CallHeight = 1f;
        float LiveCallHeight = 2f;


        public GibsonRenderer(ViewModel model)
        {
            Model = model;

            Load += GLRenderer_Load;
            Paint += GLRenderer_Paint;
            Resize += GLRenderer_Resize;

            KeyDown += new KeyEventHandler(FpsRenderer_KeyDown);
            KeyUp += new KeyEventHandler(FpsRenderer_KeyUp);
            MouseDown += new MouseEventHandler(FpsRenderer_MouseDown);
            MouseUp += new MouseEventHandler(FpsRenderer_MouseUp);
            Leave += new EventHandler(FpsRenderer_Leave);
            MouseMove += new MouseEventHandler(FpsRenderer_MouseMove);
            MouseDoubleClick += new MouseEventHandler(FpsRenderer_MouseDoubleClick);
            MouseWheel += new MouseEventHandler(FpsRenderer_MouseWheel);

            LogicTimer = new Timer();
            LogicTimer.Interval = 1000 / LogicFPS;
            LogicTimer.Tick += new EventHandler(LogicTimer_Tick);
            LogicTimer.Enabled = true;
        }

        public void Start()
        {
            Model.TwoDimensionalValues = true;
            Model.DrawSubpixel = true;
            MakeCurrent();
            LogicTimer.Enabled = true;
        }

        public void Stop()
        {
            LogicTimer.Enabled = false;
        }

        void GLRenderer_Load(object sender, EventArgs e)
        {
            SetupViewport();

            GL.ClearColor(Model.XColors.BackgroundColor);

            GL.ShadeModel(ShadingModel.Smooth);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.ColorMaterial);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            GL.Enable(EnableCap.Lighting);

            GL.Enable(EnableCap.Light0);
            GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { .7f, .7f, .7f, .7f });
            GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { .5f, .5f, .5f, .5f });
            GL.Light(LightName.Light0, LightParameter.Specular, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });
            GL.Light(LightName.Light0, LightParameter.Position, new float[] { 1f, 1f, 1f, 0f });

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

            MidWindow = new Point(Width / 2, Height / 2);

            GL.MatrixMode(MatrixMode.Projection);

            float aspect_ratio = (float)Width / (float)Height;
            Matrix4 perpective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspect_ratio, 1, 4000);
            GL.LoadMatrix(ref perpective);

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

        void LogicTimer_Tick(object sender, EventArgs e)
        {
            FpsCam.MoveTick();

            if (!MouseLook)
                return;    

            FpsCam.LookTick(PointToClient(Cursor.Position), MidWindow);

            // Reset the mouse position to the centre of the window each frame
            Cursor.Position = PointToScreen(MidWindow);
            Invalidate();
        }

        void GLRenderer_Paint(object sender, PaintEventArgs e)
        {
            if (!GLLoaded)
                return;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            // Move the camera to our location in space
            FpsCam.SetupCamera();

            if (!Model.Paused)
            {
                if (SelectionMode != SelectionModes.None)
                    DoSelectionPass();

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

            GLUtils.SafeEnable(LineCaps, () =>
            {
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

                DrawLineVbo(Outlines);
                DrawLineVbo(CallLines);

                GLUtils.SafeEnable(EnableCap.LineStipple, () =>
                {
                    //1111 1000 0000 0000
                    ushort pattern = (ushort)(0xF800 >> (XRay.DashOffset * 5));
                    GL.LineStipple(1, pattern);

                    DrawLineVbo(DashedCallLines);
                });
            });

            GLUtils.SafeSaveMatrix(() =>
            {
                GL.Rotate(90, 1.0f, 0.0f, 0.0f);
                FontMap.Values.ForEach(f => f.DrawVBOs());
            });

            if (MouseLook)
                FpsCam.DrawHud(Width, Height, MidWindow, Color.Black);

            SwapBuffers();

            Model.FpsCount++;
        }

        private void DoSelectionPass()
        {
            // turn off lighting and draw world as flat colored objects who's color corresponds to their node id
            GLUtils.SafeDisable(EnableCap.Lighting, () =>
            {
                // reset vbo
                Nodes.Reset();

                // call render
                Model.Render();

                // load vbo
                Nodes.Load();

                // draw node vbo
                Nodes.Draw(BeginMode.Triangles);

                // read pixel at cursor
                byte[] rgb = new byte[3];
                GL.ReadPixels(SelectionPoint.X, Height - SelectionPoint.Y, 1, 1, PixelFormat.Rgb, PixelType.UnsignedByte, rgb);

                // convert to color int
                int id = (rgb[0] << 24) | (rgb[1] << 16) | rgb[2];

                // look up in map
                NodeModel node;
                SelectionMap.TryGetValue(id, out node);

                if (SelectionMode == SelectionModes.Single)
                    Model.ClickNode(node);
                else
                    Model.SetRoot(node);
            });

            // clear buffer
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // turn off selection mode
            SelectionMode = SelectionModes.None;
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

        public void DrawNodeLabel(string text, Font font, Color color, RectangleF rect, NodeModel node, int depth)
        {
            QFont qfont = GetQFont(font);

            float height = LabelHeight;
            if(Model.ViewLayout == LayoutType.TreeMap)
                height = depth * LevelSize + GetNodeHeight(node); // put over call lines
            
            if (SelectionMode != SelectionModes.None) 
            {
                SelectionMap[node.ID] = node;
                color = Color.FromArgb((255 << 24) | node.ID);

                var textArea = qfont.Measure(text, rect.Width, QFontAlignment.Left);

                var normal = new Vector3(0, 1, 0);
                var v1 = new Vector3(rect.X, height, rect.Y);
                var v2 = new Vector3(rect.X, height, rect.Y + textArea.Height);
                var v3 = new Vector3(rect.X + textArea.Width, height, rect.Y + textArea.Height);
                var v4 = new Vector3(rect.X + textArea.Width, height, rect.Y);

                Nodes.AddVerticies(color, normal, v1, v2, v3, v1, v3, v4);
            }
            else
                qfont.PrintToVBO(text, rect.Width, QFontAlignment.Left, new Vector3(rect.X, rect.Y, -height), color);

        }

        public void DrawNode(Color color, RectangleF area, bool outside, NodeModel node, int depth)
        {
            float x = area.X + 0.1f;
            float y = area.Y + 0.1f;
            float width = area.Width - 0.2f;
            float length = area.Height - 0.2f;
            float bottom = depth * LevelSize + 0.1f;
            float height = GetNodeHeight(node) - 0.2f;

            if (SelectionMode != SelectionModes.None)
            {
                SelectionMap[node.ID] = node;
                color = Color.FromArgb((255 << 24) | node.ID);
            }

            if (outside)
                DrawPyramid(color, x, y, width, length, bottom, height);
            else
                GLUtils.DrawCube(Nodes, color, x, y, width, length, bottom, height);
        }

        float GetNodeHeight(NodeModel node)
        {
            if (node.ObjType == XObjType.Method)
                return Math.Max(100f * (float)node.SecondaryValue / (float)Model.MaxSecondaryValue, LevelSize);
            else
                return LevelSize;
        }

        public void DrawTextBackground(Color color, float x, float y, float width, float height)
        {
           // used for footer and tooltips, needs mouse over to worki
            // could draw in ortho view
        }

        public void DrawString(string text, Font font, Color color, float x, float y, float width, float height)
        {
            // used for footer and tooltips, needs mouse over to work
            // could draw in ortho view
            //QFont qfont = GetQFont(font);
            //qfont.PrintToVBO(text, new Vector3(x, y, 0), color);
        }


        public void DrawNodeOutline(Color color, int lineWidth, RectangleF area, bool outside, NodeModel node, int depth)
        {
            float x = area.X, z = area.Y, width = area.Width, length = area.Height;

            float floor = depth * LevelSize; 
            float height = GetNodeHeight(node);

            if(outside)
                DrawPyramidOutline(color, lineWidth, x, z, width, length, floor, height);
            else
                DrawBoxOutline(color, lineWidth, x, z, width, length, floor, height);
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

        private void DrawPyramidOutline(Color color, int lineWidth, float x, float z, float width, float length, float floor, float height)
        {
            var v1 = new Vector3(x, floor, z);
            var v2 = new Vector3(x + width, floor, z);
            var v3 = new Vector3(x + width, floor, z + length);
            var v4 = new Vector3(x, floor, z + length);

            var v5 = new Vector3(x + width / 2f, floor + height, z + length / 2f);

            var vbo = GetLineVbo(Outlines, lineWidth);

            var normal = new Vector3();

            vbo.AddVerticies(color, normal, v1, v2, v2, v3, v3, v4, v4, v1,
                                            v1, v5, v2, v5, v3, v5, v4, v5);
        }

        private void DrawPyramid(Color color, float x, float z, float width, float length, float floor, float height)
        {
            var v1 = new Vector3(x, floor, z);
            var v2 = new Vector3(x + width, floor, z);
            var v3 = new Vector3(x + width, floor, z + length);
            var v4 = new Vector3(x, floor, z + length);

            var v5 = new Vector3(x + width / 2f, floor + height, z + length / 2f);

            // bottom vertices
            var normal = new Vector3(0, -1, 0);
            Nodes.AddVerticies(color, normal, v1, v2, v3, v1, v3, v4);

            // -z facing vertices
            normal = new Vector3(0, 0, -1);
            Nodes.AddVerticies(color, normal, v3, v2, v5);

            // x facing vertices
            normal = new Vector3(1, 0, 0);
            Nodes.AddVerticies(color, normal, v4, v3, v5);

            // z facing vertices
            normal = new Vector3(0, 0, 1);
            Nodes.AddVerticies(color, normal, v1, v4, v5);

            // -x facing vertices
            normal = new Vector3(-1, 0, 0);
            Nodes.AddVerticies(color, normal, v2, v1, v5);
        }


        private void DrawBoxOutline(Color color, int lineWidth, float x, float z, float width, float length, float floor, float height)
        {
            var v1 = new Vector3(x, floor, z);
            var v2 = new Vector3(x + width, floor, z);
            var v3 = new Vector3(x + width, floor, z + length);
            var v4 = new Vector3(x, floor, z + length);

            var v5 = new Vector3(x, floor + height, z);
            var v6 = new Vector3(x + width, floor + height, z);
            var v7 = new Vector3(x + width, floor + height, z + length);
            var v8 = new Vector3(x, floor + height, z + length);

            var vbo = GetLineVbo(Outlines, lineWidth);

            var normal = new Vector3();

            vbo.AddVerticies(color, normal, v1, v2, v2, v3, v3, v4, v4, v1,
                                            v5, v6, v6, v7, v7, v8, v8, v5,
                                            v1, v5, v2, v6, v3, v7, v4, v8);
        }

        public void DrawCallLine(Color color, int lineWidth, PointF start, PointF end, bool live, NodeModel source, NodeModel destination)
        {
            float height = live ? LiveCallHeight : CallHeight;

            var a = new Vector3(start.X, height, start.Y);
            var b = new Vector3(end.X, height, end.Y);

            var vbo = GetLineVbo(live ? DashedCallLines : CallLines, lineWidth * 2);

            var normal = new Vector3();

            vbo.AddVerticies(color, normal, a, b);
        }

        public void ViewInvalidate()
        {
            Invalidate();
        }

        public Point GetCursorPosition()
        {
            return new Point(0, 0); // return PointToClient(Cursor.Position);
        }

        void FpsRenderer_KeyUp(object sender, KeyEventArgs e)
        {
            FpsCam.KeyUp(e);

            Model.View_KeyUp(e);
        }

        void FpsRenderer_KeyDown(object sender, KeyEventArgs e)
        {
            FpsCam.KeyDown(e);

            if (e.KeyCode == Keys.M || e.KeyCode == Keys.Escape)
            {
                if (e.KeyCode == Keys.Escape)
                    MouseLook = false;
                else
                    MouseLook = !MouseLook;

                if (MouseLook)
                    Cursor.Hide();
                else
                    Cursor.Show();

                Cursor.Position = PointToScreen(MidWindow);
            }

            Model.View_KeyDown(e);
        }

        void FpsRenderer_MouseDown(object sender, MouseEventArgs e)
        {
            //Model.View_MouseDown(e);

            MouseDownPoint = e.Location;
            PanStart = e.Location;

            //MouseLook = true;
            //Cursor.Hide();
            //Cursor.Position = PointToScreen(MidWindow);
        }

        void FpsRenderer_MouseUp(object sender, MouseEventArgs e)
        {
            //Model.View_MouseUp(e);
            
            if (MouseDownPoint == e.Location)
                ManualMouseClick(e, SelectionModes.Single);

            MouseDownPoint = Point.Empty;
            PanStart = Point.Empty;

            //MouseLook = false;
            //Cursor.Show();
        }

        void FpsRenderer_MouseMove(object sender, MouseEventArgs e)
        {
            //Model.View_MouseMove(e);
            
            // if moving with left button on, pan
            if (MouseDownPoint != Point.Empty && e.Button == MouseButtons.Left)
            {
                FpsCam.LookTick(e.Location, PanStart);

                PanStart = e.Location;
            }
        }

        void ManualMouseClick(MouseEventArgs e, SelectionModes mode)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                SelectionPoint = PointToClient(Cursor.Position);
                SelectionMap = new Dictionary<int, NodeModel>();

                // on double click event, mouse up will fire after so don't set selection mode back to single
                if (SelectionMode == SelectionModes.None)
                    SelectionMode = mode;
            }
            // back button zoom out
            else if (e.Button == MouseButtons.XButton1)
                Model.NavBack();

            // forward button zoom in
            else if (e.Button == MouseButtons.XButton2)
                Model.NavForward();
        }

        void FpsRenderer_Leave(object sender, EventArgs e)
        {
            Model.View_MouseLeave();
        }

        void FpsRenderer_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ManualMouseClick(e, SelectionModes.Double);
        }

        void FpsRenderer_MouseWheel(object sender, MouseEventArgs e)
        {
            float steps = (float)e.Delta / 120f;

            for(int i = 0; i < Math.Abs(steps); i++)
                FpsCam.MoveWheelTick(e.Delta > 0);
        }
    }
}
