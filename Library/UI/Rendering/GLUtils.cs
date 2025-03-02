using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
using OpenTK;
using System.Drawing;
using OpenTK.Mathematics;

namespace XLibrary
{
    public static class GLUtils
    {
        public static void SafeBegin(PrimitiveType mode, Action code)
        {
            GL.Begin(mode);

            code();

            GL.End();
        }

        public static void SafeEnable(EnableCap cap, Action code)
        {
            bool wasEnabled = GL.IsEnabled(cap);
            
            try
            {
                if (!wasEnabled)
                    GL.Enable(cap);
                
                code();
            }
            finally
            {
                if (!wasEnabled)
                    GL.Disable(cap);
            }
        }

        public static void SafeEnable(EnableCap[] caps, Action code)
        {
            bool[] wasEnabled = new bool[caps.Length];
            
            for (int i = 0; i < caps.Length; i++)
            {
                wasEnabled[i] = GL.IsEnabled(caps[i]);
                if (!wasEnabled[i])
                    GL.Enable(caps[i]);
            }

            try
            {
                code();
            }
            finally
            {
                for (int i = 0; i < caps.Length; i++)
                {
                    if (!wasEnabled[i])
                        GL.Disable(caps[i]);
                }
            }
        }

        public static void SafeDisable(EnableCap cap, Action code)
        {
            GL.Disable(cap);

            code();

            GL.Enable(cap);
        }

        public static void SafeGLEnableClientStates(ArrayCap[] caps, Action code)
        {
            foreach (var cap in caps)
                GL.EnableClientState(cap);

            code();

            foreach (var cap in caps)
                GL.DisableClientState(cap);
        }

        public static void SafeSaveMatrix(Action code)
        {
            GL.PushMatrix();

            code();

            GL.PopMatrix();
        }

        public static void SafeBlend(Action code)
        {
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Blend);

            code();

            GL.Disable(EnableCap.Blend);
        }

        public static void BlendColors(Color src, ref Color tgt)
        {
            int a = ((src.A * src.A) >> 8) + ((tgt.A * (255 - src.A)) >> 8);
            int r = ((src.R * src.A) >> 8) + ((tgt.R * (255 - src.A)) >> 8);
            int g = ((src.G * src.A) >> 8) + ((tgt.G * (255 - src.A)) >> 8);
            int b = ((src.B * src.A) >> 8) + ((tgt.B * (255 - src.A)) >> 8);

            tgt = Color.FromArgb(a, r, g, b);
        }

        public static void DrawCube(VertexBuffer vbo, Color color, float x, float z, float width, float length, float bottom, float height)
        {
            var v1 = new Vector3(x, bottom, z);
            var v2 = new Vector3(x + width, bottom, z);
            var v3 = new Vector3(x + width, bottom, z + length);
            var v4 = new Vector3(x, bottom, z + length);

            var v5 = new Vector3(x, bottom + height, z);
            var v6 = new Vector3(x + width, bottom + height, z);
            var v7 = new Vector3(x + width, bottom + height, z + length);
            var v8 = new Vector3(x, bottom + height, z + length);

            // bottom vertices
            var normal = new Vector3(0, -1, 0);
            vbo.AddVerticies(color, normal, v1, v2, v3, v1, v3, v4);

            // top vertices
            normal = new Vector3(0, 1, 0);
            vbo.AddVerticies(color, normal, v8, v7, v6, v8, v6, v5);

            // -z facing vertices
            normal = new Vector3(0, 0, -1);
            vbo.AddVerticies(color, normal, v5, v6, v2, v5, v2, v1);

            // x facing vertices
            normal = new Vector3(1, 0, 0);
            vbo.AddVerticies(color, normal, v6, v7, v3, v6, v3, v2);

            // z facing vertices
            normal = new Vector3(0, 0, 1);
            vbo.AddVerticies(color, normal, v4, v3, v7, v4, v7, v8);

            // -x facing vertices
            normal = new Vector3(-1, 0, 0);
            vbo.AddVerticies(color, normal, v1, v4, v8, v1, v8, v5);
        }

        public static void CheckGLError(string operation)
        {
            ErrorCode error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                throw new ApplicationException($"OpenGL error during {operation}: {error}");
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct VertexPositionColor
    {
        public Vector3 Position;
        public uint Color;
        public Vector3 Normal;

        public void Set(Vector3 pos, Color color)
        {
            Position = pos;
            Color = ToRgba(color);
        }

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
