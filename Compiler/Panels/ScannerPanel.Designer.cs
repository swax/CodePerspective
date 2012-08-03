namespace XBuilder
{
    partial class ScannerPanel
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
            this.FilesList = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ScanButton = new System.Windows.Forms.Button();
            this.BrowseLink = new System.Windows.Forms.LinkLabel();
            this.ResultsLabel = new System.Windows.Forms.Label();
            this.ScanCountTimer = new System.Windows.Forms.Timer(this.components);
            this.PathTextBox = new System.Windows.Forms.TextBox();
            this.AddToBuildLink = new System.Windows.Forms.LinkLabel();
            this.SubDirCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // FilesList
            // 
            this.FilesList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FilesList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader4,
            this.columnHeader3});
            this.FilesList.FullRowSelect = true;
            this.FilesList.Location = new System.Drawing.Point(3, 32);
            this.FilesList.Name = "FilesList";
            this.FilesList.Size = new System.Drawing.Size(335, 243);
            this.FilesList.TabIndex = 0;
            this.FilesList.UseCompatibleStateImageBehavior = false;
            this.FilesList.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 140;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Signed";
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "CLR";
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Path";
            this.columnHeader3.Width = 565;
            // 
            // ScanButton
            // 
            this.ScanButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ScanButton.Location = new System.Drawing.Point(196, 4);
            this.ScanButton.Name = "ScanButton";
            this.ScanButton.Size = new System.Drawing.Size(72, 23);
            this.ScanButton.TabIndex = 1;
            this.ScanButton.Text = "Scan";
            this.ScanButton.UseVisualStyleBackColor = true;
            this.ScanButton.Click += new System.EventHandler(this.ScanButton_Click);
            // 
            // BrowseLink
            // 
            this.BrowseLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BrowseLink.AutoSize = true;
            this.BrowseLink.Location = new System.Drawing.Point(148, 9);
            this.BrowseLink.Name = "BrowseLink";
            this.BrowseLink.Size = new System.Drawing.Size(42, 13);
            this.BrowseLink.TabIndex = 2;
            this.BrowseLink.TabStop = true;
            this.BrowseLink.Text = "Browse";
            this.BrowseLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.PathLink_LinkClicked);
            // 
            // ResultsLabel
            // 
            this.ResultsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ResultsLabel.AutoSize = true;
            this.ResultsLabel.Location = new System.Drawing.Point(3, 278);
            this.ResultsLabel.Name = "ResultsLabel";
            this.ResultsLabel.Size = new System.Drawing.Size(0, 13);
            this.ResultsLabel.TabIndex = 5;
            // 
            // ScanCountTimer
            // 
            this.ScanCountTimer.Enabled = true;
            this.ScanCountTimer.Interval = 1000;
            this.ScanCountTimer.Tick += new System.EventHandler(this.ScanCountTimer_Tick);
            // 
            // PathTextBox
            // 
            this.PathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PathTextBox.Location = new System.Drawing.Point(6, 6);
            this.PathTextBox.Name = "PathTextBox";
            this.PathTextBox.Size = new System.Drawing.Size(136, 20);
            this.PathTextBox.TabIndex = 6;
            this.PathTextBox.Text = "C:\\";
            this.PathTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.PathTextBox_KeyPress);
            // 
            // AddToBuildLink
            // 
            this.AddToBuildLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.AddToBuildLink.AutoSize = true;
            this.AddToBuildLink.Location = new System.Drawing.Point(229, 278);
            this.AddToBuildLink.Name = "AddToBuildLink";
            this.AddToBuildLink.Size = new System.Drawing.Size(109, 13);
            this.AddToBuildLink.TabIndex = 7;
            this.AddToBuildLink.TabStop = true;
            this.AddToBuildLink.Text = "Add Selected to Build";
            this.AddToBuildLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.AddToBuildLink_LinkClicked);
            // 
            // SubDirCheckBox
            // 
            this.SubDirCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SubDirCheckBox.AutoSize = true;
            this.SubDirCheckBox.Location = new System.Drawing.Point(274, 8);
            this.SubDirCheckBox.Name = "SubDirCheckBox";
            this.SubDirCheckBox.Size = new System.Drawing.Size(64, 17);
            this.SubDirCheckBox.TabIndex = 8;
            this.SubDirCheckBox.Text = "Sub-dirs";
            this.SubDirCheckBox.UseVisualStyleBackColor = true;
            // 
            // ScannerPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.SubDirCheckBox);
            this.Controls.Add(this.AddToBuildLink);
            this.Controls.Add(this.PathTextBox);
            this.Controls.Add(this.ResultsLabel);
            this.Controls.Add(this.BrowseLink);
            this.Controls.Add(this.ScanButton);
            this.Controls.Add(this.FilesList);
            this.Name = "ScannerPanel";
            this.Size = new System.Drawing.Size(341, 298);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView FilesList;
        private System.Windows.Forms.Button ScanButton;
        private System.Windows.Forms.LinkLabel BrowseLink;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Label ResultsLabel;
        private System.Windows.Forms.Timer ScanCountTimer;
        private System.Windows.Forms.TextBox PathTextBox;
        private System.Windows.Forms.LinkLabel AddToBuildLink;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.CheckBox SubDirCheckBox;
    }
}
