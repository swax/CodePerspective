using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenTK;


namespace XLibrary
{
    public partial class MainForm : Form, IMainUI
    {
        public ViewModel Model;

        LinkedListNode<NodeModel> Current;
        LinkedList<NodeModel> History = new LinkedList<NodeModel>();

        public Control SelectedView;
        public Dictionary<Type, Control> Renderers = new Dictionary<Type, Control>();

        bool ThreadMultiSelect;


        public MainForm()
        {
            InitializeComponent();

            Model = new ViewModel(this, new BrightColorProfile());

            InitRenderers();
            SetRenderer(typeof(GdiRenderer));

            Model.SetRoot(Model.CurrentRoot); // init first node in history


            RedrawTimer.Interval = 1000 / XRay.HitFrames;
            RedrawTimer.Enabled = true;

            RevalueTimer.Interval = 1000;
            RevalueTimer.Enabled = true;

            InstanceTab.Init(this);
            DisplayTab.Init(this);
            ConsoleTab.Init(this);
            CodeTab.Init(this);
            NamespaceTab.Init(this);

            CodeTab.Visible = false;
            InstanceTab.Visible = false;
            NamespaceTab.Visible = false;

        }

        void RedrawTimer_Tick(object sender, EventArgs e)
        {
            RefreshView(true, false);
        }

        public void RefreshView(bool redrawOnly = false, bool resetZoom = true)
        {
            if (resetZoom)
                Model.ResetZoom();

            // check if view exists
            if (redrawOnly)
                Model.DoRedraw = true;
            else
                Model.DoRevalue = true;

            Model.Renderer.ViewInvalidate();       
        }

        public void InitRenderers()
        {
            Renderers[typeof(GdiRenderer)] = new GdiRenderer(Model);
            Renderers[typeof(GLRenderer)] = new GLRenderer(Model);
            Renderers[typeof(GibsonRenderer)] = new GibsonRenderer(Model);

            foreach (var renderer in Renderers.Values)
            {
                renderer.Dock = DockStyle.Fill;
                renderer.Visible = false;
                ViewHostPanel.Controls.Add(renderer);
            }
        }

        public void SetRenderer(Type type)
        {
            //PauseLink.Visible = (Model.ViewLayout == LayoutType.Timeline);

            Control view;
            if (!Renderers.TryGetValue(type, out view))
            {
                MessageBox.Show("Renderer not found");
                return;
            }

            if (SelectedView != null)
            {
                SelectedView.Visible = false;
                (SelectedView as IRenderer).Stop();
            }
            SelectedView = view;

            view.Visible = true;
            (view as IRenderer).Start();

            Model.Renderer = view as IRenderer;
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
                button.ButtonClick += (s, e) => Model.SetRoot(uiNode);
                button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                button.ForeColor = Model.XColors.ObjColors[(int)crumb.ObjType];
                MainToolStrip.Items.Add(button);

                button.DropDownOpening += (s1, e1) =>
                    {
                        foreach (var sub in uiNode.Nodes.OrderBy(n => n, new CompareNodes()))
                        {
                            var subCopy = sub;

                            var item = new ToolStripMenuItem(sub.Name, null, (s2, e2) => Model.SetRoot(subCopy));
                            item.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                            item.ForeColor = Model.XColors.ObjColors[(int)sub.ObjType];

                            // freezing with more than 100 items..
                            //if (button.DropDownItems.Count > 100)
                            //    break;

                            button.DropDownItems.Add(item);
                        }
                    };
            }

            BackButton.Enabled = (Model.CurrentHistory != null && Model.CurrentHistory.Previous != null);
            ForwardButton.Enabled = (Model.CurrentHistory != null && Model.CurrentHistory.Next != null);
        }

        private void RevalueTimer_Tick(object sender, EventArgs e)
        {
            DisplayTab.FpsLabel.Text = string.Format("revalue: {0}/s\r\nresize {1}\r\nredraw {2}\r\nframes {3}",
                Model.RevalueCount, Model.ResizeCount, Model.RedrawCount, Model.FpsCount);

            Model.RevalueCount = 0;
            Model.ResizeCount = 0;
            Model.RedrawCount = 0;
            Model.FpsCount = 0;

            if (Model.SizeLayout == SizeLayouts.TimeInMethod ||
                Model.SizeLayout == SizeLayouts.Hits ||
                Model.SizeLayout == SizeLayouts.TimePerHit ||
                Model.ShowThreads != null) // new nodes may have been covered by the filtered threads
            {
                RefreshView(false, false);
            }
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            Model.NavBack();
        }

        private void ForwardButton_Click(object sender, EventArgs e)
        {
            Model.NavForward();
        }

