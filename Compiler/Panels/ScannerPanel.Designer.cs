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
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ScanButton = new System.Windows.Forms.Button();
            this.BrowseLink = new System.Windows.Forms.LinkLabel();
            this.ResultsLabel = new System.Windows.Forms.Label();
            this.ScanCountTimer = new System.Windows.Forms.Timer(this.components);
            this.PathTextBox = new System.Windows.Forms.TextBox();
            this.AddToBuildLink = new System.Windows.Forms.LinkLabel();
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
            this.columnHeader3});
            this.FilesList.FullRowSelect = true;
            this.FilesList.Location = new System.Drawing.Point(3, 32);
            this.FilesList.Name = "FilesList";
            this.FilesList.Size = new System.Drawing.Size(318, 240);
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
            // columnHeader3
            // 
            this.columnHeader3.Text = "Path";
            this.columnHeader3.Width = 565;
            // 
            // ScanButton
            // 
            this.ScanButton.Location = new System.Drawing.Point(3, 3);
            this.ScanButton.Name = "ScanButton";
            this.ScanButton.Size = new System.Drawing.Size(75, 23);
            this.ScanButton.TabIndex = 1;
            this.ScanButton.Text = "Scan";
            this.ScanButton.UseVisualStyleBackColor = true;
            this.ScanButton.Click += new System.EventHandler(this.ScanButton_Click);
            // 
            // BrowseLink
            // 
            this.BrowseLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BrowseLink.AutoSize = true;
            this.BrowseLink.Location = new System.Drawing.Point(279, 8);
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
            this.ResultsLabel.Location = new System.Drawing.Point(3, 275);
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
            this.PathTextBox.Location = new System.Drawing.Point(84, 5);
            this.PathTextBox.Name = "PathTextBox";
            this.PathTextBox.Size = new System.Drawing.Size(189, 20);
            this.PathTextBox.TabIndex = 6;
            this.PathTextBox.Text = "C:\\";
            // 
            // AddToBuildLink
            // 
            this.AddToBuildLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.AddToBuildLink.AutoSize = true;
            this.AddToBuildLink.Location = new System.Drawing.Point(212, 275);
            this.AddToBuildLink.Name = "AddToBuildLink";
            this.AddToBuildLink.Size = new System.Drawing.Size(109, 13);
            this.AddToBuildLink.TabIndex = 7;
            this.AddToBuildLink.TabStop = true;
            this.AddToBuildLink.Text = "Add Selected to Build";
            this.AddToBuildLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.AddToBuildLink_LinkClicked);
            // 
            // ScannerPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.AddToBuildLink);
            this.Controls.Add(this.PathTextBox);
            this.Controls.Add(this.ResultsLabel);
            this.Controls.Add(this.BrowseLink);
            this.Controls.Add(this.ScanButton);
            this.Controls.Add(this.FilesList);
            this.Name = "ScannerPanel";
            this.Size = new System.Drawing.Size(324, 295);
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
    }
}
