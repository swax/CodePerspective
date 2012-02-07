using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XLibrary
{
    public static class Utilities
    {
        public static string FormatTemplateName(string name)
        {
            return name;

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
            int numT = int.Parse(name.Substring(start, endPos - start));

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
    }
}
