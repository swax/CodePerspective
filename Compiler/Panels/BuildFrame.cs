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
    public enum BuildStep { Files, TrackingOptions, BuildOptions, ViewerOptions, Compile, Run }

    public partial class BuildFrame : UserControl
    {
        public BuildModel Model = new BuildModel();

        public Control CurrentPanel;


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
            switch (step)
            {
                case BuildStep.Files:
                    CurrentPanel = new BuildStepFiles(this, Model);
                    break;

                case BuildStep.TrackingOptions:
                    CurrentPanel = new BuildStepTrackingOptions(this, Model);
                    break;

                case BuildStep.BuildOptions:
                    CurrentPanel = new BuildStepBuildOptions(this, Model);
                    break;

                case BuildStep.ViewerOptions:
                    CurrentPanel = new BuildStepViewerOptions(this, Model);
                    break;

                case BuildStep.Compile:
                    CurrentPanel = new BuildStepCompile(this, Model);
                    break;

                case BuildStep.Run:
                    CurrentPanel = new BuildStepRun(this, Model);
                    break;
            }

            CurrentPanel.Dock = DockStyle.Fill;

            Controls.Clear();
            Controls.Add(CurrentPanel);
        }
    }
}
