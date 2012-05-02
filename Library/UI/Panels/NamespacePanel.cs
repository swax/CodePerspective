using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace XLibrary.UI.Panels
{
    public partial class NamespacePanel : UserControl
    {
        MainForm Main;
        NodeModel SelectedNode;
        IColorProfile ColorProfile = new BrightColorProfile();


        public NamespacePanel()
        {
            InitializeComponent();
        }

        internal void Init(MainForm main)
        {
            Main = main;

            NavButtons.Init(main);
        }

        internal void NavigateTo(NodeModel node)
        {
            if(SelectedNode == node || node == null)
                return;

            SelectedNode = node;

            SummaryLabel.Text = node.Name;
            SummaryLabel.ForeColor = ColorProfile.GetColorForType(node.ObjType);

            SubnodesView.Items.Clear();

            foreach (var subnode in node.Nodes)
                SubnodesView.Items.Add(new SubnodeItem(subnode));

            SubnodesView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        class SubnodeItem : ListViewItem
        {
            public NodeModel Node;

            public SubnodeItem(NodeModel node)
                : base(new string[] { node.Name, node.ObjType.ToString(), "?" })
            {
                Node = node;
            }
        }

        private void SubnodesView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (SubnodesView.SelectedItems.Count == 0)
                return;

            var selected = SubnodesView.SelectedItems[0] as SubnodeItem;

            Main.NavigatePanelTo(selected.Node);
        }
    }
}
