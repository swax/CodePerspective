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
            this.label2 = new System.Windows.Forms.Label();
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
            this.DecompileCSharpCheckBox.Location = new System.Drawing.Point(7, 159);
            this.DecompileCSharpCheckBox.Name = "DecompileCSharpCheckBox";
            this.DecompileCSharpCheckBox.Size = new System.Drawing.Size(245, 17);
            this.DecompileCSharpCheckBox.TabIndex = 33;
            this.DecompileCSharpCheckBox.Text = "Decompile methods to C# for code view (slow)";
            this.DecompileCSharpCheckBox.UseVisualStyleBackColor = true;
            // 
            // TrackInstancesCheckBox
            // 
            this.TrackInstancesCheckBox.AutoSize = true;
            this.TrackInstancesCheckBox.Location = new System.Drawing.Point(7, 113);
            this.TrackInstancesCheckBox.Name = "TrackInstancesCheckBox";
            this.TrackInstancesCheckBox.Size = new System.Drawing.Size(247, 17);
            this.TrackInstancesCheckBox.TabIndex = 31;
            this.TrackInstancesCheckBox.Text = "Track class instance construction and disposal";
            this.TrackInstancesCheckBox.UseVisualStyleBackColor = true;
            // 
            // TrackFieldsCheckBox
            // 
            this.TrackFieldsCheckBox.AutoSize = true;
            this.TrackFieldsCheckBox.Location = new System.Drawing.Point(7, 67);
            this.TrackFieldsCheckBox.Name = "TrackFieldsCheckBox";
            this.TrackFieldsCheckBox.Size = new System.Drawing.Size(234, 17);
            this.TrackFieldsCheckBox.TabIndex = 30;
            this.TrackFieldsCheckBox.Text = "Track get/set operations on class properties";
            this.TrackFieldsCheckBox.UseVisualStyleBackColor = true;
            // 
            // DecompileAgainCheckbox
            // 
            this.DecompileAgainCheckbox.AutoSize = true;
            this.DecompileAgainCheckbox.Location = new System.Drawing.Point(7, 251);
            this.DecompileAgainCheckbox.Name = "DecompileAgainCheckbox";
            this.DecompileAgainCheckbox.Size = new System.Drawing.Size(331, 17);
            this.DecompileAgainCheckbox.TabIndex = 29;
            this.DecompileAgainCheckbox.Text = "Decompile produced assembly with ILDASM (for troubleshooting)";
            this.DecompileAgainCheckbox.UseVisualStyleBackColor = true;
            // 
            // ReplaceOriginalCheckBox
            // 
            this.ReplaceOriginalCheckBox.AutoSize = true;
            this.ReplaceOriginalCheckBox.Location = new System.Drawing.Point(7, 205);
            this.ReplaceOriginalCheckBox.Name = "ReplaceOriginalCheckBox";
            this.ReplaceOriginalCheckBox.Size = new System.Drawing.Size(260, 17);
            this.ReplaceOriginalCheckBox.TabIndex = 27;
            this.ReplaceOriginalCheckBox.Text = "Overwrite assemblies (backups kept in /xBackup)";
            this.ReplaceOriginalCheckBox.UseVisualStyleBackColor = true;
            // 
            // TrackExternalCheckBox
            // 
            this.TrackExternalCheckBox.AutoSize = true;
            this.TrackExternalCheckBox.Location = new System.Drawing.Point(7, 90);
            this.TrackExternalCheckBox.Name = "TrackExternalCheckBox";
            this.TrackExternalCheckBox.Size = new System.Drawing.Size(349, 17);
            this.TrackExternalCheckBox.TabIndex = 28;
            this.TrackExternalCheckBox.Text = "Track calls to external assemblies (ones not chosen in previous step)";
            this.TrackExternalCheckBox.UseVisualStyleBackColor = true;
            // 
            // TrackFlowCheckBox
            // 
            this.TrackFlowCheckBox.AutoSize = true;
            this.TrackFlowCheckBox.Location = new System.Drawing.Point(7, 44);
            this.TrackFlowCheckBox.Name = "TrackFlowCheckBox";
            this.TrackFlowCheckBox.Size = new System.Drawing.Size(273, 17);
            this.TrackFlowCheckBox.TabIndex = 26;
            this.TrackFlowCheckBox.Text = "Track function stack (adds hooks to method returns)";
            this.TrackFlowCheckBox.UseVisualStyleBackColor = true;
            // 
            // NextButton
            // 
            this.NextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.NextButton.Location = new System.Drawing.Point(299, 314);
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
            this.BackButton.Location = new System.Drawing.Point(218, 314);
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
            this.SaveMsilCheckBox.Location = new System.Drawing.Point(7, 182);
            this.SaveMsilCheckBox.Name = "SaveMsilCheckBox";
            this.SaveMsilCheckBox.Size = new System.Drawing.Size(256, 17);
            this.SaveMsilCheckBox.TabIndex = 39;
            this.SaveMsilCheckBox.Text = "Save MSIL code (fast, but increases dat file size)";
            this.SaveMsilCheckBox.UseVisualStyleBackColor = true;
            // 
            // MsToolsCheckbox
            // 
            this.MsToolsCheckbox.AutoSize = true;
            this.MsToolsCheckbox.Location = new System.Drawing.Point(7, 297);
            this.MsToolsCheckbox.Name = "MsToolsCheckbox";
            this.MsToolsCheckbox.Size = new System.Drawing.Size(189, 17);
            this.MsToolsCheckbox.TabIndex = 38;
            this.MsToolsCheckbox.Text = "Use MS toolchain instead of Mono";
            this.MsToolsCheckbox.UseVisualStyleBackColor = true;
            this.MsToolsCheckbox.Visible = false;
            // 
            // RunVerifyCheckbox
            // 
            this.RunVerifyCheckbox.AutoSize = true;
            this.RunVerifyCheckbox.Location = new System.Drawing.Point(7, 274);
            this.RunVerifyCheckbox.Name = "RunVerifyCheckbox";
            this.RunVerifyCheckbox.Size = new System.Drawing.Size(197, 17);
            this.RunVerifyCheckbox.TabIndex = 37;
            this.RunVerifyCheckbox.Text = "Run verify on recompiled assemblies";
            this.RunVerifyCheckbox.UseVisualStyleBackColor = true;
            this.RunVerifyCheckbox.Visible = false;
            // 
            // TrackAnonCheckBox
            // 
            this.TrackAnonCheckBox.AutoSize = true;
            this.TrackAnonCheckBox.Location = new System.Drawing.Point(7, 136);
            this.TrackAnonCheckBox.Name = "TrackAnonCheckBox";
            this.TrackAnonCheckBox.Size = new System.Drawing.Size(213, 17);
            this.TrackAnonCheckBox.TabIndex = 36;
            this.TrackAnonCheckBox.Text = "Track anonymous classes and methods";
            this.TrackAnonCheckBox.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 228);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(316, 13);
            this.label2.TabIndex = 40;
            this.label2.Text = "Use this option when recompiling solo DLLs, WPF, and ASP apps";
            // 
            // BuildStepOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label2);
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
            this.Size = new System.Drawing.Size(377, 340);
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
        private System.Windows.Forms.Label label2;
    }
}
