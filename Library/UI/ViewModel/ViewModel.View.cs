using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;


namespace XLibrary
{
    public partial class ViewModel
    {
        public IMainUI MainUI;

        public LinkedList<NodeModel> HistoryList = new LinkedList<NodeModel>();
        public LinkedListNode<NodeModel> CurrentHistory;

        int HoverHash;
        public List<NodeModel> GuiHovered = new List<NodeModel>();
        public NodeModel[] NodesHovered = new NodeModel[] { };

        bool CtrlDown;

        Point MouseDownPoint;
        PointF PanStart;
        float ZoomFactor = 1;


        public void SetRoot(NodeModel node, bool logHistory = true)
        {
            if (node == null)
                return;

            // setting internal root will auto show properly sized external root area if showing it is enabled
            ResetZoom();
            CurrentRoot = (node == TopRoot) ? InternalRoot : node;

            if (logHistory)
            {
                // re-write forward log with new node
                while (CurrentHistory != HistoryList.Last)
                    HistoryList.RemoveLast();

                // dont set node if last node is already this
                var last = HistoryList.LastOrDefault();
                if (CurrentRoot != last)
                {
                    HistoryList.AddLast(CurrentRoot);
                    CurrentHistory = HistoryList.Last;
                }
            }

            MainUI.UpdateBreadCrumbs();

            DoRevalue = true;
            Renderer.ViewInvalidate();
        }

        public void ResetZoom()
        {
            PanOffset = Point.Empty;
            ZoomFactor = 1;
            CurrentThreadlineZoom = null;
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

            DoRedraw = true;
            Renderer.ViewInvalidate();
        }

        private void ClearHovered()
        {
            GuiHovered.ForEach(n => n.Hovered = false);
            GuiHovered.Clear();
        }

