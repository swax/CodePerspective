namespace XBuilder.Panels
{
    partial class BuildStepOptions
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
            this.label1 = new System.Windows.Forms.Label();
            this.DecompileCSharpCheckBox = new System.Windows.Forms.CheckBox();
            this.TrackInstancesCheckBox = new System.Windows.Forms.CheckBox();
            this.TrackFieldsCheckBox = new System.Windows.Forms.CheckBox();
            this.DecompileAgainCheckbox = new System.Windows.Forms.CheckBox();
            this.ReplaceOriginalCheckBox = new System.Windows.Forms.CheckBox();
            this.TrackExternalCheckBox = new System.Windows.Forms.CheckBox();
            this.TrackFlowCheckBox = new System.Windows.Forms.CheckBox();
            this.NextButton = new System.Windows.Forms.Button();
            this.BackButton = new System.Windows.Forms.Button();
            this.SaveMsilCheckBox = new System.Windows.Forms.CheckBox();
            this.MsToolsCheckbox = new System.Windows.Forms.CheckBox();
            this.RunVerifyCheckbox = new System.Windows.Forms.CheckBox();
            this.TrackAnonCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(144, 20);
            this.label1.TabIndex = 23;
            this.label1.Text = "Tracking Options";
            // 
            // DecompileCSharpCheckBox
            // 
            this.DecompileCSharpCheckBox.AutoSize = true;
            this.DecompileCSharpCheckBox.Location = new System.Drawing.Point(7, 137);
            this.DecompileCSharpCheckBox.Name = "DecompileCSharpCheckBox";
            this.DecompileCSharpCheckBox.Size = new System.Drawing.Size(105, 17);
            this.DecompileCSharpCheckBox.TabIndex = 33;
            this.DecompileCSharpCheckBox.Text = "Decompile to C#";
            this.DecompileCSharpCheckBox.UseVisualStyleBackColor = true;
            // 
            // TrackInstancesCheckBox
            // 
            this.TrackInstancesCheckBox.AutoSize = true;
            this.TrackInstancesCheckBox.Checked = true;
            this.TrackInstancesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.TrackInstancesCheckBox.Location = new System.Drawing.Point(7, 113);
            this.TrackInstancesCheckBox.Name = "TrackInstancesCheckBox";
            this.TrackInstancesCheckBox.Size = new System.Drawing.Size(102, 17);
            this.TrackInstancesCheckBox.TabIndex = 31;
            this.TrackInstancesCheckBox.Text = "Track instances";
            this.TrackInstancesCheckBox.UseVisualStyleBackColor = true;
            // 
            // TrackFieldsCheckBox
            // 
            this.TrackFieldsCheckBox.AutoSize = true;
            this.TrackFieldsCheckBox.Checked = true;
            this.TrackFieldsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.TrackFieldsCheckBox.Location = new System.Drawing.Point(7, 67);
            this.TrackFieldsCheckBox.Name = "TrackFieldsCheckBox";
            this.TrackFieldsCheckBox.Size = new System.Drawing.Size(81, 17);
            this.TrackFieldsCheckBox.TabIndex = 30;
            this.TrackFieldsCheckBox.Text = "Track fields";
            this.TrackFieldsCheckBox.UseVisualStyleBackColor = true;
            // 
            // DecompileAgainCheckbox
            // 
            this.DecompileAgainCheckbox.AutoSize = true;
            this.DecompileAgainCheckbox.Location = new System.Drawing.Point(7, 183);
            this.DecompileAgainCheckbox.Name = "DecompileAgainCheckbox";
            this.DecompileAgainCheckbox.Size = new System.Drawing.Size(238, 17);
            this.DecompileAgainCheckbox.TabIndex = 29;
            this.DecompileAgainCheckbox.Text = "Decompile produced assembly for debugging";
            this.DecompileAgainCheckbox.UseVisualStyleBackColor = true;
            this.DecompileAgainCheckbox.Visible = false;
            // 
            // ReplaceOriginalCheckBox
            // 
            this.ReplaceOriginalCheckBox.AutoSize = true;
            this.ReplaceOriginalCheckBox.Location = new System.Drawing.Point(7, 160);
            this.ReplaceOriginalCheckBox.Name = "ReplaceOriginalCheckBox";
            this.ReplaceOriginalCheckBox.Size = new System.Drawing.Size(221, 17);
            this.ReplaceOriginalCheckBox.TabIndex = 27;
            this.ReplaceOriginalCheckBox.Text = "Overwrite originals. For dlls, wpf, and asp.";
            this.ReplaceOriginalCheckBox.UseVisualStyleBackColor = true;
            // 
            // TrackExternalCheckBox
            // 
            this.TrackExternalCheckBox.AutoSize = true;
            this.TrackExternalCheckBox.Checked = true;
            this.TrackExternalCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.TrackExternalCheckBox.Location = new System.Drawing.Point(7, 90);
            this.TrackExternalCheckBox.Name = "TrackExternalCheckBox";
            this.TrackExternalCheckBox.Size = new System.Drawing.Size(166, 17);
            this.TrackExternalCheckBox.TabIndex = 28;
            this.TrackExternalCheckBox.Text = "Track calls to non-xrayed files";
            this.TrackExternalCheckBox.UseVisualStyleBackColor = true;
            // 
            // TrackFlowCheckBox
            // 
            this.TrackFlowCheckBox.AutoSize = true;
            this.TrackFlowCheckBox.Checked = true;
            this.TrackFlowCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.TrackFlowCheckBox.Location = new System.Drawing.Point(7, 44);
            this.TrackFlowCheckBox.Name = "TrackFlowCheckBox";
            this.TrackFlowCheckBox.Size = new System.Drawing.Size(124, 17);
            this.TrackFlowCheckBox.TabIndex = 26;
            this.TrackFlowCheckBox.Text = "Track function stack";
            this.TrackFlowCheckBox.UseVisualStyleBackColor = true;
            // 
            // NextButton
            // 
            this.NextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.NextButton.Location = new System.Drawing.Point(299, 281);
            this.NextButton.Name = "NextButton";
            this.NextButton.Size = new System.Drawing.Size(75, 23);
            this.NextButton.TabIndex = 34;
            this.NextButton.Text = "Next >";
            this.NextButton.UseVisualStyleBackColor = true;
            this.NextButton.Click += new System.EventHandler(this.NextButton_Click);
            // 
            // BackButton
            // 
            this.BackButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BackButton.Location = new System.Drawing.Point(218, 281);
            this.BackButton.Name = "BackButton";
            this.BackButton.Size = new System.Drawing.Size(75, 23);
            this.BackButton.TabIndex = 35;
            this.BackButton.Text = "< Back";
            this.BackButton.UseVisualStyleBackColor = true;
            this.BackButton.Click += new System.EventHandler(this.BackButton_Click);
            // 
            // SaveMsilCheckBox
            // 
            this.SaveMsilCheckBox.AutoSize = true;
            this.SaveMsilCheckBox.Checked = true;
            this.SaveMsilCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SaveMsilCheckBox.Location = new System.Drawing.Point(7, 275);
            this.SaveMsilCheckBox.Name = "SaveMsilCheckBox";
            this.SaveMsilCheckBox.Size = new System.Drawing.Size(51, 17);
            this.SaveMsilCheckBox.TabIndex = 39;
            this.SaveMsilCheckBox.Text = "MSIL";
            this.SaveMsilCheckBox.UseVisualStyleBackColor = true;
            this.SaveMsilCheckBox.Visible = false;
            // 
            // MsToolsCheckbox
            // 
            this.MsToolsCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.MsToolsCheckbox.AutoSize = true;
            this.MsToolsCheckbox.Location = new System.Drawing.Point(7, 229);
            this.MsToolsCheckbox.Name = "MsToolsCheckbox";
            this.MsToolsCheckbox.Size = new System.Drawing.Size(189, 17);
            this.MsToolsCheckbox.TabIndex = 38;
            this.MsToolsCheckbox.Text = "Use MS toolchain instead of Mono";
            this.MsToolsCheckbox.UseVisualStyleBackColor = true;
            this.MsToolsCheckbox.Visible = false;
            // 
            // RunVerifyCheckbox
            // 
            this.RunVerifyCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RunVerifyCheckbox.AutoSize = true;
            this.RunVerifyCheckbox.Location = new System.Drawing.Point(7, 206);
            this.RunVerifyCheckbox.Name = "RunVerifyCheckbox";
            this.RunVerifyCheckbox.Size = new System.Drawing.Size(197, 17);
            this.RunVerifyCheckbox.TabIndex = 37;
            this.RunVerifyCheckbox.Text = "Run verify on recompiled assemblies";
            this.RunVerifyCheckbox.UseVisualStyleBackColor = true;
            this.RunVerifyCheckbox.Visible = false;
            // 
            // TrackAnonCheckBox
            // 
            this.TrackAnonCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.TrackAnonCheckBox.AutoSize = true;
            this.TrackAnonCheckBox.Checked = true;
            this.TrackAnonCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.TrackAnonCheckBox.Location = new System.Drawing.Point(7, 252);
            this.TrackAnonCheckBox.Name = "TrackAnonCheckBox";
            this.TrackAnonCheckBox.Size = new System.Drawing.Size(139, 17);
            this.TrackAnonCheckBox.TabIndex = 36;
            this.TrackAnonCheckBox.Text = "Track anonymous types";
            this.TrackAnonCheckBox.UseVisualStyleBackColor = true;
            this.TrackAnonCheckBox.Visible = false;
            // 
            // BuildStepOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.SaveMsilCheckBox);
            this.Controls.Add(this.MsToolsCheckbox);
            this.Controls.Add(this.RunVerifyCheckbox);
            this.Controls.Add(this.TrackAnonCheckBox);
            this.Controls.Add(this.BackButton);
            this.Controls.Add(this.NextButton);
            this.Controls.Add(this.DecompileCSharpCheckBox);
            this.Controls.Add(this.TrackInstancesCheckBox);
            this.Controls.Add(this.TrackFieldsCheckBox);
            this.Controls.Add(this.DecompileAgainCheckbox);
            this.Controls.Add(this.ReplaceOriginalCheckBox);
            this.Controls.Add(this.TrackExternalCheckBox);
            this.Controls.Add(this.TrackFlowCheckBox);
            this.Controls.Add(this.label1);
            this.Name = "BuildStepOptions";
            this.Size = new System.Drawing.Size(377, 307);
            this.Load += new System.EventHandler(this.BuildStep2_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox DecompileCSharpCheckBox;
        private System.Windows.Forms.CheckBox TrackInstancesCheckBox;
        private System.Windows.Forms.CheckBox TrackFieldsCheckBox;
        private System.Windows.Forms.CheckBox DecompileAgainCheckbox;
        private System.Windows.Forms.CheckBox ReplaceOriginalCheckBox;
        private System.Windows.Forms.CheckBox TrackExternalCheckBox;
        private System.Windows.Forms.CheckBox TrackFlowCheckBox;
        private System.Windows.Forms.Button NextButton;
        private System.Windows.Forms.Button BackButton;
        private System.Windows.Forms.CheckBox SaveMsilCheckBox;
        private System.Windows.Forms.CheckBox MsToolsCheckbox;
        private System.Windows.Forms.CheckBox RunVerifyCheckbox;
        private System.Windows.Forms.CheckBox TrackAnonCheckBox;
    }
}
