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

        static TreeForm MainForm;

        internal static XNodeIn RootNode;

        internal const int HitFrames = 10;
        internal static int HitIndex;
        
        internal static BitArray[] HitFunctions;
        static int FunctionCount;

        public static void TestInit(string path)
        {
            LoadNodeMap(path);

            MainForm = new TreeForm();
            MainForm.Show();
        }

        
        public static void Init()
        {
            string path = Path.Combine(Application.StartupPath , "XRay.dat");

            LoadNodeMap(path);

            HitFunctions = new BitArray[HitFrames];

            FunctionCount++; // so id can be accessed in 0 based index

            for (int i = 0; i < HitFrames; i++)
                HitFunctions[i] = new BitArray(FunctionCount);


            Thread gui = new Thread(ShowGui);
            gui.SetApartmentState(ApartmentState.STA);
            gui.Start();
        }

        public static void ShowGui()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MainForm = new TreeForm();
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

                    if (node.ID > FunctionCount)
                        FunctionCount = node.ID; 
                }
            }
        }

        public static void Hit(int thread, int index)
        {

            //XRayDll.XRay.HitFunctions[index] = true;

            // remembering previous hit we could create a known function chain tracking whats called


            if (MainForm == null) 
                return; // wait for gui thread to boot up

            HitFunctions[HitIndex][index] = true;
            
            //MainForm.BeginInvoke(new Action(() => MessageBox.Show(index + " Called on Thread " + thread)));
        }
    }
}
