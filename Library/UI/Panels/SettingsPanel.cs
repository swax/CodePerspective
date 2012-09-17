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
            TrackingMethodCalls.Enabled = XRay.FlowTracking;
            TrackingClassCalls.Enabled = XRay.FlowTracking;
            TrackingInstances.Enabled = XRay.InstanceTracking;

            TrackingMethodCalls.Checked = XRay.FlowTracking;
            TrackingClassCalls.Checked = XRay.ClassTracking;
            TrackingInstances.Checked = XRay.InstanceTracking;
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
                ConnectionList.Items.Add(string.Format("client: {0}, syncs: {1}({2}), in: {3} out: {4} bps", 
                    client.Connection.RemoteIP, client.Connection.SyncsPerSecond, client.TargetFps, client.Connection.Bandwidth.InAvg(), client.Connection.Bandwidth.OutAvg()));

            if (XRay.Remote.ServerConnection != null)
            {
                var server = XRay.Remote.ServerConnection;
                ConnectionList.Items.Add(string.Format("server: {0}, syncs: {1}, in: {2} bps, out: {3} bps",
                   server.RemoteIP, server.SyncsPerSecond, server.Bandwidth.InAvg(), server.Bandwidth.OutAvg()));
            }

            ConnectionList.EndUpdate();
        }

        private void TrackingMethodCalls_CheckedChanged(object sender, EventArgs e)
        {
            XRay.FlowTracking = TrackingMethodCalls.Checked;
        }

        private void TrackingClassCalls_CheckedChanged(object sender, EventArgs e)
        {
            XRay.ClassTracking = TrackingClassCalls.Checked;
        }

        private void TrackingInstances_CheckedChanged(object sender, EventArgs e)
        {
            XRay.InstanceTracking = TrackingInstances.Checked;
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

            if (XRay.Remote != null && XRay.Remote.ServerConnection != null)
                XRay.Remote.SendClientSettings();

            RefreshContent();
        }
    }
}
