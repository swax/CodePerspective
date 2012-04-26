﻿namespace XLibrary
{
    partial class TimingPanel
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
            this.CallersLabel = new System.Windows.Forms.Label();
            this.CumulativeRadio = new System.Windows.Forms.RadioButton();
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.CalledByList = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.PerCallRadio = new System.Windows.Forms.RadioButton();
            this.CalledToList = new System.Windows.Forms.ListView();
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.CalledLabel = new System.Windows.Forms.Label();
            this.FunctionPanel = new System.Windows.Forms.SplitContainer();
            this.SelectedNameLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.FunctionPanel)).BeginInit();
            this.FunctionPanel.Panel1.SuspendLayout();
            this.FunctionPanel.Panel2.SuspendLayout();
            this.FunctionPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // CallersLabel
            // 
            this.CallersLabel.AutoSize = true;
            this.CallersLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CallersLabel.Location = new System.Drawing.Point(3, 0);
            this.CallersLabel.Name = "CallersLabel";
            this.CallersLabel.Size = new System.Drawing.Size(58, 13);
            this.CallersLabel.TabIndex = 10;
            this.CallersLabel.Text = "# calls to x";
            // 
            // CumulativeRadio
            // 
            this.CumulativeRadio.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CumulativeRadio.AutoSize = true;
            this.CumulativeRadio.Checked = true;
            this.CumulativeRadio.Location = new System.Drawing.Point(458, 0);
            this.CumulativeRadio.Name = "CumulativeRadio";
            this.CumulativeRadio.Size = new System.Drawing.Size(76, 17);
            this.CumulativeRadio.TabIndex = 29;
            this.CumulativeRadio.TabStop = true;
            this.CumulativeRadio.Text = "cumulative";
            this.CumulativeRadio.UseVisualStyleBackColor = true;
            this.CumulativeRadio.CheckedChanged += new System.EventHandler(this.CumulativeRadio_CheckedChanged);
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "t in func";
            this.columnHeader7.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader7.Width = 75;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(396, 2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 28;
            this.label1.Text = "Show time";
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "t outside";
            this.columnHeader8.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader8.Width = 75;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Calls";
            this.columnHeader6.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader6.Width = 75;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Calls";
            this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader2.Width = 75;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Called to";
            this.columnHeader5.Width = 63;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Called by";
            this.columnHeader1.Width = 66;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "t in THIS";
            this.columnHeader3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader3.Width = 75;
            // 
            // CalledByList
            // 
            this.CalledByList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CalledByList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader9});
            this.CalledByList.FullRowSelect = true;
            this.CalledByList.Location = new System.Drawing.Point(6, 16);
            this.CalledByList.Name = "CalledByList";
            this.CalledByList.Size = new System.Drawing.Size(289, 166);
            this.CalledByList.TabIndex = 11;
            this.CalledByList.UseCompatibleStateImageBehavior = false;
            this.CalledByList.View = System.Windows.Forms.View.Details;
            this.CalledByList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.CallersList_MouseDoubleClick);
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "t outside";
            this.columnHeader4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader4.Width = 75;
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "";
            // 
            // PerCallRadio
            // 
            this.PerCallRadio.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PerCallRadio.AutoSize = true;
            this.PerCallRadio.Location = new System.Drawing.Point(541, 0);
            this.PerCallRadio.Name = "PerCallRadio";
            this.PerCallRadio.Size = new System.Drawing.Size(59, 17);
            this.PerCallRadio.TabIndex = 30;
            this.PerCallRadio.Text = "per call";
            this.PerCallRadio.UseVisualStyleBackColor = true;
            this.PerCallRadio.CheckedChanged += new System.EventHandler(this.PerCallRadio_CheckedChanged);
            // 
            // CalledToList
            // 
            this.CalledToList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CalledToList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader10});
            this.CalledToList.FullRowSelect = true;
            this.CalledToList.Location = new System.Drawing.Point(6, 16);
            this.CalledToList.Name = "CalledToList";
            this.CalledToList.Size = new System.Drawing.Size(292, 166);
            this.CalledToList.TabIndex = 12;
            this.CalledToList.UseCompatibleStateImageBehavior = false;
            this.CalledToList.View = System.Windows.Forms.View.Details;
            this.CalledToList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.CalledList_MouseDoubleClick);
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "";
            // 
            // CalledLabel
            // 
            this.CalledLabel.AutoSize = true;
            this.CalledLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CalledLabel.Location = new System.Drawing.Point(3, 0);
            this.CalledLabel.Name = "CalledLabel";
            this.CalledLabel.Size = new System.Drawing.Size(69, 13);
            this.CalledLabel.TabIndex = 12;
            this.CalledLabel.Text = "# calls from x";
            // 
            // FunctionPanel
            // 
            this.FunctionPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FunctionPanel.Location = new System.Drawing.Point(0, 21);
            this.FunctionPanel.Name = "FunctionPanel";
            // 
            // FunctionPanel.Panel1
            // 
            this.FunctionPanel.Panel1.Controls.Add(this.CallersLabel);
            this.FunctionPanel.Panel1.Controls.Add(this.CalledByList);
            // 
            // FunctionPanel.Panel2
            // 
            this.FunctionPanel.Panel2.Controls.Add(this.CalledToList);
            this.FunctionPanel.Panel2.Controls.Add(this.CalledLabel);
            this.FunctionPanel.Size = new System.Drawing.Size(597, 185);
            this.FunctionPanel.SplitterDistance = 295;
            this.FunctionPanel.TabIndex = 23;
            // 
            // SelectedNameLabel
            // 
            this.SelectedNameLabel.AutoSize = true;
            this.SelectedNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SelectedNameLabel.Location = new System.Drawing.Point(3, 2);
            this.SelectedNameLabel.Name = "SelectedNameLabel";
            this.SelectedNameLabel.Size = new System.Drawing.Size(39, 13);
            this.SelectedNameLabel.TabIndex = 13;
            this.SelectedNameLabel.Text = "Name";
            // 
            // TimingPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.SelectedNameLabel);
            this.Controls.Add(this.CumulativeRadio);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.PerCallRadio);
            this.Controls.Add(this.FunctionPanel);
            this.Name = "TimingPanel";
            this.Size = new System.Drawing.Size(600, 209);
            this.FunctionPanel.Panel1.ResumeLayout(false);
            this.FunctionPanel.Panel1.PerformLayout();
            this.FunctionPanel.Panel2.ResumeLayout(false);
            this.FunctionPanel.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FunctionPanel)).EndInit();
            this.FunctionPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label CallersLabel;
        private System.Windows.Forms.RadioButton CumulativeRadio;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ListView CalledByList;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.RadioButton PerCallRadio;
        private System.Windows.Forms.ListView CalledToList;
        private System.Windows.Forms.Label CalledLabel;
        private System.Windows.Forms.SplitContainer FunctionPanel;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.Label SelectedNameLabel;
    }
}
