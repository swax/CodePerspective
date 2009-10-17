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

            Output.AppendLine("Dat Path: " + XRay.DatPath);
            Output.AppendLine();

            // settings
            Output.AppendLine("Settings:");
            Output.AppendLine("  InstanceTracking: " + XRay.InstanceTracking);
            Output.AppendLine("  ThreadTracking: " + XRay.ThreadTracking);
            Output.AppendLine("  FlowTracking: " + XRay.FlowTracking);


            // stack
            Output.AppendLine("");
            Output.AppendLine("StackMap:");

            for (int i = 0; i < XRay.FlowMap.Length; i++)
            {
                ThreadFlow flow = XRay.FlowMap.Values[i];

                if (flow == null)
                    continue;

                Output.AppendFormat("  Thread: {0}\r\n", flow.ThreadID);

                for (int x = 0; x <= flow.Pos; x++)
                {
                    XNodeIn node = XRay.Nodes[flow.Stack[x].Method];
                    if(node != null)
                        Output.AppendFormat("    {0}: Hit: {1}, Inside: {2}\r\n", x, node.ID, node.StillInside);//, (XRay.NodeMap[flow.Stack[x]].StillInside > 0));
                }
            }

            // function calls
            Output.AppendLine("");
            Output.AppendLine("CallMap:");

            for(int i = 0; i < XRay.CallMap.Length; i++)
            {
                FunctionCall call = XRay.CallMap.Values[i];
                if(call != null)
                    Output.AppendFormat("  {0} -> {1}: Hit: {2}, Inside: {3}\r\n", call.Source, call.Destination, call.Hit, call.StillInside );
            }

            // function calls
            Output.AppendLine("");
            Output.AppendLine("Log:");
            foreach(string error in XRay.ErrorMap.Keys)
                Output.AppendLine(error);

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
