using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace XLibrary.Panels
{
    public partial class ConsolePanel : UserControl
    {
        public MainForm Main;
        public XNodeIn[] Nodes = XRay.Nodes;
        public GdiRenderer Tree;


        public ConsolePanel()
        {
            InitializeComponent();
        }

        public void Init(MainForm main)
        {
            Main = main;
            Tree = main.GdiView;

            ProcessInput("help");
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return && InputBox.Text != "")
            {
                ProcessInput(InputBox.Text);
                InputBox.Text = "";
                e.Handled = true;
            }
        }

        private void ProcessInput(string input)
        {
            string result = "";
            input = input.Trim();
            try
            {
                result = RunCommand(input);
            }
            catch (Exception ex)
            {
                result = "Exception: " + ex.Message;
            }

            ConsoleBox.AppendText("\r\n> " + input + "\r\n" + result);
            ConsoleBox.SelectionStart = ConsoleBox.Text.Length;
            ConsoleBox.ScrollToCaret();
        }

        string EvalBody = @"
            using System;

            public delegate void Proc();

            public class Wrapper 
            { 
                public static object Set(string name, object value) 
                { 
                    AppDomain.CurrentDomain.SetData(name, value);
                    return value; 
                }

                public static object Get(string name) 
                { 
                    return AppDomain.CurrentDomain.GetData(name);
                }

                public static object Invoke(Proc proc) 
                { 
                    proc();
                    return null; 
                }

                public static object Eval(dynamic x) 
                { 
                    return x.{0}; 
                }
            }";

        string RunCommand(string input)
        {
            StringBuilder output = new StringBuilder();

            if(string.Compare(input, "help", true) == 0)
            {
                return
@"Commands: 
calls
datpath
eval Nodes, Main, Tree
findid <name>
help
id <#>
inits
log
settings
stacks";
            }

            else if(input.StartsWith("eval "))
            {
                string exp = input.Replace("eval ", "");
                return Eval(exp);
            }

            else if (string.Compare(input, "datpath", true) == 0)
            {
                return XRay.DatPath;
            }

            else if (string.Compare(input, "settings", true) == 0)
            {
                output.AppendLine("InstanceTracking: " + XRay.InstanceTracking);
                output.AppendLine("ThreadTracking: " + XRay.ThreadTracking);
                output.AppendLine("FlowTracking: " + XRay.FlowTracking);

                return output.ToString();
            }

            else if (string.Compare(input, "stacks", true) == 0)
            {
                foreach (ThreadFlow flow in XRay.FlowMap)
                {
                    if (flow == null)
                        continue;

                    output.AppendFormat("Thread: {0}\r\n", flow.ThreadID);

                    for (int x = 0; x <= flow.Pos; x++)
                    {
                        XNodeIn node = XRay.Nodes[flow.Stack[x].NodeID];
                        if (node != null)
                            output.AppendFormat("{0}: ID: {1}, Inside: {2}\r\n", x, node.ID, node.StillInside);//, (XRay.NodeMap[flow.Stack[x]].StillInside > 0));
                    }
                }

                return output.ToString();
            }
            else if (string.Compare(input, "calls", true) == 0)
            {

                // function calls
                output.AppendLine("Method Call Map:");
                foreach (var call in XRay.CallMap)
                    output.AppendFormat("{0} -> {1}: Hit: {2}, Inside: {3}\r\n", call.Source, call.Destination, call.Hit, call.StillInside);

                output.AppendLine("");
                output.AppendLine("Class Call Map:");
                foreach (var call in XRay.ClassCallMap)
                    output.AppendFormat("{0} -> {1}: Hit: {2}, Inside: {3}\r\n", call.Source, call.Destination, call.Hit, call.StillInside);

                return output.ToString();
            }

            else if (string.Compare(input, "inits", true) == 0)
            {
                foreach (var init in XRay.InitMap)
                    output.AppendFormat("{0} -> {1}\r\n", init.Source, init.Destination);

                return output.ToString();
            }

            else if (string.Compare(input, "log", true) == 0)
            {
                foreach (string error in XRay.ErrorLog)
                    output.AppendLine(error);

                return output.ToString();
            }

            else if (input.StartsWith("id "))
            {
                int id = int.Parse(input.Replace("id ", ""));

                return XRay.Nodes[id].FullName();
            }

            else if (input.StartsWith("findid "))
            {
                string find = input.Replace("findid ", "");

                var results = XRay.Nodes.Where(n => n.Name.Contains(find)).Select(n => n.ID + ": " + n.Name);

                return string.Join("\r\n", results);
            }

            else
            {
                return "Unrecognized commands, try help";
            }     
        }

        string Eval(string expr)
        {
            string program = EvalBody.Replace("{0}", expr);

            var provider = CodeDomProvider.CreateProvider("C#");
            CompilerParameters cp = new CompilerParameters();
            cp.ReferencedAssemblies.Add("System.Core.dll");
            cp.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
            cp.GenerateExecutable = false;
            cp.GenerateInMemory = true;

            CompilerResults results = provider.CompileAssemblyFromSource(cp, program);

            if (results.Errors.HasErrors)
            {
                if (results.Errors[0].ErrorNumber == "CS0029")
                    return Eval("Invoke(delegate { " + expr + "; })");

                return results.Errors[0].ErrorText;
            }
            else
            {
                Assembly assm = results.CompiledAssembly;
                Type target = assm.GetType("Wrapper");
                MethodInfo method = target.GetMethod("Eval");

                try
                {
                    object result = method.Invoke(null, new object[] { this });

                    return result == null ? "null" : result.ToString();
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }
    }
}
