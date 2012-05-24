using System;    
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Graphics.OpenGL;


namespace XLibrary
{
    public partial class FpsView : UserControl
    {
        public ViewModel Model;

        bool GLLoaded = false;

        bool ShowingOutside;
        bool ShowingExternal;

        Timer LogicTimer;
        int LogicFPS = 20;


        //bool ShowLabels = true;

        // colors
        Color BorderBrush = Color.Silver;

        Color[] OverBrushes = new Color[7];

        Color[] HitBrush;
        Color[] MultiHitBrush;

        Color[] FieldSetBrush;
        Color[] FieldGetBrush;

        Color[] ExceptionBrush;

        Color UnknownColor = Color.Black;
        Color FileColor = Color.Black;
        Color NamespaceColor = Color.DarkBlue;
        Color ClassColor = Color.DarkGreen;
        Color MethodColor = Color.DarkRed;
        Color FieldColor = Color.Goldenrod;

        Color HitColor = Color.FromArgb(255, 192, 128);
        Color MultiHitColor = Color.Orange;

        Color ExceptionColor = Color.Red;
        Color MultiExceptionColor = Color.DarkRed;

        Color FieldSetColor = Color.Blue;
        Color FieldGetColor = Color.LimeGreen;

        Color[] ObjBrushes;
        Color[] ObjDitheredBrushes;
        Pen[] ObjPens;
        Pen[] ObjFocusedPens;

        public static Dictionary<int, Color> ObjColors = new Dictionary<int, Color>();

        Color NothingBrush = Color.White;

        Color EntryBrush = Color.LightGreen;
        Color MultiEntryBrush = Color.LimeGreen;

        Color HoldingBrush = Color.FromArgb(255, 255, 192);
        Color MultiHoldingBrush = Color.Yellow;

        Color SearchMatchBrush = Color.Red;

        // dependencies
        Color DependentBrush = Color.Red;
        Color IndependentBrush = Color.Blue;
        Color InterdependentBrush = Color.Purple;

        HashSet<int> DependentClasses = new HashSet<int>();
        HashSet<int> IndependentClasses = new HashSet<int>();

        public float PlatformHeight = 5.0f;
       
        int FontTexture;
        int FontList;

        bool holdingForward;
        bool holdingBackward;
        bool holdingUp;
        bool holdingDown;
        bool holdingLeftStrafe;
        bool holdingRightStrafe;
        bool holdingRun;

        float camXPos = 1300;
        float camYPos = 850;
        float camZPos = -300;

        bool MouseLook;

        float movementSpeedFactor = 15.0f;

        Point MidWindow = new Point();
        float camXRot = 41;
        float camYRot = -135;

        struct Vbo 
        { 
            public int VboID;
            public int EboID; 
            public int NumElements;
        }

        Vbo TreeMapVbo = new Vbo();


        public FpsView(ViewModel model)
        {
            Model = model;

            InitColors();

            InitializeComponent();


            GLView.KeyDown += new KeyEventHandler(GLView_KeyDown);
            GLView.KeyUp += new KeyEventHandler(GLView_KeyUp);

            LogicTimer = new Timer();
            LogicTimer.Interval = 1000 / LogicFPS;
            LogicTimer.Tick += new EventHandler(LogicTimer_Tick);
            LogicTimer.Enabled = true;
        }

