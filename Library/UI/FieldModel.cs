using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;

namespace XLibrary
{
    public enum RowTypes { Root, Number, Age, Base, Declared, Selected, Enumerate, Element, Field }

    public class GridModel
    {
        public NodeModel SelectedNode;

        public string DetailsLabel;
        public string FieldFilter = "";

        public List<IFieldModel> RootNodes = new List<IFieldModel>();
        public List<string> Columns = new List<string>();

        public Dictionary<string, Tuple<Type, List<ActiveRecord>>> GenericMap = new Dictionary<string, Tuple<Type, List<ActiveRecord>>>();

        public const int MaxInstances = 20;
        public const int MaxCellChars = 50;

        public Action UpdatedTree;
        public Action<IFieldModel> ExpandedField;


        public GridModel(NodeModel node, string filter, Action updatedTree, Action<IFieldModel> expandedField)
        {
            SelectedNode = node;
            FieldFilter = filter;
            UpdatedTree = updatedTree;
            ExpandedField = expandedField;

            Columns.Add("Type");
            Columns.Add("Name");
        }

        public void BeginUpdateTree() 
        {
            if (SelectedNode == null)
                return;

            var nodeTypeName = SelectedNode.XNode.UnformattedName;
            var record = SelectedNode.XNode.Record;

            if (record == null)
            {
                if (SelectedNode.XNode.External)
                    DetailsLabel = "Not XRayed";
                else
                    DetailsLabel = "No record of being created";
                return;
            }
            DetailsLabel = String.Format("Active: {0}, Created: {1}, Deleted: {2}", record.Active.Count, record.Created, record.Deleted);

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
                    for (int i = 0; i < record.Active.Count && i < MaxInstances; i++)
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
                        if (!recordList.Contains(instance))
                            recordList.Add(instance);
                    }
                }
            }

            // add columns for each intance
            int instanceCount = 0;
            if (GenericMap.Count > 0)
                instanceCount = GenericMap.Values.Max(v => v.Item2.Count);

            var newColumns = new List<string>();

            for (int i = 0; i < instanceCount; i++)
                if (Columns.Count <= 2 + i)
                {
                    var col = "Instance " + i.ToString();
                    newColumns.Add(col);
                    Columns.Add(col);
                }

            while (Columns.Count > 2 + instanceCount)
                Columns.RemoveAt(Columns.Count - 1);

            foreach (var recordInstance in GenericMap)
            {
                var model = RootNodes.Cast<FieldModel>().FirstOrDefault(r => r.GenericName == recordInstance.Key);

                if (model != null)
                {
                    model.RefreshField();
                    continue;
                }

                model = new FieldModel(this, null, RowTypes.Root);
                model.GenericName = recordInstance.Key;
                model.FieldType = recordInstance.Value.Item1; // instance type that matches selected node
                model.Instances = recordInstance.Value.Item2; // list of instances

                if (model.Instances.Count > 0 && model.Instances[0].IsStatic)
                    Columns[2] = "Static";

                RootNodes.Add(model);
                model.Init();
                model.ExpandField(FieldFilter);
            }

            //foreach (var generic in RootNodes)
            //    generic.ExpandField();


            UpdatedTree();
        }
    }

    public interface IFieldModel
    {
        List<string> Cells { get; }
        List<IFieldModel> Nodes { get; }
        bool PossibleSubNodes { get; }
        void ExpandField(string fieldFilter = null);
    }

    public class RemoteFieldModel : IFieldModel
    {
        public List<string> Cells
        {
            get { throw new NotImplementedException(); }
        }

        public List<IFieldModel> Nodes
        {
            get { throw new NotImplementedException(); }
        }

        public bool PossibleSubNodes
        {
            get { throw new NotImplementedException(); }
        }

        public void ExpandField(string fieldFilter = null)
        {
            throw new NotImplementedException();
        }
    }

    public class FieldModel : IFieldModel
    {
        public GridModel Grid;
        public FieldModel ParentField;
        public string GenericName;
        public bool Expanded;

        RowTypes RowType;

        public Type FieldType;
        public FieldInfo TypeInfo;
        public FieldModel[] TypeChain = new FieldModel[] { };
        public int ElementIndex;

        public List<ActiveRecord> Instances = new List<ActiveRecord>();

        const int MAX_INSTANCES = 30;
        const int MAX_CELL_CHARS = 70;

        public List<IFieldModel> _nodes = new List<IFieldModel>();
        public List<IFieldModel> Nodes { get { return _nodes; } }

        public List<string> _cells = new List<string>();
        public List<string> Cells { get { return _cells; } }

        public bool _possibleSubNodes;
        public bool PossibleSubNodes { get { return _possibleSubNodes; } }


        public FieldModel(GridModel grid, FieldModel parent, RowTypes type)
        {
            Grid = grid;
            RowType = type;
            if (parent == null)
                return;

            ParentField = parent;
            Instances = parent.Instances;
        }

        public FieldModel(GridModel grid, FieldModel parent, RowTypes rowType, Type type)
            : this(grid, parent, rowType)
        {
            FieldType = type;
        }

        public FieldModel(GridModel grid, FieldModel parent, RowTypes rowType, Type type, int elementIndex)
            : this(grid, parent, rowType, type)
        {
            ElementIndex = elementIndex;
        }

        public FieldModel(GridModel grid, FieldModel parent, RowTypes rowType, FieldInfo info)
            : this(grid, parent, rowType, info.FieldType)
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

            else if (RowType == RowTypes.Element)
                InitCells(
                    Utilities.FormatTemplateName(FieldType.Name),
                    "[" + ElementIndex + "]"
                );

            // set the type chain
            var chain = new List<FieldModel>();
            var next = this;

            while (next != null)
            {
                chain.Add(next);
                next = next.ParentField;
            }

            TypeChain = chain.Reverse<FieldModel>().ToArray();
        }

        void InitCells(string type, string value)
        {
            while (_cells.Count < 2)
                _cells.Add("");

            _cells[0] = type;
            _cells[1] = value;
        }

        public void ExpandField(string fieldFilter = null)
        {
            if (Expanded)
            {
                Grid.ExpandedField(this);
                return;
            }

            Expanded = true;

            _nodes.Clear();

            if (FieldType != null)
            {
                if (RowType == RowTypes.Root && fieldFilter == null)
                {
                    AddRow(new FieldModel(Grid, this, RowTypes.Declared));
                    AddRow(new FieldModel(Grid, this, RowTypes.Selected, FieldType));
                    AddRow(new FieldModel(Grid, this, RowTypes.Number));
                    AddRow(new FieldModel(Grid, this, RowTypes.Age));
                }

                if (fieldFilter == null)
                    AddFieldMembers();
                else
                {
                    var field = FieldType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static).FirstOrDefault(f => f.Name == fieldFilter);
                    if (field != null)
                    {
                        XRay.LogError("Field " + fieldFilter + " found on " + FieldType.ToString());

                        var row = new FieldModel(Grid, this, RowTypes.Field, field);
                        AddRow(row);
                        row.ExpandField();
                    }
                    else
                        XRay.LogError("Field " + fieldFilter + " not found on " + FieldType.ToString());
                }
            }

            RefreshField();

            Grid.ExpandedField(this);
        }

        public void AddRow(FieldModel row)
        {
            _nodes.Add(row);
            row.Init();
        }

        private void AddFieldMembers()
        {
            if (FieldType.BaseType != null)
                AddRow(new FieldModel(Grid, this, RowTypes.Base, FieldType.BaseType));

            if (FieldType.GetInterface("ICollection") != null)
                AddRow(new FieldModel(Grid, this, RowTypes.Enumerate));

            foreach (var field in FieldType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static).OrderBy(f => f.Name))
                AddRow(new FieldModel(Grid, this, RowTypes.Field, field));
        }

        internal void RefreshField()
        {
            int i = 0;
            for (i = 0; i < Instances.Count && i < MAX_INSTANCES; i++)
                RefreshCell(2 + i, Instances[i]);

            for (; i < _cells.Count - 2; i++)
                SetCellValue(2 + i, "");

            if (RowType == RowTypes.Enumerate && Expanded)
                RefreshFieldEnumerations();

            if (RowType == RowTypes.Root || RowType == RowTypes.Field || RowType == RowTypes.Base || RowType == RowTypes.Enumerate || RowType == RowTypes.Element)
                if (!Expanded && _nodes.Count == 0)
                    _possibleSubNodes = true;

            foreach (var n in _nodes.Cast<FieldModel>())
                n.RefreshField();
        }

        private void RefreshCell(int i, ActiveRecord instance)
        {
            if (RowType == RowTypes.Root)
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
                SetCellValue(i, ((int)(DateTime.Now - instance.Created).TotalSeconds).ToString() + staticTag);
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

        public void SetCellValue(int i, string newValue)
        {
            while (_cells.Count <= i)
                _cells.Add("");

            if (newValue.Length > MAX_CELL_CHARS)
                newValue = newValue.Substring(0, MAX_CELL_CHARS) + "...";

            _cells[i] = newValue;
        }

        public object GetFieldValue(object rootInstanceValue)
        {
            object current = rootInstanceValue;
            object found = null;

            // dont need to traverse type chain for static types
            FieldModel[] chain = TypeChain;
            if (TypeInfo != null && TypeInfo.IsStatic)
                chain = new FieldModel[] { this };

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

        private void RefreshFieldEnumerations()
        {
            for (int i = 0; i < Instances.Count && i < MAX_INSTANCES; i++)
            {
                var instance = Instances[i];

                object target = null;
                if (instance.Ref != null && instance.Ref.Target != null)
                    target = instance.Ref.Target;

                var collection = GetFieldValue(target) as ICollection;

                if (collection != null)
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

                if (_nodes.Count <= i)
                    AddRow(new FieldModel(Grid, this, RowTypes.Element, indexValue.GetType(), i));

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
