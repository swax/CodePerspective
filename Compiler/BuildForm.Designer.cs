namespace XBuilder
{
    partial class BuildForm
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
            this.FileList = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ReCompileButton = new System.Windows.Forms.Button();
            this.TestButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.AddLink = new System.Windows.Forms.LinkLabel();
            this.RemoveLink = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // FileList
            // 
            this.FileList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.FileList.FormattingEnabled = true;
            this.FileList.Location = new System.Drawing.Point(12, 36);
            this.FileList.Name = "FileList";
            this.FileList.Size = new System.Drawing.Size(259, 108);
            this.FileList.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Source exe/dll\'s";
            // 
            // ReCompileButton
            // 
            this.ReCompileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ReCompileButton.Location = new System.Drawing.Point(10, 198);
            this.ReCompileButton.Name = "ReCompileButton";
            this.ReCompileButton.Size = new System.Drawing.Size(130, 23);
            this.ReCompileButton.TabIndex = 4;
            this.ReCompileButton.Text = "Re-Compile";
            this.ReCompileButton.UseVisualStyleBackColor = true;
            this.ReCompileButton.Click += new System.EventHandler(this.ReCompileButton_Click);
            // 
            // TestButton
            // 
            this.TestButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.TestButton.Location = new System.Drawing.Point(146, 198);
            this.TestButton.Name = "TestButton";
            this.TestButton.Size = new System.Drawing.Size(130, 23);
            this.TestButton.TabIndex = 5;
            this.TestButton.Text = "Test Selected";
            this.TestButton.UseVisualStyleBackColor = true;
            this.TestButton.Click += new System.EventHandler(this.TestButton_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.Location = new System.Drawing.Point(12, 159);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(264, 36);
            this.label2.TabIndex = 6;
            this.label2.Text = "Copies of the selected files will be re-compiled, so that when they are run XRay " +
                "information is displayed.";
            // 
            // AddLink
            // 
            this.AddLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AddLink.AutoSize = true;
            this.AddLink.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.AddLink.Location = new System.Drawing.Point(192, 20);
            this.AddLink.Name = "AddLink";
            this.AddLink.Size = new System.Drawing.Size(26, 13);
            this.AddLink.TabIndex = 7;
            this.AddLink.TabStop = true;
            this.AddLink.Text = "Add";
            this.AddLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.AddLink_LinkClicked);
            // 
            // RemoveLink
            // 
            this.RemoveLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RemoveLink.AutoSize = true;
            this.RemoveLink.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.RemoveLink.Location = new System.Drawing.Point(224, 20);
            this.RemoveLink.Name = "RemoveLink";
            this.RemoveLink.Size = new System.Drawing.Size(47, 13);
            this.RemoveLink.TabIndex = 8;
            this.RemoveLink.TabStop = true;
            this.RemoveLink.Text = "Remove";
            this.RemoveLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.RemoveLink_LinkClicked);
            // 
            // BuildForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(283, 233);
            this.Controls.Add(this.RemoveLink);
            this.Controls.Add(this.AddLink);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.TestButton);
            this.Controls.Add(this.ReCompileButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.FileList);
            this.Name = "BuildForm";
            this.Text = "XRay";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox FileList;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button ReCompileButton;
        private System.Windows.Forms.Button TestButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.LinkLabel AddLink;
        private System.Windows.Forms.LinkLabel RemoveLink;

    }
}

