namespace XBuilder
{
    partial class MonitorPanel
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
            this.ProcessListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SelectedGroupBox = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.LogTextBox = new System.Windows.Forms.TextBox();
            this.ChangeTrackingLink = new System.Windows.Forms.LinkLabel();
            this.ShowViewerLink = new System.Windows.Forms.LinkLabel();
            this.RefreshTimer = new System.Windows.Forms.Timer(this.components);
            this.LastErrorLabel = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.SelectedGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ProcessListView
            // 
            this.ProcessListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.ProcessListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProcessListView.FullRowSelect = true;
            this.ProcessListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.ProcessListView.HideSelection = false;
            this.ProcessListView.Location = new System.Drawing.Point(0, 0);
            this.ProcessListView.MultiSelect = false;
            this.ProcessListView.Name = "ProcessListView";
            this.ProcessListView.Size = new System.Drawing.Size(349, 156);
            this.ProcessListView.TabIndex = 0;
            this.ProcessListView.UseCompatibleStateImageBehavior = false;
            this.ProcessListView.View = System.Windows.Forms.View.Details;
            this.ProcessListView.SelectedIndexChanged += new System.EventHandler(this.ProcessListView_SelectedIndexChanged);
            this.ProcessListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ProcessListView_MouseDoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "XRayed Process";
            this.columnHeader1.Width = 142;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "UI";
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Tracking";
            // 
            // SelectedGroupBox
            // 
            this.SelectedGroupBox.Controls.Add(this.label1);
            this.SelectedGroupBox.Controls.Add(this.LogTextBox);
            this.SelectedGroupBox.Controls.Add(this.ChangeTrackingLink);
            this.SelectedGroupBox.Controls.Add(this.ShowViewerLink);
            this.SelectedGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SelectedGroupBox.Location = new System.Drawing.Point(0, 0);
            this.SelectedGroupBox.Name = "SelectedGroupBox";
            this.SelectedGroupBox.Size = new System.Drawing.Size(349, 155);
            this.SelectedGroupBox.TabIndex = 1;
            this.SelectedGroupBox.TabStop = false;
            this.SelectedGroupBox.Text = "Selected";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(28, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Log";
            // 
            // LogTextBox
            // 
            this.LogTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LogTextBox.BackColor = System.Drawing.Color.White;
            this.LogTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LogTextBox.ForeColor = System.Drawing.Color.Black;
            this.LogTextBox.Location = new System.Drawing.Point(9, 42);
            this.LogTextBox.Multiline = true;
            this.LogTextBox.Name = "LogTextBox";
            this.LogTextBox.ReadOnly = true;
            this.LogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.LogTextBox.Size = new System.Drawing.Size(334, 106);
            this.LogTextBox.TabIndex = 2;
            // 
            // ChangeTrackingLink
            // 
            this.ChangeTrackingLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ChangeTrackingLink.AutoSize = true;
            this.ChangeTrackingLink.Location = new System.Drawing.Point(233, 26);
            this.ChangeTrackingLink.Name = "ChangeTrackingLink";
            this.ChangeTrackingLink.Size = new System.Drawing.Size(110, 13);
            this.ChangeTrackingLink.TabIndex = 1;
            this.ChangeTrackingLink.TabStop = true;
            this.ChangeTrackingLink.Text = "Toggle Tracking Calls";
            this.ChangeTrackingLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ChangeTrackingLink_LinkClicked);
            // 
            // ShowViewerLink
            // 
            this.ShowViewerLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ShowViewerLink.AutoSize = true;
            this.ShowViewerLink.Location = new System.Drawing.Point(158, 26);
            this.ShowViewerLink.Name = "ShowViewerLink";
            this.ShowViewerLink.Size = new System.Drawing.Size(69, 13);
            this.ShowViewerLink.TabIndex = 0;
            this.ShowViewerLink.TabStop = true;
            this.ShowViewerLink.Text = "Show Viewer";
            this.ShowViewerLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ShowViewerLink_LinkClicked);
            // 
            // RefreshTimer
            // 
            this.RefreshTimer.Enabled = true;
            this.RefreshTimer.Interval = 1000;
            this.RefreshTimer.Tick += new System.EventHandler(this.RefreshTimer_Tick);
            // 
            // LastErrorLabel
            // 
            this.LastErrorLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LastErrorLabel.AutoSize = true;
            this.LastErrorLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LastErrorLabel.Location = new System.Drawing.Point(3, 296);
            this.LastErrorLabel.Name = "LastErrorLabel";
            this.LastErrorLabel.Size = new System.Drawing.Size(0, 13);
            this.LastErrorLabel.TabIndex = 3;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.ProcessListView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.SelectedGroupBox);
            this.splitContainer1.Size = new System.Drawing.Size(349, 315);
            this.splitContainer1.SplitterDistance = 156;
            this.splitContainer1.TabIndex = 4;
            // 
            // MonitorPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.LastErrorLabel);
            this.Name = "MonitorPanel";
            this.Size = new System.Drawing.Size(349, 315);
            this.SelectedGroupBox.ResumeLayout(false);
            this.SelectedGroupBox.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView ProcessListView;
        private System.Windows.Forms.GroupBox SelectedGroupBox;
        private System.Windows.Forms.LinkLabel ChangeTrackingLink;
        private System.Windows.Forms.LinkLabel ShowViewerLink;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Timer RefreshTimer;
        private System.Windows.Forms.Label LastErrorLabel;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox LogTextBox;
    }
}
