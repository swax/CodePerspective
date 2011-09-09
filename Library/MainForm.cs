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
        TreePanelGdiPlus TreeView;


        public MainForm()
        {
            InitializeComponent();

            TreeView = new TreePanelGdiPlus(this) { Dock = DockStyle.Fill };
            ViewHostPanel.Controls.Add(TreeView);

            ResetTimer.Interval = 1000 / XRay.HitFrames;
            ResetTimer.Enabled = true;

            Text = "c0re XRay"; 
            UpdateStatus();

            ShowAllCallsMenuItem.Checked = XRay.ShowAllCalls;
            ShowRTCallsMenuItem.Checked = TreeView.ShowCalls;
            ViewOutsideMenuItem.Checked = TreeView.ShowOutside;
            ViewExternalMenuItem.Checked = TreeView.ShowExternal;
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

        public void UpdateStatus()
        {
            SelectedLabel.Text = "Viewing " + TreeView.CurrentRoot.FullName();
        }

        private void HitsMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            ShowAllHitsMenuItem.Checked = TreeView.ShowLayout == ShowNodes.All;
            ShowHitMenuItem.Checked = TreeView.ShowLayout == ShowNodes.Hit;
            ShowUnhitMenuItem.Checked = TreeView.ShowLayout == ShowNodes.Unhit;
            ShowInstancesMenuItem.Checked = TreeView.ShowLayout == ShowNodes.Instances;
        }

        private void ShowAllHitsMenuItem_Click(object sender, EventArgs e)
        {
            TreeView.ShowLayout = ShowNodes.All;
            TreeView.RecalcValues();
        }

        private void ShowHitMenuItem_Click(object sender, EventArgs e)
        {
            TreeView.ShowLayout = ShowNodes.Hit;
            TreeView.RecalcValues();
        }

        private void ShowUnhitMenuItem_Click(object sender, EventArgs e)
        {
            TreeView.ShowLayout = ShowNodes.Unhit;
            TreeView.RecalcValues();
        }

        private void ResetMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < XRay.CoveredFunctions.Count; i++)
                if (XRay.Nodes[i].StillInside == 0)
                    XRay.CoveredFunctions[i] = false;

            TreeView.RecalcValues();
        }

        private void ShowInstancesMenuItem_Click(object sender, EventArgs e)
        {
            TreeView.ShowLayout = ShowNodes.Instances;
            TreeView.RecalcValues();
        }

        private void DebugMenuItem_Click(object sender, EventArgs e)
        {
            new DebugForm().Show();
        }

        private void ShowAllCallsMenuItem_Click(object sender, EventArgs e)
        {
            XRay.ShowAllCalls = ShowAllCallsMenuItem.Checked;
            TreeView.Redraw();
        }

        private void ShowRTMenuItem_Click(object sender, EventArgs e)
        {
            TreeView.ShowCalls = ShowRTCallsMenuItem.Checked;
            TreeView.Redraw();
        }

        private void ViewOutsideMenuItem_Click(object sender, EventArgs e)
        {
            TreeView.ShowOutside = ViewOutsideMenuItem.Checked;
            TreeView.RecalcValues();
        }

        private void ViewExternalMenuItem_Click(object sender, EventArgs e)
        {
            TreeView.ShowExternal = ViewExternalMenuItem.Checked;
            TreeView.RecalcValues();
        }

        private void ConstantMenuItem_Click(object sender, EventArgs e)
        {
            TreeView.SizeLayout = SizeLayouts.Constant;
            TreeView.RecalcValues();
        }

        private void MethodSizeMenuItem_Click(object sender, EventArgs e)
        {
            TreeView.SizeLayout = SizeLayouts.MethodSize;
            TreeView.RecalcValues();
        }

        private void TimeInMethodMenuItem_Click(object sender, EventArgs e)
        {
            TreeView.SizeLayout = SizeLayouts.TimeInMethod;
            TreeView.RecalcValues();
        }

        private void SizeHitsMenuItem_Click(object sender, EventArgs e)
        {
            TreeView.SizeLayout = SizeLayouts.Hits;
            TreeView.RecalcValues();
        }

        private void TimePerHitMenuItem_Click(object sender, EventArgs e)
        {
            TreeView.SizeLayout = SizeLayouts.TimePerHit;
            TreeView.RecalcValues();
        }

        private void CallGraphMenuItem_Click(object sender, EventArgs e)
        {
            TreeView.ViewLayout = LayoutType.CallGraph;
            TreeView.RecalcValues ();
        }

        private void TreeMapMenuItem_Click(object sender, EventArgs e)
        {
            TreeView.ViewLayout = LayoutType.TreeMap;
            TreeView.RecalcValues();
        }

        private void LayoutMenu_DropDownOpening(object sender, EventArgs e)
        {
            TreeMapMenuItem.Checked = TreeView.ViewLayout == LayoutType.TreeMap;
            CallGraphMenuItem.Checked = TreeView.ViewLayout == LayoutType.CallGraph;
        }

        private void SizesMenu_DropDownOpening(object sender, EventArgs e)
        {
            ConstantMenuItem.Checked = TreeView.SizeLayout == SizeLayouts.Constant;
            MethodSizeMenuItem.Checked = TreeView.SizeLayout == SizeLayouts.MethodSize;
            TimeInMethodMenuItem.Checked = TreeView.SizeLayout == SizeLayouts.TimeInMethod;
            SizeHitsMenuItem.Checked = TreeView.SizeLayout == SizeLayouts.Hits;
            TimePerHitMenuItem.Checked = TreeView.SizeLayout == SizeLayouts.TimePerHit;
        }

        private void ZoomMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            ZoomMenuItem.DropDownItems.Clear();

            var root = TreeView.CurrentRoot;

            foreach (XNode parent in root.GetParents())
            {
                var copy = parent as XNodeIn;
                ZoomMenuItem.DropDownItems.Add(new ToolStripMenuItem(copy.Name, null, (s, a) => TreeView.SetRoot(copy)));
            }
        }
    }
}
