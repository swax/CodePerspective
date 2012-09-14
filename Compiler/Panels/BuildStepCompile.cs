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
    public partial class BuildStepCompile : UserControl
    {
        BuildFrame Frame;
        BuildModel Model;

        public BuildStepCompile(BuildFrame frame, BuildModel model)
        {
            InitializeComponent();

            Frame = frame;
            Model = model;
        }

        private void ButtonRecompile_Click(object sender, EventArgs e)
        {
            Model.Recompile(false);
        }

        private void ButtonTestCompile_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Model.Recompile(true);
        }

        private void SecondTimer_Tick(object sender, EventArgs e)
        {
            ButtonRecompile.Enabled = IsBuildNotRunning();
            ButtonTestCompile.Enabled = IsBuildNotRunning();

            StatusLabel.Text = Model.BuildStatus;
            ErrorLabel.Text = Model.BuildError;

            BackButton.Enabled = IsBuildNotRunning();
            NextButton.Enabled = Model.BuildSuccess;
        }

        bool IsBuildNotRunning()
        {
            return (Model.BuildThread == null || !Model.BuildThread.IsAlive);
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            Frame.SetStep(BuildStep.Options);
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            // have timer check build thread and if it was a success

            Frame.SetStep(BuildStep.Run);
        }



    }
}
