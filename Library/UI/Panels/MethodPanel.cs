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
        ViewModel Model;
        MainForm Main;

        IColorProfile ColorProfile = new BrightColorProfile();

        Dictionary<int, CodeRow> InstructionMap = new Dictionary<int, CodeRow>();


        public CodePanel()
        {
            InitializeComponent();

            ListViewHelper.EnableDoubleBuffer(MsilView);
        }

        public void Init(MainForm main)
        {
            Main = main;
            Model = main.Model;

            if (!DesignMode)
            {
                CSharpView.DocumentText = ContentPage;
                UpdateCodeView();
            }

            ProfileView.Init(main);
            NavButtons.Init(main);
        }

        public void NavigateTo(NodeModel node)
        {
            if (node.ObjType != XObjType.Method)
                return;

            var xNode = node.XNode;

            if (xNode == SelectedNode)
                return;

            SelectedNode = xNode;
            ProfileView.NavigateTo(node);

            RefreshView();
        }

        void RefreshView()
        {
            if (!Visible)
                return;

            CurrentDisplay = SelectedNode;

            SummaryLabel.Text = "";
            SummaryLabel.ForeColor = ColorProfile.MethodColor;

            if (SelectedNode != null)
            {
                SummaryLabel.Text = Utilities.GetMethodName(SelectedNode, SelectedNode.ID);

                if (SelectedNode.External)
                    DetailsLabel.Text = "Not XRayed";
                else
                    DetailsLabel.Text = "";
            }

            RefreshMsilView();
            RefreshCSharpView();
        }

        void RefreshMsilView()
        {
            MsilView.BeginUpdate();

            InstructionMap.Clear();
            MsilView.Items.Clear();

            if (SelectedNode == null || !SelectedNode.LoadMsil())
            {
                MsilView.EndUpdate();
                return;
            }

            foreach (var inst in SelectedNode.Msil)
            {
                var row = new CodeRow(inst);
                MsilView.Items.Add(row);

                InstructionMap[inst.Offset] = row;
            }

            MsilView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

            MsilView.EndUpdate();
        }

        private void CodePanel_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible && SelectedNode != CurrentDisplay)
                RefreshView();
        }

        private void MsilRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCodeView();
        }

        private void CSharpRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCodeView();
        }

        private void ProfileRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCodeView();
        }

        void UpdateCodeView()
        {
            if (MsilRadioButton.Checked)
                ShowPanel(MsilView);
            
            else if (CSharpRadioButton.Checked)
                ShowPanel(CSharpView);

            else if (ProfileRadioButton.Checked)
                ShowPanel(ProfileView);
        }

        void ShowPanel(Control panel)
        {
            if (panel.Visible)
                return;

            panel.Dock = DockStyle.Fill;
            
            MsilView.Visible = (MsilView == panel);
            CSharpView.Visible = (CSharpView == panel);
            ProfileView.Visible = (ProfileView == panel);
        }

        const string ContentPage =
               @"<html>
                <head>
	                <style type='text/css'>
		                body { margin: 0; font-size: 8.25pt; font-family: Consolas,'Lucida Console','DejaVu Sans Mono',monospace; }

		                A:link, A:visited, A:active {text-decoration: none; color: blue;}
		                A:hover {text-decoration: underline; color: blue;}

		                .header{color: white;}
		                A.header:link, A.header:visited, A.header:active {text-decoration: none; color: white;}
		                A.header:hover {text-decoration: underline; color: white;}
                		
                        .untrusted{text-decoration: blink; line-height: 18pt;}
                        A.untrusted:link, A.untrusted:visited, A.untrusted:active {text-decoration: none; color: red;}
                        A.untrusted:hover {text-decoration: underline; color: red;}

		                .content{padding: 3px; line-height: 12pt;}
                		
	                </style>

	                <script>
		                function SetElement(id, text)
		                {
			                document.getElementById(id).innerHTML = text;
		                }
	                </script>
                </head>
                <body bgcolor='White'>

                    <div class='header' id='header'><?=header?></div>
                    <div class='content' id='content'><?=content?></div>

                </body>
                </html>";

        private void RefreshCSharpView()
        {
            try
            {
                if (SelectedNode == null || !SelectedNode.LoadCSharp())
                {
                    UpdateContent("Function not XRayed");
                    return;
                }

                // read byte stream and build html
                var code = new StringBuilder();

                // format - id, length, string
                var stream = new MemoryStream(SelectedNode.CSharp);

                while (stream.Position < stream.Length)
                {
                    var id = BitConverter.ToInt32(stream.Read(4), 0);
                    var strlen = BitConverter.ToInt32(stream.Read(4), 0);
                    string text = UTF8Encoding.UTF8.GetString(stream.Read(strlen));

                    text = text.Replace(" ", "&nbsp;"); // do here so html not messed up

                    if (id == 0)
                        code.Append(text);
                    else
                        code.Append(string.Format("<a href='http://id{0}'>{1}</a>", id, text));
                }

                code = code.Replace("\r\n", "<br />");

                UpdateContent(code.ToString());
            }
            catch (Exception ex)
            {
                UpdateContent("Error loading: " + ex.Message);
            }
        }

        private void UpdateContent(string content)
        {
            CSharpView.SafeInvokeScript("SetElement", new String[] { "content", content });
        }

        private void CSharpView_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            string url = e.Url.OriginalString;

            //if (GuiUtils.IsRunningOnMono() && url.StartsWith("wyciwyg"))
            if (url.StartsWith("wyciwyg"))
                return;

            if (url.StartsWith("about:blank"))
                return;

            e.Cancel = true;

            url = url.Replace("http://id", "");
            url = url.TrimEnd('/');

            int id;
            if (!int.TryParse(url, out id))
                return;

            var node = Model.NodeModels[id];

            Main.NavigatePanelTo(node);   
        }

        private void CodePanel_Load(object sender, EventArgs e)
        {

        }

        private void MsilView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (MsilView.SelectedItems.Count == 0)
                return;

            var item = MsilView.SelectedItems[0] as CodeRow;

            if (item.Inst.RefId == 0)
                return;

            // if a goto instruction scroll down and select that
            if (item.Inst.Line.StartsWith("goto "))
            {
                item.Selected = false;

                if (InstructionMap.ContainsKey(item.Inst.RefId))
                {
                    var target = InstructionMap[item.Inst.RefId];
                    target.Selected = true;
                    MsilView.EnsureVisible(target.Index);
                }
            }
            // else this is an instruction to another method, select thats
            else
            {
                var refNode = Model.NodeModels[item.Inst.RefId];

                Main.NavigatePanelTo(refNode);
            }
        }
    }

    public class CodeRow : ListViewItem
    {
        public XInstruction Inst;
   
        public CodeRow(XInstruction inst)
        {
            UseItemStyleForSubItems = false;

            Inst = inst;

            Text = Inst.Offset.ToString("X");
            SubItems.Add(Inst.OpCode);
            
            var lineItem = SubItems.Add(inst.Line);
      
            if (inst.RefId != 0)
                lineItem.ForeColor = Color.Blue;
        }
    }
}
