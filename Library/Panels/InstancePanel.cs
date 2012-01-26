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
        public XNodeIn SelectedNode;

        public InstancePanel()
        {
            InitializeComponent();

            //NavigateTo(node);
        }

        public void NavigateTo(XNodeIn node)
        {
            if (node.Record != null)
            {
                SelectedNode = node;
                var record = node.Record;

                string isStatic = "";// record.StaticClass ? "static " : "";

                SummaryLabel.Text = isStatic + String.Format("{0} - Created: {1}, Deleted: {2}, Active: {3}", node.Name, record.Created, record.Deleted, record.Active.Count);

                RefreshTree();
            }
            else
            {
                SelectedNode = null;

                SummaryLabel.Text = "No record of instance of " + node.Name + " type being created";

                RefreshTree();
            }
        }

        void RefreshTree()
        {
            FieldGrid.Nodes.Clear();
            FieldGrid.Columns.Clear();

            if (SelectedNode == null)
                return;

            var nodeTypeName = SelectedNode.UnformattedName;
            var record = SelectedNode.Record;
            var GenericMap = new Dictionary<string, Tuple<Type, List<ActiveRecord>>>();

            if (record != null && record.Active.Count > 0)
            {
                lock (record.Active)
                {
                    // traverse up the record's base types until we match the type for the class node selected in the UI
                    // (cant show a debug matrix for types with different properties)
                    // for example we click on the TreeView class, but the record type is of BuddyTreeView
                    for (int i = 0; i < record.Active.Count; i++)
                    {
                        var instance = record.Active[i];

                        if (!instance.IsStatic && instance.Ref.Target == null)
                            continue;

                        var recordType = instance.InstanceType;
                        var recordTypeName = "";

                        while (recordType != null)
                        {
                            recordTypeName = recordType.ToString();

                            if (recordTypeName.Contains(nodeTypeName))
                                break;

                            recordType = recordType.BaseType;
                        }

                        if (recordType == null)
                            throw new Exception(string.Format("record type not found for node type {0} and instance type {1}", nodeTypeName, recordType.ToString()));

                        recordTypeName = recordType.ToString();
                        var genericName = "";

                        if (recordTypeName.Contains('`'))
                            genericName = recordTypeName.Substring(recordTypeName.IndexOf('`'));

                        if (!GenericMap.ContainsKey(genericName))
                            GenericMap[genericName] = new Tuple<Type, List<ActiveRecord>>(recordType, new List<ActiveRecord>());

                        GenericMap[genericName].Item2.Add(instance);
                    }
                }
            }

            if (GenericMap.Values.Count == 0)
                return;

            // type col
            FieldGrid.Columns.Add(new TreeGridColumn() { HeaderText = "Type" });
            FieldGrid.Columns.Add(new DataGridViewTextBoxColumn() { HeaderText = "Name" });

            // add columns for each intance
            int mostInstances = GenericMap.Values.Max(v => v.Item2.Count);

            for (int i = 0; i < mostInstances; i++)
                FieldGrid.Columns.Add(new DataGridViewTextBoxColumn() { HeaderText = "Instance " + i.ToString() });

            if (GenericMap.Count == 1)
            {
                var recordInstance = GenericMap.First().Value;

                var row = new FieldRow(null, RowTypes.Root);
                row.Nodes = FieldGrid.Nodes;
                row.FieldType = recordInstance.Item1; // instance type that matches selected node
                row.Instances = recordInstance.Item2; // list of instances
                row.ExpandField();
            }
            else
            {
                foreach (var recordInstance in GenericMap)
                {
                    var row = new FieldRow(null, RowTypes.Root);
                    row.GenericName = recordInstance.Key;
                    row.FieldType = recordInstance.Value.Item1; // instance type that matches selected node
                    row.Instances = recordInstance.Value.Item2; // list of instances

                    FieldGrid.Nodes.Add(row);
                    row.Init();
                    row.ExpandField();
                }
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
                var width = col.Width + 10;

                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

                col.Width = Math.Min(width, 200);
            }
        }

        private void FieldGrid_NodeExpanding(object sender, ExpandingEventArgs e)
        {
            var row = e.Node as FieldRow;

            if (row.Expanded)
                return;

            row.ExpandField();

            AutoSizeColumns();
        }
    }

    public enum RowTypes { Root, Age, Base, Declared, Selected, Enumerate, Element, Field }

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

        internal void ExpandField()
        {
            if (Expanded)
                return;

            Expanded = true;
            
            Nodes.Clear();

            if (FieldType != null)
            {
                if (RowType == RowTypes.Root)
                {
                    AddRow(new FieldRow(this, RowTypes.Declared));
                    AddRow(new FieldRow(this, RowTypes.Selected, FieldType));
                    AddRow(new FieldRow(this, RowTypes.Age));
                }

                AddFieldMembers();
            }

            RefreshField();
        }

        internal void RefreshField()
        {
            for (int i = 0; i < Instances.Count && i < MAX_INSTANCES; i++)
                RefreshCell(2 + i, Instances[i]);

            if (RowType == RowTypes.Enumerate && Expanded)
                RefreshFieldEnumerations();

            if (RowType == RowTypes.Field || RowType == RowTypes.Base || RowType == RowTypes.Enumerate || RowType == RowTypes.Element)
                if(!Expanded)
                    Nodes.Add("loading...");

            foreach (var n in Nodes.OfType<FieldRow>())
                n.RefreshField();
        }

        private void RefreshCell(int i, ActiveRecord instance)
        {
            if(RowType == RowTypes.Root)
                return;

            if (RowType == RowTypes.Declared)
                Cells[i].Value = instance.InstanceType.ToString();

            // match instance with selected node type (instance may be a sub-class of the selected node)
            else if (RowType == RowTypes.Selected)
                Cells[i].Value = (FieldType != null) ? FieldType.ToString() : "<null>";

            else if (RowType == RowTypes.Base)
                Cells[i].Value = FieldType.ToString();

            else if (RowType == RowTypes.Age)
            {
                var staticTag = instance.IsStatic ? " (static)" : "";
                Cells[i].Value = ((int)(DateTime.Now - instance.Created).TotalSeconds).ToString() + staticTag;
            }

            else
            {
                if (!instance.IsStatic && instance.Ref.Target == null)
                {
                    Cells[i].Value = "<deleted>";
                    return;
                }
                
                object target = instance.IsStatic ? null : instance.Ref.Target;

                object value = GetFieldValue(target);
                if (value == null)
                {
                   // XRay.ErrorLog.Add(i.ToString());
                    Cells[i].Value = "<null>";
                    return;
                }

                if (RowType == RowTypes.Field)
                    Cells[i].Value = GetObjectLabel(value);

                else if (RowType == RowTypes.Enumerate)
                {
                    var collection = value as ICollection;

                    if (collection != null)
                        Cells[i].Value = "Count: " + collection.Count.ToString();
                    else
                        Cells[i].Value = "<not collection?>";
                }

                else if (RowType == RowTypes.Element)
                    Cells[i].Value = GetObjectLabel(value);
            }
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
                        return "<not static>";
                    
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
}
