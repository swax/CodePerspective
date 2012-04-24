using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels;
using System.IO;
using XLibrary;

namespace XBuilder
{
    public partial class MonitorPanel : UserControl
    {
        IpcClientChannel XChannel;

        public MonitorPanel()
        {
            InitializeComponent();

            SelectedGroupBox.Visible = false;

            try
            {
                XChannel = new IpcClientChannel();
                ChannelServices.RegisterChannel(XChannel, false);
            }
            catch 
            {
                LastErrorLabel.Text = "Unabled to open IPC channel";
            }
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // mark current list items to remove
                foreach (var item in ProcessListView.Items.Cast<ProcessItem>())
                    item.Remove = true;

                string pipePath = @"\\.\pipe\";
                String[] pipes = Directory.GetFiles(pipePath)
                                    .Where(p => p.StartsWith(pipePath + "xray_"))
                                    .ToArray();

                foreach (var pipe in pipes)
                {
                    var host = pipe.Replace(pipePath, "");

                    string url = "ipc://" + host + "/query";

                    var item = ProcessListView.Items.Cast<ProcessItem>().FirstOrDefault(p => p.Url == url);
                    if (item == null)
                    {
                        item = new ProcessItem(url);
                        ProcessListView.Items.Add(item);
                    }

                    item.Update();

                    item.Remove = false;
                }

                var removeItems = ProcessListView.Items.Cast<ProcessItem>().Where(p => p.Remove).ToArray();
                foreach (var item in removeItems)
                    ProcessListView.Items.Remove(item);

                LastErrorLabel.Text = "";
           }
            catch (Exception ex)
            {
                LastErrorLabel.Text = "Monitor Error: " + ex.Message;
            }
        }

        private void ShowViewerLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var item = ProcessListView.SelectedItems.Cast<ProcessItem>().FirstOrDefault();
            if (item == null)
                return;

            item.GetProcess().GuiVisible = true;
        }

        private void ChangeTrackingLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var item = ProcessListView.SelectedItems.Cast<ProcessItem>().FirstOrDefault();
            if (item == null)
                return;

            var process = item.GetProcess();

            process.Tracking = !process.Tracking;
        }

        private void ProcessListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = ProcessListView.SelectedItems.Cast<ProcessItem>().FirstOrDefault();

            SelectedGroupBox.Visible = (item != null);

            if (item != null)
                SelectedGroupBox.Text = item.Text;
        }
    }

    class ProcessItem : ListViewItem
    {
        public string Url;
        public bool Remove;

        public ProcessItem(string url)
        {
            Url = url;

            SubItems.Add("");
            SubItems.Add("");
        }

        public IpcQuery GetProcess()
        {
            return Activator.GetObject(typeof(IpcQuery), Url) as IpcQuery;
        }

        internal void Update()
        {
            try
            {
                var process = GetProcess();

                SetItem(0, process.GetName());

                SetItem(1, process.GuiVisible ? "on" : "off");

                SetItem(2, process.Tracking ? "on" : "off");
            }
            catch (Exception ex)
            {
                SetItem(0, "Error querying " + Url);
            }
        }

        void SetItem(int i, string text)
        {
            if (SubItems[i].Text != text)
                SubItems[i].Text = text;
        }
    }
}
