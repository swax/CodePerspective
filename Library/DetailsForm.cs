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
    public partial class DetailsForm : Form
    {
        LinkedListNode<XNode> Current;
        LinkedList<XNode> History = new LinkedList<XNode>();


        public DetailsForm(XNodeIn node)
        {
            InitializeComponent();

            NavigateTo(node);
        }

        private void NavigateTo(XNode node)
        {
            // remove anything after current node, add this node to top
            while (Current != null && Current.Next != null)
                History.RemoveLast();

            Current = History.AddLast(node);
            Reload();
        }
        
        private void Reload()
        {
            XNode node = Current.Value;
            string name = node.AppendClassName();

            Text = "Details for (" + node.ObjType.ToString() + ") " + name;

            ParentsLink.Enabled = node.Parent != null;
            ChildrenLink.Enabled = node.Nodes.Count > 0;

            BackLink.Enabled = Current.Previous != null;
            ForwardLink.Enabled = Current.Next != null;

            CallersList.Items.Clear();
            CalledList.Items.Clear();

            if (node.ObjType == XObjType.Method)
            {
                FunctionPanel.Panel2Collapsed = false;

                int id = node.ID;
                int count = 0;

                foreach (FunctionCall call in XRay.CallMap.Values
                    .TakeWhile(v => v != null)
                    .Where(v => v.Destination == id))
                {
                    XNode caller = XRay.Nodes[call.Source];
                    CallersList.Items.Add( new CallItem(call, caller));
                    count++;
                }
                CallersLabel.Text = count.ToString() + " methods called " + name;

                count = 0;
                foreach (FunctionCall call in XRay.CallMap.Values
                    .TakeWhile(v => v != null)
                    .Where(v => v.Source == id))
                {
                    XNode called = XRay.Nodes[call.Destination];
                    CalledList.Items.Add(new CallItem(call, called));
                    count++;
                }
                CalledLabel.Text = name + " called " + count.ToString() + " methods";
            }
            else
            {
                CallersLabel.Text = name + " has " + node.Nodes.Count.ToString() + " children";

                FunctionPanel.Panel2Collapsed = true;
                // when namespace selected, show children and total time inside / hits, summed for all children

                // need refresh button, or time the update and if update takes longer than 100ms, then use refresh link
                

                foreach (XNode child in node.Nodes)
                {
                    CallersList.Items.Add(new CallItem(null, child));
                }
            }
        }

        private void ParentsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ContextMenu menu = new ContextMenu();

            string indent = "";
            foreach (XNode parent in Current.Value.GetParents())
            {
                XNode copy = parent;
                menu.MenuItems.Add(new MenuItem(indent + copy.Name, (s, a) => NavigateTo(copy)));
                indent += "  ";
            }

            menu.Show(this, this.PointToClient(Cursor.Position));
        }

        private void ChildrenLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ContextMenu menu = new ContextMenu();

            foreach (XNode child in Current.Value.Nodes)
            {
                XNode copy = child;
                menu.MenuItems.Add(copy.Name, (s, a) => NavigateTo(copy));
            }

            menu.Show(this, this.PointToClient(Cursor.Position));
        }

        private void BackLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Current = Current.Previous;
            Reload();
        }

        private void ForwardLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Current = Current.Next;
            Reload();
        }

        private void CallersList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (CallersList.SelectedItems.Count == 0)
                return;

            CallItem item = CallersList.SelectedItems[0] as CallItem;

            NavigateTo(item.Node);
        }

        private void CalledList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (CalledList.SelectedItems.Count == 0)
                return;

            CallItem item = CalledList.SelectedItems[0] as CallItem;

            NavigateTo(item.Node);
        }
    }

    class CallItem : ListViewItem
    {
        FunctionCall Call;
        internal XNode Node;

        public CallItem(FunctionCall call, XNode node)
        {
            Call = call;
            Node = node;
            Text = node.AppendClassName();

            if (call == null)
                return;

            if (call.StillInside > 0 )
                Text += " (" + call.StillInside.ToString() + " Still Inside)";

            SubItems.Add(call.TotalHits.ToString());

            if (call.TotalHits == 0)
                return;

            long avgTicks = call.TotalTicks / call.TotalHits;

            SubItems.Add(TicksToString(avgTicks));
            SubItems.Add(TicksToString(call.TotalTicks));
        }

        private string TicksToString(long ticks)
        {
            TimeSpan span = new TimeSpan(ticks);

            if (span.TotalMinutes > 1)
                return span.TotalMinutes.ToString("0.## min");
            
            else if (span.TotalSeconds > 1)
                return span.TotalSeconds.ToString("0.## sec");
            
            else
                return span.TotalMilliseconds.ToString() + " ms";
        }

    }
}