        /*private void OnOffButton_Click(object sender, EventArgs e)
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
        }*/

        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            Model.SearchString = SearchTextBox.Text.Trim().ToLowerInvariant();
            Model.SearchStrobe = false; // so matches are shown immediately
            RefreshView(true, false);
        }

        private void SearchTimer_Tick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Model.SearchString))
                Model.SearchStrobe = !Model.SearchStrobe;
        }

        public void NavigatePanelTo(NodeModel node)
        {
            NavigatePanelTo(node, false);
        }

        public void NavigatePanelTo(NodeModel node, bool supressHistory)
        {
            if (!supressHistory)
            {
                // remove anything after current node, add this node to top
                while (Current != null && Current.Next != null)
                    History.RemoveLast();

                Current = History.AddLast(node);
            }

            if (node.ObjType == XObjType.Method)
            {
                CodeTab.NavigateTo(node);
                ShowDetailsPanel(CodeTab);
            }

            else if (node.ObjType == XObjType.Class ||
                node.ObjType == XObjType.Field)
            {
                InstanceTab.NavigateTo(node);
                ShowDetailsPanel(InstanceTab);
            }

            else
            {
                NamespaceTab.NavigateTo(node);
                ShowDetailsPanel(NamespaceTab);
            }
        }

        public void NavigatePanel(bool back)
        {
            if (Current == null)
                return;

            var nextPos = back ? Current.Previous : Current.Next;

            if (nextPos == null || nextPos.Value == null)
                return;

            Current = nextPos;

            NavigatePanelTo(Current.Value, true);
        }

        // do this so we dont change state if we dont have to and avoid ui flicker
        void ShowDetailsPanel(UserControl showPanel)
        {
            if (showPanel.Visible)
                return;

            showPanel.Dock = DockStyle.Fill;

            CodeTab.Visible = (showPanel == CodeTab);
            InstanceTab.Visible = (showPanel == InstanceTab);
            NamespaceTab.Visible = (showPanel == NamespaceTab);
        }

        private void ChangeSplitterOrientation_Click(object sender, EventArgs e)
        {
            if(splitContainer1.Orientation == Orientation.Vertical)
                splitContainer1.Orientation = Orientation.Horizontal;
            else
                splitContainer1.Orientation = Orientation.Vertical;
        }

        internal void NavigatePanelUp()
        {
            if (Current == null || Current.Value == null)
                return;

            if(Current.Value.Parent != null)
                NavigatePanelTo(Current.Value.Parent);
        }

        private void SearchToolButton_ButtonClick(object sender, EventArgs e)
        {

        }

        private void ClearSearchMenuItem_Click(object sender, EventArgs e)
        {
            SearchTextBox.Text = "";
            RefreshView(true, false);
        }

        private void SubsSearchMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            Model.SearchHighlightSubs = SubsSearchMenuItem.Checked;
            Model.LastSearch = ""; // forces search to re-run
            RefreshView(true, false);
        }

        private void ThreadButton_DropDownOpening(object sender, EventArgs e)
        {
            ThreadButton.DropDownItems.Clear();
            
            var allItem = new ToolStripMenuItem("Show All");
            allItem.CheckOnClick = true;
            allItem.Checked = (Model.ShowThreads == null);
            allItem.CheckedChanged += new EventHandler(ThreadShowAll_CheckedChanged);
            ThreadButton.DropDownItems.Add(allItem);

            var MultiSelect = new ToolStripMenuItem("Multi-Select");
            MultiSelect.CheckOnClick = true;
            MultiSelect.Checked = ThreadMultiSelect;
            MultiSelect.CheckedChanged += new EventHandler(ThreadMultiSelect_CheckedChanged);
            ThreadButton.DropDownItems.Add(MultiSelect);

            ThreadButton.DropDownItems.Add(new ToolStripSeparator());

            foreach (var flow in XRay.FlowMap)
            {
                var menuItem = new ToolStripMenuItem(string.Format("Thread {0}: {1}", flow.ThreadID, flow.Handle.Name));

                menuItem.ForeColor = flow.Handle.IsAlive ? Color.Black : Color.Gray;
                menuItem.Tag = flow;
                menuItem.CheckOnClick = true;
                menuItem.Checked = (Model.ShowThreads != null && Model.ShowThreads.Contains(flow.ThreadID));
                menuItem.CheckedChanged += new EventHandler(ThreadMenuItem_CheckedChanged);

                ThreadButton.DropDownItems.Add(menuItem);
            }
        }

        void ThreadShowAll_CheckedChanged(object sender, EventArgs e)
        {
            Model.ShowThreads = null;
            RefreshView();
        }

        void ThreadMultiSelect_CheckedChanged(object sender, EventArgs e)
        {
            ThreadMultiSelect = (sender as ToolStripMenuItem).Checked;
        }

        void ThreadMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            var flow = item.Tag as ThreadFlow;

            if (flow == null)
                return;

            if (Model.ShowThreads == null)
                Model.ShowThreads = new HashSet<int>();

            if (!ThreadMultiSelect)
                Model.ShowThreads.Clear();

            if (item.Checked)
                Model.ShowThreads.Add(flow.ThreadID);
            else
                Model.ShowThreads.Remove(flow.ThreadID);

            // do a refresh view
            RefreshView();
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
