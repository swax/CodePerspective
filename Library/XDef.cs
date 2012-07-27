using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace XLibrary
{

    public class XDef
    {
        public enum XDefType { Unknown, Root, Namespace, Class };

        public XDefType DefType;
        public string Name;

        public XDef SubDef;

        //public XDef[] Arrays;
        public string Arrays = "";

        public XDef[] Generics;
        public int? GenericCount;

        public string Remains = "";
        public String Mods;


        public XDef() { }

        public XDef(string root)
        {
            // end of field may specify modopt/req
            var cutPos = int.MaxValue;
            var comparePos = root.IndexOf("modopt(");
            if (comparePos != -1)
                cutPos = comparePos;

            comparePos = root.IndexOf("modreq(");
            if (comparePos != -1 && comparePos < cutPos)
                cutPos = comparePos;

            if (cutPos != -1 && cutPos != int.MaxValue)
            {
                Mods = root.Substring(cutPos);
                root = root.Substring(0, cutPos);
            }

            DefType = XDefType.Root;

            SubDef = new XDef();

            Remains = SubDef.Parse(root);
        }

        public string Parse(string input)
        {
            string working = "";
            bool remainingMode = false;

            for (int i = 0; i < input.Length; i++)
            {
                char current = input[i];

                // read input until . dividing namespaces, prevent messing up when name is 0...
                if (current == '.')// && working != "" && !int.TryParse(working, out dummy))
                {
                    // create namespace for previous
                    DefType = XDefType.Namespace;
                    Name = working;

                    SubDef = new XDef();
                    return SubDef.Parse(input.Substring(i + 1));
                }

                else if (current == '/')
                {
                    // create namespace for previous
                    DefType = XDefType.Class;
                    Name = working;

                    SubDef = new XDef();
                    return SubDef.Parse(input.Substring(i + 1));
                }

                // if the class has  < or > in it's name before the ` is found, then ignore
                else if (input[i] == '<' && input[i + 1] == '>')
                {
                    working += "<>";
                    i++;
                }

                else if (i == 0 && input[i] == '<')
                {
                    while (input[i] != '>')
                    {
                        working += input[i];
                        i++;
                    }
                    working += input[i];
                }

                // if divide between classes in template
                else if (current == ',' || current == '>')
                {
                    // create class for previous
                    DefType = XDefType.Class;
                    Name = working;

                    return input.Substring(i);
                }

                // else if array def
                else if (current == '[')
                {
                    while (input[i] != ']')
                    {
                        Arrays += input[i];
                        i++;
                    }
                    Arrays += input[i];
                    remainingMode = true;
                }

                // else if template
                else if (current == '<' || current == '`')
                {
                    // create class for previous
                    DefType = XDefType.Class;
                    Name = working;

                    if (current == '`')
                    {
                        // while not <, build up number
                        string num = "";
                        i++;
                        while (input[i] != '<' && input[i] != '/')
                        {
                            num += input[i];
                            i++;

                            if (i == input.Length)
                                break;
                        }

                        GenericCount = int.Parse(num);

                        // sometimes generics not specified
                        if (i == input.Length)
                            return "";

                        // if nested class
                        if (input[i] == '/')
                        {
                            SubDef = new XDef();
                            return SubDef.Parse(input.Substring(i + 1));
                        }
                    }

                    var genericsList = new List<XDef>();

                    if (working == "std::less")
                    {
                        int y = 0;
                        y++;
                    }

                    // parse class name and add to template
                    input = input.Substring(i);

                    while (input[0] != '>')
                    {
                        var def = new XDef();

                        input = def.Parse(input.Substring(1));

                        genericsList.Add(def);
                    }

                    Generics = genericsList.ToArray();

                    i = 0;
                    remainingMode = true;
                    //return input.Substring(1);
                }
                else if (current == ' ' || remainingMode)
                {
                    remainingMode = true;


                    if (input.IndexOf("modopt") != -1 || input.IndexOf("modreq") != -1)
                    {
                        Remains += input.Substring(i);
                        i = input.Length;
                    }
                    else
                        Remains += current;

                }
                else
                {
                    working += current;
                }
            }

            // end of input
            DefType = XDefType.Class;
            Name = working;
            return "";
        }

        public string GetFullName()
        {
            string fullname = Name;

            if (DefType == XDefType.Root)
                fullname = SubDef.GetFullName();

            if (DefType == XDefType.Namespace)
                fullname += "." + SubDef.GetFullName();

            if (GenericCount != null)
                fullname += "`" + GenericCount.Value.ToString();

            if (Generics != null)
                fullname += "<" + string.Join(",", Generics.Select(g => g.GetFullName())) + ">";

            if (Arrays != null)
                fullname += Arrays;
            //fullname += "[" + string.Join(",", Arrays.Select(g => g.GetFullName())) + "]";

            // nested classes
            if (DefType == XDefType.Class && SubDef != null)
                fullname += "/" + SubDef.GetFullName();

            if (Remains != null)
                fullname += Remains;

            if (Mods != null)
                fullname += Mods;

            return fullname;
        }

        public static XDef ParseAndCheck(string input)
        {
            XDef def = new XDef(input);

            string fullname = def.GetFullName();

            Debug.Assert(fullname == input);

            return def;
        }

        public static void Test()
        {
            ParseAndCheck("System.Collections.Generic.List`1/Enumerator<System.String>");
            ParseAndCheck("System.Single[0...,0...]");
            ParseAndCheck("Newtonsoft.Json.Utilities.DictionaryWrapper`2/DictionaryEnumerator`2<TKey,TValue,TKey,TValue>");
            ParseAndCheck("std._Tree_const_iterator<std::_Tree_val<std::_Tmap_traits<CefStringBase<CefStringTraitsUTF16>,CefStringBase<CefStringTraitsUTF16>,std::less<CefStringBase<CefStringTraitsUTF16> >,std::allocator<std::pair<CefStringBase<CefStringTraitsUTF16> const ,CefStringBase<CefStringTraitsUTF16> > >,1> > >");
            ParseAndCheck("CefStructBase<CefSettingsTraits>* modopt(System.Runtime.CompilerServices.IsConst) modopt(System.Runtime.CompilerServices.IsConst)");
            ParseAndCheck("std._Tree_val<std::_Tmap_traits<CefStringBase<CefStringTraitsUTF16>,CefStringBase<CefStringTraitsUTF16>,std::less<CefStringBase<CefStringTraitsUTF16> >,std::allocator<std::pair<CefStringBase<CefStringTraitsUTF16> const ,CefStringBase<CefStringTraitsUTF16> > >,1> >* modopt(System.Runtime.CompilerServices.CallConvThiscall)");
            ParseAndCheck("System.Collections.Generic.KeyValuePair`2<TKey,TVal>[]");
            ParseAndCheck("System.Single[]&");
        }

        public string GetShortName()
        {
            // decompiler calls this to get name of class

            // this method calls GetShortName to get short name for generics

            XDef def = this;

            if (DefType != XDefType.Class)
            {
                while (true)
                {
                    if (def.SubDef == null)
                        break;

                    def = def.SubDef;
                }
            }

            Debug.Assert(def.DefType == XDefType.Class);

            string shortName = def.Name;

            if (def.Generics != null)
                shortName += "<" + string.Join(",", def.Generics.Select(g => g.GetShortName())) + ">";

            if (def.Arrays != null)
                shortName += def.Arrays;

            return shortName;
        }
    }
}
