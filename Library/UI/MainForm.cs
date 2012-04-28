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
        public GLPanel GLView;
        public ViewModel Model = new ViewModel();


        public MainForm()
        {
            InitializeComponent();

            TreeView = new TreePanelGdiPlus(this, new BrightColorProfile()) { Dock = DockStyle.Fill };
            ViewHostPanel.Controls.Add(TreeView);

            RedrawTimer.Interval = 1000 / XRay.HitFrames;
            RedrawTimer.Enabled = true;

            RevalueTimer.Interval = 1000;
            RevalueTimer.Enabled = true;

            TimingTab.Init(this);
            DisplayTab.Init(this);
            ConsoleTab.Init(this);

            CodeTab.Visible = false;
            InstanceTab.Visible = false;

            TreeView.SetRoot(Model.CurrentRoot); // init first node in history
        }

        void RedrawTimer_Tick(object sender, EventArgs e)
        {
            RefreshView(true, false);
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
            var node = Model.CurrentRoot.XNode;

            while (node != null)
            {
                crumbs.Insert(0, node);

                node = node.Parent as XNodeIn;
            }

            // add crumbs
            foreach (var crumb in crumbs)
            {
                var uiNode = Model.NodeModels[crumb.ID];
                var crumbName = (crumb.ObjType == XObjType.Root) ? "View" : crumb.Name;

                var button = new ToolStripSplitButton(crumbName);
                button.ButtonClick += (s, e) => TreeView.SetRoot(uiNode);
                button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                button.ForeColor = TreePanelGdiPlus.ObjColors[(int)crumb.ObjType];
                MainToolStrip.Items.Add(button);

                button.DropDownOpening += (s1, e1) =>
                    {
                        foreach (var sub in uiNode.Nodes.OrderBy(n => n, new CompareNodes()))
                        {
                            var subCopy = sub;

                            var item = new ToolStripMenuItem(sub.Name, null, (s2, e2) => TreeView.SetRoot(subCopy));
                            item.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                            item.ForeColor = TreePanelGdiPlus.ObjColors[(int)sub.ObjType];

                            // freezing with more than 100 items..
                            //if (button.DropDownItems.Count > 100)
                            //    break;

                            button.DropDownItems.Add(item);
                        }
                    };
            }

            BackButton.Enabled = (TreeView.CurrentHistory != null && TreeView.CurrentHistory.Previous != null);
            ForwardButton.Enabled = (TreeView.CurrentHistory != null && TreeView.CurrentHistory.Next != null);
        }

        private void RevalueTimer_Tick(object sender, EventArgs e)
        {
            DisplayTab.FpsLabel.Text = string.Format("x/s - revalue: {0}, resize {1}, redraw {2}, frames {3}",
                Model.RevalueCount, Model.ResizeCount, Model.RedrawCount, Model.FpsCount);

            Model.RevalueCount = 0;
            Model.ResizeCount = 0;
            Model.RedrawCount = 0;
            Model.FpsCount = 0;

            if (Model.SizeLayout == SizeLayouts.TimeInMethod ||
                Model.SizeLayout == SizeLayouts.Hits ||
                Model.SizeLayout == SizeLayouts.TimePerHit)
            {
                RefreshView(false, false);
            }
        }

        public void RefreshView(bool redrawOnly=false, bool resetZoom=true)
        {
            if (Model.ViewLayout == LayoutType.ThreeD)
            {
                TreeView.Visible = false;

                if (GLView == null)
                {
                    GLView = new GLPanel(this) { Dock = DockStyle.Fill };
                    ViewHostPanel.Controls.Add(GLView);
                }

                GLView.Visible = true;
                Model.DoRevalue = !redrawOnly;
                GLView.Redraw();
            }
            else
            {
                if(GLView != null)
                    GLView.Visible = false;

                TreeView.Visible = true;

                if (resetZoom)
                    TreeView.ResetZoom();

                // check if view exists
                if (redrawOnly)
                    TreeView.Redraw();
                else
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

        private void OnOffButton_Click(object sender, EventArgs e)
        {
            if (XRay.XRayEnabled)
            {
                XRay.XRayEnabled = false;
                OnOffButton.Text = "off";
                OnOffButton.ForeColor = Color.DarkRed;
            }
            else
            {
                XRay.XRayEnabled = true;
                OnOffButton.Text = "on";
                OnOffButton.ForeColor = Color.Green;
            }
        }

        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            TreeView.SearchString = SearchTextBox.Text.Trim().ToLowerInvariant();
            Model.SearchStrobe = false; // so matches are shown immediately
            RefreshView(true, false);
        }

        private void SearchTimer_Tick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(TreeView.SearchString))
                Model.SearchStrobe = !Model.SearchStrobe;
        }

        internal void NavigateTo(NodeModel node)
        {
            CodeTab.Visible = false;
            InstanceTab.Visible = false;

            if (node.ObjType == XObjType.Method)
            {
                InstanceTab.Visible = false;

                CodeTab.NavigateTo(node);
                CodeTab.Dock = DockStyle.Fill;
                CodeTab.Visible = true;
            }

            else if (node.ObjType == XObjType.Class)
            {
                CodeTab.Visible = false;

                InstanceTab.NavigateTo(node);
                InstanceTab.Dock = DockStyle.Fill;
                InstanceTab.Visible = true;
            }

            TimingTab.NavigateTo(node);
        }
    }

    public class CompareNodes : IComparer<NodeModel>
    {
        public int Compare(NodeModel s1, NodeModel s2)
        {
            if (s1.ObjType != s2.ObjType)
                return ((int)s1.ObjType).CompareTo((int)s2.ObjType);
            
            return string.Compare(s1.Name, s2.Name, true);
        }
    }
}
