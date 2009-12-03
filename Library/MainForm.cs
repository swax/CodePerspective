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
                foreach (FunctionCall call in XRay.CallMap)
                {
                    if (call != null && call.Hit > 0)
                    {
                        call.Hit--;

                        call.DashOffset -= FunctionCall.DashSize;
                        if (call.DashOffset < 0)
                            call.DashOffset = FunctionCall.DashSpace;
                    }

                }
            }

            TreeView.Redraw();
        }

        public void UpdateStatus()
        {
            SelectedLabel.Text = "Viewing " + TreeView.CurrentRoot.FullName();
        }

        private void HitsMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            ShowAllHitsMenuItem.Checked = TreeView.HitLayout == HitLayouts.All;
            ShowHitMenuItem.Checked = TreeView.HitLayout == HitLayouts.Hit;
            ShowUnhitMenuItem.Checked = TreeView.HitLayout == HitLayouts.Unhit;
        }

        private void ShowAllHitsMenuItem_Click(object sender, EventArgs e)
        {
            TreeView.HitLayout = HitLayouts.All;
            TreeView.RecalcVales();
        }

        private void ShowHitMenuItem_Click(object sender, EventArgs e)
        {
            TreeView.HitLayout = HitLayouts.Hit;
            TreeView.RecalcVales();
        }

        private void ShowUnhitMenuItem_Click(object sender, EventArgs e)
        {
            TreeView.HitLayout = HitLayouts.Unhit;
            TreeView.RecalcVales();
        }

        private void ResetMenuItem_Click(object sender, EventArgs e)
        {
            XRay.CoveredFunctions.SetAll(false);
            TreeView.RecalcVales();
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
            TreeView.RecalcSizes();
        }

        private void ViewExternalMenuItem_Click(object sender, EventArgs e)
        {
            TreeView.ShowExternal = ViewExternalMenuItem.Checked;
            TreeView.RecalcSizes();
        }

        private void ConstantMenuItem_Click(object sender, EventArgs e)
        {
            TreeView.SizeLayout = SizeLayouts.Constant;
            TreeView.RecalcVales();
        }

        private void MethodSizeMenuItem_Click(object sender, EventArgs e)
        {
            TreeView.SizeLayout = SizeLayouts.MethodSize;
            TreeView.RecalcVales();
        }

        private void TimeInMethodMenuItem_Click(object sender, EventArgs e)
        {
            TreeView.SizeLayout = SizeLayouts.TimeInMethod;
            TreeView.RecalcVales();
        }

        private void SizeHitsMenuItem_Click(object sender, EventArgs e)
        {
            TreeView.SizeLayout = SizeLayouts.Hits;
            TreeView.RecalcVales();
        }

        private void TimePerHitMenuItem_Click(object sender, EventArgs e)
        {
            TreeView.SizeLayout = SizeLayouts.TimePerHit;
            TreeView.RecalcVales();
        }

        private void CallGraphMenuItem_Click(object sender, EventArgs e)
        {
            TreeView.ViewLayout = LayoutType.CallGraph;
            TreeView.RecalcSizes();
        }

        private void TreeMapMenuItem_Click(object sender, EventArgs e)
        {
            TreeView.ViewLayout = LayoutType.TreeMap;
            TreeView.RecalcSizes();
        }

        private void LayoutMenu_DropDownOpening(object sender, EventArgs e)
        {
            ConstantMenuItem.Checked = TreeView.SizeLayout == SizeLayouts.Constant;
            MethodSizeMenuItem.Checked = TreeView.SizeLayout == SizeLayouts.MethodSize;
            TimeInMethodMenuItem.Checked = TreeView.SizeLayout == SizeLayouts.TimeInMethod;
            SizeHitsMenuItem.Checked = TreeView.SizeLayout == SizeLayouts.Hits;
            TimePerHitMenuItem.Checked = TreeView.SizeLayout == SizeLayouts.TimePerHit;

            TreeMapMenuItem.Checked = TreeView.ViewLayout == LayoutType.TreeMap;
            CallGraphMenuItem.Checked = TreeView.ViewLayout == LayoutType.CallGraph;
        }
    }
}
