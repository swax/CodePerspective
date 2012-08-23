using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using XLibrary;
using System.Net;
using XLibrary.Remote;
using System.Security.Cryptography;
using System.IO;


namespace XBuilder.Panels
{
    public partial class RemotePanel : UserControl
    {
        XConnection Connection;


        public RemotePanel()
        {
            InitializeComponent();
            AddressTextBox.Text = "127.0.0.1:4566";
            KeyTextBox.Text = "43a6e878b76fc485698f2d3b2cfbd93b9f90907e1c81e8821dceac82d45252f3";

            XRay.Remote.RemoteCachePath = Path.Combine(Application.StartupPath, "Remote");
            Directory.CreateDirectory(XRay.Remote.RemoteCachePath);
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            try
            {
                XRay.InitCoreThread();

                // set key
                XRay.Remote.Encryption.Key = Utilities.HextoBytes(KeyTextBox.Text);
                
                // set address
                var address = AddressTextBox.Text;

                var parts = address.Split(':');

                var ip = parts[0];
                var port = parts[1];

                // connect
                Connection = XRay.Remote.MakeOutbound(IPAddress.Parse(ip), ushort.Parse(port));

                ConnectionTimer_Tick(this, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void ConnectionTimer_Tick(object sender, EventArgs e)
        {
            if (Connection == null)
                StatusLabel.Text = "";
            else
                StatusLabel.Text = Connection.ToString() + " - " + Connection.State.ToString() + " - " + XRay.Remote.RemoteStatus;

            ConnectButton.Enabled = (Connection == null);
            OpenButton.Enabled = (Connection != null);
            DisconnectButton.Enabled = (Connection != null);

        }

        private void DisconnectButton_Click(object sender, EventArgs e)
        {
            if (Connection == null)
                return;

            Connection.CleanClose("Forced Disconnect");
            Connection = null;

            ConnectionTimer_Tick(this, null);
        }

        private void OpenButton_Click(object sender, EventArgs e)
        {

        }
    }
}
