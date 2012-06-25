namespace XBuilder
{
    partial class BuildPanel
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
            this.TrackInstancesCheckBox = new System.Windows.Forms.CheckBox();
            this.TrackFieldsCheckBox = new System.Windows.Forms.CheckBox();
            this.StatusLabel = new System.Windows.Forms.Label();
            this.MsToolsCheckbox = new System.Windows.Forms.CheckBox();
            this.RunVerifyCheckbox = new System.Windows.Forms.CheckBox();
            this.OptionsPanel = new System.Windows.Forms.Panel();
            this.DecompileCSharpCheckBox = new System.Windows.Forms.CheckBox();
            this.ShowOnStartCheckBox = new System.Windows.Forms.CheckBox();
            this.OutputLink = new System.Windows.Forms.LinkLabel();
            this.DecompileAgainCheckbox = new System.Windows.Forms.CheckBox();
            this.TestCompile = new System.Windows.Forms.Button();
            this.TrackAnonCheckBox = new System.Windows.Forms.CheckBox();
            this.ReplaceOriginalCheckBox = new System.Windows.Forms.CheckBox();
            this.TrackExternalCheckBox = new System.Windows.Forms.CheckBox();
            this.ReCompileButton = new System.Windows.Forms.Button();
            this.LaunchButton = new System.Windows.Forms.Button();
            this.ShowMapButton = new System.Windows.Forms.Button();
            this.TrackFlowCheckBox = new System.Windows.Forms.CheckBox();
            this.FileList = new System.Windows.Forms.ListBox();
            this.RemoveLink = new System.Windows.Forms.LinkLabel();
            this.AddLink = new System.Windows.Forms.LinkLabel();
            this.ResetLink = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.OptionsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // TrackInstancesCheckBox
            // 
            this.TrackInstancesCheckBox.AutoSize = true;
            this.TrackInstancesCheckBox.Checked = true;
            this.TrackInstancesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.TrackInstancesCheckBox.Location = new System.Drawing.Point(13, 72);
            this.TrackInstancesCheckBox.Name = "TrackInstancesCheckBox";
            this.TrackInstancesCheckBox.Size = new System.Drawing.Size(102, 17);
            this.TrackInstancesCheckBox.TabIndex = 23;
            this.TrackInstancesCheckBox.Text = "Track instances";
            this.TrackInstancesCheckBox.UseVisualStyleBackColor = true;
            // 
            // TrackFieldsCheckBox
            // 
            this.TrackFieldsCheckBox.AutoSize = true;
            this.TrackFieldsCheckBox.Checked = true;
            this.TrackFieldsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.TrackFieldsCheckBox.Location = new System.Drawing.Point(13, 26);
            this.TrackFieldsCheckBox.Name = "TrackFieldsCheckBox";
            this.TrackFieldsCheckBox.Size = new System.Drawing.Size(81, 17);
            this.TrackFieldsCheckBox.TabIndex = 22;
            this.TrackFieldsCheckBox.Text = "Track fields";
            this.TrackFieldsCheckBox.UseVisualStyleBackColor = true;
            // 
            // StatusLabel
            // 
            this.StatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.StatusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StatusLabel.ForeColor = System.Drawing.Color.Green;
            this.StatusLabel.Location = new System.Drawing.Point(10, 223);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(309, 23);
            this.StatusLabel.TabIndex = 21;
            this.StatusLabel.Text = "Add Assemblies";
            this.StatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MsToolsCheckbox
            // 
            this.MsToolsCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.MsToolsCheckbox.AutoSize = true;
            this.MsToolsCheckbox.Location = new System.Drawing.Point(261, 165);
            this.MsToolsCheckbox.Name = "MsToolsCheckbox";
            this.MsToolsCheckbox.Size = new System.Drawing.Size(189, 17);
            this.MsToolsCheckbox.TabIndex = 20;
            this.MsToolsCheckbox.Text = "Use MS toolchain instead of Mono";
            this.MsToolsCheckbox.UseVisualStyleBackColor = true;
            // 
            // RunVerifyCheckbox
            // 
            this.RunVerifyCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RunVerifyCheckbox.AutoSize = true;
            this.RunVerifyCheckbox.Location = new System.Drawing.Point(261, 119);
            this.RunVerifyCheckbox.Name = "RunVerifyCheckbox";
            this.RunVerifyCheckbox.Size = new System.Drawing.Size(197, 17);
            this.RunVerifyCheckbox.TabIndex = 19;
            this.RunVerifyCheckbox.Text = "Run verify on recompiled assemblies";
            this.RunVerifyCheckbox.UseVisualStyleBackColor = true;
            // 
            // OptionsPanel
            // 
            this.OptionsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OptionsPanel.Controls.Add(this.DecompileCSharpCheckBox);
            this.OptionsPanel.Controls.Add(this.ShowOnStartCheckBox);
            this.OptionsPanel.Controls.Add(this.TrackInstancesCheckBox);
            this.OptionsPanel.Controls.Add(this.TrackFieldsCheckBox);
            this.OptionsPanel.Controls.Add(this.StatusLabel);
            this.OptionsPanel.Controls.Add(this.MsToolsCheckbox);
            this.OptionsPanel.Controls.Add(this.RunVerifyCheckbox);
            this.OptionsPanel.Controls.Add(this.OutputLink);
            this.OptionsPanel.Controls.Add(this.DecompileAgainCheckbox);
            this.OptionsPanel.Controls.Add(this.TestCompile);
            this.OptionsPanel.Controls.Add(this.TrackAnonCheckBox);
            this.OptionsPanel.Controls.Add(this.ReplaceOriginalCheckBox);
            this.OptionsPanel.Controls.Add(this.TrackExternalCheckBox);
            this.OptionsPanel.Controls.Add(this.ReCompileButton);
            this.OptionsPanel.Controls.Add(this.LaunchButton);
            this.OptionsPanel.Controls.Add(this.ShowMapButton);
            this.OptionsPanel.Controls.Add(this.TrackFlowCheckBox);
            this.OptionsPanel.Location = new System.Drawing.Point(6, 112);
            this.OptionsPanel.Name = "OptionsPanel";
            this.OptionsPanel.Size = new System.Drawing.Size(322, 250);
            this.OptionsPanel.TabIndex = 21;
            // 
            // DecompileCSharpCheckBox
            // 
            this.DecompileCSharpCheckBox.AutoSize = true;
            this.DecompileCSharpCheckBox.Location = new System.Drawing.Point(13, 94);
            this.DecompileCSharpCheckBox.Name = "DecompileCSharpCheckBox";
            this.DecompileCSharpCheckBox.Size = new System.Drawing.Size(105, 17);
            this.DecompileCSharpCheckBox.TabIndex = 25;
            this.DecompileCSharpCheckBox.Text = "Decompile to C#";
            this.DecompileCSharpCheckBox.UseVisualStyleBackColor = true;
            // 
            // ShowOnStartCheckBox
            // 
            this.ShowOnStartCheckBox.AutoSize = true;
            this.ShowOnStartCheckBox.Checked = true;
            this.ShowOnStartCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ShowOnStartCheckBox.Location = new System.Drawing.Point(13, 118);
            this.ShowOnStartCheckBox.Name = "ShowOnStartCheckBox";
            this.ShowOnStartCheckBox.Size = new System.Drawing.Size(165, 17);
            this.ShowOnStartCheckBox.TabIndex = 24;
            this.ShowOnStartCheckBox.Text = "Show viewer when app starts";
            this.ShowOnStartCheckBox.UseVisualStyleBackColor = true;
            // 
            // OutputLink
            // 
            this.OutputLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OutputLink.AutoEllipsis = true;
            this.OutputLink.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.OutputLink.Location = new System.Drawing.Point(10, 208);
            this.OutputLink.Name = "OutputLink";
            this.OutputLink.Size = new System.Drawing.Size(303, 13);
            this.OutputLink.TabIndex = 13;
            this.OutputLink.TabStop = true;
            this.OutputLink.Text = "Output: Path";
            this.OutputLink.Visible = false;
            this.OutputLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OutputLink_LinkClicked);
            // 
            // DecompileAgainCheckbox
            // 
            this.DecompileAgainCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DecompileAgainCheckbox.AutoSize = true;
            this.DecompileAgainCheckbox.Location = new System.Drawing.Point(261, 142);
            this.DecompileAgainCheckbox.Name = "DecompileAgainCheckbox";
            this.DecompileAgainCheckbox.Size = new System.Drawing.Size(238, 17);
            this.DecompileAgainCheckbox.TabIndex = 18;
            this.DecompileAgainCheckbox.Text = "Decompile produced assembly for debugging";
            this.DecompileAgainCheckbox.UseVisualStyleBackColor = true;
            // 
            // TestCompile
            // 
            this.TestCompile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.TestCompile.Location = new System.Drawing.Point(223, 90);
            this.TestCompile.Name = "TestCompile";
            this.TestCompile.Size = new System.Drawing.Size(99, 23);
            this.TestCompile.TabIndex = 16;
            this.TestCompile.Text = "Test Recompile";
            this.TestCompile.UseVisualStyleBackColor = true;
            this.TestCompile.Click += new System.EventHandler(this.TestCompile_Click);
            // 
            // TrackAnonCheckBox
            // 
            this.TrackAnonCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.TrackAnonCheckBox.AutoSize = true;
            this.TrackAnonCheckBox.Checked = true;
            this.TrackAnonCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.TrackAnonCheckBox.Location = new System.Drawing.Point(261, 184);
            this.TrackAnonCheckBox.Name = "TrackAnonCheckBox";
            this.TrackAnonCheckBox.Size = new System.Drawing.Size(139, 17);
            this.TrackAnonCheckBox.TabIndex = 15;
            this.TrackAnonCheckBox.Text = "Track anonymous types";
            this.TrackAnonCheckBox.UseVisualStyleBackColor = true;
            this.TrackAnonCheckBox.Visible = false;
            // 
            // ReplaceOriginalCheckBox
            // 
            this.ReplaceOriginalCheckBox.AutoSize = true;
            this.ReplaceOriginalCheckBox.Location = new System.Drawing.Point(13, 141);
            this.ReplaceOriginalCheckBox.Name = "ReplaceOriginalCheckBox";
            this.ReplaceOriginalCheckBox.Size = new System.Drawing.Size(221, 17);
            this.ReplaceOriginalCheckBox.TabIndex = 11;
            this.ReplaceOriginalCheckBox.Text = "Overwrite originals. For dlls, wpf, and asp.";
            this.ReplaceOriginalCheckBox.UseVisualStyleBackColor = true;
            this.ReplaceOriginalCheckBox.CheckedChanged += new System.EventHandler(this.SidebySideCheckBox_CheckedChanged);
            // 
            // TrackExternalCheckBox
            // 
            this.TrackExternalCheckBox.AutoSize = true;
            this.TrackExternalCheckBox.Checked = true;
            this.TrackExternalCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.TrackExternalCheckBox.Location = new System.Drawing.Point(13, 49);
            this.TrackExternalCheckBox.Name = "TrackExternalCheckBox";
            this.TrackExternalCheckBox.Size = new System.Drawing.Size(166, 17);
            this.TrackExternalCheckBox.TabIndex = 14;
            this.TrackExternalCheckBox.Text = "Track calls to non-xrayed files";
            this.TrackExternalCheckBox.UseVisualStyleBackColor = true;
            // 
            // ReCompileButton
            // 
            this.ReCompileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ReCompileButton.Location = new System.Drawing.Point(223, 3);
            this.ReCompileButton.Name = "ReCompileButton";
            this.ReCompileButton.Size = new System.Drawing.Size(99, 23);
            this.ReCompileButton.TabIndex = 4;
            this.ReCompileButton.Text = "Recompile";
            this.ReCompileButton.UseVisualStyleBackColor = true;
            this.ReCompileButton.Click += new System.EventHandler(this.ReCompileButton_Click);
            // 
            // LaunchButton
            // 
            this.LaunchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LaunchButton.Location = new System.Drawing.Point(223, 61);
            this.LaunchButton.Name = "LaunchButton";
            this.LaunchButton.Size = new System.Drawing.Size(99, 23);
            this.LaunchButton.TabIndex = 5;
            this.LaunchButton.Text = "Launch";
            this.LaunchButton.UseVisualStyleBackColor = true;
            this.LaunchButton.Click += new System.EventHandler(this.LaunchButton_Click);
            // 
            // ShowMapButton
            // 
            this.ShowMapButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ShowMapButton.Location = new System.Drawing.Point(223, 32);
            this.ShowMapButton.Name = "ShowMapButton";
            this.ShowMapButton.Size = new System.Drawing.Size(99, 23);
            this.ShowMapButton.TabIndex = 9;
            this.ShowMapButton.Text = "Analyze";
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
            this.TrackFlowCheckBox.Size = new System.Drawing.Size(124, 17);
            this.TrackFlowCheckBox.TabIndex = 10;
            this.TrackFlowCheckBox.Text = "Track function stack";
            this.TrackFlowCheckBox.UseVisualStyleBackColor = true;
            // 
            // FileList
            // 
            this.FileList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FileList.FormattingEnabled = true;
            this.FileList.IntegralHeight = false;
            this.FileList.Location = new System.Drawing.Point(6, 25);
            this.FileList.Name = "FileList";
            this.FileList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.FileList.Size = new System.Drawing.Size(322, 81);
            this.FileList.TabIndex = 16;
            // 
            // RemoveLink
            // 
            this.RemoveLink.AutoSize = true;
            this.RemoveLink.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.RemoveLink.Location = new System.Drawing.Point(164, 9);
            this.RemoveLink.Name = "RemoveLink";
            this.RemoveLink.Size = new System.Drawing.Size(47, 13);
            this.RemoveLink.TabIndex = 19;
            this.RemoveLink.TabStop = true;
            this.RemoveLink.Text = "Remove";
            this.RemoveLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.RemoveLink_LinkClicked);
            // 
            // AddLink
            // 
            this.AddLink.AutoSize = true;
            this.AddLink.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.AddLink.Location = new System.Drawing.Point(132, 9);
            this.AddLink.Name = "AddLink";
            this.AddLink.Size = new System.Drawing.Size(26, 13);
            this.AddLink.TabIndex = 18;
            this.AddLink.TabStop = true;
            this.AddLink.Text = "Add";
            this.AddLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.AddLink_LinkClicked);
            // 
            // ResetLink
            // 
            this.ResetLink.AutoSize = true;
            this.ResetLink.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.ResetLink.Location = new System.Drawing.Point(217, 9);
            this.ResetLink.Name = "ResetLink";
            this.ResetLink.Size = new System.Drawing.Size(35, 13);
            this.ResetLink.TabIndex = 20;
            this.ResetLink.TabStop = true;
            this.ResetLink.Text = "Reset";
            this.ResetLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ResetLink_LinkClicked);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(8, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 13);
            this.label1.TabIndex = 17;
            this.label1.Text = "Source Assemblies";
            // 
            // BuildPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.OptionsPanel);
            this.Controls.Add(this.FileList);
            this.Controls.Add(this.RemoveLink);
            this.Controls.Add(this.AddLink);
            this.Controls.Add(this.ResetLink);
            this.Controls.Add(this.label1);
            this.Name = "BuildPanel";
            this.Size = new System.Drawing.Size(331, 365);
            this.OptionsPanel.ResumeLayout(false);
            this.OptionsPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox TrackInstancesCheckBox;
        private System.Windows.Forms.CheckBox TrackFieldsCheckBox;
        private System.Windows.Forms.Label StatusLabel;
        private System.Windows.Forms.CheckBox MsToolsCheckbox;
        private System.Windows.Forms.CheckBox RunVerifyCheckbox;
        private System.Windows.Forms.Panel OptionsPanel;
        private System.Windows.Forms.LinkLabel OutputLink;
        private System.Windows.Forms.CheckBox DecompileAgainCheckbox;
        private System.Windows.Forms.Button TestCompile;
        private System.Windows.Forms.CheckBox TrackAnonCheckBox;
        private System.Windows.Forms.CheckBox ReplaceOriginalCheckBox;
        private System.Windows.Forms.CheckBox TrackExternalCheckBox;
        private System.Windows.Forms.Button ReCompileButton;
        private System.Windows.Forms.Button LaunchButton;
        private System.Windows.Forms.Button ShowMapButton;
        private System.Windows.Forms.CheckBox TrackFlowCheckBox;
        private System.Windows.Forms.ListBox FileList;
        private System.Windows.Forms.LinkLabel RemoveLink;
        private System.Windows.Forms.LinkLabel AddLink;
        private System.Windows.Forms.LinkLabel ResetLink;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox ShowOnStartCheckBox;
        private System.Windows.Forms.CheckBox DecompileCSharpCheckBox;
    }
}
