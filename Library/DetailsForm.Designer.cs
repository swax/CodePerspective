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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.NameLabel = new System.Windows.Forms.Label();
            this.TypeLabel = new System.Windows.Forms.Label();
            this.ParentsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.ChildrenPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.CallersLabel = new System.Windows.Forms.Label();
            this.CallersList = new System.Windows.Forms.ListView();
            this.CalledList = new System.Windows.Forms.ListView();
            this.CalledLabel = new System.Windows.Forms.Label();
            this.FunctionPanel = new System.Windows.Forms.Panel();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.FunctionPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(1, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(1, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Type:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(1, 61);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Parents:";
            // 
            // NameLabel
            // 
            this.NameLabel.AutoSize = true;
            this.NameLabel.Location = new System.Drawing.Point(50, 9);
            this.NameLabel.Name = "NameLabel";
            this.NameLabel.Size = new System.Drawing.Size(76, 13);
            this.NameLabel.TabIndex = 3;
            this.NameLabel.Text = "selected name";
            // 
            // TypeLabel
            // 
            this.TypeLabel.AutoSize = true;
            this.TypeLabel.Location = new System.Drawing.Point(50, 35);
            this.TypeLabel.Name = "TypeLabel";
            this.TypeLabel.Size = new System.Drawing.Size(70, 13);
            this.TypeLabel.TabIndex = 4;
            this.TypeLabel.Text = "selected type";
            // 
            // ParentsPanel
            // 
            this.ParentsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ParentsPanel.Location = new System.Drawing.Point(61, 61);
            this.ParentsPanel.Name = "ParentsPanel";
            this.ParentsPanel.Size = new System.Drawing.Size(231, 28);
            this.ParentsPanel.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(1, 98);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(57, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Children:";
            // 
            // ChildrenPanel
            // 
            this.ChildrenPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ChildrenPanel.Location = new System.Drawing.Point(61, 98);
            this.ChildrenPanel.Name = "ChildrenPanel";
            this.ChildrenPanel.Size = new System.Drawing.Size(231, 28);
            this.ChildrenPanel.TabIndex = 6;
            // 
            // CallersLabel
            // 
            this.CallersLabel.AutoSize = true;
            this.CallersLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CallersLabel.Location = new System.Drawing.Point(-3, 0);
            this.CallersLabel.Name = "CallersLabel";
            this.CallersLabel.Size = new System.Drawing.Size(61, 13);
            this.CallersLabel.TabIndex = 10;
            this.CallersLabel.Text = "# Callers:";
            // 
            // CallersList
            // 
            this.CallersList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.CallersList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.CallersList.Location = new System.Drawing.Point(0, 16);
            this.CallersList.Name = "CallersList";
            this.CallersList.Size = new System.Drawing.Size(288, 62);
            this.CallersList.TabIndex = 11;
            this.CallersList.UseCompatibleStateImageBehavior = false;
            this.CallersList.View = System.Windows.Forms.View.Details;
            // 
            // CalledList
            // 
            this.CalledList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.CalledList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2});
            this.CalledList.Location = new System.Drawing.Point(0, 106);
            this.CalledList.Name = "CalledList";
            this.CalledList.Size = new System.Drawing.Size(288, 62);
            this.CalledList.TabIndex = 13;
            this.CalledList.UseCompatibleStateImageBehavior = false;
            this.CalledList.View = System.Windows.Forms.View.Details;
            // 
            // CalledLabel
            // 
            this.CalledLabel.AutoSize = true;
            this.CalledLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CalledLabel.Location = new System.Drawing.Point(-3, 90);
            this.CalledLabel.Name = "CalledLabel";
            this.CalledLabel.Size = new System.Drawing.Size(54, 13);
            this.CalledLabel.TabIndex = 12;
            this.CalledLabel.Text = "# Called";
            // 
            // FunctionPanel
            // 
            this.FunctionPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.FunctionPanel.Controls.Add(this.CallersLabel);
            this.FunctionPanel.Controls.Add(this.CalledList);
            this.FunctionPanel.Controls.Add(this.CallersList);
            this.FunctionPanel.Controls.Add(this.CalledLabel);
            this.FunctionPanel.Location = new System.Drawing.Point(4, 135);
            this.FunctionPanel.Name = "FunctionPanel";
            this.FunctionPanel.Size = new System.Drawing.Size(288, 178);
            this.FunctionPanel.TabIndex = 14;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 140;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Name";
            this.columnHeader2.Width = 140;
            // 
            // DetailsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 316);
            this.Controls.Add(this.FunctionPanel);
            this.Controls.Add(this.ChildrenPanel);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.ParentsPanel);
            this.Controls.Add(this.TypeLabel);
            this.Controls.Add(this.NameLabel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "DetailsForm";
            this.Text = "Details";
            this.FunctionPanel.ResumeLayout(false);
            this.FunctionPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label NameLabel;
        private System.Windows.Forms.Label TypeLabel;
        private System.Windows.Forms.FlowLayoutPanel ParentsPanel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.FlowLayoutPanel ChildrenPanel;
        private System.Windows.Forms.Label CallersLabel;
        private System.Windows.Forms.ListView CallersList;
        private System.Windows.Forms.ListView CalledList;
        private System.Windows.Forms.Label CalledLabel;
        private System.Windows.Forms.Panel FunctionPanel;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
    }
}