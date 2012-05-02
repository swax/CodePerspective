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
            this.SummaryLabel = new System.Windows.Forms.Label();
            this.AutoRefresh = new System.Windows.Forms.LinkLabel();
            this.RefreshTimer = new System.Windows.Forms.Timer(this.components);
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.DetailsLabel = new System.Windows.Forms.Label();
            this.FieldGrid = new AdvancedDataGridView.TreeGridView();
            this.NavButtons = new XLibrary.UI.DetailsNav();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FieldGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // SummaryLabel
            // 
            this.SummaryLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SummaryLabel.AutoSize = true;
            this.SummaryLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SummaryLabel.Location = new System.Drawing.Point(3, 0);
            this.SummaryLabel.Name = "SummaryLabel";
            this.SummaryLabel.Size = new System.Drawing.Size(37, 13);
            this.SummaryLabel.TabIndex = 11;
            this.SummaryLabel.Text = "Class";
            this.SummaryLabel.Click += new System.EventHandler(this.SummaryLabel_Click);
            // 
            // AutoRefresh
            // 
            this.AutoRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AutoRefresh.AutoSize = true;
            this.AutoRefresh.Location = new System.Drawing.Point(324, 8);
            this.AutoRefresh.Name = "AutoRefresh";
            this.AutoRefresh.Size = new System.Drawing.Size(103, 13);
            this.AutoRefresh.TabIndex = 13;
            this.AutoRefresh.TabStop = true;
            this.AutoRefresh.Text = "Turn off auto refresh";
            this.AutoRefresh.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.AutoRefresh_LinkClicked);
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
            this.flowLayoutPanel1.Location = new System.Drawing.Point(65, 8);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(253, 16);
            this.flowLayoutPanel1.TabIndex = 14;
            // 
            // DetailsLabel
            // 
            this.DetailsLabel.AutoSize = true;
            this.DetailsLabel.Location = new System.Drawing.Point(46, 0);
            this.DetailsLabel.Name = "DetailsLabel";
            this.DetailsLabel.Size = new System.Drawing.Size(37, 13);
            this.DetailsLabel.TabIndex = 12;
            this.DetailsLabel.Text = "details";
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
            this.FieldGrid.Location = new System.Drawing.Point(3, 27);
            this.FieldGrid.Name = "FieldGrid";
            this.FieldGrid.RowHeadersVisible = false;
            this.FieldGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.FieldGrid.ShowLines = false;
            this.FieldGrid.Size = new System.Drawing.Size(424, 119);
            this.FieldGrid.TabIndex = 12;
            this.FieldGrid.NodeExpanding += new AdvancedDataGridView.ExpandingEventHandler(this.FieldGrid_NodeExpanding);
            // 
            // NavButtons
            // 
            this.NavButtons.Location = new System.Drawing.Point(3, 8);
            this.NavButtons.Name = "NavButtons";
            this.NavButtons.Size = new System.Drawing.Size(56, 16);
            this.NavButtons.TabIndex = 31;
            // 
            // InstancePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.NavButtons);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.AutoRefresh);
            this.Controls.Add(this.FieldGrid);
            this.Name = "InstancePanel";
            this.Size = new System.Drawing.Size(430, 146);
            this.VisibleChanged += new System.EventHandler(this.InstancePanel_VisibleChanged);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FieldGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label SummaryLabel;
        public AdvancedDataGridView.TreeGridView FieldGrid;
        private System.Windows.Forms.LinkLabel AutoRefresh;
        private System.Windows.Forms.Timer RefreshTimer;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label DetailsLabel;
        private UI.DetailsNav NavButtons;
    }
}
