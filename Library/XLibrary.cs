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
        internal static Dictionary<int, XNode> NodeMap = new Dictionary<int, XNode>();

        internal const int HitFrames = 10;
        internal static int HitIndex;
        internal static bool ShowOnlyHit;

        internal static bool CoverChange;
        internal static BitArray CoveredFunctions;

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

            CoveredFunctions = new BitArray(FunctionCount);

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
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                while (stream.Position < stream.Length)
                {
                    XNodeIn node = XNodeIn.Read(stream);
                    NodeMap[node.ID] = node;

                    if (RootNode == null)
                        RootNode = node;

                    if (NodeMap.ContainsKey(node.ParentID))
                    {
                        node.Parent = NodeMap[node.ParentID];
                        node.Parent.Nodes.Add(node);
                    }

                    if (node.ID > FunctionCount)
                        FunctionCount = node.ID; 
                }
            }
        }

        public static void Hit(int thread, int index)
        {
            //if (MainForm == null) 
            //    return; // wait for gui thread to boot up

            if (!CoveredFunctions[index] && NodeMap.ContainsKey(index))
            {
                CoverChange = true;

                XNode node = NodeMap[index];
                while (node != null)
                {
                    CoveredFunctions[node.ID] = true;
                    node = node.Parent;
                }
                // clear cover change on paint
            }

            HitFunctions[HitIndex][index] = true;
        }
    }
}
