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
        ITreePanel CurrentPanel;


        public MainForm()
        {
            InitializeComponent();

            ResetTimer.Interval = 1000 / XRay.HitFrames;
            ResetTimer.Enabled = true;

            AddPanel(new TreePanelGdiPlus(this, XRay.RootNode));

            UpdateText();
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

            // resset
            if (XRay.FlowTracking)
            {
                // time out function calls
                for (int i = 0; i < XRay.CallMap.Length; i++)
                {
                    FunctionCall call = XRay.CallMap.Values[i];
                    if (call != null && call.Hit > 0)
                        call.Hit--;
                }
            }

            CurrentPanel.Redraw();
        }

        public void UpdateText()
        {
            string text = "XRay: " + Path.GetFileName(Application.ExecutablePath).Split('.')[0];

            string name = CurrentPanel.GetRoot().FullName();

            if(name != "")
                text += " - " + name;

            Text = text;
        }

        private void ShowOnlyHitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            XRay.ShowOnlyHit = ShowOnlyHitMenuItem.Checked;
            XRay.CoverChange = true; // force recalc
            CurrentPanel.Redraw();
        }

        private void ResetMenuItem_Click(object sender, EventArgs e)
        {
            XRay.CoveredFunctions.SetAll(false);
            XRay.CoverChange = true; // force recalc
            CurrentPanel.Redraw();
        }

        private void ViewMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            ViewMenuItem.DropDownItems.Clear();

            ToolStripMenuItem win32Item = new ToolStripMenuItem("Win32", null, (s2, e2) =>
            {
                AddPanel(new TreePanelWin32(this, XRay.RootNode));
            });

            ToolStripMenuItem gdiItem = new ToolStripMenuItem("GDI+", null, (s2, e2) =>
            {
                AddPanel(new TreePanelGdiPlus(this, XRay.RootNode));
            });

            ToolStripMenuItem wpfItem = new ToolStripMenuItem("WPF", null, (s2, e2) =>
            {
                AddPanel(new TreePanelWpfContainer(this, XRay.RootNode));
            });


            win32Item.Checked = Controls.OfType<TreePanelWin32>().Count() > 0;
            gdiItem.Checked = Controls.OfType<TreePanelGdiPlus>().Count() > 0;
            wpfItem.Checked = Controls.OfType<TreePanelWpfContainer>().Count() > 0;


            ViewMenuItem.DropDownItems.Add(win32Item);
            ViewMenuItem.DropDownItems.Add(gdiItem);
            ViewMenuItem.DropDownItems.Add(wpfItem);
        }

        void AddPanel(Control panel)
        {
            RemoveCurrentPanel();

            panel.Dock = DockStyle.Fill;
            ViewPanel.Controls.Add(panel);

            CurrentPanel = panel as ITreePanel;
            CurrentPanel.Redraw();
        }

        void RemoveCurrentPanel()
        {
            if (CurrentPanel == null)
                return;

            ViewPanel.Controls.Remove(CurrentPanel as Control);
            (CurrentPanel as Control).Dispose();
            CurrentPanel.Dispose2();
            CurrentPanel = null;
        }

        private void TreeForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            RemoveCurrentPanel();
        }

        private void DebugMenuItem_Click(object sender, EventArgs e)
        {
            new DebugForm().Show();
        }

        private void ShowAllCallsMenuItem_Click(object sender, EventArgs e)
        {
            XRay.ShowAllCalls = ShowAllCallsMenuItem.Checked;
            CurrentPanel.Redraw();
        }
    }
}
