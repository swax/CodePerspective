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
            this.LaunchButton = new System.Windows.Forms.Button();
            this.AddLink = new System.Windows.Forms.LinkLabel();
            this.RemoveLink = new System.Windows.Forms.LinkLabel();
            this.ShowMapButton = new System.Windows.Forms.Button();
            this.FlowBox = new System.Windows.Forms.CheckBox();
            this.SidebySideBox = new System.Windows.Forms.CheckBox();
            this.ResetLink = new System.Windows.Forms.LinkLabel();
            this.OutputLink = new System.Windows.Forms.LinkLabel();
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
            this.FileList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.FileList.Size = new System.Drawing.Size(313, 121);
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
            this.ReCompileButton.Location = new System.Drawing.Point(12, 212);
            this.ReCompileButton.Name = "ReCompileButton";
            this.ReCompileButton.Size = new System.Drawing.Size(99, 23);
            this.ReCompileButton.TabIndex = 4;
            this.ReCompileButton.Text = "Re-Compile";
            this.ReCompileButton.UseVisualStyleBackColor = true;
            this.ReCompileButton.Click += new System.EventHandler(this.ReCompileButton_Click);
            // 
            // LaunchButton
            // 
            this.LaunchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LaunchButton.Location = new System.Drawing.Point(222, 212);
            this.LaunchButton.Name = "LaunchButton";
            this.LaunchButton.Size = new System.Drawing.Size(99, 23);
            this.LaunchButton.TabIndex = 5;
            this.LaunchButton.Text = "Launch";
            this.LaunchButton.UseVisualStyleBackColor = true;
            this.LaunchButton.Click += new System.EventHandler(this.LaunchButton_Click);
            // 
            // AddLink
            // 
            this.AddLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AddLink.AutoSize = true;
            this.AddLink.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.AddLink.Location = new System.Drawing.Point(201, 20);
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
            this.RemoveLink.Location = new System.Drawing.Point(233, 20);
            this.RemoveLink.Name = "RemoveLink";
            this.RemoveLink.Size = new System.Drawing.Size(47, 13);
            this.RemoveLink.TabIndex = 8;
            this.RemoveLink.TabStop = true;
            this.RemoveLink.Text = "Remove";
            this.RemoveLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.RemoveLink_LinkClicked);
            // 
            // ShowMapButton
            // 
            this.ShowMapButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ShowMapButton.Location = new System.Drawing.Point(117, 212);
            this.ShowMapButton.Name = "ShowMapButton";
            this.ShowMapButton.Size = new System.Drawing.Size(99, 23);
            this.ShowMapButton.TabIndex = 9;
            this.ShowMapButton.Text = "Test Map";
            this.ShowMapButton.UseVisualStyleBackColor = true;
            this.ShowMapButton.Click += new System.EventHandler(this.ShowMapButton_Click);
            // 
            // FlowBox
            // 
            this.FlowBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.FlowBox.AutoSize = true;
            this.FlowBox.Checked = true;
            this.FlowBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.FlowBox.Location = new System.Drawing.Point(20, 172);
            this.FlowBox.Name = "FlowBox";
            this.FlowBox.Size = new System.Drawing.Size(117, 17);
            this.FlowBox.TabIndex = 10;
            this.FlowBox.Text = "Track function flow";
            this.FlowBox.UseVisualStyleBackColor = true;
            // 
            // SidebySideBox
            // 
            this.SidebySideBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SidebySideBox.AutoSize = true;
            this.SidebySideBox.Checked = true;
            this.SidebySideBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SidebySideBox.Location = new System.Drawing.Point(143, 172);
            this.SidebySideBox.Name = "SidebySideBox";
            this.SidebySideBox.Size = new System.Drawing.Size(85, 17);
            this.SidebySideBox.TabIndex = 11;
            this.SidebySideBox.Text = "Side-by-Side";
            this.SidebySideBox.UseVisualStyleBackColor = true;
            this.SidebySideBox.CheckedChanged += new System.EventHandler(this.SidebySideBox_CheckedChanged);
            // 
            // ResetLink
            // 
            this.ResetLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ResetLink.AutoSize = true;
            this.ResetLink.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.ResetLink.Location = new System.Drawing.Point(286, 20);
            this.ResetLink.Name = "ResetLink";
            this.ResetLink.Size = new System.Drawing.Size(35, 13);
            this.ResetLink.TabIndex = 12;
            this.ResetLink.TabStop = true;
            this.ResetLink.Text = "Reset";
            this.ResetLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ResetLink_LinkClicked);
            // 
            // OutputLink
            // 
            this.OutputLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.OutputLink.AutoEllipsis = true;
            this.OutputLink.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.OutputLink.Location = new System.Drawing.Point(12, 250);
            this.OutputLink.Name = "OutputLink";
            this.OutputLink.Size = new System.Drawing.Size(309, 13);
            this.OutputLink.TabIndex = 13;
            this.OutputLink.TabStop = true;
            this.OutputLink.Text = "Output: Path";
            this.OutputLink.Visible = false;
            this.OutputLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OutputLink_LinkClicked);
            // 
            // BuildForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(337, 272);
            this.Controls.Add(this.OutputLink);
            this.Controls.Add(this.ResetLink);
            this.Controls.Add(this.SidebySideBox);
            this.Controls.Add(this.FlowBox);
            this.Controls.Add(this.ShowMapButton);
            this.Controls.Add(this.RemoveLink);
            this.Controls.Add(this.AddLink);
            this.Controls.Add(this.LaunchButton);
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
        private System.Windows.Forms.Button LaunchButton;
        private System.Windows.Forms.LinkLabel AddLink;
        private System.Windows.Forms.LinkLabel RemoveLink;
        private System.Windows.Forms.Button ShowMapButton;
        private System.Windows.Forms.CheckBox FlowBox;
        private System.Windows.Forms.CheckBox SidebySideBox;
        private System.Windows.Forms.LinkLabel ResetLink;
        private System.Windows.Forms.LinkLabel OutputLink;

    }
}