        public void InitColors()
        {
            HitBrush = new Color[XRay.HitFrames];
            MultiHitBrush = new Color[XRay.HitFrames];
            ExceptionBrush = new Color[XRay.HitFrames];
            FieldSetBrush = new Color[XRay.HitFrames];
            FieldGetBrush = new Color[XRay.HitFrames];

            //CallPen = new Pen[XRay.HitFrames];
            //CallPenFocused = new Pen[XRay.HitFrames];

            for (int i = 0; i < XRay.HitFrames; i++)
            {
                int brightness = 255 - (255 / XRay.HitFrames * i);

                HitBrush[i] = Color.FromArgb(255 - brightness, HitColor);
                MultiHitBrush[i] = Color.FromArgb(255 - brightness, MultiHitColor);
                ExceptionBrush[i] = Color.FromArgb(255 - brightness, ExceptionColor);
                FieldSetBrush[i] = Color.FromArgb(255 - brightness, FieldSetColor);
                FieldGetBrush[i] = Color.FromArgb(255 - brightness, FieldGetColor);

                /*CallPen[i] = new Pen(Color.FromArgb(255 - brightness, CallColor));
                CallPen[i].DashPattern = new float[] { FunctionCall.DashSize, FunctionCall.DashSpace };
                CallPen[i].EndCap = LineCap.ArrowAnchor;

                CallPenFocused[i] = new Pen(Color.FromArgb(255 - (brightness / 2), CallColor), 2);
                CallPenFocused[i].DashPattern = new float[] { FunctionCall.DashSize, FunctionCall.DashSpace };
                CallPenFocused[i].EndCap = LineCap.ArrowAnchor;*/
            }

            for (int i = 0; i < OverBrushes.Length; i++)
            {
                int brightness = 128 / (OverBrushes.Length  + 1) * (OverBrushes.Length - i);
                OverBrushes[i] = Color.FromArgb(128 + brightness, 128 + brightness, 255);
            }

            // set colors of differnt brush / pen arrays
            ObjColors.Clear();
            ObjColors[(int)XObjType.Root] = UnknownColor;
            ObjColors[(int)XObjType.External] = UnknownColor;
            ObjColors[(int)XObjType.Internal] = UnknownColor;
            ObjColors[(int)XObjType.File] = FileColor;
            ObjColors[(int)XObjType.Namespace] = NamespaceColor;
            ObjColors[(int)XObjType.Class] = ClassColor;
            ObjColors[(int)XObjType.Field] = FieldColor;
            ObjColors[(int)XObjType.Method] = MethodColor;

            var objTypes = Enum.GetValues(typeof(XObjType));
           
            ObjBrushes = new Color[objTypes.Length];
            ObjDitheredBrushes = new Color[objTypes.Length];
            ObjPens = new Pen[objTypes.Length];
            ObjFocusedPens = new Pen[objTypes.Length];

            for (int i = 0; i < objTypes.Length; i++ )
            {
                ObjBrushes[i] = ObjColors[i];
                ObjDitheredBrushes[i] = Color.FromArgb(128, ObjColors[i]);
                ObjPens[i] = new Pen(ObjColors[i]);
                ObjFocusedPens[i] = new Pen(ObjColors[i], 3);
            }
        }

        void LogicTimer_Tick(object sender, EventArgs e)
        {
            DoCameraMove();

            CheckMouseMove();
        }

        private double toRads(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        void DoCameraMove()
        {
            // Break up our movement into components along the X, Y and Z axis
            double moveX = 0.0f;
            double moveY = 0.0f;
            double moveZ = 0.0f;

            float speed = movementSpeedFactor;
            if (holdingRun)
                speed *= 3;

            if (holdingForward)
            {
                // Control X-Axis movement
                double pitchFactor = Math.Cos(toRads(camXRot));
                moveX += speed * Math.Sin(toRads(camYRot)) * pitchFactor;
 
                // Control Y-Axis movement
                moveY += speed * Math.Sin(toRads(camXRot)) * -1.0f;
 
                // Control Z-Axis movement
                double yawFactor = Math.Cos(toRads(camXRot));
                moveZ += speed * Math.Cos(toRads(camYRot)) * -1.0f * yawFactor;
            }
 
            if (holdingBackward)
            {
                // Control X-Axis movement
                double pitchFactor = Math.Cos(toRads(camXRot));
                moveX += speed * Math.Sin(toRads(camYRot)) * -1.0f * pitchFactor;
 
                // Control Y-Axis movement
                moveY += speed * Math.Sin(toRads(camXRot));
 
                // Control Z-Axis movement
                double yawFactor = Math.Cos(toRads(camXRot));
                moveZ += speed * Math.Cos(toRads(camYRot)) * yawFactor;
            }

            if (holdingUp)
            {
                moveY += speed;
            }

            if (holdingDown)
            {
                moveY -= speed;
            }
 
            if (holdingLeftStrafe)
            {
                // Calculate our Y-Axis rotation in radians once here because we use it twice
                double yRotRad = toRads(camYRot);
 
                moveX += -speed * Math.Cos(yRotRad);
                moveZ += -speed * Math.Sin(yRotRad);
            }
 
            if (holdingRightStrafe)
            {
                // Calculate our Y-Axis rotation in radians once here because we use it twice
                double yRotRad = toRads(camYRot);
 
                moveX += speed * Math.Cos(yRotRad);
                moveZ += speed * Math.Sin(yRotRad);
            }
 
            // After combining our movements for any & all keys pressed, assign them to our camera speed along the given axis

            camXPos += (float) moveX;
            camYPos += (float) moveY;
            camZPos += (float) moveZ;
        }

        void GLView_KeyDown(object sender, KeyEventArgs e)
        {      
            if (e.KeyCode == Keys.W)
                holdingForward = true;
            else if (e.KeyCode == Keys.S)
                holdingBackward = true;
            else if (e.KeyCode == Keys.A)
                holdingLeftStrafe = true;
            else if (e.KeyCode == Keys.D)
                holdingRightStrafe = true;
            else if (e.KeyData == (Keys.LButton | Keys.ShiftKey | Keys.Control))
                holdingDown = true;
            else if (e.KeyCode == Keys.Space)
                holdingUp = true;
            else if (e.KeyCode == Keys.ShiftKey)
                holdingRun = true;

            else if (e.KeyCode == Keys.M || e.KeyCode == Keys.Escape)
            {
                if (e.KeyCode == Keys.Escape)
                    MouseLook = false;
                else
                    MouseLook = !MouseLook;

                if (MouseLook)
                    Cursor.Hide();
                else
                    Cursor.Show();

                Cursor.Position = GLView.PointToScreen(MidWindow);
            }
        }


        void GLView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
                holdingForward = false;
            else if (e.KeyCode == Keys.S)
                holdingBackward = false;
            else if (e.KeyCode == Keys.A)
                holdingLeftStrafe = false;
            else if (e.KeyCode == Keys.D)
                holdingRightStrafe = false;
            else if (e.KeyData == (Keys.LButton | Keys.ShiftKey))
                holdingDown = false;
            else if (e.KeyCode == Keys.Space)
                holdingUp = false;
            else if (e.KeyCode == Keys.ShiftKey)
                holdingRun = false;
        }

