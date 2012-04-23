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
        public MainForm MainView;

        public ConsolePanel()
        {
            InitializeComponent();
        }

        public void Init(MainForm mainView)
        {
            MainView = mainView;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return && InputBox.Text != "")
            {
                string result = StringEval(InputBox.Text);

                ConsoleBox.AppendText("\r\n> " + InputBox.Text + "\r\n" + result);
                ConsoleBox.SelectionStart = ConsoleBox.Text.Length;
                ConsoleBox.ScrollToCaret();


                InputBox.Text = "";
                e.Handled = true;
            }
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

        string StringEval(string expr)
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
                    return StringEval("Invoke(delegate { " + expr + "; })");

                return results.Errors[0].ErrorText;
            }
            else
            {
                Assembly assm = results.CompiledAssembly;
                Type target = assm.GetType("Wrapper");
                MethodInfo method = target.GetMethod("Eval");

                try
                {
                    var x = new CommandPackage(this);

                    object result = method.Invoke(null, new object[] { x });

                    return result == null ? "null" : result.ToString();
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }
    }

    public class CommandPackage
    {
        public XNodeIn[] Nodes = XRay.Nodes;
        public MainForm  Main;
        public TreePanelGdiPlus Tree;

        public CommandPackage(ConsolePanel panel)
        {
            Main = panel.MainView;
            Tree = Main.TreeView;
        }

        /*public object GetTestBps()
        {
            return XRay.GetSimBps();
        }*/

        public string Help = "Available Commands: Nodes, Main, Tree";
    }

}
