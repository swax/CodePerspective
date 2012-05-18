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
    public partial class ProfilePanel : UserControl
    {
        public ViewModel Model;
        MainForm Main;
        NodeModel SelectedNode;


        bool ShowPerCall = false;


        public ProfilePanel()
        {
            InitializeComponent();

            ListViewHelper.EnableDoubleBuffer(CalledByList);
            ListViewHelper.EnableDoubleBuffer(CalledToList);
        }

        public void Init(MainForm main)
        {
            Main = main;
            Model = main.Model;
        }

        public void NavigateTo(NodeModel node)
        {
            SelectedNode = node;

            Reload();
        }

        private void Reload()
        {
            var node = SelectedNode;

            Text = "Details for (" + node.ObjType.ToString() + ") " + node.Name;

            /*ParentsLink.Enabled = node.Parent != null;
            ChildrenLink.Enabled = node.Nodes.Count > 0;

            BackLink.Enabled = Current.Previous != null;
            ForwardLink.Enabled = Current.Next != null;*/

            //CalledByList.SelectedItems.

            CalledByList.Items.Clear();
            CalledToList.Items.Clear();

            if (node.ObjType == XObjType.Method)
            {
                FunctionPanel.Panel2Collapsed = false;

                int id = node.ID;
                int count = 0;

                // called from
                foreach (var call in XRay.CallMap.Where(v => v.Destination == id))
                {
                    var caller = Model.NodeModels[call.Source];
                    CalledByList.Items.Add(new CallItem(call, caller, ShowPerCall));
                    count++;
                }

                if (count > 1)
                    AddTotalRow(CalledByList);

                // called to
                count = 0;
                foreach (var call in XRay.CallMap.Where(v => v.Source == id))
                {
                    var called = Model.NodeModels[call.Destination];
                    if (called.ObjType != XObjType.Method)
                        continue;

                    CalledToList.Items.Add(new CallItem(call, called, ShowPerCall));
                    count++;
                }

                if(count > 1)
                    AddTotalRow(CalledToList);
            }
            else
            {
                FunctionPanel.Panel2Collapsed = true;
                // when namespace selected, show children and total time inside / hits, summed for all children

                // need refresh button, or time the update and if update takes longer than 100ms, then use refresh link


                foreach (var child in node.Nodes)
                    CalledByList.Items.Add(new CallItem(null, child, ShowPerCall));
            }

            CalledByList.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            CalledToList.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void AddTotalRow(ListView list)
        {
            long hits = list.Items.Cast<CallItem>().Sum(i => i.Hits);
            long inside = list.Items.Cast<CallItem>().Sum(i => i.Inside);
            long outside = list.Items.Cast<CallItem>().Sum(i => i.Outside);

            list.Items.Add(new ListViewItem());
            list.Items.Add(new ListViewItem(new string[] { "Total", hits.ToString(), Utilities.TicksToString(inside), Utilities.TicksToString(outside) }));
          
        }

        /*private void ParentsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ContextMenu menu = new ContextMenu();

            string indent = "";
            foreach (var parent in Current.Value.GetParents())
            {
                var copy = parent;
                menu.MenuItems.Add(new MenuItem(indent + copy.Name, (s, a) => NavigateTo(copy)));
                indent += "  ";
            }

            menu.Show(this, this.PointToClient(Cursor.Position));
        }

        private void ChildrenLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ContextMenu menu = new ContextMenu();

            foreach (var child in Current.Value.Nodes)
            {
                var copy = child;
                menu.MenuItems.Add(copy.Name, (s, a) => NavigateTo(copy));
            }

            menu.Show(this, this.PointToClient(Cursor.Position));
        }
        */

        private void CallersList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (CalledByList.SelectedItems.Count == 0)
                return;

            CallItem item = CalledByList.SelectedItems[0] as CallItem;

            if(item != null)
                Main.NavigatePanelTo(item.Node);
        }

        private void CalledList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (CalledToList.SelectedItems.Count == 0)
                return;

            CallItem item = CalledToList.SelectedItems[0] as CallItem;

            if (item != null)
                Main.NavigatePanelTo(item.Node);
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

        //bool AutoRefreshOn = true;

        private void AutoRefresh_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Reload();

            /*AutoRefreshOn = !AutoRefreshOn;

            RefreshTimer.Enabled = AutoRefreshOn;

            AutoRefresh.Text = AutoRefreshOn ? "Turn off auto refresh" : "Turn on auto refresh";*/
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            //Reload();
        }
    }

    class CallItem : ListViewItem
    {
        FunctionCall Call;
        internal NodeModel Node;
        internal int Hits;
        internal long Inside;
        internal long Outside;
        internal long Total;

        public CallItem(FunctionCall call, NodeModel node, bool perCall)
        {
            Call = call;
            Node = node;
            Text = node.AppendClassName();

            if (call == null)
                return;

            if (call.StillInside > 0 )
                Text += " (" + call.StillInside.ToString() + " Still Inside)";

            SubItems.Add(call.TotalHits.ToString());

            Hits = call.TotalHits;
            Inside = call.TotalTimeInsideDest;
            Outside = call.TotalTimeOutsideDest;

            if (Hits == 0)
                return;

            if (perCall)
            {
                Inside /= Hits;
                Outside /= Hits;
            }

            SubItems.Add(Utilities.TicksToString(Inside));
            SubItems.Add(Utilities.TicksToString(Outside));

            Total = Inside + Outside;
        }
    }
}

