using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;


namespace XLibrary
{
    public class GLRenderer : GLControl, IRenderer
    {
        ViewModel Model;
        bool GLLoaded;

        int VertexCount = 0;
        VertexPositionColor[] Vertices = new VertexPositionColor[1000];

        int ElementCount = 0;
        int[] Elements = new int[1000];

        struct Vbo
        {
            public int VboID;
            public int EboID;
            public int NumElements;
        }

        Vbo RenderVbo = new Vbo();

        public GLRenderer(ViewModel model)
        {
            Model = model;

            Load += GLRenderer_Load;
            Paint += GLRenderer_Paint;
            Resize += GLRenderer_Resize;
        }

        void GLRenderer_Load(object sender, EventArgs e)
        {
            SetupViewport();

            GL.ClearColor(Model.XColors.BackgroundColor);
            GL.ShadeModel(ShadingModel.Smooth);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.ColorMaterial);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);

            GL.CullFace(CullFaceMode.Back);

            float[] mat_emissive = { 0f, 0f, 0f, 1f };
            float[] mat_specular = { 1f, 1f, 1f, 1.0f };

            float[] light_diffuse = { .7f, .7f, .7f, .7f };
            float[] light_specular = { 1.0f, 1.0f, 1.0f, 1.0f };
            float[] light_position = { 1f, 1f, 1f, 0f };
            float[] light_ambient = { .5f, .5f, .35f, .5f };

            GL.Light(LightName.Light0, LightParameter.Diffuse, light_diffuse);
            GL.Light(LightName.Light0, LightParameter.Ambient, light_ambient);
            GL.Light(LightName.Light0, LightParameter.Specular, light_specular);
            GL.Light(LightName.Light0, LightParameter.Position, light_position);

            GLLoaded = true;
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
            GL.Ortho(0, Width, 0, Height, -1000, 1000);
            //GL.Scale(1, -1, 1);
            //GL.Translate(0, -Height, 0);
            GL.MatrixMode(MatrixMode.Modelview);

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

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if ((!Model.DoRedraw && !Model.DoRevalue && !Model.DoResize) || Model.CurrentRoot == null)
            {
                // do nothing to vbo
            }
            else //if(TreeMapVbo.NumElements == 0)
            {
                VertexCount = 0;
                ElementCount = 0;

                //RedrawTreeMap();
                Model.Render();

                RenderVbo = LoadVBO(Vertices, VertexCount, Elements, ElementCount);
            }

            Draw(RenderVbo);

            //glPrint(10, 10, "HELLOOOOOOOO");

            SwapBuffers();

