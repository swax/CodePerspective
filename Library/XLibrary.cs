using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;


namespace XLibrary
{
    public static class XRay
    {
        public static BitArray HitFunctions;

        static Form MainForm;

        static XNodeIn RootNode;

        public static void TestInit(string path)
        {
            LoadNodeMap(path);

            MainForm = new Form();

            MainForm.Controls.Add(new TreePanel(RootNode) { Dock = DockStyle.Fill });

            MainForm.Show();
        }

        [STAThread]
        public static void Init()
        {
            string path = Path.Combine(Application.StartupPath , "XRay.dat");

            LoadNodeMap(path);

            new Thread(ShowGui).Start();
        }

        [STAThread]
        public static void Init(int count)
        {
            HitFunctions = new BitArray(count);

            // load data file

            new Thread(ShowGui).Start();

            // load data file of tree map and function id translations
        }

        public static void ShowGui()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MainForm = new Form();

            MainForm.Controls.Add(new TreePanel(RootNode) { Dock = DockStyle.Fill });

            Application.Run(MainForm);
        }

        static void LoadNodeMap(string path)
        {
            Dictionary<int, XNode> nodeMap = new Dictionary<int, XNode>();

            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                while (stream.Position < stream.Length)
                {
                    XNodeIn node = XNodeIn.Read(stream);
                    nodeMap[node.ID] = node;

                    if (RootNode == null)
                        RootNode = node;

                    if (nodeMap.ContainsKey(node.ParentID))
                    {
                        node.Parent = nodeMap[node.ParentID];
                        node.Parent.Nodes.Add(node);
                    }
                }
            }
        }

        public static void Hit(int thread, int index)
        {
            //XRayDll.XRay.HitFunctions[index] = true;

            // remembering previous hit we could create a known function chain tracking whats called


            if (MainForm == null) 
                return; // wait for gui thread to boot up

            MainForm.BeginInvoke(new Action(() => MessageBox.Show(index + " Called on Thread " + thread)));
        }
    }
}
