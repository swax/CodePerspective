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

        public static Rectangle Contract(this Rectangle rect, int amount)
        {
            Rectangle result = rect;

            if (rect.Width <= amount * 2 || rect.Height <= amount * 2)
                return result;

            rect.X += amount;
            rect.Y += amount;

            rect.Width -= amount * 2;
            rect.Height -= amount * 2;

            return rect;
        }

        public static IEnumerable<T> Do<T>(this IEnumerable<T> list, Action<T> method)
        {
            foreach (T obj in list)
            {
                method(obj);
                yield return obj;
            }
        }
    }


}