using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using XLibrary;
using System.Security.Cryptography;

namespace XBuilder.Panels
{
    public partial class BuildStepViewerOptions : UserControl
    {
        BuildFrame Frame;
        BuildModel Model;

        public BuildStepViewerOptions(BuildFrame frame, BuildModel model)
        {
            InitializeComponent();

            Frame = frame;
            Model = model;
        }

        private void BuildStepOptions2_Load(object sender, EventArgs e)
        {
            EnableLocalViewer.Checked = Model.EnableLocalViewer;
            ShowOnStartCheckBox.Checked = Model.ShowViewerOnStart;
            EnableIpcServer.Checked = Model.EnableIpcServer;

            EnableRemoteViewer.Checked = Model.EnableTcpServer;
            ListenPort.Text = Model.TcpListenPort.ToString();
            EncryptionKey.Text = Model.EncryptionKey;

            EnableLocalViewer_CheckedChanged(this, null);
            EnableRemoteViewer_CheckedChanged(this, null);
        }

        private void SaveToModel()
        {
            Model.EnableLocalViewer = EnableLocalViewer.Checked;
            Model.ShowViewerOnStart = ShowOnStartCheckBox.Checked;
            Model.EnableIpcServer = EnableIpcServer.Checked;

            Model.EnableTcpServer = EnableRemoteViewer.Checked;
            Model.TcpListenPort = int.Parse(ListenPort.Text);
            Model.EncryptionKey = EncryptionKey.Text;
        }

        private void GenerateKey_Click(object sender, EventArgs e)
        {
            EncryptionKey.Text = Utilities.BytestoHex(new RijndaelManaged().Key);
        }

        private void GeneratePort_Click(object sender, EventArgs e)
        {
            ListenPort.Text = XRay.RndGen.Next(3000, 10000).ToString();
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            SaveToModel();

            Frame.SetStep(BuildStep.BuildOptions);
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            SaveToModel();

            Frame.SetStep(BuildStep.Compile);
        }

        private void EnableLocalViewer_CheckedChanged(object sender, EventArgs e)
        {
            ShowOnStartCheckBox.Enabled = EnableLocalViewer.Checked;
            EnableIpcServer.Enabled = EnableLocalViewer.Checked;
        }

        private void EnableRemoteViewer_CheckedChanged(object sender, EventArgs e)
        {
            ListenPort.Enabled = EnableRemoteViewer.Checked;
            EncryptionKey.Enabled = EnableRemoteViewer.Checked;
            GenerateKey.Enabled = EnableRemoteViewer.Checked;
            GeneratePort.Enabled = EnableRemoteViewer.Checked;
        }


    }
}
