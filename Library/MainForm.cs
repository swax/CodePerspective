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
                for (int i = 0; i < XRay.CallMap.Length; i++)
                {
                    FunctionCall call = XRay.CallMap.Values[i];
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
            SelectedLabel.Text = "Viewing " + TreeView.GetRoot().FullName();
        }

        private void HitsMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            ShowAllHitsMenuItem.Checked = XRay.ShowHit == ShowHitMode.All;
            ShowHitMenuItem.Checked = XRay.ShowHit == ShowHitMode.Hit;
            ShowUnhitMenuItem.Checked = XRay.ShowHit == ShowHitMode.Unhit;
        }

        private void ShowAllHitsMenuItem_Click(object sender, EventArgs e)
        {
            XRay.ShowHit = ShowHitMode.All;
            XRay.CoverChange = true; // force recalc
            TreeView.Redraw();
        }

        private void ShowHitMenuItem_Click(object sender, EventArgs e)
        {
            XRay.ShowHit = ShowHitMode.Hit;
            XRay.CoverChange = true; // force recalc
            TreeView.Redraw();
        }

        private void ShowUnhitMenuItem_Click(object sender, EventArgs e)
        {
            XRay.ShowHit = ShowHitMode.Unhit;
            XRay.CoverChange = true; // force recalc
            TreeView.Redraw();
        }

        private void ResetMenuItem_Click(object sender, EventArgs e)
        {
            XRay.CoveredFunctions.SetAll(false);
            XRay.CoverChange = true; // force recalc
            TreeView.Redraw();
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
    }
}