        void CheckMouseMove()
        {
            if (!MouseLook)
                return;

            float vertMouseSensitivity = 10.0f;
            float horizMouseSensitivity = 10.0f;

            //cout << "Mouse cursor is at position (" << mouseX << ", " << mouseY << endl;

            var viewPos = GLView.PointToClient(Cursor.Position);

            float horizMovement = viewPos.X - MidWindow.X;
            float vertMovement = viewPos.Y - MidWindow.Y;

            camXRot += vertMovement / vertMouseSensitivity;
            camYRot += horizMovement / horizMouseSensitivity;

            // Control looking up and down with the mouse forward/back movement
            // Limit loking up to vertically up
            if (camXRot < -90.0f)
                camXRot = -90.0f;

            // Limit looking down to vertically down
            if (camXRot > 90.0f)
                camXRot = 90.0f;

            // Looking left and right. Keep the angles in the range -180.0f (anticlockwise turn looking behind) to 180.0f (clockwise turn looking behind)
            if (camYRot < -180.0f)
                camYRot += 360.0f;

            if (camYRot > 180.0f)
                camYRot -= 360.0f;

            // Reset the mouse position to the centre of the window each frame
            Cursor.Position = GLView.PointToScreen(MidWindow);
            GLView.Invalidate();
        }


        public void Redraw()
        {
            Model.DoRedraw = true;
            GLView.Invalidate();
        }

        private void GLView_Load(object sender, EventArgs e)
        {
            SetupViewport();

            GL.ClearColor(System.Drawing.Color.Black);
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

            GL.GenBuffers(1, out TreeMapVbo.VboID);
            GL.GenBuffers(1, out TreeMapVbo.EboID);

            //LoadTextures();
            //BuildFont();
        }

        private void GLView_Resize(object sender, EventArgs e)
        {
            base.OnResize(e);

            if (!GLLoaded)
                return;

            MidWindow = new Point(Width / 2, Height / 2);

            SetupViewport();
        }

        internal void SetupViewport()
        {
            GL.Viewport(0, 0, GLView.Width, GLView.Height);

            GL.MatrixMode(MatrixMode.Projection);

            float aspect_ratio = GLView.Width / (float)GLView.Height;
            Matrix4 perpective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspect_ratio, 1, 4000);
            GL.LoadMatrix(ref perpective);

            GLView.Invalidate();
        }


        Vector3 CameraPos = new Vector3(-500f, 0f, 0.0f);

        private void GLView_Paint(object sender, PaintEventArgs e)
        {
            if (!GLLoaded)
                return;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            // Move the camera to our location in space
            GL.Rotate(camXRot, 1.0f, 0.0f, 0.0f);        // Rotate our camera on the x-axis (looking up and down)
            GL.Rotate(camYRot, 0.0f, 1.0f, 0.0f);        // Rotate our camera on the  y-axis (looking left and right)
            GL.Translate(-camXPos, -camYPos, -camZPos);    // Translate the modelviewm matrix to the position of our camera
 
            RedrawTreeMap();

            LoadVBO(Vertices, VertexCount, Elements, ElementCount);

            Draw();

            if(MouseLook)
                DrawHud();

            GLView.SwapBuffers();

            Model.FpsCount++;
        }

