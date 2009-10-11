using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace XLibrary
{
    public partial class DebugForm : Form
    {
        StringBuilder Output = new StringBuilder(4096);

        public DebugForm()
        {
            InitializeComponent();

            CallLogCheckBox.Checked = XRay.CallLogging;
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            Output.Length = 0;

            // settings
            Output.AppendLine("Settings:");
            Output.AppendLine("  InstanceTracking: " + XRay.InstanceTracking);
            Output.AppendLine("  ThreadTracking: " + XRay.ThreadTracking);
            Output.AppendLine("  FlowTracking: " + XRay.FlowTracking);


            // stack
            Output.AppendLine("");
            Output.AppendLine("StackMap:");

            for(int i = 0; i < XRay.FlowMap.Length; i++)
            {
                ThreadFlow flow = XRay.FlowMap.Values[i];

                Output.AppendFormat("  Thread: {0}\r\n", flow.ThreadID);

                for (int x = 0; x <= flow.Pos; x++)
                    Output.AppendFormat("    {0}: {1}\r\n", x, flow.Stack[x].Method);//, (XRay.NodeMap[flow.Stack[x]].StillInside > 0));
            }

            // function calls
            Output.AppendLine("");
            Output.AppendLine("CallMap:");

            for(int i = 0; i < XRay.CallMap.Length; i++)
            {
                FunctionCall call = XRay.CallMap.Values[i];
                Output.AppendFormat("  {0} -> {1}: {2}\r\n", call.Source, call.Destination, call.Hit);
            }

            // function calls
            Output.AppendLine("");
            Output.AppendLine("Log:");
            Output.Append(XRay.DebugLog);

            DebugOutput.Text = Output.ToString();
        }

        private void ResolveLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            int id;
            if (int.TryParse(ResolveBox.Text, out id) &&
                XRay.Nodes[id] != null)
                    ResolveLabel.Text = XRay.Nodes[id].FullName();
        }

        private void CallLogCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            XRay.CallLogging = CallLogCheckBox.Checked;
        }
    }
}
