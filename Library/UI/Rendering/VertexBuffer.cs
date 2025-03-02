using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace XLibrary
{
    public class VertexBuffer : IDisposable
    {
        private int _vboId = 0;
        private int _vertexCount = 0;
        private bool _initialized = false;
        private List<Vector3> _vertices = new List<Vector3>();
        private List<Vector3> _normals = new List<Vector3>();
        private List<System.Drawing.Color> _colors = new List<System.Drawing.Color>();
        private readonly object _syncLock = new object();

        public int VertexCount => _vertexCount;

        public bool IsInitialized => _initialized;

        public void Init()
        {
            if (_initialized)
                return;

            lock (_syncLock)
            {
                if (_vboId == 0)
                {
                    GL.GenBuffers(1, out _vboId);
                }
                _initialized = true;
            }
        }

        public void Reset()
        {
            lock (_syncLock)
            {
                _vertices.Clear();
                _normals.Clear();
                _colors.Clear();
                _vertexCount = 0;
            }
        }

        public void AddVerticies(System.Drawing.Color color, Vector3 normal, params Vector3[] vertices)
        {
            if (vertices == null || vertices.Length == 0)
                return;

            lock (_syncLock)
            {
                foreach (var vertex in vertices)
                {
                    _vertices.Add(vertex);
                    _normals.Add(normal);
                    _colors.Add(color);
                }
                _vertexCount += vertices.Length;
            }
        }

        public void Load()
        {
            if (_vertexCount == 0)
                return;

            if (!_initialized || _vboId == 0)
            {
                throw new InvalidOperationException("VertexBuffer not initialized. Call Init() first.");
            }

            lock (_syncLock)
            {
                try
                {
                    // Calculate buffer size
                    int vertexSize = Vector3.SizeInBytes;
                    int colorSize = 4 * sizeof(float); // R,G,B,A as floats
                    int stride = vertexSize + vertexSize + colorSize;
                    int bufferSize = stride * _vertexCount;

                    // Create interleaved array for vertices, normals and colors
                    float[] buffer = new float[_vertexCount * 10]; // 3 for vertex, 3 for normal, 4 for color
                    
                    for (int i = 0; i < _vertexCount; i++)
                    {
                        int idx = i * 10;
                        
                        // Vertex
                        buffer[idx + 0] = _vertices[i].X;
                        buffer[idx + 1] = _vertices[i].Y;
                        buffer[idx + 2] = _vertices[i].Z;
                        
                        // Normal
                        buffer[idx + 3] = _normals[i].X;
                        buffer[idx + 4] = _normals[i].Y;
                        buffer[idx + 5] = _normals[i].Z;
                        
                        // Color
                        buffer[idx + 6] = _colors[i].R / 255.0f;
                        buffer[idx + 7] = _colors[i].G / 255.0f;
                        buffer[idx + 8] = _colors[i].B / 255.0f;
                        buffer[idx + 9] = _colors[i].A / 255.0f;
                    }

                    // Bind and upload data
                    GL.BindBuffer(BufferTarget.ArrayBuffer, _vboId);
                    GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(buffer.Length * sizeof(float)), buffer, BufferUsageHint.DynamicDraw);

                    // Verify upload
                    int uploadedSize;
                    GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out uploadedSize);

                    if (uploadedSize != buffer.Length * sizeof(float))
                    {
                        throw new ApplicationException($"Vertex data not uploaded correctly: Expected {buffer.Length * sizeof(float)} bytes but uploaded {uploadedSize} bytes");
                    }

                    // Unbind buffer
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Vertex data not uploaded correctly", ex);
                }
            }
        }

        public void Draw(PrimitiveType primitiveType)
        {
            if (_vertexCount == 0 || !_initialized || _vboId == 0)
                return;

            lock (_syncLock)
            {
                try
                {
                    GL.EnableClientState(ArrayCap.VertexArray);
                    GL.EnableClientState(ArrayCap.NormalArray);
                    GL.EnableClientState(ArrayCap.ColorArray);

                    GL.BindBuffer(BufferTarget.ArrayBuffer, _vboId);

                    GL.VertexPointer(3, VertexPointerType.Float, 10 * sizeof(float), IntPtr.Zero);
                    GL.NormalPointer(NormalPointerType.Float, 10 * sizeof(float), (IntPtr)(3 * sizeof(float)));
                    GL.ColorPointer(4, ColorPointerType.Float, 10 * sizeof(float), (IntPtr)(6 * sizeof(float)));

                    GL.DrawArrays(primitiveType, 0, _vertexCount);

                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                    GL.DisableClientState(ArrayCap.VertexArray);
                    GL.DisableClientState(ArrayCap.NormalArray);
                    GL.DisableClientState(ArrayCap.ColorArray);
                }
                catch (Exception)
                {
                    // Handle drawing errors gracefully
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                    GL.DisableClientState(ArrayCap.VertexArray);
                    GL.DisableClientState(ArrayCap.NormalArray);
                    GL.DisableClientState(ArrayCap.ColorArray);
                }
            }
        }

        public void Dispose()
        {
            if (_vboId != 0)
            {
                GL.DeleteBuffer(_vboId);
                _vboId = 0;
            }
            _initialized = false;
            Reset();
        }

        ~VertexBuffer()
        {
            Dispose();
        }
    }
}
