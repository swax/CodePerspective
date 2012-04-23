namespace XLibrary.Panels
{
    partial class CodePanel
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
            this.CodeList = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ShowSigCheck = new System.Windows.Forms.CheckBox();
            this.MethodNameLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // CodeList
            // 
            this.CodeList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CodeList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.CodeList.FullRowSelect = true;
            this.CodeList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.CodeList.HideSelection = false;
            this.CodeList.Location = new System.Drawing.Point(3, 27);
            this.CodeList.MultiSelect = false;
            this.CodeList.Name = "CodeList";
            this.CodeList.Size = new System.Drawing.Size(383, 197);
            this.CodeList.TabIndex = 15;
            this.CodeList.UseCompatibleStateImageBehavior = false;
            this.CodeList.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Offset";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "OpCode";
            this.columnHeader2.Width = 82;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Operand";
            this.columnHeader3.Width = 174;
            // 
            // ShowSigCheck
            // 
            this.ShowSigCheck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ShowSigCheck.AutoSize = true;
            this.ShowSigCheck.Location = new System.Drawing.Point(297, 4);
            this.ShowSigCheck.Name = "ShowSigCheck";
            this.ShowSigCheck.Size = new System.Drawing.Size(85, 17);
            this.ShowSigCheck.TabIndex = 16;
            this.ShowSigCheck.Text = "Show full sig";
            this.ShowSigCheck.UseVisualStyleBackColor = true;
            this.ShowSigCheck.CheckedChanged += new System.EventHandler(this.ShowSigCheck_CheckedChanged);
            // 
            // MethodNameLabel
            // 
            this.MethodNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MethodNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MethodNameLabel.Location = new System.Drawing.Point(3, 5);
            this.MethodNameLabel.Name = "MethodNameLabel";
            this.MethodNameLabel.Size = new System.Drawing.Size(288, 19);
            this.MethodNameLabel.TabIndex = 17;
            this.MethodNameLabel.Text = "method name";
            // 
            // CodePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.MethodNameLabel);
            this.Controls.Add(this.ShowSigCheck);
            this.Controls.Add(this.CodeList);
            this.Name = "CodePanel";
            this.Size = new System.Drawing.Size(389, 227);
            this.VisibleChanged += new System.EventHandler(this.CodePanel_VisibleChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView CodeList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.CheckBox ShowSigCheck;
        private System.Windows.Forms.Label MethodNameLabel;
    }
}
