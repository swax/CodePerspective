using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media.Media3D;

namespace XLibrary
{
    public static class Xtensions
    {
        public static string[] SplitNextLine(this StreamReader reader, StringBuilder il)
        {
            string line = reader.ReadLine();

            il.AppendLine(line);

            return line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static void RemoveLine(this StringBuilder builder)
        {
            for (int i = builder.Length - 1 - 2; i > 0; i--) // backup past current line
                if (builder[i] == '\r' && builder[i + 1] == '\n')
                {
                    builder.Length = i + 2;
                    break;
                }
        }

        public static void Write(this FileStream stream, byte[] buffer)
        {
            stream.Write(buffer, 0, buffer.Length);
        }

        public static byte[] Read(this FileStream stream, int size)
        {
            byte[] buffer = new byte[size];
            stream.Read(buffer, 0, size);
            return buffer;
        }

        public static string FilterComment(this string line)
        {
            int pos = line.LastIndexOf("//");

            if (pos != -1)
                line = line.Substring(0, pos);

            return line;
        }

        public static IEnumerable<T> Do<T>(this IEnumerable<T> list, Action<T> method)
        {
            foreach (T obj in list)
            {
                method(obj);
                yield return obj;
            }
        }

        public static Point3D Move(this Point3D start, double x, double y, double z)
        {
            return new Point3D(start.X + x, start.Y + y, start.Z + z);
        }

        public static PointF UpperLeftCorner(this RectangleF rect)
        {
            return rect.Location;
        }

        public static PointF UpperRightCorner(this RectangleF rect)
        {
            return new PointF(rect.X + rect.Width, rect.Y);
        }

        public static PointF LowerLeftCorner(this RectangleF rect)
        {
            return new PointF(rect.X, rect.Y + rect.Height);
        }

        public static PointF LowerRightCorner(this RectangleF rect)
        {
            return new PointF(rect.X + rect.Width, rect.Y + rect.Height);
        }
    }


}