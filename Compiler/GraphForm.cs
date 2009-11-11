using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace XBuilder
{
    public partial class GraphForm : Form
    {
        public GraphForm()
        {
            InitializeComponent();

            ChargeBox.Text = graphPanel1.NodeCharge.ToString();
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            graphPanel1.Reset();
        }

        private void SetChargeLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                graphPanel1.NodeCharge = float.Parse(ChargeBox.Text);
            }
            catch
            {
            }
        }
    }
}
