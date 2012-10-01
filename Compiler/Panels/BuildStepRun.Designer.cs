namespace XBuilder.Panels
{
    partial class BuildStepRun
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
            this.BackButton = new System.Windows.Forms.Button();
            this.NextButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.LaunchButton = new System.Windows.Forms.Button();
            this.AnalzeLink = new System.Windows.Forms.LinkLabel();
            this.VerifyLink = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // BackButton
            // 
            this.BackButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BackButton.Location = new System.Drawing.Point(147, 238);
            this.BackButton.Name = "BackButton";
            this.BackButton.Size = new System.Drawing.Size(75, 23);
            this.BackButton.TabIndex = 39;
            this.BackButton.Text = "< Back";
            this.BackButton.UseVisualStyleBackColor = true;
            this.BackButton.Click += new System.EventHandler(this.BackButton_Click);
            // 
            // NextButton
            // 
            this.NextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.NextButton.Location = new System.Drawing.Point(228, 238);
            this.NextButton.Name = "NextButton";
            this.NextButton.Size = new System.Drawing.Size(89, 23);
            this.NextButton.TabIndex = 38;
            this.NextButton.Text = "Start Over >";
            this.NextButton.UseVisualStyleBackColor = true;
            this.NextButton.Click += new System.EventHandler(this.NextButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 20);
            this.label1.TabIndex = 40;
            this.label1.Text = "Run";
            // 
            // LaunchButton
            // 
            this.LaunchButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LaunchButton.Location = new System.Drawing.Point(37, 64);
            this.LaunchButton.Name = "LaunchButton";
            this.LaunchButton.Size = new System.Drawing.Size(108, 32);
            this.LaunchButton.TabIndex = 41;
            this.LaunchButton.Text = "Launch";
            this.LaunchButton.UseVisualStyleBackColor = true;
            this.LaunchButton.Click += new System.EventHandler(this.LaunchButton_Click);
            // 
            // AnalzeLink
            // 
            this.AnalzeLink.AutoSize = true;
            this.AnalzeLink.Location = new System.Drawing.Point(34, 109);
            this.AnalzeLink.Name = "AnalzeLink";
            this.AnalzeLink.Size = new System.Drawing.Size(129, 13);
            this.AnalzeLink.TabIndex = 43;
            this.AnalzeLink.TabStop = true;
            this.AnalzeLink.Text = "Analyze generated dat file";
            this.AnalzeLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.AnalzeLink_LinkClicked);
            // 
            // VerifyLink
            // 
            this.VerifyLink.AutoSize = true;
            this.VerifyLink.Location = new System.Drawing.Point(34, 132);
            this.VerifyLink.Name = "VerifyLink";
            this.VerifyLink.Size = new System.Drawing.Size(119, 13);
            this.VerifyLink.TabIndex = 44;
            this.VerifyLink.TabStop = true;
            this.VerifyLink.Text = "PEVerify generated files";
            this.VerifyLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.VerifyLink_LinkClicked);
            // 
            // BuildStepRun
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.VerifyLink);
            this.Controls.Add(this.AnalzeLink);
            this.Controls.Add(this.LaunchButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.BackButton);
            this.Controls.Add(this.NextButton);
            this.Name = "BuildStepRun";
            this.Size = new System.Drawing.Size(320, 264);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BackButton;
        private System.Windows.Forms.Button NextButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button LaunchButton;
        private System.Windows.Forms.LinkLabel AnalzeLink;
        private System.Windows.Forms.LinkLabel VerifyLink;
    }
}
