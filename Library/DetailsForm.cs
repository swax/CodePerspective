using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace XLibrary
{
    public partial class DetailsForm : Form
    {
        XNodeIn Node;

        public DetailsForm(XNodeIn node)
        {
            InitializeComponent();

            Reload(node);
        }

        private void Reload(XNodeIn node)
        {            
            Node = node;

            NameLabel.Text = Node.Name;
            TypeLabel.Text = Node.ObjType.ToString();
            ParentsPanel.Controls.Clear();
            ChildrenPanel.Controls.Clear();

            foreach(XNode parent in Node.GetParents())
                AddFlowNode(parent, ParentsPanel);     

            foreach (XNode child in Node.Nodes)
                AddFlowNode(child, ChildrenPanel);

            CallersList.Items.Clear();
            CalledList.Items.Clear();

            // only show caller/callees for functions
            FunctionPanel.Visible = node.ObjType == XObjType.Method;
            if (!FunctionPanel.Visible)
                return;

            int id = node.ID;
            int count = 0;

            
            foreach (FunctionCall call in XRay.CallMap.Values
                .TakeWhile(v => v != null)
                .Where(v => v.Destination == id))
            {
                XNode caller = XRay.Nodes[call.Source];
                CallersList.Items.Add(caller.Name);
                count++;
            }
            CallersLabel.Text = count.ToString() + " Callers";

            count = 0;
            foreach (FunctionCall call in XRay.CallMap.Values
                .TakeWhile(v => v != null)
                .Where(v => v.Source == id))
            {
                XNode called = XRay.Nodes[call.Destination];
                CalledList.Items.Add(called.Name);
                count++;
            }
            CalledLabel.Text = count.ToString() + " Called";
        }

        private void AddFlowNode(XNode node, FlowLayoutPanel panel)
        {
            LinkLabel label = new LinkLabel()
            {
                Text = node.Name,
                AutoSize = true,
                LinkColor = TreePanelGdiPlus.ObjColors[(int)node.ObjType]
            };

            XNodeIn copy = node as XNodeIn; // so delegate works
            label.Click += (s, a) => Reload(copy);

            panel.Controls.Add(label);
        }

    }
}