        public void RefreshHovered(Point loc)
        {
            ClearHovered();

            if (ViewLayout == LayoutType.TreeMap)
            {
                if (ShowingOutside)
                    TestHovered(InternalRoot, loc);
                if (ShowingExternal)
                    TestHovered(ExternalRoot, loc);

                TestHovered(CurrentRoot, loc);
            }
            else if (ViewLayout == LayoutType.CallGraph)
            {
                var hovered = PositionMap.Values.FirstOrDefault(n => n.AreaF.Contains(loc.X, loc.Y) || n.LabelRect.Contains(loc.X, loc.Y));
                if (hovered != null)
                    AddNodeToHovered(hovered);
            }
            else if (ViewLayout == LayoutType.Timeline)
            {
                var hovered = ThreadlineNodes.FirstOrDefault(n => n.Area.Contains(loc.X, loc.Y) || n.LabelArea.Contains(loc.X, loc.Y));
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

                DoRedraw = true;
                Renderer.ViewInvalidate();
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
            {
                if (ViewLayout == LayoutType.TreeMap && node.ObjType != XObjType.Class)
                    node = node.GetParentClass(false);

                SetRoot(node);
            }
        }

        public void ClickNode(NodeModel node)
        {
            if (!CtrlDown)
            {
                FocusedNodes.ForEach(n => n.Focused = false);
                FocusedNodes.Clear();
            }

            if (node == null)
                return;

            else if (node.Focused && CtrlDown)
            {
                node.Focused = false;

                FocusedNodes.Remove(node);
            }

            else
            {
                node.Focused = true;

                FocusedNodes.Add(node);

                MainUI.NavigatePanelTo(node);
            }

            DoRedraw = true;
            Renderer.ViewInvalidate();
        }

        public void ManualMouseClick(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var node = GuiHovered.LastOrDefault();

                ClickNode(node);
            }
            else if (e.Button == MouseButtons.Right)
            {
                /*ContextMenu menu = new ContextMenu();


                var node = GuiHovered.LastOrDefault();
                if (node != null)
                {
                    menu.MenuItems.Add(node.ObjType.ToString() + " " + node.Name);
                    menu.MenuItems.Add("-");

                    bool selected = FilteredNodes.ContainsKey(node.ID);
                    bool ignored = IgnoredNodes.ContainsKey(node.ID);

                    menu.MenuItems.Add(new MenuItem("Zoom", (s, a) => SetRoot(node)));

                    menu.MenuItems.Add(new MenuItem("Filter", (s, a) => ToggleNode(FilteredNodes, node)) { Checked = selected });

                    menu.MenuItems.Add(new MenuItem("Ignore", (s, a) => ToggleNode(IgnoredNodes, node)) { Checked = ignored });
                }

                if (FilteredNodes.Count > 0 || IgnoredNodes.Count > 0)
                {
                    menu.MenuItems.Add("-");

                    menu.MenuItems.Add(new MenuItem("Clear Filtering", (s, a) =>
                    {
                        FilteredNodes.Clear();
                        IgnoredNodes.Clear();

                        DoRedraw = true;
                        Renderer.ViewInvalidate();
                    }));
                }

                menu.Show(this, e.Location);*/
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
            if (map != IgnoredNodes && IgnoredNodes.ContainsKey(node.ID))
                IgnoredNodes.Remove(node.ID);

            if (map != FilteredNodes && FilteredNodes.ContainsKey(node.ID))
                FilteredNodes.Remove(node.ID);

            // toggle the setting of the node in the map
            if (map.ContainsKey(node.ID))
                map.Remove(node.ID);
            else
                map[node.ID] = node;

            DoRedraw = true;
            Renderer.ViewInvalidate();
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


        /*private void Zoom(bool inward)
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
                if (CurrentRoot.Parent == null)
                    return;

                SetRoot(CurrentRoot.Parent);
            }

            // put cursor in the same block after the view changes
            var last = NodesHovered.LastOrDefault();

            if (last != null)
                Cursor.Position = PointToScreen(new Point((int)last.CenterF.X, (int)last.CenterF.Y));
        }*/

        public void View_MouseWheel(MouseEventArgs e)
        {
            if (ViewLayout == LayoutType.Timeline)
            {
                // move screen up/down 50%
                PanOffset.Y += (float)e.Delta / 120.0f * 0.50f;

                if (PanOffset.Y < 0)
                    PanOffset.Y = 0;

                return;
            }

            // get fractional position in model
            var modelPos = new PointF();
            modelPos.X = (e.Location.X - ScreenOffset.X) / ScreenSize.Width;
            modelPos.Y = (e.Location.Y - ScreenOffset.Y) / ScreenSize.Height;

            // get fractional position in window
            var winPos = new SizeF();
            winPos.Width = (float)e.Location.X / (float)Renderer.ViewWidth;// -0.5f;
            winPos.Height = (float)e.Location.Y / (float)Renderer.ViewHeight;// -0.5f;

            // change view point size and redraw
            float zoomAmount = (float)Math.Pow(1.3, e.Delta / 120.0);

            ZoomFactor *= zoomAmount;


            if (ZoomFactor < 1)
            {
                ZoomFactor = 1;
                PanOffset = new PointF();
            }
            else
            {
                // we want to keep the zoom over the cursor, the modify the window offset by the zoom levl
                winPos.Width /= ZoomFactor;
                winPos.Height /= ZoomFactor;

                // subtract the window pos from our target pos in the model to find the amount that should be panned
                PanOffset.X = winPos.Width - modelPos.X;
                PanOffset.Y = winPos.Height - modelPos.Y;
            }

            DoResize = true;
            Renderer.ViewInvalidate();
        }

        public void View_MouseDown(MouseEventArgs e)
        {
            MouseDownPoint = e.Location;
            PanStart = PanOffset;
        }

        public void View_MouseUp(MouseEventArgs e)
        {
            if (MouseDownPoint == e.Location)
                ManualMouseClick(e);

            MouseDownPoint = Point.Empty;
            PanStart = Point.Empty;
        }

        public void View_MouseMove(MouseEventArgs e)
        {
            // if moving with left button on, pan
            if (MouseDownPoint != Point.Empty && e.Button == MouseButtons.Left)
            {
                PanOffset.X = PanStart.X + (e.Location.X - MouseDownPoint.X) / ScreenSize.Width;
                PanOffset.Y = PanStart.Y + (e.Location.Y - MouseDownPoint.Y) / ScreenSize.Height;

                if (ViewLayout == LayoutType.Timeline && PanOffset.Y < 0)
                    PanOffset.Y = 0;

                // to make faster resizing should keep things in scale value
                // so we just have to call redraw() which is faster and would apply perspective correction at draw time

                DoResize = true;
                Renderer.ViewInvalidate();
            }
            else
                RefreshHovered(e.Location);
        }


        private void GraphDebuggingLabel(Graphics buffer, PointF pos)
        {
            /* useful for debugging call graph
            var debugNode = NodesHovered.Last();
            string debugString = "";

            float sum = 0;
            float count = 0;


            debugString += string.Format("My Loc {0}\r\n", debugNode.ScaledLocation.Y);

            if (debugNode.CallsOut != null)
                foreach (var call in debugNode.CallsOut)
                    if (PositionMap.ContainsKey(call.Destination))
                    {
                        if (call.Intermediates != null && call.Intermediates.Count > 0)
                        {
                            sum += call.Intermediates[0].ScaledLocation.Y;
                            debugString += string.Format("Out intermediate {0}\r\n", call.Intermediates[0].ScaledLocation.Y);
                        }
                        else
                        {
                            sum += PositionMap[call.Destination].ScaledLocation.Y;
                            debugString += string.Format("Out node {0}\r\n", PositionMap[call.Destination].ScaledLocation.Y);
                        }

                        count++;
                    }

            if (debugNode.CalledIn != null)
                foreach (var call in debugNode.CalledIn)
                    if (PositionMap.ContainsKey(call.Source))
                    {
                        if (call.Intermediates != null && call.Intermediates.Count > 0)
                        {
                            sum += call.Intermediates.Last().ScaledLocation.Y;
                            debugString += string.Format("In intermediate {0}\r\n", call.Intermediates.Last().ScaledLocation.Y);
                        }
                        else
                        {
                            sum += PositionMap[call.Source].ScaledLocation.Y;
                            debugString += string.Format("In node {0}\r\n", PositionMap[call.Source].ScaledLocation.Y);
                        }
                        count++;
                    }

            // should only be attached to intermediate nodes
            if (debugNode.Adjacents != null)
            {
                Debug.Assert(debugNode.ID == 0); // adjacents should only be on temp nodes

                foreach (var adj in debugNode.Adjacents)
                {
                    debugString += string.Format("Adj {0}\r\n", adj.ScaledLocation.Y);

                    sum += adj.ScaledLocation.Y;
                    count++;
                }
            }

            if (count != 0)
            {
                float result = sum / count;
                debugString += string.Format("My Avg {0}\r\n", result);
            }

            buffer.DrawString(debugString, TextFont, ObjBrushes[(int)debugNode.ObjType], pos.X, pos.Y);*/
        }
    }
}