        void DrawHud()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Ortho(0, GLView.Width, GLView.Height, 0, 0, 1);
           
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.Disable(EnableCap.CullFace);

            GL.Clear(ClearBufferMask.DepthBufferBit);

            GL.Begin(BeginMode.Lines);
            GL.Color3(Color.White);
            GL.Vertex2(MidWindow.X - 2, MidWindow.Y);
            GL.Vertex2(MidWindow.X + 3, MidWindow.Y);
            GL.Vertex2(MidWindow.X, MidWindow.Y - 3);
            GL.Vertex2(MidWindow.X, MidWindow.Y + 2);
            GL.End();

            GL.Enable(EnableCap.CullFace);

            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
        }

        void LoadVBO<TVertex>(TVertex[] vertices, int verticesLength, int[] elements, int elementsLength) where TVertex : struct
        {
            int size;

            // To create a VBO:
            // 1) Generate the buffer handles for the vertex and element buffers.
            // 2) Bind the vertex buffer handle and upload your vertex data. Check that the buffer was uploaded correctly.
            // 3) Bind the element buffer handle and upload your element data. Check that the buffer was uploaded correctly.

            GL.BindBuffer(BufferTarget.ArrayBuffer, TreeMapVbo.VboID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(verticesLength * BlittableValueType.StrideOf(vertices)), vertices, BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (verticesLength * BlittableValueType.StrideOf(vertices) != size)
                throw new ApplicationException("Vertex data not uploaded correctly");

      
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, TreeMapVbo.EboID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(elementsLength * sizeof(int)), elements, BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (elementsLength * sizeof(int) != size)
                throw new ApplicationException("Element data not uploaded correctly");


            TreeMapVbo.NumElements = elementsLength;
        }

        void Draw()
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

            GL.BindBuffer(BufferTarget.ArrayBuffer, TreeMapVbo.VboID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, TreeMapVbo.EboID);

            GL.VertexPointer(3, VertexPointerType.Float, BlittableValueType.StrideOf(Vertices), new IntPtr(0));
            GL.ColorPointer(4, ColorPointerType.UnsignedByte, BlittableValueType.StrideOf(Vertices), new IntPtr(12));
            GL.NormalPointer(NormalPointerType.Float, BlittableValueType.StrideOf(Vertices), new IntPtr(16));

            GL.DrawElements(BeginMode.Quads, TreeMapVbo.NumElements, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }

        float PanelBorderWidth = 4;
        float NodeBorderWidth = 4;
        float MapWidth = 1000;
        float MapHeight = 1000;

        private void RedrawTreeMap()
        {
            MapWidth = 1000;
            MapHeight = 1000;

            ShowingOutside = Model.ShowOutside && Model.CurrentRoot != Model.InternalRoot;
            ShowingExternal = Model.ShowExternal && !Model.CurrentRoot.XNode.External;

            if (Model.DoRevalue ||
                (Model.ShowLayout != ShowNodes.All && XRay.CoverChange) ||
                (Model.ShowLayout == ShowNodes.Instances && XRay.InstanceChange))
            {
                Model.RecalcCover(Model.InternalRoot);
                Model.RecalcCover(Model.ExternalRoot);

                XRay.CoverChange = false;
                XRay.InstanceChange = false;

                Model.DoRevalue = false;
                Model.DoResize = true;
            }


            if (Model.DoResize)
            {
                float offset = 0.0f;
                float centerWidth = MapWidth;

                Model.PositionMap.Clear();
                Model.CenterMap.Clear();

                if (ShowingOutside)
                {
                    offset = MapWidth * 1.0f / 4.0f;
                    centerWidth -= offset;

                    Model.InternalRoot.SetArea(new RectangleF(0, 0, offset - PanelBorderWidth, MapHeight));
                    Model.PositionMap[Model.InternalRoot.ID] = Model.InternalRoot;
                    SizeNode(Model.InternalRoot, Model.CurrentRoot, false);
                }
                if (ShowingExternal)
                {
                    float extWidth = MapWidth * 1.0f / 4.0f;
                    centerWidth -= extWidth;

                    Model.ExternalRoot.SetArea(new RectangleF(offset + centerWidth + PanelBorderWidth, 0, extWidth - PanelBorderWidth, MapHeight));
                    Model.PositionMap[Model.ExternalRoot.ID] = Model.ExternalRoot;
                    SizeNode(Model.ExternalRoot, null, false);
                }

                Model.CurrentRoot.SetArea(new RectangleF(offset, 0, centerWidth, MapHeight));
                Model.PositionMap[Model.CurrentRoot.ID] = Model.CurrentRoot;
                SizeNode(Model.CurrentRoot, null, true);

                Model.DoResize = false;
            }

            VertexCount = 0;
            ElementCount = 0;

            if (ShowingOutside)
            {
                FillRectangle(BorderBrush, Model.InternalRoot.AreaF.Width, 0, PanelBorderWidth, Model.InternalRoot.AreaF.Height, 0, PlatformHeight);
                DrawNode(Model.InternalRoot, 0, true, PlatformHeight);
            }

            if (ShowingExternal)
            {
                FillRectangle(BorderBrush, Model.ExternalRoot.AreaF.X - PanelBorderWidth, 0, PanelBorderWidth, Model.ExternalRoot.AreaF.Height, 0, PlatformHeight);
                DrawNode(Model.ExternalRoot, 0, true, PlatformHeight);
            }

            DrawNode(Model.CurrentRoot, 0, true, PlatformHeight);
        }

        int VertexCount = 0;
        VertexPositionColor[] Vertices = new VertexPositionColor[1000];

        int ElementCount = 0;
        int[] Elements = new int[1000];

        void FillRectangle(Color color, RectangleF rect, float floor, float ceiling)
        {
            FillRectangle(color, rect.X, rect.Y, rect.Width, rect.Height, floor, ceiling);
        }

        void FillRectangle(Color color, float x, float z, float width, float length, float floor, float ceiling)
        {
            AddNewVerticies(24);

            var v1 = new Vector3(x, floor, z);
            var v2 = new Vector3(x + width, floor, z);
            var v3 = new Vector3(x + width, floor, z + length);
            var v4 = new Vector3(x, floor, z + length);

            var v5 = new Vector3(x, floor + ceiling, z);
            var v6 = new Vector3(x + width, floor + ceiling, z);
            var v7 = new Vector3(x + width, floor + ceiling, z + length);
            var v8 = new Vector3(x, floor + ceiling, z + length);

            // bottom vertices
            var normal = new Vector3(0, -1, 0);
            Vertices[VertexCount++].Set(v1, color, normal);
            Vertices[VertexCount++].Set(v2, color, normal);
            Vertices[VertexCount++].Set(v3, color, normal);
            Vertices[VertexCount++].Set(v4, color, normal);

            // top vertices
            normal = new Vector3(0, 1, 0);
            Vertices[VertexCount++].Set(v8, color, normal);
            Vertices[VertexCount++].Set(v7, color, normal);     
            Vertices[VertexCount++].Set(v6, color, normal);
            Vertices[VertexCount++].Set(v5, color, normal);

            // -z facing vertices
            normal = new Vector3(0, 0, -1);
            Vertices[VertexCount++].Set(v5, color, normal);
            Vertices[VertexCount++].Set(v6, color, normal);
            Vertices[VertexCount++].Set(v2, color, normal);
            Vertices[VertexCount++].Set(v1, color, normal);

            // x facing vertices
            normal = new Vector3(1, 0, 0);
            Vertices[VertexCount++].Set(v6, color, normal);
            Vertices[VertexCount++].Set(v7, color, normal);
            Vertices[VertexCount++].Set(v3, color, normal);
            Vertices[VertexCount++].Set(v2, color, normal);

            // z facing vertices
            normal = new Vector3(0, 0, 1);
            Vertices[VertexCount++].Set(v4, color, normal);
            Vertices[VertexCount++].Set(v3, color, normal);
            Vertices[VertexCount++].Set(v7, color, normal);
            Vertices[VertexCount++].Set(v8, color, normal);
           
            // -x facing vertices
            normal = new Vector3(-1, 0, 0);
            Vertices[VertexCount++].Set(v1, color, normal);
            Vertices[VertexCount++].Set(v4, color, normal);
            Vertices[VertexCount++].Set(v8, color, normal);
            Vertices[VertexCount++].Set(v5, color, normal);
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

        void FillEllipse(Color color, RectangleF rect, float z, float zheight)
        {
            FillEllipse(color, rect.X, rect.Y, rect.Width, rect.Height, z, zheight);
        }

        void FillEllipse(Color color, float x, float y, float width, float height, float z, float zheight)
        {
            if (VertexCount + 4 >= Vertices.Length)
            {
                var newArray = new VertexPositionColor[Vertices.Length * 2];
                Array.Copy(Vertices, newArray, VertexCount);
                Vertices = newArray;
            }

            if (ElementCount + 4 >= Elements.Length)
            {
                var newArray = new int[Elements.Length * 2];
                Array.Copy(Elements, newArray, ElementCount);
                Elements = newArray;
            }

            int firstVertexID = VertexCount;
        }

        private void SizeNode(NodeModel root, NodeModel exclude, bool center)
        {
            if (!root.Show)
                return;

            RectangleF insideArea = root.AreaF;

            /*if (ShowLabels)
            {
                // check if enough room in root box for label
                RectangleF label = new RectangleF(root.AreaF.Location, buffer.MeasureString(root.Name, TextFont));

                float minHeight = (root.Nodes.Count > 0) ? label.Height * 2.0f : label.Height;

                if (root.AreaF.Height > minHeight && root.AreaF.Width > label.Width + LabelPadding * 2.0f)
                {
                    label.X += LabelPadding;
                    label.Y += LabelPadding;

                    insideArea.Y += label.Height;
                    insideArea.Height -= label.Height;

                    root.RoomForLabel = true;
                    root.LabelRect = label;
                }
            }*/

            List<Sector> sectors = new TreeMap(root, exclude, insideArea.Size).Results;

            foreach (Sector sector in sectors)
            {
                var node = sector.OriginalValue;

                sector.Rect = RectangleExtensions.Contract(sector.Rect, NodeBorderWidth);

                if (sector.Rect.X < NodeBorderWidth) sector.Rect.X = NodeBorderWidth;
                if (sector.Rect.Y < NodeBorderWidth) sector.Rect.Y = NodeBorderWidth;
                if (sector.Rect.X > insideArea.Width - NodeBorderWidth) sector.Rect.X = insideArea.Width - NodeBorderWidth;
                if (sector.Rect.Y > insideArea.Height - NodeBorderWidth) sector.Rect.Y = insideArea.Height - NodeBorderWidth;

                sector.Rect.X += insideArea.X;
                sector.Rect.Y += insideArea.Y;

                node.SetArea(sector.Rect);
                Model.PositionMap[node.ID] = node;

                node.RoomForLabel = false; // cant do above without graphic artifacts

                if (center)
                    Model.CenterMap[node.ID] = node;

                if (sector.Rect.Width > 1.0f && sector.Rect.Height > 1.0f)
                    SizeNode(node, exclude, center);
            }
        }


        private void DrawNode(NodeModel node, int depth, bool drawChildren, float z)
        {
            if (!node.Show)
                return;

            //bool pointBorder = node.AreaF.Width < 3.0f || node.AreaF.Height < 3.0f;

            // use a circle for external/outside nodes in the call map
            bool rect = Model.ViewLayout == LayoutType.ThreeD || Model.ViewLayout == LayoutType.TreeMap || Model.CenterMap.ContainsKey(node.ID);

            float zheight = PlatformHeight;
            if (node.ObjType == XObjType.Method)
                zheight = Math.Max(250f * (float)node.SecondaryValue / (float)Model.MaxSecondaryValue, 1);

            var xNode = node.XNode;

            Color pen;

            /*if (FilteredNodes.ContainsKey(node.ID))
                pen = FilteredPen;
            else if (IgnoredNodes.ContainsKey(node.ID))
                pen = IgnoredPen;*/

            if (Model.FocusedNodes.Contains(node))
                pen = ObjColors[(int)node.ObjType];
            else
                pen = ObjColors[(int)node.ObjType];


            // blue selection area
            if (node.Hovered)
            {
                if (depth > OverBrushes.Length - 1)
                    depth = OverBrushes.Length - 1;

                BlendColors(OverBrushes[depth], ref pen);
            }
            //else
            //    FillRectangle(NothingBrush, node.AreaF, z);

            // check if function is an entry point or holding
            if (XRay.FlowTracking && xNode.StillInside > 0)
            {
                if (xNode.EntryPoint > 0)
                {
                    if (XRay.ThreadTracking && xNode.ConflictHit > 0)
                        BlendColors(MultiEntryBrush, ref pen);
                    else
                        BlendColors(EntryBrush, ref pen);
                }
                else
                {
                    if (XRay.ThreadTracking && xNode.ConflictHit > 0)
                        BlendColors(MultiHoldingBrush, ref pen);
                    else
                        BlendColors(HoldingBrush, ref pen);
                }
            }

            // not an else if, draw over holding or entry
            if (xNode.ExceptionHit > 0)
                BlendColors(ExceptionBrush[xNode.FunctionHit], ref pen);

            else if (xNode.FunctionHit > 0)
            {
                if (XRay.ThreadTracking && xNode.ConflictHit > 0)
                    BlendColors(MultiHitBrush[xNode.FunctionHit], ref pen);

                else if (node.ObjType == XObjType.Field)
                {
                    if (xNode.LastFieldOp == FieldOp.Set)
                        BlendColors(FieldSetBrush[xNode.FunctionHit], ref pen);
                    else
                        BlendColors(FieldGetBrush[xNode.FunctionHit], ref pen);
                }
                else
                    BlendColors(HitBrush[xNode.FunctionHit], ref pen);
            }

            if (Model.FocusedNodes.Count > 0 && node.ObjType == XObjType.Class)
            {
                bool dependent = DependentClasses.Contains(node.ID);
                bool independent = IndependentClasses.Contains(node.ID);

                if (dependent && independent)
                    BlendColors(InterdependentBrush, ref pen);

                else if (dependent)
                    BlendColors(DependentBrush, ref pen);

                else if (independent)
                    BlendColors(IndependentBrush, ref pen);
            }

            if (node.SearchMatch && !Model.SearchStrobe)
                BlendColors(SearchMatchBrush, ref pen);

            // if just a point, drawing a border messes up pixels
            /*if (pointBorder)
            {
                if (FilteredNodes.ContainsKey(node.ID))
                    fillFunction(FilteredBrush);
                else if (IgnoredNodes.ContainsKey(node.ID))
                    fillFunction(IgnoredBrush);

                else if (needBorder) // dont draw the point if already lit up
                    fillFunction(ObjBrushes[(int)node.ObjType]);
            }
            else
            {*/

                try
                {
                    if (rect)
                        FillRectangle(pen, node.AreaF, z, zheight);
                    else
                        FillEllipse(pen, node.AreaF, z, zheight);
                }
                catch (Exception ex)
                {
                    File.WriteAllText("debug.txt", string.Format("{0}\r\n{1}\r\n{2}\r\n{3}\r\n{4}\r\n", ex, node.AreaF.X, node.AreaF.Y, node.AreaF.Width, node.AreaF.Height));
                }
            //}

            // draw label
            /*if (ShowLabels && node.RoomForLabel)
            {
                buffer.FillRectangle(LabelBgBrush, node.LabelRect);
                buffer.DrawString(node.Name, TextFont, ObjBrushes[(int)node.ObjType], node.LabelRect);
            }*/


            if (Model.MapMode == TreeMapMode.Dependencies && node.ObjType == XObjType.Class)
                drawChildren = false;

            if (drawChildren && node.AreaF.Width > 1 && node.AreaF.Height > 1)
                foreach (var sub in node.Nodes)
                    DrawNode(sub, depth + 1, drawChildren, z + zheight);
            

            // after drawing children, draw instance tracking on top of it all
            /*if (XRay.InstanceTracking && node.ObjType == XObjType.Class)
            {
               if (XRay.InstanceCount[node.ID] > 0)
                {
                    string count = XRay.InstanceCount[node.ID].ToString();
                    Rectangle x = new Rectangle(node.Area.Location, buffer.MeasureString(count, InstanceFont).ToSize());

                    if (node.Area.Contains(x))
                    {
                        buffer.FillRectangle(NothingBrush, x);
                        buffer.DrawString(count, InstanceFont, InstanceBrush, node.Area.Location.X + 2, node.Area.Location.Y + 2);
                    }
                }
            }*/
        }

        void BlendColors(Color src, ref Color tgt)
        {
            int a = ((src.A * src.A) >> 8) + ((tgt.A * (255 - src.A)) >> 8);
            int r = ((src.R * src.A) >> 8) + ((tgt.R * (255 - src.A)) >> 8);
            int g = ((src.G * src.A) >> 8) + ((tgt.G * (255 - src.A)) >> 8);
            int b = ((src.B * src.A) >> 8) + ((tgt.B * (255 - src.A)) >> 8);

            tgt = Color.FromArgb(a, r, g, b);
        }

        protected void LoadTextures()
        {
            var asm = Assembly.GetExecutingAssembly();
            // loading the original font causes an error in texImage2D even with 8bpp set
            var stream = asm.GetManifestResourceStream("XLibrary.Resources.Font2.png");

            var image = new Bitmap(stream);

            if (image == null)
                return;

            GL.Enable(EnableCap.Texture2D);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            FontTexture = GL.GenTexture();

            image.RotateFlip(RotateFlipType.RotateNoneFlipY);
            
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);

            BitmapData bitmapdata = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, image.PixelFormat);

            // Create Linear Filtered Texture
            GL.BindTexture(TextureTarget.Texture2D, FontTexture);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 256, 256, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bitmapdata.Scan0);

            image.UnlockBits(bitmapdata);
            image.Dispose();

            GL.Disable(EnableCap.Texture2D);
        }

        public void BuildFont()									// Build Our Font Display List
        {
            float cx;											// Holds Our X Character Coord
            float cy;											// Holds Our Y Character Coord

            FontList = GL.GenLists(256);					// Creating 256 Display Lists
            GL.BindTexture(TextureTarget.Texture2D, FontTexture);	// Select Our Font Texture
            for (int loop = 0; loop < 256; loop++)				// Loop Through All 256 Lists
            {
                cx = (float)(loop % 16) / 16.0f;				// X Position Of Current Character
                cy = (float)(loop / 16) / 16.0f;				// Y Position Of Current Character


                GL.NewList(FontList + loop, ListMode.Compile);  // Start Building A List
                GL.Begin(BeginMode.Quads);				        // Use A Quad For Each Character
                GL.TexCoord2(cx, 1 - cy - 0.0625f);			    // Texture Coord (Bottom Left)
                GL.Vertex2(0, 0);							    // Vertex Coord (Bottom Left)
                GL.TexCoord2(cx + 0.0625f, 1 - cy - 0.0625f);   // Texture Coord (Bottom Right)
                GL.Vertex2(16, 0);							    // Vertex Coord (Bottom Right)
                GL.TexCoord2(cx + 0.0625f, 1 - cy);			    // Texture Coord (Top Right)
                GL.Vertex2(16, 16);							    // Vertex Coord (Top Right)
                GL.TexCoord2(cx, 1 - cy);					    // Texture Coord (Top Left)
                GL.Vertex2(0, 16);							    // Vertex Coord (Top Left)
                GL.End();										// Done Building Our Quad (Character)
                GL.Translate(10, 0, 0);						    // Move To The Right Of The Character
                GL.EndList();									// Done Building The Display List
            }													// Loop Until All 256 Are Built
        }


        public void glPrint(int x, int y, string str)	// Where The Printing Happens
        {
            GL.BindTexture(TextureTarget.Texture2D, FontTexture);	// Select Our Font Texture
            GL.Disable(EnableCap.DepthTest);							// Disables Depth Testing
            GL.MatrixMode(MatrixMode.Projection);						// Select The Projection Matrix
            GL.PushMatrix();										// Store The Projection Matrix
            GL.LoadIdentity();									// Reset The Projection Matrix
            GL.Ortho(0, GLView.Width, 0, GLView.Height, -1, 1);						// Set Up An Ortho Screen
            GL.MatrixMode(MatrixMode.Modelview);						// Select The Modelview Matrix
            GL.PushMatrix();										// Store The Modelview Matrix
            GL.LoadIdentity();									// Reset The Modelview Matrix
            GL.Translate(x, y, 0);									// Position The Text (0,0 - Bottom Left)

            GL.ListBase(FontList - 32);	// Choose The Font Set (0 or 1)
            // This is a really, really strange quirk of the CsGL library.  It seems that the glCallLists
            // function, when passed a string, is supposed to be in unicode format, which means that we have
            // to double the length for it to print the full string.  Strange, but it works.
            
            //GL.glCallLists(str.Length * 2, GL.GL_UNSIGNED_BYTE, str);	// Write The Text To The Screen
            GL.CallLists<char>(str.Length, ListNameType.UnsignedByte, str.ToCharArray());
            GL.MatrixMode(MatrixMode.Projection);						// Select The Projection Matrix
            GL.PopMatrix();										// Restore The Old Projection Matrix
            GL.MatrixMode(MatrixMode.Modelview);					// Select The Modelview Matrix
            GL.PopMatrix();										// Restore The Old Projection Matrix
            GL.Enable(EnableCap.DepthTest);								// Enables Depth Testing
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct VertexPositionColor
    {
        public Vector3 Position;
        public uint Color;
        public Vector3 Normal;

        public void Set(Vector3 pos, Color color, Vector3 normal)
        {
            Position = pos;
            Color = ToRgba(color);
            Normal = normal;
        }

        public void Set(float x, float y, float z, Color color)
        {
            Position.X = x;
            Position.Y = y;
            Position.Z = z;

            Color = ToRgba(color);
        }

        public void Set(float x, float y, float z, Color color, float nX, float nY, float nZ)
        {
            Set(x, y, z, color);

            Normal.X = nX;
            Normal.Y = nY;
            Normal.Z = nZ;
        }

        static uint ToRgba(Color color)
        {
            return (uint)color.A << 24 | (uint)color.B << 16 | (uint)color.G << 8 | (uint)color.R;
        }
    }
}
