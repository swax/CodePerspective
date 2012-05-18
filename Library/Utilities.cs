using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

namespace XLibrary
{
    public static class Utilities
    {
        public static string FormatTemplateName(string name)
        {
            //return name;

            // i think this is left over by cecil, indicating the class was passed by ref, rare, should fig out more detail
            name = name.TrimEnd('&');

            string originalName = name;

            // make template name more readable
            int pos = name.LastIndexOf('`');

            if (pos == -1)
                return name;

            bool anonFunc = name.StartsWith("Func`");

            int endPos = name.IndexOf('[', pos);

            if (endPos == -1)
                endPos = name.IndexOf('.', pos);

            if (endPos == -1)
                endPos = name.IndexOf(' ', pos);

            bool more = true;
            if (endPos == -1)
            {
                endPos = name.Length;
                more = false;
            }

            int start = pos + 1;
            int numT;
            if (!int.TryParse(name.Substring(start, endPos - start), out numT))
                return name;

            string nameT = name.Substring(0, pos);

            if (numT == 1 && anonFunc)
                nameT += "<TResult>";

            else if (numT == 1 && !anonFunc)
                nameT += "<T>";

            else
            {
                // <T1,T2,T3>
                var tArray = Enumerable.Range(1, numT).Select(i => "T" + i.ToString()).ToArray();

                if (anonFunc)
                    tArray[numT - 1] = "TResult";

                nameT += "<" + string.Join(",", tArray) + ">";
            }


            if (more)
                nameT += " " + name.Substring(endPos);

            return nameT;
        }


        public static void RecurseTree<T>(IEnumerable<T> tree, Action<T> evaluate, Func<T, IEnumerable<T>> recurse)
        {
            foreach (var i in tree)
            {
                evaluate(i);

                IEnumerable<T> subTree = recurse(i);
                if (subTree != null)
                    RecurseTree(subTree, evaluate, recurse);
            }
        }

        public static void IterateParents<T>(T node, Action<T> evaluate, Func<T, T> traverseUp)
        {
            T parent = node;

            while (parent != null)
            {
                evaluate(parent);

                parent = traverseUp(parent);
            }
        }

        public static string TicksToString(long ticks)
        {
            if (ticks == 0)
                return "0";

            double seconds = TicksToSeconds(ticks);

            if (seconds >= 60)
            {
                double minutes = seconds / 60.0;
                return ((int)minutes).ToString("0 m");
            }
            else if (seconds >= 1.0)
                return ((int)seconds).ToString("0 s");

            else if (seconds >= 0.001)
            {
                double ms = seconds * 1000;
                return ((int)ms).ToString("0 ms");
            }
            else
            {
                double us = seconds * 1000 * 1000;
                return ((int)us).ToString("0 µs");
            }
        }

        public static double TicksToSeconds(long ticks)
        {
            return (double)ticks / (double)Stopwatch.Frequency;
        }

        public static void AttachToolTip(this Control control, string text)
        {
            new ToolTip() { AutoPopDelay = 20000 }.SetToolTip(control, text);
        }
    }
}
