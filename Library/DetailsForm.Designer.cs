namespace XLibrary
{
    partial class DetailsForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.CallersLabel = new System.Windows.Forms.Label();
            this.CallersList = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
            this.CalledLabel = new System.Windows.Forms.Label();
            this.FunctionPanel = new System.Windows.Forms.SplitContainer();
            this.CalledList = new System.Windows.Forms.ListView();
            this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader8 = new System.Windows.Forms.ColumnHeader();
            this.BackLink = new System.Windows.Forms.LinkLabel();
            this.ForwardLink = new System.Windows.Forms.LinkLabel();
            this.ParentsLink = new System.Windows.Forms.LinkLabel();
            this.ChildrenLink = new System.Windows.Forms.LinkLabel();
            this.FunctionPanel.Panel1.SuspendLayout();
            this.FunctionPanel.Panel2.SuspendLayout();
            this.FunctionPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // CallersLabel
            // 
            this.CallersLabel.AutoSize = true;
            this.CallersLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CallersLabel.Location = new System.Drawing.Point(3, 0);
            this.CallersLabel.Name = "CallersLabel";
            this.CallersLabel.Size = new System.Drawing.Size(70, 13);
            this.CallersLabel.TabIndex = 10;
            this.CallersLabel.Text = "# calls to x";
            // 
            // CallersList
            // 
            this.CallersList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.CallersList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.CallersList.FullRowSelect = true;
            this.CallersList.Location = new System.Drawing.Point(6, 16);
            this.CallersList.Name = "CallersList";
            this.CallersList.Size = new System.Drawing.Size(418, 136);
            this.CallersList.TabIndex = 11;
            this.CallersList.UseCompatibleStateImageBehavior = false;
            this.CallersList.View = System.Windows.Forms.View.Details;
            this.CallersList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.CallersList_MouseDoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 160;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Calls in";
            this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader2.Width = 75;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Avg t";
            this.columnHeader3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader3.Width = 75;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Sum t";
            this.columnHeader4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader4.Width = 75;
            // 
            // CalledLabel
            // 
            this.CalledLabel.AutoSize = true;
            this.CalledLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CalledLabel.Location = new System.Drawing.Point(0, 0);
            this.CalledLabel.Name = "CalledLabel";
            this.CalledLabel.Size = new System.Drawing.Size(83, 13);
            this.CalledLabel.TabIndex = 12;
            this.CalledLabel.Text = "# calls from x";
            // 
            // FunctionPanel
            // 
            this.FunctionPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.FunctionPanel.Location = new System.Drawing.Point(4, 37);
            this.FunctionPanel.Name = "FunctionPanel";
            this.FunctionPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // FunctionPanel.Panel1
            // 
            this.FunctionPanel.Panel1.Controls.Add(this.CallersLabel);
            this.FunctionPanel.Panel1.Controls.Add(this.CallersList);
            // 
            // FunctionPanel.Panel2
            // 
            this.FunctionPanel.Panel2.Controls.Add(this.CalledList);
            this.FunctionPanel.Panel2.Controls.Add(this.CalledLabel);
            this.FunctionPanel.Size = new System.Drawing.Size(424, 320);
            this.FunctionPanel.SplitterDistance = 155;
            this.FunctionPanel.TabIndex = 15;
            // 
            // CalledList
            // 
            this.CalledList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.CalledList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8});
            this.CalledList.FullRowSelect = true;
            this.CalledList.Location = new System.Drawing.Point(6, 16);
            this.CalledList.Name = "CalledList";
            this.CalledList.Size = new System.Drawing.Size(418, 142);
            this.CalledList.TabIndex = 12;
            this.CalledList.UseCompatibleStateImageBehavior = false;
            this.CalledList.View = System.Windows.Forms.View.Details;
            this.CalledList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.CalledList_MouseDoubleClick);
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Name";
            this.columnHeader5.Width = 160;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Calls out";
            this.columnHeader6.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader6.Width = 75;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Avg t";
            this.columnHeader7.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader7.Width = 75;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "Sum t";
            this.columnHeader8.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader8.Width = 75;
            // 
            // BackLink
            // 
            this.BackLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BackLink.AutoSize = true;
            this.BackLink.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.BackLink.Location = new System.Drawing.Point(333, 9);
            this.BackLink.Name = "BackLink";
            this.BackLink.Size = new System.Drawing.Size(32, 13);
            this.BackLink.TabIndex = 16;
            this.BackLink.TabStop = true;
            this.BackLink.Text = "Back";
            this.BackLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.BackLink_LinkClicked);
            // 
            // ForwardLink
            // 
            this.ForwardLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ForwardLink.AutoSize = true;
            this.ForwardLink.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.ForwardLink.Location = new System.Drawing.Point(371, 9);
            this.ForwardLink.Name = "ForwardLink";
            this.ForwardLink.Size = new System.Drawing.Size(45, 13);
            this.ForwardLink.TabIndex = 17;
            this.ForwardLink.TabStop = true;
            this.ForwardLink.Text = "Forward";
            this.ForwardLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ForwardLink_LinkClicked);
            // 
            // ParentsLink
            // 
            this.ParentsLink.AutoSize = true;
            this.ParentsLink.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.ParentsLink.Location = new System.Drawing.Point(7, 9);
            this.ParentsLink.Name = "ParentsLink";
            this.ParentsLink.Size = new System.Drawing.Size(43, 13);
            this.ParentsLink.TabIndex = 18;
            this.ParentsLink.TabStop = true;
            this.ParentsLink.Text = "Parents";
            this.ParentsLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ParentsLink_LinkClicked);
            // 
            // ChildrenLink
            // 
            this.ChildrenLink.AutoSize = true;
            this.ChildrenLink.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.ChildrenLink.Location = new System.Drawing.Point(56, 9);
            this.ChildrenLink.Name = "ChildrenLink";
            this.ChildrenLink.Size = new System.Drawing.Size(45, 13);
            this.ChildrenLink.TabIndex = 19;
            this.ChildrenLink.TabStop = true;
            this.ChildrenLink.Text = "Children";
            this.ChildrenLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ChildrenLink_LinkClicked);
            // 
            // DetailsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(428, 357);
            this.Controls.Add(this.ChildrenLink);
            this.Controls.Add(this.ParentsLink);
            this.Controls.Add(this.ForwardLink);
            this.Controls.Add(this.BackLink);
            this.Controls.Add(this.FunctionPanel);
            this.Name = "DetailsForm";
            this.Text = "Details";
            this.FunctionPanel.Panel1.ResumeLayout(false);
            this.FunctionPanel.Panel1.PerformLayout();
            this.FunctionPanel.Panel2.ResumeLayout(false);
            this.FunctionPanel.Panel2.PerformLayout();
            this.FunctionPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label CallersLabel;
        private System.Windows.Forms.ListView CallersList;
        private System.Windows.Forms.Label CalledLabel;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.SplitContainer FunctionPanel;
        private System.Windows.Forms.LinkLabel BackLink;
        private System.Windows.Forms.LinkLabel ForwardLink;
        private System.Windows.Forms.LinkLabel ParentsLink;
        private System.Windows.Forms.LinkLabel ChildrenLink;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ListView CalledList;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
    }
}