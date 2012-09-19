using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace XLibrary.UI.Panels
{
    public partial class SettingsPanel : UserControl
    {
        MainForm Main;
        ViewModel Model;


        public SettingsPanel()
        {
            InitializeComponent();
        }

        public void Init(MainForm main)
        {
            Main = main;
            Model = main.Model;

            // tracking
            TrackFunctionsCheckBox.Checked = XRay.TrackFunctionHits;
            TrackCalls.Checked = XRay.FlowTracking;
            TrackInstances.Checked = XRay.InstanceTracking;

            TrackProfiling.Visible = XRay.RemoteViewer;
            TrackThreadlines.Visible = XRay.RemoteViewer;

            if (XRay.Remote != null)
            {
                TrackProfiling.Checked = XRay.Remote.TrackRemoteProfiling;
                TrackThreadlines.Checked = XRay.Remote.TrackRemoteThreadlines;
            }
        }

        private void SettingsPanel_Load(object sender, EventArgs e)
        {
            RefreshContent();
        }

        public void RefreshContent()
        {
            // compile settings
            CompileSettingsList.BeginUpdate();
            CompileSettingsList.Items.Clear();

            foreach (var setting in XRay.Settings)
                CompileSettingsList.Items.Add(string.Format("{0}: {1}", setting.Key, setting.Value));

            CompileSettingsList.EndUpdate();

            // fps
            TargetFpsLink.Text = string.Format("Target rate {0} fps", XRay.TargetFps);

            // connection
            if (XRay.Remote == null)
            {
                ModeLabel.Visible = false;
                ConnectionList.Visible = false;
            }
            else
            {
                ModeLabel.Text = XRay.RemoteViewer ? "Client Mode" : "Server Mode";
            }
        }

        private void SecondTimer_Tick(object sender, EventArgs e)
        {
            if (XRay.Remote == null)
                return;

            ConnectionList.BeginUpdate();
            ConnectionList.Items.Clear();

            foreach (var client in XRay.Remote.SyncClients)
            {
                var conn = client.Connection;

                ConnectionList.Items.Add(string.Format("client: {0}, syncs: {1}({2}), in: {3}, out: {4} bps, sync size: {5}",
                    conn.RemoteIP, conn.SyncsPerSecond, client.TargetFps, conn.Bandwidth.InAvg(), conn.Bandwidth.OutAvg(), conn.LastSyncSize));
            }

            if (XRay.Remote.ServerConnection != null)
            {
                var server = XRay.Remote.ServerConnection;
                ConnectionList.Items.Add(string.Format("server: {0}, syncs: {1}, in: {2} bps, out: {3} bps, sync size: {4}",
                   server.RemoteIP, server.SyncsPerSecond, server.Bandwidth.InAvg(), server.Bandwidth.OutAvg(), server.LastSyncSize));
            }

            ConnectionList.EndUpdate();
        }

        private void TrackFunctionHits_CheckedChanged(object sender, EventArgs e)
        {
            // different variable to filter method enter
            XRay.TrackFunctionHits = TrackFunctionsCheckBox.Checked;
        }

        private void TrackCalls_CheckedChanged(object sender, EventArgs e)
        {
            XRay.FlowTracking = TrackCalls.Checked;
        }

        private void TrackInstances_CheckedChanged(object sender, EventArgs e)
        {
            XRay.InstanceTracking = TrackInstances.Checked;
        }

        private void TrackProfiling_CheckedChanged(object sender, EventArgs e)
        {
            if (XRay.Remote == null)
                return;

            XRay.Remote.TrackRemoteProfiling = TrackProfiling.Checked;
            XRay.Remote.SendClientSettings();
        }

        private void TrackThreadlines_CheckedChanged(object sender, EventArgs e)
        {
            if (XRay.Remote == null)
                return;

            XRay.Remote.TrackRemoteThreadlines = TrackThreadlines.Checked;
            XRay.Remote.SendClientSettings();
        }

        private void TargetFpsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var input = new InputDialog("Target Rate", "Enter target FPS rate (higher is more cpu)");

            var result = input.ShowDialog(this);

            if (result != DialogResult.OK)
                return;

            int rate;
            if(!int.TryParse(input.InputTextBox.Text, out rate))
                return;

            if (rate <= 0 || 100 < rate)
                return;

            XRay.TargetFps = rate;
            Main.RedrawTimer.Interval = 1000 / rate;

            if (XRay.Remote != null)
                XRay.Remote.SendClientSettings();

            RefreshContent();
        }

    }
}
