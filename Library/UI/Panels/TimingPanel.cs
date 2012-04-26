using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace XLibrary
{
    public partial class TimingPanel : UserControl
    {
        LinkedListNode<XNode> Current;
        LinkedList<XNode> History = new LinkedList<XNode>();

        bool ShowPerCall = false;

        public TimingPanel()
        {
            InitializeComponent();
        }

        public void NavigateTo(XNode node)
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

            SelectedNameLabel.Text = node.AppendClassName();

            Text = "Details for (" + node.ObjType.ToString() + ") " + node.Name;

            /*ParentsLink.Enabled = node.Parent != null;
            ChildrenLink.Enabled = node.Nodes.Count > 0;

            BackLink.Enabled = Current.Previous != null;
            ForwardLink.Enabled = Current.Next != null;*/

            CalledByList.Items.Clear();
            CalledToList.Items.Clear();

            if (node.ObjType == XObjType.Method)
            {
                FunctionPanel.Panel2Collapsed = false;

                int id = node.ID;
                int count = 0;

                foreach (FunctionCall call in XRay.CallMap.Where(v => v.Destination == id))
                {
                    XNode caller = XRay.Nodes[call.Source];
                    CalledByList.Items.Add( new CallItem(call, caller, ShowPerCall));
                    count++;
                }
                CallersLabel.Text = count.ToString() + " methods called " + node.Name;

                count = 0;
                foreach (FunctionCall call in XRay.CallMap.Where(v => v.Source == id))
                {
                    XNode called = XRay.Nodes[call.Destination];
                    CalledToList.Items.Add(new CallItem(call, called, ShowPerCall));
                    count++;
                }

                CalledLabel.Text = node.Name + " called " + count.ToString() + " methods";


                long totalOutside = CalledToList.Items.Cast<CallItem>().Sum(i => i.Total);
                var totalItem = new ListViewItem(new string[] {"Total outside", "", Xtensions.TicksToString(totalOutside)});
                CalledToList.Items.Add(new ListViewItem());
                CalledToList.Items.Add(totalItem);
            }
            else
            {
                CallersLabel.Text = node.Name + " has " + node.Nodes.Count.ToString() + " children";

                FunctionPanel.Panel2Collapsed = true;
                // when namespace selected, show children and total time inside / hits, summed for all children

                // need refresh button, or time the update and if update takes longer than 100ms, then use refresh link
                

                foreach (XNode child in node.Nodes)
                {
                    CalledByList.Items.Add(new CallItem(null, child, ShowPerCall));
                }
            }

            CalledByList.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            CalledToList.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
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
            if (CalledByList.SelectedItems.Count == 0)
                return;

            CallItem item = CalledByList.SelectedItems[0] as CallItem;

            if(item != null)
                NavigateTo(item.Node);
        }

        private void CalledList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (CalledToList.SelectedItems.Count == 0)
                return;

            CallItem item = CalledToList.SelectedItems[0] as CallItem;

            if (item != null)
                NavigateTo(item.Node);
        }

        private void CumulativeRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (!CumulativeRadio.Checked)
                return;

            ShowPerCall = false;
            Reload();
        }

        private void PerCallRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (!PerCallRadio.Checked)
                return;

            ShowPerCall = true;
            Reload();
        }
    }

    class CallItem : ListViewItem
    {
        FunctionCall Call;
        internal XNode Node;
        internal long Total;

        public CallItem(FunctionCall call, XNode node, bool perCall)
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

            long inside = call.TotalTimeInsideDest;
            long outside = call.TotalTimeOutsideDest;
           
            if (perCall)
            {
                inside /= call.TotalHits;
                outside /= call.TotalHits;
            }

            SubItems.Add(Xtensions.TicksToString(inside));
            SubItems.Add(Xtensions.TicksToString(outside));

            Total = inside + outside;
        }
    }
}

