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
    public partial class ViewHost : UserControl
    {
        public MainForm Main;
        public GdiPanel GdiView;
        public GLPanel GLView;
        public ViewModel Model;

        public LinkedList<NodeModel> HistoryList = new LinkedList<NodeModel>();
        public LinkedListNode<NodeModel> CurrentHistory;

        int HoverHash;
        public List<NodeModel> GuiHovered = new List<NodeModel>();
        public NodeModel[] NodesHovered = new NodeModel[] { };

        bool CtrlDown;


        public ViewHost()
        {
            InitializeComponent();
        }

        internal void Init(MainForm main)
        {
            Main = main;
            Model = main.Model;

            GdiView = new GdiPanel(this, new BrightColorProfile()) { Dock = DockStyle.Fill };

            Controls.Add(GdiView);

            SetRoot(Model.CurrentRoot); // init first node in history
        }

        public void RefreshView(bool redrawOnly = false, bool resetZoom = true)
        {
            if (Model.ViewLayout == LayoutType.ThreeD)
            {
                GdiView.Visible = false;

                if (GLView == null)
                {
                    GLView = new GLPanel(Main) { Dock = DockStyle.Fill };
                    Controls.Add(GLView);
                }

                GLView.Visible = true;
                Model.DoRevalue = !redrawOnly;
                GLView.Redraw();
            }
            else
            {
                if (GLView != null)
                    GLView.Visible = false;

                GdiView.Visible = true;

                if (resetZoom)
                    GdiView.ResetZoom();

                // check if view exists
                if (redrawOnly)
                    GdiView.Redraw();
                else
                    GdiView.RecalcValues();

                //PauseLink.Visible = (Model.ViewLayout == LayoutType.Timeline);
            }
        }

        public void SetRoot(NodeModel node, bool logHistory = true)
        {
            // setting internal root will auto show properly sized external root area if showing it is enabled
            GdiView.ResetZoom();
            Model.CurrentRoot = (node == Model.TopRoot) ? Model.InternalRoot : node;

            if (logHistory)
            {
                // re-write forward log with new node
                while (CurrentHistory != HistoryList.Last)
                    HistoryList.RemoveLast();

                // dont set node if last node is already this
                var last = HistoryList.LastOrDefault();
                if (Model.CurrentRoot != last)
                {
                    HistoryList.AddLast(Model.CurrentRoot);
                    CurrentHistory = HistoryList.Last;
                }
            }

            Main.UpdateBreadCrumbs();

            Model.DoRevalue = true;
            Refresh();
        }

        public void NavBack()
        {
            if (CurrentHistory != null && CurrentHistory.Previous != null)
            {
                // update current history before calling so breadcrumbs are updated properly
                var prev = CurrentHistory.Previous;

                CurrentHistory = CurrentHistory.Previous;

                SetRoot(prev.Value, false);
            }
        }

        public void NavForward()
        {
            if (CurrentHistory != null && CurrentHistory.Next != null)
            {
                // update current history before calling so breadcrumbs are updated properly
                var next = CurrentHistory.Next;

                CurrentHistory = CurrentHistory.Next;

                SetRoot(next.Value, false);
            }
        }

        public void View_MouseLeave()
        {
            ClearHovered();

            GdiView.Redraw();
        }

        private void ClearHovered()
        {
            GuiHovered.ForEach(n => n.Hovered = false);
            GuiHovered.Clear();
        }

        public void RefreshHovered(Point loc)
        {
            ClearHovered();

            if (Model.ViewLayout == LayoutType.TreeMap)
            {
                if (Model.ShowingOutside)
                    TestHovered(Model.InternalRoot, loc);
                if (Model.ShowingExternal)
                    TestHovered(Model.ExternalRoot, loc);

                TestHovered(Model.CurrentRoot, loc);
            }
            else if (Model.ViewLayout == LayoutType.CallGraph)
            {
                var hovered = Model.PositionMap.Values.FirstOrDefault(n => n.AreaF.Contains(loc.X, loc.Y) || n.LabelRect.Contains(loc.X, loc.Y));
                if (hovered != null)
                    AddNodeToHovered(hovered);
            }
            else if (Model.ViewLayout == LayoutType.Timeline)
            {
                var hovered = Model.ThreadlineNodes.FirstOrDefault(n => n.Area.Contains(loc.X, loc.Y) || n.LabelArea.Contains(loc.X, loc.Y));
                if (hovered != null)
                    AddNodeToHovered(hovered.Node);
            }

            int hash = 0;
            GuiHovered.ForEach(n => hash = n.ID ^ hash);

            // only continuously redraw label if hover is different than before
            if (hash != HoverHash)
            {
                HoverHash = hash;

                if (GuiHovered.Count > 0)
                {
                    NodesHovered = GuiHovered.Last().GetParents().ToArray();
                    //MainForm.SelectedLabel.Text = GuiHovered.Last().FullName();
                }
                else
                {
                    NodesHovered = new NodeModel[] { };
                    //MainForm.SelectedLabel.Text = "";
                }

                GdiView.Redraw();
            }
        }

        private void AddNodeToHovered(NodeModel node)
        {
            GuiHovered.Add(node);
            var parent = node.Parent;

            while (parent != null)
            {
                GuiHovered.Add(parent);
                parent = parent.Parent;
            }

            GuiHovered.Reverse();
        }

        private void TestHovered(NodeModel node, Point loc)
        {
            if (!node.Show || !node.AreaF.Contains(loc.X, loc.Y))
                return;

            node.Hovered = true;
            GuiHovered.Add(node);

            foreach (var sub in node.Nodes)
                TestHovered(sub, loc);
        }

        public void View_MouseDoubleClick(Point location)
        {
            var node = GuiHovered.LastOrDefault();
            if (node != null)
                SetRoot(node);
        }

        public void ManualMouseClick(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (!CtrlDown)
                {
                    Model.FocusedNodes.ForEach(n => n.Focused = false);
                    Model.FocusedNodes.Clear();
                }

                var node = GuiHovered.LastOrDefault();

                if (node == null)
                    return;

                else if (node.Focused && CtrlDown)
                {
                    node.Focused = false;

                    Model.FocusedNodes.Remove(node);
                }

                else
                {
                    node.Focused = true;

                    Model.FocusedNodes.Add(node);

                    Main.NavigatePanelTo(node);
                }

                GdiView.Redraw();
            }
            else if (e.Button == MouseButtons.Right)
            {
                ContextMenu menu = new ContextMenu();


                var node = GuiHovered.LastOrDefault();
                if (node != null)
                {
                    menu.MenuItems.Add(node.ObjType.ToString() + " " + node.Name);
                    menu.MenuItems.Add("-");

                    bool selected = Model.FilteredNodes.ContainsKey(node.ID);
                    bool ignored = Model.IgnoredNodes.ContainsKey(node.ID);

                    menu.MenuItems.Add(new MenuItem("Zoom", (s, a) => SetRoot(node)));

                    menu.MenuItems.Add(new MenuItem("Filter", (s, a) => ToggleNode(Model.FilteredNodes, node)) { Checked = selected });

                    menu.MenuItems.Add(new MenuItem("Ignore", (s, a) => ToggleNode(Model.IgnoredNodes, node)) { Checked = ignored });
                }

                if (Model.FilteredNodes.Count > 0 || Model.IgnoredNodes.Count > 0)
                {
                    menu.MenuItems.Add("-");

                    menu.MenuItems.Add(new MenuItem("Clear Filtering", (s, a) =>
                    {
                        Model.FilteredNodes.Clear();
                        Model.IgnoredNodes.Clear();
                        GdiView.Redraw();
                    }));
                }

                menu.Show(this, e.Location);
            }

            // back button zoom out
            else if (e.Button == MouseButtons.XButton1)
                NavBack();

            // forward button zoom in
            else if (e.Button == MouseButtons.XButton2)
                NavForward();
        }

        void ToggleNode(Dictionary<int, NodeModel> map, NodeModel node)
        {
            // make sure a node cant be selected and ignored simultaneously
            if (map != Model.IgnoredNodes && Model.IgnoredNodes.ContainsKey(node.ID))
                Model.IgnoredNodes.Remove(node.ID);

            if (map != Model.FilteredNodes && Model.FilteredNodes.ContainsKey(node.ID))
                Model.FilteredNodes.Remove(node.ID);

            // toggle the setting of the node in the map
            if (map.ContainsKey(node.ID))
                map.Remove(node.ID);
            else
                map[node.ID] = node;

            GdiView.Redraw();
        }

        public void View_KeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
                CtrlDown = true;

            /*if (e.KeyCode == Keys.Right)
                Zoom(true);
            else if (e.KeyCode == Keys.Right)
                Zoom(false);
            else if (e.KeyCode == Keys.Space)
                SetRoot(XRay.RootNode);*/
        }

        public void View_KeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
                CtrlDown = false;
        }


        private void Zoom(bool inward)
        {
            if (inward)
            {
                if (GuiHovered.Count < 2)
                    return;

                if (GuiHovered[1].Nodes.Count > 0)
                    SetRoot(GuiHovered[1]);
            }
            else
            {
                if (Model.CurrentRoot.Parent == null)
                    return;

                SetRoot(Model.CurrentRoot.Parent);
            }

            // put cursor in the same block after the view changes
            var last = NodesHovered.LastOrDefault();

            if (last != null)
                Cursor.Position = PointToScreen(new Point((int)last.CenterF.X, (int)last.CenterF.Y));
        }

    }
}
