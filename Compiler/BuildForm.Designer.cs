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
            this.TrackFlowCheckBox = new System.Windows.Forms.CheckBox();
            this.SidebySideCheckBox = new System.Windows.Forms.CheckBox();
            this.ResetLink = new System.Windows.Forms.LinkLabel();
            this.OutputLink = new System.Windows.Forms.LinkLabel();
            this.TrackExternalCheckBox = new System.Windows.Forms.CheckBox();
            this.OptionsPanel = new System.Windows.Forms.Panel();
            this.GraphButton = new System.Windows.Forms.Button();
            this.TestCompile = new System.Windows.Forms.Button();
            this.TrackAnonCheckBox = new System.Windows.Forms.CheckBox();
            this.OptionsPanel.SuspendLayout();
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
            this.FileList.Size = new System.Drawing.Size(326, 95);
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
            this.ReCompileButton.Location = new System.Drawing.Point(13, 66);
            this.ReCompileButton.Name = "ReCompileButton";
            this.ReCompileButton.Size = new System.Drawing.Size(99, 23);
            this.ReCompileButton.TabIndex = 4;
            this.ReCompileButton.Text = "Re-Compile";
            this.ReCompileButton.UseVisualStyleBackColor = true;
            this.ReCompileButton.Click += new System.EventHandler(this.ReCompileButton_Click);
            // 
            // LaunchButton
            // 
            this.LaunchButton.Location = new System.Drawing.Point(123, 66);
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
            this.AddLink.Location = new System.Drawing.Point(214, 20);
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
            this.RemoveLink.Location = new System.Drawing.Point(246, 20);
            this.RemoveLink.Name = "RemoveLink";
            this.RemoveLink.Size = new System.Drawing.Size(47, 13);
            this.RemoveLink.TabIndex = 8;
            this.RemoveLink.TabStop = true;
            this.RemoveLink.Text = "Remove";
            this.RemoveLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.RemoveLink_LinkClicked);
            // 
            // ShowMapButton
            // 
            this.ShowMapButton.Location = new System.Drawing.Point(123, 95);
            this.ShowMapButton.Name = "ShowMapButton";
            this.ShowMapButton.Size = new System.Drawing.Size(99, 23);
            this.ShowMapButton.TabIndex = 9;
            this.ShowMapButton.Text = "Test Map";
            this.ShowMapButton.UseVisualStyleBackColor = true;
            this.ShowMapButton.Click += new System.EventHandler(this.ShowMapButton_Click);
            // 
            // TrackFlowCheckBox
            // 
            this.TrackFlowCheckBox.AutoSize = true;
            this.TrackFlowCheckBox.Checked = true;
            this.TrackFlowCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.TrackFlowCheckBox.Location = new System.Drawing.Point(13, 3);
            this.TrackFlowCheckBox.Name = "TrackFlowCheckBox";
            this.TrackFlowCheckBox.Size = new System.Drawing.Size(117, 17);
            this.TrackFlowCheckBox.TabIndex = 10;
            this.TrackFlowCheckBox.Text = "Track function flow";
            this.TrackFlowCheckBox.UseVisualStyleBackColor = true;
            // 
            // SidebySideCheckBox
            // 
            this.SidebySideCheckBox.AutoSize = true;
            this.SidebySideCheckBox.Checked = true;
            this.SidebySideCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SidebySideCheckBox.Location = new System.Drawing.Point(13, 26);
            this.SidebySideCheckBox.Name = "SidebySideCheckBox";
            this.SidebySideCheckBox.Size = new System.Drawing.Size(104, 17);
            this.SidebySideCheckBox.TabIndex = 11;
            this.SidebySideCheckBox.Text = "Run side by side";
            this.SidebySideCheckBox.UseVisualStyleBackColor = true;
            this.SidebySideCheckBox.CheckedChanged += new System.EventHandler(this.SidebySideBox_CheckedChanged);
            // 
            // ResetLink
            // 
            this.ResetLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ResetLink.AutoSize = true;
            this.ResetLink.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.ResetLink.Location = new System.Drawing.Point(299, 20);
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
            this.OutputLink.Location = new System.Drawing.Point(9, 291);
            this.OutputLink.Name = "OutputLink";
            this.OutputLink.Size = new System.Drawing.Size(322, 13);
            this.OutputLink.TabIndex = 13;
            this.OutputLink.TabStop = true;
            this.OutputLink.Text = "Output: Path";
            this.OutputLink.Visible = false;
            this.OutputLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OutputLink_LinkClicked);
            // 
            // TrackExternalCheckBox
            // 
            this.TrackExternalCheckBox.AutoSize = true;
            this.TrackExternalCheckBox.Checked = true;
            this.TrackExternalCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.TrackExternalCheckBox.Location = new System.Drawing.Point(153, 3);
            this.TrackExternalCheckBox.Name = "TrackExternalCheckBox";
            this.TrackExternalCheckBox.Size = new System.Drawing.Size(140, 17);
            this.TrackExternalCheckBox.TabIndex = 14;
            this.TrackExternalCheckBox.Text = "Track external functions";
            this.TrackExternalCheckBox.UseVisualStyleBackColor = true;
            // 
            // OptionsPanel
            // 
            this.OptionsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.OptionsPanel.Controls.Add(this.GraphButton);
            this.OptionsPanel.Controls.Add(this.TestCompile);
            this.OptionsPanel.Controls.Add(this.TrackAnonCheckBox);
            this.OptionsPanel.Controls.Add(this.SidebySideCheckBox);
            this.OptionsPanel.Controls.Add(this.TrackExternalCheckBox);
            this.OptionsPanel.Controls.Add(this.ReCompileButton);
            this.OptionsPanel.Controls.Add(this.LaunchButton);
            this.OptionsPanel.Controls.Add(this.ShowMapButton);
            this.OptionsPanel.Controls.Add(this.TrackFlowCheckBox);
            this.OptionsPanel.Location = new System.Drawing.Point(12, 158);
            this.OptionsPanel.Name = "OptionsPanel";
            this.OptionsPanel.Size = new System.Drawing.Size(326, 130);
            this.OptionsPanel.TabIndex = 15;
            // 
            // GraphButton
            // 
            this.GraphButton.Location = new System.Drawing.Point(290, 66);
            this.GraphButton.Name = "GraphButton";
            this.GraphButton.Size = new System.Drawing.Size(33, 23);
            this.GraphButton.TabIndex = 17;
            this.GraphButton.Text = "X";
            this.GraphButton.UseVisualStyleBackColor = true;
            this.GraphButton.Click += new System.EventHandler(this.GraphButton_Click);
            // 
            // TestCompile
            // 
            this.TestCompile.Location = new System.Drawing.Point(13, 95);
            this.TestCompile.Name = "TestCompile";
            this.TestCompile.Size = new System.Drawing.Size(99, 23);
            this.TestCompile.TabIndex = 16;
            this.TestCompile.Text = "Test Compile";
            this.TestCompile.UseVisualStyleBackColor = true;
            this.TestCompile.Click += new System.EventHandler(this.TestCompile_Click);
            // 
            // TrackAnonCheckBox
            // 
            this.TrackAnonCheckBox.AutoSize = true;
            this.TrackAnonCheckBox.Location = new System.Drawing.Point(153, 26);
            this.TrackAnonCheckBox.Name = "TrackAnonCheckBox";
            this.TrackAnonCheckBox.Size = new System.Drawing.Size(154, 17);
            this.TrackAnonCheckBox.TabIndex = 15;
            this.TrackAnonCheckBox.Text = "Track anonymous methods";
            this.TrackAnonCheckBox.UseVisualStyleBackColor = true;
            // 
            // BuildForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(350, 313);
            this.Controls.Add(this.OptionsPanel);
            this.Controls.Add(this.OutputLink);
            this.Controls.Add(this.ResetLink);
            this.Controls.Add(this.RemoveLink);
            this.Controls.Add(this.AddLink);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.FileList);
            this.Name = "BuildForm";
            this.Text = "XRay";
            this.OptionsPanel.ResumeLayout(false);
            this.OptionsPanel.PerformLayout();
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
        private System.Windows.Forms.CheckBox TrackFlowCheckBox;
        private System.Windows.Forms.CheckBox SidebySideCheckBox;
        private System.Windows.Forms.LinkLabel ResetLink;
        private System.Windows.Forms.LinkLabel OutputLink;
        private System.Windows.Forms.CheckBox TrackExternalCheckBox;
        private System.Windows.Forms.Panel OptionsPanel;
        private System.Windows.Forms.CheckBox TrackAnonCheckBox;
        private System.Windows.Forms.Button TestCompile;
        private System.Windows.Forms.Button GraphButton;

    }
}

