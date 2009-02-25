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
        public TreeForm()
        {
            InitializeComponent();

            AppTreePanel.MainForm = this;
            AppTreePanel.UpdateMap(XRay.RootNode);
            UpdateText();

            ResetTimer.Interval = 1000 / XRay.HitFrames;
            ResetTimer.Enabled = true;
        }

        private void ResetTimer_Tick(object sender, EventArgs e)
        {
            if (XRay.HitFunctions == null)
                return;

            if (XRay.HitIndex + 1 == XRay.HitFrames)
                XRay.HitIndex = 0;
            else
                XRay.HitIndex++;

            XRay.HitFunctions[XRay.HitIndex].SetAll(false);

            AppTreePanel.Redraw();
        }

        public void UpdateText()
        {
            string text = "XRay: " + Path.GetFileName(Application.ExecutablePath).Split('.')[0];

            string name = AppTreePanel.Root.GetName();

            if(name != "")
                text += " - " + name;

            Text = text;
        }
    }
}
