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
    public partial class TreeForm : Form
    {
        ITreePanel CurrentPanel;


        public TreeForm()
        {
            InitializeComponent();

            ResetTimer.Interval = 1000 / XRay.HitFrames;
            ResetTimer.Enabled = true;

            AddPanel(new TreePanelGdiPlus(this, XRay.RootNode));

            UpdateText();
        }

        private void ResetTimer_Tick(object sender, EventArgs e)
        {
            if (XRay.HitFunctions == null)
                return;

            if (XRay.HitIndex + 1 == XRay.HitFrames)
                XRay.HitIndex = 0;
            else
                XRay.HitIndex++;

            for (int i = 0; i < XRay.HitFunctions.Length; i++)
            {
                if (XRay.HitFunctions[i] > 0)
                    XRay.HitFunctions[i]--;

                if (XRay.Conflicts[i] > 0)
                    XRay.Conflicts[i]--;
            }

            CurrentPanel.Redraw();
        }

        public void UpdateText()
        {
            string text = "XRay: " + Path.GetFileName(Application.ExecutablePath).Split('.')[0];

            string name = CurrentPanel.GetRoot().GetName();

            if(name != "")
                text += " - " + name;

            Text = text;
        }

        private void ShowOnlyHitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            XRay.ShowOnlyHit = showOnlyHitToolStripMenuItem.Checked;
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
    }
}
