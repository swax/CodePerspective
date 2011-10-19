using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace XLibrary
{
    public partial class MainForm : Form
    {
        public TreePanelGdiPlus TreeView;


        public MainForm()
        {
            InitializeComponent();

            TreeView = new TreePanelGdiPlus(this) { Dock = DockStyle.Fill };
            ViewHostPanel.Controls.Add(TreeView);

            ResetTimer.Interval = 1000 / XRay.HitFrames;
            ResetTimer.Enabled = true;

            RevalueTimer.Interval = 1000;
            RevalueTimer.Enabled = true;

            Text = "c0re XRay";

            DisplayTab.Init(TreeView);
            ConsoleTab.Init(this);

            TreeView.SetRoot(TreeView.CurrentRoot); // init first node in history
        }

        public void UpdateBreadCrumbs()
        {
            foreach(var item in MainToolStrip.Items.OfType<ToolStripSplitButton>().ToArray())
                MainToolStrip.Items.Remove(item);

            /*var label = new ToolStripLabel("Viewing: ");
            label.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            MainToolStrip.Items.Add(label);*/

            List<XNodeIn> crumbs = new List<XNodeIn>();

            // iterate up tree
            var node = TreeView.CurrentRoot;

            while (node != null && node.ObjType != XObjType.Root)
            {
                crumbs.Insert(0, node);

                node = node.Parent as XNodeIn;
            }

            // add crumbs
            foreach (var crumb in crumbs)
            {
                var crumbCopy = crumb;

                var button = new ToolStripSplitButton(crumb.Name);
                button.ButtonClick += (s, e) => TreeView.SetRoot(crumbCopy);
                button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                button.ForeColor = TreePanelGdiPlus.ObjColors[(int)crumb.ObjType];
                MainToolStrip.Items.Add(button);

                foreach (var sub in crumb.Nodes.OrderBy(n => n, new CompareNodes()))
                {
                    var subCopy = sub as XNodeIn;
                    
                    var item = new ToolStripMenuItem(sub.Name, null,  (s, e) => TreeView.SetRoot(subCopy));
                    item.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                    item.ForeColor = TreePanelGdiPlus.ObjColors[(int)sub.ObjType];

                    button.DropDownItems.Add(item);
                }
            }

            BackButton.Enabled = (TreeView.CurrentHistory != null && TreeView.CurrentHistory.Previous != null);
            ForwardButton.Enabled = (TreeView.CurrentHistory != null && TreeView.CurrentHistory.Next != null);
        }

        private void ResetTimer_Tick(object sender, EventArgs e)
        {
            if (XRay.Nodes == null)
                return;

            for (int i = 0; i < XRay.Nodes.Length; i++)
            {
                XNodeIn node = XRay.Nodes[i];

                if (node != null)
                {
                    if (node.FunctionHit > 0)
                        node.FunctionHit--;

                    if (XRay.ThreadTracking &&
                        node.ConflictHit > 0)
                        node.ConflictHit--;
                }
            }

            // reset
            if (XRay.FlowTracking)
            {
                // time out function calls
                TimeoutFunctinCalls(XRay.CallMap);
                TimeoutFunctinCalls(XRay.ClassCallMap);
            }

            TreeView.Redraw();
        }

        void TimeoutFunctinCalls(SharedDictionary<FunctionCall> callMap)
        {
            foreach (FunctionCall call in callMap)
            {
                if (call == null || call.Hit <= 0)
                    continue;
                
                call.Hit--;

                call.DashOffset -= FunctionCall.DashSize;
                if (call.DashOffset < 0)
                    call.DashOffset = FunctionCall.DashSpace;
            }
        }

        private void RevalueTimer_Tick(object sender, EventArgs e)
        {
            if (TreeView.SizeLayout == SizeLayouts.TimeInMethod ||
                TreeView.SizeLayout == SizeLayouts.Hits ||
                TreeView.SizeLayout == SizeLayouts.TimePerHit)
            {
                TreeView.RecalcValues();
            }
        }



        private void BackButton_Click(object sender, EventArgs e)
        {
            TreeView.NavBack();
        }

        private void ForwardButton_Click(object sender, EventArgs e)
        {
            TreeView.NavForward();
        }
    }

    public class CompareNodes : IComparer<XNode>
    {
        public int Compare(XNode s1, XNode s2)
        {
            if (s1.ObjType != s2.ObjType)
                return ((int)s1.ObjType).CompareTo((int)s2.ObjType);
            
            return string.Compare(s1.Name, s2.Name, true);
        }
    }
}
