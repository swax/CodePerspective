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


namespace XLibrary
{
    public partial class InstancePanel : UserControl
    {
        public InstancePanel()
        {
            InitializeComponent();

            //NavigateTo(node);
        }

        public void NavigateTo(XNodeIn node)
        {
            if (node.Record != null)
            {
                var record = node.Record;

                SummaryLabel.Text = String.Format("{0} - Static: {1}, Created: {2}, Deleted: {3}, Active: {4}", node.Name, record.StaticClass, record.Created, record.Deleted, record.Active.Count);

                Refresh(record);
            }
            else
            {
                SummaryLabel.Text = "No record of instance of " + node.Name + " type being created";

                Refresh(null);
            }
        }

        void Refresh(InstanceRecord record)
        {
            FieldGrid.Nodes.Clear();
            FieldGrid.Columns.Clear();

            if (record == null)
                return;

            lock (record.Active)
            {
                // type col
                FieldGrid.Columns.Add(new TreeGridColumn() { HeaderText = "Type" });
                FieldGrid.Columns.Add(new DataGridViewTextBoxColumn() { HeaderText = "Name" });

                // add columns for each intance
                for (int i = 0; i < record.Active.Count; i++)
                    FieldGrid.Columns.Add(new DataGridViewTextBoxColumn() { HeaderText = "Instance " + i.ToString() });

                if (record.StaticClass && record.Active.Count == 0)
                    FieldGrid.Columns.Add(new DataGridViewTextBoxColumn() { HeaderText = "Instance" });

                AddFieldRows(FieldGrid.Nodes, record.InstanceType, record.Active, true);
            }

            AutoSizeColumns();
        }

        private void AutoSizeColumns()
        {
            // set autosize mode
            foreach(DataGridViewColumn col in FieldGrid.Columns)
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            foreach(DataGridViewColumn col in FieldGrid.Columns)
            {
                var width = col.Width;

                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

                col.Width = Math.Min(width, 200);
            }
        }

        private void AddFieldRows(TreeGridNodeCollection parentNode, Type objType, List<TimedWeakRef> instances, bool hasAge)
        {
            FieldRow row = null;

            // first row age of objects, just say static if static
            if (hasAge)
            {
                row = new FieldRow();
                parentNode.Add(row);

                row.Cells[0].Value = "<instance>";
                row.Cells[1].Value = "Age";
                for (int i = 0; i < instances.Count; i++)
                {
                    var weakRef = instances[i];

                    if (weakRef.Target == null)
                    {
                        row.Cells[2 + i].Value = "<deleted>";
                        continue;
                    }

                    row.Cells[2 + i].Value = ((int)(DateTime.Now - weakRef.Created).TotalSeconds).ToString();
                }

                if (instances.Count == 0)
                    row.Cells[2].Value = "<static>";
            }

            /* underlying type
            row = new FieldRow();
            parentNode.Add(row);

            row.Cells[0].Value = "<instance>";
            row.Cells[1].Value = "Decalared Type";
            for (int i = 0; i < instances.Count; i++)
            {
                var weakRef = instances[i];

                if (weakRef.Target == null)
                {
                    row.Cells[2 + i].Value = "<deleted>";
                    continue;
                }

                row.Cells[2 + i].Value = weakRef.Target.GetType().ToString();
            }*/

            // base type
            if (objType.BaseType != null)
            {
                row = new FieldRow(objType.BaseType);
                parentNode.Add(row);

                row.ObjList = instances.ToList(); // copy cause locked and may change

                row.Cells[0].Value = objType.BaseType.ToString(); // type
                row.Cells[1].Value = "<base class>"; // name

                row.Nodes.Add("loading...");
            }

            // todo - showing instance specific values, maybe in a different

            foreach (var field in objType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static).OrderBy(f => f.Name))
            {
                row = new FieldRow(field.FieldType);
                parentNode.Add(row);

                row.Cells[0].Value = field.FieldType.Name.ToString(); // type
                row.Cells[1].Value = field.Name; // name

                for (int i = 0; i < instances.Count; i++)
                {
                    var weakRef = instances[i];

                    if (weakRef.Target == null)
                    {
                        row.Cells[2 + i].Value = "<deleted>";
                        continue;
                    }

                    var fieldValue = field.GetValue(weakRef.Target);
                    row.Cells[2 + i].Value = (fieldValue != null) ? fieldValue.ToString() : "<null>";

                    row.ObjList.Add(new TimedWeakRef(fieldValue, false));
                }

                if (instances.Count == 0 && field.IsStatic)
                {
                    var fieldValue = field.GetValue(null);
                    row.Cells[2].Value = (fieldValue != null) ? fieldValue.ToString() : "<null>";
                }

                row.Nodes.Add("loading...");
            }
        }

        private void FieldGrid_NodeExpanding(object sender, ExpandingEventArgs e)
        {
            var row = e.Node as FieldRow;

            if (row.Expanded)
                return;
            
            row.Expanded = true;

            row.Nodes.Clear();

            if (row.ObjType != null)
                AddFieldRows(row.Nodes, row.ObjType, row.ObjList, false);

            AutoSizeColumns();
        }
    }


    public class FieldRow : TreeGridNode
    {
        public bool Expanded;

        public Type ObjType;
        public List<TimedWeakRef> ObjList = new List<TimedWeakRef>();

        public FieldRow()
        {
        }

        public FieldRow(Type type)
        {
            ObjType = type;
        }
    }
}
