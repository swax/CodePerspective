using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AdvancedDataGridView;
using System.IO;

namespace XLibrary.Panels
{
    public partial class CodePanel : UserControl
    {
        XNodeIn SelectedNode;
        XNodeIn CurrentDisplay;
        FileStream DatStream;

        public CodePanel()
        {
            InitializeComponent();

            
        }

        public void NavigateTo(XNodeIn node)
        {
            if (node.ObjType != XObjType.Method)
                return;

            if (node == SelectedNode)
                return;

            SelectedNode = node;

            RefreshTree();
        }

        void RefreshTree(bool force=false)
        {
            if (!Visible)
                return;

            CodeList.Items.Clear();

            CurrentDisplay = SelectedNode; 

            MethodNameLabel.Text = "";

            if (!force && (SelectedNode == null || SelectedNode.CodePos == 0))
                return;

            MethodNameLabel.Text = GetMethodName(SelectedNode.ID);


            if (SelectedNode.Instructions == null)
            {
                using (DatStream = new FileStream(XRay.DatPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    DatStream.Position = SelectedNode.CodePos;

                    SelectedNode.Instructions = new List<XInstruction>();

                    for (int i = 0; i < SelectedNode.CodeLines; i++)
                    {
                        var inst = new XInstruction();
                        inst.Offset = BitConverter.ToInt32(DatStream.Read(4), 0);
                        inst.OpCode = XNodeIn.ReadString(DatStream);
                        inst.Line = XNodeIn.ReadString(DatStream);
                        inst.RefId = BitConverter.ToInt32(DatStream.Read(4), 0);

                        SelectedNode.Instructions.Add(inst);
                    }
                }
            }

            foreach (var inst in SelectedNode.Instructions)
            {
                string line = inst.Line;
                if (inst.RefId != 0 && !line.StartsWith("goto "))
                    line = GetMethodName(inst.RefId);

                var row = new CodeRow(inst, line);
                CodeList.Items.Add(row);
            }
        }

        string GetMethodName(int nodeID)
        {
            var node = XRay.Nodes[nodeID];

            var parentClass = node.GetParentClass(false);
            bool includeClass = (parentClass != SelectedNode.GetParentClass(false));

            if (node.ObjType == XObjType.Field)
            {
                string name = node.UnformattedName;
            
                if (includeClass)
                    name = parentClass.Name + "::" + name;

                if (node.ReturnID != 0)
                {
                    var retNode = XRay.Nodes[node.ReturnID];
                    name = retNode.Name + " " + name;
                }

                return name;
            }

            else if (node.ObjType == XObjType.Method)
            {
                return node.GetMethodName(includeClass);
            }

            return "unknown";
        }

        private void CodePanel_VisibleChanged(object sender, EventArgs e)
        {
            if (SelectedNode != CurrentDisplay)
                RefreshTree();
        }

        private void ShowSigCheck_CheckedChanged(object sender, EventArgs e)
        {
            RefreshTree(true);
        }
    }

    public class CodeRow : ListViewItem
    {
        XInstruction Inst;
   
        public CodeRow(XInstruction inst, string line)
        {
            Inst = inst;

            Text = Inst.Offset.ToString("X");
            SubItems.Add(Inst.OpCode);
            SubItems.Add(line);
        }
    }
}
