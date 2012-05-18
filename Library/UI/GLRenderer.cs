﻿using System;
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

        void GLRenderer_Load(object sender, EventArgs e)
        {
            SetupViewport();

            GL.ClearColor(Model.XColors.BackgroundColor);

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

            Model.Render();

            //glPrint(10, 10, "HELLOOOOOOOO");

            SwapBuffers();

            Model.FpsCount++;
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

            var config = new QFontBuilderConfiguration() { TextGenerationRenderHint = TextGenerationRenderHint.SystemDefault };

            qfont = new QFont(font, config);
            qfont.Options.TransformToViewport = null;
            qfont.Options.WordWrap = false;

            FontMap[font] = qfont;
            return qfont;
        }

        public void DrawString(string text, Font font, Color color, PointF point)
        {
            DrawString(text, font, color, point.X, point.Y);
        }

        public void DrawString(string text, Font font, Color color, float x, float y)
        {
            QFont qfont = GetQFont(font);

            qfont.Options.Colour = new Color4(color.R, color.G, color.B, color.A);

            qfont.Print(text, new Vector2(x, y));
        }

        public void DrawString(string text, Font font, Color color, RectangleF rect)
        {
            QFont qfont = GetQFont(font);

            qfont.Options.Colour = new Color4(color.R, color.G, color.B, color.A);

            qfont.Print(text, rect.Width, QFontAlignment.Left, new Vector2(rect.X, rect.Y));
        }

        public void FillEllipse(Color color, RectangleF area)
        {
            SafeGLBlend(() =>
            {
                SafeGLBegin(BeginMode.Polygon, () =>
                {
                    GL.Color4(color);

                    DrawEllipseVerticies(area.X, area.Y, area.Width, area.Height);
                });
            });
        }

        public void FillRectangle(Color color, RectangleF area)
        {
            FillRectangle(color, area.X, area.Y, area.Width, area.Height);
        }

        public void FillRectangle(Color color, float x, float y, float width, float height)
        {
            SafeGLBlend(() =>
            {
                SafeGLBegin(BeginMode.Quads, () =>
                {
                    GL.Color4(color);

                    GL.Vertex2(x, y);
                    GL.Vertex2(x + width, y);
                    GL.Vertex2(x + width, y + height);
                    GL.Vertex2(x, y + height);
                });
            });
        }

        public void DrawEllipse(Color color, int lineWidth, float x, float y, float width, float height)
        {
            GL.LineWidth(lineWidth);

            SafeGLBlend(() =>
            {
                SafeGLBegin(BeginMode.LineLoop, () =>
                {
                    GL.Color4(color);

                    DrawEllipseVerticies(x, y, width, height);
                });
            });
        }

        private static void DrawEllipseVerticies(float x, float y, float width, float height)
        {
            double segments = 8.0;

            // http://stackoverflow.com/questions/5886628/effecient-way-to-draw-ellipse-with-opengl-or-d3d

            double cx = x + width / 2.0;
            double cy = y + height / 2.0;

            double theta = 2.0 * 3.1415926 / segments;
            double c = Math.Cos(theta); //precalculate the sine and cosine
            double s = Math.Sin(theta);
            double t = 0;

            double xPos = width / 2.0; //we start at angle = 0 
            double yPos = 0;

            for (int ii = 0; ii < segments; ii++)
            {
                GL.Vertex2(xPos + cx, yPos + cy);//output vertex 

                //apply the rotation matrix
                t = xPos;
                xPos = c * xPos - s * yPos;
                yPos = s * t + c * yPos;
            }
        }

        public void DrawRectangle(Color color, int lineWidth, float x, float y, float width, float height)
        {
            GL.LineWidth(lineWidth);

            SafeGLBlend(() =>
            {
                SafeGLBegin(BeginMode.LineLoop, () =>
                {
                    GL.Color4(color);

                    GL.Vertex2(x, y);
                    GL.Vertex2(x + width, y);
                    GL.Vertex2(x + width, y + height);
                    GL.Vertex2(x, y + height);
                });
            });
        }

        public void DrawLine(Color color, int lineWidth, PointF start, PointF end, bool dashed)
        {
            GL.LineWidth(lineWidth);

            SafeGLBlend(() =>
            {
                if (!dashed)
                    RenderLine(color, start, end);
                else
                    SafeGLEnable(EnableCap.LineStipple, () =>
                    {
                        //1111 1000 0000 0000
                        short pattern = (short)(0xF800 >> (XRay.DashOffset * 5));
                        GL.LineStipple(1, pattern);
                        RenderLine(color, start, end);
                    });
            });
        }

        private void RenderLine(Color color, PointF start, PointF end)
        {
            SafeGLEnable(EnableCap.LineSmooth, () =>
            {
                SafeGLBegin(BeginMode.Lines, () =>
                {
                    GL.Color4(color);

                    GL.Vertex2(start.X, start.Y);
                    GL.Vertex2(end.X, end.Y);
                });
            });
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

        public void SafeGLBegin(BeginMode mode, Action code)
        {
            GL.Begin(mode);

            code();

            GL.End();
        }

        public void SafeGLEnable(EnableCap cap, Action code)
        {
            GL.Enable(cap);

            code();

            GL.Disable(cap);
        }

        public void SafeGLBlend(Action code)
        {
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Blend);

            code();

            GL.Disable(EnableCap.Blend);
        }
    }
}
