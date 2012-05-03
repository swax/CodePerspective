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

        Dictionary<string, Tuple<Type, List<ActiveRecord>>> GenericMap;

        public int MaxCols = 20;

        IColorProfile ColorProfile = new BrightColorProfile();


        public InstancePanel()
        {
            InitializeComponent();
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

            RefreshTree(true);
            RefreshSubnodes();
        }

        void RefreshTree(bool clear)
        {
            if (!Visible)
                return;

            CurrentDisplay = SelectedNode;

            if (clear)
            {
                FieldGrid.Nodes.Clear();
                FieldGrid.Columns.Clear();

                // type col
                FieldGrid.Columns.Add(new TreeGridColumn() { HeaderText = "Type" });
                FieldGrid.Columns.Add(new DataGridViewTextBoxColumn() { HeaderText = "Name" });

                GenericMap = new Dictionary<string, Tuple<Type, List<ActiveRecord>>>();
            }

            if (SelectedNode == null)
                return;

            var nodeTypeName = SelectedNode.XNode.UnformattedName;
            var record = SelectedNode.XNode.Record;

            if (record == null)
            {
                if(SelectedNode.XNode.External)
                    DetailsLabel.Text = "Not XRayed";
                else
                    DetailsLabel.Text = "No record of being created";
                return;
            }
            DetailsLabel.Text = String.Format("Active: {0}, Created: {1}, Deleted: {2}", record.Active.Count, record.Created, record.Deleted );

            // rebuild each list cause instances may be added or removed
            foreach (var recordList in GenericMap.Values)
                recordList.Item2.Clear();

            if (record != null && record.Active.Count > 0)
            {
                lock (record.Active)
                {
                    // traverse up the record's base types until we match the type for the class node selected in the UI
                    // (cant show a debug matrix for types with different properties)
                    // for example we click on the TreeView class, but the record type is of BuddyTreeView
                    for (int i = 0; i < record.Active.Count && i < MaxCols; i++)
                    {
                        var instance = record.Active[i];

                        if (!instance.IsStatic && instance.Ref.Target == null)
                            continue;

                        Type recordType = instance.InstanceType;
                        string recordTypeName = "";

                        while (recordType != null)
                        {
                            recordTypeName = recordType.ToString();

                            if (recordTypeName.Contains(nodeTypeName))
                                break;

                            recordType = recordType.BaseType;
                        }

                        if (recordType == null)
                            throw new Exception(string.Format("record type not found for node type {0} and instance type {1}", nodeTypeName, recordType.ToString()));

                        // if we're looking at a template class, then each root node is a diff type of template List<int>, List<string> etc..

                        recordTypeName = recordType.ToString();
                        string genericName = SelectedNode.Name;

                        if (recordTypeName.Contains('`'))
                            genericName = recordTypeName.Substring(recordTypeName.IndexOf('`'));

                        if (!GenericMap.ContainsKey(genericName))
                            GenericMap[genericName] = new Tuple<Type, List<ActiveRecord>>(recordType, new List<ActiveRecord>());

                        List<ActiveRecord> recordList = GenericMap[genericName].Item2;
                        if( !recordList.Contains(instance) )
                            recordList.Add(instance);
                    }
                }
            }

            // add columns for each intance
            int mostInstances = 0;
            if(GenericMap.Count > 0)
                mostInstances = GenericMap.Values.Max(v => v.Item2.Count);

            var newColumns = new List<DataGridViewColumn>();

            for (int i = 0; i < mostInstances; i++)
                if (FieldGrid.ColumnCount <= 2 + i)
                {
                    var col = new DataGridViewTextBoxColumn() { HeaderText = "Instance " + i.ToString() };
                    newColumns.Add(col);
                    FieldGrid.Columns.Add(col);
                }

            foreach (var recordInstance in GenericMap)
            {
                FieldRow row = FieldGrid.Nodes.OfType<FieldRow>().FirstOrDefault(r => r.GenericName == recordInstance.Key);
               
                if (row != null)
                {
                    row.RefreshField();
                    continue;
                }

                row = new FieldRow(null, RowTypes.Root);
                row.GenericName = recordInstance.Key;
                row.FieldType = recordInstance.Value.Item1; // instance type that matches selected node
                row.Instances = recordInstance.Value.Item2; // list of instances

                if (row.Instances.Count > 0 && row.Instances[0].IsStatic)
                    FieldGrid.Columns[2].HeaderText = "Static";

                FieldGrid.Nodes.Add(row);
                row.Init();
                row.ExpandField(FieldFilter);
            }

            foreach (FieldRow generic in FieldGrid.Nodes.OfType<FieldRow>())
                generic.Expand();

            foreach (var col in newColumns)
                AutoSizeColumn(col);
            
        }

        private void AutoSizeColumn(DataGridViewColumn col)
        {
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            var width = col.Width + 10;

            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

            col.Width = Math.Min(width, 200);
        }

        private void AutoSizeColumns()
        {
            // set autosize mode to cells
            foreach (DataGridViewColumn col in FieldGrid.Columns)
                AutoSizeColumn(col);
        }

        private void FieldGrid_NodeExpanding(object sender, ExpandingEventArgs e)
        {
            var row = e.Node as FieldRow;

            if (row.Expanded)
                return;

            row.ExpandField();

            AutoSizeColumns();
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            RefreshTimer.Enabled = false;

            RefreshTree(false);

            // start next refrseh a second after the time it took to do the actual refresh
            RefreshTimer.Enabled = AutoRefreshOn;
        }

        private void InstancePanel_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible && SelectedNode != CurrentDisplay)
                RefreshTree(true);
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

        void RefreshSubnodes()
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

    public enum RowTypes { Root, Number, Age, Base, Declared, Selected, Enumerate, Element, Field }

    public class FieldRow : TreeGridNode
    {
        public FieldRow ParentField;
        public string GenericName;
        public bool Expanded;

        RowTypes RowType;

        public Type FieldType;
        public FieldInfo TypeInfo;
        public FieldRow[] TypeChain = new FieldRow[] { };
        public int ElementIndex;

        public List<ActiveRecord> Instances = new List<ActiveRecord>();

        const int MAX_INSTANCES = 60;


        public FieldRow(FieldRow parent, RowTypes type)
        {
            RowType = type;

            if (parent == null)
                return;

            ParentField = parent;
            Instances = parent.Instances;
        }

        public FieldRow(FieldRow parent, RowTypes rowType, Type type)
            : this(parent, rowType)
        {
            FieldType = type;
        }

        public FieldRow(FieldRow parent, RowTypes rowType, Type type, int elementIndex)
            : this(parent, rowType, type)
        {
            ElementIndex = elementIndex;
        }
        
        public FieldRow(FieldRow parent, RowTypes rowType, FieldInfo info)
            : this(parent, rowType, info.FieldType)
        {
            TypeInfo = info;
        }

        public void Init()
        {
            if (RowType == RowTypes.Root)
                InitCells(GenericName, "");

            if (RowType == RowTypes.Declared)
                InitCells("<declared>", "");

            if (RowType == RowTypes.Selected)
                InitCells("<selected>", "");

            if (RowType == RowTypes.Base)
                InitCells("<base type>", "");

            else if (RowType == RowTypes.Number)
                InitCells("<instance#>", "");

            else if (RowType == RowTypes.Age)
                InitCells("<age>", "");

            else if (RowType == RowTypes.Field)
            {
                string isStatic = TypeInfo.IsStatic ? "static " : "";
                InitCells(
                    isStatic + Utilities.FormatTemplateName(FieldType.Name),
                    TypeInfo.Name
                );
            }

            else if (RowType == RowTypes.Enumerate)
                InitCells("<enumerate>", "");

            else if(RowType == RowTypes.Element)
                InitCells(
                    Utilities.FormatTemplateName(FieldType.Name),
                    "[" + ElementIndex + "]"
                );
        
            // set the type chain
            var chain = new List<FieldRow>();
            var next = this;

            while (next != null)
            {
                chain.Add(next);
                next = next.ParentField;
            }

            TypeChain = chain.Reverse<FieldRow>().ToArray();
        }

        void InitCells(string type, string value)
        {
            Cells[0].Value = type;
            Cells[1].Value = value;
        }

        internal void ExpandField(string fieldFilter=null)
        {
            if (Expanded)
                return;

            Expanded = true;
            
            Nodes.Clear();

            if (FieldType != null)
            {
                if (RowType == RowTypes.Root && fieldFilter == null)
                {
                    AddRow(new FieldRow(this, RowTypes.Declared));
                    AddRow(new FieldRow(this, RowTypes.Selected, FieldType));
                    AddRow(new FieldRow(this, RowTypes.Number)); 
                    AddRow(new FieldRow(this, RowTypes.Age));
                }

                if (fieldFilter == null)
                    AddFieldMembers();
                else
                {
                    var field = FieldType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static).FirstOrDefault(f => f.Name == fieldFilter);
                    if (field != null)
                    {
                        XRay.LogError("Field " + fieldFilter + " found on " + FieldType.ToString());

                        var row = new FieldRow(this, RowTypes.Field, field);
                        AddRow(row);
                        row.ExpandField();
                    }
                    else
                        XRay.LogError("Field " + fieldFilter + " not found on " + FieldType.ToString());
                }
            }

            RefreshField();
        }

        internal void RefreshField()
        {
            int i = 0;
            for (i = 0; i < Instances.Count && i < MAX_INSTANCES; i++)
                RefreshCell(2 + i, Instances[i]);

            for (; i < Cells.Count - 2; i++)
                SetCellValue(2 + i, "");

            if (RowType == RowTypes.Enumerate && Expanded)
                RefreshFieldEnumerations();

            if (RowType == RowTypes.Field || RowType == RowTypes.Base || RowType == RowTypes.Enumerate || RowType == RowTypes.Element)
                if(!Expanded && Nodes.Count == 0)
                    Nodes.Add("loading...");

            foreach (var n in Nodes.OfType<FieldRow>())
                n.RefreshField();
        }

        private void RefreshCell(int i, ActiveRecord instance)
        {
            if(RowType == RowTypes.Root)
                return;

            if (RowType == RowTypes.Declared)
                SetCellValue(i, instance.InstanceType.ToString());

            // match instance with selected node type (instance may be a sub-class of the selected node)
            else if (RowType == RowTypes.Selected)
                SetCellValue(i, (FieldType != null) ? FieldType.ToString() : "<null>");

            else if (RowType == RowTypes.Base)
                SetCellValue(i, FieldType.ToString());

            else if (RowType == RowTypes.Number)
                SetCellValue(i, "#" + instance.Number.ToString());

            else if (RowType == RowTypes.Age)
            {
                var staticTag = instance.IsStatic ? " (static)" : "";
                SetCellValue(i, ((int)(DateTime.Now - instance.Created).TotalSeconds).ToString() + staticTag, showUpdate: false);
            }

            else
            {
                if (!instance.IsStatic && instance.Ref.Target == null)
                {
                    SetCellValue(i, "<deleted>");
                    return;
                }
                
                object target = instance.IsStatic ? null : instance.Ref.Target;

                object value = GetFieldValue(target);
                if (value == null)
                {
                   // XRay.ErrorLog.Add(i.ToString());
                    SetCellValue(i, "<null>");
                    return;
                }

                if (RowType == RowTypes.Field)
                    SetCellValue(i, GetObjectLabel(value));

                else if (RowType == RowTypes.Enumerate)
                {
                    var collection = value as ICollection;

                    if (collection != null)
                        SetCellValue(i, "Count: " + collection.Count.ToString());
                    else
                        SetCellValue(i, "<not collection?>");
                }

                else if (RowType == RowTypes.Element)
                    SetCellValue(i, GetObjectLabel(value));
            }
        }

        public void SetCellValue(int i, string newValue, bool showUpdate=true)
        {
            var cell = Cells[i];
            var currentValue = cell.Value as string;

            if (showUpdate && currentValue != null)
            {
                if (currentValue.CompareTo(newValue) != 0)
                    cell.Style.BackColor = Color.PeachPuff;
                else
                    cell.Style.BackColor = cell.OwningRow.DefaultCellStyle.BackColor;
            }

            cell.Value = newValue;
        }

        public object GetFieldValue(object rootInstanceValue)
        {
            object current = rootInstanceValue;
            object found = null;
            
            // dont need to traverse type chain for static types
            FieldRow[] chain = TypeChain;
            if (TypeInfo != null && TypeInfo.IsStatic)
                chain = new FieldRow[] { this };

            foreach (var link in chain)
            {
                // if has index value, get that
                if (link.RowType == RowTypes.Element)
                {
                    int i = 0;
                    var e = (current as ICollection).GetEnumerator();

                    while (e.MoveNext())
                    {
                        if (i == link.ElementIndex)
                        {
                            current = e.Current;
                            break;
                        }

                        i++;
                    }
                }
                else if (link.RowType == RowTypes.Field)
                {
                    if (!link.TypeInfo.IsStatic && current == null)
                        return ""; // "<not static>";
                    
                    current = link.TypeInfo.GetValue(current);

                    // current can be null for first lookup (static, but after that needs a value)
                    if (current == null)
                    {
                        found = current;
                        break;
                    }
                }

                // check if at end of chain
                if (link == this || 
                    (RowType == RowTypes.Enumerate && ParentField == link)) 
                {
                    found = current;
                    break;
                }
            }

            //return debugChain;
            return found;
        }

        public void AddRow(FieldRow row)
        {
            Nodes.Add(row);
            row.Init();
        }

        private void AddFieldMembers()
        {
            if (FieldType.BaseType != null)
                AddRow( new FieldRow(this, RowTypes.Base, FieldType.BaseType));

            if (FieldType.GetInterface("ICollection") != null)
                AddRow(new FieldRow(this, RowTypes.Enumerate));

            foreach (var field in FieldType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static).OrderBy(f => f.Name))
                AddRow(new FieldRow(this, RowTypes.Field, field));
        }

        private void RefreshFieldEnumerations()
        {
            for (int i = 0; i < Instances.Count && i < MAX_INSTANCES; i++)
            {
                var instance = Instances[i];

                object target = null;
                if (instance.Ref != null && instance.Ref.Target != null)
                    target = instance.Ref.Target;

                var collection = GetFieldValue(target) as ICollection;

                if(collection != null)
                IterateCollection(collection, 2 + i);
            }
        }

        public void IterateCollection(ICollection collection, int column)
        {
            int i = 0;
            var e = collection.GetEnumerator();

            while (e.MoveNext())
            {
                var indexValue = e.Current;

                if (Nodes.Count <= i)
                    AddRow(new FieldRow(this, RowTypes.Element, indexValue.GetType(), i));

                if (i > 100)
                    break;

                i++;
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
}
