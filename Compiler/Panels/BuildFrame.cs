using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace XBuilder.Panels
{
    public enum BuildStep { Files, TrackingOptions, ViewerOptions, Compile, Run }

    public partial class BuildFrame : UserControl
    {
        public BuildModel Model = new BuildModel();

        public BuildFrame()
        {
            InitializeComponent();
        }

        private void BuildFrame_Load(object sender, EventArgs e)
        {
            SetStep(BuildStep.Files);
        }

        public void SetStep(BuildStep step)
        {
            Control panel = null;

            switch (step)
            {
                case BuildStep.Files:
                    panel = new BuildStepFiles(this, Model);
                    break;

                case BuildStep.TrackingOptions:
                    panel = new BuildStepOptions(this, Model);
                    break;

                case BuildStep.ViewerOptions:
                    panel = new BuildStepOptions2(this, Model);
                    break;

                case BuildStep.Compile:
                    panel = new BuildStepCompile(this, Model);
                    break;

                case BuildStep.Run:
                    panel = new BuildStepRun(this, Model);
                    break;
            }

            panel.Dock = DockStyle.Fill;

            Controls.Clear();
            Controls.Add(panel);
        }
    }
}
