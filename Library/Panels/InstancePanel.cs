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

                string isStatic = record.StaticClass ? "static " : "";

                SummaryLabel.Text = isStatic + String.Format("{0} - Created: {1}, Deleted: {2}, Active: {3}", node.Name, record.Created, record.Deleted, record.Active.Count);

                // traverse up the record's base types until we match the type for the class node selected in the UI
                // (cant show a debug matrix for types with different properties)
                string nodeTypeName = node.FullName();

                var findType = record.InstanceType;

                while (findType != null)
                {
                    string findName = findType.ToString();

                    if (nodeTypeName.EndsWith(findName))
                        break;

                    findType = findType.BaseType;
                }

                Refresh(record, findType);
            }
            else
            {
                SummaryLabel.Text = "No record of instance of " + node.Name + " type being created";

                Refresh(null, null);
            }
        }

        void Refresh(InstanceRecord record, Type selectedType)
        {
            FieldGrid.Nodes.Clear();
            FieldGrid.Columns.Clear();

            if (record == null)
                return;

            lock (record.Active)
            {
                if (record.Active.Count == 0)
                    return;

                // type col
                FieldGrid.Columns.Add(new TreeGridColumn() { HeaderText = "Type" });
                FieldGrid.Columns.Add(new DataGridViewTextBoxColumn() { HeaderText = "Name" });

                // add columns for each intance
                for (int i = 0; i < record.Active.Count; i++)
                    FieldGrid.Columns.Add(new DataGridViewTextBoxColumn() { HeaderText = "Instance " + i.ToString() });

                if (record.StaticClass && record.Active.Count == 0)
                    FieldGrid.Columns.Add(new DataGridViewTextBoxColumn() { HeaderText = "Instance" });

                AddFieldRows(FieldGrid.Nodes, selectedType, record.Active, true);
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

            // underlying type
            row = new FieldRow();
            parentNode.Add(row);

            row.Cells[0].Value = "<type>";
            row.Cells[1].Value = "";
            for (int i = 0; i < instances.Count; i++)
            {
                var weakRef = instances[i];

                if (weakRef.Target == null)
                {
                    row.Cells[2 + i].Value = "<deleted>";
                    continue;
                }

                row.Cells[2 + i].Value = weakRef.Target.GetType().ToString();
            }

            // base type
            if (objType.BaseType != null)
            {
                row = new FieldRow(objType.BaseType);
                parentNode.Add(row);

                row.ObjList = instances.ToList(); // copy cause locked and may change

                row.Cells[0].Value = "<base type>"; // name
                row.Cells[1].Value = objType.BaseType.ToString(); // type
                
                row.Nodes.Add("loading...");
            }

            // first row age of objects, just say static if static
            if (hasAge)
            {
                row = new FieldRow();
                parentNode.Add(row);

                row.Cells[0].Value = "<age>";
                row.Cells[1].Value = "";
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
                    row.Cells[2].Value = "(static)";
            }

            if (objType.GetInterface("ICollection") != null)
            {
                row = new FieldRow();
                row.Enumerate = true;
                parentNode.Add(row);

                row.Cells[0].Value = "<enumerate>"; // type
                row.Cells[1].Value = " "; // name

                row.Nodes.Add("loading...");

                for (int i = 0; i < instances.Count; i++)
                {
                    var weakRef = instances[i];

                    if (weakRef.Target == null)
                    {
                        row.Cells[2 + i].Value = "<deleted>";
                        row.ObjList.Add(new TimedWeakRef(null, false));
                        continue;
                    }

                    var objList = weakRef.Target as ICollection;

                    row.Cells[2 + i].Value = "Count: " + objList.Count.ToString();

                    row.ObjList.Add(new TimedWeakRef(objList, false));
                }
            }

            foreach (var field in objType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static).OrderBy(f => f.Name))
            {
                row = new FieldRow(field.FieldType);
                parentNode.Add(row);

                string isStatic = field.IsStatic ? "static " : "";
                row.Cells[0].Value = isStatic + Utilities.FormatTemplateName(field.FieldType.Name); // type

                row.Cells[1].Value = field.Name; // name

                for (int i = 0; i < instances.Count; i++)
                {
                    var weakRef = instances[i];

                    if (weakRef.Target == null)
                    {
                        row.Cells[2 + i].Value = "<deleted>";
                        row.ObjList.Add(new TimedWeakRef(null, false));
                        continue;
                    }
                  
                    var fieldValue = field.GetValue(weakRef.Target);
                    row.Cells[2 + i].Value = GetObjectLabel(fieldValue);

                    row.ObjList.Add(new TimedWeakRef(fieldValue, false));
                }

                if (instances.Count == 0 && field.IsStatic)
                {
                    var fieldValue = field.GetValue(null);
                    row.Cells[2].Value = GetObjectLabel(fieldValue);
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

            if (row.FieldType != null)
                AddFieldRows(row.Nodes, row.FieldType, row.ObjList, false);
            else if (row.Enumerate)
                EnumerateFieldRows(row.Nodes, row);

            AutoSizeColumns();
        }

        private void EnumerateFieldRows(TreeGridNodeCollection parentNode, FieldRow row)
        {
            var arrayRows = new List<FieldRow>();

            for (int i = 0; i < row.ObjList.Count; i++)
            {
                var weakRef = row.ObjList[i];

                if (weakRef.Target == null)
                    continue;

                var objArray = weakRef.Target as ICollection;

                int x = 0;
                var e = objArray.GetEnumerator();

                while (e.MoveNext())
                {
                    var indexValue = e.Current;

                    if (arrayRows.Count <= x)
                    {
                        row = new FieldRow(indexValue.GetType());
                        parentNode.Add(row);

                        row.Cells[0].Value = Utilities.FormatTemplateName(row.FieldType.Name); // type
                        row.Cells[1].Value = "[" + x + "]"; // name

                        arrayRows.Add(row);
                        row.Nodes.Add("loading...");
                    }
                    else
                        row = arrayRows[x];

                    row.Cells[2 + i].Value = GetObjectLabel(indexValue);

                    row.ObjList.Add(new TimedWeakRef(indexValue, false));

                    if (x > 100)
                        break;

                    x++;
                }
            }
            
        }

        public string GetObjectLabel(object obj)
        {
            if (obj == null)
                return "<null>";

            string fullname = obj.ToString();

            // System.Func`2[DeOps.Tuple`2[System.DateTime,System.String],DeOps.Interface.Tools.GeneralLogItem]
            // *xlib cant find deops namespace

            // rename instance name if it is a know type
            int pos = 0;
            int end = fullname.IndexOf('[');
            if (end == -1)
                end = fullname.Length;

            try
            {
                var firstTypeName = fullname.Substring(0, end);
                var firstType = Type.GetType(firstTypeName);
                if (firstType != null)
                    fullname = firstTypeName + fullname.Substring(end);
            }
            catch { }

            /* remove tuple dimension indicator
            int pos = name.IndexOf('`');
            int end = name.IndexOf('[');
            if (pos != -1 && end != -1 && pos < end)
            {
                name = name.Remove(pos, end - pos);
            }*/


            return fullname;

            // read until ` or [ or end of line
            

            // if substring is a known type then rewrite it

            // if ` found then replace [ with <

            // read params inside [], pass params to same function

        }

    }


    public class FieldRow : TreeGridNode
    {
        public bool Expanded;

        public Type FieldType;
        public List<TimedWeakRef> ObjList = new List<TimedWeakRef>();
        public bool Enumerate;

        public FieldRow()
        {
        }

        public FieldRow(Type type)
        {
            FieldType = type;
        }
    }
}
