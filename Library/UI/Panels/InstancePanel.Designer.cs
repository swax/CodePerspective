namespace XLibrary
{
    partial class InstancePanel
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InstancePanel));
            this.SummaryLabel = new System.Windows.Forms.Label();
            this.RefreshTimer = new System.Windows.Forms.Timer(this.components);
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.DetailsLabel = new System.Windows.Forms.Label();
            this.NavButtons = new XLibrary.UI.DetailsNav();
            this.FieldGrid = new AdvancedDataGridView.TreeGridView();
            this.PauseButton = new System.Windows.Forms.Button();
            this.PlayButton = new System.Windows.Forms.Button();
            this.FieldsRadioButton = new System.Windows.Forms.RadioButton();
            this.MethodsRadioButton = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.SubnodesView = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FieldGrid)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // SummaryLabel
            // 
            this.SummaryLabel.AutoSize = true;
            this.SummaryLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.SummaryLabel.Location = new System.Drawing.Point(4, 0);
            this.SummaryLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.SummaryLabel.Name = "SummaryLabel";
            this.SummaryLabel.Size = new System.Drawing.Size(47, 17);
            this.SummaryLabel.TabIndex = 11;
            this.SummaryLabel.Text = "Class";
            // 
            // RefreshTimer
            // 
            this.RefreshTimer.Enabled = true;
            this.RefreshTimer.Interval = 1000;
            this.RefreshTimer.Tick += new System.EventHandler(this.RefreshTimer_Tick);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.Controls.Add(this.SummaryLabel);
            this.flowLayoutPanel1.Controls.Add(this.DetailsLabel);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(87, 12);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(281, 25);
            this.flowLayoutPanel1.TabIndex = 14;
            // 
            // DetailsLabel
            // 
            this.DetailsLabel.AutoSize = true;
            this.DetailsLabel.Location = new System.Drawing.Point(59, 0);
            this.DetailsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.DetailsLabel.Name = "DetailsLabel";
            this.DetailsLabel.Size = new System.Drawing.Size(53, 20);
            this.DetailsLabel.TabIndex = 12;
            this.DetailsLabel.Text = "details";
            // 
            // NavButtons
            // 
            this.NavButtons.Location = new System.Drawing.Point(4, 12);
            this.NavButtons.Margin = new System.Windows.Forms.Padding(5, 8, 5, 8);
            this.NavButtons.Name = "NavButtons";
            this.NavButtons.Size = new System.Drawing.Size(75, 25);
            this.NavButtons.TabIndex = 31;
            // 
            // FieldGrid
            // 
            this.FieldGrid.AllowUserToAddRows = false;
            this.FieldGrid.AllowUserToDeleteRows = false;
            this.FieldGrid.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.FieldGrid.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.FieldGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FieldGrid.BackgroundColor = System.Drawing.Color.White;
            this.FieldGrid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.FieldGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.FieldGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.FieldGrid.GridColor = System.Drawing.Color.WhiteSmoke;
            this.FieldGrid.ImageList = null;
            this.FieldGrid.Location = new System.Drawing.Point(4, 5);
            this.FieldGrid.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FieldGrid.Name = "FieldGrid";
            this.FieldGrid.RowHeadersVisible = false;
            this.FieldGrid.RowHeadersWidth = 51;
            this.FieldGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.FieldGrid.ShowLines = false;
            this.FieldGrid.Size = new System.Drawing.Size(249, 165);
            this.FieldGrid.TabIndex = 12;
            this.FieldGrid.NodeExpanding += new AdvancedDataGridView.ExpandingEventHandler(this.FieldGrid_NodeExpanding);
            // 
            // PauseButton
            // 
            this.PauseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PauseButton.FlatAppearance.BorderSize = 0;
            this.PauseButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.PauseButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
            this.PauseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PauseButton.Image = ((System.Drawing.Image)(resources.GetObject("PauseButton.Image")));
            this.PauseButton.Location = new System.Drawing.Point(376, 12);
            this.PauseButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.PauseButton.Name = "PauseButton";
            this.PauseButton.Size = new System.Drawing.Size(20, 20);
            this.PauseButton.TabIndex = 32;
            this.PauseButton.UseVisualStyleBackColor = true;
            this.PauseButton.Click += new System.EventHandler(this.PauseButton_Click);
            // 
            // PlayButton
            // 
            this.PlayButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PlayButton.FlatAppearance.BorderSize = 0;
            this.PlayButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.PlayButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
            this.PlayButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PlayButton.Image = ((System.Drawing.Image)(resources.GetObject("PlayButton.Image")));
            this.PlayButton.Location = new System.Drawing.Point(376, 12);
            this.PlayButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.PlayButton.Name = "PlayButton";
            this.PlayButton.Size = new System.Drawing.Size(20, 20);
            this.PlayButton.TabIndex = 33;
            this.PlayButton.UseVisualStyleBackColor = true;
            this.PlayButton.Visible = false;
            this.PlayButton.Click += new System.EventHandler(this.PlayButton_Click);
            // 
            // FieldsRadioButton
            // 
            this.FieldsRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.FieldsRadioButton.AutoSize = true;
            this.FieldsRadioButton.Checked = true;
            this.FieldsRadioButton.Location = new System.Drawing.Point(405, 9);
            this.FieldsRadioButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FieldsRadioButton.Name = "FieldsRadioButton";
            this.FieldsRadioButton.Size = new System.Drawing.Size(68, 24);
            this.FieldsRadioButton.TabIndex = 34;
            this.FieldsRadioButton.TabStop = true;
            this.FieldsRadioButton.Text = "Fields";
            this.FieldsRadioButton.UseVisualStyleBackColor = true;
            this.FieldsRadioButton.CheckedChanged += new System.EventHandler(this.FieldsRadioButton_CheckedChanged);
            // 
            // MethodsRadioButton
            // 
            this.MethodsRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.MethodsRadioButton.AutoSize = true;
            this.MethodsRadioButton.Location = new System.Drawing.Point(481, 9);
            this.MethodsRadioButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MethodsRadioButton.Name = "MethodsRadioButton";
            this.MethodsRadioButton.Size = new System.Drawing.Size(88, 24);
            this.MethodsRadioButton.TabIndex = 35;
            this.MethodsRadioButton.Text = "Methods";
            this.MethodsRadioButton.UseVisualStyleBackColor = true;
            this.MethodsRadioButton.CheckedChanged += new System.EventHandler(this.MethodsRadioButton_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.SubnodesView);
            this.panel1.Controls.Add(this.FieldGrid);
            this.panel1.Location = new System.Drawing.Point(4, 46);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(569, 174);
            this.panel1.TabIndex = 36;
            // 
            // SubnodesView
            // 
            this.SubnodesView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.SubnodesView.FullRowSelect = true;
            this.SubnodesView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.SubnodesView.Location = new System.Drawing.Point(261, 5);
            this.SubnodesView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.SubnodesView.MultiSelect = false;
            this.SubnodesView.Name = "SubnodesView";
            this.SubnodesView.Size = new System.Drawing.Size(303, 162);
            this.SubnodesView.TabIndex = 30;
            this.SubnodesView.UseCompatibleStateImageBehavior = false;
            this.SubnodesView.View = System.Windows.Forms.View.Details;
            this.SubnodesView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.SubnodesView_MouseDoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Child";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Type";
            this.columnHeader2.Width = 82;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Time";
            this.columnHeader3.Width = 174;
            // 
            // InstancePanel
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.MethodsRadioButton);
            this.Controls.Add(this.FieldsRadioButton);
            this.Controls.Add(this.PlayButton);
            this.Controls.Add(this.PauseButton);
            this.Controls.Add(this.NavButtons);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "InstancePanel";
            this.Size = new System.Drawing.Size(573, 225);
            this.VisibleChanged += new System.EventHandler(this.InstancePanel_VisibleChanged);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FieldGrid)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label SummaryLabel;
        public AdvancedDataGridView.TreeGridView FieldGrid;
        private System.Windows.Forms.Timer RefreshTimer;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label DetailsLabel;
        private UI.DetailsNav NavButtons;
        private System.Windows.Forms.Button PauseButton;
        private System.Windows.Forms.Button PlayButton;
        private System.Windows.Forms.RadioButton FieldsRadioButton;
        private System.Windows.Forms.RadioButton MethodsRadioButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListView SubnodesView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
    }
}
