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
            this.FieldGrid = new AdvancedDataGridView.TreeGridView();
            this.AutoRefresh = new System.Windows.Forms.LinkLabel();
            this.RefreshTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.FieldGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // SummaryLabel
            // 
            this.SummaryLabel.AutoSize = true;
            this.SummaryLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SummaryLabel.Location = new System.Drawing.Point(3, 0);
            this.SummaryLabel.Name = "SummaryLabel";
            this.SummaryLabel.Size = new System.Drawing.Size(57, 13);
            this.SummaryLabel.TabIndex = 11;
            this.SummaryLabel.Text = "Summary";
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
            this.FieldGrid.Location = new System.Drawing.Point(3, 16);
            this.FieldGrid.Name = "FieldGrid";
            this.FieldGrid.RowHeadersVisible = false;
            this.FieldGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.FieldGrid.ShowLines = false;
            this.FieldGrid.Size = new System.Drawing.Size(424, 127);
            this.FieldGrid.TabIndex = 12;
            this.FieldGrid.NodeExpanding += new AdvancedDataGridView.ExpandingEventHandler(this.FieldGrid_NodeExpanding);
            // 
            // AutoRefresh
            // 
            this.AutoRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AutoRefresh.AutoSize = true;
            this.AutoRefresh.Location = new System.Drawing.Point(324, 0);
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
            // InstancePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.AutoRefresh);
            this.Controls.Add(this.FieldGrid);
            this.Controls.Add(this.SummaryLabel);
            this.Name = "InstancePanel";
            this.Size = new System.Drawing.Size(430, 146);
            this.VisibleChanged += new System.EventHandler(this.InstancePanel_VisibleChanged);
            ((System.ComponentModel.ISupportInitialize)(this.FieldGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label SummaryLabel;
        public AdvancedDataGridView.TreeGridView FieldGrid;
        private System.Windows.Forms.LinkLabel AutoRefresh;
        private System.Windows.Forms.Timer RefreshTimer;
    }
}
