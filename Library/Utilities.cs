﻿using System;
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

        public static void RecurseTree<T>(T root, Action<T> evaluate, Func<T, IEnumerable<T>> recurse)
        {
            RecurseTree(new T[] { root }, evaluate, recurse);
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

        public static T FindParent<T>(T node, Func<T,bool> evaluate, Func<T, T> traverseUp)
        {
            T parent = node;

            while (parent != null)
            {
                if (evaluate(parent))
                    return parent;

                parent = traverseUp(parent);
            }

            return default(T);
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

        public static string GetMethodName(XNodeIn localNode, int nodeID)
        {
            var node = XRay.Nodes[nodeID];

            var parentClass = node.GetParentClass(false);
            bool includeClass = (parentClass != localNode.GetParentClass(false));

            if (node.ObjType == XObjType.Field)
            {
                string name = node.UnformattedName;

                if (includeClass)
                    name = parentClass.Name + "::" + name;

                if (node.ReturnID != 0)
                {
                    var retNode = XRay.Nodes[node.ReturnID];
                    name = retNode.Name + " " + name;
                }

                return name;
            }

            else if (node.ObjType == XObjType.Method)
            {
                return node.GetMethodName(includeClass);
            }

            return "unknown";
        }

        public static IEnumerable<T> Last<T>(this IEnumerable<T> collection, int n)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            if (n < 0)
                throw new ArgumentOutOfRangeException("n", "n must be 0 or greater");

            LinkedList<T> temp = new LinkedList<T>();

            foreach (var value in collection)
            {
                temp.AddLast(value);
                if (temp.Count > n)
                    temp.RemoveFirst();
            }

            return temp;
        }
    }

    public static class String2
    {
        public static string Join(string separator, IEnumerable<string> items)
        {
            return string.Join(separator, items.ToArray());
        }
    }


    public class Tuple<T1>
    {
        public T1 Item1;

        public Tuple(T1 t1)
        {
            Item1 = t1;
        }

        public override string ToString()
        {
            return Item1.ToString();
        }

        public override int GetHashCode()
        {
            return Item1.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            Tuple<T1> tuple = obj as Tuple<T1>;

            return Item1.Equals(tuple.Item1);
        }
    }

    public class Tuple<T1, T2> : Tuple<T1>
    {
        public T2 Item2;

        public Tuple(T1 t1, T2 t2)
            : base(t1)
        {
            Item2 = t2;
        }

        public override string ToString()
        {
            return base.ToString() + " - " + Item2.ToString();
        }

        public override int GetHashCode()
        {
            return Item2.GetHashCode() ^ base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            Tuple<T1, T2> tuple = obj as Tuple<T1, T2>;

            return Item2.Equals(tuple.Item2) && base.Equals(obj);
        }
    }

    public class SortedSet<T> : IEnumerable<T>
    {
        HashSet<T> Set = new HashSet<T>();
        
        public IEnumerator<T> GetEnumerator()
        {
            foreach (T item in Set.OrderBy(i => i))
                yield return item;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal void Clear()
        {
            Set.Clear();
        }

        internal void Add(T item)
        {
            Set.Add(item);
        }
    }

}
