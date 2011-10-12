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

            DisplayTab.Init(TreeView);
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

        /*private void ZoomMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            ZoomMenuItem.DropDownItems.Clear();

            var root = TreeView.CurrentRoot;

            foreach (XNode parent in root.GetParents())
            {
                var copy = parent as XNodeIn;
                ZoomMenuItem.DropDownItems.Add(new ToolStripMenuItem(copy.Name, null, (s, a) => TreeView.SetRoot(copy)));
            }
        }*/
    }
}
