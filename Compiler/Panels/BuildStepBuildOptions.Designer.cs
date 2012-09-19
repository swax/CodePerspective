namespace XBuilder.Panels
{
    partial class BuildStepBuildOptions
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
            this.BackButton = new System.Windows.Forms.Button();
            this.NextButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.SaveMsilCheckBox = new System.Windows.Forms.CheckBox();
            this.MsToolsCheckbox = new System.Windows.Forms.CheckBox();
            this.RunVerifyCheckbox = new System.Windows.Forms.CheckBox();
            this.DecompileCSharpCheckBox = new System.Windows.Forms.CheckBox();
            this.DecompileAgainCheckbox = new System.Windows.Forms.CheckBox();
            this.ReplaceOriginalCheckBox = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.PathTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 20);
            this.label1.TabIndex = 24;
            this.label1.Text = "Build Options";
            // 
            // BackButton
            // 
            this.BackButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BackButton.Location = new System.Drawing.Point(234, 312);
            this.BackButton.Name = "BackButton";
            this.BackButton.Size = new System.Drawing.Size(75, 23);
            this.BackButton.TabIndex = 37;
            this.BackButton.Text = "< Back";
            this.BackButton.UseVisualStyleBackColor = true;
            this.BackButton.Click += new System.EventHandler(this.BackButton_Click);
            // 
            // NextButton
            // 
            this.NextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.NextButton.Location = new System.Drawing.Point(315, 312);
            this.NextButton.Name = "NextButton";
            this.NextButton.Size = new System.Drawing.Size(75, 23);
            this.NextButton.TabIndex = 36;
            this.NextButton.Text = "Next >";
            this.NextButton.UseVisualStyleBackColor = true;
            this.NextButton.Click += new System.EventHandler(this.NextButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 113);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(316, 13);
            this.label2.TabIndex = 47;
            this.label2.Text = "Use this option when recompiling solo DLLs, WPF, and ASP apps";
            // 
            // SaveMsilCheckBox
            // 
            this.SaveMsilCheckBox.AutoSize = true;
            this.SaveMsilCheckBox.Location = new System.Drawing.Point(7, 67);
            this.SaveMsilCheckBox.Name = "SaveMsilCheckBox";
            this.SaveMsilCheckBox.Size = new System.Drawing.Size(256, 17);
            this.SaveMsilCheckBox.TabIndex = 46;
            this.SaveMsilCheckBox.Text = "Save MSIL code (fast, but increases dat file size)";
            this.SaveMsilCheckBox.UseVisualStyleBackColor = true;
            // 
            // MsToolsCheckbox
            // 
            this.MsToolsCheckbox.AutoSize = true;
            this.MsToolsCheckbox.Location = new System.Drawing.Point(7, 249);
            this.MsToolsCheckbox.Name = "MsToolsCheckbox";
            this.MsToolsCheckbox.Size = new System.Drawing.Size(189, 17);
            this.MsToolsCheckbox.TabIndex = 45;
            this.MsToolsCheckbox.Text = "Use MS toolchain instead of Mono";
            this.MsToolsCheckbox.UseVisualStyleBackColor = true;
            this.MsToolsCheckbox.Visible = false;
            // 
            // RunVerifyCheckbox
            // 
            this.RunVerifyCheckbox.AutoSize = true;
            this.RunVerifyCheckbox.Location = new System.Drawing.Point(7, 226);
            this.RunVerifyCheckbox.Name = "RunVerifyCheckbox";
            this.RunVerifyCheckbox.Size = new System.Drawing.Size(197, 17);
            this.RunVerifyCheckbox.TabIndex = 44;
            this.RunVerifyCheckbox.Text = "Run verify on recompiled assemblies";
            this.RunVerifyCheckbox.UseVisualStyleBackColor = true;
            this.RunVerifyCheckbox.Visible = false;
            // 
            // DecompileCSharpCheckBox
            // 
            this.DecompileCSharpCheckBox.AutoSize = true;
            this.DecompileCSharpCheckBox.Location = new System.Drawing.Point(7, 44);
            this.DecompileCSharpCheckBox.Name = "DecompileCSharpCheckBox";
            this.DecompileCSharpCheckBox.Size = new System.Drawing.Size(245, 17);
            this.DecompileCSharpCheckBox.TabIndex = 43;
            this.DecompileCSharpCheckBox.Text = "Decompile methods to C# for code view (slow)";
            this.DecompileCSharpCheckBox.UseVisualStyleBackColor = true;
            // 
            // DecompileAgainCheckbox
            // 
            this.DecompileAgainCheckbox.AutoSize = true;
            this.DecompileAgainCheckbox.Location = new System.Drawing.Point(7, 136);
            this.DecompileAgainCheckbox.Name = "DecompileAgainCheckbox";
            this.DecompileAgainCheckbox.Size = new System.Drawing.Size(331, 17);
            this.DecompileAgainCheckbox.TabIndex = 42;
            this.DecompileAgainCheckbox.Text = "Decompile produced assembly with ILDASM (for troubleshooting)";
            this.DecompileAgainCheckbox.UseVisualStyleBackColor = true;
            // 
            // ReplaceOriginalCheckBox
            // 
            this.ReplaceOriginalCheckBox.AutoSize = true;
            this.ReplaceOriginalCheckBox.Location = new System.Drawing.Point(7, 90);
            this.ReplaceOriginalCheckBox.Name = "ReplaceOriginalCheckBox";
            this.ReplaceOriginalCheckBox.Size = new System.Drawing.Size(260, 17);
            this.ReplaceOriginalCheckBox.TabIndex = 41;
            this.ReplaceOriginalCheckBox.Text = "Overwrite assemblies (backups kept in /xBackup)";
            this.ReplaceOriginalCheckBox.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 166);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(318, 13);
            this.label3.TabIndex = 48;
            this.label3.Text = "Run path (may be different if build is moved to a different directory)";
            // 
            // PathTextBox
            // 
            this.PathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PathTextBox.Location = new System.Drawing.Point(7, 182);
            this.PathTextBox.Name = "PathTextBox";
            this.PathTextBox.Size = new System.Drawing.Size(383, 20);
            this.PathTextBox.TabIndex = 49;
            // 
            // BuildStepBuildOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PathTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.SaveMsilCheckBox);
            this.Controls.Add(this.MsToolsCheckbox);
            this.Controls.Add(this.RunVerifyCheckbox);
            this.Controls.Add(this.DecompileCSharpCheckBox);
            this.Controls.Add(this.DecompileAgainCheckbox);
            this.Controls.Add(this.ReplaceOriginalCheckBox);
            this.Controls.Add(this.BackButton);
            this.Controls.Add(this.NextButton);
            this.Controls.Add(this.label1);
            this.Name = "BuildStepBuildOptions";
            this.Size = new System.Drawing.Size(393, 338);
            this.Load += new System.EventHandler(this.BuildStepBuildOptions_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button BackButton;
        private System.Windows.Forms.Button NextButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox SaveMsilCheckBox;
        private System.Windows.Forms.CheckBox MsToolsCheckbox;
        private System.Windows.Forms.CheckBox RunVerifyCheckbox;
        private System.Windows.Forms.CheckBox DecompileCSharpCheckBox;
        private System.Windows.Forms.CheckBox DecompileAgainCheckbox;
        private System.Windows.Forms.CheckBox ReplaceOriginalCheckBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox PathTextBox;
    }
}
