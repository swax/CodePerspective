using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using AdvancedDataGridView;
using System.Reflection;
using System.Collections;
using XLibrary.UI.Panels;


namespace XLibrary
{
    public partial class InstancePanel : UserControl
    {
        MainForm Main;
        public NodeModel SelectedNode;
        public NodeModel CurrentDisplay;
        public string FieldFilter = "";

        IColorProfile ColorProfile = new BrightColorProfile();

        GridModel Model;


        public InstancePanel()
        {
            InitializeComponent();

            ListViewHelper.EnableDoubleBuffer(SubnodesView);
        }

        public void Init(MainForm main)
        {
            Main = main;

            NavButtons.Init(main);

            FieldsRadioButton_CheckedChanged(this, null);
        }

        public void NavigateTo(NodeModel node)
        {
            if (node.ObjType == XObjType.Class)
            {
                SelectedNode = node;
                FieldFilter = null;
                SummaryLabel.Text = node.Name;
                SummaryLabel.ForeColor = ColorProfile.ClassColor;
                FieldsRadioButton.Visible = true;
                MethodsRadioButton.Visible = true;
            }
            else if (node.ObjType == XObjType.Field)
            {
                SelectedNode = node.GetParentClass(false);
                FieldFilter = node.XNode.UnformattedName;
                SummaryLabel.Text = node.Name;
                SummaryLabel.ForeColor = ColorProfile.FieldColor;
                FieldsRadioButton.Visible = false;
                MethodsRadioButton.Visible = false;
                FieldsRadioButton.Checked = true;
            }
            else
            {
                SummaryLabel.Text = "";
                return;
            }

            Model = new GridModel(SelectedNode, FieldFilter);

            if (Visible)
                RefreshTree();

            RefreshSubnodesView();
        }

        void RefreshTree()
        {
            Model.RefreshTree();

            CurrentDisplay = SelectedNode;

            FieldGrid.Nodes.Clear();
            FieldGrid.Columns.Clear();

            UpdateTree();
        }

        void UpdateTree()
        {
            // if remote then function that calls this should call a diff function with this as a callback

            Model.UpdateTree();

            if (SelectedNode == null)
                return;

            DetailsLabel.Text = Model.DetailsLabel;

            var resizeColumns = new List<DataGridViewColumn>();

            for(int i = 0; i < Model.Columns.Count; i++)
            {
                if (FieldGrid.ColumnCount <= i)
                {
                    DataGridViewColumn col = (i == 0) ? new TreeGridColumn() : new DataGridViewTextBoxColumn();
                    resizeColumns.Add(col);
                    FieldGrid.Columns.Add(col);
                }
                FieldGrid.Columns[i].HeaderText = Model.Columns[i];
            }

            while(FieldGrid.Columns.Count > Model.Columns.Count)
                FieldGrid.Columns.RemoveAt(FieldGrid.Columns.Count - 1);

            SyncRows(Model.RootNodes, FieldGrid.Nodes);

            foreach (var generic in FieldGrid.Nodes.OfType<FieldRow>())
                generic.Expand();

            foreach (var col in resizeColumns)
                FieldGrid.AutoResizeColumn(col.Index);
        }

        private void SyncRows(List<IFieldModel> modelNodes, TreeGridNodeCollection viewNodes)
        {
            foreach (var model in modelNodes)
            {
                var row = viewNodes.OfType<FieldRow>().FirstOrDefault(r => r.Model == model);

                if (row == null)
                {
                    row = new FieldRow(model);
                    viewNodes.Add(row);
                    
                    if (model.PossibleSubNodes)
                        row.Nodes.Add("Loading...");
                }

                row.SyncCells();
            }
        }

        private void FieldGrid_NodeExpanding(object sender, ExpandingEventArgs e)
        {
            var row = e.Node as FieldRow;

            if (row.IsExpanded)
                return;

            row.Model.ExpandField();
            row.Nodes.Clear();

            SyncRows(row.Model.Nodes, row.Nodes);

            for (int i = 0; i < FieldGrid.Columns.Count; i++)
                FieldGrid.AutoResizeColumn(i);
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            RefreshTimer.Enabled = false;

            if (Visible) 
                UpdateTree();

            // start next refrseh a second after the time it took to do the actual refresh
            RefreshTimer.Enabled = AutoRefreshOn;
        }

        private void InstancePanel_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible && SelectedNode != CurrentDisplay)
                RefreshTree();
        }

        private void SummaryLabel_Click(object sender, EventArgs e)
        {

        }

        bool AutoRefreshOn = true;

        private void PauseButton_Click(object sender, EventArgs e)
        {
            AutoRefreshOn = false;
            RefreshTimer.Enabled = false;
            PauseButton.Visible = false;
            PlayButton.Visible = true;
        }

        private void PlayButton_Click(object sender, EventArgs e)
        {
            AutoRefreshOn = true;
            RefreshTimer.Enabled = true;
            PauseButton.Visible = true;
            PlayButton.Visible = false;
        }

        private void FieldsRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (!FieldsRadioButton.Checked)
                return;

            FieldGrid.Dock = DockStyle.Fill;
            SubnodesView.Visible = false;
            FieldGrid.Visible = true;
        }

        private void MethodsRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (!MethodsRadioButton.Checked)
                return;

            SubnodesView.Dock = DockStyle.Fill;
            FieldGrid.Visible = false;
            SubnodesView.Visible = true;
        }

        void RefreshSubnodesView()
        {
            SubnodesView.Items.Clear();

            foreach (var subnode in SelectedNode.Nodes
                                        .Where(n => n.ObjType != XObjType.Field)
                                        .OrderBy(n => n.Name)
                                        .OrderBy(n => (int)n.ObjType))
                SubnodesView.Items.Add(new SubnodeItem(subnode));

            SubnodesView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private void SubnodesView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (SubnodesView.SelectedItems.Count == 0)
                return;

            var selected = SubnodesView.SelectedItems[0] as SubnodeItem;

            Main.NavigatePanelTo(selected.Node);
        }
    }


    public class FieldRow : TreeGridNode
    {
        public string GenericName;

        public IFieldModel Model;

        public FieldRow(IFieldModel model)
        {
            Model = model;
        }

        internal void SyncCells()
        {
            for (int i = 0; i < Model.Cells.Count; i++)
                if (i < Cells.Count)
                {
                    var newValue = Model.Cells[i];

                    var cell = Cells[i];
                    var currentValue = cell.Value as string;

                    if (currentValue != null)
                    {
                        if (currentValue.CompareTo(newValue) != 0)
                        {
                            double currentD, newD;

                            if (double.TryParse(currentValue, out currentD) && double.TryParse(newValue, out newD))
                                cell.Style.BackColor = (newD > currentD) ? Color.LightGreen : Color.LightCoral;
                            else
                                cell.Style.BackColor = Color.LightBlue;
                        }
                        else
                            cell.Style.BackColor = cell.OwningRow.DefaultCellStyle.BackColor;
                    }

                    cell.Value = newValue;
                }

            if (IsExpanded)
                foreach (var node in Nodes.OfType<FieldRow>())
                    node.SyncCells();
        }
    }
}
