namespace XBuilder.Panels
{
    partial class BuildStepCompile
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
            this.components = new System.ComponentModel.Container();
            this.BackButton = new System.Windows.Forms.Button();
            this.NextButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.ButtonRecompile = new System.Windows.Forms.Button();
            this.ButtonTestCompile = new System.Windows.Forms.LinkLabel();
            this.SecondTimer = new System.Windows.Forms.Timer(this.components);
            this.StatusLabel = new System.Windows.Forms.Label();
            this.ErrorLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // BackButton
            // 
            this.BackButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BackButton.Location = new System.Drawing.Point(235, 275);
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
            this.NextButton.Enabled = false;
            this.NextButton.Location = new System.Drawing.Point(316, 275);
            this.NextButton.Name = "NextButton";
            this.NextButton.Size = new System.Drawing.Size(75, 23);
            this.NextButton.TabIndex = 36;
            this.NextButton.Text = "Next >";
            this.NextButton.UseVisualStyleBackColor = true;
            this.NextButton.Click += new System.EventHandler(this.NextButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 20);
            this.label1.TabIndex = 38;
            this.label1.Text = "Build";
            // 
            // ButtonRecompile
            // 
            this.ButtonRecompile.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonRecompile.Location = new System.Drawing.Point(7, 53);
            this.ButtonRecompile.Name = "ButtonRecompile";
            this.ButtonRecompile.Size = new System.Drawing.Size(153, 34);
            this.ButtonRecompile.TabIndex = 39;
            this.ButtonRecompile.Text = "Re-Compile";
            this.ButtonRecompile.UseVisualStyleBackColor = true;
            this.ButtonRecompile.Click += new System.EventHandler(this.ButtonRecompile_Click);
            // 
            // ButtonTestCompile
            // 
            this.ButtonTestCompile.AutoSize = true;
            this.ButtonTestCompile.Location = new System.Drawing.Point(4, 113);
            this.ButtonTestCompile.Name = "ButtonTestCompile";
            this.ButtonTestCompile.Size = new System.Drawing.Size(267, 13);
            this.ButtonTestCompile.TabIndex = 40;
            this.ButtonTestCompile.TabStop = true;
            this.ButtonTestCompile.Text = "Test Decompile/Recompile with ILASM (for debugging)";
            this.ButtonTestCompile.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ButtonTestCompile_LinkClicked);
            // 
            // SecondTimer
            // 
            this.SecondTimer.Enabled = true;
            this.SecondTimer.Interval = 500;
            this.SecondTimer.Tick += new System.EventHandler(this.SecondTimer_Tick);
            // 
            // StatusLabel
            // 
            this.StatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.StatusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StatusLabel.ForeColor = System.Drawing.Color.Green;
            this.StatusLabel.Location = new System.Drawing.Point(4, 141);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(387, 29);
            this.StatusLabel.TabIndex = 41;
            // 
            // ErrorLabel
            // 
            this.ErrorLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ErrorLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ErrorLabel.ForeColor = System.Drawing.Color.Maroon;
            this.ErrorLabel.Location = new System.Drawing.Point(4, 170);
            this.ErrorLabel.Name = "ErrorLabel";
            this.ErrorLabel.Size = new System.Drawing.Size(387, 29);
            this.ErrorLabel.TabIndex = 42;
            // 
            // BuildStepCompile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ErrorLabel);
            this.Controls.Add(this.StatusLabel);
            this.Controls.Add(this.ButtonTestCompile);
            this.Controls.Add(this.ButtonRecompile);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.BackButton);
            this.Controls.Add(this.NextButton);
            this.Name = "BuildStepCompile";
            this.Size = new System.Drawing.Size(394, 301);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BackButton;
        private System.Windows.Forms.Button NextButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button ButtonRecompile;
        private System.Windows.Forms.LinkLabel ButtonTestCompile;
        private System.Windows.Forms.Timer SecondTimer;
        private System.Windows.Forms.Label StatusLabel;
        private System.Windows.Forms.Label ErrorLabel;
    }
}