            Model.FpsCount++;
        }

        Vbo LoadVBO<TVertex>(TVertex[] vertices, int verticesLength, int[] elements, int elementsLength) where TVertex : struct
        {
            Vbo handle = new Vbo();
            int size;

            // To create a VBO:
            // 1) Generate the buffer handles for the vertex and element buffers.
            // 2) Bind the vertex buffer handle and upload your vertex data. Check that the buffer was uploaded correctly.
            // 3) Bind the element buffer handle and upload your element data. Check that the buffer was uploaded correctly.

            GL.GenBuffers(1, out handle.VboID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, handle.VboID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(verticesLength * BlittableValueType.StrideOf(vertices)), vertices, BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (verticesLength * BlittableValueType.StrideOf(vertices) != size)
                throw new ApplicationException("Vertex data not uploaded correctly");

            GL.GenBuffers(1, out handle.EboID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, handle.EboID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(elementsLength * sizeof(int)), elements, BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (elementsLength * sizeof(int) != size)
                throw new ApplicationException("Element data not uploaded correctly");

            handle.NumElements = elementsLength;
            return handle;
        }

        void Draw(Vbo handle)
        {
            // To draw a VBO:
            // 1) Ensure that the VertexArray client state is enabled.
            // 2) Bind the vertex and element buffer handles.
            // 3) Set up the data pointers (vertex, normal, color) according to your vertex format.
            // 4) Call DrawElements. (Note: the last parameter is an offset into the element buffer
            //    and will usually be IntPtr.Zero).

            GL.EnableClientState(ArrayCap.ColorArray);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.NormalArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, handle.VboID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, handle.EboID);

            GL.VertexPointer(3, VertexPointerType.Float, BlittableValueType.StrideOf(Vertices), new IntPtr(0));
            GL.ColorPointer(4, ColorPointerType.UnsignedByte, BlittableValueType.StrideOf(Vertices), new IntPtr(12));
            GL.NormalPointer(NormalPointerType.Float, BlittableValueType.StrideOf(Vertices), new IntPtr(16));

            GL.DrawElements(BeginMode.Lines, handle.NumElements, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }

        public SizeF MeasureString(string text, Font font)
        {
            return new Size();
        }

        public void DrawString(string text, Font font, Color color, PointF point)
        {

        }

        public void DrawString(string text, Font font, Color color, float x, float y)
        {
            
        }

        public void DrawString(string text, Font font, Color color, RectangleF rect)
        {
            
        }

        public void FillEllipse(Color color, RectangleF area)
        {
            
        }

        public void FillRectangle(Color color, RectangleF area)
        {
            
        }

        public void FillRectangle(Color color, float x, float y, float width, float height)
        {
            
        }

        public void DrawEllipse(Color color, int lineWidth, float x, float y, float width, float height)
        {
            
        }

        int z = 0; // hopefully this works, if not need to increase each time by a small fraction

        public void DrawRectangle(Color color, int lineWidth, float x, float y, float width, float height)
        {
            AddNewVerticies(8);

            var southWestBottom = new Vector3(x, y, z);
            var southEastBottom = new Vector3(x + width, y, z);
            var northEastBottom = new Vector3(x + width, y + height, z);
            var northWestBottom = new Vector3(x, y + height, z);

            var normal = new Vector3(0, 0, 1);
            Vertices[VertexCount++].Set(northWestBottom, color, normal);
            Vertices[VertexCount++].Set(northEastBottom, color, normal);

            Vertices[VertexCount++].Set(northEastBottom, color, normal);
            Vertices[VertexCount++].Set(southEastBottom, color, normal);

            Vertices[VertexCount++].Set(southEastBottom, color, normal);
            Vertices[VertexCount++].Set(southWestBottom, color, normal);

            Vertices[VertexCount++].Set(southWestBottom, color, normal);
            Vertices[VertexCount++].Set(northWestBottom, color, normal);
        }

        private void AddNewVerticies(int amount)
        {
            if (VertexCount + amount >= Vertices.Length)
            {
                var newArray = new VertexPositionColor[Vertices.Length * 2];
                Array.Copy(Vertices, newArray, VertexCount);
                Vertices = newArray;
            }

            if (ElementCount + amount >= Elements.Length)
            {
                var newArray = new int[Elements.Length * 2];
                Array.Copy(Elements, newArray, ElementCount);
                Elements = newArray;
            }

            for (int i = 0; i < amount; i++)
                Elements[ElementCount++] = VertexCount + i;
        }

        public void DrawLine(Color color, int lineWidth, PointF start, PointF end, bool dashed)
        {
            AddNewVerticies(2);

            var startPos = new Vector3(start.X, start.Y, z);
            var endPos = new Vector3(end.X, end.Y, z);

            var normal = new Vector3(0, 0, 1);
            Vertices[VertexCount++].Set(startPos, color, normal);
            Vertices[VertexCount++].Set(endPos, color, normal);
        }

        public void ViewInvalidate()
        {
            Invalidate();
        }

        public void ViewRefresh()
        {
            Invalidate();
        }

        public Point GetCursorPosition()
        {
            return PointToClient(Cursor.Position);
        }
    }
}
