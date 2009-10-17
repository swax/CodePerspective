using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace XLibrary
{
    public partial class TreePanelWpfContainer : UserControl, ITreePanel
    {
        internal MainForm MainForm;
        internal XNodeIn Root;

        TreePanelWPF PanelWpf;


        public TreePanelWpfContainer(MainForm main, XNodeIn root)
        {
            InitializeComponent();

            MainForm = main;
            Root = root;

            PanelWpf = WpfHost.Child as TreePanelWPF;
            PanelWpf.Init(root);
        }

        public void Redraw()
        {
            PanelWpf.Redraw();
        }

        public XNodeIn GetRoot()
        {
            return Root;
        }

        public void Dispose2()
        {
           
        }
    }
}
